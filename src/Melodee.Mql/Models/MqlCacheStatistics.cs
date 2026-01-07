namespace Melodee.Mql.Models;

/// <summary>
/// Statistics for the MQL expression cache.
/// </summary>
public sealed record MqlCacheStatistics
{
    /// <summary>
    /// Total number of cache hits.
    /// </summary>
    public long HitCount { get; init; }

    /// <summary>
    /// Total number of cache misses.
    /// </summary>
    public long MissCount { get; init; }

    /// <summary>
    /// Total number of cache entries.
    /// </summary>
    public int EntryCount { get; init; }

    /// <summary>
    /// Estimated memory usage in bytes.
    /// </summary>
    public long MemoryEstimateBytes { get; init; }

    /// <summary>
    /// Cache hit rate as a percentage (0-100).
    /// </summary>
    public double HitRatePercentage => HitCount + MissCount > 0
        ? (double)HitCount / (HitCount + MissCount) * 100
        : 0;

    /// <summary>
    /// Time of last cache eviction.
    /// </summary>
    public DateTime? LastEvictionTime { get; init; }

    /// <summary>
    /// Number of entries evicted in last cleanup.
    /// </summary>
    public int LastEvictionCount { get; init; }

    /// <summary>
    /// Maximum number of entries allowed.
    /// </summary>
    public int MaxEntries { get; init; }

    /// <summary>
    /// Default TTL in minutes.
    /// </summary>
    public int DefaultTtlMinutes { get; init; }
}
