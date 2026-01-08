namespace Melodee.Blazor.Controllers.Melodee.Models.ArtistLookup;

/// <summary>
/// Response DTO representing a candidate artist from third-party search providers.
/// </summary>
public sealed record ArtistLookupCandidate
{
    /// <summary>
    /// Display name of the provider that returned this result.
    /// </summary>
    public required string ProviderDisplayName { get; init; }

    /// <summary>
    /// Stable identifier of the provider (plugin Id).
    /// </summary>
    public string? ProviderId { get; init; }

    /// <summary>
    /// Name of the artist as returned by the provider.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Sort name for the artist.
    /// </summary>
    public string? SortName { get; init; }

    /// <summary>
    /// URL to the artist's primary image.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// URL to a thumbnail version of the artist's image.
    /// </summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// MusicBrainz ID if available.
    /// </summary>
    public Guid? MusicBrainzId { get; init; }

    /// <summary>
    /// Spotify ID if available.
    /// </summary>
    public string? SpotifyId { get; init; }

    /// <summary>
    /// Discogs ID if available.
    /// </summary>
    public string? DiscogsId { get; init; }

    /// <summary>
    /// AMG ID if available.
    /// </summary>
    public string? AmgId { get; init; }

    /// <summary>
    /// WikiData ID if available.
    /// </summary>
    public string? WikiDataId { get; init; }

    /// <summary>
    /// iTunes ID if available.
    /// </summary>
    public string? ItunesId { get; init; }

    /// <summary>
    /// Last.fm ID if available.
    /// </summary>
    public string? LastFmId { get; init; }
}
