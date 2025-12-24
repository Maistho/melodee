using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(Slug), IsUnique = true)]
public sealed class Chart : DataModelBase
{
    public const string CacheRegion = "urn:region:chart";
    public const string ImagesDirectoryName = "chart-images";

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Slug { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Title { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SourceName { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? SourceUrl { get; set; }

    public int? Year { get; set; }

    public bool IsVisible { get; set; }

    public bool IsGeneratedPlaylistEnabled { get; set; }

    public ICollection<ChartItem> Items { get; set; } = new List<ChartItem>();

    public override string ToString()
    {
        return $"Id [{Id}] ApiKey [{ApiKey}] Slug [{Slug}] Title [{Title}]";
    }
}
