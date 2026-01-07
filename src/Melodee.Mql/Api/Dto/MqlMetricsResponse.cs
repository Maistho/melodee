using Melodee.Mql.Services;

namespace Melodee.Mql.Api.Dto;

/// <summary>
/// Response containing MQL performance metrics.
/// </summary>
public sealed class MqlMetricsResponse
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public QueryMetricsSummary QueryMetrics { get; init; } = new();
    public CacheMetrics CacheMetrics { get; init; } = new();
    public string Status { get; init; } = "healthy";
}
