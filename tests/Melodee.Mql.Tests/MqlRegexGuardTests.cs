using FluentAssertions;
using Melodee.Mql.Security;

namespace Melodee.Mql.Tests;

public class MqlRegexGuardTests
{
    private readonly MqlRegexGuard _regexGuard;

    public MqlRegexGuardTests()
    {
        _regexGuard = new MqlRegexGuard();
    }

    [Fact]
    public void ValidatePattern_AcceptsValidPattern()
    {
        var pattern = @"^[a-zA-Z0-9]+$";

        var result = _regexGuard.ValidatePattern(pattern);

        result.IsValid.Should().BeTrue();
        result.IsBlocked.Should().BeFalse();
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void ValidatePattern_RejectsEmptyPattern()
    {
        var pattern = string.Empty;

        var result = _regexGuard.ValidatePattern(pattern);

        result.IsValid.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
        result.ErrorCode.Should().Be("MQL_EMPTY_PATTERN");
    }

    [Fact]
    public void ValidatePattern_RejectsTooLongPattern()
    {
        var pattern = new string('a', 101);

        var result = _regexGuard.ValidatePattern(pattern);

        result.IsValid.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
        result.ErrorCode.Should().Be("MQL_REGEX_TOO_LONG");
    }

    [Fact]
    public void ValidatePattern_RejectsRedosPattern()
    {
        var pattern = "(a+)+";

        var result = _regexGuard.ValidatePattern(pattern);

        result.IsValid.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
        result.ErrorCode.Should().Be("MQL_REGEX_DANGEROUS");
    }

    [Fact]
    public void ValidatePattern_RejectsProhibitedPattern()
    {
        var pattern = "(.*)*";

        var result = _regexGuard.ValidatePattern(pattern);

        result.IsValid.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
        result.ErrorCode.Should().BeOneOf("MQL_REGEX_DANGEROUS", "MQL_REGEX_PROHIBITED");
    }

    [Fact]
    public void ValidatePattern_RejectsDoubleRepeat()
    {
        var pattern = "(.+)+";

        var result = _regexGuard.ValidatePattern(pattern);

        result.IsValid.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void ValidatePattern_RejectsNestedStar()
    {
        var pattern = "([a-z]*)*";

        var result = _regexGuard.ValidatePattern(pattern);

        result.IsValid.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void ValidatePattern_RejectsInvalidRegex()
    {
        var pattern = "[unclosed";

        var result = _regexGuard.ValidatePattern(pattern);

        result.IsValid.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
        result.ErrorCode.Should().Be("MQL_REGEX_INVALID");
    }

    [Fact]
    public void ValidatePattern_ReturnsSafePattern()
    {
        var pattern = @"test.value*";

        var result = _regexGuard.ValidatePattern(pattern);

        result.IsValid.Should().BeTrue();
        result.SafePattern.Should().NotBeNull();
        result.SafePattern.Should().NotContain(pattern);
    }

    [Fact]
    public void SafeMatch_ValidatesAndMatches()
    {
        var pattern = @"^[a-z]+$";
        var testString = "pinkfloyd";

        var result = _regexGuard.SafeMatch(pattern, testString);

        result.IsValid.Should().BeTrue();
        result.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void SafeMatch_RejectsNonMatchingString()
    {
        var pattern = @"^[0-9]+$";
        var testString = "not-a-number";

        var result = _regexGuard.SafeMatch(pattern, testString);

        result.IsValid.Should().BeTrue();
        result.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void SafeMatch_RejectsInvalidPattern()
    {
        var pattern = "[invalid";
        var testString = "test";

        var result = _regexGuard.SafeMatch(pattern, testString);

        result.IsValid.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void SafeMatch_TruncatesLongTestString()
    {
        var pattern = @"^[a-z]+$";
        var testString = new string('a', 20000);

        var result = _regexGuard.SafeMatch(pattern, testString);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SafeMatch_IncludesEvaluationTime()
    {
        var pattern = @"^[a-z]+$";
        var testString = "pinkfloyd";

        var result = _regexGuard.SafeMatch(pattern, testString);

        result.EvaluationTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }
}
