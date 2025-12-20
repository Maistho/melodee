using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine;
using Melodee.Common.Plugins.SearchEngine.Brave;
using Melodee.Common.Plugins.SearchEngine.Deezer;
using Melodee.Common.Plugins.SearchEngine.ITunes;
using Melodee.Common.Plugins.SearchEngine.LastFm;
using Melodee.Common.Plugins.SearchEngine.MetalApi;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Plugins.SearchEngine.Spotify;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Common.Services.SearchEngines;

/// <summary>
///     Uses enabled Image Search plugins to get images for album query.
/// </summary>
public class AlbumImageSearchEngineService(
    ILogger logger,
    ICacheManager cacheManager,
    ISerializer serializer,
    SettingService settingService,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMusicBrainzRepository musicBrainzRepository,
    ISpotifyClientBuilder spotifyClientBuilder,
    IHttpClientFactory httpClientFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<OperationResult<ImageSearchResult[]>> DoSearchAsync(AlbumQuery query, int? maxResults,
        CancellationToken token = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(token);

        var maxResultsValue = maxResults ?? configuration.GetValue<int>(SettingRegistry.SearchEngineDefaultPageSize);

        var searchEngines = new List<IAlbumImageSearchEnginePlugin>
        {
            new MusicBrainzCoverArtArchiveSearchEngine(configuration, musicBrainzRepository)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineMusicBrainzEnabled)
            },
            new DeezerSearchEngine(Logger, serializer, httpClientFactory)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineDeezerEnabled)
            },
            new ITunesSearchEngine(Logger, serializer, httpClientFactory, CacheManager)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineITunesEnabled)
            },
            new Spotify(Logger, configuration, CacheManager, spotifyClientBuilder, settingService, ContextFactory)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineSpotifyEnabled)
            },
            new LastFm(Logger, configuration, serializer, httpClientFactory, CacheManager)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineLastFmEnabled)
            },
            new MetalApiAlbumImageSearchEngine(
                new MetalApiClient(
                    httpClientFactory.CreateClient(),
                    Logger,
                    new MetalApiOptions { Enabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineMetalApiEnabled) }),
                Logger,
                new MetalApiOptions { Enabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineMetalApiEnabled) })
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineMetalApiEnabled)
            },
            new BraveAlbumImageSearchEnginePlugin(Logger, httpClientFactory, configuration)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineBraveEnabled)
            }
        };
        var result = new List<ImageSearchResult>();
        var enabledEngines = searchEngines.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder).ToArray();

        Logger.Debug("Starting album image search for query [{Query}] with [{Count}] enabled search engines: [{Engines}]",
            query, enabledEngines.Length, string.Join(", ", enabledEngines.Select(x => x.DisplayName)));

        foreach (var searchEngine in enabledEngines)
        {
            try
            {
                Logger.Debug("[{Plugin}] searching for album images with query [{Query}]", searchEngine.DisplayName, query);
                var searchResult = await searchEngine.DoAlbumImageSearch(query, maxResultsValue, token);
                if (searchResult.IsSuccess)
                {
                    var foundCount = searchResult.Data?.Length ?? 0;
                    if (foundCount > 0)
                    {
                        Logger.Debug("[{Plugin}] found [{Count}] image(s) for query [{Query}]",
                            searchEngine.DisplayName, foundCount, query);
                        result.AddRange(searchResult.Data ?? []);
                    }
                    else
                    {
                        Logger.Debug("[{Plugin}] found no images for query [{Query}]", searchEngine.DisplayName, query);
                    }
                }
                else
                {
                    Logger.Warning("[{Plugin}] search failed for query [{Query}]: [{Errors}]",
                        searchEngine.DisplayName, query, string.Join(", ", searchResult.Errors ?? []));
                }

                if (searchEngine.StopProcessing || result.Count >= maxResultsValue)
                {
                    Logger.Debug("[{Plugin}] stopping search - StopProcessing: {Stop}, ResultCount: {Count}/{Max}",
                        searchEngine.DisplayName, searchEngine.StopProcessing, result.Count, maxResultsValue);
                    break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "[{Plugin}] threw error with query [{Query}]", searchEngine.DisplayName, query);
            }
        }

        Logger.Debug("Album image search completed for query [{Query}] with [{Count}] total result(s)", query, result.Count);

        return new OperationResult<ImageSearchResult[]>
        {
            Data = result.OrderByDescending(x => x.Rank).ToArray()
        };
    }
}
