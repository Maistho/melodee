using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Staging;

/// <summary>
/// Staging table for release group data during album import.
/// Used temporarily to avoid loading all data into memory.
/// </summary>
[Table("ReleaseGroupStaging")]
[Index(nameof(ReleaseGroupId))]
public sealed record ReleaseGroupStaging
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ReleaseGroupId { get; init; }

    public long ArtistCreditId { get; init; }

    public int ReleaseType { get; init; }

    [MaxLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string MusicBrainzIdRaw { get; init; }
}
