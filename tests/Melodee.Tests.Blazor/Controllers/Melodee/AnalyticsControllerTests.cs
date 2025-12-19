using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for AnalyticsController.
/// </summary>
public class AnalyticsControllerTests
{
    #region Route Attribute Tests

    [Fact]
    public void Controller_HasCorrectRouteAttribute()
    {
        // Arrange
        var routeAttribute = typeof(AnalyticsController)
            .GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/analytics");
    }

    #endregion

    #region GetListeningStatisticsAsync Tests

    [Fact]
    public void GetListeningStatisticsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(AnalyticsController).GetMethod(nameof(AnalyticsController.GetListeningStatisticsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetListeningStatisticsAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(AnalyticsController).GetMethod(nameof(AnalyticsController.GetListeningStatisticsAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("listening");
    }

    [Fact]
    public void GetListeningStatisticsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(AnalyticsController).GetMethod(nameof(AnalyticsController.GetListeningStatisticsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("period");
        parameters[0].DefaultValue.Should().Be("week");
    }

    #endregion

    #region GetTopContentAsync Tests

    [Fact]
    public void GetTopContentAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(AnalyticsController).GetMethod(nameof(AnalyticsController.GetTopContentAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetTopContentAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(AnalyticsController).GetMethod(nameof(AnalyticsController.GetTopContentAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("top/{period}");
    }

    #endregion

    #region Model Tests

    [Fact]
    public void AnalyticsItem_HasAllRequiredProperties()
    {
        // Arrange & Act
        var item = new AnalyticsItem(
            Guid.NewGuid(),
            "Artist Name",
            150,
            45000.0);

        // Assert
        item.Id.Should().NotBeEmpty();
        item.Name.Should().Be("Artist Name");
        item.PlayCount.Should().Be(150);
        item.PlayTime.Should().Be(45000.0);
    }

    [Fact]
    public void AnalyticsGenre_HasAllRequiredProperties()
    {
        // Arrange & Act
        var genre = new AnalyticsGenre(
            "Rock",
            500,
            180000.0);

        // Assert
        genre.Name.Should().Be("Rock");
        genre.PlayCount.Should().Be(500);
        genre.PlayTime.Should().Be(180000.0);
    }

    [Fact]
    public void ListeningByHour_HasCorrectHourRange()
    {
        // Arrange & Act
        var hour0 = new ListeningByHour(0, 100.0);
        var hour12 = new ListeningByHour(12, 500.0);
        var hour23 = new ListeningByHour(23, 200.0);

        // Assert
        hour0.Hour.Should().Be(0);
        hour12.Hour.Should().Be(12);
        hour23.Hour.Should().Be(23);
    }

    [Theory]
    [InlineData("monday")]
    [InlineData("tuesday")]
    [InlineData("wednesday")]
    [InlineData("thursday")]
    [InlineData("friday")]
    [InlineData("saturday")]
    [InlineData("sunday")]
    public void ListeningByDay_SupportsAllDays(string day)
    {
        // Arrange & Act
        var listeningDay = new ListeningByDay(day, 3600.0);

        // Assert
        listeningDay.Day.Should().Be(day);
        listeningDay.PlayTime.Should().Be(3600.0);
    }

    [Fact]
    public void ListeningStatistics_HasAllRequiredProperties()
    {
        // Arrange
        var topArtists = new[] { new AnalyticsItem(Guid.NewGuid(), "Artist", 100, 1000.0) };
        var topAlbums = new[] { new AnalyticsItem(Guid.NewGuid(), "Album", 50, 500.0) };
        var topGenres = new[] { new AnalyticsGenre("Rock", 200, 2000.0) };
        var byHour = new[] { new ListeningByHour(12, 300.0) };
        var byDay = new[] { new ListeningByDay("monday", 3600.0) };

        // Act
        var stats = new ListeningStatistics(
            "week",
            36000.0,
            500,
            topArtists,
            topAlbums,
            topGenres,
            byHour,
            byDay);

        // Assert
        stats.Period.Should().Be("week");
        stats.TotalPlayTime.Should().Be(36000.0);
        stats.TotalTracksPlayed.Should().Be(500);
        stats.TopArtists.Should().HaveCount(1);
        stats.TopAlbums.Should().HaveCount(1);
        stats.TopGenres.Should().HaveCount(1);
        stats.ListeningByTimeOfDay.Should().HaveCount(1);
        stats.ListeningByDayOfWeek.Should().HaveCount(1);
    }

    [Fact]
    public void TopContentItem_HasAllRequiredProperties()
    {
        // Arrange & Act
        var item = new TopContentItem(
            Guid.NewGuid(),
            "Top Song",
            200,
            12000.0,
            1);

        // Assert
        item.Id.Should().NotBeEmpty();
        item.Name.Should().Be("Top Song");
        item.PlayCount.Should().Be(200);
        item.PlayTime.Should().Be(12000.0);
        item.Rank.Should().Be(1);
    }

    [Fact]
    public void TopContentResponse_HasAllRequiredProperties()
    {
        // Arrange
        var items = new[]
        {
            new TopContentItem(Guid.NewGuid(), "Song 1", 100, 5000.0, 1),
            new TopContentItem(Guid.NewGuid(), "Song 2", 90, 4500.0, 2)
        };

        // Act
        var response = new TopContentResponse("week", "song", items);

        // Assert
        response.Period.Should().Be("week");
        response.Type.Should().Be("song");
        response.Items.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("day")]
    [InlineData("week")]
    [InlineData("month")]
    [InlineData("year")]
    [InlineData("all_time")]
    public void ListeningStatistics_SupportsAllPeriods(string period)
    {
        // Arrange & Act
        var stats = new ListeningStatistics(period, 0, 0, [], [], [], [], []);

        // Assert
        stats.Period.Should().Be(period);
    }

    [Theory]
    [InlineData("song")]
    [InlineData("album")]
    [InlineData("artist")]
    public void TopContentResponse_SupportsAllTypes(string type)
    {
        // Arrange & Act
        var response = new TopContentResponse("week", type, []);

        // Assert
        response.Type.Should().Be(type);
    }

    #endregion
}
