namespace Melodee.Blazor.Controllers.Melodee.Models.ArtistLookup;

/// <summary>
/// Response DTO for artist lookup containing candidates and provider status.
/// </summary>
public sealed record ArtistLookupResponse
{
    /// <summary>
    /// List of artist candidates from search providers.
    /// </summary>
    public required ArtistLookupCandidate[] Candidates { get; init; }

    /// <summary>
    /// Indicates whether some providers failed during the search.
    /// </summary>
    public bool HasPartialFailures { get; init; }

    /// <summary>
    /// Information about available and enabled search providers.
    /// </summary>
    public required ProviderInfo[] Providers { get; init; }
}

/// <summary>
/// Information about a search provider.
/// </summary>
public sealed record ProviderInfo
{
    /// <summary>
    /// Stable identifier for the provider (plugin Id).
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Human-readable display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Whether the provider is currently enabled.
    /// </summary>
    public bool IsEnabled { get; init; }
}
