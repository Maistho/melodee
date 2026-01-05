using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Staging;

/// <summary>
/// Staging table for artist credit names during album import.
/// Used temporarily to avoid loading all data into memory.
/// </summary>
[Table("ArtistCreditNameStaging")]
[Index(nameof(ArtistCreditId))]
public sealed record ArtistCreditNameStaging
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ArtistCreditId { get; init; }

    public int Position { get; init; }

    public long ArtistId { get; init; }
}
