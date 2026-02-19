using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ToDoList.API.Filters;
using ToDoList.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDbContext<ToDoList.Infrastructure.Data.ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ToDoList.Application.Validators.RegisterDtoValidator>();

#region JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

#endregion

builder.Services.AddAuthorization();

// Register Repositories
builder.Services.AddScoped<ToDoList.Domain.Interfaces.IUserRepository, ToDoList.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<ToDoList.Domain.Interfaces.ICategoryRepository, ToDoList.Infrastructure.Repositories.CategoryRepository>();
builder.Services.AddScoped<ToDoList.Domain.Interfaces.ITodoTaskRepository, ToDoList.Infrastructure.Repositories.TodoTaskRepository>();

// Register Services
builder.Services.AddScoped<ToDoList.Application.Interfaces.IAuthService, ToDoList.Application.Services.AuthService>();
builder.Services.AddScoped<ToDoList.Application.Interfaces.IJwtTokenService, ToDoList.Infrastructure.Services.JwtTokenService>();
builder.Services.AddScoped<ToDoList.Application.Interfaces.ICategoryService, ToDoList.Application.Services.CategoryService>();
builder.Services.AddScoped<ToDoList.Application.Interfaces.ITodoTaskService, ToDoList.Application.Services.TodoTaskService>();

// Register Background Job Service
builder.Services.AddScoped<ToDoList.Infrastructure.BackgroundJobs.IBackgroundJobService, ToDoList.Infrastructure.BackgroundJobs.BackgroundJobService>();

#region Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

#region Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header. Enter your token in the text input below."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();


app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter(app.Environment.IsDevelopment())],
    DashboardTitle = "ToDoList Background Jobs"
});

app.MapControllers();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ToDoList.Infrastructure.Data.ApplicationDbContext>();
    var seeder = new ToDoList.Infrastructure.Data.DatabaseSeeder(context);
    await seeder.SeedAsync();
    Log.Information("Database seeded successfully");
}

#region Hangfire background jobs
RecurringJob.AddOrUpdate<ToDoList.Infrastructure.BackgroundJobs.IBackgroundJobService>(
    "send-overdue-reminders",
    service => service.SendOverdueTaskReminders(),
    Cron.Daily(9));

RecurringJob.AddOrUpdate<ToDoList.Infrastructure.BackgroundJobs.IBackgroundJobService>(
    "cleanup-deleted-records",
    service => service.CleanupSoftDeletedRecords(),
    Cron.Weekly(DayOfWeek.Sunday, 2));

Log.Information("Hangfire recurring jobs configured");

#endregion

try
{
    Log.Information("Starting ToDoList API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
