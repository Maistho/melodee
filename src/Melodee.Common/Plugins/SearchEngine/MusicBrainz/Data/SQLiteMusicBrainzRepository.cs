using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using Album = Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized.Album;
using Artist = Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized.Artist;
using Directory = System.IO.Directory;


namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;

/// <summary>
///     SQLite backend database created from MusicBrainz data dumps using Entity Framework Core.
///     <remarks>
///         See https://metabrainz.org/datasets/postgres-dumps#musicbrainz
///     </remarks>
/// </summary>
public class SQLiteMusicBrainzRepository(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MusicBrainzDbContext> dbContextFactory) : MusicBrainzRepositoryBase(logger, configurationFactory), IDisposable
{
    private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
    private const int CacheMaxSize = 10000;
    private const int CacheExpirationMinutes = 60;

    private static readonly object LuceneLock = new();
    private static FSDirectory? _luceneDirectory;
    private static DirectoryReader? _luceneReader;
    private static IndexSearcher? _luceneSearcher;
    private static string? _lucenePath;

    private static readonly ConcurrentDictionary<string, CachedSearchResult> SearchCache = new();

    private sealed record CachedSearchResult(PagedResult<ArtistSearchResult> Result, DateTime CachedAt);

    public void Dispose()
    {
        CloseLuceneResources();
        GC.SuppressFinalize(this);
    }

    private static void CloseLuceneResources()
    {
        lock (LuceneLock)
        {
            _luceneSearcher = null;
            _luceneReader?.Dispose();
            _luceneReader = null;
            _luceneDirectory?.Dispose();
            _luceneDirectory = null;
            _lucenePath = null;
        }
    }

    private IndexSearcher? GetOrCreateSearcher(string lucenePath)
    {
        lock (LuceneLock)
        {
            // If path changed or not initialized, recreate
            if (_lucenePath != lucenePath || _luceneDirectory == null || _luceneReader == null)
            {
                CloseLuceneResources();

                if (!Directory.Exists(lucenePath) || Directory.GetFiles(lucenePath).Length == 0)
                {
                    return null;
                }

                _lucenePath = lucenePath;
                _luceneDirectory = FSDirectory.Open(lucenePath);
                _luceneReader = DirectoryReader.Open(_luceneDirectory);
                _luceneSearcher = new IndexSearcher(_luceneReader);

                Logger.Debug("[{RepoName}] Initialized persistent Lucene searcher at [{Path}]",
                    nameof(SQLiteMusicBrainzRepository), lucenePath);
            }

            return _luceneSearcher;
        }
    }

    private static void CleanExpiredCache()
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-CacheExpirationMinutes);
        var expiredKeys = SearchCache.Where(kvp => kvp.Value.CachedAt < cutoff).Select(kvp => kvp.Key).ToList();
        foreach (var key in expiredKeys)
        {
            SearchCache.TryRemove(key, out _);
        }

        // If still over max size, remove oldest entries
        if (SearchCache.Count > CacheMaxSize)
        {
            var toRemove = SearchCache.OrderBy(kvp => kvp.Value.CachedAt).Take(SearchCache.Count - CacheMaxSize + 100).Select(kvp => kvp.Key).ToList();
            foreach (var key in toRemove)
            {
                SearchCache.TryRemove(key, out _);
            }
        }
    }

    public override async Task<Album?> GetAlbumByMusicBrainzId(Guid musicBrainzId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var musicBrainzIdRaw = musicBrainzId.ToString();

        return await context.Albums
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MusicBrainzIdRaw == musicBrainzIdRaw, cancellationToken);
    }

    public override async Task<PagedResult<ArtistSearchResult>> SearchArtist(
        ArtistQuery query,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        // Check cancellation token early
        cancellationToken.ThrowIfCancellationRequested();

        var startTicks = Stopwatch.GetTimestamp();

        // Check cache first
        var cacheKey = $"{query.NameNormalized}:{query.MusicBrainzIdValue}:{maxResults}";
        if (SearchCache.TryGetValue(cacheKey, out var cached) &&
            cached.CachedAt > DateTime.UtcNow.AddMinutes(-CacheExpirationMinutes))
        {
            Logger.Debug("[{RepoName}] Cache HIT for [{Query}]", nameof(SQLiteMusicBrainzRepository), LogSanitizer.Sanitize(query.NameNormalized));
            return cached.Result;
        }

        var data = new List<ArtistSearchResult>();
        var maxLuceneResults = 10;
        var totalCount = 0;

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var storagePath = configuration.GetValue<string>(SettingRegistry.SearchEngineMusicBrainzStoragePath) ??
                          throw new InvalidOperationException(
                              $"Invalid setting for [{SettingRegistry.SearchEngineMusicBrainzStoragePath}]");

        var musicBrainzIdsFromLucene = new List<string>();
        var shouldUseDirectSearch = query.MusicBrainzIdValue == null;

        if (query.MusicBrainzIdValue != null)
        {
            Logger.Debug("[{RepoName}] Searching by MusicBrainzId: {MusicBrainzId}",
                nameof(SQLiteMusicBrainzRepository), query.MusicBrainzIdValue.Value);
            musicBrainzIdsFromLucene.Add(query.MusicBrainzIdValue.Value.ToString());
            shouldUseDirectSearch = false;
        }
        else
        {
            var lucenePath = Path.Combine(storagePath, "lucene");
            var searcher = GetOrCreateSearcher(lucenePath);

            if (searcher != null)
            {
                Logger.Debug("[{RepoName}] Using Lucene index at [{Path}] for query: NameNormalized=[{NameNormalized}], NameNormalizedReversed=[{NameNormalizedReversed}]",
                    nameof(SQLiteMusicBrainzRepository), lucenePath, LogSanitizer.Sanitize(query.NameNormalized), LogSanitizer.Sanitize(query.NameNormalizedReversed));

                // Build a comprehensive query with multiple search strategies
                BooleanQuery categoryQuery = [];

                // Strategy 1: Exact match on normalized name (highest priority via boosting)
                var exactQuery = new TermQuery(new Term(nameof(Artist.NameNormalized), query.NameNormalized)) { Boost = 10.0f };
                categoryQuery.Add(new BooleanClause(exactQuery, Occur.SHOULD));

                // Strategy 2: Exact match on reversed name
                var reversedQuery = new TermQuery(new Term(nameof(Artist.NameNormalized), query.NameNormalizedReversed)) { Boost = 8.0f };
                categoryQuery.Add(new BooleanClause(reversedQuery, Occur.SHOULD));

                // Strategy 3: Prefix match for partial names (e.g., "METALLICA" matches "METALLICABAND")
                if (query.NameNormalized.Length >= 4)
                {
                    var prefixQuery = new PrefixQuery(new Term(nameof(Artist.NameNormalized), query.NameNormalized)) { Boost = 5.0f };
                    categoryQuery.Add(new BooleanClause(prefixQuery, Occur.SHOULD));
                }

                // Strategy 4: Fuzzy match for typos/variations (edit distance 1-2)
                if (query.NameNormalized.Length >= 5)
                {
                    var fuzzyQuery = new FuzzyQuery(new Term(nameof(Artist.NameNormalized), query.NameNormalized), 2) { Boost = 3.0f };
                    categoryQuery.Add(new BooleanClause(fuzzyQuery, Occur.SHOULD));
                }

                // Strategy 5: Search in alternate names (aliases)
                var alternateQuery = new TermQuery(new Term(nameof(Artist.AlternateNames), query.NameNormalized)) { Boost = 4.0f };
                categoryQuery.Add(new BooleanClause(alternateQuery, Occur.SHOULD));

                // Strategy 6: Word tokenization - for multi-word artists like "David Sylvian And Robert Fripp"
                // or collaborations like "Smokey Robinson Miracles" -> search for "SMOKEY", "ROBINSON", "MIRACLES"
                var words = ExtractWordsFromNormalized(query.NameNormalized);
                if (words.Length > 1)
                {
                    Logger.Debug("[{RepoName}] Tokenized query into [{WordCount}] words: [{Words}]",
                        nameof(SQLiteMusicBrainzRepository), words.Length, LogSanitizer.Sanitize(string.Join(", ", words.Take(5))));

                    foreach (var word in words.Where(w => w.Length >= 4).Take(3))
                    {
                        // Search for each significant word as a prefix
                        var wordPrefixQuery = new PrefixQuery(new Term(nameof(Artist.NameNormalized), word)) { Boost = 2.0f };
                        categoryQuery.Add(new BooleanClause(wordPrefixQuery, Occur.SHOULD));

                        // Also search in alternate names
                        var wordAltQuery = new TermQuery(new Term(nameof(Artist.AlternateNames), word));
                        categoryQuery.Add(new BooleanClause(wordAltQuery, Occur.SHOULD));
                    }
                }

                ScoreDoc[] hits = searcher.Search(categoryQuery, maxLuceneResults).ScoreDocs;
                musicBrainzIdsFromLucene.AddRange(hits.Select(t => searcher.Doc(t.Doc))
                    .Select(hitDoc => hitDoc.Get(nameof(Artist.MusicBrainzIdRaw))));

                Logger.Debug("[{RepoName}] Lucene search returned [{HitCount}] hits for [{NameNormalized}]",
                    nameof(SQLiteMusicBrainzRepository), hits.Length, LogSanitizer.Sanitize(query.NameNormalized));

                // OPTIMIZATION: If Lucene index exists and returns 0 results, don't fall back to direct search
                // The Lucene index contains the same data as the database, so if Lucene finds nothing,
                // direct search won't find anything either (just slower). Only use direct search when
                // Lucene is completely unavailable.
                shouldUseDirectSearch = false;
            }
            else
            {
                Logger.Warning("[{RepoName}] Lucene index not available at [{Path}], falling back to direct database search",
                    nameof(SQLiteMusicBrainzRepository), lucenePath);
                // Only enable direct search when Lucene is completely unavailable
                shouldUseDirectSearch = true;
            }
        }

        try
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{Name}] SearchArtist [{ArtistQuery}]",
                       nameof(SQLiteMusicBrainzRepository), query))
            {
                await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                // Use direct search when Lucene is not available or found no results
                if (shouldUseDirectSearch && !string.IsNullOrEmpty(query.NameNormalized))
                {
                    Logger.Debug("[{RepoName}] Using direct database search for [{NameNormalized}]",
                        nameof(SQLiteMusicBrainzRepository), LogSanitizer.Sanitize(query.NameNormalized));

                    // OPTIMIZED: First try exact match (uses index), then fall back to broader search
                    var directArtists = await context.Artists
                        .AsNoTracking()
                        .Where(a => a.NameNormalized == query.NameNormalized)
                        .OrderBy(a => a.SortName)
                        .Take(maxLuceneResults)
                        .ToArrayAsync(cancellationToken);

                    // If no exact match, try reversed name match (also uses index)
                    if (directArtists.Length == 0 && query.NameNormalizedReversed != query.NameNormalized)
                    {
                        directArtists = await context.Artists
                            .AsNoTracking()
                            .Where(a => a.NameNormalized == query.NameNormalizedReversed)
                            .OrderBy(a => a.SortName)
                            .Take(maxLuceneResults)
                            .ToArrayAsync(cancellationToken);
                    }

                    // If still no match, try searching in alternate names
                    if (directArtists.Length == 0)
                    {
                        directArtists = await context.Artists
                            .AsNoTracking()
                            .Where(a => a.AlternateNames != null && a.AlternateNames.Contains(query.NameNormalized))
                            .OrderBy(a => a.SortName)
                            .Take(maxLuceneResults)
                            .ToArrayAsync(cancellationToken);
                    }

                    Logger.Debug("[{RepoName}] Direct search found [{Count}] artists for [{NameNormalized}]",
                        nameof(SQLiteMusicBrainzRepository), directArtists.Length, LogSanitizer.Sanitize(query.NameNormalized));

                    if (directArtists.Length > 0)
                    {
                        // OPTIMIZED: Batch load all albums for found artists in a single query
                        var artistIds = directArtists.Select(a => a.MusicBrainzArtistId).ToArray();
                        var allAlbums = await context.Albums
                            .AsNoTracking()
                            .Where(a => artistIds.Contains(a.MusicBrainzArtistId) && a.ReleaseDate > DateTime.MinValue)
                            .ToArrayAsync(cancellationToken);

                        var albumsByArtist = allAlbums
                            .GroupBy(a => a.MusicBrainzArtistId)
                            .ToDictionary(g => g.Key, g => g
                                .GroupBy(x => x.ReleaseGroupMusicBrainzIdRaw)
                                .Select(rg => rg.OrderBy(x => x.ReleaseDate).First())
                                .ToArray());

                        foreach (var artist in directArtists)
                        {
                            var rank = artist.NameNormalized == query.NameNormalized ? 10 : 1;
                            if (artist.AlternateNamesValues.Contains(query.NameNormalized))
                            {
                                rank++;
                            }

                            if (artist.AlternateNamesValues.Contains(query.Name.CleanString().ToNormalizedString()))
                            {
                                rank++;
                            }

                            if (artist.AlternateNamesValues.Contains(query.NameNormalizedReversed))
                            {
                                rank++;
                            }

                            var artistAlbums = albumsByArtist.GetValueOrDefault(artist.MusicBrainzArtistId, []);
                            rank += artistAlbums.Length;

                            if (query.AlbumKeyValues != null)
                            {
                                rank += artistAlbums.Length;
                                foreach (var albumKeyValues in query.AlbumKeyValues)
                                {
                                    rank += artistAlbums.Count(x =>
                                        x.ReleaseDate.Year.ToString() == albumKeyValues.Key &&
                                        x.NameNormalized == albumKeyValues.Value.ToNormalizedString());
                                }
                            }

                            data.Add(new ArtistSearchResult
                            {
                                AlternateNames = artist.AlternateNames?.ToTags()?.ToArray() ?? [],
                                FromPlugin =
                                    $"{nameof(MusicBrainzArtistSearchEnginePlugin)}:{nameof(SQLiteMusicBrainzRepository)}",
                                UniqueId = SafeParser.Hash(artist.MusicBrainzId.ToString()),
                                Rank = rank,
                                Name = artist.Name,
                                SortName = artist.SortName,
                                MusicBrainzId = artist.MusicBrainzId,
                                AlbumCount = artistAlbums.Count(x => x.ReleaseDate > DateTime.MinValue),
                                Releases = artistAlbums
                                    .Where(x => x.ReleaseDate > DateTime.MinValue)
                                    .OrderBy(x => x.ReleaseDate)
                                    .ThenBy(x => x.SortName).Select(x => new AlbumSearchResult
                                    {
                                        AlbumType = SafeParser.ToEnum<AlbumType>(x.ReleaseType),
                                        ReleaseDate = x.ReleaseDate.ToString("o", CultureInfo.InvariantCulture),
                                        UniqueId = SafeParser.Hash(x.MusicBrainzId.ToString()),
                                        Name = x.Name,
                                        NameNormalized = x.NameNormalized,
                                        MusicBrainzResourceGroupId = x.ReleaseGroupMusicBrainzId,
                                        SortName = x.SortName,
                                        MusicBrainzId = x.MusicBrainzId
                                    }).ToArray()
                            });
                        }
                    }

                    totalCount = directArtists.Length;
                }
                else if (musicBrainzIdsFromLucene.Count > 0)
                {
                    // Optimized EF Core query with no tracking for read-only operations
                    var artists = await context.Artists
                        .AsNoTracking()
                        .Where(a => musicBrainzIdsFromLucene.Contains(a.MusicBrainzIdRaw))
                        .OrderBy(a => a.SortName)
                        .ToArrayAsync(cancellationToken);

                    if (artists.Length > 0)
                    {
                        // OPTIMIZED: Batch load all albums for found artists in a single query
                        var artistIds = artists.Select(a => a.MusicBrainzArtistId).ToArray();
                        var allAlbums = await context.Albums
                            .AsNoTracking()
                            .Where(a => artistIds.Contains(a.MusicBrainzArtistId) && a.ReleaseDate > DateTime.MinValue)
                            .ToArrayAsync(cancellationToken);

                        var albumsByArtist = allAlbums
                            .GroupBy(a => a.MusicBrainzArtistId)
                            .ToDictionary(g => g.Key, g => g
                                .GroupBy(x => x.ReleaseGroupMusicBrainzIdRaw)
                                .Select(rg => rg.OrderBy(x => x.ReleaseDate).First())
                                .ToArray());

                        foreach (var artist in artists)
                        {
                            var rank = artist.NameNormalized == query.NameNormalized ? 10 : 1;
                            if (artist.AlternateNamesValues.Contains(query.NameNormalized))
                            {
                                rank++;
                            }

                            if (artist.AlternateNamesValues.Contains(query.Name.CleanString().ToNormalizedString()))
                            {
                                rank++;
                            }

                            if (artist.AlternateNamesValues.Contains(query.NameNormalizedReversed))
                            {
                                rank++;
                            }

                            var artistAlbums = albumsByArtist.GetValueOrDefault(artist.MusicBrainzArtistId, []);
                            rank += artistAlbums.Length;

                            if (query.AlbumKeyValues != null)
                            {
                                rank += artistAlbums.Length;
                                foreach (var albumKeyValues in query.AlbumKeyValues)
                                {
                                    rank += artistAlbums.Count(x =>
                                        x.ReleaseDate.Year.ToString() == albumKeyValues.Key &&
                                        x.NameNormalized == albumKeyValues.Value.ToNormalizedString());
                                }
                            }

                            data.Add(new ArtistSearchResult
                            {
                                AlternateNames = artist.AlternateNames?.ToTags()?.ToArray() ?? [],
                                FromPlugin =
                                    $"{nameof(MusicBrainzArtistSearchEnginePlugin)}:{nameof(SQLiteMusicBrainzRepository)}",
                                UniqueId = SafeParser.Hash(artist.MusicBrainzId.ToString()),
                                Rank = rank,
                                Name = artist.Name,
                                SortName = artist.SortName,
                                MusicBrainzId = artist.MusicBrainzId,
                                AlbumCount = artistAlbums.Count(x => x.ReleaseDate > DateTime.MinValue),
                                Releases = artistAlbums
                                    .Where(x => x.ReleaseDate > DateTime.MinValue)
                                    .OrderBy(x => x.ReleaseDate)
                                    .ThenBy(x => x.SortName).Select(x => new AlbumSearchResult
                                    {
                                        AlbumType = SafeParser.ToEnum<AlbumType>(x.ReleaseType),
                                        ReleaseDate = x.ReleaseDate.ToString("o", CultureInfo.InvariantCulture),
                                        UniqueId = SafeParser.Hash(x.MusicBrainzId.ToString()),
                                        Name = x.Name,
                                        NameNormalized = x.NameNormalized,
                                        MusicBrainzResourceGroupId = x.ReleaseGroupMusicBrainzId,
                                        SortName = x.SortName,
                                        MusicBrainzId = x.MusicBrainzId
                                    }).ToArray()
                            });
                        }
                    }

                    totalCount = artists.Length;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Let cancellation exceptions propagate
            throw;
        }
        catch (Exception e)
        {
            Logger.Error(e, "[MusicBrainzRepository] Search Engine Exception ArtistQuery [{Query}]", query.ToString());
        }

        var elapsedMs = Stopwatch.GetElapsedTime(startTicks).TotalMilliseconds;

        var result = new PagedResult<ArtistSearchResult>
        {
            OperationTime = (long)elapsedMs * 1000, // Convert to microseconds
            TotalCount = totalCount,
            TotalPages = maxResults > 0 ? SafeParser.ToNumber<int>((totalCount + maxResults - 1) / maxResults) : 0,
            Data = data.OrderByDescending(x => x.Rank).Take(Math.Max(0, maxResults)).ToArray()
        };

        // Cache the result
        SearchCache[cacheKey] = new CachedSearchResult(result, DateTime.UtcNow);

        // Periodically clean expired cache entries (every ~100 searches)
        if (SearchCache.Count > CacheMaxSize / 10 && Random.Shared.Next(100) == 0)
        {
            CleanExpiredCache();
        }

        if (data.Count > 0)
        {
            Logger.Debug("[{RepoName}] SearchArtist COMPLETE: Found [{Count}] results for [{Query}] in {ElapsedMs:F1}ms. Top result: [{TopArtist}]",
                nameof(SQLiteMusicBrainzRepository), data.Count, LogSanitizer.Sanitize(query.NameNormalized), elapsedMs, LogSanitizer.Sanitize(data.First().Name));
        }
        else
        {
            Logger.Debug("[{RepoName}] SearchArtist COMPLETE: NO RESULTS for [{Query}] in {ElapsedMs:F1}ms (LuceneHits={LuceneHits}, DirectSearch={DirectSearch})",
                nameof(SQLiteMusicBrainzRepository), LogSanitizer.Sanitize(query.NameNormalized), elapsedMs, musicBrainzIdsFromLucene.Count, shouldUseDirectSearch);
        }

        return result;
    }

    public override async Task<OperationResult<bool>> ImportData(
        ImportProgressCallback? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: ImportData (Streaming)"))
        {
            var configuration =
                await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

            var storagePath = configuration.GetValue<string>(SettingRegistry.SearchEngineMusicBrainzStoragePath);
            if (storagePath == null || !Directory.Exists(storagePath))
            {
                Logger.Warning("MusicBrainz storage path is invalid [{KeyNam}]",
                    SettingRegistry.SearchEngineMusicBrainzStoragePath);
                return new OperationResult<bool>
                {
                    Data = false
                };
            }

            // Prepare database context and optimizations
            await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);

            // SQLite performance optimizations for bulk insert
            await context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = OFF", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = MEMORY", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("PRAGMA temp_store = MEMORY", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("PRAGMA cache_size = -64000", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("PRAGMA auto_vacuum = NONE", cancellationToken);

            try
            {
                // Use the new streaming importer that never loads full datasets into memory
                var importer = new StreamingMusicBrainzImporter(Logger);
                var luceneIndexPath = Path.Combine(storagePath, "lucene");

                await importer.ImportAsync(
                    context,
                    storagePath,
                    luceneIndexPath,
                    progressCallback,
                    cancellationToken);

                // Verify import results
                var artistCount = await context.Artists.CountAsync(cancellationToken);
                var albumCount = await context.Albums.CountAsync(cancellationToken);

                Logger.Information(
                    "MusicBrainzRepository: Streaming import complete. Artists: {ArtistCount:N0}, Albums: {AlbumCount:N0}",
                    artistCount, albumCount);

                return new OperationResult<bool>
                {
                    Data = artistCount > 0 && albumCount > 0
                };
            }
            finally
            {
                // Restore safe SQLite settings after bulk import
                await context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = NORMAL", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL", cancellationToken);
            }
        }
    }

    /// <summary>
    /// Extracts individual words from a normalized artist name for tokenized search.
    /// Uses common word boundaries and patterns to split concatenated names.
    /// </summary>
    /// <example>
    /// "SMOKEYROBINSONMIRACLES" -> ["SMOKEY", "ROBINSON", "MIRACLES"]
    /// "ARMINVANBUURENANDDJSHAH" -> ["ARMIN", "VAN", "BUUREN", "AND", "DJ", "SHAH"]
    /// </example>
    private static string[] ExtractWordsFromNormalized(string normalizedName)
    {
        if (string.IsNullOrEmpty(normalizedName) || normalizedName.Length < 4)
        {
            return [];
        }

        var words = new List<string>();

        // Common patterns that indicate word boundaries in normalized (uppercase, no spaces) text
        // These are common prefixes/suffixes/conjunctions that help identify where words start/end
        string[] commonPatterns =
        [
            "AND", "THE", "FEAT", "FEATURING", "WITH", "VS", "VERSUS",
            "DJ", "MC", "DR", "MR", "MRS", "MS",
            "VAN", "VON", "DE", "LA", "LE", "EL",
            "BAND", "GROUP", "TRIO", "QUARTET", "QUINTET", "ORCHESTRA", "ENSEMBLE",
            "PROJECT", "EXPERIENCE", "COLLECTIVE", "FAMILY", "BROTHERS", "SISTERS"
        ];

        var remaining = normalizedName;

        // First pass: try to identify known patterns/words
        foreach (var pattern in commonPatterns.OrderByDescending(p => p.Length))
        {
            var idx = remaining.IndexOf(pattern, StringComparison.Ordinal);
            if (idx >= 0)
            {
                // Found a known pattern - this helps us identify word boundaries
                if (idx > 0)
                {
                    var before = remaining[..idx];
                    if (before.Length >= 3)
                    {
                        words.Add(before);
                    }
                }

                words.Add(pattern);

                if (idx + pattern.Length < remaining.Length)
                {
                    var after = remaining[(idx + pattern.Length)..];
                    if (after.Length >= 3)
                    {
                        // Recursively extract from the remainder
                        words.AddRange(ExtractWordsFromNormalized(after));
                    }
                }

                return words.Distinct().Where(w => w.Length >= 3).ToArray();
            }
        }

        // Second pass: if no known patterns found, try to split on uppercase transitions
        // This works for names like "BEATLES" which is just one word
        // For longer concatenated names, we'll return the whole string plus try some heuristic splits

        // If the name is reasonably short, it's probably a single word/name
        if (normalizedName.Length <= 12)
        {
            return [normalizedName];
        }

        // For longer strings, try splitting at common name lengths (5-8 chars)
        // This is a heuristic that helps with names like "SMOKEYROBINSON" -> "SMOKEY" + "ROBINSON"
        words.Add(normalizedName);

        // Also add potential first-name splits (common first name lengths)
        foreach (var splitLen in new[] { 5, 6, 7, 8 })
        {
            if (normalizedName.Length > splitLen + 3)
            {
                var firstPart = normalizedName[..splitLen];
                var secondPart = normalizedName[splitLen..];

                if (firstPart.Length >= 4 && secondPart.Length >= 4)
                {
                    words.Add(firstPart);
                    words.Add(secondPart);
                }
            }
        }

        return words.Distinct().Where(w => w.Length >= 3).ToArray();
    }
}
