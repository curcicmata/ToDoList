using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ToDoList.Application.DTOs;

namespace ToDoList.IntegrationTests;

public class ApiIntegrationTests(ToDoListWebApplicationFactory factory) : IClassFixture<ToDoListWebApplicationFactory>
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private HttpClient CreateClient() => factory.CreateClient();

    private async Task<string> RegisterAndGetTokenAsync(HttpClient client, string email)
    {
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = "Password123!"
        });

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        return auth!.Token;
    }

    [Fact]
    public async Task Register_WithValidData_Returns201Created()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "register@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenInResponse()
    {
        var client = CreateClient();
        var email = "login@test.com";

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        Assert.NotNull(auth);
        Assert.NotEmpty(auth.Token);
    }

    [Fact]
    public async Task GetTasks_WithoutAuthentication_Returns401Unauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithAuthentication_Returns201Created()
    {
        var client = CreateClient();
        var token = await RegisterAndGetTokenAsync(client, "createtask@test.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            Title = "Integration Test Task",
            Priority = 1
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TodoTaskDto>(_jsonOptions);
        Assert.NotNull(task);
        Assert.Equal("Integration Test Task", task.Title);
    }

    [Fact]
    public async Task CreateCategory_WithAuthentication_Returns201Created()
    {
        var client = CreateClient();
        var token = await RegisterAndGetTokenAsync(client, "createcategory@test.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/categories", new
        {
            Name = "Test Category",
            Color = "#FF5733"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>(_jsonOptions);
        Assert.NotNull(category);
        Assert.Equal("Test Category", category.Name);
    }
}
