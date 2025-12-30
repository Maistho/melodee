namespace Melodee.Common.Services.Models.ArtistDuplicate;

/// <summary>
/// An artist candidate within a duplicate group.
/// </summary>
/// <param name="ArtistId">The internal database ID of the artist.</param>
/// <param name="ApiKey">The API key (GUID) of the artist.</param>
/// <param name="Name">The display name of the artist.</param>
/// <param name="SortName">The sort name of the artist (e.g., "John, Elton").</param>
/// <param name="ExternalIds">Map of external ID provider names to their values.</param>
/// <param name="AlbumCount">Number of albums for this artist.</param>
/// <param name="SongCount">Number of songs for this artist.</param>
public sealed record ArtistDuplicateCandidate(
    int ArtistId,
    Guid ApiKey,
    string Name,
    string? SortName,
    IReadOnlyDictionary<string, string> ExternalIds,
    int AlbumCount,
    int SongCount);
