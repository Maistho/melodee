namespace Melodee.Common.Services.Models.ArtistDuplicate;

/// <summary>
/// Criteria for searching for duplicate artists.
/// </summary>
/// <param name="MinScore">Minimum score threshold (0.0 to 1.0). Only pairs with score >= this value are returned.</param>
/// <param name="Limit">Maximum number of duplicate groups to return.</param>
/// <param name="Source">Limit to artists whose external IDs include a given source (e.g., "musicbrainz", "spotify").</param>
/// <param name="ArtistId">Restrict search to duplicates of a single artist (by database ID).</param>
/// <param name="IncludeLowConfidence">Include low-scoring candidates that would normally be filtered out.</param>
public sealed record ArtistDuplicateSearchCriteria(
    double MinScore = 0.7,
    int? Limit = null,
    string? Source = null,
    int? ArtistId = null,
    bool IncludeLowConfidence = false);
