using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Staging;

/// <summary>
/// Staging table for link data during artist relation import.
/// </summary>
[Table("LinkStaging")]
[Index(nameof(LinkId))]
public sealed record LinkStaging
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long LinkId { get; init; }
    
    public DateTime? BeginDate { get; init; }
    
    public DateTime? EndDate { get; init; }
}
