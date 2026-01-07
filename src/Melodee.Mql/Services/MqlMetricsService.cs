using System.Collections.Concurrent;
using System.Diagnostics;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql.Services;

/// <summary>
/// Service for collecting and aggregating MQL performance metrics.
/// </summary>
public sealed class MqlMetricsService
{
    private readonly ConcurrentQueue<QueryMetrics> _metrics = new();
    private readonly ConcurrentDictionary<string, long> _errorCounts = new();
    private readonly TimeSpan _window = TimeSpan.FromMinutes(5);
    private const int MaxMetrics = 10000;

    public void RecordQuery(TimeSpan duration, string entityType, bool isValid, string? errorCode = null)
    {
        _metrics.Enqueue(new QueryMetrics
        {
            Timestamp = DateTime.UtcNow,
            DurationMs = duration.TotalMilliseconds,
            EntityType = entityType,
            IsValid = isValid,
            ErrorCode = errorCode
        });

        while (_metrics.Count > MaxMetrics)
        {
            _metrics.TryDequeue(out _);
        }

        if (!isValid && errorCode != null)
        {
            _errorCounts.AddOrIncrement(errorCode);
        }
    }

    public QueryMetricsSummary GetSummary()
    {
        var now = DateTime.UtcNow;
        var windowStart = now - _window;

        var recentMetrics = _metrics.Where(m => m.Timestamp >= windowStart).ToList();

        if (!recentMetrics.Any())
        {
            return new QueryMetricsSummary
            {
                WindowStart = windowStart,
                WindowEnd = now,
                TotalQueries = 0
            };
        }

        var durations = recentMetrics.Select(m => m.DurationMs).OrderBy(x => x).ToList();
        var validQueries = recentMetrics.Where(m => m.IsValid).ToList();
        var totalCount = recentMetrics.Count;

        return new QueryMetricsSummary
        {
            WindowStart = windowStart,
            WindowEnd = now,
            TotalQueries = totalCount,
            ValidQueries = validQueries.Count,
            FailedQueries = totalCount - validQueries.Count,
            MinLatencyMs = durations.FirstOrDefault(),
            MaxLatencyMs = durations.LastOrDefault(),
            P50LatencyMs = GetPercentile(durations, 50),
            P95LatencyMs = GetPercentile(durations, 95),
            P99LatencyMs = GetPercentile(durations, 99),
            ErrorCounts = _errorCounts.ToDictionary(kv => kv.Key, kv => kv.Value)
        };
    }

    public CacheMetrics GetCacheMetrics(IMqlExpressionCache cache)
    {
        var stats = cache.GetStatistics();
        return new CacheMetrics
        {
            EntryCount = stats.EntryCount,
            HitCount = stats.HitCount,
            MissCount = stats.MissCount,
            EvictionCount = stats.LastEvictionCount,
            MemoryUsageBytes = stats.MemoryEstimateBytes,
            HitRate = stats.HitCount + stats.MissCount > 0
                ? (double)stats.HitCount / (stats.HitCount + stats.MissCount)
                : 0
        };
    }

    private static double GetPercentile(List<double> sortedValues, int percentile)
    {
        if (!sortedValues.Any())
        {
            return 0;
        }

        var index = (int)Math.Ceiling((percentile / 100.0) * sortedValues.Count) - 1;
        return sortedValues[Math.Max(0, index)];
    }

    public void Reset()
    {
        while (_metrics.TryDequeue(out _))
        {
        }

        _errorCounts.Clear();
    }
}

public sealed record QueryMetrics
{
    public DateTime Timestamp { get; init; }
    public double DurationMs { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public string? ErrorCode { get; init; }
}

public sealed record QueryMetricsSummary
{
    public DateTime WindowStart { get; init; }
    public DateTime WindowEnd { get; init; }
    public int TotalQueries { get; init; }
    public int ValidQueries { get; init; }
    public int FailedQueries { get; init; }
    public double MinLatencyMs { get; init; }
    public double MaxLatencyMs { get; init; }
    public double P50LatencyMs { get; init; }
    public double P95LatencyMs { get; init; }
    public double P99LatencyMs { get; init; }
    public Dictionary<string, long> ErrorCounts { get; init; } = new();

    public double ErrorRate => TotalQueries > 0 ? (double)FailedQueries / TotalQueries : 0;
}

public sealed record CacheMetrics
{
    public int EntryCount { get; init; }
    public long HitCount { get; init; }
    public long MissCount { get; init; }
    public long EvictionCount { get; init; }
    public long MemoryUsageBytes { get; init; }
    public double HitRate { get; init; }
}

public static class ConcurrentDictionaryExtensions
{
    public static void AddOrIncrement(this ConcurrentDictionary<string, long> dictionary, string key)
    {
        dictionary.AddOrUpdate(key, 1, (_, existing) => existing + 1);
    }
}
