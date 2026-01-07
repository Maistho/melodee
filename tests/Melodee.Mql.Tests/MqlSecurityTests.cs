using FluentAssertions;
using Melodee.Mql.Security;

namespace Melodee.Mql.Tests;

public class MqlSecurityTests
{
    [Fact]
    public void SanitizeForFreeText_RemovesDangerousCharacters()
    {
        var input = "Test; DROP TABLE-- ' OR '1'='1";
        var result = MqlTextSanitizer.SanitizeForFreeText(input);

        result.Should().Contain("\\;");
        result.Should().Contain("\\-\\-");
        result.Should().Contain("\\'");
    }

    [Fact]
    public void SanitizeForFreeText_EscapesSpecialCharacters()
    {
        var input = "test(input) and [brackets]";
        var result = MqlTextSanitizer.SanitizeForFreeText(input);

        result.Should().Contain("\\(");
        result.Should().Contain("\\)");
        result.Should().Contain("\\[");
        result.Should().Contain("\\]");
    }

    [Fact]
    public void SanitizeForFreeText_ReplacesControlCharacters()
    {
        var input = "test\nvalue\twith\rcontrol";
        var result = MqlTextSanitizer.SanitizeForFreeText(input);

        result.Should().NotContain("\n");
        result.Should().NotContain("\r");
        result.Should().NotContain("\t");
    }

    [Fact]
    public void SanitizeForFreeText_HandlesNullInput()
    {
        var result = MqlTextSanitizer.SanitizeForFreeText(null!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeForFreeText_HandlesEmptyInput()
    {
        var result = MqlTextSanitizer.SanitizeForFreeText(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeForRegex_EscapesMetaCharacters()
    {
        var input = "test.pattern+quantifier*group";
        var result = MqlTextSanitizer.SanitizeForRegex(input);

        result.Should().Contain("\\.");
        result.Should().Contain("\\+");
        result.Should().Contain("\\*");
    }

    [Fact]
    public void ContainsDangerousPatterns_DetectsSqlInjection()
    {
        var maliciousInput = "'; DROP TABLE users;--";

        var result = MqlTextSanitizer.ContainsDangerousPatterns(maliciousInput);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsDangerousPatterns_DetectsUnionSelect()
    {
        var maliciousInput = "' UNION SELECT * FROM users--";

        var result = MqlTextSanitizer.ContainsDangerousPatterns(maliciousInput);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsDangerousPatterns_DetectsExecXp()
    {
        var maliciousInput = "EXEC xp_cmdshell";

        var result = MqlTextSanitizer.ContainsDangerousPatterns(maliciousInput);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsDangerousPatterns_DetectsHexInjection()
    {
        var maliciousInput = "0x41424344";

        var result = MqlTextSanitizer.ContainsDangerousPatterns(maliciousInput);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsDangerousPatterns_AcceptsNormalInput()
    {
        var normalInput = "Pink Floyd - The Dark Side of the Moon";

        var result = MqlTextSanitizer.ContainsDangerousPatterns(normalInput);

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsDangerousPatterns_HandlesNullInput()
    {
        var result = MqlTextSanitizer.ContainsDangerousPatterns(null!);

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsRedosPattern_DetectsNestedQuantifiers()
    {
        var dangerousPattern = "(a+)+";

        var result = MqlTextSanitizer.ContainsRedosPattern(dangerousPattern);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsRedosPattern_DetectsDoubleRepeat()
    {
        var dangerousPattern = "(.*)*";

        var result = MqlTextSanitizer.ContainsRedosPattern(dangerousPattern);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsRedosPattern_DetectsExponentialPattern()
    {
        var dangerousPattern = "(x+x+y)+";

        var result = MqlTextSanitizer.ContainsRedosPattern(dangerousPattern);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsRedosPattern_AcceptsSafePattern()
    {
        var safePattern = "^[a-zA-Z0-9]+$";

        var result = MqlTextSanitizer.ContainsRedosPattern(safePattern);

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsRedosPattern_RejectsTooLongPattern()
    {
        var longPattern = new string('a', 101);

        var result = MqlTextSanitizer.ContainsRedosPattern(longPattern);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidFreeText_AcceptsValidInput()
    {
        var input = "Pink Floyd - The Dark Side of the Moon";

        var result = MqlTextSanitizer.IsValidFreeText(input);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidFreeText_RejectsSpecialCharacters()
    {
        var input = "test; DROP--";

        var result = MqlTextSanitizer.IsValidFreeText(input);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidFieldName_AcceptsValidFieldName()
    {
        var fieldName = "artist";

        var result = MqlTextSanitizer.IsValidFieldName(fieldName);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidFieldName_RejectsInvalidFieldName()
    {
        var fieldName = "123artist";

        var result = MqlTextSanitizer.IsValidFieldName(fieldName);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidNumeric_AcceptsValidNumber()
    {
        var value = "123.45";

        var result = MqlTextSanitizer.IsValidNumeric(value);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidNumeric_RejectsInvalidNumber()
    {
        var value = "abc";

        var result = MqlTextSanitizer.IsValidNumeric(value);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidDate_AcceptsValidDate()
    {
        var value = "2025-01-15";

        var result = MqlTextSanitizer.IsValidDate(value);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidDate_RejectsInvalidDate()
    {
        var value = "not-a-date";

        var result = MqlTextSanitizer.IsValidDate(value);

        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveDangerousCharacters_RemovesNullBytes()
    {
        var input = "test\0value";

        var result = MqlTextSanitizer.RemoveDangerousCharacters(input);

        result.Should().NotContain("\0");
    }

    [Fact]
    public void RemoveDangerousCharacters_PreservesValidText()
    {
        var input = "Pink Floyd - The Dark Side of the Moon";

        var result = MqlTextSanitizer.RemoveDangerousCharacters(input);

        result.Should().Be(input);
    }
}
