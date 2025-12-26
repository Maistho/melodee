namespace Melodee.Common.Models.AlbumMerge;

/// <summary>
/// Request to merge multiple albums into a single target album
/// </summary>
public record AlbumMergeRequest
{
    /// <summary>
    /// ID of the artist owning the albums
    /// </summary>
    public required int ArtistId { get; init; }

    /// <summary>
    /// ID of the target album (merge into)
    /// </summary>
    public required int TargetAlbumId { get; init; }

    /// <summary>
    /// IDs of source albums (merge from)
    /// </summary>
    public required int[] SourceAlbumIds { get; init; }

    /// <summary>
    /// Resolutions for detected conflicts
    /// </summary>
    public AlbumMergeResolution[]? Resolutions { get; init; }
}
