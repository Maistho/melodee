using Melodee.Common.Models;
using Microsoft.JSInterop;

namespace Melodee.Blazor.Services;

/// <summary>
/// Client-side service for managing theme application and switching
/// </summary>
public interface IThemeClientService
{
    /// <summary>
    /// Apply a theme by loading its CSS and optionally hiding NavMenu items
    /// </summary>
    Task ApplyThemeAsync(ThemePack theme, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the currently applied theme ID
    /// </summary>
    Task<string?> GetCurrentThemeIdAsync();

    /// <summary>
    /// Set the current theme ID (stored in local storage)
    /// </summary>
    Task SetCurrentThemeIdAsync(string themeId);
}

public sealed class ThemeClientService(IJSRuntime jsRuntime) : IThemeClientService
{
    private const string CurrentThemeIdKey = "melodee_current_theme_id";
    private string? _currentThemeId;

    public async Task ApplyThemeAsync(ThemePack theme, CancellationToken cancellationToken = default)
    {
        try
        {
            // Load theme CSS
            await jsRuntime.InvokeVoidAsync("melodeeTheme.loadTheme", cancellationToken, theme.ThemeCssPath);

            // Apply typography if metadata available
            if (theme.Metadata?.Fonts != null)
            {
                var fonts = theme.Metadata.Fonts;
                if (!string.IsNullOrWhiteSpace(fonts.Base))
                {
                    await jsRuntime.InvokeVoidAsync("melodeeTheme.setFontFamily", cancellationToken, "base", fonts.Base);
                }
                if (!string.IsNullOrWhiteSpace(fonts.Heading))
                {
                    await jsRuntime.InvokeVoidAsync("melodeeTheme.setFontFamily", cancellationToken, "heading", fonts.Heading);
                }
                if (!string.IsNullOrWhiteSpace(fonts.Mono))
                {
                    await jsRuntime.InvokeVoidAsync("melodeeTheme.setFontFamily", cancellationToken, "mono", fonts.Mono);
                }
            }

            // Apply navigation visibility
            await jsRuntime.InvokeVoidAsync("melodeeTheme.showAllNavMenuItems", cancellationToken);
            if (theme.Metadata?.NavMenu?.Hidden != null && theme.Metadata.NavMenu.Hidden.Count > 0)
            {
                await jsRuntime.InvokeVoidAsync("melodeeTheme.hideNavMenuItems", cancellationToken, theme.Metadata.NavMenu.Hidden);
            }

            // Apply branding
            if (theme.Metadata?.Branding != null)
            {
                await jsRuntime.InvokeVoidAsync("melodeeTheme.applyBranding", cancellationToken, theme.Metadata.Branding);
            }
            else
            {
                await jsRuntime.InvokeVoidAsync("melodeeTheme.resetBranding", cancellationToken);
            }

            _currentThemeId = theme.Id;
            await SetCurrentThemeIdAsync(theme.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying theme: {ex.Message}");
            // For Radzen built-in themes, we don't load custom CSS - just reset to default
            // The RadzenTheme component in App.razor handles built-in themes
        }
    }

    public async Task<string?> GetCurrentThemeIdAsync()
    {
        if (_currentThemeId != null)
        {
            return _currentThemeId;
        }

        try
        {
            _currentThemeId = await jsRuntime.InvokeAsync<string>("localStorage.getItem", CurrentThemeIdKey);
            return _currentThemeId;
        }
        catch
        {
            return null;
        }
    }

    public async Task SetCurrentThemeIdAsync(string themeId)
    {
        try
        {
            _currentThemeId = themeId;
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", CurrentThemeIdKey, themeId);
            await jsRuntime.InvokeVoidAsync("eval", $"document.cookie = 'melodee_ui_theme={themeId}; path=/; max-age=31536000; samesite=lax'");
        }
        catch
        {
            // Ignore localStorage errors
        }
    }
}
