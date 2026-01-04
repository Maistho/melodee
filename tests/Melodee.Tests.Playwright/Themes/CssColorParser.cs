using System.Globalization;
using System.Text.RegularExpressions;

namespace Melodee.Tests.Playwright.Themes;

/// <summary>
/// Parses CSS files and extracts color values for specific selectors.
/// Used for WCAG contrast ratio validation.
/// </summary>
public partial class CssColorParser
{
    private readonly string _cssContent;
    private readonly Dictionary<string, CssRule> _rules = new();

    public CssColorParser(string cssContent)
    {
        _cssContent = cssContent;
        Parse();
    }

    /// <summary>
    /// Represents a parsed CSS rule with color properties.
    /// </summary>
    public record CssRule(
        string Selector,
        string? Color,
        string? BackgroundColor,
        string? Background);

    /// <summary>
    /// Represents an RGB color.
    /// </summary>
    public readonly record struct RgbColor(int R, int G, int B)
    {
        public static RgbColor White => new(255, 255, 255);
        public static RgbColor Black => new(0, 0, 0);
        
        public override string ToString() => $"rgb({R}, {G}, {B})";
    }

    /// <summary>
    /// Gets all parsed CSS rules.
    /// </summary>
    public IReadOnlyDictionary<string, CssRule> Rules => _rules;

    /// <summary>
    /// Gets the color property for a selector, searching with partial matching.
    /// </summary>
    public RgbColor? GetTextColor(string selectorPattern)
    {
        var rule = FindRule(selectorPattern);
        if (rule?.Color == null) return null;
        return ParseColor(rule.Color);
    }

    /// <summary>
    /// Gets the background color for a selector, searching with partial matching.
    /// </summary>
    public RgbColor? GetBackgroundColor(string selectorPattern)
    {
        var rule = FindRule(selectorPattern);
        var colorValue = rule?.BackgroundColor ?? rule?.Background;
        if (colorValue == null) return null;
        return ParseColor(colorValue);
    }

    /// <summary>
    /// Finds a rule matching the selector pattern.
    /// </summary>
    public CssRule? FindRule(string selectorPattern)
    {
        // Try exact match first
        if (_rules.TryGetValue(selectorPattern, out var exactRule))
            return exactRule;

        // Try partial match
        foreach (var (selector, rule) in _rules)
        {
            if (selector.Contains(selectorPattern, StringComparison.OrdinalIgnoreCase))
                return rule;
        }

        return null;
    }

    /// <summary>
    /// Gets all rules matching a selector pattern.
    /// </summary>
    public IEnumerable<CssRule> FindRules(string selectorPattern)
    {
        return _rules
            .Where(kvp => kvp.Key.Contains(selectorPattern, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Value);
    }

    /// <summary>
    /// Parses a CSS color value to RGB.
    /// Supports: hex (#RGB, #RRGGBB), rgb(), rgba(), and named colors.
    /// </summary>
    public static RgbColor? ParseColor(string? colorValue)
    {
        if (string.IsNullOrWhiteSpace(colorValue))
            return null;

        colorValue = colorValue.Trim().ToLowerInvariant();
        
        // Remove !important
        colorValue = colorValue.Replace("!important", "").Trim();

        // Handle hex colors
        if (colorValue.StartsWith('#'))
        {
            return ParseHexColor(colorValue);
        }

        // Handle rgb/rgba
        if (colorValue.StartsWith("rgb"))
        {
            return ParseRgbFunction(colorValue);
        }

        // Handle CSS variables - can't resolve these statically
        if (colorValue.StartsWith("var("))
        {
            return null;
        }

        // Handle gradients - extract first color
        if (colorValue.StartsWith("linear-gradient") || colorValue.StartsWith("radial-gradient"))
        {
            return ParseGradientFirstColor(colorValue);
        }

        // Handle named colors
        return ParseNamedColor(colorValue);
    }

    private static RgbColor? ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');

        if (hex.Length == 3)
        {
            // #RGB -> #RRGGBB
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        if (hex.Length == 6 || hex.Length == 8)
        {
            if (int.TryParse(hex[..2], NumberStyles.HexNumber, null, out var r) &&
                int.TryParse(hex[2..4], NumberStyles.HexNumber, null, out var g) &&
                int.TryParse(hex[4..6], NumberStyles.HexNumber, null, out var b))
            {
                return new RgbColor(r, g, b);
            }
        }

        return null;
    }

    private static RgbColor? ParseRgbFunction(string rgb)
    {
        var match = RgbFunctionRegex().Match(rgb);
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out var r) &&
                int.TryParse(match.Groups[2].Value, out var g) &&
                int.TryParse(match.Groups[3].Value, out var b))
            {
                return new RgbColor(r, g, b);
            }
        }

        return null;
    }

    private static RgbColor? ParseGradientFirstColor(string gradient)
    {
        // Extract first color from gradient
        var hexMatch = HexColorInGradientRegex().Match(gradient);
        if (hexMatch.Success)
        {
            return ParseHexColor(hexMatch.Value);
        }

        var rgbMatch = RgbInGradientRegex().Match(gradient);
        if (rgbMatch.Success)
        {
            return ParseRgbFunction(rgbMatch.Value);
        }

        return null;
    }

    private static RgbColor? ParseNamedColor(string name)
    {
        return name switch
        {
            "white" => new RgbColor(255, 255, 255),
            "black" => new RgbColor(0, 0, 0),
            "red" => new RgbColor(255, 0, 0),
            "green" => new RgbColor(0, 128, 0),
            "blue" => new RgbColor(0, 0, 255),
            "yellow" => new RgbColor(255, 255, 0),
            "cyan" or "aqua" => new RgbColor(0, 255, 255),
            "magenta" or "fuchsia" => new RgbColor(255, 0, 255),
            "gray" or "grey" => new RgbColor(128, 128, 128),
            "transparent" => null,
            _ => null
        };
    }

    private void Parse()
    {
        // Match CSS rules: selector { properties }
        var ruleMatches = CssRuleRegex().Matches(_cssContent);

        foreach (Match match in ruleMatches)
        {
            var selector = match.Groups[1].Value.Trim();
            var properties = match.Groups[2].Value;

            // Skip @-rules, :root, etc.
            if (selector.StartsWith('@') || selector == ":root")
                continue;

            var color = ExtractProperty(properties, "color");
            var backgroundColor = ExtractProperty(properties, "background-color");
            var background = ExtractProperty(properties, "background");

            // Only add if we found color-related properties
            if (color != null || backgroundColor != null || background != null)
            {
                _rules[selector] = new CssRule(selector, color, backgroundColor, background);
            }
        }
    }

    private static string? ExtractProperty(string properties, string propertyName)
    {
        // Match property: value; or property: value}
        // Use word boundary to avoid matching "background-color" when looking for "color"
        // For "color", we need negative lookbehind to exclude "background-color"
        var pattern = propertyName == "color"
            ? @"(?<!background-)(?<!-)color\s*:\s*([^;}\s]+)"
            : $@"(?:^|[;\s]){Regex.Escape(propertyName)}\s*:\s*([^;}}]+)";
        var match = Regex.Match(properties, pattern, RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            var value = match.Groups[1].Value.Trim();
            // Don't return CSS variables as we can't resolve them
            if (value.StartsWith("var("))
                return null;
            return value;
        }

        return null;
    }

    [GeneratedRegex(@"([^{}]+)\s*\{([^{}]*)\}", RegexOptions.Singleline)]
    private static partial Regex CssRuleRegex();

    [GeneratedRegex(@"rgba?\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)")]
    private static partial Regex RgbFunctionRegex();

    [GeneratedRegex(@"#[0-9a-fA-F]{3,8}")]
    private static partial Regex HexColorInGradientRegex();

    [GeneratedRegex(@"rgba?\s*\([^)]+\)")]
    private static partial Regex RgbInGradientRegex();
}
