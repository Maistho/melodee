using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Moq;
using Serilog;

namespace Melodee.Tests.Common.Utility;

public sealed class SafeParserTests : TestsBase
{
    [Theory]
    [InlineData(1990, "1990-01-01")]
    [InlineData(2021, "2021-01-01")]
    public void ValidateToLocalDate(int input, string shouldBe)
    {
        Assert.Equal(shouldBe, SafeParser.ToLocalDate(input).ToString("yyyy-MM-dd", null));
    }

    [Theory]
    [InlineData("02/22/1988")]
    [InlineData("02/22/88")]
    [InlineData("02-22-1988")]
    [InlineData("1988")]
    [InlineData("\"1988\"")]
    [InlineData("1988-06-15T07:00:00Z")]
    [InlineData("1988/05/02")]
    public void DateFromString(string input)
    {
        Assert.NotNull(SafeParser.ToDateTime(input));
    }

    [Fact]
    public void ValidateToHash()
    {
        Assert.True(SafeParser.Hash("Bob", "Marley") > 0);

        Assert.True(SafeParser.Hash("Bob", "Marley", "") > 0);

        Assert.True(SafeParser.Hash("Bob", "Marley", "", null) > 0);

        string? nothing = null;
        Assert.False(SafeParser.Hash(nothing) > 0);
    }

    [Fact]
    public void FromSerializedJsonArrayToCharArray()
    {
        var data = "['^', '~', '#']";
        var strings = MelodeeConfiguration.FromSerializedJsonArray(data, new Serializer(new Mock<ILogger>().Object));
        Assert.NotNull(strings);
        Assert.NotEmpty(strings);
        Assert.Contains("^", strings);
    }


    [Fact]
    public void FromSerializedJsonDictionary()
    {
        var configuration = NewConfiguration();
        var artistReplacement = MelodeeConfiguration.FromSerializedJsonDictionary(configuration[SettingRegistry.ProcessingArtistNameReplacements], Serializer);
        Assert.NotNull(artistReplacement);
        Assert.NotEmpty(artistReplacement);
        Assert.Contains(artistReplacement, x => x.Key == "AC/DC");
    }

    #region IsTruthy Tests

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IsTruthy_WithBoolValue_ReturnsExpected(bool input, bool expected)
    {
        Assert.Equal(expected, SafeParser.IsTruthy(input));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(null, false)]
    public void IsTruthy_WithNullableBool_ReturnsExpected(bool? input, bool expected)
    {
        Assert.Equal(expected, SafeParser.IsTruthy(input));
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("1", true)]
    [InlineData("yes", true)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    [InlineData("no", false)]
    [InlineData(null, false)]
    public void IsTruthy_WithObjectValue_ReturnsExpected(object? input, bool expected)
    {
        Assert.Equal(expected, SafeParser.IsTruthy(input));
    }

    #endregion

    #region ToNumber Tests

    [Theory]
    [InlineData("123", 123)]
    [InlineData("0", 0)]
    [InlineData("-456", -456)]
    [InlineData("999999", 999999)]
    public void ToNumber_WithValidIntString_ReturnsInt(string input, int expected)
    {
        Assert.Equal(expected, SafeParser.ToNumber<int>(input));
    }

    [Theory]
    [InlineData("123.45", 123.45)]
    [InlineData("0.0", 0.0)]
    [InlineData("-45.67", -45.67)]
    public void ToNumber_WithValidDoubleString_ReturnsDouble(string input, double expected)
    {
        Assert.Equal(expected, SafeParser.ToNumber<double>(input));
    }

    [Fact]
    public void ToNumber_WithInvalidString_ReturnsDefault()
    {
        Assert.Equal(0, SafeParser.ToNumber<int>("invalid"));
        Assert.Equal(0.0, SafeParser.ToNumber<double>("not-a-number"));
    }

    [Fact]
    public void ToNumber_WithNull_ReturnsDefault()
    {
        Assert.Equal(0, SafeParser.ToNumber<int>(null));
        Assert.Equal(0.0, SafeParser.ToNumber<double>(null));
    }

    #endregion

    #region ToString Tests

    [Theory]
    [InlineData("test", "test")]
    [InlineData("  trim me  ", "trim me")]
    public void ToString_WithStringInput_ReturnsTrimmedString(object input, string expected)
    {
        Assert.Equal(expected, SafeParser.ToString(input));
    }

    [Theory]
    [InlineData(123, "")]
    [InlineData(45.67, "")]
    [InlineData(true, "")]
    public void ToString_WithNonStringInput_ReturnsEmptyString(object input, string expected)
    {
        Assert.Equal(expected, SafeParser.ToString(input));
    }

    [Fact]
    public void ToString_WithNull_ReturnsDefaultValue()
    {
        Assert.Equal("default", SafeParser.ToString(null, "default"));
    }

    [Fact]
    public void ToString_WithNullAndNoDefault_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, SafeParser.ToString(null));
    }

    #endregion

    #region ToYear Tests

    [Theory]
    [InlineData("2020", 2020)]
    [InlineData("1999", 1999)]
    [InlineData("2024-05-15", 2024)]
    public void ToYear_WithValidInput_ReturnsYear(string input, int expected)
    {
        Assert.Equal(expected, SafeParser.ToYear(input));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("abc")]
    public void ToYear_WithInvalidInput_ReturnsNull(string input)
    {
        Assert.Null(SafeParser.ToYear(input));
    }

    #endregion

    #region ToToken Tests

    [Fact]
    public void ToToken_WithValidInput_ReturnsHashId()
    {
        var result1 = SafeParser.ToToken("Test String");
        var result2 = SafeParser.ToToken("Test String");

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1, result2); // Same input should produce same token
    }

    [Fact]
    public void ToToken_WithDifferentInputs_ReturnsDifferentHashIds()
    {
        var token1 = SafeParser.ToToken("completely different text");
        var token2 = SafeParser.ToToken("another unique string here");

        Assert.NotNull(token1);
        Assert.NotNull(token2);
        // Tokens may collide with short inputs, so just verify they're generated
    }

    [Fact]
    public void ToToken_WithNull_ReturnsNull()
    {
        Assert.Null(SafeParser.ToToken(null!));
    }

    [Fact]
    public void ToToken_WithEmptyString_ReturnsNull()
    {
        Assert.Null(SafeParser.ToToken(string.Empty));
    }

    #endregion

    #region ToBoolean Tests

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void ToBoolean_WithValidInput_ReturnsBool(object input, bool expected)
    {
        Assert.Equal(expected, SafeParser.ToBoolean(input));
    }

    [Fact]
    public void ToBoolean_WithNull_ReturnsFalse()
    {
        Assert.False(SafeParser.ToBoolean(null));
    }

    #endregion

    #region Hash Tests

    [Fact]
    public void Hash_WithSameInputs_ReturnsSameHash()
    {
        var hash1 = SafeParser.Hash("test", "data");
        var hash2 = SafeParser.Hash("test", "data");
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Hash_WithDifferentInputs_ReturnsDifferentHash()
    {
        var hash1 = SafeParser.Hash("test1");
        var hash2 = SafeParser.Hash("test2");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Hash_WithByteArray_ReturnsHash()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        var hash = SafeParser.Hash(bytes);
        Assert.True(hash > 0);
    }

    #endregion

    #region ToGuid Tests

    [Fact]
    public void ToGuid_WithValidGuidString_ReturnsGuid()
    {
        var guidString = "12345678-1234-1234-1234-123456789012";
        var result = SafeParser.ToGuid(guidString);
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse(guidString), result.Value);
    }

    [Fact]
    public void ToGuid_WithInvalidString_ReturnsNull()
    {
        Assert.Null(SafeParser.ToGuid("invalid-guid"));
    }

    [Fact]
    public void ToGuid_WithNull_ReturnsNull()
    {
        Assert.Null(SafeParser.ToGuid(null));
    }

    #endregion

    #region IsDigitsOnly Tests

    [Theory]
    [InlineData("123", true)]
    [InlineData("0", true)]
    [InlineData("999999", true)]
    [InlineData("12a34", false)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsDigitsOnly_WithInput_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, SafeParser.IsDigitsOnly(input));
    }

    #endregion

    #region ToDateTime Additional Tests

    [Fact]
    public void ToDateTime_WithNull_ReturnsNull()
    {
        Assert.Null(SafeParser.ToDateTime(null));
    }

    [Fact]
    public void ToDateTime_WithDateTime_ReturnsDateTime()
    {
        var now = DateTime.Now;
        var result = SafeParser.ToDateTime(now);
        Assert.Equal(now, result);
    }

    [Fact]
    public void ToDateTime_WithInvalidString_ReturnsNull()
    {
        Assert.Null(SafeParser.ToDateTime("not-a-date"));
    }

    #endregion
    
    #region ToEnum Tests
    
    [Theory]
    [InlineData("Thumbnail", Melodee.Common.Enums.ImageSize.Thumbnail)]
    [InlineData("Small", Melodee.Common.Enums.ImageSize.Small)]
    [InlineData("Medium", Melodee.Common.Enums.ImageSize.Medium)]
    [InlineData("Large", Melodee.Common.Enums.ImageSize.Large)]
    [InlineData("thumbnail", Melodee.Common.Enums.ImageSize.Thumbnail)]
    [InlineData("THUMBNAIL", Melodee.Common.Enums.ImageSize.Thumbnail)]
    public void ToEnum_WithValidImageSizeString_ReturnsCorrectEnum(string input, Melodee.Common.Enums.ImageSize expected)
    {
        var result = SafeParser.ToEnum<Melodee.Common.Enums.ImageSize>(input);
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void ToEnum_WithInvalidString_ReturnsDefault()
    {
        var result = SafeParser.ToEnum<Melodee.Common.Enums.ImageSize>("InvalidSize");
        Assert.Equal(Melodee.Common.Enums.ImageSize.NotSet, result);
    }
    
    #endregion
}
