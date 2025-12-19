using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for GenresController favorites/dislikes endpoints.
/// </summary>
public class GenresControllerFavoritesTests
{
    #region Toggle Starred Tests

    [Fact]
    public void ToggleGenreStarred_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ToggleGenreStarred));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("starred/{id}/{isStarred:bool}");
    }

    [Fact]
    public void ToggleGenreStarred_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ToggleGenreStarred));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ToggleGenreStarred_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ToggleGenreStarred));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("id");
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[1].Name.Should().Be("isStarred");
        parameters[1].ParameterType.Should().Be(typeof(bool));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void ToggleGenreStarred_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ToggleGenreStarred));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Toggle Hated Tests

    [Fact]
    public void ToggleGenreHated_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ToggleGenreHated));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("hated/{id}/{isHated:bool}");
    }

    [Fact]
    public void ToggleGenreHated_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ToggleGenreHated));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ToggleGenreHated_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ToggleGenreHated));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("id");
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[1].Name.Should().Be("isHated");
        parameters[1].ParameterType.Should().Be(typeof(bool));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void ToggleGenreHated_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.ToggleGenreHated));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Get Starred Genres Tests

    [Fact]
    public void GetStarredGenres_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GetStarredGenres));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("starred");
    }

    [Fact]
    public void GetStarredGenres_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GetStarredGenres));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetStarredGenres_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GetStarredGenres));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].Name.Should().Be("cancellationToken");
        parameters[0].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void GetStarredGenres_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GetStarredGenres));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Get Hated Genres Tests

    [Fact]
    public void GetHatedGenres_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GetHatedGenres));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("hated");
    }

    [Fact]
    public void GetHatedGenres_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GetHatedGenres));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetHatedGenres_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GetHatedGenres));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].Name.Should().Be("cancellationToken");
        parameters[0].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void GetHatedGenres_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(GenresController).GetMethod(nameof(GenresController.GetHatedGenres));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion
}
