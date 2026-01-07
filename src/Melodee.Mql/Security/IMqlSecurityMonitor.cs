namespace Melodee.Mql.Security;

/// <summary>
/// Represents security-related metrics for monitoring.
/// </summary>
public sealed record SecurityMetrics
{
    public long TotalViolations { get; init; }
    public long SqlInjectionAttempts { get; init; }
    public long RedosAttempts { get; init; }
    public long DangerousPatternDetections { get; init; }
    public long RateLimitExceeded { get; init; }
    public TimeSpan Window { get; init; }
    public DateTime FromTime { get; init; }
    public DateTime ToTime { get; init; }
}

/// <summary>
/// Represents a single security event.
/// </summary>
public sealed record SecurityEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString("N")[..16];
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Query { get; init; }
    public string? IpAddress { get; init; }
    public string? UserId { get; init; }
    public bool IsBlocked { get; init; }
    public Dictionary<string, string> AdditionalData { get; init; } = new();
}

/// <summary>
/// Interface for monitoring and logging security events.
/// </summary>
public interface IMqlSecurityMonitor
{
    /// <summary>
    /// Logs a security warning.
    /// </summary>
    void LogWarning(string message, string? query = null);

    /// <summary>
    /// Logs a security violation.
    /// </summary>
    void LogViolation(string errorCode, string message, string query, bool isBlocked = true);

    /// <summary>
    /// Gets security metrics for a given time window.
    /// </summary>
    Task<SecurityMetrics> GetMetricsAsync(TimeSpan window, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a security event.
    /// </summary>
    void RecordEvent(SecurityEvent securityEvent);

    /// <summary>
    /// Gets recent security events.
    /// </summary>
    IReadOnlyList<SecurityEvent> GetRecentEvents(int count = 100);
}
