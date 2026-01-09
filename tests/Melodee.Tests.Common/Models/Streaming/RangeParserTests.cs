using Melodee.Common.Models.Streaming;
using Xunit;

namespace Melodee.Tests.Common.Models.Streaming;

public class RangeParserTests
{
    [Fact]
    public void ParseRange_ValidRange_ReturnsRangeInfo()
    {
        var result = RangeParser.ParseRange("bytes=0-499", 1000);
        Assert.NotNull(result);
        Assert.Equal(0, result.Start);
        Assert.Equal(499, result.End);
    }

    [Fact]
    public void ParseRange_StartOnly_ReturnsRangeInfo()
    {
        var result = RangeParser.ParseRange("bytes=500-", 1000);
        Assert.NotNull(result);
        Assert.Equal(500, result.Start);
        Assert.Null(result.End);
    }

    [Fact]
    public void ParseRange_Suffix_ReturnsRangeInfo()
    {
        var result = RangeParser.ParseRange("bytes=-100", 1000);
        Assert.NotNull(result);
        Assert.Equal(900, result.Start);
        Assert.Equal(999, result.End);
    }

    [Fact]
    public void ParseRange_InvalidStart_ReturnsNull()
    {
        var result = RangeParser.ParseRange("bytes=1100-", 1000);
        Assert.Null(result);
    }

    [Fact]
    public void ParseRange_EndLessThanStart_ReturnsNull()
    {
        var result = RangeParser.ParseRange("bytes=500-400", 1000);
        Assert.Null(result);
    }
}
