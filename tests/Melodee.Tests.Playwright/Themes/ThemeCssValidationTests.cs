namespace Melodee.Tests.Playwright.Themes;

/// <summary>
/// Unit tests that validate CSS theme files contain required styles.
/// These tests don't require a running browser - they parse CSS files directly.
/// </summary>
public class ThemeCssValidationTests
{
    private static readonly string ThemesDirectory = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "..", "..", "..", "..", "..",
        "src", "Melodee.Blazor", "wwwroot");
    
    private static readonly string[] RequiredThemeFiles =
    [
        "melodee.css",
        "melodee-dark.css",
        "synthwave.css",
        "midnight-galaxy.css",
        "forest.css",
        "ocean-breeze.css",
        "sunset-vibes.css"
    ];
    
    private static readonly string[] RequiredPanelMenuSelectors =
    [
        ".rz-panelmenu",
        ".rz-panelmenu-header",
        ".rz-panelmenu-content",
        ".rz-panelmenu-content .rz-menuitem"
    ];
    
    private static readonly string[] RequiredNestedMenuSelectors =
    [
        ".rz-panelmenu .rz-navigation-item",
        ".rz-panelmenu .rz-menuitem-text",
        ".rz-panelmenu-panel .rz-menuitem-link"
    ];
    
    private static readonly string[] RequiredProfileMenuSelectors =
    [
        ".rz-profile-menu",
        ".rz-profile-menu .rz-menuitem"
    ];
    
    private static readonly string[] RequiredHeaderSelectors =
    [
        ".rz-header",
        ".rz-layout-header"
    ];

    public static IEnumerable<object[]> ThemeFilesData => 
        RequiredThemeFiles.Select(f => new object[] { f });

    [Theory]
    [MemberData(nameof(ThemeFilesData))]
    public void ThemeFile_ShouldExist(string themeFileName)
    {
        var filePath = Path.Combine(ThemesDirectory, themeFileName);
        File.Exists(filePath).Should().BeTrue($"Theme file '{themeFileName}' should exist at {filePath}");
    }

    [Theory]
    [MemberData(nameof(ThemeFilesData))]
    public void ThemeFile_ShouldContainPanelMenuStyles(string themeFileName)
    {
        var css = ReadThemeCss(themeFileName);
        
        foreach (var selector in RequiredPanelMenuSelectors)
        {
            css.Should().Contain(selector,
                $"Theme '{themeFileName}' should contain panel menu selector '{selector}'");
        }
    }

    [Theory]
    [MemberData(nameof(ThemeFilesData))]
    public void ThemeFile_ShouldContainNestedMenuStyles(string themeFileName)
    {
        var css = ReadThemeCss(themeFileName);
        
        foreach (var selector in RequiredNestedMenuSelectors)
        {
            css.Should().Contain(selector,
                $"Theme '{themeFileName}' should contain nested menu selector '{selector}' for Admin submenu visibility");
        }
    }

    [Theory]
    [MemberData(nameof(ThemeFilesData))]
    public void ThemeFile_ShouldContainProfileMenuStyles(string themeFileName)
    {
        var css = ReadThemeCss(themeFileName);
        
        foreach (var selector in RequiredProfileMenuSelectors)
        {
            css.Should().Contain(selector,
                $"Theme '{themeFileName}' should contain profile menu selector '{selector}'");
        }
    }

    [Theory]
    [MemberData(nameof(ThemeFilesData))]
    public void ThemeFile_ShouldContainHeaderStyles(string themeFileName)
    {
        var css = ReadThemeCss(themeFileName);
        
        foreach (var selector in RequiredHeaderSelectors)
        {
            css.Should().Contain(selector,
                $"Theme '{themeFileName}' should contain header selector '{selector}'");
        }
    }

    [Theory]
    [InlineData("melodee-dark.css")]
    [InlineData("synthwave.css")]
    [InlineData("midnight-galaxy.css")]
    public void DarkTheme_ShouldContainInputBackgroundStyles(string themeFileName)
    {
        var css = ReadThemeCss(themeFileName);
        
        css.Should().Contain(".rz-textbox",
            $"Dark theme '{themeFileName}' should style .rz-textbox for proper input backgrounds");
        
        css.Should().Contain("input[type=\"text\"]",
            $"Dark theme '{themeFileName}' should style native text inputs");
    }

    [Theory]
    [InlineData("melodee-dark.css")]
    [InlineData("synthwave.css")]
    [InlineData("midnight-galaxy.css")]
    public void DarkTheme_ShouldContainDropdownPanelStyles(string themeFileName)
    {
        var css = ReadThemeCss(themeFileName);
        
        css.Should().Contain(".rz-dropdown-panel",
            $"Dark theme '{themeFileName}' should style dropdown panels for proper dark backgrounds");
        
        css.Should().Contain(".rz-dropdown-item",
            $"Dark theme '{themeFileName}' should style dropdown items");
    }

    [Theory]
    [MemberData(nameof(ThemeFilesData))]
    public void ThemeFile_ShouldHaveColorVariablesOrExplicitColors(string themeFileName)
    {
        var css = ReadThemeCss(themeFileName);
        
        var hasColorDefinitions = css.Contains("--rz-primary") || 
                                   css.Contains("color:") || 
                                   css.Contains("background-color:");
        
        hasColorDefinitions.Should().BeTrue(
            $"Theme '{themeFileName}' should define colors via CSS variables or explicit values");
    }

    [Theory]
    [MemberData(nameof(ThemeFilesData))]
    public void ThemeFile_ShouldNotHaveObviousSyntaxErrors(string themeFileName)
    {
        var css = ReadThemeCss(themeFileName);
        
        var openBraces = css.Count(c => c == '{');
        var closeBraces = css.Count(c => c == '}');
        
        openBraces.Should().Be(closeBraces,
            $"Theme '{themeFileName}' should have balanced braces (found {openBraces} open, {closeBraces} close)");
    }

    [Theory]
    [MemberData(nameof(ThemeFilesData))]
    public void ThemeFile_ShouldDefineHoverStates(string themeFileName)
    {
        var css = ReadThemeCss(themeFileName);
        
        css.Should().Contain(":hover",
            $"Theme '{themeFileName}' should define hover states for interactive elements");
    }

    [Theory]
    [MemberData(nameof(ThemeFilesData))]
    public void ThemeFile_ShouldDefineActiveStates(string themeFileName)
    {
        var css = ReadThemeCss(themeFileName);
        
        var hasActiveState = css.Contains(".rz-state-active") || 
                             css.Contains("-active") ||
                             css.Contains(".active");
        
        hasActiveState.Should().BeTrue(
            $"Theme '{themeFileName}' should define active states for selected items");
    }

    private static string ReadThemeCss(string fileName)
    {
        var filePath = Path.Combine(ThemesDirectory, fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Theme file not found: {filePath}");
        }
        return File.ReadAllText(filePath);
    }
}
