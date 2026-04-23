using YardMovePlanningService.Domain;
using Xunit;

namespace YardMovePlanningService.UnitTests;

public sealed class YardMoveRulesTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void IsValidPriority_ReturnsTrue_ForAllowedRange(int priority)
    {
        Assert.True(YardMoveRules.IsValidPriority(priority));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void IsValidPriority_ReturnsFalse_ForOutOfRange(int priority)
    {
        Assert.False(YardMoveRules.IsValidPriority(priority));
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("assigned")]
    [InlineData("rescheduled")]
    public void CanBeCompleted_ReturnsTrue_ForAllowedStatuses(string status)
    {
        Assert.True(YardMoveRules.CanBeCompleted(status));
    }

    [Fact]
    public void CanBeCompleted_ReturnsFalse_ForCompletedStatus()
    {
        Assert.False(YardMoveRules.CanBeCompleted("completed"));
    }
}
