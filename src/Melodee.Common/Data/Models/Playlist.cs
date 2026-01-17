using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId), nameof(Name), IsUnique = true)]
public class Playlist : DataModelBase
{
    public const string DynamicPlaylistDirectoryName = "dynamic";

    public const string ImagesDirectoryName = "images";

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    /// <summary>
    ///     This is plain text and served to OpenSubsonic clients.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? Comment { get; set; }

    [RequiredGreaterThanZero] public int UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>
    /// Source type of the playlist (Manual, M3UImport, Dynamic)
    /// </summary>
    public PlaylistSourceType SourceType { get; set; } = PlaylistSourceType.Manual;

    /// <summary>
    /// Reference to the uploaded file if this playlist was created from an import
    /// </summary>
    public int? PlaylistUploadedFileId { get; set; }

    public PlaylistUploadedFile? PlaylistUploadedFile { get; set; }

    public bool IsPublic { get; set; }

    public short? SongCount { get; set; }

    public double Duration { get; set; }

    /// <summary>
    ///     Pipe seperated list.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? AllowedUserIds { get; set; }

    public ICollection<PlaylistSong> Songs { get; set; } = new List<PlaylistSong>();

    [NotMapped] public bool IsDynamic { get; set; }
}
