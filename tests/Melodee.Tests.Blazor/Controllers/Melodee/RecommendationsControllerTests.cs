using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for RecommendationsController.
/// </summary>
public class RecommendationsControllerTests
{
    #region Route Attribute Tests

    [Fact]
    public void Controller_HasCorrectRouteAttribute()
    {
        // Arrange
        var routeAttribute = typeof(RecommendationsController)
            .GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/recommendations");
    }

    #endregion

    #region GetRecommendationsAsync Tests

    [Fact]
    public void GetRecommendationsAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(RecommendationsController).GetMethod(nameof(RecommendationsController.GetRecommendationsAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetRecommendationsAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(RecommendationsController).GetMethod(nameof(RecommendationsController.GetRecommendationsAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(4);
        parameters[0].Name.Should().Be("limit");
        parameters[1].Name.Should().Be("type");
        parameters[2].Name.Should().Be("category");
        parameters[3].Name.Should().Be("cancellationToken");
    }

    #endregion

    #region Model Tests

    [Fact]
    public void RecommendationItem_HasAllRequiredProperties()
    {
        // Arrange & Act
        var item = new RecommendationItem(
            Guid.NewGuid(),
            "Recommended Song",
            "song",
            "Artist Name",
            "Based on your listening history",
            "https://example.com/image.jpg");

        // Assert
        item.Id.Should().NotBeEmpty();
        item.Name.Should().Be("Recommended Song");
        item.Type.Should().Be("song");
        item.Artist.Should().Be("Artist Name");
        item.Reason.Should().Be("Based on your listening history");
        item.ImageUrl.Should().Be("https://example.com/image.jpg");
    }

    [Fact]
    public void RecommendationsResponse_HasAllRequiredProperties()
    {
        // Arrange
        var items = new[]
        {
            new RecommendationItem(Guid.NewGuid(), "Song 1", "song", "Artist 1", "Reason 1", "url1"),
            new RecommendationItem(Guid.NewGuid(), "Song 2", "song", "Artist 2", "Reason 2", "url2")
        };

        // Act
        var response = new RecommendationsResponse(items, "discover");

        // Assert
        response.Recommendations.Should().HaveCount(2);
        response.Category.Should().Be("discover");
    }

    [Theory]
    [InlineData("song")]
    [InlineData("album")]
    [InlineData("artist")]
    public void RecommendationItem_SupportsAllTypes(string type)
    {
        // Arrange & Act
        var item = new RecommendationItem(
            Guid.NewGuid(),
            "Test",
            type,
            null,
            null,
            null);

        // Assert
        item.Type.Should().Be(type);
    }

    [Theory]
    [InlineData("discover")]
    [InlineData("similar")]
    [InlineData("missed")]
    [InlineData("based_on_recent")]
    public void RecommendationsResponse_SupportsAllCategories(string category)
    {
        // Arrange & Act
        var response = new RecommendationsResponse([], category);

        // Assert
        response.Category.Should().Be(category);
    }

    [Fact]
    public void RecommendationItem_AllowsNullOptionalFields()
    {
        // Arrange & Act
        var item = new RecommendationItem(
            Guid.NewGuid(),
            "Test Song",
            "song",
            null,
            null,
            null);

        // Assert
        item.Artist.Should().BeNull();
        item.Reason.Should().BeNull();
        item.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void RecommendationsResponse_CanHaveEmptyRecommendations()
    {
        // Arrange & Act
        var response = new RecommendationsResponse([], "discover");

        // Assert
        response.Recommendations.Should().BeEmpty();
    }

    #endregion
}
