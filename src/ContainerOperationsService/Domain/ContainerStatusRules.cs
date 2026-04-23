namespace ContainerOperationsService.Domain;

public static class ContainerStatusRules
{
    private static readonly HashSet<string> AllowedStatuses =
    [
        "inbound",
        "outbound",
        "hold",
        "customs-release",
        "loaded",
        "unloaded"
    ];

    public static bool IsValidStatus(string status) => AllowedStatuses.Contains(status);
}
