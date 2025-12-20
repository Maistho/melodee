using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for SearchController suggest endpoint.
/// </summary>
public class SearchControllerSuggestTests
{
    #region Suggest Endpoint Tests

    [Fact]
    public void SuggestAsync_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SearchController).GetMethod(nameof(SearchController.SuggestAsync));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("suggest");
    }

    [Fact]
    public void SuggestAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(SearchController).GetMethod(nameof(SearchController.SuggestAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void SuggestAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SearchController).GetMethod(nameof(SearchController.SuggestAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("q");
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[1].Name.Should().Be("limit");
        parameters[1].ParameterType.Should().Be(typeof(short?));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void SuggestAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SearchController).GetMethod(nameof(SearchController.SuggestAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Response Model Tests

    [Fact]
    public void SearchSuggestion_HasAllRequiredProperties()
    {
        // Arrange & Act
        var suggestion = new SearchSuggestion(
            Guid.NewGuid(),
            "Test Name",
            "artist",
            "https://example.com/thumb.jpg"
        );

        // Assert
        suggestion.Id.Should().NotBeEmpty();
        suggestion.Name.Should().Be("Test Name");
        suggestion.Type.Should().Be("artist");
        suggestion.ThumbnailUrl.Should().Be("https://example.com/thumb.jpg");
    }

    [Theory]
    [InlineData("artist")]
    [InlineData("album")]
    [InlineData("song")]
    [InlineData("playlist")]
    public void SearchSuggestion_SupportsAllTypes(string type)
    {
        // Arrange & Act
        var suggestion = new SearchSuggestion(Guid.NewGuid(), "Test", type, "https://example.com/thumb.jpg");

        // Assert
        suggestion.Type.Should().Be(type);
    }

    [Fact]
    public void SearchSuggestResponse_HasAllCategories()
    {
        // Arrange
        var artists = new[] { new SearchSuggestion(Guid.NewGuid(), "Artist 1", "artist", "url1") };
        var albums = new[] { new SearchSuggestion(Guid.NewGuid(), "Album 1", "album", "url2") };
        var songs = new[] { new SearchSuggestion(Guid.NewGuid(), "Song 1", "song", "url3") };
        var playlists = new[] { new SearchSuggestion(Guid.NewGuid(), "Playlist 1", "playlist", "url4") };

        // Act
        var response = new SearchSuggestResponse(artists, albums, songs, playlists);

        // Assert
        response.Artists.Should().HaveCount(1);
        response.Albums.Should().HaveCount(1);
        response.Songs.Should().HaveCount(1);
        response.Playlists.Should().HaveCount(1);
    }

    [Fact]
    public void SearchSuggestResponse_CanHaveEmptyArrays()
    {
        // Arrange & Act
        var response = new SearchSuggestResponse([], [], [], []);

        // Assert
        response.Artists.Should().BeEmpty();
        response.Albums.Should().BeEmpty();
        response.Songs.Should().BeEmpty();
        response.Playlists.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void SearchSuggestion_WithEmptyName_CanBeCreated()
    {
        // Arrange & Act
        var suggestion = new SearchSuggestion(Guid.NewGuid(), string.Empty, "artist", "url");

        // Assert
        suggestion.Name.Should().BeEmpty();
    }

    [Fact]
    public void SearchSuggestion_WithEmptyGuid_CanBeCreated()
    {
        // Arrange & Act
        var suggestion = new SearchSuggestion(Guid.Empty, "Test", "artist", "url");

        // Assert
        suggestion.Id.Should().Be(Guid.Empty);
    }

    #endregion
}
