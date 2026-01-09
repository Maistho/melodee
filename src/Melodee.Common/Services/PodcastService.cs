using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

/// <summary>
///     Service for managing podcast channels and episodes.
/// </summary>
public sealed class PodcastService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory) : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<DbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<OperationResult<IEnumerable<PodcastChannel>>> ListChannelsAsync(
        int userId,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var query = context.PodcastChannels
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .Include(x => x.Episodes)
                .OrderByDescending(x => x.LastSyncAt);

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            var channels = await query
                .Skip(offset ?? 0)
                .Take(limit ?? 100)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            return new OperationResult<IEnumerable<PodcastChannel>>
            {
                Data = channels,
                AdditionalData = new Dictionary<string, object> { ["TotalCount"] = totalCount }
            };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error listing channels for user {UserId}", nameof(PodcastService), userId);
            return new OperationResult<IEnumerable<PodcastChannel>>(ex.Message);
        }
    }

    public async Task<OperationResult<PodcastChannel?>> GetChannelAsync(
        int channelId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var channel = await context.PodcastChannels
                .Include(x => x.Episodes)
                .FirstOrDefaultAsync(x => x.Id == channelId && x.UserId == userId && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (channel == null)
            {
                return new OperationResult<PodcastChannel?>("Channel not found") { Data = null };
            }

            return new OperationResult<PodcastChannel?> { Data = channel };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error getting channel {ChannelId}", nameof(PodcastService), channelId);
            return new OperationResult<PodcastChannel?>(ex.Message) { Data = null };
        }
    }

    public async Task<OperationResult<PodcastChannel>> CreateChannelAsync(
        int userId,
        string feedUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
            var allowHttp = configuration.GetValue<bool>(SettingRegistry.PodcastHttpAllowHttp);

            if (!feedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !feedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return new OperationResult<PodcastChannel>("Invalid feed URL: must start with http:// or https://") { Data = null! };
            }

            if (feedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !allowHttp)
            {
                return new OperationResult<PodcastChannel>("HTTP feeds are disabled. Enable podcast.http.allowHttp to allow.") { Data = null! };
            }

            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var existingChannel = await context.PodcastChannels
                .FirstOrDefaultAsync(x => x.UserId == userId && x.FeedUrl == feedUrl, cancellationToken)
                .ConfigureAwait(false);

            if (existingChannel != null)
            {
                return new OperationResult<PodcastChannel>("Channel with this feed URL already exists") { Data = null! };
            }

            var channel = new PodcastChannel
            {
                UserId = userId,
                FeedUrl = feedUrl,
                Title = "New Podcast",
                CreatedAt = SystemClock.Instance.GetCurrentInstant()
            };

            context.PodcastChannels.Add(channel);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            Logger.Information("[{ServiceName}] Created channel {ChannelId} for user {UserId}", nameof(PodcastService), channel.Id, userId);

            return new OperationResult<PodcastChannel> { Data = channel };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error creating channel for user {UserId}", nameof(PodcastService), userId);
            return new OperationResult<PodcastChannel>(ex.Message) { Data = null! };
        }
    }

    public async Task<OperationResult<bool>> DeleteChannelAsync(
        int channelId,
        int userId,
        bool softDelete = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var channel = await context.PodcastChannels
                .Include(x => x.Episodes)
                .FirstOrDefaultAsync(x => x.Id == channelId && x.UserId == userId, cancellationToken)
                .ConfigureAwait(false);

            if (channel == null)
            {
                return new OperationResult<bool>("Channel not found") { Data = false };
            }

            if (softDelete)
            {
                channel.IsDeleted = true;
                channel.LastUpdatedAt = SystemClock.Instance.GetCurrentInstant();

                foreach (var episode in channel.Episodes.Where(x => x.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded))
                {
                    episode.DownloadStatus = PodcastEpisodeDownloadStatus.None;
                    episode.LocalPath = null;
                    episode.LocalFileSize = null;
                }
            }
            else
            {
                context.PodcastEpisodes.RemoveRange(channel.Episodes);
                context.PodcastChannels.Remove(channel);
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            Logger.Information("[{ServiceName}] Deleted channel {ChannelId} for user {UserId}", nameof(PodcastService), channelId, userId);

            return new OperationResult<bool> { Data = true };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error deleting channel {ChannelId}", nameof(PodcastService), channelId);
            return new OperationResult<bool>(ex.Message) { Data = false };
        }
    }

    public async Task<OperationResult<IEnumerable<PodcastEpisode>>> ListEpisodesAsync(
        int channelId,
        int userId,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var channel = await context.PodcastChannels
                .FirstOrDefaultAsync(x => x.Id == channelId && x.UserId == userId && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (channel == null)
            {
                return new OperationResult<IEnumerable<PodcastEpisode>>("Channel not found") { Data = [] };
            }

            var query = context.PodcastEpisodes
                .Where(x => x.PodcastChannelId == channelId)
                .OrderByDescending(x => x.PublishDate);

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            var episodes = await query
                .Skip(offset ?? 0)
                .Take(limit ?? 100)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            return new OperationResult<IEnumerable<PodcastEpisode>>
            {
                Data = episodes,
                AdditionalData = new Dictionary<string, object> { ["TotalCount"] = totalCount }
            };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error listing episodes for channel {ChannelId}", nameof(PodcastService), channelId);
            return new OperationResult<IEnumerable<PodcastEpisode>>(ex.Message) { Data = [] };
        }
    }

    public async Task<OperationResult<PodcastEpisode?>> GetEpisodeAsync(
        int episodeId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var episode = await context.PodcastEpisodes
                .Include(x => x.PodcastChannel)
                .FirstOrDefaultAsync(x => x.Id == episodeId && x.PodcastChannel.UserId == userId, cancellationToken)
                .ConfigureAwait(false);

            if (episode == null)
            {
                return new OperationResult<PodcastEpisode?>("Episode not found") { Data = null };
            }

            return new OperationResult<PodcastEpisode?> { Data = episode };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error getting episode {EpisodeId}", nameof(PodcastService), episodeId);
            return new OperationResult<PodcastEpisode?>(ex.Message) { Data = null };
        }
    }

    public async Task<OperationResult<bool>> QueueDownloadAsync(
        int episodeId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var episode = await context.PodcastEpisodes
                .Include(x => x.PodcastChannel)
                .FirstOrDefaultAsync(x => x.Id == episodeId && x.PodcastChannel.UserId == userId, cancellationToken)
                .ConfigureAwait(false);

            if (episode == null)
            {
                return new OperationResult<bool>("Episode not found") { Data = false };
            }

            if (episode.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded)
            {
                return new OperationResult<bool> { Data = true };
            }

            episode.DownloadStatus = PodcastEpisodeDownloadStatus.Queued;
            episode.DownloadError = null;
            episode.LastUpdatedAt = SystemClock.Instance.GetCurrentInstant();

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            Logger.Information("[{ServiceName}] Queued download for episode {EpisodeId}", nameof(PodcastService), episodeId);

            return new OperationResult<bool> { Data = true };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error queuing download for episode {EpisodeId}", nameof(PodcastService), episodeId);
            return new OperationResult<bool>(ex.Message) { Data = false };
        }
    }

    public async Task<OperationResult<bool>> DeleteEpisodeAsync(
        int episodeId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var episode = await context.PodcastEpisodes
                .Include(x => x.PodcastChannel)
                .FirstOrDefaultAsync(x => x.Id == episodeId && x.PodcastChannel.UserId == userId, cancellationToken)
                .ConfigureAwait(false);

            if (episode == null)
            {
                return new OperationResult<bool>("Episode not found") { Data = false };
            }

            if (!string.IsNullOrEmpty(episode.LocalPath))
            {
                var podcastLibraryResult = await libraryService.GetPodcastLibraryAsync(cancellationToken).ConfigureAwait(false);
                var podcastLibrary = podcastLibraryResult.Data;

                if (podcastLibrary != null)
                {
                    var filePath = Path.Combine(podcastLibrary.Path, episode.LocalPath);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }

            episode.DownloadStatus = PodcastEpisodeDownloadStatus.None;
            episode.LocalPath = null;
            episode.LocalFileSize = null;
            episode.LastUpdatedAt = SystemClock.Instance.GetCurrentInstant();

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            Logger.Information("[{ServiceName}] Deleted episode {EpisodeId}", nameof(PodcastService), episodeId);

            return new OperationResult<bool> { Data = true };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error deleting episode {EpisodeId}", nameof(PodcastService), episodeId);
            return new OperationResult<bool>(ex.Message) { Data = false };
        }
    }

    public async Task<OperationResult<IEnumerable<PodcastEpisode>>> GetNewestEpisodesAsync(
        int userId,
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var episodes = await context.PodcastEpisodes
                .Include(x => x.PodcastChannel)
                .Where(x => x.PodcastChannel.UserId == userId && !x.PodcastChannel.IsDeleted)
                .OrderByDescending(x => x.PublishDate)
                .Take(count)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new OperationResult<IEnumerable<PodcastEpisode>> { Data = episodes };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error getting newest episodes for user {UserId}", nameof(PodcastService), userId);
            return new OperationResult<IEnumerable<PodcastEpisode>>(ex.Message) { Data = [] };
        }
    }
}
