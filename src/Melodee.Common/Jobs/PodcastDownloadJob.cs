using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
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
    LibraryService libraryService,
    PodcastHttpClient podcastHttpClient) : JobBase(logger, configurationFactory)
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
        var maxRedirects = configuration.GetValue<int>(SettingRegistry.PodcastHttpMaxRedirects, v => v > 0 ? v : SettingDefaults.PodcastHttpMaxRedirects);

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

            var userId = episode.PodcastChannel?.UserId;

            if (userId == null || processedUserIds.Contains(userId.Value))
            {
                Logger.Debug("[{JobName}] Per-user limit reached for user {UserId}, skipping.", nameof(PodcastDownloadJob), userId);
                continue;
            }

            if (maxBytesPerUser > 0)
            {
                long currentUsage = 0;
                if (userId.HasValue && !userUsageCache.TryGetValue(userId.Value, out currentUsage))
                {
                    currentUsage = await dbContext.PodcastEpisodes
                        .Where(x => x.PodcastChannel != null && x.PodcastChannel.UserId == userId.Value && x.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded)
                        .SumAsync(x => x.LocalFileSize ?? 0, context.CancellationToken)
                        .ConfigureAwait(false);
                    userUsageCache[userId.Value] = currentUsage;
                }

                if (currentUsage >= maxBytesPerUser)
                {
                    Logger.Warning("[{JobName}] User {UserId} has reached their storage quota of {Quota} bytes.", nameof(PodcastDownloadJob), userId.Value, maxBytesPerUser);
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
                if (userId.HasValue)
                {
                    processedUserIds.Add(userId.Value);
                }

                if (maxBytesPerUser > 0 && episode.DownloadStatus == PodcastEpisodeDownloadStatus.Downloaded)
                {
                    if (userId.HasValue)
                    {
                        userUsageCache[userId.Value] += episode.LocalFileSize ?? 0;
                    }
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
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var mimeType = episode.MimeType ?? "application/octet-stream";
        var extension = GetExtensionFromMimeType(mimeType);

        var fileName = $"{episode.Id}{extension}";
        var userIdString = episode.PodcastChannel?.UserId.ToString() ?? string.Empty;
        var userDirectory = Path.Combine(podcastLibrary.Path, userIdString);
        var channelDirectory = Path.Combine(userDirectory, episode.PodcastChannelId.ToString());
        Directory.CreateDirectory(channelDirectory);

        var tempFilePath = Path.Combine(channelDirectory, $".{fileName}.tmp");
        var finalFilePath = Path.Combine(channelDirectory, fileName);

        try
        {
            await using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

            var downloadResult = await podcastHttpClient.DownloadToStreamAsync(
                episode.EnclosureUrl,
                fileStream,
                maxEnclosureBytes,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!downloadResult.IsSuccess)
            {
                throw new Exception(downloadResult.ErrorMessage ?? "Download failed");
            }

            await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            fileStream.Close();

            var downloadedBytes = new FileInfo(tempFilePath).Length;

            File.Move(tempFilePath, finalFilePath, overwrite: true);

            // Update MIME type if the server provided one
            if (!string.IsNullOrEmpty(downloadResult.ContentType))
            {
                mimeType = downloadResult.ContentType;
                extension = GetExtensionFromMimeType(mimeType);

                // If extension changed, rename the file
                var newFileName = $"{episode.Id}{extension}";
                if (newFileName != fileName)
                {
                    var newFinalFilePath = Path.Combine(channelDirectory, newFileName);
                    File.Move(finalFilePath, newFinalFilePath, overwrite: true);
                    fileName = newFileName;
                    finalFilePath = newFinalFilePath;
                }
            }

            episode.LocalPath = Path.Combine(episode.PodcastChannel?.UserId.ToString() ?? string.Empty, episode.PodcastChannelId.ToString(), fileName);
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
