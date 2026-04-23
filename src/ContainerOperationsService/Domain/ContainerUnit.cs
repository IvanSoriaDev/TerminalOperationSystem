namespace ContainerOperationsService.Domain;

public sealed class ContainerUnit
{
    public Guid Id { get; set; }
    public string ContainerNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "inbound";
    public DateTime LastUpdatedUtc { get; set; }
}
