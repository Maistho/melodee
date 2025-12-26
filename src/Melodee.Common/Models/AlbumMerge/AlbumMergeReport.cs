namespace Melodee.Common.Models.AlbumMerge;

/// <summary>
/// Report of a completed album merge operation
/// </summary>
public record AlbumMergeReport
{
    /// <summary>
    /// ID of the target album
    /// </summary>
    public required int TargetAlbumId { get; init; }

    /// <summary>
    /// Name of the target album
    /// </summary>
    public required string TargetAlbumName { get; init; }

    /// <summary>
    /// IDs of source albums that were merged and deleted
    /// </summary>
    public required int[] SourceAlbumIds { get; init; }

    /// <summary>
    /// Names of source albums
    /// </summary>
    public required string[] SourceAlbumNames { get; init; }

    /// <summary>
    /// Number of songs moved to target
    /// </summary>
    public int SongsMoved { get; init; }

    /// <summary>
    /// Number of songs skipped due to deduplication
    /// </summary>
    public int SongsSkipped { get; init; }

    /// <summary>
    /// Number of images moved to target
    /// </summary>
    public int ImagesMoved { get; init; }

    /// <summary>
    /// Number of images skipped due to deduplication
    /// </summary>
    public int ImagesSkipped { get; init; }

    /// <summary>
    /// Number of metadata items merged
    /// </summary>
    public int MetadataMerged { get; init; }

    /// <summary>
    /// Number of metadata items skipped
    /// </summary>
    public int MetadataSkipped { get; init; }

    /// <summary>
    /// Conflicts that were resolved
    /// </summary>
    public AlbumMergeConflict[]? ResolvedConflicts { get; init; }

    /// <summary>
    /// Resolutions applied
    /// </summary>
    public AlbumMergeResolution[]? AppliedResolutions { get; init; }

    /// <summary>
    /// Detailed action log
    /// </summary>
    public string[]? ActionLog { get; init; }
}
