using ContainerOperationsService.Domain;
using Xunit;

namespace ContainerOperationsService.UnitTests;

public sealed class ContainerStatusRulesTests
{
    [Theory]
    [InlineData("inbound")]
    [InlineData("outbound")]
    [InlineData("hold")]
    [InlineData("customs-release")]
    [InlineData("loaded")]
    [InlineData("unloaded")]
    public void IsValidStatus_ReturnsTrue_ForKnownStatuses(string status)
    {
        Assert.True(ContainerStatusRules.IsValidStatus(status));
    }

    [Fact]
    public void IsValidStatus_ReturnsFalse_ForUnknownStatus()
    {
        Assert.False(ContainerStatusRules.IsValidStatus("flying"));
    }
}
