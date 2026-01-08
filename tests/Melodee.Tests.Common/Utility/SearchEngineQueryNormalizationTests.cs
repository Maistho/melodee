using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Utility;

/// <summary>
///     Tests for SearchEngineQueryNormalization utility class.
/// </summary>
public class SearchEngineQueryNormalizationTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("  \t\n  ", null)]
    public void NormalizeQuery_ReturnsNullForEmptyInput(string? input, string? expected)
    {
        var result = SearchEngineQueryNormalization.NormalizeQuery(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", "test")]
    [InlineData("  test  ", "test")]
    [InlineData("test name", "test name")]
    [InlineData("test\tname", "test name")]
    public void NormalizeQuery_TrimsWhitespace(string input, string expected)
    {
        var result = SearchEngineQueryNormalization.NormalizeQuery(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test    name", "test name")]
    [InlineData("test\t\t\tname", "test name")]
    [InlineData("  test   name  ", "test name")]
    public void NormalizeQuery_CollapsesWhitespace(string input, string expected)
    {
        var result = SearchEngineQueryNormalization.NormalizeQuery(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test name", "test  name")]
    [InlineData("test   name", "test  name")]
    public void NormalizeQuery_DoesNotCollapseWhitespaceWhenDisabled(string input, string notExpected)
    {
        var result = SearchEngineQueryNormalization.NormalizeQuery(input, collapseWhitespace: false);
        Assert.NotEqual(notExpected, result);
    }

    [Theory]
    [InlineData("a", "a")]
    [InlineData("ab", "ab")]
    [InlineData("abc", "abc")]
    public void NormalizeQuery_HandlesShortStrings(string input, string expected)
    {
        var result = SearchEngineQueryNormalization.NormalizeQuery(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeQuery_CapsLengthAt256Characters()
    {
        var input = new string('a', 300);
        var result = SearchEngineQueryNormalization.NormalizeQuery(input);
        Assert.NotNull(result);
        Assert.Equal(256, result.Length);
    }

    [Fact]
    public void NormalizeQuery_TruncatesLongInputWithSpaces()
    {
        var input = new string('a', 200) + " " + new string('b', 100);
        var result = SearchEngineQueryNormalization.NormalizeQuery(input);
        Assert.NotNull(result);
        Assert.Equal(256, result.Length);
        Assert.Contains("aaaa", result);
    }

    [Theory]
    [InlineData("123456", true)]
    [InlineData("0", true)]
    [InlineData("12345678901234567890", true)]
    public void ValidateAmgId_ReturnsTrueForDigitOnlyStrings(string amgId, bool expected)
    {
        var result = SearchEngineQueryNormalization.ValidateAmgId(amgId);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("abc", false)]
    [InlineData("123abc", false)]
    [InlineData("123-456", false)]
    [InlineData("123.456", false)]
    public void ValidateAmgId_ReturnsFalseForNonDigitStrings(string? amgId, bool expected)
    {
        var result = SearchEngineQueryNormalization.ValidateAmgId(amgId);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidateAmgId_ReturnsFalseForTooLongString()
    {
        var amgId = new string('1', 25); // Max is 20
        var result = SearchEngineQueryNormalization.ValidateAmgId(amgId);
        Assert.False(result);
    }

    [Fact]
    public void ValidateAmgId_ReturnsTrueForMaxLengthString()
    {
        var amgId = new string('1', 20); // Exactly max length
        var result = SearchEngineQueryNormalization.ValidateAmgId(amgId);
        Assert.True(result);
    }

    [Fact]
    public void ValidateQuery_ReturnsNormalizedDataForValidInput()
    {
        var result = SearchEngineQueryNormalization.ValidateQuery("  test query  ");
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("test query", result.Data);
    }

    [Fact]
    public void ValidateQuery_ReturnsErrorForEmptyInput()
    {
        var result = SearchEngineQueryNormalization.ValidateQuery("");
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("cannot be empty"));
    }

    [Fact]
    public void ValidateQuery_ReturnsErrorForWhitespaceOnly()
    {
        var result = SearchEngineQueryNormalization.ValidateQuery("   \t\n  ");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ValidateQuery_UsesCustomParameterName()
    {
        var result = SearchEngineQueryNormalization.ValidateQuery("", "artistName");
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("artistName"));
    }

    [Fact]
    public void ValidateAmgIdResult_ReturnsSuccessForValidAmgId()
    {
        var result = SearchEngineQueryNormalization.ValidateAmgIdResult("123456");
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public void ValidateAmgIdResult_ReturnsErrorForInvalidAmgId()
    {
        var result = SearchEngineQueryNormalization.ValidateAmgIdResult("abc123");
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("only digits"));
    }

    [Fact]
    public void ValidateAmgIdResult_ReturnsErrorForEmptyAmgId()
    {
        var result = SearchEngineQueryNormalization.ValidateAmgIdResult("");
        Assert.False(result.IsSuccess);
    }
}
