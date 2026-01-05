using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Staging;

/// <summary>
/// Staging table for release data during album import.
/// Used temporarily to avoid loading all data into memory.
/// </summary>
[Table("ReleaseStaging")]
[Index(nameof(ReleaseId))]
[Index(nameof(ReleaseGroupId))]
[Index(nameof(ArtistCreditId))]
public sealed record ReleaseStaging
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ReleaseId { get; init; }

    public long ReleaseGroupId { get; init; }

    public long ArtistCreditId { get; init; }

    [MaxLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string MusicBrainzIdRaw { get; init; }

    [MaxLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string Name { get; init; }

    [MaxLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string NameNormalized { get; init; }

    [MaxLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string SortName { get; init; }
}
