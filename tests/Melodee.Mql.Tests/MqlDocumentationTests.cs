using FluentAssertions;
using Melodee.Mql.Api.Dto;
using Melodee.Mql.Services;

namespace Melodee.Mql.Tests;

public class MqlDocumentationServiceTests
{
    [Fact]
    public void MqlQueryAnalyzer_AnalyzesSimpleQuery_ReturnsValidResult()
    {
        var analyzer = new MqlQueryAnalyzer(new MqlParser(), new MqlValidator(), new MqlTokenizer());
        var result = analyzer.Analyze("artist:Beatles", "songs");

        result.IsValid.Should().BeTrue();
        result.Query.Should().Be("artist:Beatles");
        result.EntityType.Should().Be("songs");
        result.Recommendations.Should().NotBeNull();
    }

    [Fact]
    public void MqlQueryAnalyzer_DetectsComplexity()
    {
        var analyzer = new MqlQueryAnalyzer(new MqlParser(), new MqlValidator(), new MqlTokenizer());
        var result = analyzer.Analyze("artist:Beatles AND year:>=1970 AND genre:Rock AND NOT live", "songs");

        result.ComplexityScore.Should().BeGreaterThan(0);
        result.FieldCount.Should().Be(3);
    }

    [Fact]
    public void MqlQueryAnalyzer_DetectsNestedParentheses()
    {
        var analyzer = new MqlQueryAnalyzer(new MqlParser(), new MqlValidator(), new MqlTokenizer());
        var result = analyzer.Analyze("(artist:Beatles OR (artist:Pink Floyd AND year:>=1970)) AND year:<=1990", "songs");

        result.HasNestedParentheses.Should().BeTrue();
    }

    [Fact]
    public void MqlQueryAnalyzer_InvalidQuery_ReturnsInvalidWithRecommendations()
    {
        var analyzer = new MqlQueryAnalyzer(new MqlParser(), new MqlValidator(), new MqlTokenizer());
        var result = analyzer.Analyze("invalidfield:test", "songs");

        result.IsValid.Should().BeFalse();
        result.Recommendations.Should().Contain(r => r.Contains("validation errors"));
    }

    [Fact]
    public void MqlQueryAnalyzer_ProvidesRecommendationsForHighComplexity()
    {
        var analyzer = new MqlQueryAnalyzer(new MqlParser(), new MqlValidator(), new MqlTokenizer());
        var validFields = new[] { "artist", "album", "title", "genre", "year", "duration", "bpm", "rating", "plays", "mood" };
        var complexQuery = string.Join(" ", Enumerable.Range(0, 11).Select(i => $"{validFields[i % validFields.Length]}:value{i}"));
        var result = analyzer.Analyze(complexQuery, "songs");

        result.FieldCount.Should().Be(11);
        result.Recommendations.Should().Contain(r => r.Contains("field filters"));
    }
}

public class MqlErrorDisplayDtoTests
{
    [Fact]
    public void MqlErrorResponse_CreatesWithTimestamp()
    {
        var error = new MqlErrorResponse
        {
            ErrorCode = "MQL_INVALID_TOKEN",
            Message = "Invalid token"
        };

        error.ErrorCode.Should().Be("MQL_INVALID_TOKEN");
        error.Message.Should().Be("Invalid token");
        error.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MqlErrorResponse_WithPosition_HasCorrectValues()
    {
        var error = new MqlErrorResponse
        {
            ErrorCode = "MQL_UNKNOWN_FIELD",
            Message = "Unknown field",
            Position = new MqlPositionDto
            {
                Position = 10,
                Line = 1,
                Column = 11,
                Length = 5
            }
        };

        error.Position.Should().NotBeNull();
        error.Position!.Position.Should().Be(10);
        error.Position.Line.Should().Be(1);
        error.Position.Column.Should().Be(11);
        error.Position.Length.Should().Be(5);
    }

    [Fact]
    public void MqlErrorResponse_WithSuggestions_CountsCorrectly()
    {
        var error = new MqlErrorResponse
        {
            ErrorCode = "MQL_UNKNOWN_FIELD",
            Message = "Unknown field",
            Suggestions = new List<MqlSuggestionDto>
            {
                new() { Text = "artist", Type = "field" },
                new() { Text = "album", Type = "field" }
            }
        };

        error.Suggestions.Should().HaveCount(2);
    }

    [Fact]
    public void MqlSuggestionDto_DefaultValues()
    {
        var suggestion = new MqlSuggestionDto
        {
            Text = "Beatles",
            Type = "value"
        };

        suggestion.Text.Should().Be("Beatles");
        suggestion.Type.Should().Be("value");
        suggestion.Confidence.Should().Be(0);
        suggestion.Offset.Should().BeNull();
        suggestion.Description.Should().BeNull();
    }
}

public class MqlMetricsResponseTests
{
    [Fact]
    public void MqlMetricsResponse_CreatesWithDefaults()
    {
        var response = new MqlMetricsResponse();

        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        response.QueryMetrics.Should().NotBeNull();
        response.CacheMetrics.Should().NotBeNull();
        response.Status.Should().Be("healthy");
    }

    [Fact]
    public void MqlMetricsResponse_WithData_PopulatesCorrectly()
    {
        var response = new MqlMetricsResponse
        {
            Timestamp = DateTime.UtcNow,
            Status = "degraded",
            QueryMetrics = new QueryMetricsSummary
            {
                TotalQueries = 100,
                ValidQueries = 95,
                FailedQueries = 5,
                P95LatencyMs = 1500
            },
            CacheMetrics = new CacheMetrics
            {
                HitCount = 80,
                MissCount = 20
            }
        };

        response.QueryMetrics.TotalQueries.Should().Be(100);
        response.QueryMetrics.ErrorRate.Should().Be(0.05);
        response.CacheMetrics.HitRate.Should().Be(0.8);
        response.Status.Should().Be("degraded");
    }
}

public class QueryMetricsSummaryTests
{
    [Fact]
    public void QueryMetricsSummary_CalculatesErrorRate()
    {
        var summary = new QueryMetricsSummary
        {
            TotalQueries = 100,
            ValidQueries = 90,
            FailedQueries = 10
        };

        summary.ErrorRate.Should().Be(0.1);
    }

    [Fact]
    public void QueryMetricsSummary_NoQueries_HasZeroErrorRate()
    {
        var summary = new QueryMetricsSummary
        {
            TotalQueries = 0
        };

        summary.ErrorRate.Should().Be(0);
    }

    [Fact]
    public void QueryMetricsSummary_Percentiles_CalculatedCorrectly()
    {
        var summary = new QueryMetricsSummary
        {
            TotalQueries = 100,
            MinLatencyMs = 10,
            MaxLatencyMs = 5000,
            P50LatencyMs = 100,
            P95LatencyMs = 1500,
            P99LatencyMs = 3000
        };

        summary.MinLatencyMs.Should().Be(10);
        summary.P99LatencyMs.Should().Be(3000);
    }
}

public class CacheMetricsTests
{
    [Fact]
    public void CacheMetrics_CalculatesHitRate()
    {
        var metrics = new CacheMetrics
        {
            HitCount = 80,
            MissCount = 20,
            EntryCount = 100
        };

        metrics.HitRate.Should().Be(0.8);
    }

    [Fact]
    public void CacheMetrics_NoEntries_HasZeroHitRate()
    {
        var metrics = new CacheMetrics
        {
            HitCount = 0,
            MissCount = 0,
            EntryCount = 0
        };

        metrics.HitRate.Should().Be(0);
    }
}
