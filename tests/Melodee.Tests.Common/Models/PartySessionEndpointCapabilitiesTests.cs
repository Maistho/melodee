using Melodee.Common.Models.PartyMode;

namespace Melodee.Tests.Common.Models;

/// <summary>
/// Tests for PartySessionEndpointCapabilities model.
/// </summary>
public class PartySessionEndpointCapabilitiesTests
{
    [Fact]
    public void DefaultCapabilities_HasAllControlsEnabled()
    {
        var capabilities = new PartySessionEndpointCapabilities();

        Assert.True(capabilities.CanPlay);
        Assert.True(capabilities.CanPause);
        Assert.True(capabilities.CanSkip);
        Assert.True(capabilities.CanSeek);
        Assert.True(capabilities.CanSetVolume);
        Assert.True(capabilities.CanReportPosition);
    }

    [Fact]
    public void WebPlayerDefault_HasCorrectValues()
    {
        var capabilities = PartySessionEndpointCapabilities.WebPlayerDefault();

        Assert.True(capabilities.CanPlay);
        Assert.True(capabilities.CanPause);
        Assert.True(capabilities.CanSkip);
        Assert.True(capabilities.CanSeek);
        Assert.True(capabilities.CanSetVolume);
        Assert.True(capabilities.CanReportPosition);
        Assert.Equal("Web Player", capabilities.DisplayName);
    }

    [Fact]
    public void SystemBackendDefault_Mpv_HasExtendedFormats()
    {
        var capabilities = PartySessionEndpointCapabilities.SystemBackendDefault("mpv");

        Assert.Equal("mpv Backend", capabilities.DisplayName);
        Assert.Contains("m4a", capabilities.SupportedFormats);
        Assert.Contains("aac", capabilities.SupportedFormats);
        Assert.Contains("opus", capabilities.SupportedFormats);
    }

    [Fact]
    public void SystemBackendDefault_Mpd_HasCorrectFormats()
    {
        var capabilities = PartySessionEndpointCapabilities.SystemBackendDefault("mpd");

        Assert.Equal("mpd Backend", capabilities.DisplayName);
        Assert.Contains("m4a", capabilities.SupportedFormats);
        Assert.Contains("aac", capabilities.SupportedFormats);
        Assert.DoesNotContain("opus", capabilities.SupportedFormats);
    }

    [Fact]
    public void SystemBackendDefault_Unknown_HasDefaultFormats()
    {
        var capabilities = PartySessionEndpointCapabilities.SystemBackendDefault("unknown");

        Assert.Equal("unknown Backend", capabilities.DisplayName);
        Assert.Equal(new[] { "mp3", "flac", "ogg", "wav" }, capabilities.SupportedFormats);
    }

    [Fact]
    public void CanControl_ReturnsTrue_WhenControlIsEnabled()
    {
        var capabilities = new PartySessionEndpointCapabilities
        {
            CanPlay = true,
            CanPause = true,
            CanSkip = true,
            CanSeek = true,
            CanSetVolume = true
        };

        Assert.True(capabilities.CanControl(PlaybackControlType.Play));
        Assert.True(capabilities.CanControl(PlaybackControlType.Pause));
        Assert.True(capabilities.CanControl(PlaybackControlType.Skip));
        Assert.True(capabilities.CanControl(PlaybackControlType.Seek));
        Assert.True(capabilities.CanControl(PlaybackControlType.Volume));
    }

    [Fact]
    public void CanControl_ReturnsFalse_WhenControlIsDisabled()
    {
        var capabilities = new PartySessionEndpointCapabilities
        {
            CanPlay = false,
            CanPause = false,
            CanSkip = false,
            CanSeek = false,
            CanSetVolume = false
        };

        Assert.False(capabilities.CanControl(PlaybackControlType.Play));
        Assert.False(capabilities.CanControl(PlaybackControlType.Pause));
        Assert.False(capabilities.CanControl(PlaybackControlType.Skip));
        Assert.False(capabilities.CanControl(PlaybackControlType.Seek));
        Assert.False(capabilities.CanControl(PlaybackControlType.Volume));
    }

    [Fact]
    public void CanControl_ReturnsFalse_ForUnknownControl()
    {
        var capabilities = new PartySessionEndpointCapabilities();

        Assert.False(capabilities.CanControl((PlaybackControlType)999));
    }

    [Fact]
    public void CanControl_WithPartialCapabilities()
    {
        var capabilities = new PartySessionEndpointCapabilities
        {
            CanPlay = true,
            CanPause = true,
            CanSkip = false,
            CanSeek = false,
            CanSetVolume = true
        };

        Assert.True(capabilities.CanControl(PlaybackControlType.Play));
        Assert.True(capabilities.CanControl(PlaybackControlType.Pause));
        Assert.False(capabilities.CanControl(PlaybackControlType.Skip));
        Assert.False(capabilities.CanControl(PlaybackControlType.Seek));
        Assert.True(capabilities.CanControl(PlaybackControlType.Volume));
    }

    [Fact]
    public void DefaultVolumeRange_IsZeroToOne()
    {
        var capabilities = new PartySessionEndpointCapabilities();

        Assert.Equal(0.0, capabilities.MinVolume);
        Assert.Equal(1.0, capabilities.MaxVolume);
    }

    [Fact]
    public void DefaultSupportedFormats_ContainsCommonFormats()
    {
        var capabilities = new PartySessionEndpointCapabilities();

        Assert.Contains("mp3", capabilities.SupportedFormats);
        Assert.Contains("flac", capabilities.SupportedFormats);
        Assert.Contains("ogg", capabilities.SupportedFormats);
        Assert.Contains("wav", capabilities.SupportedFormats);
    }

    [Fact]
    public void JsonSerialization_RoundTrip()
    {
        var original = new PartySessionEndpointCapabilities
        {
            CanPlay = true,
            CanPause = false,
            CanSkip = true,
            CanSeek = false,
            CanSetVolume = true,
            CanReportPosition = true,
            DisplayName = "Test Player",
            AudioDevice = "default",
            MinVolume = 0.1,
            MaxVolume = 0.9
        };

        var json = System.Text.Json.JsonSerializer.Serialize(original);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<PartySessionEndpointCapabilities>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.CanPlay, deserialized.CanPlay);
        Assert.Equal(original.CanPause, deserialized.CanPause);
        Assert.Equal(original.CanSkip, deserialized.CanSkip);
        Assert.Equal(original.CanSeek, deserialized.CanSeek);
        Assert.Equal(original.CanSetVolume, deserialized.CanSetVolume);
        Assert.Equal(original.CanReportPosition, deserialized.CanReportPosition);
        Assert.Equal(original.DisplayName, deserialized.DisplayName);
        Assert.Equal(original.AudioDevice, deserialized.AudioDevice);
        Assert.Equal(original.MinVolume, deserialized.MinVolume);
        Assert.Equal(original.MaxVolume, deserialized.MaxVolume);
    }
}
