using System.Text;
using System.Text.RegularExpressions;

namespace Melodee.Common.Utility;

/// <summary>
///     Utility for normalizing Unicode strings to handle special characters and invisible characters
/// </summary>
public static partial class UnicodeNormalizer
{
    [GeneratedRegex(@"\p{Cf}", RegexOptions.Compiled)]
    private static partial Regex InvisibleCharactersRegex();

    [GeneratedRegex(@"\p{Mn}", RegexOptions.Compiled)]
    private static partial Regex NonSpacingMarksRegex();

    /// <summary>
    ///     Normalize a string by removing invisible characters and normalizing Unicode
    /// </summary>
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // First normalize to FormC (Canonical Composition)
        var normalized = input.Normalize(NormalizationForm.FormC);

        // Remove zero-width spaces and other format characters (Unicode category Cf)
        normalized = InvisibleCharactersRegex().Replace(normalized, string.Empty);

        // Trim whitespace
        normalized = normalized.Trim();

        return normalized;
    }

    /// <summary>
    ///     Normalize and remove accents/diacritics for searching
    /// </summary>
    public static string NormalizeForSearch(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // First apply basic normalization
        var normalized = Normalize(input);

        // Decompose to FormD (Canonical Decomposition)
        normalized = normalized.Normalize(NormalizationForm.FormD);

        // Remove non-spacing marks (accents/diacritics)
        normalized = NonSpacingMarksRegex().Replace(normalized, string.Empty);

        // Recompose to FormC
        normalized = normalized.Normalize(NormalizationForm.FormC);

        return normalized;
    }
}
