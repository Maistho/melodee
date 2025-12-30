using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Services;
using Melodee.Common.Services.Models.ArtistDuplicate;
using NodaTime;

namespace Melodee.Tests.Common.Services;

public sealed class ArtistDuplicateFinderTests : ServiceTestBase
{
    [Fact]
    public async Task FindDuplicatesAsync_WithExactNormalizedNameMatch_ReturnsGroup()
    {
        var factory = MockFactory();
        var ctx = await factory.CreateDbContextAsync();

        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        ctx.Libraries.Add(library);
        await ctx.SaveChangesAsync();

        var artist1 = new Artist
        {
            Name = "The Beatles",
            NameNormalized = "BEATLES",
            Directory = "the-beatles",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        var artist2 = new Artist
        {
            Name = "Beatles",
            NameNormalized = "BEATLES",
            Directory = "beatles",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        ctx.Artists.Add(artist1);
        ctx.Artists.Add(artist2);
        await ctx.SaveChangesAsync();

        var finder = new ArtistDuplicateFinder(Logger, MockFactory(), MockConfigurationFactory());
        var criteria = new ArtistDuplicateSearchCriteria(MinScore: 0.7);

        var groups = await finder.FindDuplicatesAsync(criteria);

        Assert.NotEmpty(groups);
        var group = groups.First();
        Assert.Equal(2, group.Artists.Count);
        Assert.Contains(group.Pairs.First().Reasons, r => r == ArtistDuplicateMatchReason.ExactNormalizedNameMatch);
    }

    [Fact]
    public async Task FindDuplicatesAsync_WithFirstLastReversal_DetectsPattern()
    {
        var factory = MockFactory();
        var ctx = await factory.CreateDbContextAsync();

        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        ctx.Libraries.Add(library);
        await ctx.SaveChangesAsync();

        var artist1 = new Artist
        {
            Name = "Elton John",
            NameNormalized = "ELTONJOHN",
            Directory = "elton-john",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        var artist2 = new Artist
        {
            Name = "John, Elton",
            NameNormalized = "JOHNELTON",
            SortName = "John, Elton",
            Directory = "john-elton",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        ctx.Artists.Add(artist1);
        ctx.Artists.Add(artist2);
        await ctx.SaveChangesAsync();

        var finder = new ArtistDuplicateFinder(Logger, MockFactory(), MockConfigurationFactory());
        var criteria = new ArtistDuplicateSearchCriteria(MinScore: 0.7);

        var groups = await finder.FindDuplicatesAsync(criteria);

        Assert.NotEmpty(groups);
        var group = groups.First();
        Assert.Contains(group.Pairs.First().Reasons, r => r == ArtistDuplicateMatchReason.NameFirstLastReversal);
    }

    [Fact]
    public async Task FindDuplicatesAsync_WithHighNameSimilarity_ReturnsGroup()
    {
        var factory = MockFactory();
        var ctx = await factory.CreateDbContextAsync();

        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        ctx.Libraries.Add(library);
        await ctx.SaveChangesAsync();

        var artist1 = new Artist
        {
            Name = "Led Zeppelin",
            NameNormalized = "LEDZEPPELIN",
            Directory = "led-zeppelin",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        var artist2 = new Artist
        {
            Name = "Led Zepelin",
            NameNormalized = "LEDZEPELIN",
            Directory = "led-zepelin",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        ctx.Artists.Add(artist1);
        ctx.Artists.Add(artist2);
        await ctx.SaveChangesAsync();

        var finder = new ArtistDuplicateFinder(Logger, MockFactory(), MockConfigurationFactory());
        var criteria = new ArtistDuplicateSearchCriteria(MinScore: 0.7);

        var groups = await finder.FindDuplicatesAsync(criteria);

        Assert.NotEmpty(groups);
        var group = groups.First();
        Assert.Equal(2, group.Artists.Count);
        Assert.True(group.MaxScore >= 0.7, $"Expected MaxScore >= 0.7, got {group.MaxScore}");
    }

    [Fact]
    public async Task FindDuplicatesAsync_WithNoArtists_ReturnsEmptyList()
    {
        var finder = new ArtistDuplicateFinder(Logger, MockFactory(), MockConfigurationFactory());
        var criteria = new ArtistDuplicateSearchCriteria(MinScore: 0.7);

        var groups = await finder.FindDuplicatesAsync(criteria);

        Assert.Empty(groups);
    }

    [Fact]
    public async Task FindDuplicatesAsync_WithNoSimilarArtists_ReturnsEmptyList()
    {
        var factory = MockFactory();
        var ctx = await factory.CreateDbContextAsync();

        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        ctx.Libraries.Add(library);
        await ctx.SaveChangesAsync();

        var artist1 = new Artist
        {
            Name = "Pink Floyd",
            NameNormalized = "PINKFLOYD",
            Directory = "pink-floyd",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        var artist2 = new Artist
        {
            Name = "The Rolling Stones",
            NameNormalized = "ROLLINGSTONES",
            Directory = "rolling-stones",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        ctx.Artists.Add(artist1);
        ctx.Artists.Add(artist2);
        await ctx.SaveChangesAsync();

        var finder = new ArtistDuplicateFinder(Logger, MockFactory(), MockConfigurationFactory());
        var criteria = new ArtistDuplicateSearchCriteria(MinScore: 0.7);

        var groups = await finder.FindDuplicatesAsync(criteria);

        Assert.Empty(groups);
    }

    [Fact]
    public async Task FindDuplicatesAsync_WithArtistIdFilter_ReturnsOnlyThatArtistGroups()
    {
        var factory = MockFactory();
        var ctx = await factory.CreateDbContextAsync();

        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        ctx.Libraries.Add(library);
        await ctx.SaveChangesAsync();

        var artist1 = new Artist
        {
            Name = "AC/DC",
            NameNormalized = "ACDC",
            Directory = "acdc",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        var artist2 = new Artist
        {
            Name = "AC DC",
            NameNormalized = "ACDC",
            Directory = "ac-dc",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        var artist3 = new Artist
        {
            Name = "Metallica",
            NameNormalized = "METALLICA",
            Directory = "metallica",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        ctx.Artists.Add(artist1);
        ctx.Artists.Add(artist2);
        ctx.Artists.Add(artist3);
        await ctx.SaveChangesAsync();

        var finder = new ArtistDuplicateFinder(Logger, MockFactory(), MockConfigurationFactory());
        var criteria = new ArtistDuplicateSearchCriteria(MinScore: 0.7, ArtistId: artist1.Id);

        var groups = await finder.FindDuplicatesAsync(criteria);

        foreach (var group in groups)
        {
            Assert.Contains(group.Artists, a => a.ArtistId == artist1.Id);
        }
    }

    [Fact]
    public async Task FindDuplicatesAsync_WithLimit_ReturnsLimitedResults()
    {
        var factory = MockFactory();
        var ctx = await factory.CreateDbContextAsync();

        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        ctx.Libraries.Add(library);
        await ctx.SaveChangesAsync();

        for (var i = 0; i < 10; i++)
        {
            var artist1 = new Artist
            {
                Name = $"Test Band {i}",
                NameNormalized = $"TESTBAND{i}",
                Directory = $"test-band-{i}",
                ApiKey = Guid.NewGuid(),
                LibraryId = library.Id,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };

            var artist2 = new Artist
            {
                Name = $"Testband {i}",
                NameNormalized = $"TESTBAND{i}",
                Directory = $"testband-{i}",
                ApiKey = Guid.NewGuid(),
                LibraryId = library.Id,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };

            ctx.Artists.Add(artist1);
            ctx.Artists.Add(artist2);
        }

        await ctx.SaveChangesAsync();

        var finder = new ArtistDuplicateFinder(Logger, MockFactory(), MockConfigurationFactory());
        var criteria = new ArtistDuplicateSearchCriteria(MinScore: 0.7, Limit: 3);

        var groups = await finder.FindDuplicatesAsync(criteria);

        Assert.True(groups.Count <= 3);
    }

    [Fact]
    public void ArtistReadModel_GetExternalIds_ReturnsValidIdsOnly()
    {
        var model = new ArtistReadModel(
            ArtistId: 1,
            ApiKey: Guid.NewGuid(),
            Name: "Test Artist",
            NameNormalized: "TESTARTIST",
            SortName: null,
            AlbumCount: 0,
            SongCount: 0,
            SpotifyId: "valid-spotify",
            MusicBrainzId: Guid.NewGuid(),
            DiscogsId: "0",
            ItunesId: null,
            DeezerId: 0,
            LastFmId: "",
            AmgId: "valid-amg",
            WikiDataId: null,
            Albums: []);

        var ids = model.GetExternalIds();

        Assert.Equal(3, ids.Count);
        Assert.Contains("spotify", ids.Keys);
        Assert.Contains("musicbrainz", ids.Keys);
        Assert.Contains("amg", ids.Keys);
        Assert.DoesNotContain("discogs", ids.Keys);
        Assert.DoesNotContain("itunes", ids.Keys);
        Assert.DoesNotContain("deezer", ids.Keys);
        Assert.DoesNotContain("lastfm", ids.Keys);
    }

    [Fact]
    public void ArtistReadModel_GetExternalIds_FiltersSentinelValues()
    {
        var model = new ArtistReadModel(
            ArtistId: 1,
            ApiKey: Guid.NewGuid(),
            Name: "Test Artist",
            NameNormalized: "TESTARTIST",
            SortName: null,
            AlbumCount: 0,
            SongCount: 0,
            SpotifyId: "-1",
            MusicBrainzId: Guid.Empty,
            DiscogsId: "unknown",
            ItunesId: "UNKNOWN",
            DeezerId: -1,
            LastFmId: "null",
            AmgId: "n/a",
            WikiDataId: "none",
            Albums: []);

        var ids = model.GetExternalIds();

        Assert.Empty(ids);
    }

    [Fact]
    public void ArtistReadModel_HasExternalIdForSource_ChecksCorrectly()
    {
        var model = new ArtistReadModel(
            ArtistId: 1,
            ApiKey: Guid.NewGuid(),
            Name: "Test Artist",
            NameNormalized: "TESTARTIST",
            SortName: null,
            AlbumCount: 0,
            SongCount: 0,
            SpotifyId: "valid-spotify",
            MusicBrainzId: null,
            DiscogsId: null,
            ItunesId: null,
            DeezerId: null,
            LastFmId: null,
            AmgId: null,
            WikiDataId: null,
            Albums: []);

        Assert.True(model.HasExternalIdForSource("spotify"));
        Assert.True(model.HasExternalIdForSource("Spotify"));
        Assert.False(model.HasExternalIdForSource("musicbrainz"));
        Assert.False(model.HasExternalIdForSource("unknown"));
    }

    [Fact]
    public void ArtistDuplicateSearchCriteria_DefaultValues_AreCorrect()
    {
        var criteria = new ArtistDuplicateSearchCriteria();

        Assert.Equal(0.7, criteria.MinScore);
        Assert.Null(criteria.Limit);
        Assert.Null(criteria.Source);
        Assert.Null(criteria.ArtistId);
        Assert.False(criteria.IncludeLowConfidence);
    }

    [Fact]
    public async Task FindDuplicatesAsync_WithSharedAlbums_ReturnsGroup()
    {
        var factory = MockFactory();
        var ctx = await factory.CreateDbContextAsync();

        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        ctx.Libraries.Add(library);
        await ctx.SaveChangesAsync();

        var artist1 = new Artist
        {
            Name = "Test Artist One",
            NameNormalized = "TESTARTIST ONE",
            Directory = "test-artist-one",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        var artist2 = new Artist
        {
            Name = "Test Artist 1",
            NameNormalized = "TESTARTIST 1",
            Directory = "test-artist-1",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        ctx.Artists.Add(artist1);
        ctx.Artists.Add(artist2);
        await ctx.SaveChangesAsync();

        var album1 = new Album
        {
            Name = "Debut Album",
            NameNormalized = "DEBUTALBUM",
            Directory = "debut-album",
            ApiKey = Guid.NewGuid(),
            ArtistId = artist1.Id,
            ReleaseDate = new NodaTime.LocalDate(2020, 1, 1),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        var album2 = new Album
        {
            Name = "Debut Album",
            NameNormalized = "DEBUTALBUM",
            Directory = "debut-album-2",
            ApiKey = Guid.NewGuid(),
            ArtistId = artist2.Id,
            ReleaseDate = new NodaTime.LocalDate(2020, 1, 1),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        ctx.Albums.Add(album1);
        ctx.Albums.Add(album2);
        await ctx.SaveChangesAsync();

        var finder = new ArtistDuplicateFinder(Logger, MockFactory(), MockConfigurationFactory());
        var criteria = new ArtistDuplicateSearchCriteria(MinScore: 0.5, IncludeLowConfidence: true);

        var groups = await finder.FindDuplicatesAsync(criteria);

        Assert.NotEmpty(groups);
        var group = groups.First();
        Assert.Equal(2, group.Artists.Count);
    }

    [Fact]
    public void ArtistReadModel_ComputePrimaryScore_PrefersMoreExternalIds()
    {
        var artistWithIds = new ArtistReadModel(
            ArtistId: 1,
            ApiKey: Guid.NewGuid(),
            Name: "Test Artist",
            NameNormalized: "TESTARTIST",
            SortName: null,
            AlbumCount: 5,
            SongCount: 50,
            SpotifyId: "spotify123",
            MusicBrainzId: Guid.NewGuid(),
            DiscogsId: "discogs456",
            ItunesId: null,
            DeezerId: null,
            LastFmId: null,
            AmgId: null,
            WikiDataId: null,
            Albums: []);

        var artistWithoutIds = new ArtistReadModel(
            ArtistId: 2,
            ApiKey: Guid.NewGuid(),
            Name: "Test Artist",
            NameNormalized: "TESTARTIST",
            SortName: null,
            AlbumCount: 5,
            SongCount: 50,
            SpotifyId: null,
            MusicBrainzId: null,
            DiscogsId: null,
            ItunesId: null,
            DeezerId: null,
            LastFmId: null,
            AmgId: null,
            WikiDataId: null,
            Albums: []);

        Assert.True(artistWithIds.ComputePrimaryScore() > artistWithoutIds.ComputePrimaryScore());
    }

    [Fact]
    public void ArtistReadModel_ComputePrimaryScore_PrefersProperCase()
    {
        var properCase = new ArtistReadModel(
            ArtistId: 1,
            ApiKey: Guid.NewGuid(),
            Name: "Miles Davis",
            NameNormalized: "MILESDAVIS",
            SortName: null,
            AlbumCount: 0,
            SongCount: 0,
            SpotifyId: null,
            MusicBrainzId: null,
            DiscogsId: null,
            ItunesId: null,
            DeezerId: null,
            LastFmId: null,
            AmgId: null,
            WikiDataId: null,
            Albums: []);

        var allCaps = new ArtistReadModel(
            ArtistId: 2,
            ApiKey: Guid.NewGuid(),
            Name: "MILES DAVIS",
            NameNormalized: "MILESDAVIS",
            SortName: null,
            AlbumCount: 0,
            SongCount: 0,
            SpotifyId: null,
            MusicBrainzId: null,
            DiscogsId: null,
            ItunesId: null,
            DeezerId: null,
            LastFmId: null,
            AmgId: null,
            WikiDataId: null,
            Albums: []);

        Assert.True(properCase.ComputePrimaryScore() > allCaps.ComputePrimaryScore());
    }

    [Fact]
    public void ArtistReadModel_ComputePrimaryScore_PrefersNonInvertedName()
    {
        var normalName = new ArtistReadModel(
            ArtistId: 1,
            ApiKey: Guid.NewGuid(),
            Name: "Miles Davis",
            NameNormalized: "MILESDAVIS",
            SortName: null,
            AlbumCount: 0,
            SongCount: 0,
            SpotifyId: null,
            MusicBrainzId: null,
            DiscogsId: null,
            ItunesId: null,
            DeezerId: null,
            LastFmId: null,
            AmgId: null,
            WikiDataId: null,
            Albums: []);

        var invertedName = new ArtistReadModel(
            ArtistId: 2,
            ApiKey: Guid.NewGuid(),
            Name: "Davis, Miles",
            NameNormalized: "DAVISMILES",
            SortName: null,
            AlbumCount: 0,
            SongCount: 0,
            SpotifyId: null,
            MusicBrainzId: null,
            DiscogsId: null,
            ItunesId: null,
            DeezerId: null,
            LastFmId: null,
            AmgId: null,
            WikiDataId: null,
            Albums: []);

        Assert.True(normalName.ComputePrimaryScore() > invertedName.ComputePrimaryScore());
    }

    [Fact]
    public async Task FindDuplicatesAsync_SuggestsPrimaryArtist()
    {
        var factory = MockFactory();
        var ctx = await factory.CreateDbContextAsync();

        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        ctx.Libraries.Add(library);
        await ctx.SaveChangesAsync();

        var artistWithIds = new Artist
        {
            Name = "Miles Davis",
            NameNormalized = "MILESDAVIS",
            Directory = "miles-davis",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            SpotifyId = "spotify123",
            MusicBrainzId = Guid.NewGuid(),
            AlbumCount = 10,
            SongCount = 100,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        var artistWithoutIds = new Artist
        {
            Name = "Davis, Miles",
            NameNormalized = "DAVISMILES",
            Directory = "davis-miles",
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            AlbumCount = 2,
            SongCount = 20,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        ctx.Artists.Add(artistWithIds);
        ctx.Artists.Add(artistWithoutIds);
        await ctx.SaveChangesAsync();

        var finder = new ArtistDuplicateFinder(Logger, MockFactory(), MockConfigurationFactory());
        var criteria = new ArtistDuplicateSearchCriteria(MinScore: 0.5, IncludeLowConfidence: true);

        var groups = await finder.FindDuplicatesAsync(criteria);

        Assert.NotEmpty(groups);
        var group = groups.First();
        Assert.Equal(artistWithIds.Id, group.SuggestedPrimaryArtistId);
    }
}
