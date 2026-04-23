using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace YardMovePlanningService.IntegrationTests;

public sealed class YardSecurityFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public YardSecurityFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(Path.GetFullPath("../../../../src/YardMovePlanningService", AppContext.BaseDirectory));
        });
    }

    [Fact]
    public async Task GetPlanningStatus_ReturnsUnauthorized_WhenTokenIsMissing()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/planning/status");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPlanningStatus_ReturnsOk_WhenTokenHasReadScope()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken("yard.read"));

        var response = await client.GetAsync("/api/planning/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ChangePriority_ReturnsForbidden_WhenTokenDoesNotHaveWriteScope()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken("yard.read"));

        var response = await client.PatchAsJsonAsync("/api/moves/00000000-0000-0000-0000-000000000000/priority", new
        {
            priority = 2
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static string CreateToken(params string[] scopes)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "test-client"),
            new(ClaimTypes.Role, "tester")
        };
        claims.AddRange(scopes.Select(scope => new Claim("scope", scope)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super-long-signing-key-for-demo-only-change-in-production")),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "terminal-operation-system",
            audience: "tos-api-clients",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
