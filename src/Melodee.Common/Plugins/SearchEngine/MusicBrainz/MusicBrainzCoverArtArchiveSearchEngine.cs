using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz;

/// <summary>
///     https://musicbrainz.org/doc/Cover_Art_Archive/API
/// </summary>
public sealed class MusicBrainzCoverArtArchiveSearchEngine(
    IMelodeeConfiguration configuration,
    IMusicBrainzRepository repository) : IAlbumImageSearchEnginePlugin
{
    public bool StopProcessing { get; private set; }

    public string Id => "3E6C2DD3-AC1A-452D-B52B-4C292BA1CC49";

    public string DisplayName => nameof(MusicBrainzCoverArtArchiveSearchEngine);

    public bool IsEnabled { get; set; }

    public int SortOrder { get; } = 0;

    public async Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(AlbumQuery query, int maxResults,
        CancellationToken cancellationToken = default)
    {
        StopProcessing = false;

        var result = new List<ImageSearchResult>();

        if (!configuration.GetValue<bool>(SettingRegistry.SearchEngineMusicBrainzEnabled))
        {
            Log.Debug("[{PluginName}] MusicBrainz is disabled, skipping cover art search",
                DisplayName);
            return new OperationResult<ImageSearchResult[]?>(
                "MusicBrainz Cover Art Archive image search plugin is disabled.")
            {
                Data = null
            };
        }

        try
        {
            Log.Debug("[{PluginName}] Searching for album cover: Artist=[{Artist}], Album=[{Album}], Year=[{Year}], ArtistMusicBrainzId=[{ArtistMbId}], AlbumMusicBrainzId=[{AlbumMbId}]",
                DisplayName, query?.Artist, query?.Name, query?.Year, query?.ArtistMusicBrainzId, query?.MusicBrainzIdValue);
            
            var artistSearchResult = await repository.SearchArtist(new ArtistQuery
            {
                MusicBrainzId = query?.ArtistMusicBrainzId,
                Name = query?.Artist ?? string.Empty,
                AlbumMusicBrainzIds = query?.MusicBrainzIdValue == null ? null : [query.MusicBrainzIdValue.Value],
                AlbumKeyValues =
                [
                    new KeyValue(query?.Year.ToString() ?? string.Empty, query?.NameNormalized)
                ]
            }, 1, cancellationToken).ConfigureAwait(false);
            
            if (artistSearchResult.IsSuccess)
            {
                var artist = artistSearchResult.Data.FirstOrDefault();
                var rg = artist?.Releases?.FirstOrDefault()?.MusicBrainzResourceGroupId;
                
                Log.Debug("[{PluginName}] Artist search returned: Found=[{Found}], ArtistName=[{ArtistName}], ReleaseCount=[{ReleaseCount}], ReleaseGroupId=[{ReleaseGroupId}]",
                    DisplayName, artist != null, artist?.Name, artist?.Releases?.Length ?? 0, rg);
                
                if (rg != null)
                {
                    result.Add(new ImageSearchResult
                    {
                        FromPlugin = DisplayName,
                        Rank = 10,
                        ThumbnailUrl = string.Empty,
                        MediaUrl = $"https://coverartarchive.org/release-group/{rg}/front"
                    });
                    
                    Log.Debug("[{PluginName}] FOUND cover art URL for album [{Album}]: {Url}",
                        DisplayName, query?.Name, result.First().MediaUrl);
                }
            }
            else
            {
                Log.Debug("[{PluginName}] Artist search failed for album [{Album}]",
                    DisplayName, query?.Name);
            }
        }
        catch (Exception e)
        {
            Log.Error(e,
                "[{PluginName}] attempting to query cover art archive image search plugin failed Query [{Query}]",
                nameof(MusicBrainzArtistSearchEnginePlugin), query);
        }

        if (result.Count == 0)
        {
            Log.Debug("[{PluginName}] NO cover art found for album [{Album}] by [{Artist}] ({Year})",
                DisplayName, query?.Name, query?.Artist, query?.Year);
        }

        return new OperationResult<ImageSearchResult[]?>
        {
            Data = result.ToArray()
        };
    }
}
