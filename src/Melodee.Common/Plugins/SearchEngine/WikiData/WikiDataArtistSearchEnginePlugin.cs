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

namespace Melodee.Common.Plugins.SearchEngine.WikiData;

/// <summary>
///     WikiData artist search engine plugin using SPARQL queries.
/// </summary>
public class WikiDataArtistSearchEnginePlugin(
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

    public string Id => "7B9C8E1F-2A3D-4E5B-8C7D-9E0F1A2B3C4D";

    public string DisplayName => "WikiData Search Engine";

    public bool IsEnabled { get; set; }

    public int SortOrder { get; } = 5;

    public bool StopProcessing { get; } = false;

    public async Task<PagedResult<ArtistSearchResult>> DoArtistSearchAsync(ArtistQuery query, int maxResults,
        CancellationToken cancellationToken = default)
    {
        var normalizedInput = SearchEngineQueryNormalization.NormalizeQuery(query.Name);
        if (normalizedInput == null)
        {
            return new PagedResult<ArtistSearchResult>(["Query Name value is invalid."]) { Data = [] };
        }

        var cacheKey = $"wikidata:artist:{normalizedInput}:{maxResults}";

        return await cacheManager.GetAsync(cacheKey, async () =>
        {
            await _concurrencyLimiter.WaitAsync(cancellationToken);
            try
            {
                var results = new List<ArtistSearchResult>();
                var userAgent = configuration.GetValue<string>(SettingRegistry.SearchEngineUserAgent);

                try
                {
                    var httpClient = httpClientFactory.CreateClient();
                    httpClient.Timeout = RequestTimeout;
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

                    var safeSearchTerm = EscapeForSparql(normalizedInput);
                    var sparql = $@"
                        SELECT ?item ?itemLabel ?description ?image WHERE {{
                            ?item wdt:P31 wd:Q5;
                                  rdfs:label ""{safeSearchTerm}""@en;
                                  schema:description ?description.
                            OPTIONAL {{ ?item wdt:P18 ?image. }}
                            FILTER(LANG(?description) = 'en')
                        }} LIMIT {maxResults}";

                    var encodedSparql = Uri.EscapeDataString(sparql);
                    var requestUri = $"https://query.wikidata.org/sparql?format=json&query={encodedSparql}";

                    var response = await _retryPipeline.ExecuteAsync(
                        async ct => await httpClient.GetAsync(requestUri, ct),
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        logger.Warning("[{Plugin}] WikiData search failed with status {StatusCode}",
                            DisplayName, response.StatusCode);
                        return new PagedResult<ArtistSearchResult>(
                            [$"WikiData search failed with status {response.StatusCode}"])
                        {
                            Data = []
                        };
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                    var searchResult = serializer.Deserialize<WikiDataSparqlResult>(jsonResponse);

                    if (searchResult?.Results?.Bindings?.Any() ?? false)
                    {
                        foreach (var binding in searchResult.Results.Bindings)
                        {
                            var itemId = ExtractWikiDataId(binding.Item?.Value);
                            if (itemId == null)
                            {
                                continue;
                            }

                            var name = binding.ItemLabel?.Value ?? normalizedInput;
                            var isExact = name.ToNormalizedString() == normalizedInput;
                            var rank = isExact ? 10 : 1;

                            results.Add(new ArtistSearchResult
                            {
                                Name = name,
                                SortName = name.ToNormalizedString(),
                                FromPlugin = DisplayName,
                                UniqueId = SafeParser.Hash($"{itemId}:{name}"),
                                Rank = rank,
                                WikiDataId = itemId,
                                ImageUrl = binding.Image?.Value,
                                ThumbnailUrl = binding.Image?.Value
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
                    logger.Error(ex, "Error searching WikiData for artist [{Query}]", query.Name);
                    return new PagedResult<ArtistSearchResult>(["WikiData search failed."]) { Data = [] };
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

    private static string EscapeForSparql(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", " ")
            .Replace("\r", " ");
    }

    private static string? ExtractWikiDataId(string? uri)
    {
        if (uri == null)
        {
            return null;
        }

        var lastSlash = uri.LastIndexOf('/');
        return lastSlash >= 0 ? uri[(lastSlash + 1)..] : uri;
    }

    private ISerializer serializer => new Serialization.Serializer(logger);
}
