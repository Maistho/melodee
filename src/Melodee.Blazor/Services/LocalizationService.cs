using System.Globalization;
using Melodee.Blazor.Resources;
using Microsoft.Extensions.Localization;

namespace Melodee.Blazor.Services;

/// <summary>
/// Implementation of localization service for managing application cultures and translations.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private const string CultureStorageKey = "user_preferred_culture";

    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<LocalizationService> _logger;
    private CultureInfo _currentCulture;

    private static readonly CultureInfo[] _supportedCultures =
    [
        new("en-US"), // English (United States)
        new("de-DE"), // German (Germany)
        new("es-ES"), // Spanish (Spain)
        new("fr-FR"), // French (France)
        new("it-IT"), // Italian (Italy)
        new("ja-JP"), // Japanese (Japan)
        new("pt-BR"), // Portuguese (Brazil)
        new("ru-RU"), // Russian (Russia)
        new("zh-CN"), // Chinese (Simplified, China)
        new("ar-SA")  // Arabic (Saudi Arabia)
    ];

    public LocalizationService(
        IStringLocalizer<SharedResources> localizer,
        ILocalStorageService localStorage,
        ILogger<LocalizationService> logger)
    {
        _localizer = localizer;
        _localStorage = localStorage;
        _logger = logger;
        _currentCulture = CultureInfo.CurrentCulture;
    }

    public CultureInfo CurrentCulture => _currentCulture;

    public IReadOnlyList<CultureInfo> SupportedCultures => _supportedCultures;

    public event Action<CultureInfo>? CultureChanged;

    public string Localize(string key)
    {
        try
        {
            var localizedString = _localizer[key];
            if (localizedString.ResourceNotFound)
            {
                _logger.LogWarning("Resource key not found: {Key}", key);
                return key;
            }
            return localizedString.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error localizing key: {Key}", key);
            return key;
        }
    }

    public string Localize(string key, string fallback)
    {
        try
        {
            var localizedString = _localizer[key];
            return localizedString.ResourceNotFound ? fallback : localizedString.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error localizing key: {Key}", key);
            return fallback;
        }
    }

    public string Localize(string key, params object[] args)
    {
        try
        {
            // Get the template string without formatting
            var template = _localizer[key];
            if (template.ResourceNotFound)
            {
                _logger.LogWarning("Resource key not found: {Key}", key);
                return string.Format(key, args);
            }

            // If we have arguments, explicitly format the template
            if (args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(template.Value, args);
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(ex, "Failed to format localized string for key: {Key}, template: {Template}", key, template.Value);
                    return template.Value;
                }
            }

            return template.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error localizing key: {Key} with args", key);
            try
            {
                return string.Format(key, args);
            }
            catch
            {
                return key;
            }
        }
    }

    public async Task SetCultureAsync(CultureInfo culture)
    {
        if (!_supportedCultures.Any(c => c.Name == culture.Name))
        {
            _logger.LogWarning("Unsupported culture: {Culture}. Falling back to en-US", culture.Name);
            culture = new CultureInfo("en-US");
        }

        _currentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        try
        {
            await _localStorage.SetItemAsStringAsync(CultureStorageKey, culture.Name);
        }
        catch (InvalidOperationException)
        {
            // Expected during prerendering - localStorage not available yet
        }
        catch (Microsoft.JSInterop.JSDisconnectedException)
        {
            // Circuit disconnected - culture will be re-read on reconnect
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving culture to local storage");
        }

        CultureChanged?.Invoke(culture);
        _logger.LogInformation("Culture changed to: {Culture}", culture.Name);
    }

    public async Task SetCultureAsync(string cultureCode)
    {
        try
        {
            var culture = new CultureInfo(cultureCode);
            await SetCultureAsync(culture);
        }
        catch (CultureNotFoundException ex)
        {
            _logger.LogError(ex, "Invalid culture code: {CultureCode}", cultureCode);
            await SetCultureAsync(new CultureInfo("en-US"));
        }
    }

    public async Task<CultureInfo> GetUserCultureAsync()
    {
        try
        {
            var cultureName = await _localStorage.GetItemAsStringAsync(CultureStorageKey);
            if (!string.IsNullOrEmpty(cultureName))
            {
                var culture = new CultureInfo(cultureName);
                if (_supportedCultures.Any(c => c.Name == culture.Name))
                {
                    return culture;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user culture from storage");
        }

        // Fallback to browser culture if supported, otherwise default to en-US
        var browserCulture = CultureInfo.CurrentUICulture;
        if (_supportedCultures.Any(c => c.Name == browserCulture.Name))
        {
            return browserCulture;
        }

        return new CultureInfo("en-US");
    }

    public string FormatDate(DateTime date, string? format = null)
    {
        try
        {
            return string.IsNullOrEmpty(format)
                ? date.ToString("d", _currentCulture)
                : date.ToString(format, _currentCulture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting date");
            return date.ToString("d");
        }
    }

    public string FormatNumber(decimal number, string? format = null)
    {
        try
        {
            return string.IsNullOrEmpty(format)
                ? number.ToString("N2", _currentCulture)
                : number.ToString(format, _currentCulture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting number");
            return number.ToString("N2");
        }
    }

    public bool IsRightToLeft()
    {
        return _currentCulture.TextInfo.IsRightToLeft;
    }

    public string GetTextDirection()
    {
        return IsRightToLeft() ? "rtl" : "ltr";
    }
}
