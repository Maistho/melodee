using System.Linq.Expressions;
using FluentAssertions;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Mql.Models;
using NodaTime;

namespace Melodee.Mql.Tests;

/// <summary>
/// Tests for MqlPodcastEpisodeCompiler - verifies MQL queries compile and execute correctly for podcast episodes.
/// Note: The compiler normalizes strings to uppercase, so test data should account for this when testing actual execution.
/// </summary>
public class MqlPodcastEpisodeCompilerTests
{
    private readonly MqlPodcastEpisodeCompiler _compiler;
    private readonly MqlTokenizer _tokenizer;
    private readonly MqlParser _parser;

    public MqlPodcastEpisodeCompilerTests()
    {
        _compiler = new MqlPodcastEpisodeCompiler();
        _tokenizer = new MqlTokenizer();
        _parser = new MqlParser();
    }

    private Expression<Func<PodcastEpisode, bool>> CompileQuery(string query, int? userId = null)
    {
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "podcasts");
        parseResult.IsValid.Should().BeTrue($"Query '{query}' should parse successfully: {string.Join(", ", parseResult.Errors.Select(e => e.Message))}");
        parseResult.Ast.Should().NotBeNull();
        return _compiler.Compile(parseResult.Ast!, userId);
    }

    private static PodcastChannel CreateChannel(int id, string title)
    {
        return new PodcastChannel
        {
            Id = id,
            ApiKey = Guid.NewGuid(),
            UserId = 1,
            Title = title,
            TitleNormalized = title.ToUpperInvariant(),
            FeedUrl = $"https://example.com/feed/{id}",
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };
    }

    private static PodcastEpisode CreateEpisode(int id, string title, PodcastChannel channel, TimeSpan? duration = null, Instant? publishDate = null, string? description = null)
    {
        return new PodcastEpisode
        {
            Id = id,
            ApiKey = Guid.NewGuid(),
            PodcastChannelId = channel.Id,
            PodcastChannel = channel,
            Title = title,
            TitleNormalized = title.ToUpperInvariant(),
            Description = description,
            EpisodeKey = $"episode-{id}",
            EnclosureUrl = $"https://example.com/episode/{id}.mp3",
            Duration = duration,
            PublishDate = publishDate,
            DownloadStatus = PodcastEpisodeDownloadStatus.None,
            CreatedAt = Instant.FromUtc(2025, 1, 1, 0, 0)
        };
    }

    // ========== Free Text Search Tests ==========

    [Fact]
    public void Compile_FreeTextQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("technology");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_FreeTextQuery_HandlesNullDescription()
    {
        var expression = CompileQuery("test");
        var compiled = expression.Compile();

        var channel = CreateChannel(1, "Test Channel");
        var episodeWithNullDescription = CreateEpisode(1, "Episode 1", channel, description: null);

        var result = () => compiled(episodeWithNullDescription);
        result.Should().NotThrow();
    }

    [Fact]
    public void Compile_FreeTextQuery_MultiWord_CompilesSuccessfully()
    {
        var expression = CompileQuery("Tech Talk");
        expression.Should().NotBeNull();
    }

    // ========== Field Query Tests ==========

    [Fact]
    public void Compile_TitleFieldQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("title:Interview");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_ChannelFieldQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("channel:Science");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_CaseInsensitiveFieldName_Works()
    {
        var expression1 = CompileQuery("TITLE:test");
        var expression2 = CompileQuery("title:test");
        var expression3 = CompileQuery("Title:test");

        expression1.Should().NotBeNull();
        expression2.Should().NotBeNull();
        expression3.Should().NotBeNull();
    }

    [Fact]
    public void Compile_UnknownField_ReturnsTrue()
    {
        var expression = CompileQuery("unknownfield:value");
        var compiled = expression.Compile();

        var channel = CreateChannel(1, "Podcast");
        var episode = CreateEpisode(1, "Episode 1", channel);

        compiled(episode).Should().BeTrue();
    }

    // ========== Boolean Operator Tests ==========

    [Fact]
    public void Compile_NotQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("NOT sponsored");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_AndQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("title:Interview AND channel:Tech");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_OrQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("channel:Tech OR channel:Science");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_ParenthesesQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("(channel:Tech OR channel:Science) AND title:Review");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_NestedNotExpression_CompilesSuccessfully()
    {
        var expression = CompileQuery("NOT (channel:Music OR channel:Sports)");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_MultipleAndConditions_CompilesSuccessfully()
    {
        var expression = CompileQuery("title:Episode AND channel:Tech AND duration:>30");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_MultipleOrConditions_CompilesSuccessfully()
    {
        var expression = CompileQuery("channel:Tech OR channel:Science OR channel:News");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_MixedBooleanOperators_CompilesSuccessfully()
    {
        var expression = CompileQuery("channel:Tech AND (title:Review OR title:Interview)");
        expression.Should().NotBeNull();
    }

    // ========== Comparison Operator Tests ==========

    [Fact]
    public void Compile_DurationGreaterThan_CompilesSuccessfully()
    {
        var expression = CompileQuery("duration:>30");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_DurationLessThan_CompilesSuccessfully()
    {
        var expression = CompileQuery("duration:<60");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_DurationLessThanOrEquals_CompilesSuccessfully()
    {
        var expression = CompileQuery("duration:<=60");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_DurationGreaterThanOrEquals_CompilesSuccessfully()
    {
        var expression = CompileQuery("duration:>=30");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_NotEqualsOperator_CompilesSuccessfully()
    {
        var expression = CompileQuery("channel:!=Tech");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_ExactEquals_CompilesSuccessfully()
    {
        var expression = CompileQuery("title:=Interview");
        expression.Should().NotBeNull();
    }

    // ========== String Operator Tests ==========

    [Fact]
    public void Compile_ContainsOperator_CompilesSuccessfully()
    {
        var expression = CompileQuery("title:contains Interview");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_StartsWithOperator_CompilesSuccessfully()
    {
        var expression = CompileQuery("title:startsWith Episode");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_EndsWithOperator_SingleWord_CompilesSuccessfully()
    {
        var expression = CompileQuery("title:endsWith Finale");
        expression.Should().NotBeNull();
    }

    [Fact]
    public void Compile_WildcardOperator_CompilesSuccessfully()
    {
        var expression = CompileQuery("title:wildcard Ep*");
        expression.Should().NotBeNull();
    }

    // ========== Range and Date Tests ==========

    [Fact]
    public void Compile_DurationRangeQuery_CompilesSuccessfully()
    {
        var expression = CompileQuery("duration:30-60");
        expression.Should().NotBeNull();
    }

    // ========== Null Value Handling Tests ==========

    [Fact]
    public void Compile_DurationHandlesNullValue()
    {
        var expression = CompileQuery("duration:>30");
        var compiled = expression.Compile();

        var channel = CreateChannel(1, "Podcast");
        var episodeWithNullDuration = CreateEpisode(1, "Episode", channel, duration: null);

        compiled(episodeWithNullDuration).Should().BeFalse();
    }

    // ========== Complex Query Tests ==========

    [Fact]
    public void Compile_ComplexBooleanExpression_CompilesSuccessfully()
    {
        var expression = CompileQuery("(title:Interview OR title:Review) AND channel:Tech");
        expression.Should().NotBeNull();
    }

    // ========== Field Registry Tests ==========

    [Fact]
    public void PodcastEntityType_HasExpectedFields()
    {
        var fields = MqlFieldRegistry.GetFieldNames("podcasts").ToList();

        fields.Should().Contain("channel");
        fields.Should().Contain("title");
        fields.Should().Contain("published");
        fields.Should().Contain("downloaded");
        fields.Should().Contain("duration");
    }

    [Fact]
    public void GetField_TitleField_ReturnsCorrectInfo()
    {
        var fieldInfo = MqlFieldRegistry.GetField("title", "podcasts");

        fieldInfo.Should().NotBeNull();
        fieldInfo!.Name.Should().Be("title");
        fieldInfo.Type.Should().Be(MqlFieldType.String);
        fieldInfo.DefaultOperator.Should().Be("contains");
    }

    [Fact]
    public void GetField_ChannelField_ReturnsCorrectInfo()
    {
        var fieldInfo = MqlFieldRegistry.GetField("channel", "podcasts");

        fieldInfo.Should().NotBeNull();
        fieldInfo!.Name.Should().Be("channel");
        fieldInfo.Type.Should().Be(MqlFieldType.String);
        fieldInfo.DefaultOperator.Should().Be("contains");
    }

    [Fact]
    public void GetField_DurationField_ReturnsCorrectInfo()
    {
        var fieldInfo = MqlFieldRegistry.GetField("duration", "podcasts");

        fieldInfo.Should().NotBeNull();
        fieldInfo!.Name.Should().Be("duration");
        fieldInfo.Type.Should().Be(MqlFieldType.Number);
        fieldInfo.ValueMultiplier.Should().Be(1000);
    }

    [Fact]
    public void GetField_PublishedField_ReturnsCorrectInfo()
    {
        var fieldInfo = MqlFieldRegistry.GetField("published", "podcasts");

        fieldInfo.Should().NotBeNull();
        fieldInfo!.Name.Should().Be("published");
        fieldInfo.Type.Should().Be(MqlFieldType.Date);
    }

    [Fact]
    public void GetField_DateAlias_ReturnsSameAsPublished()
    {
        var publishedField = MqlFieldRegistry.GetField("published", "podcasts");
        var dateField = MqlFieldRegistry.GetField("date", "podcasts");

        publishedField.Should().NotBeNull();
        dateField.Should().NotBeNull();
        dateField!.Name.Should().Be(publishedField!.Name);
    }

    [Fact]
    public void GetField_DownloadedField_ReturnsCorrectInfo()
    {
        var fieldInfo = MqlFieldRegistry.GetField("downloaded", "podcasts");

        fieldInfo.Should().NotBeNull();
        fieldInfo!.Name.Should().Be("downloaded");
        fieldInfo.Type.Should().Be(MqlFieldType.Boolean);
    }
}
