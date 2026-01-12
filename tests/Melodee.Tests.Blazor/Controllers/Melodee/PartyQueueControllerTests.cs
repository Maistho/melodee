using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for PartyQueueController routes and attributes.
/// </summary>
public class PartyQueueControllerTests
{
    [Fact]
    public void PartyQueueController_HasCorrectRouteAttribute()
    {
        // Arrange
        var controllerType = typeof(PartyQueueController);

        // Assert
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/party-sessions/{sessionId:guid}/queue");
    }

    [Fact]
    public void PartyQueueController_HasApiControllerAttribute()
    {
        // Arrange
        var controllerType = typeof(PartyQueueController);

        // Assert
        var apiControllerAttribute = controllerType.GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        apiControllerAttribute.Should().NotBeNull();
    }

    [Fact]
    public void PartyQueueController_HasApiVersionAttribute()
    {
        // Arrange
        var controllerType = typeof(PartyQueueController);

        // Assert
        var apiVersionAttribute = controllerType.GetCustomAttributes(typeof(ApiVersionAttribute), false).FirstOrDefault() as ApiVersionAttribute;
        apiVersionAttribute.Should().NotBeNull();
        apiVersionAttribute!.Versions.Should().Contain(1);
    }

    [Fact]
    public void Get_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(PartyQueueController).GetMethod(nameof(PartyQueueController.Get));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Get_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyQueueController).GetMethod(nameof(PartyQueueController.Get));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().BeEmpty();
    }

    [Fact]
    public void AddItems_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyQueueController).GetMethod(nameof(PartyQueueController.AddItems));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void AddItems_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyQueueController).GetMethod(nameof(PartyQueueController.AddItems));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("items");
    }

    [Fact]
    public void RemoveItem_HasHttpDeleteAttribute()
    {
        // Arrange
        var method = typeof(PartyQueueController).GetMethod(nameof(PartyQueueController.RemoveItem));

        // Assert
        method.Should().NotBeNull();
        var httpDeleteAttribute = method!.GetCustomAttributes(typeof(HttpDeleteAttribute), false).FirstOrDefault();
        httpDeleteAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RemoveItem_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyQueueController).GetMethod(nameof(PartyQueueController.RemoveItem));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("items/{itemId:guid}");
    }

    [Fact]
    public void ReorderItem_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyQueueController).GetMethod(nameof(PartyQueueController.ReorderItem));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ReorderItem_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyQueueController).GetMethod(nameof(PartyQueueController.ReorderItem));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("items/{itemId:guid}/reorder");
    }

    [Fact]
    public void Clear_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PartyQueueController).GetMethod(nameof(PartyQueueController.Clear));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void Clear_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PartyQueueController).GetMethod(nameof(PartyQueueController.Clear));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("clear");
    }
}
