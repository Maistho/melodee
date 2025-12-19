using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for SearchController.
/// </summary>
public class SearchControllerTests
{
    #region Route Attribute Tests

    [Fact]
    public void Controller_HasCorrectRouteAttribute()
    {
        // Arrange
        var routeAttribute = typeof(SearchController)
            .GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/search");
    }

    #endregion

    #region AdvancedSearchAsync Tests

    [Fact]
    public void AdvancedSearchAsync_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(SearchController).GetMethod(nameof(SearchController.AdvancedSearchAsync));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void AdvancedSearchAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SearchController).GetMethod(nameof(SearchController.AdvancedSearchAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("advanced");
    }

    [Fact]
    public void AdvancedSearchAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SearchController).GetMethod(nameof(SearchController.AdvancedSearchAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("request");
        parameters[0].ParameterType.Should().Be(typeof(AdvancedSearchRequest));
    }

    #endregion

    #region FindSimilarAsync Tests

    [Fact]
    public void FindSimilarAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(SearchController).GetMethod(nameof(SearchController.FindSimilarAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void FindSimilarAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SearchController).GetMethod(nameof(SearchController.FindSimilarAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("similar/{id:guid}/{type}");
    }

    #endregion

    #region Model Tests

    [Fact]
    public void RangeFilter_HasMinAndMaxProperties()
    {
        // Arrange & Act
        var filter = new RangeFilter<int>(2000, 2023);

        // Assert
        filter.Min.Should().Be(2000);
        filter.Max.Should().Be(2023);
    }

    [Fact]
    public void RangeFilter_AllowsNullValues()
    {
        // Arrange & Act
        var filter = new RangeFilter<double>(null, 180.0);

        // Assert
        filter.Min.Should().BeNull();
        filter.Max.Should().Be(180.0);
    }

    [Fact]
    public void AdvancedSearchFilters_HasAllFilterTypes()
    {
        // Arrange & Act
        var filters = new AdvancedSearchFilters(
            new RangeFilter<int>(2000, 2023),
            new RangeFilter<double>(80, 120),
            new RangeFilter<double>(180, 300),
            new[] { "Rock", "Metal" },
            new[] { "Energetic" },
            "C Major",
            "Artist Name",
            "Album Name");

        // Assert
        filters.Year.Should().NotBeNull();
        filters.Year!.Min.Should().Be(2000);
        filters.Year!.Max.Should().Be(2023);
        filters.Bpm!.Min.Should().Be(80);
        filters.Bpm!.Max.Should().Be(120);
        filters.Duration!.Min.Should().Be(180);
        filters.Genre.Should().HaveCount(2);
        filters.Mood.Should().HaveCount(1);
        filters.Key.Should().Be("C Major");
        filters.Artist.Should().Be("Artist Name");
        filters.Album.Should().Be("Album Name");
    }

    [Fact]
    public void AdvancedSearchRequest_HasAllProperties()
    {
        // Arrange & Act
        var request = new AdvancedSearchRequest(
            "search query",
            new AdvancedSearchFilters(null, null, null, null, null, null, null, null),
            new[] { "song", "album" },
            "relevance",
            "desc",
            1,
            50);

        // Assert
        request.Query.Should().Be("search query");
        request.Filters.Should().NotBeNull();
        request.Types.Should().HaveCount(2);
        request.SortBy.Should().Be("relevance");
        request.SortOrder.Should().Be("desc");
        request.Page.Should().Be(1);
        request.Limit.Should().Be(50);
    }

    [Fact]
    public void SimilarItem_HasAllRequiredProperties()
    {
        // Arrange & Act
        var item = new SimilarItem(
            Guid.NewGuid(),
            "Similar Song",
            "song",
            0.85,
            "https://example.com/image.jpg");

        // Assert
        item.Id.Should().NotBeEmpty();
        item.Name.Should().Be("Similar Song");
        item.Type.Should().Be("song");
        item.SimilarityScore.Should().Be(0.85);
        item.ImageUrl.Should().Be("https://example.com/image.jpg");
    }

    [Theory]
    [InlineData("song")]
    [InlineData("album")]
    [InlineData("artist")]
    [InlineData("playlist")]
    public void AdvancedSearchRequest_AcceptsValidTypes(string type)
    {
        // Arrange & Act
        var request = new AdvancedSearchRequest(
            "test",
            null,
            new[] { type },
            null,
            null,
            null,
            null);

        // Assert
        request.Types.Should().Contain(type);
    }

    [Theory]
    [InlineData("relevance")]
    [InlineData("date")]
    [InlineData("popularity")]
    [InlineData("rating")]
    public void AdvancedSearchRequest_AcceptsValidSortByValues(string sortBy)
    {
        // Arrange & Act
        var request = new AdvancedSearchRequest(
            "test",
            null,
            null,
            sortBy,
            null,
            null,
            null);

        // Assert
        request.SortBy.Should().Be(sortBy);
    }

    #endregion
}
