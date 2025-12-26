using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Melodee.Common.Utility;

/// <summary>
/// Provides deterministic text normalization for search/matching.
/// Rules: trim, lower, collapse internal whitespace, remove punctuation.
/// </summary>
public static partial class TextNormalizer
{
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^\p{L}\p{N}\s]", RegexOptions.Compiled)]
    private static partial Regex PunctuationRegex();

    /// <summary>
    /// Normalizes text for search/matching: trim, lowercase, collapse whitespace, remove punctuation.
    /// </summary>
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var result = input.Trim().ToLowerInvariant();
        result = PunctuationRegex().Replace(result, string.Empty);
        result = WhitespaceRegex().Replace(result, " ");
        return result.Trim();
    }

    /// <summary>
    /// Normalizes text with diacritics removal (optional, more aggressive).
    /// </summary>
    public static string? NormalizeWithDiacriticsRemoval(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var normalized = Normalize(input);
        if (normalized is null)
        {
            return null;
        }

        var decomposed = normalized.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
