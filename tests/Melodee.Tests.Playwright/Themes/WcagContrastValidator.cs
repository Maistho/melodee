namespace Melodee.Tests.Playwright.Themes;

/// <summary>
/// Validates color contrast ratios according to WCAG 2.1 guidelines.
/// </summary>
public static class WcagContrastValidator
{
    /// <summary>
    /// WCAG AA minimum contrast ratio for normal text (less than 18pt or 14pt bold).
    /// </summary>
    public const double WcagAaNormalText = 4.5;

    /// <summary>
    /// WCAG AA minimum contrast ratio for large text (18pt+ or 14pt+ bold).
    /// </summary>
    public const double WcagAaLargeText = 3.0;

    /// <summary>
    /// WCAG AAA minimum contrast ratio for normal text.
    /// </summary>
    public const double WcagAaaNormalText = 7.0;

    /// <summary>
    /// WCAG AAA minimum contrast ratio for large text.
    /// </summary>
    public const double WcagAaaLargeText = 4.5;

    /// <summary>
    /// Calculates the contrast ratio between two colors.
    /// Returns a value between 1 (no contrast) and 21 (maximum contrast).
    /// </summary>
    public static double CalculateContrastRatio(CssColorParser.RgbColor foreground, CssColorParser.RgbColor background)
    {
        var luminanceFg = CalculateRelativeLuminance(foreground);
        var luminanceBg = CalculateRelativeLuminance(background);

        var lighter = Math.Max(luminanceFg, luminanceBg);
        var darker = Math.Min(luminanceFg, luminanceBg);

        return (lighter + 0.05) / (darker + 0.05);
    }

    /// <summary>
    /// Calculates the relative luminance of an RGB color.
    /// Formula: https://www.w3.org/TR/WCAG21/#dfn-relative-luminance
    /// </summary>
    public static double CalculateRelativeLuminance(CssColorParser.RgbColor color)
    {
        var r = NormalizeColorChannel(color.R);
        var g = NormalizeColorChannel(color.G);
        var b = NormalizeColorChannel(color.B);

        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    /// <summary>
    /// Normalizes an 8-bit color channel value (0-255) to the sRGB luminance scale.
    /// </summary>
    private static double NormalizeColorChannel(int value)
    {
        var normalized = value / 255.0;
        
        if (normalized <= 0.03928)
        {
            return normalized / 12.92;
        }
        
        return Math.Pow((normalized + 0.055) / 1.055, 2.4);
    }

    /// <summary>
    /// Checks if a color pair meets WCAG AA requirements for normal text.
    /// </summary>
    public static bool MeetsWcagAaNormalText(CssColorParser.RgbColor foreground, CssColorParser.RgbColor background)
    {
        return CalculateContrastRatio(foreground, background) >= WcagAaNormalText;
    }

    /// <summary>
    /// Checks if a color pair meets WCAG AA requirements for large text.
    /// </summary>
    public static bool MeetsWcagAaLargeText(CssColorParser.RgbColor foreground, CssColorParser.RgbColor background)
    {
        return CalculateContrastRatio(foreground, background) >= WcagAaLargeText;
    }

    /// <summary>
    /// Gets a human-readable rating of the contrast ratio.
    /// </summary>
    public static string GetContrastRating(double ratio)
    {
        return ratio switch
        {
            >= WcagAaaNormalText => "AAA (Excellent)",
            >= WcagAaNormalText => "AA (Good)",
            >= WcagAaLargeText => "AA Large Text Only",
            _ => "Fail (Poor)"
        };
    }

    /// <summary>
    /// Represents a contrast check result with detailed information.
    /// </summary>
    public record ContrastResult(
        string Selector,
        CssColorParser.RgbColor? Foreground,
        CssColorParser.RgbColor? Background,
        double? ContrastRatio,
        bool PassesAaNormal,
        bool PassesAaLarge,
        string? Error)
    {
        public static ContrastResult Success(
            string selector,
            CssColorParser.RgbColor foreground,
            CssColorParser.RgbColor background,
            double ratio)
        {
            return new ContrastResult(
                selector,
                foreground,
                background,
                ratio,
                ratio >= WcagAaNormalText,
                ratio >= WcagAaLargeText,
                null);
        }

        public static ContrastResult FailedToParse(string selector, string error)
        {
            return new ContrastResult(selector, null, null, null, false, false, error);
        }

        public override string ToString()
        {
            if (Error != null)
                return $"{Selector}: {Error}";

            var rating = GetContrastRating(ContrastRatio ?? 0);
            return $"{Selector}: {ContrastRatio:F2}:1 ({rating}) - FG: {Foreground}, BG: {Background}";
        }
    }
}
