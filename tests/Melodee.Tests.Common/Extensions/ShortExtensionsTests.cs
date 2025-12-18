using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class ShortExtensionsTests
{
    [Theory]
    [InlineData((short)1, 3, '0', "001")]
    [InlineData((short)42, 5, '0', "00042")]
    [InlineData((short)123, 3, '0', "123")]
    [InlineData((short)1234, 3, '0', "1234")]
    [InlineData((short)5, 4, '*', "***5")]
    public void ToStringPadLeft_NonNullable_ReturnsExpected(short input, short padLeft, char padWith, string expected)
    {
        Assert.Equal(expected, input.ToStringPadLeft(padLeft, padWith));
    }

    [Theory]
    [InlineData(null, 3, '0', null)]
    [InlineData((short)1, 3, '0', "001")]
    [InlineData((short)42, 5, '0', "00042")]
    public void ToStringPadLeft_Nullable_ReturnsExpected(short? input, short padLeft, char padWith, string? expected)
    {
        Assert.Equal(expected, input.ToStringPadLeft(padLeft, padWith));
    }

    [Fact]
    public void ToStringPadLeft_DefaultPadChar_UsesZero()
    {
        short value = 7;
        Assert.Equal("007", value.ToStringPadLeft(3));
    }
}
