using System.Globalization;

namespace Melodee.Blazor.Services;

/// <summary>
/// Service for handling application localization and culture management.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the currently active culture.
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// Gets all supported cultures.
    /// </summary>
    IReadOnlyList<CultureInfo> SupportedCultures { get; }

    /// <summary>
    /// Event raised when the current culture changes.
    /// </summary>
    event Action<CultureInfo>? CultureChanged;

    /// <summary>
    /// Localizes a string by resource key.
    /// </summary>
    /// <param name="key">The resource key (e.g., "Navigation.Dashboard")</param>
    /// <returns>Localized string or the key if not found</returns>
    string Localize(string key);

    /// <summary>
    /// Localizes a string by resource key with fallback.
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="fallback">Fallback value if key not found</param>
    /// <returns>Localized string or fallback</returns>
    string Localize(string key, string fallback);

    /// <summary>
    /// Localizes a string with format arguments.
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    string Localize(string key, params object[] args);

    /// <summary>
    /// Sets the current culture.
    /// </summary>
    /// <param name="culture">Culture to set as current</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task SetCultureAsync(CultureInfo culture);

    /// <summary>
    /// Sets the current culture by culture code.
    /// </summary>
    /// <param name="cultureCode">Culture code (e.g., "en-US", "es-ES")</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task SetCultureAsync(string cultureCode);

    /// <summary>
    /// Gets the user's preferred culture from storage.
    /// </summary>
    /// <returns>User's preferred culture or default culture</returns>
    Task<CultureInfo> GetUserCultureAsync();

    /// <summary>
    /// Formats a date according to current culture.
    /// </summary>
    /// <param name="date">Date to format</param>
    /// <param name="format">Optional format string</param>
    /// <returns>Formatted date string</returns>
    string FormatDate(DateTime date, string? format = null);

    /// <summary>
    /// Formats a number according to current culture.
    /// </summary>
    /// <param name="number">Number to format</param>
    /// <param name="format">Optional format string</param>
    /// <returns>Formatted number string</returns>
    string FormatNumber(decimal number, string? format = null);

    /// <summary>
    /// Determines if the current culture is a right-to-left (RTL) language.
    /// </summary>
    /// <returns>True if current culture is RTL, false otherwise</returns>
    bool IsRightToLeft();

    /// <summary>
    /// Gets the text direction for the current culture.
    /// </summary>
    /// <returns>"rtl" for right-to-left languages, "ltr" for left-to-right languages</returns>
    string GetTextDirection();
}
