using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for AlbumsController starred, setrating, and hated endpoints.
/// </summary>
public class AlbumsControllerTests
{
    #region Route Tests

    [Fact]
    public void ToggleAlbumStarred_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.ToggleAlbumStarred));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("starred/{apiKey:guid}/{isStarred:bool}");
    }

    [Fact]
    public void ToggleAlbumStarred_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.ToggleAlbumStarred));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void SetAlbumRating_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.SetAlbumRating));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("setrating/{apiKey:guid}/{rating:int}");
    }

    [Fact]
    public void SetAlbumRating_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.SetAlbumRating));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ToggleAlbumHated_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.ToggleAlbumHated));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("hated/{apiKey:guid}/{isHated:bool}");
    }

    [Fact]
    public void ToggleAlbumHated_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.ToggleAlbumHated));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void ToggleAlbumStarred_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.ToggleAlbumStarred));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("isStarred");
        parameters[1].ParameterType.Should().Be(typeof(bool));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void SetAlbumRating_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.SetAlbumRating));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("rating");
        parameters[1].ParameterType.Should().Be(typeof(int));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void ToggleAlbumHated_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.ToggleAlbumHated));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("isHated");
        parameters[1].ParameterType.Should().Be(typeof(bool));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void ToggleAlbumStarred_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.ToggleAlbumStarred));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    [Fact]
    public void SetAlbumRating_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.SetAlbumRating));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    [Fact]
    public void ToggleAlbumHated_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(AlbumsController).GetMethod(nameof(AlbumsController.ToggleAlbumHated));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Controller Attribute Tests

    [Fact]
    public void AlbumsController_HasApiControllerAttribute()
    {
        // Arrange & Assert
        var attribute = typeof(AlbumsController).GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void AlbumsController_HasCorrectRoutePrefix()
    {
        // Arrange
        var routeAttribute = typeof(AlbumsController).GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/[controller]");
    }

    #endregion
}
