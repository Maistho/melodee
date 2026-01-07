using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

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
        var method = typeof(SmartPlaylistsController).GetMethod(nameof(SmartPlaylistsController.CreateAsync));

        // Assert
        method.Should().NotBeNull();
        var httpPostAttribute = method!.GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault();
        httpPostAttribute.Should().NotBeNull();
    }

    [Fact]
    public void CreateSmartPlaylistAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(SmartPlaylistsController).GetMethod(nameof(SmartPlaylistsController.CreateAsync));

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
    public void SmartPlaylistDto_HasAllRequiredProperties()
    {
        // Arrange & Act
        var dto = new SmartPlaylistDto
        {
            ApiKey = Guid.NewGuid(),
            Name = "Test Playlist",
            MqlQuery = "genre = 'Rock'",
            EntityType = "songs",
            LastResultCount = 100,
            IsPublic = true,
            NormalizedQuery = "genre = 'rock'",
            CreatedAt = Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        // Assert
        dto.ApiKey.Should().NotBe(Guid.Empty);
        dto.Name.Should().Be("Test Playlist");
        dto.MqlQuery.Should().Be("genre = 'Rock'");
        dto.EntityType.Should().Be("songs");
        dto.LastResultCount.Should().Be(100);
        dto.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void CreateSmartPlaylistRequest_HasAllRequiredProperties()
    {
        // Arrange & Act
        var request = new CreateSmartPlaylistRequest
        {
            Name = "Rock Hits 2020+",
            MqlQuery = "genre = 'Rock' && year >= 2020",
            EntityType = "songs",
            IsPublic = true
        };

        // Assert
        request.Name.Should().Be("Rock Hits 2020+");
        request.MqlQuery.Should().Be("genre = 'Rock' && year >= 2020");
        request.EntityType.Should().Be("songs");
        request.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void UpdateSmartPlaylistRequest_AllowsPartialUpdates()
    {
        // Arrange & Act
        var request = new UpdateSmartPlaylistRequest
        {
            Name = "Updated Name",
            MqlQuery = null,
            EntityType = null,
            IsPublic = false
        };

        // Assert
        request.Name.Should().Be("Updated Name");
        request.MqlQuery.Should().BeNull();
        request.EntityType.Should().BeNull();
        request.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void SmartPlaylistModel_HasAllRequiredProperties()
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
            Array.Empty<string>(),
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            "2024-01-01",
            "2024-01-01"
        );

        // Act
        var model = new SmartPlaylistModel(
            Guid.NewGuid(),
            "Test Playlist",
            "genre = 'Rock'",
            "songs",
            100,
            "2024-01-01",
            true,
            "genre = 'rock'",
            "2024-01-01",
            owner
        );

        // Assert
        model.ApiKey.Should().NotBe(Guid.Empty);
        model.Name.Should().Be("Test Playlist");
        model.MqlQuery.Should().Be("genre = 'Rock'");
        model.EntityType.Should().Be("songs");
        model.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void SmartPlaylistPagedResponse_HasAllRequiredProperties()
    {
        // Arrange
        var meta = new PaginationMetadata(100, 10, 1, 10);
        var data = Array.Empty<SmartPlaylistModel>();

        // Act
        var response = new SmartPlaylistPagedResponse(meta, data);

        // Assert
        response.Meta.Should().Be(meta);
        response.Data.Should().BeEmpty();
    }

    [Theory]
    [InlineData("songs")]
    [InlineData("albums")]
    [InlineData("artists")]
    public void SmartPlaylistDto_SupportsAllEntityTypes(string entityType)
    {
        // Arrange & Act
        var dto = new SmartPlaylistDto
        {
            ApiKey = Guid.NewGuid(),
            Name = "Test",
            MqlQuery = "test",
            EntityType = entityType,
            CreatedAt = Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        // Assert
        dto.EntityType.Should().Be(entityType);
    }

    [Fact]
    public void SmartPlaylistEvaluateResponse_HasAllRequiredProperties()
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
            Array.Empty<string>(),
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            "2024-01-01",
            "2024-01-01"
        );
        var playlist = new SmartPlaylistModel(
            Guid.NewGuid(),
            "Test",
            "test",
            "songs",
            0,
            "",
            false,
            null,
            "",
            owner
        );
        var meta = new PaginationMetadata(0, 10, 1, 0);
        var data = Array.Empty<dynamic>();

        // Act
        var response = new SmartPlaylistEvaluateResponse(playlist, meta, data);

        // Assert
        response.Playlist.Should().Be(playlist);
        response.Meta.Should().Be(meta);
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public void CreateSmartPlaylistRequest_DefaultIsPublicIsFalse()
    {
        // Arrange & Act
        var request = new CreateSmartPlaylistRequest
        {
            Name = "Test",
            MqlQuery = "test",
            EntityType = "songs"
        };

        // Assert
        request.IsPublic.Should().BeFalse();
    }

    #endregion
}
