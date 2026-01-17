using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(PlaylistUploadedFileId), nameof(SortOrder))]
public class PlaylistUploadedFileItem
{
    public int Id { get; set; }

    [RequiredGreaterThanZero]
    public int PlaylistUploadedFileId { get; set; }

    public PlaylistUploadedFile PlaylistUploadedFile { get; set; } = null!;

    /// <summary>
    /// The resolved song ID, null if not yet resolved
    /// </summary>
    public int? SongId { get; set; }

    public Song? Song { get; set; }

    [Required]
    public int SortOrder { get; set; }

    [Required]
    public PlaylistItemStatus Status { get; set; } = PlaylistItemStatus.Missing;

    /// <summary>
    /// Original raw line from the playlist file
    /// </summary>
    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public required string RawReference { get; set; }

    /// <summary>
    /// Normalized reference (decoded, path separators normalized)
    /// </summary>
    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public required string NormalizedReference { get; set; }

    /// <summary>
    /// JSON with hints for matching: filename, artistFolder, albumFolder, etc.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? HintsJson { get; set; }

    /// <summary>
    /// Last time we attempted to resolve this item
    /// </summary>
    public Instant? LastAttemptUtc { get; set; }
}
