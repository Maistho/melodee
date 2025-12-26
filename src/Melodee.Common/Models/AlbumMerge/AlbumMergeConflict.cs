using Melodee.Common.Enums;

namespace Melodee.Common.Models.AlbumMerge;

/// <summary>
/// Represents a conflict detected during album merge analysis
/// </summary>
public record AlbumMergeConflict
{
    /// <summary>
    /// Unique identifier for this conflict
    /// </summary>
    public required string ConflictId { get; init; }

    /// <summary>
    /// Type of conflict
    /// </summary>
    public required AlbumMergeConflictType ConflictType { get; init; }

    /// <summary>
    /// Human-readable description of the conflict
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Field or property name involved in the conflict
    /// </summary>
    public string? FieldName { get; init; }

    /// <summary>
    /// Value from the target album
    /// </summary>
    public string? TargetValue { get; init; }

    /// <summary>
    /// Values from source albums (keyed by album ID)
    /// </summary>
    public Dictionary<int, string>? SourceValues { get; init; }

    /// <summary>
    /// For track conflicts: the track number involved
    /// </summary>
    public int? TrackNumber { get; init; }

    /// <summary>
    /// For track conflicts: the track IDs involved (keyed by album ID)
    /// </summary>
    public Dictionary<int, int>? TrackIds { get; init; }

    /// <summary>
    /// Additional context data for the conflict
    /// </summary>
    public Dictionary<string, object>? Context { get; init; }

    /// <summary>
    /// Whether this conflict must be resolved before merge can proceed
    /// </summary>
    public bool IsRequired { get; init; } = true;
}
