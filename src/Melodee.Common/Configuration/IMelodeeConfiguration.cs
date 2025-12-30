using Melodee.Common.Enums;

namespace Melodee.Common.Configuration;

public interface IMelodeeConfiguration
{
    Dictionary<string, object?> Configuration { get; }

    string GenerateImageUrl(string apiKey, ImageSize imageSize);

    string GenerateWebSearchUrl(object[] searchTerms);

    T? GetValue<T>(string key, Func<T?, T>? returnValue = null);

    string? RemoveUnwantedArticles(string? input);

    /// <summary>
    ///     Normalizes a name for comparison by stripping leading articles and applying standard normalization.
    ///     This is useful for detecting duplicates like "The Beatles" vs "Beatles".
    /// </summary>
    /// <param name="input">The name to normalize.</param>
    /// <returns>The fully normalized name suitable for comparison.</returns>
    string? NormalizeNameForComparison(string? input);

    /// <summary>
    ///     Gets the configured ignored articles as a pipe-delimited string.
    /// </summary>
    string? GetIgnoredArticles();

    int BatchProcessingSize();

    void SetSetting<T>(string key, T? value);

    TimeSpan? CacheDuration();
}
