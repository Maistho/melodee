using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for UserStatsController endpoints.
/// </summary>
public class UserStatsControllerTests
{
    #region Get Stats Tests

    [Fact]
    public void GetStatsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetStatsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetStatsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetStatsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("days");
        parameters[0].ParameterType.Should().Be(typeof(int?));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void GetStatsAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetStatsAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Top Songs Tests

    [Fact]
    public void GetTopSongsAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetTopSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("top-songs");
    }

    [Fact]
    public void GetTopSongsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetTopSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetTopSongsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetTopSongsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("days");
        parameters[0].ParameterType.Should().Be(typeof(int?));
        parameters[1].Name.Should().Be("limit");
        parameters[1].ParameterType.Should().Be(typeof(int?));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    #endregion

    #region Top Genres Tests

    [Fact]
    public void GetTopGenresAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetTopGenresAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("top-genres");
    }

    [Fact]
    public void GetTopGenresAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetTopGenresAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetTopGenresAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetTopGenresAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("days");
        parameters[0].ParameterType.Should().Be(typeof(int?));
        parameters[1].Name.Should().Be("limit");
        parameters[1].ParameterType.Should().Be(typeof(int?));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    #endregion

    #region Plays Per Day Tests

    [Fact]
    public void GetPlaysPerDayAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetPlaysPerDayAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("plays-per-day");
    }

    [Fact]
    public void GetPlaysPerDayAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetPlaysPerDayAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetPlaysPerDayAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetPlaysPerDayAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("days");
        parameters[0].ParameterType.Should().Be(typeof(int?));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    #endregion

    #region History Tests

    [Fact]
    public void GetHistoryAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetHistoryAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("history");
    }

    [Fact]
    public void GetHistoryAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetHistoryAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetHistoryAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(UserStatsController).GetMethod(nameof(UserStatsController.GetHistoryAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("limit");
        parameters[0].ParameterType.Should().Be(typeof(int?));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    #endregion

    #region Response Model Tests

    [Fact]
    public void UserStatsResponse_HasAllRequiredProperties()
    {
        // Arrange & Act
        var response = new UserStatsResponse(30, 100, 50, 25, 10, 45);

        // Assert
        response.PeriodDays.Should().Be(30);
        response.TotalPlays.Should().Be(100);
        response.FavoriteSongs.Should().Be(50);
        response.FavoriteAlbums.Should().Be(25);
        response.FavoriteArtists.Should().Be(10);
        response.RatedSongs.Should().Be(45);
    }

    [Fact]
    public void TopItemResponse_HasAllRequiredProperties()
    {
        // Arrange
        var songId = Guid.NewGuid();

        // Act
        var response = new TopItemResponse("Test Song", 50, songId);

        // Assert
        response.Name.Should().Be("Test Song");
        response.PlayCount.Should().Be(50);
        response.SongId.Should().Be(songId);
    }

    [Fact]
    public void TimeSeriesDataPoint_HasAllRequiredProperties()
    {
        // Arrange & Act
        var dataPoint = new TimeSeriesDataPoint("2024-01-15", 25);

        // Assert
        dataPoint.Date.Should().Be("2024-01-15");
        dataPoint.Plays.Should().Be(25);
    }

    #endregion
}
