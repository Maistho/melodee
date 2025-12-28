using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for SharesController CRUD endpoints.
/// </summary>
public class SharesControllerTests
{
    #region Controller Attribute Tests

    [Fact]
    public void SharesController_HasApiControllerAttribute()
    {
        // Arrange & Assert
        var attribute = typeof(SharesController).GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void SharesController_HasCorrectRoutePrefix()
    {
        // Arrange
        var routeAttribute = typeof(SharesController).GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/shares");
    }

    [Fact]
    public void SharesController_HasRequireCapabilityAttribute()
    {
        // Arrange & Assert
        var attribute = typeof(SharesController).GetCustomAttributes(typeof(RequireCapabilityAttribute), false).FirstOrDefault() as RequireCapabilityAttribute;
        attribute.Should().NotBeNull();
        attribute!.Capability.Should().Be(UserCapability.Share);
    }

    #endregion

    #region GetShareByApiKey Tests

    [Fact]
    public void GetShareByApiKey_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.GetShareByApiKey));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{apiKey:guid}");
    }

    [Fact]
    public void GetShareByApiKey_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.GetShareByApiKey));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetShareByApiKey_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.GetShareByApiKey));

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
    public void GetShareByApiKey_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.GetShareByApiKey));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region ListAsync Tests

    [Fact]
    public void ListAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ListAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("page");
        parameters[0].ParameterType.Should().Be(typeof(short));
        parameters[1].Name.Should().Be("pageSize");
        parameters[1].ParameterType.Should().Be(typeof(short));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void ListAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.ListAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region CreateShare Tests

    [Fact]
    public void CreateShare_HasHttpPostAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.CreateShare));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void CreateShare_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.CreateShare));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("request");
        parameters[0].ParameterType.Should().Be(typeof(CreateShareRequest));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void CreateShare_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.CreateShare));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region UpdateShare Tests

    [Fact]
    public void UpdateShare_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.UpdateShare));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{apiKey:guid}");
    }

    [Fact]
    public void UpdateShare_HasHttpPutAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.UpdateShare));

        // Assert
        method.Should().NotBeNull();
        var httpPutAttribute = method!.GetCustomAttributes(typeof(HttpPutAttribute), false).FirstOrDefault();
        httpPutAttribute.Should().NotBeNull();
    }

    [Fact]
    public void UpdateShare_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.UpdateShare));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].Name.Should().Be("apiKey");
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].Name.Should().Be("request");
        parameters[1].ParameterType.Should().Be(typeof(UpdateShareRequest));
        parameters[2].Name.Should().Be("cancellationToken");
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void UpdateShare_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.UpdateShare));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region DeleteShare Tests

    [Fact]
    public void DeleteShare_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.DeleteShare));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{apiKey:guid}");
    }

    [Fact]
    public void DeleteShare_HasHttpDeleteAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.DeleteShare));

        // Assert
        method.Should().NotBeNull();
        var httpDeleteAttribute = method!.GetCustomAttributes(typeof(HttpDeleteAttribute), false).FirstOrDefault();
        httpDeleteAttribute.Should().NotBeNull();
    }

    [Fact]
    public void DeleteShare_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.DeleteShare));

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
    public void DeleteShare_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.DeleteShare));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region GetPublicShare Tests

    [Fact]
    public void GetPublicShare_HasCorrectRouteAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.GetPublicShare));

        // Assert
        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("public/{shareUniqueId}");
    }

    [Fact]
    public void GetPublicShare_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.GetPublicShare));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetPublicShare_HasAllowAnonymousAttribute()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.GetPublicShare));

        // Assert
        method.Should().NotBeNull();
        var allowAnonymousAttribute = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).FirstOrDefault();
        allowAnonymousAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetPublicShare_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.GetPublicShare));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("shareUniqueId");
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void GetPublicShare_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(SharesController).GetMethod(nameof(SharesController.GetPublicShare));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Request Model Tests

    [Fact]
    public void CreateShareRequest_HasRequiredProperties()
    {
        // Arrange & Act
        var request = new CreateShareRequest(ShareType.Song, Guid.NewGuid(), "Test Description", true, "2025-12-31T23:59:59Z");

        // Assert
        request.ShareType.Should().Be(ShareType.Song);
        request.ResourceId.Should().NotBeEmpty();
        request.Description.Should().Be("Test Description");
        request.IsDownloadable.Should().BeTrue();
        request.ExpiresAt.Should().Be("2025-12-31T23:59:59Z");
    }

    [Fact]
    public void CreateShareRequest_DefaultValues()
    {
        // Arrange & Act
        var request = new CreateShareRequest(ShareType.Album, Guid.NewGuid());

        // Assert
        request.ShareType.Should().Be(ShareType.Album);
        request.ResourceId.Should().NotBeEmpty();
        request.Description.Should().BeNull();
        request.IsDownloadable.Should().BeFalse();
        request.ExpiresAt.Should().BeNull();
    }

    [Theory]
    [InlineData(ShareType.Song)]
    [InlineData(ShareType.Album)]
    [InlineData(ShareType.Playlist)]
    [InlineData(ShareType.Artist)]
    public void CreateShareRequest_SupportsAllShareTypes(ShareType shareType)
    {
        // Arrange & Act
        var request = new CreateShareRequest(shareType, Guid.NewGuid());

        // Assert
        request.ShareType.Should().Be(shareType);
    }

    [Fact]
    public void UpdateShareRequest_HasOptionalProperties()
    {
        // Arrange & Act
        var request = new UpdateShareRequest("New Description", true, "2025-12-31T23:59:59Z");

        // Assert
        request.Description.Should().Be("New Description");
        request.IsDownloadable.Should().BeTrue();
        request.ExpiresAt.Should().Be("2025-12-31T23:59:59Z");
    }

    [Fact]
    public void UpdateShareRequest_AllowsNullValues()
    {
        // Arrange & Act
        var request = new UpdateShareRequest(null, null, null);

        // Assert
        request.Description.Should().BeNull();
        request.IsDownloadable.Should().BeNull();
        request.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void UpdateShareRequest_AllowsEmptyExpiresAtToClearExpiration()
    {
        // Arrange & Act - empty string can be used to clear expiration
        var request = new UpdateShareRequest(null, null, "");

        // Assert
        request.ExpiresAt.Should().Be("");
    }

    #endregion

    #region Response Model Tests

    [Fact]
    public void ShareModel_HasAllRequiredProperties()
    {
        // Arrange
        var owner = new User(
            Guid.NewGuid(),
            "https://example.com/thumb.jpg",
            "https://example.com/image.jpg",
            "testuser",
            "test@example.com",
            false,
            false,
            [],
            0, 0, 0, 0, 0, 0, 0,
            "2025-01-01T00:00:00Z",
            "2025-01-01T00:00:00Z"
        );

        // Act
        var share = new Share(
            Guid.NewGuid(),
            "https://example.com/share/abc123",
            "Song",
            Guid.NewGuid(),
            "Test Song",
            "https://example.com/thumb.jpg",
            "https://example.com/image.jpg",
            "Test description",
            true,
            5,
            owner,
            "2025-01-01T00:00:00Z",
            "2025-12-31T23:59:59Z",
            "2025-06-15T12:00:00Z"
        );

        // Assert
        share.Id.Should().NotBeEmpty();
        share.ShareUrl.Should().Be("https://example.com/share/abc123");
        share.ShareType.Should().Be("Song");
        share.ResourceId.Should().NotBeEmpty();
        share.ResourceName.Should().Be("Test Song");
        share.ResourceThumbnailUrl.Should().Be("https://example.com/thumb.jpg");
        share.ResourceImageUrl.Should().Be("https://example.com/image.jpg");
        share.Description.Should().Be("Test description");
        share.IsDownloadable.Should().BeTrue();
        share.VisitCount.Should().Be(5);
        share.Owner.Should().Be(owner);
        share.CreatedAt.Should().Be("2025-01-01T00:00:00Z");
        share.ExpiresAt.Should().Be("2025-12-31T23:59:59Z");
        share.LastVisitedAt.Should().Be("2025-06-15T12:00:00Z");
    }

    [Fact]
    public void PublicShareResponse_HasAllRequiredProperties()
    {
        // Arrange
        var songInfo = new PublicSongInfo(Guid.NewGuid(), "Test Song", 1, 180000, "https://example.com/stream");

        // Act
        var response = new PublicShareResponse(
            "Song",
            "Test Song",
            "Test description",
            "https://example.com/thumb.jpg",
            "https://example.com/image.jpg",
            true,
            "2025-01-01T00:00:00Z",
            "2025-12-31T23:59:59Z",
            null,
            null,
            songInfo,
            null
        );

        // Assert
        response.ShareType.Should().Be("Song");
        response.ResourceName.Should().Be("Test Song");
        response.Description.Should().Be("Test description");
        response.ThumbnailUrl.Should().Be("https://example.com/thumb.jpg");
        response.ImageUrl.Should().Be("https://example.com/image.jpg");
        response.IsDownloadable.Should().BeTrue();
        response.CreatedAt.Should().Be("2025-01-01T00:00:00Z");
        response.ExpiresAt.Should().Be("2025-12-31T23:59:59Z");
        response.Artist.Should().BeNull();
        response.Album.Should().BeNull();
        response.Song.Should().Be(songInfo);
        response.Playlist.Should().BeNull();
    }

    [Fact]
    public void PublicArtistInfo_HasRequiredProperties()
    {
        // Arrange & Act
        var artistInfo = new PublicArtistInfo(Guid.NewGuid(), "Test Artist");

        // Assert
        artistInfo.Id.Should().NotBeEmpty();
        artistInfo.Name.Should().Be("Test Artist");
    }

    [Fact]
    public void PublicAlbumInfo_HasRequiredPropertiesWithSongs()
    {
        // Arrange
        var songs = new[]
        {
            new PublicSongInfo(Guid.NewGuid(), "Track 1", 1, 180000, "https://example.com/stream/1"),
            new PublicSongInfo(Guid.NewGuid(), "Track 2", 2, 200000, "https://example.com/stream/2")
        };

        // Act
        var albumInfo = new PublicAlbumInfo(Guid.NewGuid(), "Test Album", "Test Artist", 2024, songs);

        // Assert
        albumInfo.Id.Should().NotBeEmpty();
        albumInfo.Name.Should().Be("Test Album");
        albumInfo.ArtistName.Should().Be("Test Artist");
        albumInfo.ReleaseYear.Should().Be(2024);
        albumInfo.Songs.Should().HaveCount(2);
    }

    [Fact]
    public void PublicSongInfo_HasRequiredProperties()
    {
        // Arrange & Act
        var songInfo = new PublicSongInfo(Guid.NewGuid(), "Test Song", 3, 240000, "https://example.com/stream");

        // Assert
        songInfo.Id.Should().NotBeEmpty();
        songInfo.Title.Should().Be("Test Song");
        songInfo.TrackNumber.Should().Be(3);
        songInfo.DurationMs.Should().Be(240000);
        songInfo.StreamUrl.Should().Be("https://example.com/stream");
    }

    [Fact]
    public void PublicPlaylistInfo_HasRequiredPropertiesWithSongs()
    {
        // Arrange
        var songs = new[]
        {
            new PublicSongInfo(Guid.NewGuid(), "Playlist Track 1", 1, 180000, "https://example.com/stream/1"),
            new PublicSongInfo(Guid.NewGuid(), "Playlist Track 2", 2, 200000, "https://example.com/stream/2"),
            new PublicSongInfo(Guid.NewGuid(), "Playlist Track 3", 3, 220000, "https://example.com/stream/3")
        };

        // Act
        var playlistInfo = new PublicPlaylistInfo(Guid.NewGuid(), "Test Playlist", "A great playlist", songs);

        // Assert
        playlistInfo.Id.Should().NotBeEmpty();
        playlistInfo.Name.Should().Be("Test Playlist");
        playlistInfo.Description.Should().Be("A great playlist");
        playlistInfo.Songs.Should().HaveCount(3);
    }

    #endregion

    #region ShareType Enum Tests

    [Fact]
    public void ShareType_HasArtistValue()
    {
        // Assert - Verify Artist was added to ShareType enum
        ShareType.Artist.Should().BeDefined();
    }

    [Theory]
    [InlineData(ShareType.NotSet, 0)]
    [InlineData(ShareType.Song, 1)]
    [InlineData(ShareType.Album, 2)]
    [InlineData(ShareType.Playlist, 3)]
    [InlineData(ShareType.Artist, 4)]
    public void ShareType_HasCorrectValues(ShareType shareType, int expectedValue)
    {
        // Assert
        ((int)shareType).Should().Be(expectedValue);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void CreateShareRequest_WithNotSetShareType_IsValid()
    {
        // Arrange & Act - NotSet should be rejected by the controller
        var request = new CreateShareRequest(ShareType.NotSet, Guid.NewGuid());

        // Assert - The model can be created, but controller should reject it
        request.ShareType.Should().Be(ShareType.NotSet);
    }

    [Fact]
    public void CreateShareRequest_WithEmptyGuid_IsValid()
    {
        // Arrange & Act - Empty Guid should be rejected by the controller
        var request = new CreateShareRequest(ShareType.Song, Guid.Empty);

        // Assert - The model can be created, but controller should reject it
        request.ResourceId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ShareModel_WithNullableFieldsNull_IsValid()
    {
        // Arrange
        var owner = new User(
            Guid.NewGuid(),
            "https://example.com/thumb.jpg",
            "https://example.com/image.jpg",
            "testuser",
            "test@example.com",
            false,
            false,
            [],
            0, 0, 0, 0, 0, 0, 0,
            "2025-01-01T00:00:00Z",
            "2025-01-01T00:00:00Z"
        );

        // Act - Create share with nullable fields as null
        var share = new Share(
            Guid.NewGuid(),
            "https://example.com/share/abc123",
            "Song",
            Guid.NewGuid(),
            "Test Song",
            "https://example.com/thumb.jpg",
            "https://example.com/image.jpg",
            null, // Description can be null
            false,
            0,
            owner,
            "2025-01-01T00:00:00Z",
            null, // ExpiresAt can be null (no expiration)
            null  // LastVisitedAt can be null (never visited)
        );

        // Assert
        share.Description.Should().BeNull();
        share.ExpiresAt.Should().BeNull();
        share.LastVisitedAt.Should().BeNull();
    }

    #endregion
}
