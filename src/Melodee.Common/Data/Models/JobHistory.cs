using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
///     Records execution history for background jobs.
/// </summary>
[Serializable]
[Index(nameof(JobName), nameof(StartedAt))]
[Index(nameof(StartedAt))]
public class JobHistory
{
    public int Id { get; set; }

    /// <summary>
    ///     Name of the job that was executed.
    /// </summary>
    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string JobName { get; set; }

    /// <summary>
    ///     When the job started execution.
    /// </summary>
    [Required]
    public required Instant StartedAt { get; set; }

    /// <summary>
    ///     When the job completed execution (success or failure).
    /// </summary>
    public Instant? CompletedAt { get; set; }

    /// <summary>
    ///     Duration of job execution in milliseconds.
    /// </summary>
    public double? DurationInMs { get; set; }

    /// <summary>
    ///     Whether the job completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     Error message if the job failed.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     Whether this was a manual trigger (vs scheduled).
    /// </summary>
    public bool WasManualTrigger { get; set; }

    [NotMapped]
    public Duration? DurationValue => DurationInMs.HasValue ? Duration.FromMilliseconds(DurationInMs.Value) : null;
}
