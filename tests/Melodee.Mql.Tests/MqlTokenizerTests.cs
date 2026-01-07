using FluentAssertions;
using Melodee.Mql.Models;

namespace Melodee.Mql.Tests;

public class MqlTokenizerTests
{
    private readonly MqlTokenizer _tokenizer;

    public MqlTokenizerTests()
    {
        _tokenizer = new MqlTokenizer();
    }

    [Fact]
    public void Tokenize_EmptyQuery_ReturnsEndOfInput()
    {
        var tokens = _tokenizer.Tokenize("").ToList();

        tokens.Should().ContainSingle();
        tokens[0].Type.Should().Be(MqlTokenType.EndOfInput);
    }

    [Fact]
    public void Tokenize_NullQuery_ReturnsEndOfInput()
    {
        var tokens = _tokenizer.Tokenize(null!).ToList();

        tokens.Should().ContainSingle();
        tokens[0].Type.Should().Be(MqlTokenType.EndOfInput);
    }

    [Fact]
    public void Tokenize_SimpleFieldValuePair_ReturnsFieldAndValueTokens()
    {
        var tokens = _tokenizer.Tokenize("artist:Beatles").ToList();

        tokens.Should().HaveCount(4); // artist, :, Beatles, EOF
        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("artist");
        tokens[1].Type.Should().Be(MqlTokenType.Operator);
        tokens[1].Value.Should().Be(":");
        tokens[2].Type.Should().Be(MqlTokenType.FreeText);
        tokens[2].Value.Should().Be("Beatles");
        tokens[3].Type.Should().Be(MqlTokenType.EndOfInput);
    }

    [Fact]
    public void Tokenize_QuotedString_ReturnsStringLiteral()
    {
        var tokens = _tokenizer.Tokenize("artist:\"Pink Floyd\"").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.StringLiteral);
        tokens[2].Value.Should().Be("Pink Floyd");
    }

    [Fact]
    public void Tokenize_QuotedStringWithEscapes_HandlesEscapes()
    {
        var tokens = _tokenizer.Tokenize("title:\"Test\\\"Song\"").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.StringLiteral);
        tokens[2].Value.Should().Be("Test\"Song");
    }

    [Fact]
    public void Tokenize_AndOperator_ReturnsAndToken()
    {
        var tokens = _tokenizer.Tokenize("artist:Beatles AND year:1970").ToList();

        tokens.Should().Contain(t => t.Type == MqlTokenType.And);
        tokens.First(t => t.Type == MqlTokenType.And).Value.Should().Be("AND");
    }

    [Fact]
    public void Tokenize_OrOperator_ReturnsOrToken()
    {
        var tokens = _tokenizer.Tokenize("artist:Beatles OR artist:Pink Floyd").ToList();

        tokens.Should().Contain(t => t.Type == MqlTokenType.Or);
    }

    [Fact]
    public void Tokenize_NotOperator_ReturnsNotToken()
    {
        var tokens = _tokenizer.Tokenize("NOT live").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.Not);
        tokens[0].Value.Should().Be("NOT");
    }

    [Fact]
    public void Tokenize_Parentheses_ReturnsParenTokens()
    {
        var tokens = _tokenizer.Tokenize("(artist:Beatles)").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.LeftParen);
        tokens[0].Value.Should().Be("(");
        tokens[1].Type.Should().Be(MqlTokenType.FieldName);
        tokens[1].Value.Should().Be("artist");
        tokens[4].Type.Should().Be(MqlTokenType.RightParen);
        tokens[4].Value.Should().Be(")");
    }

    [Fact]
    public void Tokenize_ComparisonOperator_ReturnsOperator()
    {
        var tokens = _tokenizer.Tokenize("year:>=2000").ToList();

        tokens[1].Type.Should().Be(MqlTokenType.Operator);
        tokens[1].Value.Should().Be(":>=");
    }

    [Theory]
    [InlineData(":=", "Equals")]
    [InlineData(":!=", "NotEquals")]
    [InlineData(":<", "LessThan")]
    [InlineData(":<=", "LessThanOrEquals")]
    [InlineData(":>", "GreaterThan")]
    [InlineData(":>=", "GreaterThanOrEquals")]
    public void Tokenize_AllComparisonOperators_ReturnsCorrectOperator(string op, string _)
    {
        var tokens = _tokenizer.Tokenize($"year{op}2000").ToList();

        tokens[1].Type.Should().Be(MqlTokenType.Operator);
        tokens[1].Value.Should().Be(op);
    }

    [Fact]
    public void Tokenize_RangeExpression_ReturnsRangeToken()
    {
        var tokens = _tokenizer.Tokenize("year:1970-1980").ToList();

        var rangeToken = tokens.Should().Contain(t => t.Type == MqlTokenType.Range).Subject;
        rangeToken.Value.Should().Be("1970-1980");
    }

    [Fact]
    public void Tokenize_IntegerNumber_ReturnsNumberLiteral()
    {
        var tokens = _tokenizer.Tokenize("rating:5").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.NumberLiteral);
        tokens[2].Value.Should().Be("5");
    }

    [Fact]
    public void Tokenize_DecimalNumber_ReturnsNumberLiteral()
    {
        var tokens = _tokenizer.Tokenize("rating:4.5").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.NumberLiteral);
        tokens[2].Value.Should().Be("4.5");
    }

    [Fact]
    public void Tokenize_DateLiteral_ReturnsDateLiteral()
    {
        var tokens = _tokenizer.Tokenize("added:2026-01-06").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.DateLiteral);
        tokens[2].Value.Should().Be("2026-01-06");
    }

    [Fact]
    public void Tokenize_RelativeDateToday_ReturnsDateLiteral()
    {
        var tokens = _tokenizer.Tokenize("added:today").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.DateLiteral);
        tokens[2].Value.Should().Be("today");
    }

    [Fact]
    public void Tokenize_RelativeDateYesterday_ReturnsDateLiteral()
    {
        var tokens = _tokenizer.Tokenize("added:yesterday").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.DateLiteral);
        tokens[2].Value.Should().Be("yesterday");
    }

    [Fact]
    public void Tokenize_RelativeDateLastWeek_ReturnsDateLiteral()
    {
        var tokens = _tokenizer.Tokenize("added:last-week").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.DateLiteral);
        tokens[2].Value.Should().Be("last-week");
    }

    [Fact]
    public void Tokenize_RelativeDateWithSuffix_ReturnsDateLiteral()
    {
        var tokens = _tokenizer.Tokenize("added:-7d").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.DateLiteral);
        tokens[2].Value.Should().Be("-7d");
    }

    [Fact]
    public void Tokenize_BooleanTrue_ReturnsBooleanLiteral()
    {
        var tokens = _tokenizer.Tokenize("starred:true").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.BooleanLiteral);
        tokens[2].Value.Should().Be("true");
    }

    [Fact]
    public void Tokenize_BooleanFalse_ReturnsBooleanLiteral()
    {
        var tokens = _tokenizer.Tokenize("starred:false").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.BooleanLiteral);
        tokens[2].Value.Should().Be("false");
    }

    [Fact]
    public void Tokenize_BooleanKeywords_CaseInsensitive()
    {
        var andTokens = _tokenizer.Tokenize("and").ToList();
        andTokens[0].Type.Should().Be(MqlTokenType.And);
        andTokens[0].Value.Should().Be("and");

        var orTokens = _tokenizer.Tokenize("OR").ToList();
        orTokens[0].Type.Should().Be(MqlTokenType.Or);

        var notTokens = _tokenizer.Tokenize("Not").ToList();
        notTokens[0].Type.Should().Be(MqlTokenType.Not);
    }

    [Fact]
    public void Tokenize_RegexPattern_ReturnsRegexToken()
    {
        var tokens = _tokenizer.Tokenize("title:/remix/i").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.Regex);
        tokens[2].Value.Should().Be("/remix/i");
    }

    [Fact]
    public void Tokenize_RegexPatternWithoutFlags_ReturnsRegexToken()
    {
        var tokens = _tokenizer.Tokenize("title:/remix/").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.Regex);
        tokens[2].Value.Should().Be("/remix/");
    }

    [Fact]
    public void Tokenize_FreeText_ReturnsFreeTextToken()
    {
        var tokens = _tokenizer.Tokenize("pink floyd").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FreeText);
        tokens[0].Value.Should().Be("pink");
        tokens[1].Type.Should().Be(MqlTokenType.FreeText);
        tokens[1].Value.Should().Be("floyd");
    }

    [Fact]
    public void Tokenize_MixedQuery_ReturnsCorrectSequence()
    {
        var tokens = _tokenizer.Tokenize("artist:\"Pink Floyd\" AND year:>=1970 NOT live").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[1].Type.Should().Be(MqlTokenType.Operator);
        tokens[2].Type.Should().Be(MqlTokenType.StringLiteral);
        tokens[3].Type.Should().Be(MqlTokenType.And);
        tokens[4].Type.Should().Be(MqlTokenType.FieldName);
        tokens[5].Type.Should().Be(MqlTokenType.Operator);
        tokens[6].Type.Should().Be(MqlTokenType.NumberLiteral);
        tokens[7].Type.Should().Be(MqlTokenType.Not);
        tokens[8].Type.Should().Be(MqlTokenType.FreeText);
        tokens[9].Type.Should().Be(MqlTokenType.EndOfInput);
    }

    [Fact]
    public void Tokenize_PositionTracking_TracksCorrectPositions()
    {
        var tokens = _tokenizer.Tokenize("artist:Beatles").ToList();

        tokens[0].StartPosition.Should().Be(0);
        tokens[0].EndPosition.Should().Be(6);
        tokens[0].Column.Should().Be(1);
        tokens[1].StartPosition.Should().Be(6);
        tokens[1].Column.Should().Be(7);
        tokens[2].StartPosition.Should().Be(7);
        tokens[2].Column.Should().Be(8);
    }

    [Fact]
    public void Tokenize_PositionTracking_TracksNewlines()
    {
        var tokens = _tokenizer.Tokenize("artist:\nBeatles").ToList();

        tokens[0].Line.Should().Be(1);
        tokens[0].Column.Should().Be(1);
        tokens[1].Line.Should().Be(1);
        tokens[2].Line.Should().Be(2);
        tokens[2].Column.Should().Be(1);
    }

    [Fact]
    public void Tokenize_Whitespace_SkipsWhitespace()
    {
        var tokens = _tokenizer.Tokenize("artist   :   Beatles").ToList();

        tokens[0].Value.Should().Be("artist");
        tokens[1].Value.Should().Be(":");
        tokens[2].Value.Should().Be("Beatles");
    }

    [Fact]
    public void Tokenize_UnclosedQuote_ReturnsStringLiteral()
    {
        var tokens = _tokenizer.Tokenize("title:\"unclosed").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.StringLiteral);
        tokens[2].Value.Should().Be("unclosed");
    }

    [Fact]
    public void Tokenize_NegativeNumber_ReturnsNumberLiteral()
    {
        var tokens = _tokenizer.Tokenize("rating:-5").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.NumberLiteral);
        tokens[2].Value.Should().Be("-5");
    }

    [Fact]
    public void Tokenize_ComplexQuery_ReturnsAllTokens()
    {
        var query = "(artist:Beatles OR artist:Pink Floyd) AND year:1970-1980 AND rating:>=4 NOT live";
        var tokens = _tokenizer.Tokenize(query).ToList();

        tokens.Should().NotBeEmpty();
        tokens.Last().Type.Should().Be(MqlTokenType.EndOfInput);

        // Verify key tokens are present
        tokens.Should().Contain(t => t.Type == MqlTokenType.LeftParen);
        tokens.Should().Contain(t => t.Type == MqlTokenType.RightParen);
        tokens.Should().Contain(t => t.Type == MqlTokenType.Or);
        tokens.Should().Contain(t => t.Type == MqlTokenType.And);
        tokens.Should().Contain(t => t.Type == MqlTokenType.Not);
        tokens.Should().Contain(t => t.Type == MqlTokenType.Range);
    }

    [Fact]
    public void Tokenize_FieldWithSpecialCharsInQuotes_ReturnsStringLiteral()
    {
        var tokens = _tokenizer.Tokenize("title:\"Test (Remastered)\"").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.StringLiteral);
        tokens[2].Value.Should().Be("Test (Remastered)");
    }

    [Fact]
    public void Tokenize_WildcardCharacter_ReturnsFreeText()
    {
        var tokens = _tokenizer.Tokenize("test*value").ToList();

        // Wildcard in free text is just part of the token
        tokens[0].Type.Should().Be(MqlTokenType.FreeText);
        tokens[0].Value.Should().Be("test*value");
    }

    [Fact]
    public void Tokenize_DiscNumberField_ReturnsFieldName()
    {
        var tokens = _tokenizer.Tokenize("discnumber:1").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("discnumber");
    }

    [Fact]
    public void Tokenize_TrackNumberField_ReturnsFieldName()
    {
        var tokens = _tokenizer.Tokenize("tracknumber:5").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("tracknumber");
    }

    [Fact]
    public void Tokenize_CommentField_ReturnsFieldName()
    {
        var tokens = _tokenizer.Tokenize("comment:test").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("comment");
    }

    [Fact]
    public void Tokenize_GenreField_ReturnsFieldName()
    {
        var tokens = _tokenizer.Tokenize("genre:Rock").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("genre");
    }

    [Fact]
    public void Tokenize_MoodField_ReturnsFieldName()
    {
        var tokens = _tokenizer.Tokenize("mood:Chill").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("mood");
    }

    [Fact]
    public void Tokenize_BpmField_ReturnsFieldName()
    {
        var tokens = _tokenizer.Tokenize("bpm:>120").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("bpm");
        tokens[1].Value.Should().Be(":>");
        tokens[2].Value.Should().Be("120");
    }

    [Fact]
    public void Tokenize_DurationField_ReturnsFieldName()
    {
        var tokens = _tokenizer.Tokenize("duration:<300").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("duration");
    }

    [Fact]
    public void Tokenize_ComposerField_ReturnsFieldName()
    {
        var tokens = _tokenizer.Tokenize("composer:Bach").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("composer");
    }

    [Fact]
    public void Tokenize_TitleField_ReturnsFieldName()
    {
        var tokens = _tokenizer.Tokenize("title:Time").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("title");
    }

    [Fact]
    public void Tokenize_AlbumField_ReturnsFieldName()
    {
        var tokens = _tokenizer.Tokenize("album:Walls").ToList();

        tokens[0].Type.Should().Be(MqlTokenType.FieldName);
        tokens[0].Value.Should().Be("album");
    }

    [Theory]
    [InlineData("last-week", true)]
    [InlineData("last-month", true)]
    [InlineData("last-year", true)]
    [InlineData("today", true)]
    [InlineData("yesterday", true)]
    [InlineData("2026-01-06", true)]
    [InlineData("-7d", true)]
    [InlineData("-3w", true)]
    [InlineData("-12h", true)]
    [InlineData("notadate", false)]
    [InlineData("123", false)]
    public void Tokenize_DateKeywords_ReturnsDateLiteral(string value, bool expectedIsDate)
    {
        var tokens = _tokenizer.Tokenize($"added:{value}").ToList();

        var isDate = tokens[2].Type == MqlTokenType.DateLiteral;
        isDate.Should().Be(expectedIsDate);
    }

    [Fact]
    public void Tokenize_LastWeekWithHyphen_ReturnsDateLiteral()
    {
        var tokens = _tokenizer.Tokenize("added:last-week").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.DateLiteral);
    }

    [Fact]
    public void Tokenize_ContainsOperator_ReturnsOperator()
    {
        var tokens = _tokenizer.Tokenize("title:contains(test)").ToList();

        tokens[1].Type.Should().Be(MqlTokenType.Operator);
        tokens[1].Value.Should().Be(":");
    }

    [Fact]
    public void Tokenize_StartsWithOperator_ReturnsOperator()
    {
        var tokens = _tokenizer.Tokenize("title:startsWith(test)").ToList();

        tokens[1].Type.Should().Be(MqlTokenType.Operator);
    }

    [Fact]
    public void Tokenize_EndsWithOperator_ReturnsOperator()
    {
        var tokens = _tokenizer.Tokenize("title:endsWith(test)").ToList();

        tokens[1].Type.Should().Be(MqlTokenType.Operator);
    }

    [Fact]
    public void Tokenize_EmptyStringInQuotes_ReturnsEmptyString()
    {
        var tokens = _tokenizer.Tokenize("title:\"\"").ToList();

        tokens[2].Type.Should().Be(MqlTokenType.StringLiteral);
        tokens[2].Value.Should().BeEmpty();
    }
}
