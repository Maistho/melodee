using System.Text;
using FluentAssertions;
using Melodee.Mql.Constants;
using Melodee.Mql.Models;

namespace Melodee.Mql.Tests;

public class MqlEdgeCaseTests
{
    private readonly MqlValidator _validator;
    private readonly MqlTokenizer _tokenizer;
    private readonly MqlParser _parser;

    public MqlEdgeCaseTests()
    {
        _validator = new MqlValidator();
        _tokenizer = new MqlTokenizer();
        _parser = new MqlParser();
    }

    [Fact]
    public void EmptyQuery_ReturnsHelpfulMessage()
    {
        var result = _validator.Validate("", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlEmptyQuery);
    }

    [Fact]
    public void OnlyWhitespace_HandledGracefully()
    {
        var result = _validator.Validate("   \t\n  ", "songs");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void MaxLengthQuery_Accepted()
    {
        var query = new string('a', 500);
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void OverMaxLengthQuery_Rejected()
    {
        var query = new string('a', 501);
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlQueryTooLong);
    }

    [Fact]
    public void SpecialCharacters_InFreeText_Handled()
    {
        var query = "title:\"Test (special chars)\"";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SpecialCharacters_InFieldValue_Handled()
    {
        var query = "artist:\"Test Artist\"";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UnicodeCharacters_InQuery_Handled()
    {
        var query = "artist:\"日本語テスト\" AND title:\"🎵 emoji\"";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void VeryLongFieldValue_Handled()
    {
        var longValue = new string('x', 400);
        var query = $"artist:\"{longValue}\"";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void NestedParentheses_AtMaxDepth_Accepted()
    {
        var query = new StringBuilder();
        for (int i = 0; i < 9; i++)
            query.Append('(');
        query.Append("artist:Beatles");
        for (int i = 0; i < 9; i++)
            query.Append(')');

        var result = _validator.Validate(query.ToString(), "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void NestedParentheses_OverMaxDepth_Rejected()
    {
        var query = new StringBuilder();
        for (int i = 0; i < 11; i++)
            query.Append('(');
        query.Append("artist:Beatles");
        for (int i = 0; i < 11; i++)
            query.Append(')');

        var result = _validator.Validate(query.ToString(), "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlTooDeep);
    }

    [Fact]
    public void UnbalancedParentheses_Open_Detected()
    {
        var query = "artist:Beatles AND (year:>=1970";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlUnbalancedParens);
    }

    [Fact]
    public void UnbalancedParentheses_Close_Detected()
    {
        var query = "artist:Beatles) AND year:>=1970";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlUnbalancedParens);
    }

    [Fact]
    public void MaxFieldCount_AtLimit_Accepted()
    {
        var query = "artist:Beatles AND year:>=1970 AND genre:Rock";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void OverMaxFieldCount_Rejected()
    {
        var query = string.Join(" AND ", Enumerable.Range(0, 25)
            .Select(i => $"f{i}:v{i}"));

        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlTooManyFields);
    }

    [Fact]
    public void AllOperators_HandledCorrectly()
    {
        var queries = new[]
        {
            "year:=1970",
            "year:!=1970",
            "year:<1970",
            "year:<=1970",
            "year:>1970",
            "year:>=1970",
            "year:1970-1980",
            "title:contains(test)",
            "title:startsWith(test)",
            "title:endsWith(test)",
            "title:wildcard(*test*)",
            "starred:true",
            "starred:false"
        };

        foreach (var query in queries)
        {
            var result = _validator.Validate(query, "songs");
            result.IsValid.Should().BeTrue($"Failed for query: {query}");
        }
    }

    [Fact]
    public void ImplicitAnd_BetweenTerms_Handled()
    {
        var query = "artist:Beatles year:>=1970";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CaseInsensitive_Keywords_Work()
    {
        var queries = new[]
        {
            "AND artist:Beatles",
            "and artist:Beatles",
            "or artist:Beatles",
            "OR artist:Beatles",
            "not live",
            "NOT live"
        };

        foreach (var query in queries)
        {
            var result = _validator.Validate(query, "songs");
            result.IsValid.Should().BeTrue($"Failed for query: {query}");
        }
    }

    [Fact]
    public void QuotedString_WithSpaces_Work()
    {
        var query = "artist:\"Pink Floyd\"";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EscapedQuotes_InString_Work()
    {
        var query = "title:\"Test\\\"Song\"";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RelativeDates_Work()
    {
        var queries = new[]
        {
            "added:today",
            "added:yesterday",
            "added:last-week",
            "added:last-month",
            "added:last-year",
            "added:-7d",
            "added:-3w",
            "added:-12h"
        };

        foreach (var query in queries)
        {
            var result = _validator.Validate(query, "songs");
            result.IsValid.Should().BeTrue($"Failed for query: {query}");
        }
    }

    [Fact]
    public void BooleanLiterals_Work()
    {
        var queries = new[]
        {
            "starred:true",
            "starred:false",
            "starred:True",
            "starred:False"
        };

        foreach (var query in queries)
        {
            var result = _validator.Validate(query, "songs");
            result.IsValid.Should().BeTrue($"Failed for query: {query}");
        }
    }

    [Fact]
    public void UnknownField_SuggestsSimilar()
    {
        var query = "arttist:Beatles";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlUnknownField);
        result.Errors.First().Suggestions.Should().Contain("artist");
    }

    [Fact]
    public void MixedBooleanOperators_Work()
    {
        var query = "artist:Beatles AND year:>=1970 OR artist:Pink Floyd";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void MultipleNotOperators_Work()
    {
        var query = "NOT live AND NOT demo AND artist:Beatles";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RegexPattern_Work()
    {
        var query = "title:/remix/i";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RegexPatternTooLong_Rejected()
    {
        var pattern = new string('a', 101);
        var query = $"title:/{pattern}/";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlRegexTooComplex);
    }

    [Fact]
    public void DangerousRegexPattern_Rejected()
    {
        var query = "title:/(.*)*/";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FreeText_WithSpaces_WorksAsImplicitAnd()
    {
        var query = "pink floyd";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ConsecutiveSpaces_InFreeText_Handled()
    {
        var query = "pink   floyd";
        var tokens = _tokenizer.Tokenize(query).ToList();

        tokens.Should().Contain(t => t.Type == MqlTokenType.FreeText && t.Value == "pink");
        tokens.Should().Contain(t => t.Type == MqlTokenType.FreeText && t.Value == "floyd");
    }

    [Fact]
    public void TabCharacter_InQuery_Handled()
    {
        var query = "artist:\tBeatles";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void NewlineCharacter_InQuery_Handled()
    {
        var query = "artist:Beatles\nyear:>=1970";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ZeroFieldValue_Works()
    {
        var query = "plays:0";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DecimalFieldValue_Works()
    {
        var query = "rating:4.5";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void NegativeNumberFieldValue_Works()
    {
        var query = "rating:-5";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CommentsInQuery_HandledAsText()
    {
        var query = "artist:Beatles comment:Great album";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyFieldValue_Handled()
    {
        var query = "artist:";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyQuotedFieldValue_Handled()
    {
        var query = "artist:\"\"";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FieldWithNumberValue_Works()
    {
        var query = "bpm:120";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FieldWithDuration_Works()
    {
        var query = "duration:<300";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FieldWithRatingValue_Works()
    {
        var query = "rating:4";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FieldWithNotEqual_Works()
    {
        var query = "artist:!=Beatles";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ComplexQuery_WithMultipleOperators_Works()
    {
        var query = "(artist:Beatles OR artist:Pink Floyd) AND year:>=1970 AND NOT live AND genre:Rock";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Query_WithContainsOperator_Works()
    {
        var query = "title:contains love";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Query_WithStartsWithOperator_Works()
    {
        var query = "title:startsWith A";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Query_WithEndsWithOperator_Works()
    {
        var query = "title:endsWith love";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Query_WithWildcardOperator_Works()
    {
        var query = "title:wildcard *love*";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Query_WithRegexPattern_Works()
    {
        var query = "title:/remix/i";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Query_WithMultipleRegexPatterns_Works()
    {
        var query = "title:/remix/ AND artist:/test/";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyParentheses_Handled()
    {
        var query = "artist:Beatles";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void OnlyNotOperator_Handled()
    {
        var query = "NOT artist:Beatles";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FieldValueWithEqualsSign_Works()
    {
        var query = "comment:test=value";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FieldValueWithAtSign_Works()
    {
        var query = "artist:@test";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FieldValueWithHashSign_Works()
    {
        var query = "genre:#rock";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void QueryWithTrailingOperator_Handled()
    {
        var query = "artist:Beatles AND year:";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void QueryWithLeadingOperator_Handled()
    {
        var query = "AND artist:Beatles";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void QueryWithDoubleColon_Works()
    {
        var query = "artist:Beatles";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void QueryWithMultipleSpaces_Works()
    {
        var query = "artist   :   Beatles   AND   year   :   >=   1970";
        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void QueryWithMixedCaseOperators_Works()
    {
        var queries = new[]
        {
            "artist:Beatles and year:>=1970",
            "artist:Beatles or year:>=1970",
            "NOT artist:Beatles"
        };

        foreach (var query in queries)
        {
            var result = _validator.Validate(query, "songs");
            result.IsValid.Should().BeTrue($"Failed for query: {query}");
        }
    }
}
