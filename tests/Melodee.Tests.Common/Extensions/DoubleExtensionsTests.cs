using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Tests.Common.Extensions;

public class DoubleExtensionsTests
{
    [Theory]
    [InlineData(1, 3, '0', "001")]
    [InlineData(42, 5, '0', "00042")]
    [InlineData(123.5, 6, '0', "0123.5")]
    public void ToStringPadLeft_NonNullable_ReturnsExpected(double input, short padLeft, char padWith, string expected)
    {
        Assert.Equal(expected, input.ToStringPadLeft(padLeft, padWith));
    }

    [Theory]
    [InlineData(null, 3, '0', null)]
    [InlineData(1.0, 3, '0', "001")]
    public void ToStringPadLeft_Nullable_ReturnsExpected(double? input, short padLeft, char padWith, string? expected)
    {
        Assert.Equal(expected, input.ToStringPadLeft(padLeft, padWith));
    }

    [Theory]
    [InlineData(1000, 1)]
    [InlineData(2500, 2)]
    [InlineData(60000, 60)]
    public void ToSeconds_ConvertsMillisecondsCorrectly(double input, int expected)
    {
        Assert.Equal(expected, input.ToSeconds());
    }

    [Fact]
    public void ToSeconds_LessThan1000_ReturnsZeroOrOne()
    {
        // 999 / 1000 = 0.999 which rounds to 1 when converted to int
        Assert.Equal(1, (999.0).ToSeconds());
        Assert.Equal(0, (499.0).ToSeconds());
    }

    [Fact]
    public void ToDuration_NullableNull_ReturnsDurationZero()
    {
        double? input = null;
        Assert.Equal(Duration.Zero, input.ToDuration());
    }

    [Fact]
    public void ToDuration_NullableWithValue_ReturnsDuration()
    {
        double? input = 5000;
        var result = input.ToDuration();
        Assert.Equal(Duration.FromMilliseconds(5000), result);
    }

    [Fact]
    public void ToDuration_NonNullable_ReturnsDuration()
    {
        double input = 3000;
        var result = input.ToDuration();
        Assert.Equal(Duration.FromMilliseconds(3000), result);
    }

    [Fact]
    public void ToTimeSpan_NullableNull_ReturnsTimeSpanZero()
    {
        double? input = null;
        Assert.Equal(TimeSpan.Zero, input.ToTimeSpan());
    }

    [Fact]
    public void ToTimeSpan_NullableWithValue_ReturnsTimeSpan()
    {
        double? input = 5000;
        var result = input.ToTimeSpan();
        Assert.Equal(TimeSpan.FromMilliseconds(5000), result);
    }

    [Fact]
    public void ToTimeSpan_Zero_ReturnsTimeSpanZero()
    {
        double input = 0;
        Assert.Equal(TimeSpan.Zero, input.ToTimeSpan());
    }

    [Fact]
    public void ToTimeSpan_NonZero_ReturnsTimeSpan()
    {
        double input = 2500;
        Assert.Equal(TimeSpan.FromMilliseconds(2500), input.ToTimeSpan());
    }

    [Theory]
    [InlineData(3661000, "01:01:01")]
    [InlineData(0, "00:00:00")]
    public void ToFormattedDateTimeOffset_DefaultFormat_ReturnsExpected(double input, string expected)
    {
        Assert.Equal(expected, input.ToFormattedDateTimeOffset());
    }

    [Fact]
    public void ToFormattedDateTimeOffset_CustomFormat_ReturnsExpected()
    {
        var result = (3661000.0).ToFormattedDateTimeOffset("mm\\:ss");
        Assert.Equal("01:01", result);
    }
}
