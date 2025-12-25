using NodaTime;

namespace Melodee.Common.Models;

public sealed record TimeSeriesPoint(LocalDate Day, double Value)
{
    /// <summary>
    /// Day formatted as DateTime for chart compatibility.
    /// </summary>
    public DateTime DayAsDateTime => Day.ToDateTimeUnspecified();

    /// <summary>
    /// Day formatted as string (MM/dd) for chart axis labels.
    /// </summary>
    public string DayLabel => Day.ToString("MM/dd", null);
}
