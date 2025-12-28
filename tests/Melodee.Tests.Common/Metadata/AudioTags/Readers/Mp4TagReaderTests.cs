using System.Reflection;
using System.Text;
using Melodee.Common.Metadata.AudioTags.Readers;

namespace Melodee.Tests.Common.Metadata.AudioTags.Readers;

public class Mp4TagReaderExtractStringValueTests
{
    private readonly MethodInfo _extractStringValueMethod;
    private readonly Mp4TagReader _reader;

    public Mp4TagReaderExtractStringValueTests()
    {
        _reader = new Mp4TagReader();
        _extractStringValueMethod = typeof(Mp4TagReader).GetMethod("ExtractStringValue",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    private string InvokeExtractStringValue(byte[] data)
    {
        return (string)_extractStringValueMethod.Invoke(_reader, [data])!;
    }

    private byte[] CreateMp4DataAtom(string value, int typeIndicator = 1)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value);
        var dataSize = 16 + valueBytes.Length;
        var result = new byte[dataSize];

        result[0] = (byte)(dataSize >> 24);
        result[1] = (byte)(dataSize >> 16);
        result[2] = (byte)(dataSize >> 8);
        result[3] = (byte)dataSize;

        Encoding.ASCII.GetBytes("data").CopyTo(result, 4);

        result[8] = (byte)(typeIndicator >> 24);
        result[9] = (byte)(typeIndicator >> 16);
        result[10] = (byte)(typeIndicator >> 8);
        result[11] = (byte)typeIndicator;

        valueBytes.CopyTo(result, 16);

        return result;
    }

    [Fact]
    public void ExtractStringValue_NullData_ReturnsEmpty()
    {
        var result = InvokeExtractStringValue(null!);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractStringValue_DataTooShort_ReturnsEmpty()
    {
        var data = new byte[] { 0x00, 0x01, 0x02 };
        var result = InvokeExtractStringValue(data);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractStringValue_ValidUtf8_TypeIndicator1_ReturnsString()
    {
        var data = CreateMp4DataAtom("Test Artist", 1);
        var result = InvokeExtractStringValue(data);
        Assert.Equal("Test Artist", result);
    }

    [Fact]
    public void ExtractStringValue_ValidUtf8_TypeIndicator0_ReturnsString()
    {
        var data = CreateMp4DataAtom("Album Name", 0);
        var result = InvokeExtractStringValue(data);
        Assert.Equal("Album Name", result);
    }

    [Fact]
    public void ExtractStringValue_ValidUtf8_TypeIndicator3_ReturnsString()
    {
        var data = CreateMp4DataAtom("Song Title", 3);
        var result = InvokeExtractStringValue(data);
        Assert.Equal("Song Title", result);
    }

    [Fact]
    public void ExtractStringValue_Utf16_TypeIndicator2_ReturnsString()
    {
        var value = "Unicode Title";
        var valueBytes = Encoding.Unicode.GetBytes(value);
        var dataSize = 16 + valueBytes.Length;
        var data = new byte[dataSize];

        data[0] = (byte)(dataSize >> 24);
        data[1] = (byte)(dataSize >> 16);
        data[2] = (byte)(dataSize >> 8);
        data[3] = (byte)dataSize;
        Encoding.ASCII.GetBytes("data").CopyTo(data, 4);
        data[11] = 2;
        valueBytes.CopyTo(data, 16);

        var result = InvokeExtractStringValue(data);
        Assert.Equal(value, result);
    }

    [Fact]
    public void ExtractStringValue_WithNullTerminator_TrimsNull()
    {
        var data = CreateMp4DataAtom("Test\0\0", 1);
        var result = InvokeExtractStringValue(data);
        Assert.Equal("Test", result);
    }

    [Fact]
    public void ExtractStringValue_EmptyString_ReturnsEmpty()
    {
        var data = CreateMp4DataAtom("", 1);
        var result = InvokeExtractStringValue(data);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractStringValue_UnicodeCharacters_ReturnsCorrectly()
    {
        var data = CreateMp4DataAtom("Café ñoño", 1);
        var result = InvokeExtractStringValue(data);
        Assert.Equal("Café ñoño", result);
    }

    [Fact]
    public void ExtractStringValue_LongString_ReturnsComplete()
    {
        var longString = new string('A', 500);
        var data = CreateMp4DataAtom(longString, 1);
        var result = InvokeExtractStringValue(data);
        Assert.Equal(longString, result);
    }

    [Fact]
    public void ExtractStringValue_InvalidDataType_ReturnsEmpty()
    {
        var data = new byte[20];
        data[3] = 0x14;
        Encoding.ASCII.GetBytes("wrng").CopyTo(data, 4);
        var result = InvokeExtractStringValue(data);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractStringValue_SizeMismatch_ReturnsEmpty()
    {
        var data = CreateMp4DataAtom("Test", 1);
        data[3] = 0xFF;
        var result = InvokeExtractStringValue(data);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractStringValue_UnknownTypeIndicator_FallsBackToUtf8()
    {
        var data = CreateMp4DataAtom("Fallback Test", 99);
        var result = InvokeExtractStringValue(data);
        Assert.Equal("Fallback Test", result);
    }

    [Fact]
    public void ExtractStringValue_SpecialCharacters_PreservesCharacters()
    {
        var data = CreateMp4DataAtom("Test: Song! (2020)", 1);
        var result = InvokeExtractStringValue(data);
        Assert.Equal("Test: Song! (2020)", result);
    }
}

public class Mp4TagReaderExtractNumberPairValueTests
{
    private readonly MethodInfo _extractNumberPairValueMethod;
    private readonly Mp4TagReader _reader;

    public Mp4TagReaderExtractNumberPairValueTests()
    {
        _reader = new Mp4TagReader();
        _extractNumberPairValueMethod = typeof(Mp4TagReader).GetMethod("ExtractNumberPairValue",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    private Tuple<int, int> InvokeExtractNumberPairValue(byte[] data)
    {
        return (Tuple<int, int>)_extractNumberPairValueMethod.Invoke(_reader, [data])!;
    }

    private byte[] CreateMp4NumberPairAtom(int firstNumber, int secondNumber)
    {
        var dataSize = 22;
        var data = new byte[dataSize];

        data[0] = (byte)(dataSize >> 24);
        data[1] = (byte)(dataSize >> 16);
        data[2] = (byte)(dataSize >> 8);
        data[3] = (byte)dataSize;

        Encoding.ASCII.GetBytes("data").CopyTo(data, 4);

        data[18] = (byte)(firstNumber >> 8);
        data[19] = (byte)firstNumber;
        data[20] = (byte)(secondNumber >> 8);
        data[21] = (byte)secondNumber;

        return data;
    }

    [Fact]
    public void ExtractNumberPairValue_NullData_ReturnsZeros()
    {
        var result = InvokeExtractNumberPairValue(null!);
        Assert.Equal(0, result.Item1);
        Assert.Equal(0, result.Item2);
    }

    [Fact]
    public void ExtractNumberPairValue_DataTooShort_ReturnsZeros()
    {
        var data = new byte[10];
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(0, result.Item1);
        Assert.Equal(0, result.Item2);
    }

    [Fact]
    public void ExtractNumberPairValue_ValidTrackNumber_ReturnsCorrectly()
    {
        var data = CreateMp4NumberPairAtom(5, 12);
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(5, result.Item1);
        Assert.Equal(12, result.Item2);
    }

    [Fact]
    public void ExtractNumberPairValue_TrackNumberOnly_ReturnsWithZeroTotal()
    {
        var data = CreateMp4NumberPairAtom(7, 0);
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(7, result.Item1);
        Assert.Equal(0, result.Item2);
    }

    [Fact]
    public void ExtractNumberPairValue_MaxTrackNumbers_ReturnsCorrectly()
    {
        var data = CreateMp4NumberPairAtom(255, 255);
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(255, result.Item1);
        Assert.Equal(255, result.Item2);
    }

    [Fact]
    public void ExtractNumberPairValue_SingleDigitTrack_ReturnsCorrectly()
    {
        var data = CreateMp4NumberPairAtom(1, 10);
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(1, result.Item1);
        Assert.Equal(10, result.Item2);
    }

    [Fact]
    public void ExtractNumberPairValue_DiscNumber_ReturnsCorrectly()
    {
        var data = CreateMp4NumberPairAtom(2, 3);
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(2, result.Item1);
        Assert.Equal(3, result.Item2);
    }

    [Fact]
    public void ExtractNumberPairValue_InvalidDataType_ReturnsZeros()
    {
        var data = new byte[22];
        data[3] = 22;
        Encoding.ASCII.GetBytes("wrng").CopyTo(data, 4);
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(0, result.Item1);
        Assert.Equal(0, result.Item2);
    }

    [Fact]
    public void ExtractNumberPairValue_SizeMismatch_ReturnsZeros()
    {
        var data = CreateMp4NumberPairAtom(5, 10);
        data[3] = 0xFF;
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(0, result.Item1);
        Assert.Equal(0, result.Item2);
    }

    [Fact]
    public void ExtractNumberPairValue_BothZero_ReturnsZeros()
    {
        var data = CreateMp4NumberPairAtom(0, 0);
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(0, result.Item1);
        Assert.Equal(0, result.Item2);
    }

    [Fact]
    public void ExtractNumberPairValue_LargeTrackNumbers_ReturnsCorrectly()
    {
        var data = CreateMp4NumberPairAtom(99, 150);
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(99, result.Item1);
        Assert.Equal(150, result.Item2);
    }

    [Theory]
    [InlineData(1, 12)]
    [InlineData(3, 8)]
    [InlineData(15, 20)]
    [InlineData(99, 99)]
    public void ExtractNumberPairValue_VariousValues_ReturnsCorrectly(int track, int total)
    {
        var data = CreateMp4NumberPairAtom(track, total);
        var result = InvokeExtractNumberPairValue(data);
        Assert.Equal(track, result.Item1);
        Assert.Equal(total, result.Item2);
    }
}
