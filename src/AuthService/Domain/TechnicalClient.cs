namespace AuthService.Domain;

public sealed class TechnicalClient
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Role { get; set; } = "operator";
    public string AllowedScopes { get; set; } = string.Empty;
}
