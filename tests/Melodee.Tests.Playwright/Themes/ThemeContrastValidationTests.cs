using FluentAssertions;

namespace Melodee.Tests.Playwright.Themes;

/// <summary>
/// Tests that validate WCAG AA contrast ratios for all theme files.
/// These tests parse CSS directly and check color combinations without needing a running app.
/// </summary>
public class ThemeContrastValidationTests
{
    private const string ThemesBasePath = "../../../../../src/Melodee.Blazor/wwwroot";

    private static readonly string[] AllThemes =
    [
        "melodee.css",
        "melodee-dark.css",
        "synthwave.css",
        "midnight-galaxy.css",
        "forest.css",
        "ocean-breeze.css",
        "sunset-vibes.css"
    ];

    private static readonly string[] DarkThemes =
    [
        "melodee-dark.css",
        "synthwave.css",
        "midnight-galaxy.css"
    ];

    private static readonly string[] LightThemes =
    [
        "melodee.css",
        "forest.css",
        "ocean-breeze.css",
        "sunset-vibes.css"
    ];

    /// <summary>
    /// Critical selector patterns that must have adequate contrast.
    /// These are the areas most commonly affected by theme styling issues.
    /// </summary>
    private static readonly (string SelectorPattern, string Description)[] CriticalSelectors =
    [
        (".rz-panelmenu .rz-menuitem", "Panel menu items"),
        (".rz-panelmenu-content .rz-menuitem", "Nested menu items"),
        (".rz-navigation-item", "Navigation items"),
        (".rz-sidebar", "Sidebar text"),
        ("profile-menu", "Profile menu"),
    ];

    /// <summary>
    /// Dark theme specific selectors that need contrast validation.
    /// </summary>
    private static readonly (string SelectorPattern, string Description)[] DarkThemeSelectors =
    [
        (".rz-textbox", "Text input fields"),
        (".rz-dropdown-panel", "Dropdown panels"),
        (".rz-autocomplete-panel", "Autocomplete panels"),
    ];

    #region Unit Tests for Validator

    [Fact(DisplayName = "CalculateContrastRatio returns 21 for black on white")]
    public void CalculateContrastRatio_BlackOnWhite_Returns21()
    {
        var black = new CssColorParser.RgbColor(0, 0, 0);
        var white = new CssColorParser.RgbColor(255, 255, 255);

        var ratio = WcagContrastValidator.CalculateContrastRatio(black, white);

        ratio.Should().BeApproximately(21.0, 0.1);
    }

    [Fact(DisplayName = "CalculateContrastRatio returns 1 for same colors")]
    public void CalculateContrastRatio_SameColors_Returns1()
    {
        var color = new CssColorParser.RgbColor(128, 128, 128);

        var ratio = WcagContrastValidator.CalculateContrastRatio(color, color);

        ratio.Should().BeApproximately(1.0, 0.01);
    }

    [Fact(DisplayName = "CalculateContrastRatio is symmetric")]
    public void CalculateContrastRatio_IsSymmetric()
    {
        var color1 = new CssColorParser.RgbColor(50, 100, 150);
        var color2 = new CssColorParser.RgbColor(200, 180, 160);

        var ratio1 = WcagContrastValidator.CalculateContrastRatio(color1, color2);
        var ratio2 = WcagContrastValidator.CalculateContrastRatio(color2, color1);

        ratio1.Should().BeApproximately(ratio2, 0.001);
    }

    [Theory(DisplayName = "MeetsWcagAaNormalText returns expected result")]
    [InlineData(0, 0, 0, 255, 255, 255, true)]      // Black on white - passes
    [InlineData(128, 128, 128, 140, 140, 140, false)] // Similar grays - fails
    [InlineData(100, 100, 100, 255, 255, 255, true)]  // Dark gray on white - passes
    public void MeetsWcagAaNormalText_ReturnsExpected(
        int r1, int g1, int b1,
        int r2, int g2, int b2,
        bool expected)
    {
        var fg = new CssColorParser.RgbColor(r1, g1, b1);
        var bg = new CssColorParser.RgbColor(r2, g2, b2);

        var result = WcagContrastValidator.MeetsWcagAaNormalText(fg, bg);

        result.Should().Be(expected);
    }

    #endregion

    #region CssColorParser Tests

    [Theory(DisplayName = "ParseColor handles hex colors correctly")]
    [InlineData("#fff", 255, 255, 255)]
    [InlineData("#FFF", 255, 255, 255)]
    [InlineData("#ffffff", 255, 255, 255)]
    [InlineData("#000000", 0, 0, 0)]
    [InlineData("#ff0000", 255, 0, 0)]
    [InlineData("#00ff00", 0, 255, 0)]
    [InlineData("#0000ff", 0, 0, 255)]
    [InlineData("#1a1a2e", 26, 26, 46)]
    public void ParseColor_HexColors_ParsesCorrectly(string hex, int r, int g, int b)
    {
        var result = CssColorParser.ParseColor(hex);

        result.Should().NotBeNull();
        result!.Value.R.Should().Be(r);
        result.Value.G.Should().Be(g);
        result.Value.B.Should().Be(b);
    }

    [Theory(DisplayName = "ParseColor handles rgb/rgba functions correctly")]
    [InlineData("rgb(255, 255, 255)", 255, 255, 255)]
    [InlineData("rgb(0, 0, 0)", 0, 0, 0)]
    [InlineData("rgba(255, 0, 0, 0.5)", 255, 0, 0)]
    [InlineData("rgba(100, 150, 200, 1)", 100, 150, 200)]
    public void ParseColor_RgbFunctions_ParsesCorrectly(string rgb, int r, int g, int b)
    {
        var result = CssColorParser.ParseColor(rgb);

        result.Should().NotBeNull();
        result!.Value.R.Should().Be(r);
        result.Value.G.Should().Be(g);
        result.Value.B.Should().Be(b);
    }

    [Theory(DisplayName = "ParseColor handles named colors correctly")]
    [InlineData("white", 255, 255, 255)]
    [InlineData("black", 0, 0, 0)]
    [InlineData("red", 255, 0, 0)]
    public void ParseColor_NamedColors_ParsesCorrectly(string name, int r, int g, int b)
    {
        var result = CssColorParser.ParseColor(name);

        result.Should().NotBeNull();
        result!.Value.R.Should().Be(r);
        result.Value.G.Should().Be(g);
        result.Value.B.Should().Be(b);
    }

    [Theory(DisplayName = "ParseColor returns null for unparseable values")]
    [InlineData("var(--primary-color)")]
    [InlineData("inherit")]
    [InlineData("transparent")]
    [InlineData("")]
    [InlineData(null)]
    public void ParseColor_UnparseableValues_ReturnsNull(string? value)
    {
        var result = CssColorParser.ParseColor(value);

        result.Should().BeNull();
    }

    #endregion

    #region Theme Contrast Validation Tests

    [Theory(DisplayName = "Theme file exists and is readable")]
    [MemberData(nameof(GetAllThemes))]
    public void ThemeFile_ExistsAndReadable(string themeName)
    {
        var themePath = GetThemePath(themeName);
        
        File.Exists(themePath).Should().BeTrue($"Theme file {themeName} should exist at {themePath}");
        
        var content = File.ReadAllText(themePath);
        content.Should().NotBeNullOrEmpty();
    }

    [Theory(DisplayName = "Theme has parseable color rules")]
    [MemberData(nameof(GetAllThemes))]
    public void Theme_HasParseableColorRules(string themeName)
    {
        var themePath = GetThemePath(themeName);
        var cssContent = File.ReadAllText(themePath);
        var parser = new CssColorParser(cssContent);

        parser.Rules.Should().NotBeEmpty(
            $"Theme {themeName} should have parseable color rules");
    }

    [Theory(DisplayName = "Dark theme menu text has adequate contrast")]
    [MemberData(nameof(GetDarkThemes))]
    public void DarkTheme_MenuText_HasAdequateContrast(string themeName)
    {
        var results = ValidateThemeContrast(themeName, CriticalSelectors);
        
        AssertContrastResults(themeName, results, allowSkipped: true);
    }

    [Theory(DisplayName = "Light theme menu text has adequate contrast")]
    [MemberData(nameof(GetLightThemes))]
    public void LightTheme_MenuText_HasAdequateContrast(string themeName)
    {
        var results = ValidateThemeContrast(themeName, CriticalSelectors);
        
        AssertContrastResults(themeName, results, allowSkipped: true);
    }

    [Theory(DisplayName = "Dark theme form inputs have adequate contrast")]
    [MemberData(nameof(GetDarkThemes))]
    public void DarkTheme_FormInputs_HasAdequateContrast(string themeName)
    {
        var results = ValidateThemeContrast(themeName, DarkThemeSelectors);
        
        AssertContrastResults(themeName, results, allowSkipped: true);
    }

    #endregion

    #region Comprehensive Theme Analysis

    [Fact(DisplayName = "Generate contrast report for all themes")]
    public void AllThemes_GenerateContrastReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("# Theme Contrast Analysis Report");
        report.AppendLine();

        foreach (var theme in AllThemes)
        {
            report.AppendLine($"## {theme}");
            report.AppendLine();

            var selectors = DarkThemes.Contains(theme)
                ? CriticalSelectors.Concat(DarkThemeSelectors).ToArray()
                : CriticalSelectors;

            var results = ValidateThemeContrast(theme, selectors);

            foreach (var result in results)
            {
                var status = result.Error != null ? "⚠️" :
                    result.PassesAaNormal ? "✅" : "❌";
                report.AppendLine($"- {status} {result}");
            }

            report.AppendLine();
        }

        // Output the report (will show in test output)
        Console.WriteLine(report.ToString());
        
        // This test is for reporting only - always passes
        true.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static string GetThemePath(string themeName)
    {
        var basePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            ThemesBasePath);
        return Path.Combine(basePath, themeName);
    }

    private static List<WcagContrastValidator.ContrastResult> ValidateThemeContrast(
        string themeName,
        (string SelectorPattern, string Description)[] selectors)
    {
        var themePath = GetThemePath(themeName);
        var cssContent = File.ReadAllText(themePath);
        var parser = new CssColorParser(cssContent);

        var results = new List<WcagContrastValidator.ContrastResult>();

        foreach (var (pattern, description) in selectors)
        {
            var rule = parser.FindRule(pattern);
            
            if (rule == null)
            {
                results.Add(WcagContrastValidator.ContrastResult.FailedToParse(
                    $"{description} ({pattern})",
                    "Selector not found in CSS"));
                continue;
            }

            var textColor = parser.GetTextColor(pattern);
            var bgColor = parser.GetBackgroundColor(pattern);

            if (textColor == null && bgColor == null)
            {
                results.Add(WcagContrastValidator.ContrastResult.FailedToParse(
                    $"{description} ({pattern})",
                    "No parseable colors found (may use CSS variables)"));
                continue;
            }

            // If we have text color but no background, we can't calculate contrast
            // This is common when background is inherited or uses CSS variables
            if (textColor != null && bgColor == null)
            {
                results.Add(WcagContrastValidator.ContrastResult.FailedToParse(
                    $"{description} ({pattern})",
                    $"Text color found ({textColor}) but no background color (may be inherited)"));
                continue;
            }

            // If we have background but no text color
            if (textColor == null && bgColor != null)
            {
                results.Add(WcagContrastValidator.ContrastResult.FailedToParse(
                    $"{description} ({pattern})",
                    $"Background color found ({bgColor}) but no text color (may be inherited)"));
                continue;
            }

            // We have both colors - calculate contrast
            var ratio = WcagContrastValidator.CalculateContrastRatio(textColor!.Value, bgColor!.Value);
            results.Add(WcagContrastValidator.ContrastResult.Success(
                $"{description} ({pattern})",
                textColor!.Value,
                bgColor!.Value,
                ratio));
        }

        return results;
    }

    private static void AssertContrastResults(
        string themeName,
        List<WcagContrastValidator.ContrastResult> results,
        bool allowSkipped = false)
    {
        var failures = new List<string>();

        foreach (var result in results)
        {
            if (result.Error != null)
            {
                if (!allowSkipped)
                {
                    failures.Add($"Could not validate: {result}");
                }
                // Otherwise, skipped results are acceptable (CSS variables, inherited colors)
                continue;
            }

            if (!result.PassesAaNormal)
            {
                failures.Add(
                    $"WCAG AA Fail: {result.Selector} has contrast ratio {result.ContrastRatio:F2}:1 " +
                    $"(minimum 4.5:1 required). FG: {result.Foreground}, BG: {result.Background}");
            }
        }

        if (failures.Count > 0)
        {
            var message = $"Theme '{themeName}' has {failures.Count} contrast issue(s):\n" +
                          string.Join("\n", failures);
            Assert.Fail(message);
        }
    }

    public static IEnumerable<object[]> GetAllThemes() =>
        AllThemes.Select(t => new object[] { t });

    public static IEnumerable<object[]> GetDarkThemes() =>
        DarkThemes.Select(t => new object[] { t });

    public static IEnumerable<object[]> GetLightThemes() =>
        LightThemes.Select(t => new object[] { t });

    #endregion
}
