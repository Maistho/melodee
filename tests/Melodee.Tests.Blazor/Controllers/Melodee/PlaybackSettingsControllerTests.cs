using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for PlaybackSettingsController.
/// </summary>
public class PlaybackSettingsControllerTests
{
    #region Route Attribute Tests

    [Fact]
    public void Controller_HasCorrectRouteAttribute()
    {
        // Arrange
        var routeAttribute = typeof(PlaybackSettingsController)
            .GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/playback/settings");
    }

    #endregion

    #region GetSettingsAsync Tests

    [Fact]
    public void GetSettingsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(PlaybackSettingsController).GetMethod(nameof(PlaybackSettingsController.GetSettingsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetSettingsAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(PlaybackSettingsController).GetMethod(nameof(PlaybackSettingsController.GetSettingsAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region UpdateSettingsAsync Tests

    [Fact]
    public void UpdateSettingsAsync_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PlaybackSettingsController).GetMethod(nameof(PlaybackSettingsController.UpdateSettingsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void UpdateSettingsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(PlaybackSettingsController).GetMethod(nameof(PlaybackSettingsController.UpdateSettingsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("request");
        parameters[0].ParameterType.Should().Be(typeof(UpdatePlaybackSettingsRequest));
        parameters[1].Name.Should().Be("cancellationToken");
    }

    #endregion

    #region Model Tests

    [Fact]
    public void PlaybackSettings_HasAllRequiredProperties()
    {
        // Arrange & Act
        var settings = new PlaybackSettings(
            2.5,
            true,
            false,
            "track",
            "high",
            "Rock",
            "iPhone");

        // Assert
        settings.CrossfadeDuration.Should().Be(2.5);
        settings.GaplessPlayback.Should().BeTrue();
        settings.VolumeNormalization.Should().BeFalse();
        settings.ReplayGain.Should().Be("track");
        settings.AudioQuality.Should().Be("high");
        settings.EqualizerPreset.Should().Be("Rock");
        settings.LastUsedDevice.Should().Be("iPhone");
    }

    [Fact]
    public void UpdatePlaybackSettingsRequest_AllowsNullFields()
    {
        // Arrange & Act
        var request = new UpdatePlaybackSettingsRequest(
            null,
            null,
            null,
            null,
            null,
            null);

        // Assert
        request.CrossfadeDuration.Should().BeNull();
        request.GaplessPlayback.Should().BeNull();
        request.VolumeNormalization.Should().BeNull();
        request.ReplayGain.Should().BeNull();
        request.AudioQuality.Should().BeNull();
        request.EqualizerPreset.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    public void UpdatePlaybackSettingsRequest_AcceptsValidCrossfadeDuration(double duration)
    {
        // Arrange & Act
        var request = new UpdatePlaybackSettingsRequest(
            duration,
            null,
            null,
            null,
            null,
            null);

        // Assert
        request.CrossfadeDuration.Should().Be(duration);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("track")]
    [InlineData("album")]
    public void UpdatePlaybackSettingsRequest_AcceptsValidReplayGainValues(string replayGain)
    {
        // Arrange & Act
        var request = new UpdatePlaybackSettingsRequest(
            null,
            null,
            null,
            replayGain,
            null,
            null);

        // Assert
        request.ReplayGain.Should().Be(replayGain);
    }

    [Theory]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData("high")]
    [InlineData("lossless")]
    public void UpdatePlaybackSettingsRequest_AcceptsValidAudioQualityValues(string quality)
    {
        // Arrange & Act
        var request = new UpdatePlaybackSettingsRequest(
            null,
            null,
            null,
            null,
            quality,
            null);

        // Assert
        request.AudioQuality.Should().Be(quality);
    }

    #endregion
}
