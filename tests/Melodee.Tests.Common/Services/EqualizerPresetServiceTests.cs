using Melodee.Common.Services;

namespace Melodee.Tests.Common.Common.Services;

/// <summary>
/// Tests for EqualizerPresetService focusing on validation logic and static methods.
/// Database operations are tested through integration tests.
/// </summary>
public class EqualizerPresetServiceTests
{
    [Fact]
    public void ParseBands_WithValidJson_ReturnsBands()
    {
        var json = "[{\"Frequency\":32,\"Gain\":5},{\"Frequency\":64,\"Gain\":3}]";

        var bands = EqualizerPresetService.ParseBands(json);

        Assert.NotNull(bands);
        Assert.Equal(2, bands.Length);
        Assert.Equal(32, bands[0].Frequency);
        Assert.Equal(5, bands[0].Gain);
        Assert.Equal(64, bands[1].Frequency);
        Assert.Equal(3, bands[1].Gain);
    }

    [Fact]
    public void ParseBands_WithInvalidJson_ReturnsEmptyArray()
    {
        var json = "invalid json";

        var bands = EqualizerPresetService.ParseBands(json);

        Assert.NotNull(bands);
        Assert.Empty(bands);
    }

    [Fact]
    public void ParseBands_WithNullInput_ReturnsEmptyArray()
    {
        var bands = EqualizerPresetService.ParseBands(null!);

        Assert.NotNull(bands);
        Assert.Empty(bands);
    }

    [Fact]
    public void ParseBands_WithEmptyString_ReturnsEmptyArray()
    {
        var bands = EqualizerPresetService.ParseBands(string.Empty);

        Assert.NotNull(bands);
        Assert.Empty(bands);
    }

    [Fact]
    public void ParseBands_WithComplexBands_ParsesCorrectly()
    {
        var json = "[{\"Frequency\":31.25,\"Gain\":-12.5},{\"Frequency\":62.5,\"Gain\":0.0},{\"Frequency\":125,\"Gain\":6.5},{\"Frequency\":250,\"Gain\":8.0},{\"Frequency\":500,\"Gain\":4.5}]";

        var bands = EqualizerPresetService.ParseBands(json);

        Assert.NotNull(bands);
        Assert.Equal(5, bands.Length);
        Assert.Equal(31.25, bands[0].Frequency);
        Assert.Equal(-12.5, bands[0].Gain);
        Assert.Equal(8.0, bands[3].Gain);
    }

    [Theory]
    [InlineData("[{\"Frequency\":32,\"Gain\":5}]")]
    [InlineData("[{\"Frequency\":32,\"Gain\":5},{\"Frequency\":64,\"Gain\":3}]")]
    [InlineData("[{\"Frequency\":32,\"Gain\":5},{\"Frequency\":64,\"Gain\":3},{\"Frequency\":125,\"Gain\":0}]")]
    public void ParseBands_WithVariousBandCounts_ParsesCorrectly(string json)
    {
        var bands = EqualizerPresetService.ParseBands(json);

        Assert.NotNull(bands);
        Assert.NotEmpty(bands);
        Assert.True(bands.All(b => b.Frequency > 0));
    }

    [Fact]
    public void ParseBands_WithNegativeGain_HandlesCorrectly()
    {
        var json = "[{\"Frequency\":32,\"Gain\":-12},{\"Frequency\":64,\"Gain\":-6}]";

        var bands = EqualizerPresetService.ParseBands(json);

        Assert.NotNull(bands);
        Assert.Equal(2, bands.Length);
        Assert.Equal(-12, bands[0].Gain);
        Assert.Equal(-6, bands[1].Gain);
    }

    [Fact]
    public void ParseBands_WithZeroFrequency_ParsesAsIs()
    {
        var json = "[{\"Frequency\":0,\"Gain\":5}]";

        var bands = EqualizerPresetService.ParseBands(json);

        Assert.NotNull(bands);
        Assert.Single(bands);
        Assert.Equal(0, bands[0].Frequency);
    }

    [Fact]
    public void ParseBands_WithMalformedJson_ReturnsEmptyArray()
    {
        var json = "[{\"Frequency\":32,\"Gain\":}]";

        var bands = EqualizerPresetService.ParseBands(json);

        Assert.NotNull(bands);
        Assert.Empty(bands);
    }

    [Fact]
    public void ParseBands_WithIncompleteJson_ReturnsEmptyArray()
    {
        var json = "[{\"Frequency\":32";

        var bands = EqualizerPresetService.ParseBands(json);

        Assert.NotNull(bands);
        Assert.Empty(bands);
    }
}
