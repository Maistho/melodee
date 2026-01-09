using System.Security.Cryptography;
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

namespace Melodee.Common.Jobs;

/// <summary>
///     Downloads queued podcast episodes to local storage.
/// </summary>
[DisallowConcurrentExecution]
public sealed class PodcastDownloadJob(
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
            Logger.Debug("[{JobName}] Podcast feature is disabled, skipping.", nameof(PodcastDownloadJob));
            return;
        }

        var podcastLibrary = (await libraryService.GetPodcastLibraryAsync(context.CancellationToken).ConfigureAwait(false)).Data;
        if (podcastLibrary == null)
        {
            Logger.Warning("[{JobName}] No podcast library configured.", nameof(PodcastDownloadJob));
            return;
        }

        var maxConcurrentGlobal = configuration.GetValue<int>(SettingRegistry.PodcastDownloadMaxConcurrentGlobal);
        var maxConcurrentPerUser = configuration.GetValue<int>(SettingRegistry.PodcastDownloadMaxConcurrentPerUser);
        var maxEnclosureBytes = configuration.GetValue<long>(SettingRegistry.PodcastDownloadMaxEnclosureBytes);
        var maxBytesPerUser = configuration.GetValue<long>(SettingRegistry.PodcastQuotaMaxBytesPerUser);
        var timeoutSeconds = configuration.GetValue<int>(SettingRegistry.PodcastHttpTimeoutSeconds);
        var maxRedirects = configuration.GetValue<int>(SettingRegistry.PodcastHttpMaxRedirects);

        var episodesToDownload = await dbContext.PodcastEpisodes
            .Include(x => x.PodcastChannel)
            .Where(x => x.DownloadStatus == PodcastEpisodeDownloadStatus.Queued)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(context.CancellationToken).ConfigureAwait(false);

        if (episodesToDownload.Count == 0)
        {
            Logger.Debug("[{JobName}] No episodes to download.", nameof(PodcastDownloadJob));
            return;
        }

        Logger.Information("[{JobName}] Processing {EpisodeCount} episodes for download.", nameof(PodcastDownloadJob), episodesToDownload.Count);

        var processedUserIds = new HashSet<int>();
        var userUsageCache = new Dictionary<int, long>();
        var processedCount = 0;

        foreach (var episode in episodesToDownload)
        {
            if (processedCount >= maxConcurrentGlobal)
            {
                Logger.Debug("[{JobName}] Global concurrent limit reached.", nameof(PodcastDownloadJob));
                break;
            }

            var userId = episode.PodcastChannel.UserId;

            if (processedUserIds.Contains(userId))
            {
                Logger.Debug("[{JobName}] Per-user limit reached for user {UserId}, skipping.", nameof(PodcastDownloadJob), userId);
                continue;
            }

            if (maxBytesPerUser > 0)
            {
                if (!userUsageCache.TryGetValue(userId, out var currentUsage))
                {
                    currentUsage = await dbContext.PodcastEpisodes
                        .Where(x => x.PodcastChannel.UserId == userId && x.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded)
                        .SumAsync(x => x.LocalFileSize ?? 0, context.CancellationToken)
                        .ConfigureAwait(false);
                    userUsageCache[userId] = currentUsage;
                }

                if (currentUsage >= maxBytesPerUser)
                {
                    Logger.Warning("[{JobName}] User {UserId} has reached their storage quota of {Quota} bytes.", nameof(PodcastDownloadJob), userId, maxBytesPerUser);
                    episode.DownloadStatus = PodcastEpisodeDownloadStatus.Failed;
                    episode.DownloadError = "User storage quota reached.";
                    await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
                    continue;
                }
            }

            try
            {
                await DownloadEpisodeAsync(episode, podcastLibrary, maxEnclosureBytes, timeoutSeconds, maxRedirects, configuration, context.CancellationToken).ConfigureAwait(false);
                processedCount++;
                processedUserIds.Add(userId);
                
                if (maxBytesPerUser > 0 && episode.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded)
                {
                    userUsageCache[userId] += episode.LocalFileSize ?? 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[{JobName}] Error downloading episode [{EpisodeId}]", nameof(PodcastDownloadJob), episode.Id);
                episode.DownloadStatus = PodcastEpisodeDownloadStatus.Failed;
                episode.DownloadError = ex.Message;
                await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task DownloadEpisodeAsync(PodcastEpisode episode, Library podcastLibrary, long maxEnclosureBytes, int timeoutSeconds, int maxRedirects, IMelodeeConfiguration configuration, CancellationToken cancellationToken)
    {
        Logger.Information("[{JobName}] Downloading episode [{EpisodeId}] {EpisodeTitle}", nameof(PodcastDownloadJob), episode.Id, episode.Title);

        episode.DownloadStatus = PodcastEpisodeDownloadStatus.Downloading;

        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        var redirectCount = 0;
        var currentUrl = episode.EnclosureUrl;
        HttpResponseMessage? response = null;

        while (redirectCount <= maxRedirects)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, currentUrl);
            response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            var isRedirect = response.StatusCode == System.Net.HttpStatusCode.MovedPermanently ||
                             response.StatusCode == System.Net.HttpStatusCode.Found ||
                             response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                             response.StatusCode == System.Net.HttpStatusCode.RedirectKeepVerb ||
                             response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect ||
                             response.StatusCode == System.Net.HttpStatusCode.PermanentRedirect;

            if (isRedirect)
            {
                var redirectUrl = response.Headers.Location;
                if (redirectUrl == null)
                {
                    throw new Exception("Redirect without location header");
                }

                currentUrl = redirectUrl.ToString();
                redirectCount++;
                continue;
            }

            break;
        }

        if (response == null || !response.IsSuccessStatusCode)
        {
            throw new Exception($"HTTP {(int)(response?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError)}: {response?.ReasonPhrase}");
        }

        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength.HasValue && contentLength.Value > maxEnclosureBytes)
        {
            throw new Exception($"File size {contentLength.Value} exceeds maximum allowed {maxEnclosureBytes}");
        }

        var mimeType = response.Content.Headers.ContentType?.MediaType ?? episode.MimeType ?? "application/octet-stream";
        var extension = GetExtensionFromMimeType(mimeType);

        var fileName = $"{episode.Id}{extension}";
        var userDirectory = Path.Combine(podcastLibrary.Path, episode.PodcastChannel.UserId.ToString());
        var channelDirectory = Path.Combine(userDirectory, episode.PodcastChannelId.ToString());
        Directory.CreateDirectory(channelDirectory);

        var tempFilePath = Path.Combine(channelDirectory, $".{fileName}.tmp");
        var finalFilePath = Path.Combine(channelDirectory, fileName);

        try
        {
            await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }

            var downloadedBytes = new FileInfo(tempFilePath).Length;
            if (contentLength.HasValue && downloadedBytes != contentLength.Value)
            {
                throw new Exception($"Downloaded {downloadedBytes} bytes but expected {contentLength.Value}");
            }

            File.Move(tempFilePath, finalFilePath, overwrite: true);

            episode.LocalPath = Path.Combine(episode.PodcastChannel.UserId.ToString(), episode.PodcastChannelId.ToString(), fileName);
            episode.LocalFileSize = downloadedBytes;
            episode.MimeType = mimeType;
            episode.DownloadStatus = PodcastEpisodeDownloadStatus.Downloaded;
            episode.DownloadError = null;

            Logger.Information("[{JobName}] Successfully downloaded episode [{EpisodeId}] to {FilePath}", nameof(PodcastDownloadJob), episode.Id, episode.LocalPath);
        }
        catch (Exception ex)
        {
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch
                {
                    Logger.Warning("[{JobName}] Failed to delete temp file {TempPath}", nameof(PodcastDownloadJob), tempFilePath);
                }
            }

            episode.DownloadStatus = PodcastEpisodeDownloadStatus.Failed;
            episode.DownloadError = $"Download failed: {ex.Message}";
            throw;
        }
        finally
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static string GetExtensionFromMimeType(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            "audio/mpeg" => ".mp3",
            "audio/mp3" => ".mp3",
            "audio/x-m4a" => ".m4a",
            "audio/mp4" => ".m4a",
            "audio/aac" => ".aac",
            "audio/ogg" => ".ogg",
            "audio/flac" => ".flac",
            "audio/wav" => ".wav",
            "audio/x-wav" => ".wav",
            "video/mp4" => ".mp4",
            "video/mpeg" => ".mpg",
            "video/quicktime" => ".mov",
            _ => ".mp3"
        };
    }
}
