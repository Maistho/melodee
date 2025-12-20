using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Utility;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.LastFm;

public class LastFm(
    ILogger logger,
    IMelodeeConfiguration configuration,
    ISerializer serializer,
    IHttpClientFactory httpClientFactory,
    ICacheManager cacheManager)
    : IArtistSearchEnginePlugin, IArtistTopSongsSearchEnginePlugin, IAlbumImageSearchEnginePlugin
{
    public async Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(AlbumQuery query, int maxResults,
        CancellationToken cancellationToken = default)
    {
        //http://ws.audioscrobbler.com/2.0/?method=album.Search&api_key=<key>&format=json&album=Rising

        var apiKey = configuration.GetValue<string>(SettingRegistry.ScrobblingLastFmApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new OperationResult<ImageSearchResult[]?>("Last.fm API key not configured.")
            {
                Data = []
            };
        }

        var results = new List<ImageSearchResult>();

        if (string.IsNullOrWhiteSpace(query.Name))
        {
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = []
            };
        }

        var httpClient = httpClientFactory.CreateClient();
        var requestUri =
            $"https://ws.audioscrobbler.com/2.0/?method=album.Search&api_key={apiKey}&format=json&album={Uri.EscapeDataString(query.Name.Trim())}";

        try
        {
            var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new OperationResult<ImageSearchResult[]?>
                {
                    Data = []
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var searchResult = serializer.Deserialize<LastFmResult>(jsonResponse);
            if (searchResult?.Results?.Albummatches?.album?.Any() ?? false)
            {
                var na = query.Artist.ToNormalizedString();
                foreach (var sr in searchResult.Results?.Albummatches?.album?.Where(x => x.image != null) ?? [])
                {
                    if (sr.artist.ToNormalizedString() == na)
                    {
                        var tnUrl = sr.image?.FirstOrDefault(x => x.size == "large")?._text;
                        var mediaUrl = sr.image?.FirstOrDefault(x => x.size == "extralarge")?._text
                            ?.Replace("300x300", "600x600");
                        if (tnUrl != null && mediaUrl != null)
                        {
                            results.Add(new ImageSearchResult
                            {
                                FromPlugin = DisplayName,
                                Height = 600,
                                MusicBrainzId = SafeParser.ToGuid(sr.mbid),
                                MediaUrl = mediaUrl,
                                Rank = 10,
                                ThumbnailUrl = tnUrl,
                                Title = sr.name,
                                UniqueId = SafeParser.Hash(mediaUrl),
                                Width = 600
                            });
                        }
                    }
                }

                if (results.Count > 0)
                {
                    logger.Debug("[{DisplayName}] found [{ImageCount}] for Album [{Query}]",
                        DisplayName,
                        results.Count,
                        query.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error searching for album image url [{Url}] query [{Query}]", requestUri, query.Name);
        }

        return new OperationResult<ImageSearchResult[]?>
        {
            Data = results.OrderBy(x => x.Rank).ToArray()
        };
    }

    public bool StopProcessing { get; } = false;

    public string Id => "3E43D998-2E9C-45B0-8128-EE6F0A41419E";

    public string DisplayName => "Last FM Search Engine";

    public bool IsEnabled { get; set; } = false;

    public int SortOrder { get; } = 1;

    public async Task<PagedResult<ArtistSearchResult>> DoArtistSearchAsync(ArtistQuery query, int maxResults,
        CancellationToken cancellationToken = default)
    {
        var apiKey = configuration.GetValue<string>(SettingRegistry.ScrobblingLastFmApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new PagedResult<ArtistSearchResult>(["Last.fm API key not configured."])
            {
                Data = []
            };
        }

        if (string.IsNullOrWhiteSpace(query.Name))
        {
            return new PagedResult<ArtistSearchResult>(["Query Name value is invalid."]) { Data = [] };
        }

        var cacheKey = $"lastfm:artist:{query.NameNormalized}:{maxResults}";

        return await cacheManager.GetAsync(cacheKey, async () =>
        {
            var results = new List<ArtistSearchResult>();
            var httpClient = httpClientFactory.CreateClient();
            var requestUri =
                $"https://ws.audioscrobbler.com/2.0/?method=artist.search&artist={Uri.EscapeDataString(query.Name.Trim())}&api_key={apiKey}&format=json&limit={maxResults}";

            try
            {
                var response = await httpClient.GetAsync(requestUri, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new PagedResult<ArtistSearchResult>([$"Last.fm search failed with status {response.StatusCode}"])
                    {
                        Data = []
                    };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                var searchResult = serializer.Deserialize<LastFmArtistSearchResult>(jsonResponse);
                var artists = searchResult?.Results?.Artistmatches?.Artist ?? [];
                var normalizedQuery = query.NameNormalized;
                foreach (var artist in artists)
                {
                    if (artist.Name.Nullify() == null)
                    {
                        continue;
                    }

                    var isExact = artist.Name.ToNormalizedString() == normalizedQuery;
                    var rank = isExact ? 10 : 1;
                    var lastFmId = ExtractLastFmId(artist.Url);
                    results.Add(new ArtistSearchResult
                    {
                        Name = artist.Name!,
                        SortName = artist.Name.ToNormalizedString(),
                        FromPlugin = DisplayName,
                        UniqueId = SafeParser.Hash($"{artist.Mbid}:{artist.Name}"),
                        Rank = rank,
                        ImageUrl = artist.image?.FirstOrDefault(x => x.size == "extralarge")?._text ??
                                   artist.image?.FirstOrDefault(x => x.size == "large")?._text,
                        ThumbnailUrl = artist.image?.FirstOrDefault(x => x.size == "medium")?._text ??
                                       artist.image?.FirstOrDefault(x => x.size == "small")?._text,
                        MusicBrainzId = SafeParser.ToGuid(artist.Mbid),
                        LastFmId = lastFmId
                    });
                }

                if (results.Count > 0)
                {
                    logger.Debug("[{DisplayName}] found [{ImageCount}] for Artist [{Query}]",
                        DisplayName,
                        results.Count,
                        query.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error searching for artist url [{Url}] query [{Query}]", requestUri, query.Name);
                return new PagedResult<ArtistSearchResult>(["Last.fm search failed."]) { Data = [] };
            }

            var ordered = results.OrderByDescending(x => x.Rank).ThenBy(x => x.SortName ?? x.Name).Take(maxResults)
                .ToArray();

            return new PagedResult<ArtistSearchResult>
            {
                Data = ordered,
                TotalCount = ordered.Length,
                TotalPages = 1,
                CurrentPage = 1
            };
        }, cancellationToken, TimeSpan.FromHours(6), ServiceBase.CacheName);
    }


    public Task<PagedResult<SongSearchResult>> DoArtistTopSongsSearchAsync(int forArtist, int maxResults,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PagedResult<SongSearchResult>(["LastFm Not implemented"]) { Data = [] });
    }

    private static string? ExtractLastFmId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        try
        {
            var uri = new Uri(url);
            var segments = uri.Segments;
            if (segments.Length > 0)
            {
                return segments.Last().Trim('/').Nullify();
            }
        }
        catch
        {
            // ignore parsing errors
        }

        return null;
    }
}
