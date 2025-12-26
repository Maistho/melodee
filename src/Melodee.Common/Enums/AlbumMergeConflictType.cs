namespace Melodee.Common.Enums;

/// <summary>
/// Types of conflicts that can occur during album merging
/// </summary>
public enum AlbumMergeConflictType
{
    /// <summary>
    /// Album-level field differences (title, year, etc.)
    /// </summary>
    AlbumFieldConflict = 1,

    /// <summary>
    /// Two tracks with the same track number but different content
    /// </summary>
    TrackNumberCollision = 2,

    /// <summary>
    /// Same track title exists at different track numbers (compilation case)
    /// </summary>
    DuplicateTitleDifferentNumber = 3,

    /// <summary>
    /// Metadata collision (tags, genres, external IDs, etc.)
    /// </summary>
    MetadataCollision = 4,

    /// <summary>
    /// Image collision (same or different checksums)
    /// </summary>
    ImageCollision = 5
}
