using Melodee.Common.Extensions;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;

namespace Melodee.Common.Plugins.SearchEngine.MetalApi;

/// <summary>
///     Maps Metal API responses to ImageSearchResult
/// </summary>
internal static class MetalApiImageMapper
{
    /// <summary>
    ///     Map a Metal album to an ImageSearchResult
    /// </summary>
    /// <param name="album">The Metal album with cover URL</param>
    /// <param name="bandName">The band/artist name for matching</param>
    /// <param name="isExactMatch">Whether this is an exact album + artist match</param>
    /// <param name="isArtistFallback">Whether this is being used as artist artwork fallback</param>
    /// <returns>ImageSearchResult or null if coverUrl is missing</returns>
    public static ImageSearchResult? FromAlbum(
        MetalAlbum album,
        string? bandName,
        bool isExactMatch,
        bool isArtistFallback)
    {
        if (album.CoverUrl.Nullify() == null)
        {
            return null;
        }

        var rank = CalculateRank(isExactMatch, isArtistFallback);
        var title = isArtistFallback 
            ? $"{bandName ?? album.Band?.Name ?? "Unknown"} album art"
            : album.Name ?? "Unknown Album";

        // Parse release date if available
        DateTime? releaseDate = null;
        if (album.ReleaseDate.Nullify() != null)
        {
            if (DateTime.TryParse(album.ReleaseDate, out var parsedDate))
            {
                releaseDate = parsedDate;
            }
        }

        return new ImageSearchResult
        {
            FromPlugin = "Metal API",
            MediaUrl = album.CoverUrl!,
            ThumbnailUrl = album.CoverUrl!,
            Title = title,
            ReleaseDate = releaseDate,
            Rank = rank,
            UniqueId = SafeParser.Hash(album.CoverUrl!),
            Width = 0,  // Unknown from API
            Height = 0  // Unknown from API
        };
    }

    /// <summary>
    ///     Calculate rank based on match quality
    /// </summary>
    private static short CalculateRank(bool isExactMatch, bool isArtistFallback)
    {
        if (isArtistFallback)
        {
            return 5; // Lower rank for fallback artist images from album art
        }

        if (isExactMatch)
        {
            return 15; // Higher rank for exact album + artist matches
        }

        return 8; // Medium rank for partial matches
    }

    /// <summary>
    ///     Deduplicate and sort image search results
    /// </summary>
    /// <param name="results">Image search results to process</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <returns>Deduplicated and sorted results limited to maxResults</returns>
    public static ImageSearchResult[] DeduplicateAndSort(
        IEnumerable<ImageSearchResult> results,
        int maxResults)
    {
        if (results == null)
        {
            return [];
        }

        // Deduplicate by MediaUrl (case-insensitive), preferring higher rank
        var deduplicated = results
            .GroupBy(r => r.MediaUrl.ToLowerInvariant())
            .Select(g => g.OrderByDescending(r => r.Rank).First())
            .OrderByDescending(r => r.Rank)
            .ThenByDescending(r => r.ReleaseDate ?? DateTime.MinValue)
            .Take(maxResults)
            .ToArray();

        return deduplicated;
    }
}
