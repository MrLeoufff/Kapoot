using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Kapoot.Tests.Api;

public class AuthApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsCreated_WithUser()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = "Test123!",
            Pseudo = "TestUser"
        });
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(user);
        Assert.Equal(email, user.Email);
        Assert.Equal("TestUser", user.Pseudo);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "nonexistent@example.com",
            Password = "wrong"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_Then_Login_ReturnsToken()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = "Test123!",
            Pseudo = "LoginUser"
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = "Test123!"
        });
        loginResponse.EnsureSuccessStatusCode();
        var body = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.NotNull(body.Token);
        Assert.NotNull(body.User);
        Assert.Equal(email, body.User.Email);
    }

    private record RegisterResponse(Guid Id, string Email, string Pseudo, string? AvatarUrl);
    private record LoginResponse(string Token, RegisterResponse User);
}
