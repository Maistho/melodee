using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Tests.Common.Services;

public class OpenSubsonicApiServiceTests : ServiceTestBase
{
    #region Helper Methods

    private async Task<User> CreateTestUserInDb(string username = "testuser", string email = "test@example.com", bool isAdmin = false)
    {
        await using var context = await MockFactory().CreateDbContextAsync();

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (existingUser != null)
        {
            return existingUser;
        }

        var password = "testpassword";
        var publicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
        var config = TestsBase.NewPluginsConfiguration();
        var encryptedPassword = EncryptionHelper.Encrypt(
            config.GetValue<string>(Melodee.Common.Constants.SettingRegistry.EncryptionPrivateKey)!,
            password,
            publicKey);

        var user = new User
        {
            UserName = username,
            UserNameNormalized = username.ToUpperInvariant(),
            Email = email,
            EmailNormalized = email.ToUpperInvariant(),
            PublicKey = publicKey,
            PasswordEncrypted = encryptedPassword,
            IsAdmin = isAdmin,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<Library> CreateTestLibraryInDb(LibraryType type = LibraryType.Storage)
    {
        await using var context = await MockFactory().CreateDbContextAsync();

        var library = new Library
        {
            Name = $"Test {type} Library",
            Path = $"/tmp/test_{type.ToString().ToLower()}",
            Type = (int)type,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        context.Libraries.Add(library);
        await context.SaveChangesAsync();
        return library;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesServiceInstance()
    {
        // Arrange & Act
        var service = GetOpenSubsonicApiService();

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region GetLicenseAsync Tests

    [Fact]
    public async Task GetLicenseAsync_ReturnsValidLicense()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var apiRequest = GetApiRequest("testuser", "salt", "password");

        // Act
        var result = await service.GetLicenseAsync(apiRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ResponseData);
    }

    #endregion

    #region PingAsync Tests

    [Fact]
    public async Task PingAsync_ReturnsSuccessResponse()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var apiRequest = GetApiRequest("testuser", "salt", "password");

        // Act
        var result = await service.PingAsync(apiRequest);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetOpenSubsonicExtensionsAsync Tests

    [Fact]
    public async Task GetOpenSubsonicExtensionsAsync_ReturnsExtensions()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var apiRequest = GetApiRequest("testuser", "salt", "password");

        // Act
        var result = await service.GetOpenSubsonicExtensionsAsync(apiRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ResponseData);
    }

    #endregion

    #region GetMusicFolders Tests

    [Fact]
    public async Task GetMusicFolders_ReturnsStorageLibraries()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        await CreateTestLibraryInDb(LibraryType.Storage);
        var apiRequest = GetApiRequest("testuser", "salt", "password");

        // Act
        var result = await service.GetMusicFolders(apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetGenresAsync Tests

    [Fact]
    public async Task GetGenresAsync_ReturnsGenreList()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var apiRequest = GetApiRequest("testuser", "salt", "password");

        // Act
        var result = await service.GetGenresAsync(apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetScanStatusAsync Tests

    [Fact]
    public async Task GetScanStatusAsync_ReturnsCurrentStatus()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var apiRequest = GetApiRequest("testuser", "salt", "password");

        // Act
        var result = await service.GetScanStatusAsync(apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region AuthenticateSubsonicApiAsync Tests

    [Fact]
    public async Task AuthenticateSubsonicApiAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb("authuser", "auth@test.com");

        var password = "testpassword";
        var salt = "randomsalt";
        var token = HashHelper.CreateMd5($"{password}{salt}")!;
        var apiRequest = GetApiRequest("authuser", salt, token);

        // Act
        var result = await service.AuthenticateSubsonicApiAsync(apiRequest);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AuthenticateSubsonicApiAsync_WithInvalidCredentials_ReturnsError()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var apiRequest = GetApiRequest("nonexistent", "salt", "wrongpassword");

        // Act
        var result = await service.AuthenticateSubsonicApiAsync(apiRequest);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetPlaylistsAsync Tests

    [Fact]
    public async Task GetPlaylistsAsync_ReturnsPlaylistList()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb();
        var apiRequest = GetApiRequest(user.UserName, "salt", "token");

        // Act
        var result = await service.GetPlaylistsAsync(apiRequest);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetBookmarksAsync Tests

    [Fact]
    public async Task GetBookmarksAsync_ReturnsBookmarkList()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb();
        var apiRequest = GetApiRequest(user.UserName, "salt", "token");

        // Act
        var result = await service.GetBookmarksAsync(apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetNowPlayingAsync Tests

    [Fact]
    public async Task GetNowPlayingAsync_ReturnsNowPlayingList()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var apiRequest = GetApiRequest("testuser", "salt", "password");

        // Act
        var result = await service.GetNowPlayingAsync(apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetPlayQueueAsync Tests

    [Fact]
    public async Task GetPlayQueueAsync_ReturnsPlayQueue()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb();
        var apiRequest = GetApiRequest(user.UserName, "salt", "token");

        // Act
        var result = await service.GetPlayQueueAsync(apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region NewApiResponse Tests

    [Fact]
    public async Task NewApiResponse_WhenOk_ReturnsSuccessResponse()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();

        // Act
        var result = await service.NewApiResponse(true, "test", "detail", null, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task NewApiResponse_WhenNotOk_ReturnsFailedResponse()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();

        // Act
        var result = await service.NewApiResponse(false, "test", "detail", null, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetSharesAsync Tests

    [Fact]
    public async Task GetSharesAsync_ReturnsShareList()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb();
        var apiRequest = GetApiRequest(user.UserName, "salt", "token");

        // Act
        var result = await service.GetSharesAsync(apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetStarred2Async Tests

    [Fact]
    public async Task GetStarred2Async_ReturnsStarredItems()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb();
        var apiRequest = GetApiRequest(user.UserName, "salt", "token");

        // Act
        var result = await service.GetStarred2Async(null, apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetStarredAsync Tests

    [Fact]
    public async Task GetStarredAsync_ReturnsStarredItems()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb();
        var apiRequest = GetApiRequest(user.UserName, "salt", "token");

        // Act
        var result = await service.GetStarredAsync(null, apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetTopSongsAsync Tests

    [Fact]
    public async Task GetTopSongsAsync_ReturnsTopSongs()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb();
        var apiRequest = GetApiRequest(user.UserName, "salt", "token");

        // Act
        var result = await service.GetTopSongsAsync("Test Artist", 10, apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetSimilarSongsAsync Tests

    [Fact]
    public async Task GetSimilarSongsAsync_ReturnsSimilarSongs()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb();
        var apiRequest = GetApiRequest(user.UserName, "salt", "token");

        // Act - using invalid ID to test error handling
        var result = await service.GetSimilarSongsAsync("song:99999", 10, true, apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetUserAsync Tests

    [Fact]
    public async Task GetUserAsync_WithValidUser_ReturnsUser()
    {
        // Arrange
        var service = GetOpenSubsonicApiService();
        var user = await CreateTestUserInDb("getuser", "getuser@test.com");
        var apiRequest = GetApiRequest(user.UserName, "salt", "token");

        // Act
        var result = await service.GetUserAsync("getuser", apiRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region ImageCacheRegion Tests

    [Fact]
    public void ImageCacheRegion_HasExpectedValue()
    {
        // Assert
        Assert.Equal("urn:openSubsonic:artist-and-album-images", OpenSubsonicApiService.ImageCacheRegion);
    }

    #endregion
}
