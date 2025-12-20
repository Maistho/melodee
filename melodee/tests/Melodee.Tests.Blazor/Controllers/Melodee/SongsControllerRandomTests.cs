using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for SongsController random songs endpoint.
/// </summary>
public class SongsControllerRandomTests
{
    [Fact]
    public void RandomSongsAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.RandomSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("random");
    }

    [Fact]
    public void RandomSongsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.RandomSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RandomSongsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.RandomSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(7);
        parameters[0].Name.Should().Be("count");
        parameters[0].ParameterType.Should().Be(typeof(short?));
        parameters[1].Name.Should().Be("artistId");
        parameters[1].ParameterType.Should().Be(typeof(Guid?));
        parameters[2].Name.Should().Be("albumId");
        parameters[2].ParameterType.Should().Be(typeof(Guid?));
        parameters[3].Name.Should().Be("genre");
        parameters[3].ParameterType.Should().Be(typeof(string));
        parameters[4].Name.Should().Be("fromYear");
        parameters[4].ParameterType.Should().Be(typeof(int?));
        parameters[5].Name.Should().Be("toYear");
        parameters[5].ParameterType.Should().Be(typeof(int?));
        parameters[6].Name.Should().Be("cancellationToken");
        parameters[6].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void RandomSongsAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SongsController).GetMethod(nameof(SongsController.RandomSongsAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }
}
