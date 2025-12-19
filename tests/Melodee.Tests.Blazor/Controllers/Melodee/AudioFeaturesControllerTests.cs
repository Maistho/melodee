using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for AudioFeaturesController.
/// </summary>
public class AudioFeaturesControllerTests
{
    #region Route Attribute Tests

    [Fact]
    public void Controller_HasCorrectRouteAttribute()
    {
        // Arrange
        var routeAttribute = typeof(AudioFeaturesController)
            .GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/audio");
    }

    #endregion

    #region GetAudioFeaturesAsync Tests

    [Fact]
    public void GetAudioFeaturesAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(AudioFeaturesController).GetMethod(nameof(AudioFeaturesController.GetAudioFeaturesAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetAudioFeaturesAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(AudioFeaturesController).GetMethod(nameof(AudioFeaturesController.GetAudioFeaturesAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("features/{id:guid}");
    }

    #endregion

    #region GetTracksByBpmAsync Tests

    [Fact]
    public void GetTracksByBpmAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(AudioFeaturesController).GetMethod(nameof(AudioFeaturesController.GetTracksByBpmAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetTracksByBpmAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(AudioFeaturesController).GetMethod(nameof(AudioFeaturesController.GetTracksByBpmAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("bpm");
    }

    [Fact]
    public void GetTracksByBpmAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(AudioFeaturesController).GetMethod(nameof(AudioFeaturesController.GetTracksByBpmAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(5);
        parameters[0].Name.Should().Be("min");
        parameters[0].ParameterType.Should().Be(typeof(double));
        parameters[1].Name.Should().Be("max");
        parameters[1].ParameterType.Should().Be(typeof(double));
        parameters[2].Name.Should().Be("page");
        parameters[3].Name.Should().Be("limit");
    }

    #endregion

    #region Model Tests

    [Fact]
    public void AudioFeatures_HasAllRequiredProperties()
    {
        // Arrange & Act
        var features = new AudioFeatures(
            Guid.NewGuid(),
            120.0,
            "C Major",
            "major",
            4,
            0.6,
            0.8,
            0.7,
            0.1,
            0.2,
            -8.0,
            0.05,
            0.75);

        // Assert
        features.Id.Should().NotBeEmpty();
        features.Tempo.Should().Be(120.0);
        features.Key.Should().Be("C Major");
        features.Mode.Should().Be("major");
        features.TimeSignature.Should().Be(4);
        features.Acousticness.Should().Be(0.6);
        features.Danceability.Should().Be(0.8);
        features.Energy.Should().Be(0.7);
        features.Instrumentalness.Should().Be(0.1);
        features.Liveness.Should().Be(0.2);
        features.Loudness.Should().Be(-8.0);
        features.Speechiness.Should().Be(0.05);
        features.Valence.Should().Be(0.75);
    }

    [Fact]
    public void BpmTrack_HasAllRequiredProperties()
    {
        // Arrange & Act
        var track = new BpmTrack(
            Guid.NewGuid(),
            "Test Song",
            "Test Artist",
            128.5);

        // Assert
        track.Id.Should().NotBeEmpty();
        track.Title.Should().Be("Test Song");
        track.Artist.Should().Be("Test Artist");
        track.Bpm.Should().Be(128.5);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void AudioFeatures_AcceptsValidNormalizedValues(double value)
    {
        // Arrange & Act
        var features = new AudioFeatures(
            Guid.NewGuid(),
            120.0,
            null,
            null,
            4,
            value,
            value,
            value,
            value,
            value,
            -10.0,
            value,
            value);

        // Assert
        features.Acousticness.Should().Be(value);
        features.Danceability.Should().Be(value);
        features.Energy.Should().Be(value);
        features.Instrumentalness.Should().Be(value);
        features.Liveness.Should().Be(value);
        features.Speechiness.Should().Be(value);
        features.Valence.Should().Be(value);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(90)]
    [InlineData(120)]
    [InlineData(140)]
    [InlineData(180)]
    public void BpmTrack_AcceptsVariousBpmValues(double bpm)
    {
        // Arrange & Act
        var track = new BpmTrack(
            Guid.NewGuid(),
            "Test",
            "Artist",
            bpm);

        // Assert
        track.Bpm.Should().Be(bpm);
    }

    #endregion
}
