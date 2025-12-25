using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Quartz job listener that records job execution history to the database.
/// </summary>
/// <remarks>
///     <para>
///         This listener is automatically invoked by Quartz before and after each job execution.
///         It creates a JobHistory record to track when jobs run, how long they take, and whether they succeed.
///     </para>
///     <para>
///         Jobs can opt out of history recording by overriding DoCreateJobHistory to return false.
///         This is useful for high-frequency jobs like NowPlayingCleanup that would create excessive records.
///     </para>
/// </remarks>
public class JobHistoryListener(IServiceScopeFactory scopeFactory, ILogger logger) : IJobListener
{
    private const string StartTimeKey = "JobHistoryStartTime";

    public string Name => "JobHistoryListener";

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        context.Put(StartTimeKey, SystemClock.Instance.GetCurrentInstant());
        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        try
        {
            if (context.JobInstance is JobBase jobBase && !jobBase.DoCreateJobHistory)
            {
                return;
            }

            var startTime = context.Get(StartTimeKey) as Instant? ?? Instant.FromDateTimeOffset(context.FireTimeUtc);
            var completedAt = SystemClock.Instance.GetCurrentInstant();
            var duration = completedAt - startTime;

            // Check if this was a manual trigger (recovering or triggered now vs scheduled)
            var wasManualTrigger = context.Recovering ||
                                   context.Trigger.Key.Name.Contains("Manual", StringComparison.OrdinalIgnoreCase) ||
                                   context.Trigger is ISimpleTrigger { RepeatCount: 0 };

            var jobHistory = new JobHistory
            {
                JobName = context.JobDetail.Key.Name,
                StartedAt = startTime,
                CompletedAt = completedAt,
                DurationInMs = duration.TotalMilliseconds,
                Success = jobException == null,
                ErrorMessage = jobException?.Message,
                WasManualTrigger = wasManualTrigger
            };

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MelodeeDbContext>();

            dbContext.JobHistories.Add(jobHistory);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.Debug(
                "[JobHistoryListener] Recorded history for job {JobName}: Success={Success}, Duration={Duration:F2}ms, Manual={Manual}",
                jobHistory.JobName,
                jobHistory.Success,
                jobHistory.DurationInMs,
                jobHistory.WasManualTrigger);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "[JobHistoryListener] Failed to record job history for {JobName}", context.JobDetail.Key.Name);
        }
    }
}
