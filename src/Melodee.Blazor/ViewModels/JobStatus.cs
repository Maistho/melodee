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

    public string NextFireTimeDisplay => NextFireTimeUtc?.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not scheduled";

    public string PreviousFireTimeDisplay => PreviousFireTimeUtc?.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never";

    public string RunTimeDisplay => RunTime.HasValue
        ? RunTime.Value.TotalSeconds < 60
            ? $"{RunTime.Value.TotalSeconds:F1}s"
            : $"{RunTime.Value.TotalMinutes:F1}m"
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
}
