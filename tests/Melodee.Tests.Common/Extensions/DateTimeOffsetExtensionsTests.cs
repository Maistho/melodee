using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class DateTimeOffsetExtensionsTests
{
    [Fact]
    public void ToXmlSchemaDateTimeFormat_ReturnsExpectedFormat()
    {
        var dateTimeOffset = new DateTimeOffset(2024, 3, 15, 14, 30, 45, TimeSpan.Zero);
        var result = dateTimeOffset.ToXmlSchemaDateTimeFormat();
        Assert.Equal("2024-03-15T02:30:45", result);
    }

    [Fact]
    public void ToXmlSchemaDateTimeFormat_MinValue_ReturnsValidFormat()
    {
        var result = DateTimeOffset.MinValue.ToXmlSchemaDateTimeFormat();
        Assert.NotNull(result);
        Assert.Contains("0001-01-01", result);
    }

    [Fact]
    public void ToXmlSchemaDateTimeFormat_WithOffset_FormatsCorrectly()
    {
        var dateTimeOffset = new DateTimeOffset(2024, 12, 25, 8, 0, 0, TimeSpan.FromHours(-5));
        var result = dateTimeOffset.ToXmlSchemaDateTimeFormat();
        Assert.NotNull(result);
        Assert.Contains("2024-12-25", result);
    }
}
