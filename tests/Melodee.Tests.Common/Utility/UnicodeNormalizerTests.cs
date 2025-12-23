using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Utility;

public sealed class UnicodeNormalizerTests
{
    [Fact]
    public void Normalize_WithNullInput_ReturnsEmptyString()
    {
        var result = UnicodeNormalizer.Normalize(null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Normalize_WithWhitespaceInput_ReturnsEmptyString()
    {
        var result = UnicodeNormalizer.Normalize("   ");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Normalize_WithZeroWidthSpace_RemovesInvisibleCharacters()
    {
        var input = "Dy5on\u200BAlison Wade"; // Contains zero-width space
        var result = UnicodeNormalizer.Normalize(input);
        Assert.Equal("Dy5onAlison Wade", result);
    }

    [Fact]
    public void Normalize_WithVariousInvisibleCharacters_RemovesAll()
    {
        var input = "DJ\u200BDean\u200CAirwaze\uFEFF"; // Zero-width space, zero-width non-joiner, zero-width no-break space
        var result = UnicodeNormalizer.Normalize(input);
        Assert.Equal("DJDeanAirwaze", result);
    }

    [Fact]
    public void Normalize_WithRegularText_PreservesText()
    {
        var input = "Normal Artist Name";
        var result = UnicodeNormalizer.Normalize(input);
        Assert.Equal("Normal Artist Name", result);
    }

    [Fact]
    public void Normalize_WithLeadingTrailingWhitespace_TrimsWhitespace()
    {
        var input = "  Artist Name  ";
        var result = UnicodeNormalizer.Normalize(input);
        Assert.Equal("Artist Name", result);
    }

    [Fact]
    public void NormalizeForSearch_WithNullInput_ReturnsEmptyString()
    {
        var result = UnicodeNormalizer.NormalizeForSearch(null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizeForSearch_WithAccentedCharacters_RemovesAccents()
    {
        var input = "Café Résumé Naïve";
        var result = UnicodeNormalizer.NormalizeForSearch(input);
        Assert.Equal("Cafe Resume Naive", result);
    }

    [Fact]
    public void NormalizeForSearch_WithDiacritics_RemovesAll()
    {
        var input = "Àáâãäåçèéêë";
        var result = UnicodeNormalizer.NormalizeForSearch(input);
        Assert.Equal("Aaaaaaceeee", result);
    }

    [Fact]
    public void NormalizeForSearch_WithInvisibleCharacters_RemovesThem()
    {
        var input = "Artist\u200BName\uFEFF";
        var result = UnicodeNormalizer.NormalizeForSearch(input);
        Assert.Equal("ArtistName", result);
    }

    [Fact]
    public void NormalizeForSearch_WithChineseCharacters_PreservesCharacters()
    {
        var input = "华语音乐";
        var result = UnicodeNormalizer.NormalizeForSearch(input);
        Assert.Equal("华语音乐", result);
    }

    [Fact]
    public void Normalize_WithCombinedDiacritics_NormalizesCorrectly()
    {
        var input = "é"; // Can be represented as single char or 'e' + combining acute
        var result = UnicodeNormalizer.Normalize(input);
        Assert.NotEmpty(result);
        Assert.Contains('é', result);
    }
}
