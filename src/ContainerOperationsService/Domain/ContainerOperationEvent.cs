namespace ContainerOperationsService.Domain;

public sealed class ContainerOperationEvent
{
    public Guid Id { get; set; }
    public Guid ContainerUnitId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
}
