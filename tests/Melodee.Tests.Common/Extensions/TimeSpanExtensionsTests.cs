using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class TimeSpanExtensionsTests
{
    [Fact]
    public void ToYearDaysMinutesHours_Zero_ReturnsAllZeros()
    {
        var result = TimeSpan.Zero.ToYearDaysMinutesHours();
        Assert.Equal("000:000:00:00", result);
    }

    [Fact]
    public void ToYearDaysMinutesHours_OneYear_ReturnsExpected()
    {
        var timeSpan = TimeSpan.FromDays(365);
        var result = timeSpan.ToYearDaysMinutesHours();
        Assert.Equal("001:000:00:00", result);
    }

    [Fact]
    public void ToYearDaysMinutesHours_OneYearOneDayOneHourOneMinute_ReturnsExpected()
    {
        var timeSpan = TimeSpan.FromDays(366) + TimeSpan.FromHours(1) + TimeSpan.FromMinutes(1);
        var result = timeSpan.ToYearDaysMinutesHours();
        Assert.Equal("001:001:01:01", result);
    }

    [Fact]
    public void ToYearDaysMinutesHours_JustHoursAndMinutes_ReturnsExpected()
    {
        var timeSpan = TimeSpan.FromHours(5) + TimeSpan.FromMinutes(30);
        var result = timeSpan.ToYearDaysMinutesHours();
        Assert.Equal("000:000:05:30", result);
    }

    [Fact]
    public void ToYearDaysMinutesHours_LargeValue_ReturnsExpected()
    {
        var timeSpan = TimeSpan.FromDays(730) + TimeSpan.FromHours(12) + TimeSpan.FromMinutes(45);
        var result = timeSpan.ToYearDaysMinutesHours();
        Assert.Equal("002:000:12:45", result);
    }
}
