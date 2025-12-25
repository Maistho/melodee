using System.Diagnostics;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Services.Caching;
using Serilog;

namespace Melodee.Common.Services.Scanning;

/// <summary>
///     Context for a single library processing run.
///     Holds per-run caches to avoid duplicate API calls within a processing session.
/// </summary>
public sealed class DirectoryRunContext : IDisposable
{
    private readonly Stopwatch _runStopwatch;
    private long _pluginTimeMs;
    private long _albumProcessingTimeMs;
    private long _enrichmentTimeMs;
    private long _copyTimeMs;
    private int _directoriesProcessed;

    /// <summary>
    ///     Per-run cache for artist search results.
    ///     Keyed by normalized artist identity (name + optional MBID/SpotifyId).
    /// </summary>
    public SingleFlightCache<ArtistQuery, ArtistSearchResult[]> ArtistSearchCache { get; }

    /// <summary>
    ///     Per-run cache for album image search results.
    ///     Keyed by normalized album identity (artist + album name + year).
    /// </summary>
    public SingleFlightCache<AlbumQuery, ImageSearchResult[]> AlbumImageCache { get; }

    /// <summary>
    ///     Global throttler for external API calls.
    /// </summary>
    public ExternalApiThrottler ApiThrottler { get; }

    public DirectoryRunContext(
        int artistCacheSize = 500,
        int albumImageCacheSize = 500,
        TimeSpan? negativeCacheTtl = null)
    {
        _runStopwatch = Stopwatch.StartNew();

        var negTtl = negativeCacheTtl ?? TimeSpan.FromMinutes(2);

        ArtistSearchCache = new SingleFlightCache<ArtistQuery, ArtistSearchResult[]>(
            NormalizeArtistKey,
            maxSize: artistCacheSize,
            positiveTtl: TimeSpan.FromHours(24),
            negativeTtl: negTtl,
            cacheName: "ArtistRunCache");

        AlbumImageCache = new SingleFlightCache<AlbumQuery, ImageSearchResult[]>(
            NormalizeAlbumImageKey,
            maxSize: albumImageCacheSize,
            positiveTtl: TimeSpan.FromHours(24),
            negativeTtl: negTtl,
            cacheName: "AlbumImageRunCache");

        ApiThrottler = new ExternalApiThrottler();
    }

    /// <summary>
    ///     Normalizes artist query to a cache key.
    ///     Uses name + MBID + SpotifyId for unique identification.
    /// </summary>
    public static string NormalizeArtistKey(ArtistQuery query)
    {
        var name = (query.NameNormalized ?? query.Name ?? string.Empty)
            .Trim()
            .ToUpperInvariant();

        var mbid = query.MusicBrainzId ?? string.Empty;
        var spotifyId = query.SpotifyId ?? string.Empty;

        return $"ARTIST:{name}|MBID:{mbid}|SPOTIFY:{spotifyId}";
    }

    /// <summary>
    ///     Normalizes album query to a cache key.
    ///     Uses artist + album name + year for unique identification.
    /// </summary>
    public static string NormalizeAlbumImageKey(AlbumQuery query)
    {
        var artist = (query.Artist ?? string.Empty)
            .Trim()
            .ToUpperInvariant();

        var name = (query.Name ?? string.Empty)
            .Trim()
            .ToUpperInvariant();

        var year = query.Year.ToString();

        return $"ALBUM:{artist}|{name}|{year}";
    }

    /// <summary>
    ///     Records time spent in plugin processing.
    /// </summary>
    public void AddPluginTime(long milliseconds)
    {
        Interlocked.Add(ref _pluginTimeMs, milliseconds);
    }

    /// <summary>
    ///     Records time spent in album processing.
    /// </summary>
    public void AddAlbumProcessingTime(long milliseconds)
    {
        Interlocked.Add(ref _albumProcessingTimeMs, milliseconds);
    }

    /// <summary>
    ///     Records time spent in external enrichment (API calls).
    /// </summary>
    public void AddEnrichmentTime(long milliseconds)
    {
        Interlocked.Add(ref _enrichmentTimeMs, milliseconds);
    }

    /// <summary>
    ///     Records time spent copying files.
    /// </summary>
    public void AddCopyTime(long milliseconds)
    {
        Interlocked.Add(ref _copyTimeMs, milliseconds);
    }

    /// <summary>
    ///     Increments the count of processed directories.
    /// </summary>
    public void IncrementDirectoriesProcessed()
    {
        Interlocked.Increment(ref _directoriesProcessed);
    }

    /// <summary>
    ///     Logs summary statistics for this run.
    /// </summary>
    public void LogSummary()
    {
        var artistStats = ArtistSearchCache.GetStatistics();
        var albumImageStats = AlbumImageCache.GetStatistics();
        var throttleStats = ApiThrottler.GetStatistics();

        Log.Information(
            "[DirectoryRunContext] Run completed in {TotalMs}ms | " +
            "Directories: {DirCount} | " +
            "Plugin: {PluginMs}ms | Album: {AlbumMs}ms | Enrichment: {EnrichMs}ms | Copy: {CopyMs}ms",
            _runStopwatch.ElapsedMilliseconds,
            _directoriesProcessed,
            _pluginTimeMs,
            _albumProcessingTimeMs,
            _enrichmentTimeMs,
            _copyTimeMs);

        Log.Information(
            "[DirectoryRunContext] Artist cache: {Entries} entries, {Hits} hits, {Misses} misses, {Coalesced} coalesced ({HitRate:P1} hit rate)",
            artistStats.TotalEntries,
            artistStats.Hits,
            artistStats.Misses,
            artistStats.CoalescedRequests,
            artistStats.HitRate);

        Log.Information(
            "[DirectoryRunContext] Album image cache: {Entries} entries, {Hits} hits, {Misses} misses, {Coalesced} coalesced ({HitRate:P1} hit rate)",
            albumImageStats.TotalEntries,
            albumImageStats.Hits,
            albumImageStats.Misses,
            albumImageStats.CoalescedRequests,
            albumImageStats.HitRate);

        foreach (var (provider, stats) in throttleStats)
        {
            Log.Information(
                "[DirectoryRunContext] {Provider} throttle: {Total} total requests, {Throttled} rate-limited",
                provider,
                stats.TotalRequests,
                stats.ThrottledRequests);
        }
    }

    public void Dispose()
    {
        LogSummary();
        ArtistSearchCache.Dispose();
        AlbumImageCache.Dispose();
        ApiThrottler.Dispose();
    }
}
