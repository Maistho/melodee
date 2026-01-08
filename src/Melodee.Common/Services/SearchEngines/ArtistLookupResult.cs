using Melodee.Common.Models.SearchEngines;

namespace Melodee.Common.Services.SearchEngines;

/// <summary>
/// Result of an artist lookup operation containing candidates and status information.
/// </summary>
public sealed record ArtistLookupResult
{
    /// <summary>
    /// Artist candidates from search providers.
    /// </summary>
    public required ArtistSearchResult[] Candidates { get; init; }

    /// <summary>
    /// Indicates whether some providers failed during the search.
    /// </summary>
    public bool HasPartialFailures { get; init; }

    /// <summary>
    /// IDs of providers that failed during the search.
    /// </summary>
    public string[] FailedProviderIds { get; init; } = [];

    /// <summary>
    /// Total operation time in milliseconds.
    /// </summary>
    public long OperationTime { get; init; }
}
