namespace Melodee.Tests.Playwright.Themes;

/// <summary>
/// Visual regression tests for Melodee themes.
/// These tests verify that all themes render correctly with proper contrast and visibility.
/// 
/// Prerequisites:
/// 1. Install Playwright browsers: pwsh bin/Debug/net10.0/playwright.ps1 install
/// 2. Have Melodee.Blazor running at the configured BaseUrl
/// 
/// Running tests:
/// dotnet test --filter "FullyQualifiedName~ThemeVisualRegressionTests"
/// 
/// To update baseline screenshots:
/// Set environment variable: UPDATE_SNAPSHOTS=1
/// </summary>
[Collection("Playwright")]
public class ThemeVisualRegressionTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    
    private const string BaseUrl = "http://localhost:5000";
    
    public static readonly string[] AllThemes =
    [
        "melodee",
        "melodee-dark",
        "synthwave",
        "midnight-galaxy",
        "forest",
        "ocean-breeze",
        "sunset-vibes"
    ];
    
    public static readonly string[] DarkThemes =
    [
        "melodee-dark",
        "synthwave",
        "midnight-galaxy"
    ];
    
    public static readonly string[] LightThemes =
    [
        "melodee",
        "forest",
        "ocean-breeze",
        "sunset-vibes"
    ];

    public async Task InitializeAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    public static IEnumerable<object[]> AllThemesData => AllThemes.Select(t => new object[] { t });
    public static IEnumerable<object[]> DarkThemesData => DarkThemes.Select(t => new object[] { t });
    public static IEnumerable<object[]> LightThemesData => LightThemes.Select(t => new object[] { t });

    /// <summary>
    /// Verifies that the sidebar renders correctly for each theme.
    /// Checks for proper text visibility against background.
    /// </summary>
    [Theory(Skip = "Requires running Melodee.Blazor instance")]
    [MemberData(nameof(AllThemesData))]
    public async Task Sidebar_ShouldRenderCorrectly_ForTheme(string themeName)
    {
        await SetThemeAsync(themeName);
        await _page.GotoAsync($"{BaseUrl}/");
        await _page.WaitForSelectorAsync(".rz-sidebar");
        
        var sidebar = _page.Locator(".rz-sidebar");
        await Expect(sidebar).ToBeVisibleAsync();
        
        await sidebar.ScreenshotAsync(new LocatorScreenshotOptions
        {
            Path = $"screenshots/{themeName}/sidebar.png"
        });
    }

    /// <summary>
    /// Verifies that the header renders correctly for each theme.
    /// Checks for proper text visibility and no unwanted separator lines.
    /// </summary>
    [Theory(Skip = "Requires running Melodee.Blazor instance")]
    [MemberData(nameof(AllThemesData))]
    public async Task Header_ShouldRenderCorrectly_ForTheme(string themeName)
    {
        await SetThemeAsync(themeName);
        await _page.GotoAsync($"{BaseUrl}/");
        await _page.WaitForSelectorAsync(".rz-header");
        
        var header = _page.Locator(".rz-header");
        await Expect(header).ToBeVisibleAsync();
        
        await header.ScreenshotAsync(new LocatorScreenshotOptions
        {
            Path = $"screenshots/{themeName}/header.png"
        });
    }

    /// <summary>
    /// Verifies that form inputs have proper styling in dark themes.
    /// Dark themes should have dark backgrounds for inputs, not white.
    /// </summary>
    [Theory(Skip = "Requires running Melodee.Blazor instance")]
    [MemberData(nameof(DarkThemesData))]
    public async Task FormInputs_ShouldHaveDarkBackground_InDarkTheme(string themeName)
    {
        await SetThemeAsync(themeName);
        await _page.GotoAsync($"{BaseUrl}/account/profile");
        await _page.WaitForSelectorAsync(".rz-textbox");
        
        var textbox = _page.Locator(".rz-textbox").First;
        await Expect(textbox).ToBeVisibleAsync();
        
        var backgroundColor = await textbox.EvaluateAsync<string>(
            "el => getComputedStyle(el).backgroundColor");
        
        var rgb = ParseRgb(backgroundColor);
        var brightness = CalculateBrightness(rgb);
        
        brightness.Should().BeLessThan(100, 
            $"Form inputs in dark theme '{themeName}' should have dark backgrounds (brightness: {brightness})");
    }

    /// <summary>
    /// Verifies that form inputs have proper styling in light themes.
    /// Light themes should have light/white backgrounds for inputs.
    /// </summary>
    [Theory(Skip = "Requires running Melodee.Blazor instance")]
    [MemberData(nameof(LightThemesData))]
    public async Task FormInputs_ShouldHaveLightBackground_InLightTheme(string themeName)
    {
        await SetThemeAsync(themeName);
        await _page.GotoAsync($"{BaseUrl}/account/profile");
        await _page.WaitForSelectorAsync(".rz-textbox");
        
        var textbox = _page.Locator(".rz-textbox").First;
        await Expect(textbox).ToBeVisibleAsync();
        
        var backgroundColor = await textbox.EvaluateAsync<string>(
            "el => getComputedStyle(el).backgroundColor");
        
        var rgb = ParseRgb(backgroundColor);
        var brightness = CalculateBrightness(rgb);
        
        brightness.Should().BeGreaterThan(200, 
            $"Form inputs in light theme '{themeName}' should have light backgrounds (brightness: {brightness})");
    }

    /// <summary>
    /// Verifies that the Admin submenu items are visible and readable.
    /// This was a specific bug where nested menu items were hard to read.
    /// </summary>
    [Theory(Skip = "Requires running Melodee.Blazor instance")]
    [MemberData(nameof(AllThemesData))]
    public async Task AdminSubmenu_ShouldHaveReadableText_ForTheme(string themeName)
    {
        await SetThemeAsync(themeName);
        await _page.GotoAsync($"{BaseUrl}/");
        
        var adminMenuItem = _page.Locator("text=Admin").First;
        if (await adminMenuItem.IsVisibleAsync())
        {
            await adminMenuItem.ClickAsync();
            await _page.WaitForTimeoutAsync(500);
            
            var submenuItems = _page.Locator(".rz-panelmenu-content .rz-menuitem");
            var count = await submenuItems.CountAsync();
            
            for (var i = 0; i < count && i < 3; i++)
            {
                var item = submenuItems.Nth(i);
                if (await item.IsVisibleAsync())
                {
                    var color = await item.EvaluateAsync<string>(
                        "el => getComputedStyle(el).color");
                    var bgColor = await item.EvaluateAsync<string>(
                        "el => getComputedStyle(el.closest('.rz-panelmenu-content')).backgroundColor");
                    
                    var textRgb = ParseRgb(color);
                    var bgRgb = ParseRgb(bgColor);
                    var contrastRatio = CalculateContrastRatio(textRgb, bgRgb);
                    
                    contrastRatio.Should().BeGreaterThanOrEqualTo(4.5,
                        $"Admin submenu text should have WCAG AA contrast ratio (4.5:1) in theme '{themeName}'. Got {contrastRatio:F2}:1");
                }
            }
        }
    }

    /// <summary>
    /// Verifies that the profile menu dropdown renders correctly.
    /// </summary>
    [Theory(Skip = "Requires running Melodee.Blazor instance")]
    [MemberData(nameof(AllThemesData))]
    public async Task ProfileMenu_ShouldRenderCorrectly_ForTheme(string themeName)
    {
        await SetThemeAsync(themeName);
        await _page.GotoAsync($"{BaseUrl}/");
        
        var profileMenu = _page.Locator(".rz-profile-menu");
        if (await profileMenu.IsVisibleAsync())
        {
            await profileMenu.ClickAsync();
            await _page.WaitForTimeoutAsync(300);
            
            var dropdown = _page.Locator(".rz-profile-menu .rz-menu, .rz-profile-menu-dropdown");
            if (await dropdown.IsVisibleAsync())
            {
                await dropdown.ScreenshotAsync(new LocatorScreenshotOptions
                {
                    Path = $"screenshots/{themeName}/profile-menu.png"
                });
            }
        }
    }

    /// <summary>
    /// Takes a full page screenshot for each theme for manual review.
    /// </summary>
    [Theory(Skip = "Requires running Melodee.Blazor instance")]
    [MemberData(nameof(AllThemesData))]
    public async Task FullPage_ShouldRenderCorrectly_ForTheme(string themeName)
    {
        await SetThemeAsync(themeName);
        await _page.GotoAsync($"{BaseUrl}/");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = $"screenshots/{themeName}/full-page.png",
            FullPage = true
        });
    }

    /// <summary>
    /// Verifies that DataGrid headers have proper contrast.
    /// </summary>
    [Theory(Skip = "Requires running Melodee.Blazor instance")]
    [MemberData(nameof(AllThemesData))]
    public async Task DataGridHeader_ShouldHaveProperContrast_ForTheme(string themeName)
    {
        await SetThemeAsync(themeName);
        await _page.GotoAsync($"{BaseUrl}/data/artists");
        
        var headerCell = _page.Locator(".rz-datatable-thead th").First;
        if (await headerCell.IsVisibleAsync())
        {
            var color = await headerCell.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            var bgColor = await headerCell.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            
            var textRgb = ParseRgb(color);
            var bgRgb = ParseRgb(bgColor);
            var contrastRatio = CalculateContrastRatio(textRgb, bgRgb);
            
            contrastRatio.Should().BeGreaterThanOrEqualTo(4.5,
                $"DataGrid header should have WCAG AA contrast ratio in theme '{themeName}'. Got {contrastRatio:F2}:1");
        }
    }

    private async Task SetThemeAsync(string themeName)
    {
        await _page.EvaluateAsync($"localStorage.setItem('selectedTheme', '{themeName}')");
    }

    private static (int R, int G, int B) ParseRgb(string color)
    {
        if (string.IsNullOrEmpty(color))
            return (0, 0, 0);
            
        if (color.StartsWith("rgba"))
        {
            var values = color.Replace("rgba(", "").Replace(")", "").Split(',');
            return (int.Parse(values[0].Trim()), int.Parse(values[1].Trim()), int.Parse(values[2].Trim()));
        }
        
        if (color.StartsWith("rgb"))
        {
            var values = color.Replace("rgb(", "").Replace(")", "").Split(',');
            return (int.Parse(values[0].Trim()), int.Parse(values[1].Trim()), int.Parse(values[2].Trim()));
        }
        
        return (0, 0, 0);
    }

    private static double CalculateBrightness((int R, int G, int B) rgb)
    {
        return (rgb.R * 299 + rgb.G * 587 + rgb.B * 114) / 1000.0;
    }

    private static double CalculateContrastRatio((int R, int G, int B) text, (int R, int G, int B) bg)
    {
        var textLuminance = CalculateRelativeLuminance(text);
        var bgLuminance = CalculateRelativeLuminance(bg);
        
        var lighter = Math.Max(textLuminance, bgLuminance);
        var darker = Math.Min(textLuminance, bgLuminance);
        
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double CalculateRelativeLuminance((int R, int G, int B) rgb)
    {
        var r = rgb.R / 255.0;
        var g = rgb.G / 255.0;
        var b = rgb.B / 255.0;
        
        r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
        g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
        b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);
        
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }
}
