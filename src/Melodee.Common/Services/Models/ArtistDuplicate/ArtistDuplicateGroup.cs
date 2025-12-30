namespace Melodee.Common.Services.Models.ArtistDuplicate;

/// <summary>
/// A group of artists believed to be potential duplicates of one another.
/// </summary>
/// <param name="GroupId">Unique identifier for this duplicate group.</param>
/// <param name="MaxScore">The highest pairwise score within this group.</param>
/// <param name="Artists">All artists in this duplicate group.</param>
/// <param name="Pairs">All pairwise comparisons within this group.</param>
/// <param name="SuggestedPrimaryArtistId">The artist ID recommended to keep when merging duplicates.</param>
public sealed record ArtistDuplicateGroup(
    string GroupId,
    double MaxScore,
    IReadOnlyList<ArtistDuplicateCandidate> Artists,
    IReadOnlyList<ArtistDuplicatePair> Pairs,
    int SuggestedPrimaryArtistId);
