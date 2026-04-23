using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AuthService.IntegrationTests;

public sealed class AuthTokenFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthTokenFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TokenEndpoint_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/token", new
        {
            clientId = "unknown-client",
            clientSecret = "wrong-secret"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenEndpoint_ReturnsToken_WhenCredentialsAreValid()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/token", new
        {
            clientId = "terminal-web-client",
            clientSecret = "change-me-in-production"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.Equal("Bearer", body.TokenType);
    }

    public sealed class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
    }
}
