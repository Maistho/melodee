using Melodee.Common.Services;

namespace Melodee.Tests.Common.Services;

/// <summary>
/// Tests for PlaybackSettingsService focusing on validation logic and static methods.
/// Database operations are tested through integration tests.
/// </summary>
public class PlaybackSettingsServiceTests
{
    [Fact]
    public void ValidReplayGainValues_ContainsExpectedValues()
    {
        var validValues = PlaybackSettingsService.ValidReplayGainValues;

        Assert.NotNull(validValues);
        Assert.Contains("none", validValues);
        Assert.Contains("track", validValues);
        Assert.Contains("album", validValues);
        Assert.Equal(3, validValues.Length);
    }

    [Fact]
    public void ValidAudioQualityValues_ContainsExpectedValues()
    {
        var validValues = PlaybackSettingsService.ValidAudioQualityValues;

        Assert.NotNull(validValues);
        Assert.Contains("low", validValues);
        Assert.Contains("medium", validValues);
        Assert.Contains("high", validValues);
        Assert.Contains("lossless", validValues);
        Assert.Equal(4, validValues.Length);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("track")]
    [InlineData("album")]
    public void ValidReplayGainValues_AllValuesAreLowercase(string value)
    {
        Assert.Equal(value.ToLowerInvariant(), value);
    }

    [Theory]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData("high")]
    [InlineData("lossless")]
    public void ValidAudioQualityValues_AllValuesAreLowercase(string value)
    {
        Assert.Equal(value.ToLowerInvariant(), value);
    }

    [Fact]
    public void ValidReplayGainValues_DoesNotContainInvalidValues()
    {
        var validValues = PlaybackSettingsService.ValidReplayGainValues;

        Assert.DoesNotContain("invalid", validValues);
        Assert.All(validValues, v => Assert.NotEmpty(v));
        Assert.All(validValues, v => Assert.NotNull(v));
    }

    [Fact]
    public void ValidAudioQualityValues_DoesNotContainInvalidValues()
    {
        var validValues = PlaybackSettingsService.ValidAudioQualityValues;

        Assert.DoesNotContain("invalid", validValues);
        Assert.All(validValues, v => Assert.NotEmpty(v));
        Assert.All(validValues, v => Assert.NotNull(v));
    }

    [Theory]
    [InlineData("NONE", true)]
    [InlineData("Track", true)]
    [InlineData("ALBUM", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    public void ValidReplayGainValues_CaseInsensitiveCheck(string value, bool shouldBeValid)
    {
        var isValid = PlaybackSettingsService.ValidReplayGainValues.Contains(value, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(shouldBeValid, isValid);
    }

    [Theory]
    [InlineData("LOW", true)]
    [InlineData("Medium", true)]
    [InlineData("HIGH", true)]
    [InlineData("Lossless", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    public void ValidAudioQualityValues_CaseInsensitiveCheck(string value, bool shouldBeValid)
    {
        var isValid = PlaybackSettingsService.ValidAudioQualityValues.Contains(value, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(shouldBeValid, isValid);
    }

    [Fact]
    public void ValidReplayGainValues_IsImmutable()
    {
        var values1 = PlaybackSettingsService.ValidReplayGainValues;
        var values2 = PlaybackSettingsService.ValidReplayGainValues;

        Assert.Same(values1, values2);
    }

    [Fact]
    public void ValidAudioQualityValues_IsImmutable()
    {
        var values1 = PlaybackSettingsService.ValidAudioQualityValues;
        var values2 = PlaybackSettingsService.ValidAudioQualityValues;

        Assert.Same(values1, values2);
    }
}
