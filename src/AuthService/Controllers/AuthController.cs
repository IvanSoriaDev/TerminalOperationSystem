using AuthService.Data;
using AuthService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthDbContext dbContext, ITokenService tokenService) : ControllerBase
{
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToken([FromBody] TokenRequest request, CancellationToken cancellationToken)
    {
        var technicalClient = await dbContext.TechnicalClients
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.ClientId == request.ClientId, cancellationToken);

        if (technicalClient is null || technicalClient.ClientSecret != request.ClientSecret)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication failed",
                Detail = "Provided client credentials are invalid.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var token = tokenService.GenerateToken(technicalClient);

        return Ok(new TokenResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Scope = technicalClient.AllowedScopes
        });
    }
}

public sealed class TokenRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public sealed class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string Scope { get; set; } = string.Empty;
}
