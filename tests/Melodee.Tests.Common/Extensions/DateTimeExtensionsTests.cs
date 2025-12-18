using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void ToEtag_ReturnsTicksAsString()
    {
        var dateTime = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var result = dateTime.ToEtag();
        Assert.Equal(dateTime.Ticks.ToString(), result);
    }

    [Fact]
    public void ToEtag_DifferentDates_ReturnDifferentValues()
    {
        var date1 = new DateTime(2024, 1, 15, 10, 30, 0);
        var date2 = new DateTime(2024, 1, 15, 10, 30, 1);

        Assert.NotEqual(date1.ToEtag(), date2.ToEtag());
    }

    [Fact]
    public void ToEtag_MinValue_ReturnsValidString()
    {
        var result = DateTime.MinValue.ToEtag();
        Assert.Equal("0", result);
    }
}
