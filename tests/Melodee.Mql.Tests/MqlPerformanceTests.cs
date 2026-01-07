using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FluentAssertions;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql.Tests;

public class MqlPerformanceTests
{
    private readonly MqlTokenizer _tokenizer;
    private readonly MqlParser _parser;
    private readonly MqlCachedCompiler _compiler;
    private readonly MqlValidator _validator;

    public MqlPerformanceTests()
    {
        _tokenizer = new MqlTokenizer();
        _parser = new MqlParser();
        _compiler = new MqlCachedCompiler();
        _validator = new MqlValidator();
    }

    [Fact]
    public void SimpleQuery_Tokenization_Under5ms()
    {
        var query = "artist:Beatles";
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _tokenizer.Tokenize(query).ToList();
        }
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(500, "100 tokenizations should complete quickly");
        var avgMs = sw.ElapsedMilliseconds / 100.0;
        avgMs.Should().BeLessThan(5, "average tokenization should be under 5ms");
    }

    [Fact]
    public void SimpleQuery_Parsing_Under10ms()
    {
        var query = "artist:Beatles";
        var tokens = _tokenizer.Tokenize(query).ToList();

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _parser.Parse(tokens, "songs");
        }
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(1000, "100 parses should complete quickly");
        var avgMs = sw.ElapsedMilliseconds / 100.0;
        avgMs.Should().BeLessThan(10, "average parsing should be under 10ms");
    }

    [Fact]
    public void SimpleQuery_Compilation_Under50ms()
    {
        var query = "artist:Beatles";
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "songs");
        parseResult.Ast.Should().NotBeNull();

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _compiler.CompileSong(parseResult.Ast!);
        }
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(5000, "100 compilations should complete within budget");
        var avgMs = sw.ElapsedMilliseconds / 100.0;
        avgMs.Should().BeLessThan(50, "average compilation should be under 50ms");
    }

    [Fact]
    public void MediumQuery_Parsing_Under20ms()
    {
        var query = "artist:\"Pink Floyd\" AND year:>=1970 AND genre:Rock";
        var tokens = _tokenizer.Tokenize(query).ToList();

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _parser.Parse(tokens, "songs");
        }
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(2000, "100 medium parses should complete quickly");
        var avgMs = sw.ElapsedMilliseconds / 100.0;
        avgMs.Should().BeLessThan(20, "average medium parsing should be under 20ms");
    }

    [Fact]
    public void ComplexQuery_Parsing_Under50ms()
    {
        var query = "(artist:Beatles OR artist:Pink Floyd) AND (year:1970-1980 OR year:1990-2000) AND NOT live AND rating:>=4";
        var tokens = _tokenizer.Tokenize(query).ToList();

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _parser.Parse(tokens, "songs");
        }
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(5000, "100 complex parses should complete within budget");
        var avgMs = sw.ElapsedMilliseconds / 100.0;
        avgMs.Should().BeLessThan(50, "average complex parsing should be under 50ms");
    }

    [Fact]
    public void Validation_SimpleQuery_Under5ms()
    {
        var query = "artist:Beatles";
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _validator.Validate(query, "songs");
        }
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(500, "100 validations should complete quickly");
        var avgMs = sw.ElapsedMilliseconds / 100.0;
        avgMs.Should().BeLessThan(5, "average validation should be under 5ms");
    }

    [Fact]
    public void Cache_Effectiveness_RepeatedQuery()
    {
        var query = "artist:\"The Beatles\"";
        var tokens = _tokenizer.Tokenize(query).ToList();
        var parseResult = _parser.Parse(tokens, "songs");
        parseResult.Ast.Should().NotBeNull();

        // First compilation - cache miss
        var sw1 = Stopwatch.StartNew();
        var expression1 = _compiler.CompileSong(parseResult.Ast!);
        sw1.Stop();
        var firstTime = sw1.ElapsedMilliseconds;

        // Subsequent compilations - cache hits
        var sw2 = Stopwatch.StartNew();
        for (int i = 0; i < 10; i++)
        {
            _compiler.CompileSong(parseResult.Ast!);
        }
        sw2.Stop();
        var cacheHitTime = sw2.ElapsedMilliseconds / 10.0;

        cacheHitTime.Should().BeLessThanOrEqualTo(firstTime, "cached compilations should be as fast or faster");
        cacheHitTime.Should().BeLessThan(10, "cached compilation should be very fast");
    }

    [Fact]
    public void Cache_DifferentQueries_SeparateEntries()
    {
        var queries = new[]
        {
            "artist:Beatles",
            "artist:Pink Floyd",
            "year:>=1970",
            "genre:Rock"
        };

        foreach (var query in queries)
        {
            var tokens = _tokenizer.Tokenize(query).ToList();
            var parseResult = _parser.Parse(tokens, "songs");
            _compiler.CompileSong(parseResult.Ast!);
        }

        var stats = _compiler.Cache.GetStatistics();
        stats.EntryCount.Should().BeGreaterThanOrEqualTo(queries.Length, "each query should have a cached entry");
    }

    [Fact]
    public void ManyFields_Validation_StillFast()
    {
        var query = "artist:Beatles AND year:>=1970 AND genre:Rock AND mood:Chill";

        var sw = Stopwatch.StartNew();
        var result = _validator.Validate(query, "songs");
        sw.Stop();

        result.IsValid.Should().BeTrue();
        sw.ElapsedMilliseconds.Should().BeLessThan(50, "validation with many fields should still be fast");
    }

    [Fact]
    public void LongQuery_Validation_Under20ms()
    {
        var query = new string('a', 500);
        var sw = Stopwatch.StartNew();
        var result = _validator.Validate(query, "songs");
        sw.Stop();

        result.IsValid.Should().BeTrue();
        sw.ElapsedMilliseconds.Should().BeLessThan(20, "validation of max-length query should be fast");
    }

    [Fact]
    public void DeepNesting_Parsing_StillHandles()
    {
        var query = new StringBuilder();
        for (int i = 0; i < 10; i++)
            query.Append('(');
        query.Append("artist:Beatles");
        for (int i = 0; i < 10; i++)
            query.Append(')');

        var tokens = _tokenizer.Tokenize(query.ToString()).ToList();
        var sw = Stopwatch.StartNew();
        var result = _parser.Parse(tokens, "songs");
        sw.Stop();

        result.IsValid.Should().BeTrue();
        sw.ElapsedMilliseconds.Should().BeLessThan(100, "parsing deeply nested queries should complete within budget");
    }

    [Fact]
    public void Tokenizer_PositionTracking_Accurate()
    {
        var query = "artist:Beatles";
        var tokens = _tokenizer.Tokenize(query).ToList();

        tokens.Should().Contain(t => t.Value == "artist" && t.StartPosition == 0 && t.Column == 1);
        tokens.Should().Contain(t => t.Value == ":" && t.StartPosition == 6);
        tokens.Should().Contain(t => t.Value == "Beatles" && t.StartPosition == 7);
    }

    [Fact]
    public void Tokenizer_Unicode_HandledEfficiently()
    {
        var query = "artist:\"日本語テスト\"";
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _tokenizer.Tokenize(query).ToList();
        }
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(500, "100 unicode tokenizations should complete quickly");
    }

    [Fact]
    public void Parser_NormalizedQuery_Generated()
    {
        var query = "artist:Beatles AND year:>=1970";
        var tokens = _tokenizer.Tokenize(query).ToList();
        var result = _parser.Parse(tokens, "songs");

        result.IsValid.Should().BeTrue();
        result.NormalizedQuery.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ManyRegexPatterns_ValidationStillFast()
    {
        var query = "title:/test1/ AND title:/test2/ AND title:/test3/";
        var sw = Stopwatch.StartNew();
        var result = _validator.Validate(query, "songs");
        sw.Stop();

        result.IsValid.Should().BeTrue();
        sw.ElapsedMilliseconds.Should().BeLessThan(50, "validation with multiple regex patterns should be fast");
    }

    [Fact]
    public void Compilation_CacheMemory_Reasonable()
    {
        var queries = Enumerable.Range(0, 100)
            .Select(i => $"artist:Artist{i}")
            .ToList();

        foreach (var query in queries)
        {
            var tokens = _tokenizer.Tokenize(query).ToList();
            var parseResult = _parser.Parse(tokens, "songs");
            if (parseResult.IsValid)
            {
                _compiler.CompileSong(parseResult.Ast!);
            }
        }

        var stats = _compiler.Cache.GetStatistics();
        stats.EntryCount.Should().BeLessThanOrEqualTo(100, "cache should not exceed query count");
    }
}
