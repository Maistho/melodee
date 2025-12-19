using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for SmartPlaylistsController.
/// </summary>
public class SmartPlaylistsControllerTests
{
    #region Route Attribute Tests

    [Fact]
    public void Controller_HasCorrectRouteAttribute()
    {
        // Arrange
        var routeAttribute = typeof(SmartPlaylistsController)
            .GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/playlists/smart");
    }

    #endregion

    #region CreateSmartPlaylistAsync Tests

    [Fact]
    public void CreateSmartPlaylistAsync_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(SmartPlaylistsController).GetMethod(nameof(SmartPlaylistsController.CreateSmartPlaylistAsync));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void CreateSmartPlaylistAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SmartPlaylistsController).GetMethod(nameof(SmartPlaylistsController.CreateSmartPlaylistAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("request");
        parameters[0].ParameterType.Should().Be(typeof(CreateSmartPlaylistRequest));
    }

    #endregion

    #region Model Tests

    [Fact]
    public void SmartPlaylistRule_HasAllRequiredProperties()
    {
        // Arrange & Act
        var rule = new SmartPlaylistRule(
            "genre",
            "equals",
            "Rock");

        // Assert
        rule.Field.Should().Be("genre");
        rule.Operator.Should().Be("equals");
        rule.Value.Should().Be("Rock");
    }

    [Fact]
    public void CreateSmartPlaylistRequest_HasAllRequiredProperties()
    {
        // Arrange
        var rules = new[]
        {
            new SmartPlaylistRule("genre", "equals", "Rock"),
            new SmartPlaylistRule("year", "greaterThan", 2020)
        };

        // Act
        var request = new CreateSmartPlaylistRequest(
            "Rock Hits 2020+",
            "Rock songs from 2020 onwards",
            rules,
            100,
            true);

        // Assert
        request.Name.Should().Be("Rock Hits 2020+");
        request.Description.Should().Be("Rock songs from 2020 onwards");
        request.Rules.Should().HaveCount(2);
        request.Limit.Should().Be(100);
        request.AutoUpdate.Should().BeTrue();
    }

    [Fact]
    public void SmartPlaylistResponse_HasAllRequiredProperties()
    {
        // Arrange
        var rules = new[]
        {
            new SmartPlaylistRule("rating", "greaterThan", 4)
        };

        // Act
        var response = new SmartPlaylistResponse(
            Guid.NewGuid(),
            "High Rated",
            "Songs with 4+ stars",
            rules,
            150,
            true);

        // Assert
        response.Id.Should().NotBeEmpty();
        response.Name.Should().Be("High Rated");
        response.Description.Should().Be("Songs with 4+ stars");
        response.Rules.Should().HaveCount(1);
        response.TrackCount.Should().Be(150);
        response.AutoUpdate.Should().BeTrue();
    }

    [Theory]
    [InlineData("genre")]
    [InlineData("year")]
    [InlineData("rating")]
    [InlineData("playCount")]
    [InlineData("bpm")]
    [InlineData("duration")]
    [InlineData("artist")]
    [InlineData("album")]
    public void SmartPlaylistRule_SupportsAllFields(string field)
    {
        // Arrange & Act
        var rule = new SmartPlaylistRule(field, "equals", "value");

        // Assert
        rule.Field.Should().Be(field);
    }

    [Theory]
    [InlineData("equals")]
    [InlineData("contains")]
    [InlineData("greaterThan")]
    [InlineData("lessThan")]
    [InlineData("between")]
    public void SmartPlaylistRule_SupportsAllOperators(string op)
    {
        // Arrange & Act
        var rule = new SmartPlaylistRule("genre", op, "value");

        // Assert
        rule.Operator.Should().Be(op);
    }

    [Fact]
    public void SmartPlaylistRule_BetweenOperator_AcceptsRangeValue()
    {
        // Arrange & Act
        var rangeValue = new { min = 2000, max = 2023 };
        var rule = new SmartPlaylistRule("year", "between", rangeValue);

        // Assert
        rule.Value.Should().NotBeNull();
    }

    [Fact]
    public void CreateSmartPlaylistRequest_AllowsNullDescription()
    {
        // Arrange & Act
        var rules = new[] { new SmartPlaylistRule("genre", "equals", "Rock") };
        var request = new CreateSmartPlaylistRequest(
            "Rock Playlist",
            null,
            rules,
            50,
            false);

        // Assert
        request.Description.Should().BeNull();
    }

    [Fact]
    public void CreateSmartPlaylistRequest_AllowsNullLimit()
    {
        // Arrange & Act
        var rules = new[] { new SmartPlaylistRule("genre", "equals", "Rock") };
        var request = new CreateSmartPlaylistRequest(
            "Rock Playlist",
            null,
            rules,
            null,
            false);

        // Assert
        request.Limit.Should().BeNull();
    }

    [Fact]
    public void SmartPlaylistResponse_AllowsNullDescription()
    {
        // Arrange & Act
        var rules = new[] { new SmartPlaylistRule("genre", "equals", "Rock") };
        var response = new SmartPlaylistResponse(
            Guid.NewGuid(),
            "Test Playlist",
            null,
            rules,
            10,
            true);

        // Assert
        response.Description.Should().BeNull();
    }

    #endregion
}
