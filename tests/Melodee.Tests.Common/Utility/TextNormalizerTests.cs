using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Utility;

public class TextNormalizerTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("Hello World", "hello world")]
    [InlineData("  Hello   World  ", "hello world")]
    [InlineData("AC/DC", "acdc")]
    [InlineData("The Beatles!", "the beatles")]
    [InlineData("Guns N' Roses", "guns n roses")]
    [InlineData("Test...String", "teststring")]
    [InlineData("Rock & Roll", "rock roll")]
    public void Normalize_ReturnsExpectedResult(string? input, string? expected)
    {
        var result = TextNormalizer.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Mötley Crüe", "motley crue")]
    [InlineData("Björk", "bjork")]
    [InlineData("Café", "cafe")]
    [InlineData("naïve", "naive")]
    public void NormalizeWithDiacriticsRemoval_ReturnsExpectedResult(string? input, string? expected)
    {
        var result = TextNormalizer.NormalizeWithDiacriticsRemoval(input);
        Assert.Equal(expected, result);
    }
}
