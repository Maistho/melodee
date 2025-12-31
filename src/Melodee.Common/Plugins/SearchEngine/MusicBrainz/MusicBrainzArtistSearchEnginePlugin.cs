using System.Diagnostics;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz;

public class MusicBrainzArtistSearchEnginePlugin(IMusicBrainzRepository repository) : IArtistSearchEnginePlugin
{
    public bool StopProcessing { get; } = false;

    public string Id => "018A798D-7B68-4F3E-80CD-1BAF03998C0B";

    public string DisplayName => "Music Brainz Database";

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 1;

    public async Task<PagedResult<ArtistSearchResult>> DoArtistSearchAsync(ArtistQuery query, int maxResults,
        CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();
        
        Log.Debug("[{PluginName}] Searching for artist: Name=[{Name}], NameNormalized=[{NameNormalized}], MusicBrainzId=[{MusicBrainzId}]",
            DisplayName, query.Name, query.NameNormalized, query.MusicBrainzId);
        
        var result = await repository.SearchArtist(query, maxResults, cancellationToken);
        
        var elapsedMs = Stopwatch.GetElapsedTime(startTicks).TotalMilliseconds;
        
        if (result.IsSuccess && result.Data.Any())
        {
            var topResult = result.Data.First();
            Log.Debug("[{PluginName}] FOUND artist [{ArtistName}] (MusicBrainzId={MusicBrainzId}, Albums={AlbumCount}, Rank={Rank}) in {ElapsedMs:F1}ms",
                DisplayName, topResult.Name, topResult.MusicBrainzId, topResult.AlbumCount, topResult.Rank, elapsedMs);
        }
        else
        {
            Log.Debug("[{PluginName}] NO MATCH for artist [{ArtistName}] (NameNormalized={NameNormalized}) in {ElapsedMs:F1}ms",
                DisplayName, query.Name, query.NameNormalized, elapsedMs);
        }
        
        return result;
    }
}
