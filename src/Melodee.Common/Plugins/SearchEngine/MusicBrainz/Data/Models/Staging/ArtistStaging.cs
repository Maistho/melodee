using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Staging;

/// <summary>
/// Staging table for raw artist data during import.
/// </summary>
[Table("ArtistStaging")]
[Index(nameof(ArtistId))]
public sealed record ArtistStaging
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ArtistId { get; init; }
    
    [MaxLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string MusicBrainzIdRaw { get; init; }
    
    [MaxLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string Name { get; init; }
    
    [MaxLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string NameNormalized { get; init; }
    
    [MaxLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string SortName { get; init; }
}
