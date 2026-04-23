namespace YardMovePlanningService.Domain;

public static class YardMoveRules
{
    public static bool IsValidPriority(int priority) => priority is >= 1 and <= 5;

    public static bool CanBeCompleted(string currentStatus) => currentStatus is "pending" or "assigned" or "rescheduled";
}
