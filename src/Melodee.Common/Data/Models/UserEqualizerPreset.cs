using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

/// <summary>
/// User-scoped equalizer preset.
/// </summary>
[Serializable]
[Index(nameof(UserId), nameof(Name), IsUnique = true)]
public class UserEqualizerPreset : DataModelBase
{
    public User User { get; set; } = null!;

    public int UserId { get; set; }

    /// <summary>
    /// Name of the preset. Unique per user.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// Normalized name for uniqueness checks.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string NameNormalized { get; set; }

    /// <summary>
    /// JSON-serialized array of band data: [{ "frequency": double, "gain": double }, ...]
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    [Required]
    public required string BandsJson { get; set; }

    /// <summary>
    /// Whether this is the user's default preset.
    /// </summary>
    public bool IsDefault { get; set; }
}
