using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId))]
[Index(nameof(IsPublic))]
public class SmartPlaylist : DataModelBase
{
    [RequiredGreaterThanZero]
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    [Required]
    public required string MqlQuery { get; set; }

    /// <summary>
    ///     The entity type this smart playlist targets: "songs", "albums", or "artists".
    /// </summary>
    [MaxLength(20)]
    [Required]
    public required string EntityType { get; set; }

    /// <summary>
    ///     The number of results from the last evaluation.
    /// </summary>
    public int LastResultCount { get; set; }

    /// <summary>
    ///     When the playlist was last evaluated.
    /// </summary>
    public Instant? LastEvaluatedAt { get; set; }

    /// <summary>
    ///     Whether this playlist is visible to other users.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    ///     Optional normalized form of the query for display purposes.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? NormalizedQuery { get; set; }
}
