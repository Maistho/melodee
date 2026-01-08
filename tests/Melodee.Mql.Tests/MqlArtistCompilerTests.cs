using System.Linq.Expressions;
using FluentAssertions;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Mql.Tests;

public class MqlArtistCompilerTests
{
    private readonly MqlArtistCompiler _compiler;
    private readonly MqlTokenizer _tokenizer;
    private readonly MqlParser _parser;

    public MqlArtistCompilerTests()
    {
        _compiler = new MqlArtistCompiler();
        _tokenizer = new MqlTokenizer();
        _parser = new MqlParser();
    }

    private Expression<Func<Artist, bool>> CompileQuery(string query, int? userId = null)
    {
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "artists");
        parseResult.IsValid.Should().BeTrue($"Query '{query}' should parse successfully: {string.Join(", ", parseResult.Errors.Select(e => e.Message))}");
        parseResult.Ast.Should().NotBeNull();
        return _compiler.Compile(parseResult.Ast!, userId);
    }

    private static Artist CreateArtist(int id, string name, Library library)
    {
        return new Artist
        {
            Id = id,
            ApiKey = Guid.NewGuid(),
            Directory = name.Replace(" ", "").ToLowerInvariant(),
            Name = name,
            NameNormalized = name.ToNormalizedString() ?? name.ToUpperInvariant(),
            LibraryId = library.Id,
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0),
            SongCount = 10,
            AlbumCount = 3,
            PlayedCount = 100
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
        var expression = CompileQuery("Pink Floyd");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var matchingArtist = CreateArtist(1, "Pink Floyd", library);
        var nonMatchingArtist = CreateArtist(2, "The Beatles", library);

        compiled(matchingArtist).Should().BeTrue();
        compiled(nonMatchingArtist).Should().BeFalse();
    }

    [Fact]
    public void Compile_NotQuery_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("NOT unknown");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var knownArtist = CreateArtist(1, "Famous Artist", library);
        var unknownArtist = CreateArtist(2, "Unknown Artist", library);

        compiled(knownArtist).Should().BeTrue();
        compiled(unknownArtist).Should().BeFalse();
    }

    [Fact]
    public void Compile_StarredFieldQuery_RequiresUserId()
    {
        var expression = CompileQuery("starred:true", userId: 1);
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        artist.UserArtists = new List<UserArtist>
        {
            new UserArtist { UserId = 1, IsStarred = true, ArtistId = 1, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) }
        };

        compiled(artist).Should().BeTrue();
    }

    [Fact]
    public void Compile_StarredFieldQuery_WithoutUserId_ReturnsFalse()
    {
        var expression = CompileQuery("starred:true");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);

        compiled(artist).Should().BeFalse();
    }

    [Fact]
    public void Compile_RatingFieldQuery_RequiresUserId()
    {
        var expression = CompileQuery("rating:>=4", userId: 1);
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        artist.UserArtists = new List<UserArtist>
        {
            new UserArtist { UserId = 1, Rating = 4, ArtistId = 1, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) }
        };

        compiled(artist).Should().BeTrue();
    }

    [Fact]
    public void Compile_RatingFieldQuery_WithoutUserId_ReturnsFalse()
    {
        var expression = CompileQuery("rating:>=4");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);

        compiled(artist).Should().BeFalse();
    }

    [Fact]
    public void Compile_Query_ParsesSuccessfully()
    {
        var query = "artist:Beatles AND plays:>50";
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "artists");

        parseResult.IsValid.Should().BeTrue();
        parseResult.Ast.Should().NotBeNull();
    }

    [Fact]
    public void Compile_FreeTextQuery_ParsesSuccessfully()
    {
        var query = "Pink Floyd";
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "artists");

        parseResult.IsValid.Should().BeTrue();
        parseResult.Ast.Should().NotBeNull();
    }

    [Fact]
    public void Compile_NumericComparison_ParsesSuccessfully()
    {
        var query = "plays:>50";
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "artists");

        parseResult.IsValid.Should().BeTrue();
        parseResult.Ast.Should().NotBeNull();
    }

    [Fact]
    public void Compile_PlaysLessThan_CompilesSuccessfully()
    {
        var expression = CompileQuery("plays:<50");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_PlaysLessThanOrEqual_CompilesSuccessfully()
    {
        var expression = CompileQuery("plays:<=50");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_PlaysGreaterThan_CompilesSuccessfully()
    {
        var expression = CompileQuery("plays:>50");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_SongCountRangeQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("songcount:5-15");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_AlbumCountGreaterThan_CompilesSuccessfully()
    {
        var expression = CompileQuery("albumcount:>2");
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
        var expression = CompileQuery("artist:contains Floyd");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_StartsWithOperator_CompilesSuccessfully()
    {
        var expression = CompileQuery("artist:startsWith Pink");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_EndsWithOperator_CompilesSuccessfully()
    {
        var expression = CompileQuery("artist:endsWith Floyd");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_AndQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("plays:>50 AND albumcount:>=2");
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
        var expression = CompileQuery("(artist:Beatles OR artist:Pink Floyd) AND plays:>0");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_RatingGreaterThan_WithUserId_CompilesSuccessfully()
    {
        var expression = CompileQuery("rating:>3", userId: 1);
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_PlaysGreaterThan_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("plays:>50");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var popularArtist = CreateArtist(1, "Popular Artist", library);
        popularArtist.PlayedCount = 100;
        var newArtist = CreateArtist(2, "New Artist", library);
        newArtist.PlayedCount = 10;

        compiled(popularArtist).Should().BeTrue();
        compiled(newArtist).Should().BeFalse();
    }

    [Fact]
    public void Compile_SongCountGreaterThan_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("songcount:>5");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var prolificArtist = CreateArtist(1, "Prolific Artist", library);
        prolificArtist.SongCount = 50;
        var newArtist = CreateArtist(2, "New Artist", library);
        newArtist.SongCount = 2;

        compiled(prolificArtist).Should().BeTrue();
        compiled(newArtist).Should().BeFalse();
    }

    [Fact]
    public void Compile_AlbumCountGreaterThan_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("albumcount:>2");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var prolificArtist = CreateArtist(1, "Prolific Artist", library);
        prolificArtist.AlbumCount = 10;
        var newArtist = CreateArtist(2, "New Artist", library);
        newArtist.AlbumCount = 1;

        compiled(prolificArtist).Should().BeTrue();
        compiled(newArtist).Should().BeFalse();
    }

    [Fact]
    public void Compile_SongCountRangeQuery_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("songcount:5-20");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var midRangeArtist = CreateArtist(1, "Mid Range Artist", library);
        midRangeArtist.SongCount = 10;
        var highCountArtist = CreateArtist(2, "High Count Artist", library);
        highCountArtist.SongCount = 50;
        var lowCountArtist = CreateArtist(3, "Low Count Artist", library);
        lowCountArtist.SongCount = 2;

        compiled(midRangeArtist).Should().BeTrue();
        compiled(highCountArtist).Should().BeFalse();
        compiled(lowCountArtist).Should().BeFalse();
    }

    [Fact]
    public void Compile_PlaysRangeQuery_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("plays:10-100");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var moderateArtist = CreateArtist(1, "Moderate Artist", library);
        moderateArtist.PlayedCount = 50;
        var veryPopularArtist = CreateArtist(2, "Very Popular Artist", library);
        veryPopularArtist.PlayedCount = 200;
        var newArtist = CreateArtist(3, "New Artist", library);
        newArtist.PlayedCount = 5;

        compiled(moderateArtist).Should().BeTrue();
        compiled(veryPopularArtist).Should().BeFalse();
        compiled(newArtist).Should().BeFalse();
    }

    [Fact]
    public void Compile_ArtistNameEquals_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("artist:\"Pink Floyd\"");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var pinkFloyd = CreateArtist(1, "Pink Floyd", library);
        var beatles = CreateArtist(2, "The Beatles", library);

        compiled(pinkFloyd).Should().BeTrue();
        compiled(beatles).Should().BeFalse();
    }

    [Fact]
    public void Compile_ComplexBooleanExpression_ReturnsCorrectExpression()
    {
        var expression = CompileQuery("plays:>50 AND songcount:>=5");
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var establishedArtist = CreateArtist(1, "Established Artist", library);
        establishedArtist.PlayedCount = 100;
        establishedArtist.SongCount = 20;

        var newPopularArtist = CreateArtist(2, "New Popular Artist", library);
        newPopularArtist.PlayedCount = 100;
        newPopularArtist.SongCount = 2;

        compiled(establishedArtist).Should().BeTrue();
        compiled(newPopularArtist).Should().BeFalse();
    }

    [Fact]
    public void Compile_StarredWithDifferentUserId_ReturnsFalse()
    {
        var expression = CompileQuery("starred:true", userId: 2);
        var compiled = expression.Compile();

        var library = CreateLibrary(1);
        var artist = CreateArtist(1, "Test Artist", library);
        artist.UserArtists = new List<UserArtist>
        {
            new UserArtist { UserId = 1, IsStarred = true, ArtistId = 1, CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0) }
        };

        compiled(artist).Should().BeFalse();
    }
}
