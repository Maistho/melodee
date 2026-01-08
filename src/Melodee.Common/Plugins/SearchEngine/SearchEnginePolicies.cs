using System.Net;
using System.Net.Http;
using Polly;
using Polly.Retry;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine;

/// <summary>
///     Provides shared Polly retry policies for search engine plugins.
/// </summary>
public static class SearchEnginePolicies
{
    /// <summary>
    ///     Creates a retry pipeline for HTTP requests implementing the standard search engine defaults.
    ///     - Retries network exceptions, HTTP 5xx, and HTTP 429 (preferring Retry-After header)
    ///     - Does not retry other 4xx errors
    ///     - Uses exponential backoff with 3 retry attempts
    /// </summary>
    /// <param name="logger">Optional logger for policy events</param>
    /// <param name="maxRetryAttempts">Number of retry attempts (default: 3)</param>
    /// <param name="baseDelaySeconds">Base delay in seconds for exponential backoff (default: 2)</param>
    /// <returns>An async retry strategy ready to use</returns>
    public static ResiliencePipeline<HttpResponseMessage> CreateHttpRetryPipeline(
        ILogger? logger = null,
        int maxRetryAttempts = 3,
        int baseDelaySeconds = 2)
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = maxRetryAttempts,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(baseDelaySeconds),
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>() // Timeout exceptions
                    .HandleResult(response =>
                    {
                        var statusCode = (int)response.StatusCode;
                        // Retry on 5xx errors or 429 (Too Many Requests)
                        return statusCode >= 500 || statusCode == 429;
                    }),
                OnRetry = args =>
                {
                    if (args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests &&
                        args.Outcome.Result.Headers.RetryAfter is { } retryAfter)
                    {
                        logger?.Debug("[SearchEnginePolicies] Retry {Attempt} of {Max} after rate limit (Retry-After: {Delay})",
                            args.AttemptNumber, maxRetryAttempts, retryAfter.Delta);
                        return ValueTask.CompletedTask;
                    }

                    var calculatedDelay = TimeSpan.FromSeconds(Math.Pow(baseDelaySeconds, args.AttemptNumber));
                    var statusCode = args.Outcome.Result != null ? (int)args.Outcome.Result.StatusCode : 0;
                    logger?.Debug("[SearchEnginePolicies] Retry {Attempt} of {Max} after {Delay}ms ({Reason})",
                        args.AttemptNumber, maxRetryAttempts, calculatedDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? $"HTTP {statusCode}");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }
}
