namespace Melodee.Common.Services.Models.ArtistDuplicate;

/// <summary>
/// A pairwise comparison between two artists within a duplicate group.
/// </summary>
/// <param name="LeftArtistId">The database ID of the first artist.</param>
/// <param name="RightArtistId">The database ID of the second artist.</param>
/// <param name="Score">The computed similarity score between 0.0 and 1.0.</param>
/// <param name="Reasons">Human-readable reasons explaining why these artists are considered duplicates.</param>
public sealed record ArtistDuplicatePair(
    int LeftArtistId,
    int RightArtistId,
    double Score,
    IReadOnlyCollection<string> Reasons);
