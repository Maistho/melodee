using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(ChartId), nameof(Rank), IsUnique = true)]
[Index(nameof(ChartId), nameof(LinkedAlbumId))]
public sealed class ChartItem
{
    public int Id { get; set; }

    [RequiredGreaterThanZero]
    public int ChartId { get; set; }

    public Chart Chart { get; set; } = null!;

    [Required]
    public int Rank { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string ArtistName { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string AlbumTitle { get; set; }

    public int? ReleaseYear { get; set; }

    public int? LinkedArtistId { get; set; }

    public Artist? LinkedArtist { get; set; }

    public int? LinkedAlbumId { get; set; }

    public Album? LinkedAlbum { get; set; }

    public short LinkStatus { get; set; } = (short)ChartItemLinkStatus.Unlinked;

    [NotMapped]
    public ChartItemLinkStatus LinkStatusValue => SafeParser.ToEnum<ChartItemLinkStatus>(LinkStatus);

    public decimal? LinkConfidence { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? LinkNotes { get; set; }

    [Required]
    public required Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

    public Instant? LastUpdatedAt { get; set; }

    public override string ToString()
    {
        return $"Id [{Id}] ChartId [{ChartId}] Rank [{Rank}] Artist [{ArtistName}] Album [{AlbumTitle}]";
    }
}
