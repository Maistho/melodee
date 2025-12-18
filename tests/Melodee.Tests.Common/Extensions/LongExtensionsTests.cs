using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class LongExtensionsTests
{
    [Theory]
    [InlineData(1L, 3, '0', "001")]
    [InlineData(42L, 5, '0', "00042")]
    [InlineData(123L, 3, '0', "123")]
    [InlineData(1234L, 3, '0', "1234")]
    [InlineData(5L, 4, '*', "***5")]
    public void ToStringPadLeft_NonNullable_ReturnsExpected(long input, short padLeft, char padWith, string expected)
    {
        Assert.Equal(expected, input.ToStringPadLeft(padLeft, padWith));
    }

    [Theory]
    [InlineData(null, 3, '0', null)]
    [InlineData(1L, 3, '0', "001")]
    [InlineData(42L, 5, '0', "00042")]
    public void ToStringPadLeft_Nullable_ReturnsExpected(long? input, short padLeft, char padWith, string? expected)
    {
        Assert.Equal(expected, input.ToStringPadLeft(padLeft, padWith));
    }

    [Fact]
    public void ToStringPadLeft_DefaultPadChar_UsesZero()
    {
        long value = 7;
        Assert.Equal("007", value.ToStringPadLeft(3));
    }

    [Theory]
    [InlineData(500L, "500 B")]
    [InlineData(899L, "899 B")]
    public void FormatFileSize_Bytes_ReturnsBytes(long size, string expected)
    {
        Assert.Equal(expected, size.FormatFileSize());
    }

    [Theory]
    [InlineData(1024L, "1.00 KB")]
    [InlineData(2048L, "2.00 KB")]
    public void FormatFileSize_Kilobytes_ReturnsKB(long size, string expected)
    {
        Assert.Equal(expected, size.FormatFileSize());
    }

    [Theory]
    [InlineData(1048576L, "1.00 MB")]
    [InlineData(5242880L, "5.00 MB")]
    public void FormatFileSize_Megabytes_ReturnsMB(long size, string expected)
    {
        Assert.Equal(expected, size.FormatFileSize());
    }

    [Fact]
    public void FormatFileSize_Gigabytes_ReturnsGB()
    {
        var size = 1073741824L; // 1 GB
        var result = size.FormatFileSize();
        Assert.Contains("GB", result);
        Assert.StartsWith("1", result);
    }

    [Fact]
    public void FormatFileSize_Terabytes_ReturnsTB()
    {
        var size = 1099511627776L; // 1 TB
        var result = size.FormatFileSize();
        Assert.Contains("TB", result);
    }

    [Fact]
    public void FormatFileSize_Petabytes_ReturnsPB()
    {
        var size = 1125899906842624L; // 1 PB
        var result = size.FormatFileSize();
        Assert.Contains("PB", result);
    }

    [Fact]
    public void FormatFileSize_Zero_ReturnsZeroBytes()
    {
        Assert.Equal("0 B", 0L.FormatFileSize());
    }
}
