using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for PlaylistsController CRUD endpoints.
/// </summary>
public class PlaylistsControllerTests
{
    #region Create Playlist Tests

    [Fact]
    public void CreatePlaylist_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.CreatePlaylist));

        // Assert
        method.Should().NotBeNull();
        // Create uses the controller's base route without additional route attribute
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault() as HttpPostAttribute;
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void CreatePlaylist_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.CreatePlaylist));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void CreatePlaylist_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.CreatePlaylist));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("request");
        parameters[0].ParameterType.Should().Be(typeof(CreatePlaylistRequest));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void CreatePlaylist_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.CreatePlaylist));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Update Playlist Tests

    [Fact]
    public void UpdatePlaylist_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.UpdatePlaylist));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{apiKey:guid}");
    }

    [Fact]
    public void UpdatePlaylist_HasHttpPutAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.UpdatePlaylist));

        // Assert
        method.Should().NotBeNull();
        var httpPutAttribute = method!.GetCustomAttributes(typeof(HttpPutAttribute), false).FirstOrDefault();
        httpPutAttribute.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePlaylist_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.UpdatePlaylist));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("request");
        parameters[1].ParameterType.Should().Be(typeof(UpdatePlaylistRequest));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void UpdatePlaylist_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.UpdatePlaylist));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Delete Playlist Tests

    [Fact]
    public void DeletePlaylist_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.DeletePlaylist));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{apiKey:guid}");
    }

    [Fact]
    public void DeletePlaylist_HasHttpDeleteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.DeletePlaylist));

        // Assert
        method.Should().NotBeNull();
        var httpDeleteAttribute = method!.GetCustomAttributes(typeof(HttpDeleteAttribute), false).FirstOrDefault();
        httpDeleteAttribute.Should().NotBeNull();
    }

    [Fact]
    public void DeletePlaylist_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.DeletePlaylist));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void DeletePlaylist_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.DeletePlaylist));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Add Songs To Playlist Tests

    [Fact]
    public void AddSongsToPlaylist_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.AddSongsToPlaylist));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{apiKey:guid}/songs");
    }

    [Fact]
    public void AddSongsToPlaylist_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.AddSongsToPlaylist));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void AddSongsToPlaylist_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.AddSongsToPlaylist));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("songIds");
        parameters[1].ParameterType.Should().Be(typeof(Guid[]));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void AddSongsToPlaylist_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.AddSongsToPlaylist));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Remove Songs From Playlist Tests

    [Fact]
    public void RemoveSongsFromPlaylist_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.RemoveSongsFromPlaylist));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{apiKey:guid}/songs");
    }

    [Fact]
    public void RemoveSongsFromPlaylist_HasHttpDeleteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.RemoveSongsFromPlaylist));

        // Assert
        method.Should().NotBeNull();
        var httpDeleteAttribute = method!.GetCustomAttributes(typeof(HttpDeleteAttribute), false).FirstOrDefault();
        httpDeleteAttribute.Should().NotBeNull();
    }

    [Fact]
    public void RemoveSongsFromPlaylist_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.RemoveSongsFromPlaylist));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("songIds");
        parameters[1].ParameterType.Should().Be(typeof(Guid[]));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void RemoveSongsFromPlaylist_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.RemoveSongsFromPlaylist));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Reorder Playlist Songs Tests

    [Fact]
    public void ReorderPlaylistSongs_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.ReorderPlaylistSongs));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{apiKey:guid}/songs/reorder");
    }

    [Fact]
    public void ReorderPlaylistSongs_HasHttpPutAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.ReorderPlaylistSongs));

        // Assert
        method.Should().NotBeNull();
        var httpPutAttribute = method!.GetCustomAttributes(typeof(HttpPutAttribute), false).FirstOrDefault();
        httpPutAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ReorderPlaylistSongs_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.ReorderPlaylistSongs));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("request");
        parameters[1].ParameterType.Should().Be(typeof(ReorderPlaylistSongsRequest));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void ReorderPlaylistSongs_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.ReorderPlaylistSongs));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Upload Playlist Image Tests

    [Fact]
    public void UploadPlaylistImage_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.UploadPlaylistImage));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{apiKey:guid}/image");
    }

    [Fact]
    public void UploadPlaylistImage_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.UploadPlaylistImage));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void UploadPlaylistImage_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.UploadPlaylistImage));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("file");
        parameters[1].ParameterType.Should().Be(typeof(IFormFile));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void UploadPlaylistImage_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.UploadPlaylistImage));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Delete Playlist Image Tests

    [Fact]
    public void DeletePlaylistImage_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.DeletePlaylistImage));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{apiKey:guid}/image");
    }

    [Fact]
    public void DeletePlaylistImage_HasHttpDeleteAttribute()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.DeletePlaylistImage));

        // Assert
        method.Should().NotBeNull();
        var httpDeleteAttribute = method!.GetCustomAttributes(typeof(HttpDeleteAttribute), false).FirstOrDefault();
        httpDeleteAttribute.Should().NotBeNull();
    }

    [Fact]
    public void DeletePlaylistImage_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.DeletePlaylistImage));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void DeletePlaylistImage_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(PlaylistsController).GetMethod(nameof(PlaylistsController.DeletePlaylistImage));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Controller Attribute Tests

    [Fact]
    public void PlaylistsController_HasApiControllerAttribute()
    {
        // Arrange & Assert
        var attribute = typeof(PlaylistsController).GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void PlaylistsController_HasCorrectRoutePrefix()
    {
        // Arrange
        var routeAttribute = typeof(PlaylistsController).GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/[controller]");
    }

    #endregion

    #region Request Model Tests

    [Fact]
    public void CreatePlaylistRequest_HasRequiredProperties()
    {
        // Arrange & Act
        var request = new CreatePlaylistRequest("Test Playlist", "Test Comment", true, [Guid.NewGuid()]);

        // Assert
        request.Name.Should().Be("Test Playlist");
        request.Comment.Should().Be("Test Comment");
        request.IsPublic.Should().BeTrue();
        request.SongIds.Should().HaveCount(1);
    }

    [Fact]
    public void CreatePlaylistRequest_DefaultValues()
    {
        // Arrange & Act
        var request = new CreatePlaylistRequest("Test Playlist", null);

        // Assert
        request.Name.Should().Be("Test Playlist");
        request.Comment.Should().BeNull();
        request.IsPublic.Should().BeFalse();
        request.SongIds.Should().BeNull();
    }

    [Fact]
    public void UpdatePlaylistRequest_HasOptionalProperties()
    {
        // Arrange & Act
        var request = new UpdatePlaylistRequest("New Name", "New Comment", true);

        // Assert
        request.Name.Should().Be("New Name");
        request.Comment.Should().Be("New Comment");
        request.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void UpdatePlaylistRequest_AllowsNullValues()
    {
        // Arrange & Act
        var request = new UpdatePlaylistRequest(null, null, null);

        // Assert
        request.Name.Should().BeNull();
        request.Comment.Should().BeNull();
        request.IsPublic.Should().BeNull();
    }

    [Fact]
    public void ReorderPlaylistSongsRequest_HasSongIds()
    {
        // Arrange
        var songIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var request = new ReorderPlaylistSongsRequest(songIds);

        // Assert
        request.SongIds.Should().HaveCount(3);
        request.SongIds.Should().BeEquivalentTo(songIds);
    }

    #endregion
}
