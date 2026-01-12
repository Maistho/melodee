using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Refreshes podcast channels by fetching and parsing their RSS/Atom feeds.
/// </summary>
[DisallowConcurrentExecution]
public sealed class PodcastRefreshJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    PodcastService podcastService) : JobBase(logger, configurationFactory)
{
    public override bool DoCreateJobHistory => true;

    public override async Task Execute(IJobExecutionContext context)
    {
        var configuration = await ConfigurationFactory.GetConfigurationAsync(context.CancellationToken).ConfigureAwait(false);
        var podcastEnabled = configuration.GetValue<bool>(SettingRegistry.PodcastEnabled);
        if (!podcastEnabled)
        {
            Logger.Debug("[{JobName}] Podcast feature is disabled, skipping.", nameof(PodcastRefreshJob));
            return;
        }

        await using var dbContext = await contextFactory.CreateDbContextAsync(context.CancellationToken).ConfigureAwait(false);

        var channelsToRefresh = await dbContext.PodcastChannels
            .Where(x => !x.IsDeleted)
            .Where(x => x.NextSyncAt == null || x.NextSyncAt <= SystemClock.Instance.GetCurrentInstant())
            .Select(x => x.Id)
            .ToListAsync(context.CancellationToken).ConfigureAwait(false);

        if (channelsToRefresh.Count == 0)
        {
            Logger.Debug("[{JobName}] No channels need refresh.", nameof(PodcastRefreshJob));
            return;
        }

        Logger.Information("[{JobName}] Refreshing {ChannelCount} channels.", nameof(PodcastRefreshJob), channelsToRefresh.Count);

        foreach (var channelId in channelsToRefresh)
        {
            await podcastService.RefreshChannelAsync(channelId, context.CancellationToken).ConfigureAwait(false);
        }
    }
}
