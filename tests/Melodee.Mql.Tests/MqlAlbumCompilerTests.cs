using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Mql.Models;
using NodaTime;

namespace Melodee.Mql.Tests;

public class MqlAlbumCompilerTests
{
    private readonly MqlAlbumCompiler _compiler;
    private readonly MqlTokenizer _tokenizer;
    private readonly MqlParser _parser;

    public MqlAlbumCompilerTests()
    {
        _compiler = new MqlAlbumCompiler();
        _tokenizer = new MqlTokenizer();
        _parser = new MqlParser();
    }

    private Expression<Func<Album, bool>> CompileQuery(string query, int? userId = null)
    {
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "albums");
        parseResult.IsValid.Should().BeTrue($"Query '{query}' should parse successfully: {string.Join(", ", parseResult.Errors.Select(e => e.Message))}");
        parseResult.Ast.Should().NotBeNull();
        return _compiler.Compile(parseResult.Ast!, userId);
    }

    private static Album CreateAlbum(int id, string name, Artist artist, Library library)
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

    private static Library CreateLibrary(int id)
    {
        return new Library
        {
            Id = id,
            ApiKey = Guid.NewGuid(),
            Name = $"Library {id}",
            Path = $"/music/library{id}",
            Type = (int)LibraryType.Storage,
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };
    }

    [Fact]
    public void Compile_FreeTextQuery_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("Abbey Road");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "The Beatles", library);
        var matchingAlbum = CreateAlbum(1, "Abbey Road", artist, library);
        var nonMatchingAlbum = CreateAlbum(2, "Dark Side of the Moon", artist, library);

        compiled(matchingAlbum).Should().BeTrue();
        compiled(nonMatchingAlbum).Should().BeFalse();
    }

    [Fact]
    public void Compile_NotQuery_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("NOT live");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        var studioAlbum = CreateAlbum(1, "Studio Album", artist, library);
        var liveAlbum = CreateAlbum(2, "Live Album", artist, library);

        compiled(studioAlbum).Should().BeTrue();
        compiled(liveAlbum).Should().BeFalse();
    }

    [Fact]
    public void Compile_GenreArrayQuery_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("genre:Rock");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        var rockAlbum = CreateAlbum(1, "Rock Album", artist, library);
        rockAlbum.Genres = ["Rock", "Classic Rock"];
        var jazzAlbum = CreateAlbum(2, "Jazz Album", artist, library);
        jazzAlbum.Genres = ["Jazz"];

        compiled(rockAlbum).Should().BeTrue();
        compiled(jazzAlbum).Should().BeFalse();
    }

    [Fact]
    public void Compile_StarredFieldQuery_RequiresUserId()
    {
        var expression = CompileQuery("starred:true", userId: 1);
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        var album = CreateAlbum(1, "Test Album", artist, library);
        album.UserAlbums = new List<UserAlbum>
        {
            new UserAlbum { UserId = 1, IsStarred = true, AlbumId = 1, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) }
        };

        compiled(album).Should().BeTrue();
    }

    [Fact]
    public void Compile_StarredFieldQuery_WithoutUserId_ReturnsFalse()
    {
        var expression = CompileQuery("starred:true");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        var album = CreateAlbum(1, "Test Album", artist, library);

        compiled(album).Should().BeFalse();
    }

    [Fact]
    public void Compile_Query_ParsesSuccessfully()
    {
        var query = "artist:Beatles AND year:>=1970";
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "albums");

        parseResult.IsValid.Should().BeTrue();
        parseResult.Ast.Should().NotBeNull();
    }

    [Fact]
    public void Compile_FreeTextQuery_ParsesSuccessfully()
    {
        var query = "Abbey Road";
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "albums");

        parseResult.IsValid.Should().BeTrue();
        parseResult.Ast.Should().NotBeNull();
    }

    [Fact]
    public void Compile_YearLessThan_CompilesSuccessfully()
    {
        var expression = CompileQuery("year:<1980");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_YearLessThanOrEqual_CompilesSuccessfully()
    {
        var expression = CompileQuery("year:<=1980");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_YearGreaterThan_CompilesSuccessfully()
    {
        var expression = CompileQuery("year:>1980");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_YearRangeQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("year:1970-1980");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_NotEqualsQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("artist:!=Beatles");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_ContainsOperator_CompilesSuccessfully()
    {
        var expression = CompileQuery("album:contains Abbey");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_StartsWithOperator_CompilesSuccessfully()
    {
        var expression = CompileQuery("album:startsWith Abb");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_EndsWithOperator_CompilesSuccessfully()
    {
        var expression = CompileQuery("album:endsWith Road");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_PlaysLessThan_WithUserId_CompilesSuccessfully()
    {
        var expression = CompileQuery("plays:<10", userId: 1);
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_PlaysGreaterThan_WithUserId_CompilesSuccessfully()
    {
        var expression = CompileQuery("plays:>10", userId: 1);
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_AndQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("artist:Beatles AND year:>=1970");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_OrQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("artist:Beatles OR artist:Pink Floyd");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_ParenthesesQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("(artist:Beatles OR artist:Pink Floyd) AND year:>=1970");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_DurationGreaterThan_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("duration:>3000");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        var longAlbum = CreateAlbum(1, "Long Album", artist, library);
        longAlbum.Duration = 5000;
        var shortAlbum = CreateAlbum(2, "Short Album", artist, library);
        shortAlbum.Duration = 1000;

        compiled(longAlbum).Should().BeTrue();
        compiled(shortAlbum).Should().BeFalse();
    }

    [Fact]
    public void Compile_SongCountComparison_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("songcount:>=10");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        var largeAlbum = CreateAlbum(1, "Large Album", artist, library);
        largeAlbum.SongCount = 15;
        var smallAlbum = CreateAlbum(2, "Small Album", artist, library);
        smallAlbum.SongCount = 5;

        compiled(largeAlbum).Should().BeTrue();
        compiled(smallAlbum).Should().BeFalse();
    }

    [Fact]
    public void Compile_MoodArrayQuery_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("mood:Energetic");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        var energeticAlbum = CreateAlbum(1, "Energetic Album", artist, library);
        energeticAlbum.Moods = ["Energetic", "Upbeat"];
        var mellowAlbum = CreateAlbum(2, "Mellow Album", artist, library);
        mellowAlbum.Moods = ["Calm", "Relaxing"];

        compiled(energeticAlbum).Should().BeTrue();
        compiled(mellowAlbum).Should().BeFalse();
    }

    [Fact]
    public void Compile_RatingQuery_WithUserId_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("rating:>=4", userId: 1);
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        var highRatedAlbum = CreateAlbum(1, "High Rated Album", artist, library);
        highRatedAlbum.UserAlbums = new List<UserAlbum>
        {
            new UserAlbum { UserId = 1, Rating = 5, AlbumId = 1, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) }
        };
        var lowRatedAlbum = CreateAlbum(2, "Low Rated Album", artist, library);
        lowRatedAlbum.UserAlbums = new List<UserAlbum>
        {
            new UserAlbum { UserId = 1, Rating = 2, AlbumId = 2, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) }
        };

        compiled(highRatedAlbum).Should().BeTrue();
        compiled(lowRatedAlbum).Should().BeFalse();
    }

    [Fact]
    public void Compile_RatingQuery_WithoutUserId_ReturnsFalse()
    {
        var expression = CompileQuery("rating:>=4");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        var album = CreateAlbum(1, "Test Album", artist, library);
        album.UserAlbums = new List<UserAlbum>
        {
            new UserAlbum { UserId = 1, Rating = 5, AlbumId = 1, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) }
        };

        compiled(album).Should().BeFalse();
    }

    [Fact]
    public void Compile_DurationRangeQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("duration:2000-5000");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        var midAlbum = CreateAlbum(1, "Mid Duration Album", artist, library);
        midAlbum.Duration = 3500;
        var shortAlbum = CreateAlbum(2, "Short Album", artist, library);
        shortAlbum.Duration = 1000;

        compiled(midAlbum).Should().BeTrue();
        compiled(shortAlbum).Should().BeFalse();
    }

    [Fact]
    public void Compile_FreeTextSearch_MatchesArtistName()
    {
        var expression = CompileQuery("Beatles");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var beatles = CreateArtist(1, "The Beatles", library);
        var floyd = CreateArtist(2, "Pink Floyd", library);
        var beatlesAlbum = CreateAlbum(1, "Abbey Road", beatles, library);
        var floydAlbum = CreateAlbum(2, "The Wall", floyd, library);

        compiled(beatlesAlbum).Should().BeTrue();
        compiled(floydAlbum).Should().BeFalse();
    }

    [Fact]
    public void Compile_ComplexBooleanExpression_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("(genre:Rock OR genre:Jazz) AND year:>=1970");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        
        var rockAlbum1980 = CreateAlbum(1, "Rock 1980", artist, library);
        rockAlbum1980.Genres = ["Rock"];
        rockAlbum1980.ReleaseDate = new LocalDate(1980, 1, 1);
        
        var jazzAlbum1960 = CreateAlbum(2, "Jazz 1960", artist, library);
        jazzAlbum1960.Genres = ["Jazz"];
        jazzAlbum1960.ReleaseDate = new LocalDate(1960, 1, 1);

        compiled(rockAlbum1980).Should().BeTrue();
        compiled(jazzAlbum1960).Should().BeFalse();
    }
}
