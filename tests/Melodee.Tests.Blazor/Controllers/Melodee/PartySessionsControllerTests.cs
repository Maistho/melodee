using System.Reflection;
using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for PartySessionsController routes and attributes.
/// </summary>
public class PartySessionsControllerTests
{
    [Fact]
    public void PartySessionsController_HasCorrectRouteAttribute()
    {
        // Arrange
        var controllerType = typeof(PartySessionsController);

        // Assert
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/party-sessions");
    }

    [Fact]
    public void PartySessionsController_HasApiControllerAttribute()
    {
        // Arrange
        var controllerType = typeof(PartySessionsController);

        // Assert
        var apiControllerAttribute = controllerType.GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        apiControllerAttribute.Should().NotBeNull();
    }

    [Fact]
    public void PartySessionsController_HasApiVersionAttribute()
    {
        // Arrange
        var controllerType = typeof(PartySessionsController);

        // Assert
        var apiVersionAttribute = controllerType.GetCustomAttributes(typeof(ApiVersionAttribute), false).FirstOrDefault() as ApiVersionAttribute;
        apiVersionAttribute.Should().NotBeNull();
        apiVersionAttribute!.Versions.Should().Contain(1);
    }

    [Fact]
    public void Create_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.Create));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Get_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.Get));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Get_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.Get));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}");
    }

    [Fact]
    public void Join_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.Join));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Join_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.Join));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}/join");
    }

    [Fact]
    public void Leave_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.Leave));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Leave_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.Leave));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}/leave");
    }

    [Fact]
    public void End_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.End));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void End_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.End));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}/end");
    }

    [Fact]
    public void GetParticipants_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.GetParticipants));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetParticipants_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartySessionsController).GetMethod(nameof(PartySessionsController.GetParticipants));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{id:guid}/participants");
    }
}
