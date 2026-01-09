using System.Net.Http.Headers;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Utility;
using Polly;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.Discogs;

/// <summary>
///     Discogs artist search engine plugin.
/// </summary>
public class DiscogsArtistSearchEnginePlugin(
    ILogger logger,
    IMelodeeConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ICacheManager cacheManager)
    : IArtistSearchEnginePlugin
{
    private const int MaxConcurrency = 2;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(2);
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
    private readonly SemaphoreSlim _concurrencyLimiter = new(MaxConcurrency, MaxConcurrency);
    private readonly ResiliencePipeline<HttpResponseMessage> _retryPipeline = SearchEnginePolicies.CreateHttpRetryPipeline();

    public string Id => "5A8D2E5B-3C9D-4E7F-9B7A-A1B2C3D4E5F6";

    public string DisplayName => "Discogs Search Engine";

    public bool IsEnabled { get; set; }

    public int SortOrder { get; } = 4;

    public bool StopProcessing { get; } = false;

    public async Task<PagedResult<ArtistSearchResult>> DoArtistSearchAsync(ArtistQuery query, int maxResults,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Name))
        {
            return new PagedResult<ArtistSearchResult>(["Query Name value is invalid."]) { Data = [] };
        }

        var normalizedName = query.NameNormalized;
        var cacheKey = $"discogs:artist:{normalizedName}:{maxResults}";

        return await cacheManager.GetAsync(cacheKey, async () =>
        {
            await _concurrencyLimiter.WaitAsync(cancellationToken);
            try
            {
                var results = new List<ArtistSearchResult>();
                var userToken = configuration.GetValue<string>(SettingRegistry.SearchEngineDiscogsUserToken);
                var userAgent = configuration.GetValue<string>(SettingRegistry.SearchEngineUserAgent);

                try
                {
                    var httpClient = httpClientFactory.CreateClient();
                    httpClient.Timeout = RequestTimeout;
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

                    if (!string.IsNullOrWhiteSpace(userToken))
                    {
                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Discogs", $"token={userToken}");
                    }

                    var searchTerm = Uri.EscapeDataString(query.Name.Trim());
                    var requestUri =
                        $"https://api.discogs.com/database/search?q={searchTerm}&type=artist&per_page={maxResults}";

                    var response = await _retryPipeline.ExecuteAsync(
                        async ct => await httpClient.GetAsync(requestUri, ct),
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        logger.Warning("[{Plugin}] Discogs search failed with status {StatusCode}",
                            DisplayName, response.StatusCode);
                        return new PagedResult<ArtistSearchResult>(
                            [$"Discogs search failed with status {response.StatusCode}"])
                        {
                            Data = []
                        };
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                    var searchResult = serializer.Deserialize<DiscogsSearchResult>(jsonResponse);

                    if (searchResult?.Results?.Any() ?? false)
                    {
                        foreach (var discogsResult in searchResult.Results.Take(maxResults))
                        {
                            if (discogsResult.Title.Nullify() == null)
                            {
                                continue;
                            }

                            var isExact = discogsResult.Title.ToNormalizedString() == normalizedName;
                            var rank = isExact ? 10 : 1;

                            results.Add(new ArtistSearchResult
                            {
                                Name = discogsResult.Title!,
                                SortName = discogsResult.Title.ToNormalizedString(),
                                FromPlugin = DisplayName,
                                UniqueId = SafeParser.Hash($"{discogsResult.Id}:{discogsResult.Title}"),
                                Rank = rank,
                                DiscogsId = discogsResult.Id.ToString(),
                                ThumbnailUrl = discogsResult.Thumb.Nullify(),
                                ImageUrl = discogsResult.CoverImage.Nullify()
                            });
                        }

                        if (results.Count > 0)
                        {
                            logger.Debug("[{Plugin}] found [{Count}] artists for [{Query}]",
                                DisplayName, results.Count, LogSanitizer.Sanitize(query.Name));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error searching Discogs for artist [{Query}]", query.Name);
                    return new PagedResult<ArtistSearchResult>(["Discogs search failed."]) { Data = [] };
                }

                var ordered = results.OrderByDescending(x => x.Rank).ThenBy(x => x.SortName ?? x.Name)
                    .Take(maxResults).ToArray();

                return new PagedResult<ArtistSearchResult>
                {
                    Data = ordered,
                    TotalCount = ordered.Length,
                    TotalPages = 1,
                    CurrentPage = 1
                };
            }
            finally
            {
                _concurrencyLimiter.Release();
            }
        }, cancellationToken, CacheDuration, ServiceBase.CacheName);
    }

    private ISerializer serializer => new Serialization.Serializer(logger);
}
