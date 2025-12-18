using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class DecimalExtensionsTests
{
    [Theory]
    [InlineData(1, 3, '0', "001")]
    [InlineData(42, 5, '0', "00042")]
    [InlineData(123, 3, '0', "123")]
    [InlineData(1234, 3, '0', "1234")]
    [InlineData(5, 4, '*', "***5")]
    public void ToStringPadLeft_NonNullable_ReturnsExpected(decimal input, short padLeft, char padWith, string expected)
    {
        Assert.Equal(expected, input.ToStringPadLeft(padLeft, padWith));
    }

    [Fact]
    public void ToStringPadLeft_Nullable_NullInput_ReturnsNull()
    {
        decimal? input = null;
        Assert.Null(input.ToStringPadLeft(3, '0'));
    }

    [Fact]
    public void ToStringPadLeft_Nullable_WithValue_ReturnsPaddedString()
    {
        decimal? input = 1m;
        Assert.Equal("001", input.ToStringPadLeft(3, '0'));
    }

    [Fact]
    public void ToStringPadLeft_Nullable_LargerValue_ReturnsPaddedString()
    {
        decimal? input = 42m;
        Assert.Equal("00042", input.ToStringPadLeft(5, '0'));
    }

    [Fact]
    public void ToStringPadLeft_DefaultPadChar_UsesZero()
    {
        decimal value = 7;
        Assert.Equal("007", value.ToStringPadLeft(3));
    }
}
