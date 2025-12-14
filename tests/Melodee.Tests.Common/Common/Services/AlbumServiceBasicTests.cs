using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using NodaTime;
using dbModels = Melodee.Common.Data.Models;

namespace Melodee.Tests.Common.Common.Services;

/// <summary>
/// Basic tests for AlbumService focusing on core functionality without complex data models
/// </summary>
public class AlbumServiceBasicTests : ServiceTestBase
{
    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithInvalidIdZero_ThrowsException()
    {
        // Arrange
        var service = GetAlbumService();
        var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetAsync(invalidId));
    }

    [Fact]
    public async Task GetAsync_WithInvalidIdNegative_ThrowsException()
    {
        // Arrange
        var service = GetAlbumService();
        var invalidId = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetAsync(invalidId));
    }

    [Fact]
    public async Task GetAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var service = GetAlbumService();
        var invalidId = 99999;

        // Act
        var result = await service.GetAsync(invalidId);

        // Assert
        Assert.NotNull(result);
        // The service may return false for IsSuccess when not found
        Assert.Null(result.Data);
    }

    #endregion

    #region GetByApiKeyAsync Tests

    [Fact]
    public async Task GetByApiKeyAsync_WithEmptyGuid_ThrowsException()
    {
        // Arrange
        var service = GetAlbumService();
        var emptyGuid = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetByApiKeyAsync(emptyGuid));
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithNonExistentApiKey_ReturnsError()
    {
        // Arrange
        var service = GetAlbumService();
        var nonExistentApiKey = Guid.NewGuid();

        // Act
        var result = await service.GetByApiKeyAsync(nonExistentApiKey);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Contains("Unknown album", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    #endregion

    #region GetByMusicBrainzIdAsync Tests

    [Fact]
    public async Task GetByMusicBrainzIdAsync_WithNonExistentMusicBrainzId_ReturnsError()
    {
        // Arrange
        var service = GetAlbumService();
        var nonExistentMusicBrainzId = Guid.NewGuid();

        // Act
        var result = await service.GetByMusicBrainzIdAsync(nonExistentMusicBrainzId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Contains("Unknown album", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    #endregion

    #region ClearCache Tests

    [Fact]
    public void ClearCache_WithValidAlbum_DoesNotThrowException()
    {
        // Arrange
        var service = GetAlbumService();
        var album = new dbModels.Album
        {
            Id = 1,
            ApiKey = Guid.NewGuid(),
            Name = "Test Album",
            NameNormalized = "testalbum",
            MusicBrainzId = Guid.NewGuid(),
            Directory = "TestAlbum",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            ArtistId = 1
        };

        // Act & Assert - Should not throw exception
        service.ClearCache(album);
        Assert.True(true); // Test passes if no exception is thrown
    }

    [Fact]
    public void ClearCache_WithAlbumWithoutMusicBrainzId_DoesNotThrowException()
    {
        // Arrange
        var service = GetAlbumService();
        var album = new dbModels.Album
        {
            Id = 1,
            ApiKey = Guid.NewGuid(),
            Name = "Test Album",
            NameNormalized = "testalbum",
            MusicBrainzId = null,
            Directory = "TestAlbum",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            ArtistId = 1
        };

        // Act & Assert - Should not throw exception
        service.ClearCache(album);
        Assert.True(true); // Test passes if no exception is thrown
    }

    #endregion

    #region ListAsync Tests

    [Fact]
    public async Task ListAsync_WithEmptyDatabase_ReturnsEmptyResult()
    {
        // Arrange
        var service = GetAlbumService();
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await service.ListAsync(pagedRequest);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalCount);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithNullArray_ThrowsException()
    {
        // Arrange
        var service = GetAlbumService();
        int[] nullArray = null!;

        // Act & Assert
        // ArgumentNullException is a subclass of ArgumentException
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.DeleteAsync(nullArray));
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyArray_ThrowsException()
    {
        // Arrange
        var service = GetAlbumService();
        var emptyArray = Array.Empty<int>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.DeleteAsync(emptyArray));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentAlbumId_ReturnsError()
    {
        // Arrange
        var service = GetAlbumService();
        var nonExistentIds = new[] { 99999 };

        // Act
        var result = await service.DeleteAsync(nonExistentIds);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.False(result.Data);
        Assert.Contains("Unknown album", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    #endregion
}
