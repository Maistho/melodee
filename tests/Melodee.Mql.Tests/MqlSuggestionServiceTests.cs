using FluentAssertions;
using Melodee.Mql.Models;
using Melodee.Mql.Services;

namespace Melodee.Mql.Tests;

public class MqlSuggestionServiceTests
{
    private readonly MqlSuggestionService _service;

    public MqlSuggestionServiceTests()
    {
        _service = new MqlSuggestionService();
    }

    #region GetSuggestions Tests

    [Fact]
    public void GetSuggestions_EmptyQuery_ReturnsFieldSuggestions()
    {
        var result = _service.GetSuggestions("", "songs", 0);

        result.Suggestions.Should().NotBeEmpty();
        result.Suggestions.Should().OnlyContain(s => s.Type == MqlSuggestionType.Field);
        result.DetectedContext.Should().Be("startofquery");
    }

    [Fact]
    public void GetSuggestions_EmptyQueryWithEntity_ReturnsFieldSuggestions()
    {
        var result = _service.GetSuggestions(string.Empty, "albums", 0);

        result.Suggestions.Should().NotBeEmpty();
        result.Suggestions.Should().OnlyContain(s => s.Type == MqlSuggestionType.Field);
        result.DetectedContext.Should().Be("startofquery");
    }

    [Fact]
    public void GetSuggestions_AfterSpace_ReturnsFieldsAndKeywords()
    {
        var result = _service.GetSuggestions("artist:Beatles ", "songs", 15);

        result.Suggestions.Should().NotBeEmpty();
        result.DetectedContext.Should().Be("afterspace");
    }

    [Fact]
    public void GetSuggestions_InFieldName_ReturnsMatchingFields()
    {
        var result = _service.GetSuggestions("art", "songs", 3);

        result.Suggestions.Should().NotBeEmpty();
        result.Suggestions.Should().Contain(s => s.Text == "artist");
        result.DetectedContext.Should().Be("infieldname");
    }

    [Fact]
    public void GetSuggestions_AfterFieldName_ReturnsOperators()
    {
        var result = _service.GetSuggestions("artist", "songs", 6);

        result.Suggestions.Should().NotBeEmpty();
        result.Suggestions.Should().Contain(s => s.Type == MqlSuggestionType.Operator);
        result.DetectedContext.Should().Be("afterfieldname");
    }

    [Fact]
    public void GetSuggestions_AfterColon_ReturnsOperators()
    {
        var result = _service.GetSuggestions("artist:", "songs", 7);

        result.Suggestions.Should().NotBeEmpty();
        result.Suggestions.Should().OnlyContain(s => s.Type == MqlSuggestionType.Operator);
        result.DetectedContext.Should().Be("aftercolon");
    }

    [Fact]
    public void GetSuggestions_InValue_ReturnsValueSuggestions()
    {
        var result = _service.GetSuggestions("year:19", "songs", 7);

        result.Suggestions.Should().NotBeEmpty();
        result.Suggestions.Should().Contain(s => s.Text.StartsWith("19"));
        result.DetectedContext.Should().Be("invalue");
    }

    [Fact]
    public void GetSuggestions_ProcessingTimeIsSet()
    {
        var result = _service.GetSuggestions("artist", "songs", 6);

        result.ProcessingTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region GetFieldSuggestions Tests

    [Fact]
    public void GetFieldSuggestions_NoPartial_ReturnsAllFields()
    {
        var result = _service.GetFieldSuggestions("", "songs").ToList();

        result.Should().NotBeEmpty();
        result.Should().Contain(s => s.Text == "artist");
        result.Should().Contain(s => s.Text == "title");
        result.Should().Contain(s => s.Text == "album");
    }

    [Fact]
    public void GetFieldSuggestions_PartialArt_ReturnsMatchingFields()
    {
        var result = _service.GetFieldSuggestions("art", "songs").ToList();

        result.Should().NotBeEmpty();
        result.Should().Contain(s => s.Text == "artist");
        result.Should().NotContain(s => s.Text == "title");
    }

    [Fact]
    public void GetFieldSuggestions_PartialYear_ReturnsYear()
    {
        var result = _service.GetFieldSuggestions("year", "songs").ToList();

        result.Should().Contain(s => s.Text == "year");
    }

    [Fact]
    public void GetFieldSuggestions_InvalidEntity_ReturnsEmpty()
    {
        var result = _service.GetFieldSuggestions("artist", "invalid").ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetFieldSuggestions_CaseInsensitive_ReturnsMatches()
    {
        var result = _service.GetFieldSuggestions("ART", "songs").ToList();

        result.Should().Contain(s => s.Text == "artist");
    }

    #endregion

    #region GetOperatorSuggestions Tests

    [Fact]
    public void GetOperatorSuggestions_NoField_ReturnsAllComparisonOperators()
    {
        var result = _service.GetOperatorSuggestions(null, "songs").ToList();

        result.Should().NotBeEmpty();
        result.Should().Contain(s => s.Text == ":=");
        result.Should().Contain(s => s.Text == ":>=");
    }

    [Fact]
    public void GetOperatorSuggestions_NumericField_ReturnsComparisonOperators()
    {
        var result = _service.GetOperatorSuggestions("year", "songs").ToList();

        result.Should().NotBeEmpty();
        result.Should().Contain(s => s.Text == ":>=");
    }

    [Fact]
    public void GetOperatorSuggestions_StringField_ReturnsStringOperators()
    {
        var result = _service.GetOperatorSuggestions("artist", "songs").ToList();

        result.Should().NotBeEmpty();
        result.Should().Contain(s => s.Text == "contains");
        result.Should().Contain(s => s.Text == "startsWith");
    }

    [Fact]
    public void GetOperatorSuggestions_BooleanField_ReturnsBooleanOperators()
    {
        var result = _service.GetOperatorSuggestions("starred", "songs").ToList();

        result.Should().Contain(s => s.Text == ":=");
        result.Should().Contain(s => s.Text == ":!");
    }

    #endregion

    #region GetValueSuggestions Tests

    [Fact]
    public void GetValueSuggestions_BooleanField_ReturnsBooleanValues()
    {
        var result = _service.GetValueSuggestions("starred", "", "songs").ToList();

        result.Should().Contain(s => s.Text == "true");
        result.Should().Contain(s => s.Text == "false");
    }

    [Fact]
    public void GetValueSuggestions_DateField_ReturnsRelativeDates()
    {
        var result = _service.GetValueSuggestions("added", "", "songs").ToList();

        result.Should().Contain(s => s.Text == "today");
        result.Should().Contain(s => s.Text == "yesterday");
        result.Should().Contain(s => s.Text == "last-week");
    }

    [Fact]
    public void GetValueSuggestions_YearField_ReturnsYears()
    {
        var result = _service.GetValueSuggestions("year", "19", "songs").ToList();

        result.Should().Contain(s => s.Text.StartsWith("19"));
    }

    [Fact]
    public void GetValueSuggestions_GenreField_ReturnsKnownGenres()
    {
        var result = _service.GetValueSuggestions("genre", "Rock", "songs").ToList();

        result.Should().Contain(s => s.Text == "\"Rock\"");
    }

    [Fact]
    public void GetValueSuggestions_MoodField_ReturnsKnownMoods()
    {
        var result = _service.GetValueSuggestions("mood", "Chill", "songs").ToList();

        result.Should().Contain(s => s.Text == "\"Chill\"");
    }

    [Fact]
    public void GetValueSuggestions_UnknownField_ReturnsEmpty()
    {
        var result = _service.GetValueSuggestions("unknownfield", "value", "songs").ToList();

        result.Should().BeEmpty();
    }

    #endregion

    #region GetKeywordSuggestions Tests

    [Fact]
    public void GetKeywordSuggestions_NoPartial_ReturnsAllKeywords()
    {
        var result = _service.GetKeywordSuggestions("").ToList();

        result.Should().Contain(s => s.Text == "AND");
        result.Should().Contain(s => s.Text == "OR");
        result.Should().Contain(s => s.Text == "NOT");
    }

    [Fact]
    public void GetKeywordSuggestions_PartialAN_ReturnsMatchingKeywords()
    {
        var result = _service.GetKeywordSuggestions("an").ToList();

        result.Should().Contain(s => s.Text == "AND");
        result.Should().NotContain(s => s.Text == "OR");
    }

    [Fact]
    public void GetKeywordSuggestions_PartialN_ReturnsNot()
    {
        var result = _service.GetKeywordSuggestions("n").ToList();

        result.Should().Contain(s => s.Text == "NOT");
    }

    #endregion

    #region Context Detection Tests

    [Fact]
    public void GetSuggestions_QueryWithColon_ReturnsAfterColonContext()
    {
        var result = _service.GetSuggestions("genre:", "songs", 6);

        result.DetectedContext.Should().Be("aftercolon");
    }

    [Fact]
    public void GetSuggestions_QueryWithCompleteField_ReturnsAfterFieldNameContext()
    {
        var result = _service.GetSuggestions("title", "songs", 5);

        result.DetectedContext.Should().Be("afterfieldname");
    }

    [Fact]
    public void GetSuggestions_QueryWithOperator_ReturnsAfterOperatorContext()
    {
        var result = _service.GetSuggestions("year:>=", "songs", 8);

        result.DetectedContext.Should().Be("afteroperator");
    }

    #endregion

    #region Entity Type Tests

    [Theory]
    [InlineData("songs")]
    [InlineData("albums")]
    [InlineData("artists")]
    public void GetSuggestions_SupportedEntityTypes_ReturnsSuggestions(string entityType)
    {
        var result = _service.GetSuggestions("", entityType, 0);

        result.Suggestions.Should().NotBeEmpty();
    }

    [Fact]
    public void GetFieldSuggestions_Albums_ReturnsAlbumFields()
    {
        var result = _service.GetFieldSuggestions("", "albums").ToList();

        result.Should().Contain(s => s.Text == "album");
        result.Should().Contain(s => s.Text == "songCount");
    }

    [Fact]
    public void GetFieldSuggestions_Artists_ReturnsArtistFields()
    {
        var result = _service.GetFieldSuggestions("", "artists").ToList();

        result.Should().Contain(s => s.Text == "artist");
        result.Should().Contain(s => s.Text == "albumCount");
    }

    #endregion

    #region Confidence and Sorting Tests

    [Fact]
    public void GetFieldSuggestions_ExactMatch_ReturnsHighConfidence()
    {
        var result = _service.GetFieldSuggestions("artist", "songs").ToList();

        var artistSuggestion = result.First(s => s.Text == "artist");
        artistSuggestion.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void GetSuggestions_ResultsAreLimitedTo10()
    {
        var result = _service.GetSuggestions("", "songs", 0);

        result.Suggestions.Should().HaveCountLessThanOrEqualTo(10);
    }

    #endregion
}
