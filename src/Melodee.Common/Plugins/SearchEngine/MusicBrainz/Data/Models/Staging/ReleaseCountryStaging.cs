using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Staging;

/// <summary>
/// Staging table for release country data during album import.
/// Used temporarily to avoid loading all data into memory.
/// </summary>
[Table("ReleaseCountryStaging")]
[Index(nameof(ReleaseId))]
public sealed record ReleaseCountryStaging
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ReleaseId { get; init; }

    public int DateYear { get; init; }

    public int DateMonth { get; init; }

    public int DateDay { get; init; }

    public int DateYearValue => DateYear > DateTime.MinValue.Year && DateYear < DateTime.MaxValue.Year
        ? DateYear
        : DateTime.MinValue.Year;

    public int DateMonthValue => DateMonth is > 0 and < 12 ? DateMonth : 1;

    public int DateDayValue => DateDay is > 0 and < 31 ? DateDay : 1;

    public bool IsValid => ReleaseId > 0 && DateDayValue > 0 && DateMonthValue > 0 && DateYearValue > 0;

    [NotMapped]
    public DateTime ReleaseDate =>
        DateTime.Parse(
            $"{DateYearValue.ToStringPadLeft(4)}-{DateMonthValue.ToStringPadLeft(2)}-{DateDayValue.ToStringPadLeft(2)}T00:00:00",
            System.Globalization.CultureInfo.InvariantCulture);
}
