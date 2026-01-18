using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId))]
public class PlaylistUploadedFile : DataModelBase
{
    [RequiredGreaterThanZero]
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string OriginalFileName { get; set; }

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string ContentType { get; set; }

    [RequiredGreaterThanZero]
    public required long Length { get; set; }

    /// <summary>
    /// Original uploaded file data for traceability and re-processing
    /// </summary>
    public byte[]? FileData { get; set; }

    public ICollection<PlaylistUploadedFileItem> Items { get; set; } = new List<PlaylistUploadedFileItem>();
}
