using NodaTime;

namespace Melodee.Common.Models;

public sealed record TimeSeriesPoint(LocalDate Day, double Value);
