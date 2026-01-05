using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Staging;

/// <summary>
/// Staging table for artist-to-artist link data during import.
/// </summary>
[Table("LinkArtistToArtistStaging")]
[Index(nameof(Artist0))]
[Index(nameof(Artist1))]
[Index(nameof(LinkId))]
public sealed record LinkArtistToArtistStaging
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long LinkId { get; init; }

    public long Artist0 { get; init; }

    public long Artist1 { get; init; }

    public int LinkOrder { get; init; }
}
