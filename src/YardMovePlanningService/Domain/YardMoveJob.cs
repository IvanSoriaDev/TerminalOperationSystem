namespace YardMovePlanningService.Domain;

public sealed class YardMoveJob
{
    public Guid Id { get; set; }
    public string JobCode { get; set; } = string.Empty;
    public string ContainerNumber { get; set; } = string.Empty;
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public int Priority { get; set; } = 3;
    public string? AssignedTo { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
}
