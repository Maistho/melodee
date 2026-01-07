using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Mql.Models;
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
            NameNormalized = name.ToUpperInvariant(),
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
}
