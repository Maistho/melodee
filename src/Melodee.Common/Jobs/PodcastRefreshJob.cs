using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;
using System.IO;
using System.Net.Http.Headers;
using System.ServiceModel.Syndication;
using System.Xml;

namespace Melodee.Common.Jobs;

/// <summary>
///     Refreshes podcast channels by fetching and parsing their RSS/Atom feeds.
/// </summary>
[DisallowConcurrentExecution]
public sealed class PodcastRefreshJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    MelodeeDbContext dbContext,
    LibraryService libraryService) : JobBase(logger, configurationFactory)
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

        var podcastLibrary = (await libraryService.GetPodcastLibraryAsync(context.CancellationToken).ConfigureAwait(false)).Data;
        if (podcastLibrary == null)
        {
            Logger.Warning("[{JobName}] No podcast library configured.", nameof(PodcastRefreshJob));
            return;
        }

        var maxItemsPerChannel = configuration.GetValue<int>(SettingRegistry.PodcastRefreshMaxItemsPerChannel);

        var channelsToRefresh = await dbContext.PodcastChannels
            .Where(x => !x.IsDeleted)
            .Where(x => x.NextSyncAt == null || x.NextSyncAt <= SystemClock.Instance.GetCurrentInstant())
            .ToListAsync(context.CancellationToken).ConfigureAwait(false);

        if (channelsToRefresh.Count == 0)
        {
            Logger.Debug("[{JobName}] No channels need refresh.", nameof(PodcastRefreshJob));
            return;
        }

        Logger.Information("[{JobName}] Refreshing {ChannelCount} channels.", nameof(PodcastRefreshJob), channelsToRefresh.Count);

        foreach (var channel in channelsToRefresh)
        {
            try
            {
                await RefreshChannelAsync(channel, podcastLibrary, maxItemsPerChannel, configuration, context.CancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[{JobName}] Error refreshing channel [{ChannelId}]", nameof(PodcastRefreshJob), channel.Id);
                channel.LastSyncAttemptAt = SystemClock.Instance.GetCurrentInstant();
                channel.LastSyncError = ex.Message;
                channel.ConsecutiveFailureCount++;
                channel.NextSyncAt = CalculateNextSyncTime(channel.ConsecutiveFailureCount, configuration);
                await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task RefreshChannelAsync(PodcastChannel channel, Library podcastLibrary, int maxItemsPerChannel, IMelodeeConfiguration configuration, CancellationToken cancellationToken)
    {
        Logger.Information("[{JobName}] Refreshing channel [{ChannelId}] {ChannelTitle}", nameof(PodcastRefreshJob), channel.Id, channel.Title);

        channel.LastSyncAttemptAt = SystemClock.Instance.GetCurrentInstant();

        try
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>(SettingRegistry.PodcastHttpTimeoutSeconds))
            };

            var request = new HttpRequestMessage(HttpMethod.Get, channel.FeedUrl);
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
                Logger.Debug("[{JobName}] Channel [{ChannelId}] not modified, skipping", nameof(PodcastRefreshJob), channel.Id);
                channel.LastSyncAt = SystemClock.Instance.GetCurrentInstant();
                channel.ConsecutiveFailureCount = 0;
                channel.NextSyncAt = null;
                channel.LastSyncError = null;
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            channel.Etag = response.Headers.ETag?.Tag;
            channel.LastModified = response.Content.Headers.LastModified;

            await ParseAndUpdateEpisodesAsync(channel, content, maxItemsPerChannel, podcastLibrary, configuration, cancellationToken).ConfigureAwait(false);

            channel.LastSyncAt = SystemClock.Instance.GetCurrentInstant();
            channel.LastSyncError = null;
            channel.ConsecutiveFailureCount = 0;
            channel.NextSyncAt = null;
        }
        catch (Exception ex)
        {
            channel.LastSyncError = ex.Message;
            channel.ConsecutiveFailureCount++;
            channel.NextSyncAt = CalculateNextSyncTime(channel.ConsecutiveFailureCount, configuration);
            throw;
        }
        finally
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ParseAndUpdateEpisodesAsync(PodcastChannel channel, string feedContent, int maxItemsPerChannel, Library podcastLibrary, IMelodeeConfiguration configuration, CancellationToken cancellationToken)
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
                Logger.Warning(ex, "[{JobName}] Failed to download cover art for channel [{ChannelId}]", nameof(PodcastRefreshJob), channel.Id);
            }
        }

        var existingEpisodes = (await dbContext.PodcastEpisodes
            .Where(x => x.PodcastChannelId == channel.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(x => x.EpisodeKey);

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
                dbContext.PodcastEpisodes.Add(episode);
                existingEpisodes[episodeKey] = episode;
            }

            episode.Title = item.Title?.Text ?? "Untitled";
            episode.Description = item.Summary?.Text ?? item.Content?.ToString();
            episode.PublishDate = item.PublishDate;
            episode.Guid = item.Id;
            episode.EnclosureUrl = enclosure.Uri.ToString();
            episode.MimeType = enclosure.MediaType;

            if (!enclosure.Length.HasValue)
            {
                episode.EnclosureLength = null;
            }

            episodeCount++;
        }

        Logger.Information("[{JobName}] Processed {EpisodeCount} episodes for channel [{ChannelId}]", nameof(PodcastRefreshJob), episodeCount, channel.Id);
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
                Logger.Warning("[{JobName}] Failed to download cover art for channel [{ChannelId}]: HTTP {StatusCode}", nameof(PodcastRefreshJob), channel.Id, (int)response.StatusCode);
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
            Logger.Information("[{JobName}] Downloaded cover art for channel [{ChannelId}] to {Path}", nameof(PodcastRefreshJob), channel.Id, relativePath);

            return relativePath;
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "[{JobName}] Error downloading cover art for channel [{ChannelId}]", nameof(PodcastRefreshJob), channel.Id);
            return null;
        }
    }

    private static Instant? CalculateNextSyncTime(int failureCount, IMelodeeConfiguration configuration)
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
}
