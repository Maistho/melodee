using System.Diagnostics;
using Polly;
using Polly.Retry;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Common.Extensions;

/// <summary>
///     Progress information for a file download operation.
/// </summary>
public record DownloadProgress(
    long BytesDownloaded,
    long? TotalBytes,
    double? BytesPerSecond = null,
    TimeSpan? ElapsedTime = null)
{
    /// <summary>
    ///     Percentage complete (0-100), or null if total size is unknown.
    /// </summary>
    public double? PercentComplete => TotalBytes > 0 ? (double)BytesDownloaded / TotalBytes * 100 : null;

    /// <summary>
    ///     Human-readable downloaded size (e.g., "1.5 GB").
    /// </summary>
    public string BytesDownloadedFormatted => FormatBytes(BytesDownloaded);

    /// <summary>
    ///     Human-readable total size (e.g., "3.2 GB"), or "Unknown" if not available.
    /// </summary>
    public string TotalBytesFormatted => TotalBytes.HasValue ? FormatBytes(TotalBytes.Value) : "Unknown";

    /// <summary>
    ///     Human-readable download speed (e.g., "15.2 MB/s"), or null if not available.
    /// </summary>
    public string? SpeedFormatted => BytesPerSecond.HasValue ? $"{FormatBytes((long)BytesPerSecond.Value)}/s" : null;

    /// <summary>
    ///     Estimated time remaining, or null if not calculable.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining
    {
        get
        {
            if (!TotalBytes.HasValue || !BytesPerSecond.HasValue || BytesPerSecond.Value <= 0)
                return null;
            var remainingBytes = TotalBytes.Value - BytesDownloaded;
            if (remainingBytes <= 0) return TimeSpan.Zero;
            return TimeSpan.FromSeconds(remainingBytes / BytesPerSecond.Value);
        }
    }

    /// <summary>
    ///     Human-readable ETA (e.g., "2m 30s"), or null if not calculable.
    /// </summary>
    public string? EstimatedTimeRemainingFormatted
    {
        get
        {
            var eta = EstimatedTimeRemaining;
            if (!eta.HasValue) return null;
            if (eta.Value.TotalHours >= 1)
                return $"{(int)eta.Value.TotalHours}h {eta.Value.Minutes}m";
            if (eta.Value.TotalMinutes >= 1)
                return $"{(int)eta.Value.TotalMinutes}m {eta.Value.Seconds}s";
            return $"{eta.Value.Seconds}s";
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        var i = 0;
        double size = bytes;
        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }
        return $"{size:F1} {suffixes[i]}";
    }
}

public static class HttpClientExtensions
{
    private const int DefaultBufferSize = 81920; // 80KB buffer for progress reporting

    /// <summary>
    ///     Download given url to a file and if a file exists with same name execute the condition.
    /// </summary>
    /// <returns>True if the file downloaded was kept, false if deleted as failed condition or errored.</returns>
    public static async Task<bool> DownloadFileAsync(this HttpClient httpClient, string url, string filePath,
        Func<FileInfo, FileInfo, CancellationToken, Task<bool>>? overrideCondition = null,
        CancellationToken cancellationToken = default)
    {
        return await DownloadFileAsync(httpClient, url, filePath, overrideCondition, null, cancellationToken);
    }

    /// <summary>
    ///     Download given url to a file with progress reporting.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use.</param>
    /// <param name="url">URL to download from.</param>
    /// <param name="filePath">Local file path to save to.</param>
    /// <param name="overrideCondition">Optional condition to check before overwriting existing file.</param>
    /// <param name="progressCallback">Optional callback invoked during download with progress info.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file downloaded was kept, false if deleted as failed condition or errored.</returns>
    public static async Task<bool> DownloadFileAsync(this HttpClient httpClient, string url, string filePath,
        Func<FileInfo, FileInfo, CancellationToken, Task<bool>>? overrideCondition,
        Action<DownloadProgress>? progressCallback,
        CancellationToken cancellationToken = default)
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromMinutes(3)
            })
            .Build();
        return await pipeline
            .ExecuteAsync(
                async _ =>
                    await DownloadFileActionAsync(httpClient, url, filePath, overrideCondition, progressCallback, cancellationToken),
                cancellationToken).ConfigureAwait(false);
    }

    private static async Task<bool> DownloadFileActionAsync(this HttpClient httpClient, string url, string filePath,
        Func<FileInfo, FileInfo, CancellationToken, Task<bool>>? overrideCondition,
        Action<DownloadProgress>? progressCallback,
        CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(filePath);
        var tempDownloadName = Path.Combine(fileInfo.DirectoryName!, $"{Guid.NewGuid()}{fileInfo.Extension}");

        try
        {
            using (Operation.At(LogEventLevel.Debug)
                       .Time("\u2584 Downloaded url [{Url}] to file [{File}]", url, filePath))
            {
                using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength;

                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var fileStream = new FileStream(tempDownloadName, FileMode.Create, FileAccess.Write, FileShare.None, DefaultBufferSize, true);

                if (progressCallback != null)
                {
                    var buffer = new byte[DefaultBufferSize];
                    long totalBytesRead = 0;
                    int bytesRead;
                    var downloadStartTime = Stopwatch.GetTimestamp();
                    var lastProgressReport = Stopwatch.GetTimestamp();
                    var lastBytesForSpeed = 0L;
                    var lastSpeedCalcTime = downloadStartTime;
                    double currentSpeed = 0;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                        totalBytesRead += bytesRead;

                        var now = Stopwatch.GetTimestamp();
                        var elapsedSinceLastReport = Stopwatch.GetElapsedTime(lastProgressReport);

                        // Report progress at most every 250ms to avoid excessive UI updates
                        if (elapsedSinceLastReport.TotalMilliseconds >= 250)
                        {
                            // Calculate speed over the last interval for smoother readings
                            var elapsedSinceLastSpeedCalc = Stopwatch.GetElapsedTime(lastSpeedCalcTime);
                            if (elapsedSinceLastSpeedCalc.TotalSeconds > 0)
                            {
                                var bytesSinceLastCalc = totalBytesRead - lastBytesForSpeed;
                                currentSpeed = bytesSinceLastCalc / elapsedSinceLastSpeedCalc.TotalSeconds;
                                lastBytesForSpeed = totalBytesRead;
                                lastSpeedCalcTime = now;
                            }

                            var totalElapsed = Stopwatch.GetElapsedTime(downloadStartTime);
                            progressCallback(new DownloadProgress(totalBytesRead, totalBytes, currentSpeed, totalElapsed));
                            lastProgressReport = now;
                        }
                    }

                    // Final progress report
                    var finalElapsed = Stopwatch.GetElapsedTime(downloadStartTime);
                    var avgSpeed = finalElapsed.TotalSeconds > 0 ? totalBytesRead / finalElapsed.TotalSeconds : 0;
                    progressCallback(new DownloadProgress(totalBytesRead, totalBytes, avgSpeed, finalElapsed));
                }
                else
                {
                    await contentStream.CopyToAsync(fileStream, cancellationToken);
                }
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Attempting to download [{url}] to file [{filePath}], [{e}]");
            return false;
        }

        if (fileInfo.Exists && overrideCondition == null)
        {
            fileInfo.Delete();
            File.Move(tempDownloadName, filePath);
        }

        if (fileInfo.Exists && overrideCondition != null &&
            await overrideCondition(fileInfo, new FileInfo(tempDownloadName), cancellationToken))
        {
            fileInfo.Delete();
            File.Move(tempDownloadName, filePath);
        }
        else if (fileInfo.Exists && overrideCondition != null &&
                 !await overrideCondition(fileInfo, new FileInfo(tempDownloadName), cancellationToken))
        {
            File.Delete(tempDownloadName);
            return false;
        }
        else
        {
            File.Move(tempDownloadName, filePath);
        }

        return true;
    }
}
