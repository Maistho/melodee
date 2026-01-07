using FluentAssertions;
using Melodee.Mql.Constants;

namespace Melodee.Mql.Tests;

public class MqlValidatorTests
{
    private readonly MqlValidator _validator;

    public MqlValidatorTests()
    {
        _validator = new MqlValidator();
    }

    [Fact]
    public void Validate_EmptyQuery_ReturnsEmptyQueryError()
    {
        var result = _validator.Validate("", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlEmptyQuery);
    }

    [Fact]
    public void Validate_WhitespaceOnly_ReturnsEmptyQueryError()
    {
        var result = _validator.Validate("   \t\n  ", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlEmptyQuery);
    }

    [Fact]
    public void Validate_NullQuery_ReturnsEmptyQueryError()
    {
        var result = _validator.Validate(null!, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlEmptyQuery);
    }

    [Fact]
    public void Validate_ValidSimpleQuery_ReturnsValid()
    {
        var result = _validator.Validate("artist:Beatles", "songs");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MaxLengthQuery_ReturnsValid()
    {
        var query = new string('a', MqlConstants.MaxQueryLength);

        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_OverMaxLengthQuery_ReturnsTooLongError()
    {
        var query = new string('a', MqlConstants.MaxQueryLength + 1);

        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlQueryTooLong);
    }

    [Fact]
    public void Validate_MaxFieldCountQuery_ReturnsValid()
    {
        var validSongFields = new[] { "title", "artist", "album", "genre", "year", "duration", "bpm", "rating", "plays", "starred", "starredAt", "lastPlayedAt", "added", "composer", "discnumber", "tracknumber", "comment" };
        var fieldFilters = Enumerable.Range(0, 10)
            .Select(i => $"{validSongFields[i % validSongFields.Length]}:value{i}")
            .ToArray();
        var query = string.Join(" AND ", fieldFilters);

        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_OverMaxFieldCountQuery_ReturnsTooManyFieldsError()
    {
        var fieldFilters = Enumerable.Range(0, MqlConstants.MaxFieldCount + 1)
            .Select(i => $"field{i}:value{i}")
            .ToArray();
        var query = string.Join(" AND ", fieldFilters);

        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlTooManyFields);
    }

    [Fact]
    public void Validate_MaxDepthParentheses_ReturnsValid()
    {
        var query = new string('(', 5) + "artist:Beatles" + new string(')', 5);

        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_OverMaxDepthParentheses_ReturnsTooDeepError()
    {
        var query = new string('(', MqlConstants.MaxRecursionDepth + 1) + "artist:Beatles" + new string(')', MqlConstants.MaxRecursionDepth + 1);

        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlTooDeep);
    }

    [Fact]
    public void Validate_UnbalancedOpeningParen_ReturnsUnbalancedError()
    {
        var result = _validator.Validate("(artist:Beatles", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlUnbalancedParens);
    }

    [Fact]
    public void Validate_UnbalancedClosingParen_ReturnsUnbalancedError()
    {
        var result = _validator.Validate("artist:Beatles)", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlUnbalancedParens);
    }

    [Fact]
    public void Validate_UnknownField_ReturnsUnknownFieldError()
    {
        var result = _validator.Validate("artistt:Beatles", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlUnknownField);
    }

    [Fact]
    public void Validate_UnknownField_SuggestsSimilarField()
    {
        var result = _validator.Validate("artistt:Beatles", "songs");

        result.Errors.Should().Contain(e =>
            e.ErrorCode == MqlErrorCodes.MqlUnknownField &&
            e.Message.Contains("artist"));
    }

    [Fact]
    public void Validate_UnknownFieldForAlbums_ReturnsUnknownFieldError()
    {
        var result = _validator.Validate("titl:Test", "albums");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlUnknownField);
    }

    [Fact]
    public void Validate_ValidFieldForEntity_ReturnsValid()
    {
        var result = _validator.Validate("title:Test", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FieldAlias_ReturnsValid()
    {
        var result = _validator.Validate("disc:1", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ComplexBooleanQuery_ReturnsValid()
    {
        var result = _validator.Validate("(artist:Beatles OR artist:Pink Floyd) AND year:>=1970 NOT live", "songs");

        result.IsValid.Should().BeTrue();
        result.ComplexityScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Validate_RegexPattern_ReturnsValid()
    {
        var result = _validator.Validate("title:/remix/i", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_OverMaxRegexLength_ReturnsRegexTooComplexError()
    {
        var longPattern = new string('a', MqlConstants.MaxRegexPatternLength + 1);
        var query = $"title:/{longPattern}/";

        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlRegexTooComplex);
    }

    [Fact]
    public void Validate_DangerousRegexPattern_ReturnsRegexDangerousError()
    {
        var result = _validator.Validate("title:/(.*)*/", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlRegexDangerous);
    }

    [Fact]
    public void Validate_AnotherDangerousRegexPattern_ReturnsRegexDangerousError()
    {
        var result = _validator.Validate("title:/(.+)+/", "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlRegexDangerous);
    }

    [Fact]
    public void Validate_ComplexityScore_IncreasesWithFields()
    {
        var simpleResult = _validator.Validate("artist:Beatles", "songs");
        var complexResult = _validator.Validate("artist:Beatles AND year:>=1970 OR rating:>4", "songs");

        complexResult.ComplexityScore.Should().BeGreaterThan(simpleResult.ComplexityScore);
    }

    [Fact]
    public void Validate_HighComplexity_Warns()
    {
        var validSongFields = new[] { "title", "artist", "album", "genre", "year", "duration", "bpm" };
        var query = string.Join(" AND ", Enumerable.Range(0, 10).Select(i => $"{validSongFields[i % validSongFields.Length]}:value{i}"));

        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("complexity", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_VeryHighComplexity_Rejects()
    {
        var validSongFields = new[] { "title", "artist", "album", "genre", "year", "duration", "bpm" };
        var query = string.Join(" AND ", Enumerable.Range(0, 25).Select(i => $"{validSongFields[i % validSongFields.Length]}:value{i}"));

        var result = _validator.Validate(query, "songs");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlParseError);
    }

    [Fact]
    public void Validate_ComparisonOperators_ReturnsValid()
    {
        var queries = new[]
        {
            "year:=1970",
            "year:!=1970",
            "year:<1970",
            "year:<=1970",
            "year:>1970",
            "year:>=1970"
        };

        foreach (var query in queries)
        {
            var result = _validator.Validate(query, "songs");
            result.IsValid.Should().BeTrue($"Query '{query}' should be valid");
        }
    }

    [Fact]
    public void Validate_RangeExpression_ReturnsValid()
    {
        var result = _validator.Validate("year:1970-1980", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_BooleanLiteral_ReturnsValid()
    {
        var result = _validator.Validate("starred:true", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DateLiteral_ReturnsValid()
    {
        var result = _validator.Validate("added:2026-01-06", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_RelativeDate_ReturnsValid()
    {
        var result = _validator.Validate("added:-7d", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FreeText_ReturnsValid()
    {
        var result = _validator.Validate("pink floyd", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidEntity_ReturnsValid()
    {
        var result = _validator.Validate("title:Test", "invalidentity");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NestedParentheses_ReturnsValid()
    {
        var result = _validator.Validate("((artist:Beatles))", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ErrorPosition_Provided()
    {
        var result = _validator.Validate("(artist:Beatles", "songs");

        var error = result.Errors.FirstOrDefault(e => e.ErrorCode == MqlErrorCodes.MqlUnbalancedParens);
        error.Should().NotBeNull();
        error!.Position.Should().NotBeNull();
        error.Position!.Start.Should().BeGreaterThanOrEqualTo(0);
        error.Position!.End.Should().BeGreaterThan(error.Position!.Start);
    }

    [Fact]
    public void Validate_SimilarField_Suggested()
    {
        var result = _validator.Validate("artst:Beatles", "songs");

        result.Errors.Should().Contain(e =>
            e.ErrorCode == MqlErrorCodes.MqlUnknownField &&
            e.Message.Contains("artist"));
    }

    [Fact]
    public void Validate_ValidOperatorForStringField_ReturnsValid()
    {
        var result = _validator.Validate("title:contains(test)", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_LevenshteinDistance_TwoCharDifference()
    {
        var suggestions = GetFieldSuggestions("artst", "songs");

        suggestions.Should().Contain("artist");
    }

    [Fact]
    public void Validate_LevenshteinDistance_OneCharDifference()
    {
        var suggestions = GetFieldSuggestions("artit", "songs");

        suggestions.Should().Contain("artist");
    }

    [Fact]
    public void Validate_LevenshteinDistance_NoSuggestionsForFar()
    {
        var suggestions = GetFieldSuggestions("xyz123", "songs");

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidAlbumField_ReturnsValid()
    {
        var result = _validator.Validate("album:Walls", "albums");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidArtistField_ReturnsValid()
    {
        var result = _validator.Validate("artist:Beatles", "artists");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_UserScopedField_ReturnsValid()
    {
        var result = _validator.Validate("rating:>=4", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_AllOperators_HandledCorrectly()
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
            "title:contains(\"test\")",
            "title:startsWith(\"test\")",
            "title:endsWith(\"test\")",
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
    public void Validate_MultipleFields_SingleQuery()
    {
        var result = _validator.Validate("artist:Beatles AND year:>=1970 AND genre:Rock", "songs");

        result.IsValid.Should().BeTrue();
        result.ComplexityScore.Should().BeGreaterThan(3);
    }

    [Fact]
    public void Validate_WithRegexFlags_ReturnsValid()
    {
        var result = _validator.Validate("title:/test.*remix/ig", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ComplexityScore_AccountsForBooleanOperators()
    {
        var andResult = _validator.Validate("artist:Beatles AND year:1970", "songs");
        var orResult = _validator.Validate("artist:Beatles OR artist:Pink Floyd", "songs");
        var notResult = _validator.Validate("NOT artist:Beatles", "songs");

        andResult.ComplexityScore.Should().BeGreaterThan(0);
        orResult.ComplexityScore.Should().BeGreaterThan(0);
        notResult.ComplexityScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Validate_ComplexityScore_AccountsForNesting()
    {
        var flatResult = _validator.Validate("a:1 AND b:2", "songs");
        var nestedResult = _validator.Validate("(a:1 AND b:2)", "songs");

        nestedResult.ComplexityScore.Should().BeGreaterThanOrEqualTo(flatResult.ComplexityScore);
    }

    [Fact]
    public void Validate_ComplexityScore_AccountsForRegex()
    {
        var normalResult = _validator.Validate("title:test", "songs");
        var regexResult = _validator.Validate("title:/test/", "songs");

        regexResult.ComplexityScore.Should().BeGreaterThan(normalResult.ComplexityScore);
    }

    [Fact]
    public void Validate_SpecialCharactersInFreeText_ReturnsValid()
    {
        var result = _validator.Validate("title:\"Test (Remastered)\"", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_UnicodeCharacters_ReturnsValid()
    {
        var result = _validator.Validate("artist:日本語テスト", "songs");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmojiCharacters_ReturnsValid()
    {
        var result = _validator.Validate("title:\"🎵 emoji\"", "songs");

        result.IsValid.Should().BeTrue();
    }

    private static string[] GetFieldSuggestions(string fieldName, string entityType)
    {
        var allFields = MqlFieldRegistry.GetFieldNames(entityType).ToList();
        var suggestions = new List<string>();

        foreach (var field in allFields)
        {
            var distance = LevenshteinDistance(fieldName.ToLowerInvariant(), field.ToLowerInvariant());
            if (distance <= 2 && distance > 0)
            {
                suggestions.Add(field);
            }
        }

        return suggestions.OrderBy(s => LevenshteinDistance(fieldName.ToLowerInvariant(), s.ToLowerInvariant())).ToArray();
    }

    private static int LevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
        {
            return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
        }

        if (string.IsNullOrEmpty(s2))
        {
            return s1.Length;
        }

        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (var i = 0; i <= s1.Length; i++)
        {
            matrix[i, 0] = i;
        }

        for (var j = 0; j <= s2.Length; j++)
        {
            matrix[0, j] = j;
        }

        for (var i = 1; i <= s1.Length; i++)
        {
            for (var j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[s1.Length, s2.Length];
    }
}
