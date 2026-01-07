using System.Collections.Concurrent;

namespace Melodee.Mql.Security;

/// <summary>
/// In-memory implementation of the security monitor for development and testing.
/// In production, this should be replaced with a proper logging/monitoring system.
/// </summary>
public sealed class MqlSecurityMonitor : IMqlSecurityMonitor
{
    private readonly ConcurrentQueue<SecurityEvent> _events;
    private readonly ConcurrentDictionary<string, long> _violationCounts;
    private readonly int _maxEvents;
    private readonly SemaphoreSlim _cleanupSemaphore = new(1, 1);

    public MqlSecurityMonitor(int maxEvents = 10000)
    {
        _events = new ConcurrentQueue<SecurityEvent>();
        _violationCounts = new ConcurrentDictionary<string, long>();
        _maxEvents = maxEvents;
    }

    public void LogWarning(string message, string? query = null)
    {
        var securityEvent = new SecurityEvent
        {
            EventType = "Warning",
            Message = message,
            Query = TruncateQuery(query),
            IsBlocked = false
        };

        RecordEvent(securityEvent);
    }

    public void LogViolation(string errorCode, string message, string query, bool isBlocked = true)
    {
        var securityEvent = new SecurityEvent
        {
            EventType = "Violation",
            Message = message,
            Query = TruncateQuery(query),
            IsBlocked = isBlocked,
            AdditionalData = new Dictionary<string, string>
            {
                ["ErrorCode"] = errorCode
            }
        };

        _violationCounts.AddOrIncrement(errorCode);
        RecordEvent(securityEvent);
    }

    public async Task<SecurityMetrics> GetMetricsAsync(TimeSpan window, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow - window;
        var eventsInWindow = _events.Where(e => e.Timestamp >= cutoffTime).ToList();

        var sqlInjectionCount = eventsInWindow.Count(e => e.EventType == "Violation" &&
            e.AdditionalData.TryGetValue("ErrorCode", out var code) &&
            (code == "MQL_SQL_INJECTION" || code == "MQL_DANGEROUS_PATTERN"));

        var redosCount = eventsInWindow.Count(e => e.EventType == "Violation" &&
            e.AdditionalData.TryGetValue("ErrorCode", out var code) &&
            code == "MQL_REGEX_DANGEROUS");

        var dangerousPatternCount = eventsInWindow.Count(e => e.EventType == "Violation" &&
            e.AdditionalData.TryGetValue("ErrorCode", out var code) &&
            (code == "MQL_DANGEROUS_PATTERN" || code == "MQL_SQL_INJECTION"));

        await Task.CompletedTask;

        return new SecurityMetrics
        {
            TotalViolations = eventsInWindow.Count(e => e.EventType == "Violation"),
            SqlInjectionAttempts = sqlInjectionCount,
            RedosAttempts = redosCount,
            DangerousPatternDetections = dangerousPatternCount,
            Window = window,
            FromTime = cutoffTime,
            ToTime = DateTime.UtcNow
        };
    }

    public void RecordEvent(SecurityEvent securityEvent)
    {
        _events.Enqueue(securityEvent);

        while (_events.Count > _maxEvents)
        {
            _events.TryDequeue(out _);
        }
    }

    public IReadOnlyList<SecurityEvent> GetRecentEvents(int count = 100)
    {
        return _events.TakeLast(count).ToList();
    }

    private static string? TruncateQuery(string? query)
    {
        if (query == null)
        {
            return null;
        }

        return query.Length > 500 ? query[..500] + "..." : query;
    }
}

/// <summary>
/// Extension methods for ConcurrentDictionary counters.
/// </summary>
internal static class ConcurrentDictionaryExtensions
{
    public static long AddOrIncrement(this ConcurrentDictionary<string, long> dictionary, string key)
    {
        return dictionary.AddOrUpdate(key, 1, (_, oldValue) => oldValue + 1);
    }
}
