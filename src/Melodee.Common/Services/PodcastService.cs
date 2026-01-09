using Melodee.Common.Models.Collection;
using Melodee.Common.Filtering;
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
using System.IO;
using System.Net.Http.Headers;
using System.ServiceModel.Syndication;
using System.Xml;

namespace Melodee.Common.Services;

/// <summary>
///     Service for managing podcast channels and episodes.
/// </summary>
public sealed class PodcastService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    LibraryService libraryService) : ServiceBase(logger, cacheManager, contextFactory)
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
            return new OperationResult<IEnumerable<PodcastChannel>>(ex.Message) { Data = [] };
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

            Logger.Information("[{ServiceName}] Created channel {ChannelId} for user {UserId}", nameof(PodcastService), channel.Id, userId);

            // Refesh the channel immediately to populate metadata and episodes
            await RefreshChannelAsync(channel.Id, cancellationToken).ConfigureAwait(false);

            // Reload to get the latest updates
            var updatedChannel = await context.PodcastChannels
                .FirstOrDefaultAsync(x => x.Id == channel.Id, cancellationToken)
                .ConfigureAwait(false);

            return new OperationResult<PodcastChannel> { Data = updatedChannel ?? channel };
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

    public async Task<OperationResult<bool>> RefreshChannelAsync(
        int channelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var channel = await context.PodcastChannels
                .Include(x => x.Episodes)
                .FirstOrDefaultAsync(x => x.Id == channelId, cancellationToken)
                .ConfigureAwait(false);

            if (channel == null)
            {
                return new OperationResult<bool>("Channel not found") { Data = false };
            }

            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
            var podcastLibraryResult = await libraryService.GetPodcastLibraryAsync(cancellationToken).ConfigureAwait(false);
            var podcastLibrary = podcastLibraryResult.Data;

            if (podcastLibrary == null)
            {
                return new OperationResult<bool>("No podcast library configured") { Data = false };
            }

            var maxItemsPerChannel = configuration.GetValue<int>(SettingRegistry.PodcastRefreshMaxItemsPerChannel);

            Logger.Information("[{ServiceName}] Refreshing channel [{ChannelId}] {ChannelTitle}", nameof(PodcastService), channel.Id, channel.Title);

            channel.LastSyncAttemptAt = SystemClock.Instance.GetCurrentInstant();

            try
            {
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>(SettingRegistry.PodcastHttpTimeoutSeconds))
                };

                using var request = new HttpRequestMessage(HttpMethod.Get, channel.FeedUrl);
                var currentEtag = channel.Etag;
                if (!string.IsNullOrEmpty(currentEtag))
                {
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{currentEtag}\""));
                }

                if (channel.LastModified.HasValue)
                {
                    request.Headers.IfModifiedSince = channel.LastModified.Value;
                }

                using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                {
                    Logger.Debug("[{ServiceName}] Channel [{ChannelId}] not modified, skipping", nameof(PodcastService), channel.Id);
                    channel.LastSyncAt = SystemClock.Instance.GetCurrentInstant();
                    channel.ConsecutiveFailureCount = 0;
                    channel.NextSyncAt = null;
                    channel.LastSyncError = null;
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return new OperationResult<bool> { Data = true };
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                channel.Etag = response.Headers.ETag?.Tag;
                channel.LastModified = response.Content.Headers.LastModified;

                await ParseAndUpdateEpisodesAsync(context, channel, content, maxItemsPerChannel, podcastLibrary, configuration, cancellationToken).ConfigureAwait(false);

                channel.LastSyncAt = SystemClock.Instance.GetCurrentInstant();
                channel.LastSyncError = null;
                channel.ConsecutiveFailureCount = 0;
                channel.NextSyncAt = null;
            }
            catch (Exception ex)
            {
                channel.LastSyncError = ex.Message;
                channel.ConsecutiveFailureCount++;
                channel.NextSyncAt = CalculateNextSyncTime(channel.ConsecutiveFailureCount);
                Logger.Error(ex, "[{ServiceName}] Error refreshing channel [{ChannelId}]", nameof(PodcastService), channel.Id);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return new OperationResult<bool>(ex.Message) { Data = false };
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return new OperationResult<bool> { Data = true };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error in RefreshChannelAsync for channel {ChannelId}", nameof(PodcastService), channelId);
            return new OperationResult<bool>(ex.Message) { Data = false };
        }
    }

    private async Task ParseAndUpdateEpisodesAsync(MelodeeDbContext context, PodcastChannel channel, string feedContent, int maxItemsPerChannel, Library podcastLibrary, IMelodeeConfiguration configuration, CancellationToken cancellationToken)
    {
        var feed = SyndicationFeed.Load(
            XmlReader.Create(new StringReader(feedContent), new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            }));

        if (feed.Id.Nullify() == null && feed.Title.Text.Nullify() == null)
        {
            throw new Exception("Invalid feed: missing id or title");
        }

        channel.Title = feed.Title.Text;
        channel.Description = feed.Description?.Text;
        channel.SiteUrl = feed.Links.FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri?.ToString();

        var imageLink = feed.ImageUrl?.ToString() ??
                        feed.Links.FirstOrDefault(l => l.MediaType?.Contains("image") == true)?.Uri?.ToString();
        channel.ImageUrl = imageLink;

        if (!string.IsNullOrEmpty(imageLink))
        {
            try
            {
                channel.CoverArtLocalPath = await DownloadCoverArtAsync(channel, imageLink, podcastLibrary, configuration, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "[{ServiceName}] Failed to download cover art for channel [{ChannelId}]", nameof(PodcastService), channel.Id);
            }
        }

        var existingEpisodes = channel.Episodes.ToDictionary(x => x.EpisodeKey);
        // If episodes weren't loaded, we might want to query them, but typically we Include them in RefreshChannelAsync or CreateChannelAsync is new.
        // In RefreshChannelAsync above we used .Include(x => x.Episodes).

        var episodeCount = 0;
        foreach (var item in feed.Items.Take(maxItemsPerChannel))
        {
            var episodeKey = item.Id.Nullify() ?? $"{item.Links.FirstOrDefault(l => l.Uri != null)?.Uri}#{item.PublishDate:yyyy-MM-dd HH:mm:ss}";

            var enclosure = item.Links.FirstOrDefault(l => l.MediaType?.StartsWith("audio/") == true || l.MediaType?.StartsWith("video/") == true);

            if (enclosure?.Uri == null)
            {
                continue;
            }

            if (!existingEpisodes.TryGetValue(episodeKey, out var episode))
            {
                episode = new PodcastEpisode
                {
                    PodcastChannelId = channel.Id,
                    EpisodeKey = episodeKey,
                    Title = "Untitled",
                    EnclosureUrl = enclosure.Uri.ToString(),
                    CreatedAt = SystemClock.Instance.GetCurrentInstant()
                };
                context.PodcastEpisodes.Add(episode);
                existingEpisodes[episodeKey] = episode;
            }

            episode.Title = item.Title?.Text ?? "Untitled";
            episode.Description = item.Summary?.Text ?? item.Content?.ToString();
            episode.PublishDate = item.PublishDate;
            episode.Guid = item.Id;
            episode.EnclosureUrl = enclosure.Uri.ToString();
            episode.MimeType = enclosure.MediaType;

            if (enclosure.Length > 0)
            {
                episode.EnclosureLength = enclosure.Length;
            }
            else
            {
                episode.EnclosureLength = null;
            }

            episodeCount++;
        }
        
        Logger.Information("[{ServiceName}] Processed {EpisodeCount} episodes for channel [{ChannelId}]", nameof(PodcastService), episodeCount, channel.Id);
    }

    private async Task<string?> DownloadCoverArtAsync(PodcastChannel channel, string imageUrl, Library podcastLibrary, IMelodeeConfiguration configuration, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>(SettingRegistry.PodcastHttpTimeoutSeconds))
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Logger.Warning("[{ServiceName}] Failed to download cover art for channel [{ChannelId}]: HTTP {StatusCode}", nameof(PodcastService), channel.Id, (int)response.StatusCode);
                return null;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            var extension = contentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                _ => ".jpg"
            };

            var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

            var userDirectory = Path.Combine(podcastLibrary.Path, channel.UserId.ToString());
            var channelDirectory = Path.Combine(userDirectory, channel.Id.ToString());
            Directory.CreateDirectory(channelDirectory);

            var fileName = $"cover{extension}";
            var filePath = Path.Combine(channelDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken).ConfigureAwait(false);

            var relativePath = Path.Combine(channel.UserId.ToString(), channel.Id.ToString(), fileName);
            Logger.Information("[{ServiceName}] Downloaded cover art for channel [{ChannelId}] to {Path}", nameof(PodcastService), channel.Id, relativePath);

            return relativePath;
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "[{ServiceName}] Error downloading cover art for channel [{ChannelId}]", nameof(PodcastService), channel.Id);
            return null;
        }
    }

    private static Instant? CalculateNextSyncTime(int failureCount)
    {
        var initialInterval = TimeSpan.FromMinutes(15);
        var maxBackoff = TimeSpan.FromHours(24);

        var backoff = TimeSpan.FromMinutes(Math.Pow(2, failureCount) * initialInterval.TotalMinutes);
        if (backoff > maxBackoff)
        {
            backoff = maxBackoff;
        }

        return SystemClock.Instance.GetCurrentInstant() + Duration.FromTimeSpan(backoff);
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

    public async Task<PagedResult<PodcastChannelDataInfo>> ListChannelsAsync(PagedRequest request, int userId, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var query = context.PodcastChannels.Where(x => x.UserId == userId && !x.IsDeleted).AsNoTracking();

        if (request.FilterBy?.Any() == true)
        {
            var filter = request.FilterBy.First(); // Assuming simple filter for now
            if (filter.PropertyName.Equals(nameof(PodcastChannelDataInfo.TitleNormalized), StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => EF.Functions.Like(x.Title, $"%{filter.Value}%"));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var channels = await query
            .OrderBy(x => x.Title)
            .Skip(request.SkipValue)
            .Take(request.TakeValue)
            .Select(x => new PodcastChannelDataInfo(
                x.Id,
                x.ApiKey,
                x.Title,
                x.Title,
                x.Description ?? string.Empty,
                x.ImageUrl ?? string.Empty,
                x.FeedUrl,
                x.SiteUrl ?? string.Empty,
                x.LastSyncAt,
                x.CreatedAt,
                string.Empty,
                false,
                0,
                null,
                0))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<PodcastChannelDataInfo>
        {
            TotalCount = totalCount,
            TotalPages = request.TotalPages(totalCount),
            Data = channels
        };
    }

    public async Task<PagedResult<PodcastEpisodeDataInfo>> ListEpisodesAsync(PagedRequest request, int userId, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var query = context.PodcastEpisodes
            .Include(x => x.PodcastChannel)
            .Where(x => x.PodcastChannel.UserId == userId && !x.PodcastChannel.IsDeleted)
            .AsNoTracking();

        if (request.FilterBy?.Any() == true)
        {
            var filter = request.FilterBy.First();
            if (filter.PropertyName.Equals(nameof(PodcastEpisodeDataInfo.TitleNormalized), StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => EF.Functions.Like(x.Title, $"%{filter.Value}%"));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var episodes = await query
            .OrderByDescending(x => x.PublishDate)
            .Skip(request.SkipValue)
            .Take(request.TakeValue)
            .Select(x => new PodcastEpisodeDataInfo(
                x.Id,
                x.ApiKey,
                x.Title,
                x.Title,
                x.Description ?? string.Empty,
                x.PublishDate ?? x.CreatedAt.ToDateTimeOffset(),
                0, // Duration
                x.PodcastChannel.Title,
                x.PodcastChannel.ApiKey,
                x.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded,
                x.CreatedAt,
                string.Empty,
                false,
                0,
                null,
                0))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<PodcastEpisodeDataInfo>
        {
            TotalCount = totalCount,
            TotalPages = request.TotalPages(totalCount),
            Data = episodes
        };
    }
}
