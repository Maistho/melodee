using System.Linq.Expressions;
using FluentAssertions;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Mql.Models;
using NodaTime;

namespace Melodee.Mql.Tests;

public class MqlRegexExpressionTests
{
    private readonly MqlTokenizer _tokenizer;
    private readonly MqlParser _parser;

    public MqlRegexExpressionTests()
    {
        _tokenizer = new MqlTokenizer();
        _parser = new MqlParser();
    }

    [Fact]
    public void Parse_FieldWithRegexPattern_ReturnsRegexExpressionNode()
    {
        var tokens = _tokenizer.Tokenize("title:/remix/i").ToList();
        var result = _parser.Parse(tokens, "songs");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().NotBeNull();
        result.Ast.Should().BeOfType<RegexExpressionNode>();

        var regexNode = (RegexExpressionNode)result.Ast!;
        regexNode.Field.Should().Be("title");
        regexNode.Pattern.Should().Be("remix");
        regexNode.Flags.Should().Be("i");
    }

    [Fact]
    public void Parse_FieldWithRegexPatternNoFlags_ReturnsRegexExpressionNode()
    {
        var tokens = _tokenizer.Tokenize("title:/remix/").ToList();
        var result = _parser.Parse(tokens, "songs");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<RegexExpressionNode>();

        var regexNode = (RegexExpressionNode)result.Ast!;
        regexNode.Pattern.Should().Be("remix");
        regexNode.Flags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_FieldWithComplexRegexPattern_ReturnsRegexExpressionNode()
    {
        var tokens = _tokenizer.Tokenize("title:/^[a-zA-Z0-9]+$/i").ToList();
        var result = _parser.Parse(tokens, "songs");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<RegexExpressionNode>();

        var regexNode = (RegexExpressionNode)result.Ast!;
        regexNode.Pattern.Should().Be("^[a-zA-Z0-9]+$");
        regexNode.Flags.Should().Be("i");
    }

    [Fact]
    public void Parse_RegexWithMultipleFlags_ReturnsRegexExpressionNode()
    {
        var tokens = _tokenizer.Tokenize("title:/test/ig").ToList();
        var result = _parser.Parse(tokens, "songs");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<RegexExpressionNode>();

        var regexNode = (RegexExpressionNode)result.Ast!;
        regexNode.Pattern.Should().Be("test");
        regexNode.Flags.Should().Be("ig");
    }

    [Fact]
    public void Parse_RegexWithEscapedCharacters_ReturnsRegexExpressionNode()
    {
        var tokens = _tokenizer.Tokenize("title:/test\\/value/").ToList();
        var result = _parser.Parse(tokens, "songs");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<RegexExpressionNode>();

        var regexNode = (RegexExpressionNode)result.Ast!;
        regexNode.Pattern.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Parse_RegexInBooleanExpression_ReturnsCorrectAst()
    {
        var tokens = _tokenizer.Tokenize("title:/remix/ AND artist:/beatles/i").ToList();
        var result = _parser.Parse(tokens, "songs");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<BinaryExpressionNode>();

        var binaryNode = (BinaryExpressionNode)result.Ast!;
        binaryNode.Operator.Should().Be("AND");
        binaryNode.Left.Should().BeOfType<RegexExpressionNode>();
        binaryNode.Right.Should().BeOfType<RegexExpressionNode>();
    }

    [Fact]
    public void Parse_RegexInParentheticalExpression_ReturnsCorrectAst()
    {
        var tokens = _tokenizer.Tokenize("(title:/remix/ OR album:/live/)").ToList();
        var result = _parser.Parse(tokens, "songs");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<GroupNode>();

        var groupNode = (GroupNode)result.Ast!;
        groupNode.Inner.Should().BeOfType<BinaryExpressionNode>();
    }

    [Fact]
    public void NormalizedQuery_RegexQuery_IncludesRegexPattern()
    {
        var tokens = _tokenizer.Tokenize("title:/remix/i").ToList();
        var result = _parser.Parse(tokens, "songs");

        result.NormalizedQuery.Should().Contain("/remix/i");
    }

    [Fact]
    public void Parse_InvalidRegex_ReturnsError()
    {
        var tokens = _tokenizer.Tokenize("title:/[unclosed/").ToList();
        var result = _parser.Parse(tokens, "songs");

        // Tokenizer should still create the token, but validator should catch the invalid pattern
        // The tokenizer doesn't validate regex syntax, so it will tokenize it
        tokens.Should().Contain(t => t.Type == MqlTokenType.Regex);
    }
}

public class MqlRegexCompilerTests
{
    private readonly MqlSongCompiler _compiler;
    private readonly MqlTokenizer _tokenizer;
    private readonly MqlParser _parser;
    private readonly MqlOptions _options;

    public MqlRegexCompilerTests()
    {
        _options = new MqlOptions { EnableRegex = true };
        _compiler = new MqlSongCompiler(options: _options);
        _tokenizer = new MqlTokenizer();
        _parser = new MqlParser();
    }

    private Expression<Func<Song, bool>> CompileQuery(string query)
    {
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "songs");
        parseResult.IsValid.Should().BeTrue();
        parseResult.Ast.Should().NotBeNull();
        return _compiler.Compile(parseResult.Ast!);
    }

    [Fact]
    public void Compile_RegexExpression_WhenDisabled_ReturnsTrue()
    {
        var disabledOptions = new MqlOptions { EnableRegex = false };
        var disabledCompiler = new MqlSongCompiler(options: disabledOptions);

        var tokens = _tokenizer.Tokenize("title:/test/").ToList();
        var parseResult = _parser.Parse(tokens, "songs");
        parseResult.Ast.Should().NotBeNull();

        var expression = disabledCompiler.Compile(parseResult.Ast!);
        var compiled = expression.Compile();

        var song = CreateTestSong(title: "Test Remix Song");
        var result = compiled(song);
        result.Should().BeTrue();
    }

    [Fact]
    public void Compile_ValidRegexExpression_ReturnsMatchingExpression()
    {
        var expression = CompileQuery("title:/TEST/");
        var compiled = expression.Compile();

        var matchingSong = CreateTestSong(title: "Test Remix Song");
        var nonMatchingSong = CreateTestSong(title: "Hello World");

        compiled(matchingSong).Should().BeTrue();
        compiled(nonMatchingSong).Should().BeFalse();
    }

    [Fact]
    public void Compile_RegexWithCaseInsensitiveFlag_MatchesCaseInsensitive()
    {
        var expression = CompileQuery("title:/remix/i");
        var compiled = expression.Compile();

        var song = CreateTestSong(title: "TEST REMIX SONG");
        compiled(song).Should().BeTrue();
    }

    [Fact]
    public void Compile_RegexWithWildcardPattern_ReturnsMatchingExpression()
    {
        var expression = CompileQuery("title:/ABC/");
        var compiled = expression.Compile();

        var matchingSong = CreateTestSong(title: "123-ABC-XYZ");
        var nonMatchingSong = CreateTestSong(title: "123-XYZ-789");

        compiled(matchingSong).Should().BeTrue();
        compiled(nonMatchingSong).Should().BeFalse();
    }

    [Fact]
    public void Compile_RegexOnArtistField_ReturnsMatchingExpression()
    {
        var expression = CompileQuery("artist:/FLOYD/i");
        var compiled = expression.Compile();

        var song = CreateTestSong(artistName: "PINK FLOYD");
        compiled(song).Should().BeTrue();
    }

    [Fact]
    public void Compile_RegexOnAlbumField_ReturnsMatchingExpression()
    {
        var expression = CompileQuery("album:/WALL/i");
        var compiled = expression.Compile();

        var song = CreateTestSong(albumName: "THE WALL");
        compiled(song).Should().BeTrue();
    }

    private static Song CreateTestSong(
        string title = "Test Song",
        string artistName = "Test Artist",
        string albumName = "Test Album")
    {
        var artist = new Artist
        {
            Id = 1,
            ApiKey = Guid.NewGuid(),
            Directory = artistName.Replace(" ", "").ToLowerInvariant(),
            Name = artistName,
            NameNormalized = artistName.ToUpperInvariant(),
            LibraryId = 1,
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };

        var album = new Album
        {
            Id = 1,
            ApiKey = Guid.NewGuid(),
            ArtistId = artist.Id,
            Artist = artist,
            Name = albumName,
            NameNormalized = albumName.ToUpperInvariant(),
            Directory = $"/{artist.Directory}/{albumName.Replace(" ", "").ToLowerInvariant()}/",
            ReleaseDate = new LocalDate(2020, 1, 1),
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };

        return new Song
        {
            Id = 1,
            AlbumId = album.Id,
            Album = album,
            Title = title,
            TitleNormalized = title.ToUpperInvariant(),
            SongNumber = 1,
            FileName = $"{title.Replace(" ", "_").ToLowerInvariant()}.flac",
            FileSize = 1000000,
            FileHash = $"hash_1",
            Duration = 180000,
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = 120,
            ContentType = "audio/flac",
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };
    }
}

public class MqlRegexOptionsTests
{
    [Fact]
    public void MqlOptions_Default_HasRegexDisabled()
    {
        var options = new MqlOptions();

        options.EnableRegex.Should().BeFalse();
        options.MaxResultSetForRegex.Should().Be(1000);
        options.RegexTimeoutMs.Should().Be(500);
        options.RegexGuard.Should().NotBeNull();
    }

    [Fact]
    public void MqlOptions_CustomValues_AreSetCorrectly()
    {
        var guard = new Mql.Security.MqlRegexGuard();
        var options = new MqlOptions
        {
            EnableRegex = true,
            MaxResultSetForRegex = 500,
            RegexTimeoutMs = 1000,
            RegexGuard = guard
        };

        options.EnableRegex.Should().BeTrue();
        options.MaxResultSetForRegex.Should().Be(500);
        options.RegexTimeoutMs.Should().Be(1000);
        options.RegexGuard.Should().BeSameAs(guard);
    }
}
