using System.Net;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Services.Security;
using Polly;
using Polly.Retry;
using Serilog;

namespace Melodee.Common.Services;

/// <summary>
/// Provides resilient HTTP client functionality for podcast operations.
/// Implements Polly retry policies and SSRF protection.
/// </summary>
public sealed class PodcastHttpClient : IDisposable
{
    private readonly ILogger _logger;
    private readonly ISsrfValidator _ssrfValidator;
    private readonly IMelodeeConfigurationFactory _configurationFactory;
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline<HttpResponseMessage> _retryPipeline;
    private bool _disposed;

    public PodcastHttpClient(
        ILogger logger,
        ISsrfValidator ssrfValidator,
        IMelodeeConfigurationFactory configurationFactory)
    {
        _logger = logger;
        _ssrfValidator = ssrfValidator;
        _configurationFactory = configurationFactory;

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false // We handle redirects manually for SSRF validation
        };

        _httpClient = new HttpClient(handler);
        _retryPipeline = CreateRetryPipeline();
    }

    /// <summary>
    /// Sends an HTTP GET request with SSRF validation and retry policies.
    /// </summary>
    public async Task<PodcastHttpResult> GetAsync(
        string url,
        Dictionary<string, string>? headers = null,
        long? maxResponseBytes = null,
        CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var timeoutSeconds = configuration.GetValue<int>(SettingRegistry.PodcastHttpTimeoutSeconds);
        var maxRedirects = configuration.GetValue<int>(SettingRegistry.PodcastHttpMaxRedirects, v => v > 0 ? v : SettingDefaults.PodcastHttpMaxRedirects);

        // Use CancellationTokenSource with timeout instead of setting HttpClient.Timeout
        // to avoid "Properties can only be modified before sending the first request" error
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var validationResult = await _ssrfValidator.ValidateUrlAsync(url, timeoutCts.Token).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return PodcastHttpResult.Failed(validationResult.ErrorMessage ?? "SSRF validation failed");
        }

        var currentUrl = url;
        var redirectCount = 0;

        while (true)
        {
            try
            {
                var response = await _retryPipeline.ExecuteAsync(async ct =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, currentUrl);

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }

                    return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                }, timeoutCts.Token).ConfigureAwait(false);

                if (IsRedirect(response.StatusCode))
                {
                    var redirectLocation = response.Headers.Location?.ToString();
                    if (string.IsNullOrEmpty(redirectLocation))
                    {
                        return PodcastHttpResult.Failed("Redirect without Location header");
                    }

                    _logger.Debug("[PodcastHttpClient] Redirect {Count}/{Max}: {From} -> {To}",
                        redirectCount + 1, maxRedirects, currentUrl, redirectLocation);

                    var redirectValidation = await _ssrfValidator.ValidateRedirectAsync(
                        currentUrl,
                        redirectLocation,
                        redirectCount,
                        maxRedirects,
                        timeoutCts.Token).ConfigureAwait(false);

                    if (!redirectValidation.IsValid)
                    {
                        _logger.Warning("[PodcastHttpClient] Redirect validation failed after {Count} redirects: {Error}",
                            redirectCount, redirectValidation.ErrorMessage);
                        return PodcastHttpResult.Failed(redirectValidation.ErrorMessage ?? "Redirect validation failed");
                    }

                    currentUrl = new Uri(new Uri(currentUrl), redirectLocation).ToString();
                    redirectCount++;
                    response.Dispose();
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                    response.Dispose();
                    return PodcastHttpResult.Failed(errorMessage);
                }

                if (maxResponseBytes.HasValue)
                {
                    var contentLength = response.Content.Headers.ContentLength;
                    if (contentLength.HasValue && contentLength.Value > maxResponseBytes.Value)
                    {
                        response.Dispose();
                        return PodcastHttpResult.Failed($"Response size {contentLength.Value} exceeds maximum {maxResponseBytes.Value}");
                    }
                }

                return PodcastHttpResult.Success(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.Warning(ex, "[PodcastHttpClient] Request failed for {Url}", currentUrl);
                return PodcastHttpResult.Failed($"Request failed: {ex.Message}");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.Warning("[PodcastHttpClient] Request timed out for {Url}", currentUrl);
                return PodcastHttpResult.Failed("Request timed out");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[PodcastHttpClient] Unexpected error for {Url}", currentUrl);
                return PodcastHttpResult.Failed($"Unexpected error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Downloads content to a stream with size validation.
    /// </summary>
    public async Task<PodcastDownloadResult> DownloadToStreamAsync(
        string url,
        Stream destination,
        long maxBytes,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync(url, maxResponseBytes: maxBytes, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || result.Response == null)
        {
            return new PodcastDownloadResult
            {
                IsSuccess = false,
                ErrorMessage = result.ErrorMessage
            };
        }

        using var response = result.Response;

        try
        {
            var contentType = response.Content.Headers.ContentType?.MediaType;
            var contentLength = response.Content.Headers.ContentLength;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            var buffer = new byte[81920];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead > maxBytes)
                {
                    return new PodcastDownloadResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Download exceeded maximum size of {maxBytes} bytes"
                    };
                }

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                progress?.Report(totalBytesRead);
            }

            return new PodcastDownloadResult
            {
                IsSuccess = true,
                BytesDownloaded = totalBytesRead,
                ContentType = contentType,
                ContentLength = contentLength
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[PodcastHttpClient] Download failed for {Url}", url);
            return new PodcastDownloadResult
            {
                IsSuccess = false,
                ErrorMessage = $"Download failed: {ex.Message}"
            };
        }
    }

    private static bool IsRedirect(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.MovedPermanently ||
               statusCode == HttpStatusCode.Found ||
               statusCode == HttpStatusCode.SeeOther ||
               statusCode == HttpStatusCode.TemporaryRedirect ||
               statusCode == HttpStatusCode.PermanentRedirect;
    }

    private ResiliencePipeline<HttpResponseMessage> CreateRetryPipeline()
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .HandleResult(response =>
                    {
                        var statusCode = (int)response.StatusCode;
                        return statusCode >= 500 || statusCode == 429;
                    }),
                OnRetry = args =>
                {
                    var statusCode = args.Outcome.Result != null ? (int)args.Outcome.Result.StatusCode : 0;
                    _logger.Debug("[PodcastHttpClient] Retry {Attempt} of 3 ({Reason})",
                        args.AttemptNumber,
                        args.Outcome.Exception?.Message ?? $"HTTP {statusCode}");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _httpClient.Dispose();
        _disposed = true;
    }
}

/// <summary>
/// Result of an HTTP request.
/// </summary>
public sealed class PodcastHttpResult : IDisposable
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public HttpResponseMessage? Response { get; init; }

    public static PodcastHttpResult Success(HttpResponseMessage response) => new()
    {
        IsSuccess = true,
        Response = response
    };

    public static PodcastHttpResult Failed(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };

    public void Dispose()
    {
        Response?.Dispose();
    }
}

/// <summary>
/// Result of a download operation.
/// </summary>
public sealed record PodcastDownloadResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public long BytesDownloaded { get; init; }
    public string? ContentType { get; init; }
    public long? ContentLength { get; init; }
}
