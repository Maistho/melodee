using System.Reflection;
using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for PartyEndpointsController routes and attributes.
/// </summary>
public class PartyEndpointsControllerTests
{
    [Fact]
    public void PartyEndpointsController_HasCorrectRouteAttribute()
    {
        // Arrange
        var controllerType = typeof(PartyEndpointsController);

        // Assert
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/party-endpoints");
    }

    [Fact]
    public void PartyEndpointsController_HasApiControllerAttribute()
    {
        // Arrange
        var controllerType = typeof(PartyEndpointsController);

        // Assert
        var apiControllerAttribute = controllerType.GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        apiControllerAttribute.Should().NotBeNull();
    }

    [Fact]
    public void PartyEndpointsController_HasApiVersionAttribute()
    {
        // Arrange
        var controllerType = typeof(PartyEndpointsController);

        // Assert
        var apiVersionAttribute = controllerType.GetCustomAttributes(typeof(ApiVersionAttribute), false).FirstOrDefault() as ApiVersionAttribute;
        apiVersionAttribute.Should().NotBeNull();
        apiVersionAttribute!.Versions.Should().Contain(1);
    }

    [Fact]
    public void Register_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.Register));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Register_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.Register));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("register");
    }

    [Fact]
    public void Get_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.Get));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Get_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.Get));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}");
    }

    [Fact]
    public void UpdateCapabilities_HasHttpPutAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.UpdateCapabilities));

        // Assert
        method.Should().NotBeNull();
        var httpPutAttribute = method!.GetCustomAttributes(typeof(HttpPutAttribute), false).FirstOrDefault();
        httpPutAttribute.Should().NotBeNull();
    }

    [Fact]
    public void UpdateCapabilities_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.UpdateCapabilities));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}/capabilities");
    }

    [Fact]
    public void Heartbeat_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.Heartbeat));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Heartbeat_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.Heartbeat));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}/heartbeat");
    }

    [Fact]
    public void AttachToSession_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.AttachToSession));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void AttachToSession_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.AttachToSession));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}/attach");
    }

    [Fact]
    public void Detach_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.Detach));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Detach_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyEndpointsController).GetMethod(nameof(PartyEndpointsController.Detach));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}/detach");
    }
}
