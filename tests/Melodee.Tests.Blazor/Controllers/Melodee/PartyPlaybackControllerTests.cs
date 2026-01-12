using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for PartyPlaybackController routes and attributes.
/// </summary>
public class PartyPlaybackControllerTests
{
    [Fact]
    public void PartyPlaybackController_HasCorrectRouteAttribute()
    {
        // Arrange
        var controllerType = typeof(PartyPlaybackController);

        // Assert
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/party-sessions/{sessionId:guid}/playback");
    }

    [Fact]
    public void PartyPlaybackController_HasApiControllerAttribute()
    {
        // Arrange
        var controllerType = typeof(PartyPlaybackController);

        // Assert
        var apiControllerAttribute = controllerType.GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        apiControllerAttribute.Should().NotBeNull();
    }

    [Fact]
    public void PartyPlaybackController_HasApiVersionAttribute()
    {
        // Arrange
        var controllerType = typeof(PartyPlaybackController);

        // Assert
        var apiVersionAttribute = controllerType.GetCustomAttributes(typeof(ApiVersionAttribute), false).FirstOrDefault() as ApiVersionAttribute;
        apiVersionAttribute.Should().NotBeNull();
        apiVersionAttribute!.Versions.Should().Contain(1);
    }

    [Fact]
    public void GetState_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.GetState));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetState_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.GetState));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().BeEmpty();
    }

    [Fact]
    public void Play_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.Play));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Play_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.Play));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("play");
    }

    [Fact]
    public void Pause_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.Pause));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Pause_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.Pause));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("pause");
    }

    [Fact]
    public void Skip_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.Skip));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Skip_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.Skip));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("skip");
    }

    [Fact]
    public void Seek_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.Seek));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Seek_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.Seek));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("seek");
    }

    [Fact]
    public void SetVolume_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.SetVolume));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void SetVolume_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyPlaybackController).GetMethod(nameof(PartyPlaybackController.SetVolume));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("volume");
    }
}
