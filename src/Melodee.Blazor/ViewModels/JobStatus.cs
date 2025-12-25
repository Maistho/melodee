using Quartz;

namespace Melodee.Blazor.ViewModels;

public sealed record JobStatus(string Group, string Name, string TriggerName, string TriggerGroup, string TriggerType, string TriggerState, string? NextFireTime, string? PreviousFireTime);

/// <summary>
///     Extended job status information for the admin jobs page.
/// </summary>
public sealed record JobStatusDetail
{
    public required string JobName { get; init; }

    public required string JobGroup { get; init; }

    public required JobKey JobKey { get; init; }

    public string? Description { get; init; }

    public required string TriggerState { get; init; }

    public required string TriggerType { get; init; }

    public string? CronExpression { get; init; }

    public string? CronDescription { get; init; }

    public DateTimeOffset? NextFireTimeUtc { get; init; }

    public DateTimeOffset? PreviousFireTimeUtc { get; init; }

    public DateTimeOffset? StartTimeUtc { get; init; }

    public DateTimeOffset? EndTimeUtc { get; init; }

    public int TimesTriggered { get; init; }

    public bool IsCurrentlyExecuting { get; init; }

    public TimeSpan? RunTime { get; init; }

    public bool IsDurable { get; init; }

    public bool DisallowConcurrentExecution { get; init; }

    public bool PersistJobDataAfterExecution { get; init; }

    public bool RequestsRecovery { get; init; }

    public IDictionary<string, object>? JobData { get; init; }

    // Job History Stats
    public int TotalRunCount { get; init; }

    public int SuccessCount { get; init; }

    public int FailureCount { get; init; }

    public int ManualTriggerCount { get; init; }

    public double? AverageDurationMs { get; init; }

    public double? MinDurationMs { get; init; }

    public double? MaxDurationMs { get; init; }

    public string? LastErrorMessage { get; init; }

    public DateTimeOffset? LastFailureTime { get; init; }

    public string NextFireTimeDisplay => NextFireTimeUtc?.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not scheduled";

    public string PreviousFireTimeDisplay => PreviousFireTimeUtc?.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never";

    public string RunTimeDisplay => RunTime.HasValue
        ? RunTime.Value.TotalSeconds < 60
            ? $"{RunTime.Value.TotalSeconds:F1}s"
            : $"{RunTime.Value.TotalMinutes:F1}m"
        : "---";

    public double SuccessRate => TotalRunCount > 0 ? (double)SuccessCount / TotalRunCount * 100 : 0;

    public string SuccessRateDisplay => TotalRunCount > 0 ? $"{SuccessRate:F1}%" : "---";

    public string AverageDurationDisplay => AverageDurationMs.HasValue
        ? AverageDurationMs.Value < 1000
            ? $"{AverageDurationMs.Value:F0}ms"
            : AverageDurationMs.Value < 60000
                ? $"{AverageDurationMs.Value / 1000:F1}s"
                : $"{AverageDurationMs.Value / 60000:F1}m"
        : "---";

    public string MinMaxDurationDisplay => MinDurationMs.HasValue && MaxDurationMs.HasValue
        ? $"{FormatDuration(MinDurationMs.Value)} - {FormatDuration(MaxDurationMs.Value)}"
        : "---";

    public string StatusBadgeStyle => TriggerState switch
    {
        "Normal" => "Success",
        "Paused" => "Warning",
        "Blocked" => "Info",
        "Error" => "Danger",
        "Complete" => "Secondary",
        _ => "Light"
    };

    private static string FormatDuration(double ms) => ms < 1000
        ? $"{ms:F0}ms"
        : ms < 60000
            ? $"{ms / 1000:F1}s"
            : $"{ms / 60000:F1}m";
}
