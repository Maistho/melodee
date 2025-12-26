namespace Melodee.Common.Models.AlbumMerge;

/// <summary>
/// Result of conflict detection analysis
/// </summary>
public record AlbumMergeConflictDetectionResult
{
    /// <summary>
    /// Whether any conflicts were detected
    /// </summary>
    public bool HasConflicts => Conflicts?.Any() == true;

    /// <summary>
    /// List of detected conflicts
    /// </summary>
    public AlbumMergeConflict[]? Conflicts { get; init; }

    /// <summary>
    /// Number of conflicts that require user resolution
    /// </summary>
    public int RequiredConflictCount => Conflicts?.Count(c => c.IsRequired) ?? 0;

    /// <summary>
    /// Target album ID
    /// </summary>
    public required int TargetAlbumId { get; init; }

    /// <summary>
    /// Source album IDs
    /// </summary>
    public required int[] SourceAlbumIds { get; init; }

    /// <summary>
    /// Summary of what would be merged (if no conflicts)
    /// </summary>
    public string? MergeSummary { get; init; }
}
