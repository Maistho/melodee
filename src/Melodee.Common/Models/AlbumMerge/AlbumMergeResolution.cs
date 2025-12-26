namespace Melodee.Common.Models.AlbumMerge;

/// <summary>
/// Represents a user's resolution decision for a specific conflict
/// </summary>
public record AlbumMergeResolution
{
    /// <summary>
    /// ID of the conflict being resolved
    /// </summary>
    public required string ConflictId { get; init; }

    /// <summary>
    /// Resolution action chosen by user
    /// </summary>
    public required AlbumMergeResolutionAction Action { get; init; }

    /// <summary>
    /// For field conflicts: the selected value
    /// </summary>
    public string? SelectedValue { get; init; }

    /// <summary>
    /// For field conflicts: the album ID the value came from (0 for target)
    /// </summary>
    public int? SelectedFromAlbumId { get; init; }

    /// <summary>
    /// For track conflicts: the selected track ID to keep
    /// </summary>
    public int? SelectedTrackId { get; init; }

    /// <summary>
    /// For renumbering: the new track number
    /// </summary>
    public int? NewTrackNumber { get; init; }

    /// <summary>
    /// Additional resolution context
    /// </summary>
    public Dictionary<string, object>? Context { get; init; }
}

/// <summary>
/// Actions that can be taken to resolve a conflict
/// </summary>
public enum AlbumMergeResolutionAction
{
    /// <summary>
    /// Keep the target album's value
    /// </summary>
    KeepTarget = 1,

    /// <summary>
    /// Replace with source value
    /// </summary>
    ReplaceWithSource = 2,

    /// <summary>
    /// Skip the source item
    /// </summary>
    SkipSource = 3,

    /// <summary>
    /// Keep both items (for metadata union)
    /// </summary>
    KeepBoth = 4,

    /// <summary>
    /// Renumber the track
    /// </summary>
    Renumber = 5
}
