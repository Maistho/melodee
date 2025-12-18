using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Utility;

public class TimeInfoTests
{
    [Fact]
    public void Constructor_ZeroMilliseconds_AllValuesZero()
    {
        var timeInfo = new TimeInfo(0);

        Assert.Equal(0, timeInfo.Seconds);
        Assert.Equal(0, timeInfo.Minutes);
        Assert.Equal(0, timeInfo.Hours);
        Assert.Equal(0, timeInfo.Days);
        Assert.Equal(0, timeInfo.Years);
    }

    [Fact]
    public void Constructor_OneSecond_CorrectValues()
    {
        var timeInfo = new TimeInfo(1000);

        Assert.Equal(1, timeInfo.Seconds);
        Assert.Equal(0, timeInfo.Minutes);
        Assert.Equal(0, timeInfo.Hours);
    }

    [Fact]
    public void Constructor_OneMinute_CorrectValues()
    {
        var timeInfo = new TimeInfo(60000);

        Assert.Equal(0, timeInfo.Seconds);
        Assert.Equal(1, timeInfo.Minutes);
        Assert.Equal(0, timeInfo.Hours);
    }

    [Fact]
    public void Constructor_OneHour_CorrectValues()
    {
        var timeInfo = new TimeInfo(3600000);

        Assert.Equal(0, timeInfo.Seconds);
        Assert.Equal(0, timeInfo.Minutes);
        Assert.Equal(1, timeInfo.Hours);
    }

    [Fact]
    public void Constructor_OneDay_CorrectValues()
    {
        var timeInfo = new TimeInfo(86400000);

        Assert.Equal(0, timeInfo.Seconds);
        Assert.Equal(0, timeInfo.Minutes);
        Assert.Equal(0, timeInfo.Hours);
        Assert.Equal(1, timeInfo.Days);
    }

    [Fact]
    public void Constructor_OneYear_CorrectValues()
    {
        var millisecondsInYear = 365m * 24 * 60 * 60 * 1000;
        var timeInfo = new TimeInfo(millisecondsInYear);

        Assert.Equal(1, timeInfo.Years);
        Assert.Equal(0, timeInfo.Days);
    }

    [Fact]
    public void Constructor_ComplexTime_CorrectValues()
    {
        // 1 hour, 30 minutes, 45 seconds
        var milliseconds = (1 * 3600 + 30 * 60 + 45) * 1000m;
        var timeInfo = new TimeInfo(milliseconds);

        Assert.Equal(45, timeInfo.Seconds);
        Assert.Equal(30, timeInfo.Minutes);
        Assert.Equal(1, timeInfo.Hours);
    }

    [Fact]
    public void TotalMinutes_ReturnsCorrectValue()
    {
        var milliseconds = (2 * 3600 + 30 * 60) * 1000m; // 2 hours 30 minutes
        var timeInfo = new TimeInfo(milliseconds);

        Assert.Equal(150, timeInfo.TotalMinutes); // 2*60 + 30 = 150
    }

    [Fact]
    public void SecondsFormatted_ReturnsFormattedString()
    {
        var timeInfo = new TimeInfo(5000); // 5 seconds
        Assert.Equal("05", timeInfo.SecondsFormatted);
    }

    [Fact]
    public void MinutesFormatted_ReturnsFormattedString()
    {
        var timeInfo = new TimeInfo(300000); // 5 minutes
        Assert.Equal("05", timeInfo.MinutesFormatted);
    }

    [Fact]
    public void HoursFormatted_ReturnsFormattedString()
    {
        var timeInfo = new TimeInfo(7200000); // 2 hours
        Assert.Equal("02", timeInfo.HoursFormatted);
    }

    [Fact]
    public void DaysFormatted_ReturnsFormattedString()
    {
        var timeInfo = new TimeInfo(172800000); // 2 days
        Assert.Equal("002", timeInfo.DaysFormatted);
    }

    [Fact]
    public void YearsFormatted_WhenZero_ReturnsNull()
    {
        var timeInfo = new TimeInfo(1000);
        Assert.Null(timeInfo.YearsFormatted);
    }

    [Fact]
    public void YearsFormatted_WhenGreaterThanZero_ReturnsValue()
    {
        var millisecondsInYear = 365m * 24 * 60 * 60 * 1000;
        var timeInfo = new TimeInfo(millisecondsInYear * 2);
        Assert.Equal("2", timeInfo.YearsFormatted);
    }

    [Fact]
    public void ToString_WithoutDays_ReturnsHoursMinutesSeconds()
    {
        var milliseconds = (1 * 3600 + 30 * 60 + 45) * 1000m;
        var timeInfo = new TimeInfo(milliseconds);
        Assert.Equal("01:30:45", timeInfo.ToString());
    }

    [Fact]
    public void ToString_WithDays_ReturnsDaysHoursMinutesSeconds()
    {
        var milliseconds = (25 * 3600 + 30 * 60 + 45) * 1000m; // 1 day, 1 hour, 30 min, 45 sec
        var timeInfo = new TimeInfo(milliseconds);
        Assert.Contains(":", timeInfo.ToString());
        Assert.Contains(timeInfo.DaysFormatted, timeInfo.ToString());
    }

    [Fact]
    public void ToShortFormattedString_ReturnsMinutesSeconds()
    {
        var milliseconds = (5 * 60 + 30) * 1000m; // 5 min 30 sec
        var timeInfo = new TimeInfo(milliseconds);
        Assert.Equal("05:30", timeInfo.ToShortFormattedString());
    }

    [Fact]
    public void ToFullFormattedString_WithoutYears_ReturnsNormalFormat()
    {
        var milliseconds = (1 * 3600 + 30 * 60 + 45) * 1000m;
        var timeInfo = new TimeInfo(milliseconds);
        var result = timeInfo.ToFullFormattedString();
        Assert.Equal("01:30:45", result);
    }

    [Fact]
    public void ToFullFormattedString_WithYears_IncludesYears()
    {
        var millisecondsInYear = 365m * 24 * 60 * 60 * 1000;
        var timeInfo = new TimeInfo(millisecondsInYear);
        var result = timeInfo.ToFullFormattedString();
        Assert.StartsWith("1:", result);
    }
}
