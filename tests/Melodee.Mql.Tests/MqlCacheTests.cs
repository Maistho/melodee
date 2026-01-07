using System.Linq.Expressions;
using FluentAssertions;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using NodaTime;

namespace Melodee.Mql.Tests;

public class MqlCacheTests
{
    private readonly MqlTokenizer _tokenizer;
    private readonly MqlParser _parser;
    private readonly MqlSongCompiler _baseCompiler;
    private readonly MqlCachedCompiler _cachedCompiler;

    public MqlCacheTests()
    {
        _tokenizer = new MqlTokenizer();
        _parser = new MqlParser();
        _baseCompiler = new MqlSongCompiler();
        _cachedCompiler = new MqlCachedCompiler();
    }

    [Fact]
    public void CacheHit_ReturnsSameExpressionOnSecondCall()
    {
        var expression = CompileCachedQuery("artist:Pink Floyd");
        var expression2 = CompileCachedQuery("artist:Pink Floyd");

        expression2.Should().BeSameAs(expression);
    }

    [Fact]
    public void CacheMiss_DifferentQuery_CreatesNewExpression()
    {
        var expression1 = CompileCachedQuery("artist:Pink Floyd");
        var expression2 = CompileCachedQuery("artist:The Beatles");

        expression2.Should().NotBeSameAs(expression1);
    }

    [Fact]
    public void CacheKey_IncludesUserId()
    {
        var expression1 = CompileCachedQuery("rating:>3", userId: 1);
        var expression2 = CompileCachedQuery("rating:>3", userId: 2);

        expression2.Should().NotBeSameAs(expression1);
    }

    [Fact]
    public void CacheKey_SameUserId_ReturnsCachedExpression()
    {
        var expression1 = CompileCachedQuery("rating:>3", userId: 1);
        var expression2 = CompileCachedQuery("rating:>3", userId: 1);

        expression2.Should().BeSameAs(expression1);
    }

    [Fact]
    public void CacheKey_AnonymousUser_HasDifferentKeyThanAuthenticated()
    {
        var expression1 = CompileCachedQuery("rating:>3", userId: null);
        var expression2 = CompileCachedQuery("rating:>3", userId: 1);

        expression2.Should().NotBeSameAs(expression1);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var expression1 = CompileCachedQuery("artist:Pink Floyd");
        var expression2 = CompileCachedQuery("artist:The Beatles");

        _cachedCompiler.Cache.ClearAll();

        var expression3 = CompileCachedQuery("artist:Pink Floyd");
        var expression4 = CompileCachedQuery("artist:The Beatles");

        expression3.Should().NotBeSameAs(expression1);
        expression4.Should().NotBeSameAs(expression2);
    }

    [Fact]
    public void ClearEntityType_RemovesOnlyEntityEntries()
    {
        var expression1 = CompileCachedQuery("artist:Pink Floyd");
        var statsBefore = _cachedCompiler.Cache.GetStatistics();
        statsBefore.EntryCount.Should().BeGreaterThan(0);

        _cachedCompiler.Cache.Clear<Song>();

        var statsAfter = _cachedCompiler.Cache.GetStatistics();
        statsAfter.EntryCount.Should().Be(0);
    }

    [Fact]
    public void GetStatistics_TracksHitsAndMisses()
    {
        CompileCachedQuery("artist:Pink Floyd");
        CompileCachedQuery("artist:Pink Floyd");
        CompileCachedQuery("artist:Pink Floyd");

        var stats = _cachedCompiler.Cache.GetStatistics();
        stats.HitCount.Should().Be(2);
        stats.MissCount.Should().Be(1);
        stats.EntryCount.Should().Be(1);
        stats.HitRatePercentage.Should().BeApproximately(66.67, 0.5);
    }

    [Fact]
    public void GetStatistics_ShowsCorrectEntryCount()
    {
        CompileCachedQuery("artist:Pink Floyd");
        CompileCachedQuery("artist:The Beatles");
        CompileCachedQuery("artist:Queen");

        var stats = _cachedCompiler.Cache.GetStatistics();
        stats.EntryCount.Should().Be(3);
    }

    [Fact]
    public void GetStatistics_ShowsMaxEntries()
    {
        var cache = new MqlExpressionCache(maxEntries: 100);
        var compiler = new MqlCachedCompiler(cache);

        var stats = cache.GetStatistics();
        stats.MaxEntries.Should().Be(100);
    }

    [Fact]
    public void GetStatistics_ShowsDefaultTtl()
    {
        var cache = new MqlExpressionCache();
        var stats = cache.GetStatistics();
        stats.DefaultTtlMinutes.Should().Be(30);
    }

    [Fact]
    public void CustomTtl_ExpiresEntry()
    {
        var cache = new MqlExpressionCache(defaultTtl: TimeSpan.FromMilliseconds(100));
        var compiler = new MqlCachedCompiler(cache);

        var expression1 = CompileWithCache(compiler, "artist:Pink Floyd");
        var stats1 = cache.GetStatistics();
        stats1.EntryCount.Should().Be(1);

        Thread.Sleep(150);

        var expression2 = CompileWithCache(compiler, "artist:Pink Floyd");
        var stats2 = cache.GetStatistics();

        stats2.MissCount.Should().BeGreaterThan(stats1.MissCount);
    }

    [Fact]
    public void MaxEntries_EnforcesSizeLimit()
    {
        var cache = new MqlExpressionCache(maxEntries: 5);
        var compiler = new MqlCachedCompiler(cache);

        for (int i = 0; i < 10; i++)
        {
            CompileWithCache(compiler, $"artist:Band{i}");
        }

        var stats = cache.GetStatistics();
        stats.EntryCount.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public void LRU_Eviction_RemovesLeastRecentlyUsed()
    {
        var cache = new MqlExpressionCache(maxEntries: 3);
        var compiler = new MqlCachedCompiler(cache);

        CompileWithCache(compiler, "artist:Band0");
        CompileWithCache(compiler, "artist:Band1");
        CompileWithCache(compiler, "artist:Band2");

        CompileWithCache(compiler, "artist:Band0");
        CompileWithCache(compiler, "artist:Band3");

        var stats = cache.GetStatistics();
        stats.EntryCount.Should().BeLessThanOrEqualTo(3);
    }

    [Fact]
    public void InvalidateByEntityType_RemovesSongEntries()
    {
        var expression1 = CompileCachedQuery("artist:Pink Floyd");
        var statsBefore = _cachedCompiler.Cache.GetStatistics();
        statsBefore.EntryCount.Should().BeGreaterThan(0);

        _cachedCompiler.Cache.InvalidateByEntityType("Song");

        var statsAfter = _cachedCompiler.Cache.GetStatistics();
        statsAfter.EntryCount.Should().Be(0);
    }

    [Fact]
    public void ThreadSafety_MultipleThreadsCanAccessCache()
    {
        var cache = new MqlExpressionCache();
        var compiler = new MqlCachedCompiler(cache);

        var tasks = Enumerable.Range(0, 10)
            .Select(i => Task.Run(() =>
            {
                for (int j = 0; j < 10; j++)
                {
                    CompileWithCache(compiler, $"artist:Band{j}");
                }
            }))
            .ToArray();

        Task.WaitAll(tasks);

        var stats = cache.GetStatistics();
        stats.EntryCount.Should().Be(10);
        var totalOperations = stats.HitCount + stats.MissCount;
        totalOperations.Should().Be(100);
    }

    [Fact]
    public void ExpressionFromCache_WorksCorrectly()
    {
        var songs = CreateTestData().AsQueryable();
        var expression = CompileCachedQuery("bpm:>110");
        var filtered = songs.Where(expression).ToList();

        filtered.Should().HaveCount(2);
        filtered.All(s => s.BPM > 110).Should().BeTrue();
    }

    [Fact]
    public void CachedExpression_ProducesSameResults()
    {
        var songs = CreateTestData().AsQueryable();
        var expression = CompileCachedQuery("bpm:>100");

        var results1 = songs.Where(expression).ToList();
        var results2 = songs.Where(expression).ToList();

        results1.Should().HaveSameCount(results2);
        results1.Select(s => s.Id).Should().BeEquivalentTo(results2.Select(s => s.Id));
    }

    private Expression<Func<Song, bool>> CompileCachedQuery(string query, int? userId = null)
    {
        return CompileWithCache(_cachedCompiler, query, userId);
    }

    private static Expression<Func<Song, bool>> CompileWithCache(MqlCachedCompiler compiler, string query, int? userId = null)
    {
        var tokenizer = new MqlTokenizer();
        var parser = new MqlParser();

        var tokens = tokenizer.Tokenize(query).ToList();
        var parseResult = parser.Parse(tokens, "songs");

        parseResult.IsValid.Should().BeTrue($"Query '{query}' should parse successfully");
        parseResult.Ast.Should().NotBeNull();

        return compiler.CompileSong(parseResult.Ast!, userId);
    }

    private static Artist CreateArtist(int id, string name, Library library)
    {
        return new Artist
        {
            Id = id,
            ApiKey = Guid.NewGuid(),
            Directory = name.Replace(" ", "").ToLowerInvariant(),
            Name = name,
            NameNormalized = name.ToUpperInvariant(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };
    }

    private static Album CreateAlbum(int id, string name, Artist artist)
    {
        return new Album
        {
            Id = id,
            ApiKey = Guid.NewGuid(),
            ArtistId = artist.Id,
            Artist = artist,
            Name = name,
            NameNormalized = name.ToUpperInvariant(),
            Directory = $"/{artist.Directory}/{name.Replace(" ", "").ToLowerInvariant()}/",
            ReleaseDate = new LocalDate(2020, 1, 1),
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };
    }

    private static Song CreateSong(int id, string title, Album album, int bpm = 120, string[]? genres = null, string[]? moods = null, int? imageCount = null)
    {
        return new Song
        {
            Id = id,
            AlbumId = album.Id,
            Album = album,
            Title = title,
            TitleNormalized = title.ToUpperInvariant(),
            SongNumber = 1,
            FileName = $"{title.Replace(" ", "_").ToLowerInvariant()}.flac",
            FileSize = 1000000,
            FileHash = $"hash_{id}",
            Duration = 180000,
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = bpm,
            ContentType = "audio/flac",
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0),
            Genres = genres ?? Array.Empty<string>(),
            Moods = moods ?? Array.Empty<string>(),
            ImageCount = imageCount
        };
    }

    private static IQueryable<Song> CreateTestData()
    {
        var library = new Library
        {
            Id = 1,
            Name = "Test Library",
            Path = "/test/path",
            Type = (int)LibraryType.Storage,
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };

        var artist1 = CreateArtist(1, "Pink Floyd", library);
        var artist2 = CreateArtist(2, "The Beatles", library);

        var album1 = CreateAlbum(1, "The Dark Side of the Moon", artist1);
        var album2 = CreateAlbum(2, "Abbey Road", artist2);

        var song1 = CreateSong(1, "Time", album1, 120, new[] { "Rock", "Progressive Rock" }, new[] { "Psychedelic", "Atmospheric" }, 5);
        var song2 = CreateSong(2, "Money", album1, 130, new[] { "Rock" }, new[] { "Driving" }, 3);
        var song3 = CreateSong(3, "Come Together", album2, 100, new[] { "Rock" }, new[] { "Classic" }, 1500);
        var song4 = CreateSong(4, "Something", album2, 90, new[] { "Rock", "Ballad" }, new[] { "Romantic" }, 10);

        return new List<Song> { song1, song2, song3, song4 }.AsQueryable();
    }
}
