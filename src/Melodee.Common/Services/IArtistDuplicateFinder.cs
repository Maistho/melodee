using Melodee.Common.Services.Models.ArtistDuplicate;

namespace Melodee.Common.Services;

/// <summary>
/// Service interface for detecting duplicate artists based on external IDs, name similarity, and album overlap.
/// </summary>
public interface IArtistDuplicateFinder
{
    /// <summary>
    /// Find potential duplicate artist groups based on the specified criteria.
    /// </summary>
    /// <param name="criteria">Search criteria including minimum score, limits, and filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of duplicate groups ordered by max score descending.</returns>
    Task<IReadOnlyList<ArtistDuplicateGroup>> FindDuplicatesAsync(
        ArtistDuplicateSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
