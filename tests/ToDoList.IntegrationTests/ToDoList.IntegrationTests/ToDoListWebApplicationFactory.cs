using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ToDoList.Infrastructure.Data;

namespace ToDoList.IntegrationTests;

public class ToDoListWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace PostgreSQL DbContext with InMemory.
            // The DB name must be captured outside the lambda — if it were inside,
            // each DbContext creation would get a different database and requests
            // wouldn't share data.
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Use a dedicated internal service provider so the InMemory and Npgsql
            // providers don't conflict inside the same EF Core service provider.
            var inMemoryServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var dbName = $"TestDb_{Guid.NewGuid()}";
            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseInMemoryDatabase(dbName)
                    .UseInternalServiceProvider(inMemoryServiceProvider));

            // Remove all IHostedService registrations. Hangfire's server is registered
            // via a factory delegate (ImplementationType is null), so checking the type
            // name is unreliable. Removing all hosted services is safe for a test server
            // since HTTP request processing doesn't depend on them.
            var hostedServices = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();
            foreach (var descriptor in hostedServices)
                services.Remove(descriptor);
        });
    }
}
