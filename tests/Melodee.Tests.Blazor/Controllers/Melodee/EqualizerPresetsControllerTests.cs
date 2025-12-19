using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for EqualizerPresetsController.
/// </summary>
public class EqualizerPresetsControllerTests
{
    #region Route Attribute Tests

    [Fact]
    public void Controller_HasCorrectRouteAttribute()
    {
        // Arrange
        var routeAttribute = typeof(EqualizerPresetsController)
            .GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/equalizer/presets");
    }

    #endregion

    #region ListPresetsAsync Tests

    [Fact]
    public void ListPresetsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(EqualizerPresetsController).GetMethod(nameof(EqualizerPresetsController.ListPresetsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ListPresetsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(EqualizerPresetsController).GetMethod(nameof(EqualizerPresetsController.ListPresetsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("page");
        parameters[1].Name.Should().Be("limit");
        parameters[2].Name.Should().Be("cancellationToken");
    }

    #endregion

    #region UpsertPresetAsync Tests

    [Fact]
    public void UpsertPresetAsync_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(EqualizerPresetsController).GetMethod(nameof(EqualizerPresetsController.UpsertPresetAsync));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void UpsertPresetAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(EqualizerPresetsController).GetMethod(nameof(EqualizerPresetsController.UpsertPresetAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("request");
        parameters[0].ParameterType.Should().Be(typeof(CreateEqualizerPresetRequest));
    }

    #endregion

    #region DeletePresetAsync Tests

    [Fact]
    public void DeletePresetAsync_HasHttpDeleteAttribute()
    {
        // Arrange
        var method = typeof(EqualizerPresetsController).GetMethod(nameof(EqualizerPresetsController.DeletePresetAsync));

        // Assert
        method.Should().NotBeNull();
        var httpDeleteAttribute = method!.GetCustomAttributes(typeof(HttpDeleteAttribute), false).FirstOrDefault();
        httpDeleteAttribute.Should().NotBeNull();
    }

    [Fact]
    public void DeletePresetAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(EqualizerPresetsController).GetMethod(nameof(EqualizerPresetsController.DeletePresetAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}");
    }

    #endregion

    #region Model Tests

    [Fact]
    public void EqualizerBand_HasCorrectProperties()
    {
        // Arrange & Act
        var band = new EqualizerBand(1000, 3.5);

        // Assert
        band.Frequency.Should().Be(1000);
        band.Gain.Should().Be(3.5);
    }

    [Fact]
    public void EqualizerPreset_HasAllRequiredProperties()
    {
        // Arrange
        var bands = new[]
        {
            new EqualizerBand(60, 2.0),
            new EqualizerBand(250, -1.5),
            new EqualizerBand(1000, 0),
            new EqualizerBand(4000, 1.5),
            new EqualizerBand(16000, 3.0)
        };

        // Act
        var preset = new EqualizerPreset(
            Guid.NewGuid(),
            "Rock",
            bands,
            true);

        // Assert
        preset.Id.Should().NotBeEmpty();
        preset.Name.Should().Be("Rock");
        preset.Bands.Should().HaveCount(5);
        preset.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void CreateEqualizerPresetRequest_HasCorrectProperties()
    {
        // Arrange
        var bands = new[]
        {
            new EqualizerBand(60, 0),
            new EqualizerBand(250, 0)
        };

        // Act
        var request = new CreateEqualizerPresetRequest(
            "Flat",
            bands,
            false);

        // Assert
        request.Name.Should().Be("Flat");
        request.Bands.Should().HaveCount(2);
        request.IsDefault.Should().BeFalse();
    }

    [Theory]
    [InlineData(20)]
    [InlineData(60)]
    [InlineData(250)]
    [InlineData(1000)]
    [InlineData(4000)]
    [InlineData(16000)]
    public void EqualizerBand_AcceptsVariousFrequencies(double frequency)
    {
        // Arrange & Act
        var band = new EqualizerBand(frequency, 0);

        // Assert
        band.Frequency.Should().Be(frequency);
    }

    [Theory]
    [InlineData(-12)]
    [InlineData(-6)]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(12)]
    public void EqualizerBand_AcceptsVariousGainValues(double gain)
    {
        // Arrange & Act
        var band = new EqualizerBand(1000, gain);

        // Assert
        band.Gain.Should().Be(gain);
    }

    #endregion
}
