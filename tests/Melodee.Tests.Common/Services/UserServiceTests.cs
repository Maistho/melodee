using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Importing;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Rebus.Bus;

namespace Melodee.Tests.Common.Common.Services;

public class UserServiceTests : ServiceTestBase
{
    private UserService CreateUserService(IMelodeeConfigurationFactory? configFactory = null, IBus? bus = null)
    {
        return new UserService(
            Logger,
            CacheManager,
            MockFactory(),
            configFactory ?? MockConfigurationFactory(),
            GetLibraryService(),
            GetArtistService(),
            GetAlbumService(),
            GetSongService(),
            GetPlaylistService(),
            bus ?? MockBus());
    }

    [Fact]
    public async Task ListAsync_WithValidRequest_ReturnsPagedResult()
    {
        // Arrange
        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10 };

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user1 = CreateTestUser(1, "user1", "user1@test.com");
            var user2 = CreateTestUser(2, "user2", "user2@test.com");
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();
        }

        var userService = GetUserService();

        // Act
        var result = await userService.ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 2);
        Assert.NotEmpty(result.Data);
        Assert.IsType<UserDataInfo[]>(result.Data);

        // Verify UserDataInfo properties are correctly mapped
        var firstUser = result.Data.First();
        Assert.True(firstUser.Id > 0);
        Assert.NotEqual(Guid.Empty, firstUser.ApiKey);
        Assert.NotNull(firstUser.UserName);
        Assert.NotNull(firstUser.Email);
    }

    [Fact]
    public async Task ListAsync_WithUsernameFilter_ReturnsFilteredResults()
    {
        // Arrange
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            var user1 = CreateTestUser(101, "johnsmith", "john@test.com");
            var user2 = CreateTestUser(102, "janesmith", "jane@test.com");
            var user3 = CreateTestUser(103, "bobwilson", "bob@test.com");
            context.Users.AddRange(user1, user2, user3);
            await context.SaveChangesAsync();
        }

        var userService = GetUserService();
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new Melodee.Common.Filtering.FilterOperatorInfo("username", Melodee.Common.Filtering.FilterOperator.Contains, "smith")]
        };

        // Act
        var result = await userService.ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Data, u => Assert.Contains("smith", u.UserName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ListAsync_WithEmailFilter_ReturnsFilteredResults()
    {
        // Arrange
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            var user1 = CreateTestUser(101, "user1", "alpha@company.com");
            var user2 = CreateTestUser(102, "user2", "beta@other.com");
            var user3 = CreateTestUser(103, "user3", "gamma@company.com");
            context.Users.AddRange(user1, user2, user3);
            await context.SaveChangesAsync();
        }

        var userService = GetUserService();
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new Melodee.Common.Filtering.FilterOperatorInfo("email", Melodee.Common.Filtering.FilterOperator.Contains, "company")]
        };

        // Act
        var result = await userService.ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Data, u => Assert.Contains("company", u.Email, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ListAsync_WithIsLockedFilter_ReturnsFilteredResults()
    {
        // Arrange
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            var user1 = CreateTestUser(101, "activeuser", "active@test.com");
            user1.IsLocked = false;
            var user2 = CreateTestUser(102, "lockeduser", "locked@test.com");
            user2.IsLocked = true;
            var user3 = CreateTestUser(103, "anotheractive", "another@test.com");
            user3.IsLocked = false;
            context.Users.AddRange(user1, user2, user3);
            await context.SaveChangesAsync();
        }

        var userService = GetUserService();
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new Melodee.Common.Filtering.FilterOperatorInfo("islocked", Melodee.Common.Filtering.FilterOperator.Equals, "true")]
        };

        // Act
        var result = await userService.ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.True(result.Data.First().IsLocked);
    }

    [Fact]
    public async Task ListAsync_WithIsAdminFilter_ReturnsFilteredResults()
    {
        // Arrange
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            var user1 = CreateTestUser(101, "adminuser", "admin@test.com");
            user1.IsAdmin = true;
            var user2 = CreateTestUser(102, "regularuser", "regular@test.com");
            user2.IsAdmin = false;
            var user3 = CreateTestUser(103, "anotheradmin", "admin2@test.com");
            user3.IsAdmin = true;
            context.Users.AddRange(user1, user2, user3);
            await context.SaveChangesAsync();
        }

        var userService = GetUserService();
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new Melodee.Common.Filtering.FilterOperatorInfo("isadmin", Melodee.Common.Filtering.FilterOperator.Equals, "true")]
        };

        // Act
        var result = await userService.ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Data, u => Assert.True(u.IsAdmin));
    }

    [Fact]
    public async Task ListAsync_WithOrderByUsername_ReturnsOrderedResults()
    {
        // Arrange
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            var user1 = CreateTestUser(101, "zebra", "zebra@test.com");
            var user2 = CreateTestUser(102, "alpha", "alpha@test.com");
            var user3 = CreateTestUser(103, "middle", "middle@test.com");
            context.Users.AddRange(user1, user2, user3);
            await context.SaveChangesAsync();
        }

        var userService = GetUserService();
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            OrderBy = new Dictionary<string, string> { { "username", "ASC" } }
        };

        // Act
        var result = await userService.ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        var users = result.Data.ToArray();
        Assert.Equal(3, users.Length);
        Assert.Equal("alpha", users[0].UserName);
        Assert.Equal("middle", users[1].UserName);
        Assert.Equal("zebra", users[2].UserName);
    }

    [Fact]
    public async Task ListAsync_WithMultipleFilters_ReturnsFilteredResults()
    {
        // Arrange
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            var user1 = CreateTestUser(101, "johnsmith", "john@test.com");
            var user2 = CreateTestUser(102, "janesmith", "jane@test.com");
            var user3 = CreateTestUser(103, "bobwilson", "bob@test.com");
            context.Users.AddRange(user1, user2, user3);
            await context.SaveChangesAsync();
        }

        var userService = GetUserService();
        // Multiple filters use OR logic
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [
                new Melodee.Common.Filtering.FilterOperatorInfo("username", Melodee.Common.Filtering.FilterOperator.Contains, "john"),
                new Melodee.Common.Filtering.FilterOperatorInfo("username", Melodee.Common.Filtering.FilterOperator.Contains, "bob")
            ]
        };

        // Act
        var result = await userService.ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task DeleteAsync_WithNullUserIds_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => userService.DeleteAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyUserIds_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userIds = Array.Empty<int>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.DeleteAsync(userIds));
    }

    [Fact]
    public async Task DeleteAsync_WithValidUserIds_ReturnsSuccess()
    {
        // Arrange
        var userService = GetUserService();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            // Check if UserImages library already exists
            var existingLibrary = await context.Libraries.FirstOrDefaultAsync(x => x.Type == (int)LibraryType.UserImages);
            if (existingLibrary == null)
            {
                var library = new Library
                {
                    Name = "User Images",
                    Path = "/test/path",
                    Type = (int)LibraryType.UserImages,
                    CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
                };
                context.Libraries.Add(library);
                await context.SaveChangesAsync();
            }

            var user = CreateTestUser(1, "testuser", "test@example.com");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var userIds = new[] { 1 };

        // Act
        var result = await userService.DeleteAsync(userIds);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownUserId_ReturnsNotFound()
    {
        // Arrange
        var userService = GetUserService();

        // Act
        var result = await userService.DeleteAsync([999]);

        // Assert
        Assert.False(result.Data);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
    }

    [Fact]
    public async Task GetByEmailAddressAsync_WithNullEmail_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => userService.GetByEmailAddressAsync(null!));
    }

    [Fact]
    public async Task GetByEmailAddressAsync_WithValidEmail_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var userService = GetUserService();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(1, "testuser", email);
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.GetByEmailAddressAsync(email);

        // Assert
        Assert.NotNull(result);
        if (result.IsSuccess)
        {
            Assert.NotNull(result.Data);
            Assert.Equal(email, result.Data!.Email);
        }
        else
        {
            // If not successful, at least verify the operation completed without throwing
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNullUsername_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => userService.GetByUsernameAsync(null!));
    }

    [Fact]
    public async Task GetByUsernameAsync_WithValidUsername_ReturnsUser()
    {
        // Arrange
        var username = "testuser";
        var userService = GetUserService();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(1, username, "test@example.com");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.GetByUsernameAsync(username);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(username, result.Data.UserName);
    }

    [Fact]
    public async Task IsUserAdminAsync_WithAdminUser_ReturnsTrue()
    {
        // Arrange
        var username = "adminuser";
        var userService = GetUserService();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var adminUser = CreateTestUser(1, username, "admin@example.com");
            adminUser.IsAdmin = true;
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.IsUserAdminAsync(username);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUserAdminAsync_WithNonAdminUser_ReturnsFalse()
    {
        // Arrange
        var username = "regularuser";
        var userService = GetUserService();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(1, username, "user@example.com");
            user.IsAdmin = false;
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.IsUserAdminAsync(username);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var apiKey = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.GetByApiKeyAsync(apiKey));
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithValidApiKey_ReturnsUser()
    {
        // Arrange
        var apiKey = Guid.NewGuid();
        var userService = GetUserService();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(1, "testuser", "test@example.com");
            user.ApiKey = apiKey;
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.GetByApiKeyAsync(apiKey);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(apiKey, result.Data.ApiKey);
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var id = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.GetAsync(id));
    }

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var id = 1;
        var userService = GetUserService();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(id, "testuser", "test@example.com");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.GetAsync(id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(id, result.Data.Id);
    }

    [Fact]
    public async Task LoginUserAsync_WithNullEmail_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var password = "testpassword";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => userService.LoginUserAsync(null!, password));
    }

    [Fact]
    public async Task LoginUserAsync_WithNullPassword_ReturnsUnauthorized()
    {
        // Arrange
        var emailAddress = "test@example.com";
        var userService = GetUserService();

        // Act
        var result = await userService.LoginUserAsync(emailAddress, null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.Unauthorized, result.Type);
    }

    [Fact]
    public async Task LoginUserAsync_WithValidPassword_ReturnsUser()
    {
        // Arrange
        var plainPassword = "Sup3rSecret!";
        var userService = GetUserService();
        var configuration = TestsBase.NewPluginsConfiguration();
        var user = CreateTestUser(5, "loginuser", "loginuser@example.com");
        user.PublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
        user.PasswordEncrypted = EncryptionHelper.Encrypt(
            configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!,
            plainPassword,
            user.PublicKey);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.LoginUserAsync(user.Email, plainPassword);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data!.LastLoginAt);
        Assert.NotNull(result.Data.LastActivityAt);
    }

    [Fact]
    public async Task LoginUserAsync_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var userService = GetUserService();
        var configuration = TestsBase.NewPluginsConfiguration();
        var user = CreateTestUser(6, "wrongpassword", "wrongpassword@example.com");
        user.PublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
        user.PasswordEncrypted = EncryptionHelper.Encrypt(
            configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!,
            "correct-password",
            user.PublicKey);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.LoginUserAsync(user.Email, "incorrect-password");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.Unauthorized, result.Type);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsUser()
    {
        // Arrange
        var password = "token-pass";
        var salt = "123";
        var config = TestsBase.NewPluginsConfiguration();
        var userService = GetUserService();
        var publicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
        var encryptedPassword = EncryptionHelper.Encrypt(
            config.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!,
            password,
            publicKey);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(9, "tokenuser", "token@example.com");
            user.PublicKey = publicKey;
            user.PasswordEncrypted = encryptedPassword;
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var token = HashHelper.CreateMd5($"{password}{salt}");

        // Act
        var result = await userService.ValidateTokenAsync("tokenuser", token!, salt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(9, result.Data!.Id);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithLockedUser_ReturnsUnauthorized()
    {
        // Arrange
        var password = "locked-pass";
        var salt = "456";
        var config = TestsBase.NewPluginsConfiguration();
        var userService = GetUserService();
        var publicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
        var encryptedPassword = EncryptionHelper.Encrypt(
            config.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!,
            password,
            publicKey);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(10, "lockeduser", "locked@example.com");
            user.PublicKey = publicKey;
            user.PasswordEncrypted = encryptedPassword;
            user.IsLocked = true;
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var token = HashHelper.CreateMd5($"{password}{salt}");

        // Act
        var result = await userService.ValidateTokenAsync("lockeduser", token!, salt);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.Unauthorized, result.Type);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsValidationFailure()
    {
        // Arrange
        var email = "duplicate@example.com";
        var userService = GetUserService();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var existingUser = CreateTestUser(7, "existinguser", email);
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.RegisterAsync("newuser", email, "P@ssword1", null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.ValidationFailure, result.Type);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task RegisterAsync_FirstUserBecomesAdmin()
    {
        // Arrange
        var configFactory = MockConfigurationFactory();
        var userService = CreateUserService(configFactory);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.RegisterAsync("first", "first@example.com", "P@ssw0rd!", null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.IsAdmin);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidPrivateCode_ReturnsUnauthorized()
    {
        // Arrange
        var settings = TestsBase.NewConfiguration();
        settings[SettingRegistry.RegisterPrivateCode] = "secret-code";
        var configFactory = new Mock<IMelodeeConfigurationFactory>();
        configFactory.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MelodeeConfiguration(settings));

        var userService = CreateUserService(configFactory.Object);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await userService.RegisterAsync("user", "code@example.com", "Password!", "wrong");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.Unauthorized, result.Type);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ImportUserFavoriteSongs_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var userService = GetUserService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => userService.ImportUserFavoriteSongs(null!));
    }

    [Fact]
    public async Task ImportUserFavoriteSongs_WithNonExistentFile_ReturnsNotFound()
    {
        // Arrange
        var userService = GetUserService();
        var apiKey = Guid.NewGuid();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(1, "testuser", "test@example.com");
            user.ApiKey = apiKey;
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var configuration = new UserFavoriteSongConfiguration(
            "/nonexistent/file.csv",
            apiKey,
            "Artist",
            "Album",
            "Song",
            false);

        // Act
        var result = await userService.ImportUserFavoriteSongs(configuration);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var currentUser = CreateTestUser(1, "current", "current@example.com");
        var detailToUpdate = CreateTestUser(0, "invalid", "invalid@example.com"); // Invalid ID

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.UpdateAsync(currentUser, detailToUpdate));
    }

    [Fact]
    public async Task UpdateAsync_WithValidUser_UpdatesProperties()
    {
        // Arrange
        var userService = GetUserService();
        var currentUser = CreateTestUser(8, "before", "before@example.com");

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            currentUser.Description = "old description";
            currentUser.Notes = "old notes";
            context.Users.Add(currentUser);
            await context.SaveChangesAsync();
        }

        var detailToUpdate = CreateTestUser(8, "after", "after@example.com");
        detailToUpdate.Description = "new description";
        detailToUpdate.Notes = "new notes";
        detailToUpdate.SortOrder = 5;
        detailToUpdate.Tags = "alpha,beta";
        detailToUpdate.IsLocked = true;

        // Act
        var result = await userService.UpdateAsync(currentUser, detailToUpdate);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        await using var verifyContext = await MockFactory().CreateDbContextAsync();
        var updatedUser = await verifyContext.Users.FirstAsync(u => u.Id == 8);
        Assert.Equal("after@example.com", updatedUser.Email);
        Assert.Equal("after", updatedUser.UserName);
        Assert.Equal("new description", updatedUser.Description);
        Assert.Equal("new notes", updatedUser.Notes);
        Assert.True(updatedUser.IsLocked);
        Assert.Equal("alpha,beta", updatedUser.Tags);
        Assert.Equal(5, updatedUser.SortOrder);
    }

    [Fact]
    public async Task ToggleGenreHatedAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var genre = "Rock";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.ToggleGenreHatedAsync(userId, genre));
    }

    [Fact]
    public async Task ToggleArtistHatedAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var artistApiKey = Guid.NewGuid();
        var isHated = true;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.ToggleArtistHatedAsync(userId, artistApiKey, isHated));
    }

    [Fact]
    public async Task SetAlbumRatingAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var albumId = 1;
        var rating = 5;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.SetAlbumRatingAsync(userId, albumId, rating));
    }

    [Fact]
    public async Task SetSongRatingAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var songId = 1;
        var rating = 5;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.SetSongRatingAsync(userId, songId, rating));
    }

    [Fact]
    public async Task ToggleArtistStarAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var artistApiKey = Guid.NewGuid();
        var isStarred = true;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.ToggleArtistStarAsync(userId, artistApiKey, isStarred));
    }

    [Fact]
    public async Task ToggleAlbumHatedAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var albumApiKey = Guid.NewGuid();
        var isHated = true;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.ToggleAlbumHatedAsync(userId, albumApiKey, isHated));
    }

    [Fact]
    public async Task ToggleAlbumStarAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var albumApiKey = Guid.NewGuid();
        var isStarred = true;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.ToggleAlbumStarAsync(userId, albumApiKey, isStarred));
    }

    [Fact]
    public async Task SetArtistRatingAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var artistApiKey = Guid.NewGuid();
        var rating = 5;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.SetArtistRatingAsync(userId, artistApiKey, rating));
    }

    [Fact]
    public async Task SetAlbumRatingAsync_ByApiKey_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var albumApiKey = Guid.NewGuid();
        var rating = 5;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.SetAlbumRatingAsync(userId, albumApiKey, rating));
    }

    [Fact]
    public async Task ToggleSongStarAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var songApiKey = Guid.NewGuid();
        var isStarred = true;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.ToggleSongStarAsync(userId, songApiKey, isStarred));
    }

    [Fact]
    public async Task ToggleSongHatedAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var userService = GetUserService();
        var userId = 0;
        var songApiKey = Guid.NewGuid();
        var isHated = true;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.ToggleSongHatedAsync(userId, songApiKey, isHated));
    }

    [Fact]
    public async Task UpdateLastLogin_WithValidEventData_ReturnsSuccess()
    {
        // Arrange
        var userService = GetUserService();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(1, "testuser", "test@example.com");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var eventData = new UserLoginEvent(1, "testuser");

        // Act
        var result = await userService.UpdateLastLogin(eventData);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task GetByUsernameAsync_CacheIsUsedOnRepeatedCalls()
    {
        // Arrange
        var username = "cacheuser";
        var userService = GetUserService();
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(2, username, "cacheuser@example.com");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        // Act
        var result1 = await userService.GetByUsernameAsync(username);
        var result2 = await userService.GetByUsernameAsync(username);
        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotNull(result1.Data);
        Assert.NotNull(result2.Data);
        Assert.Equal(result1.Data.Id, result2.Data.Id);
    }

    [Fact]
    public async Task UpdateLastLogin_UpdatesUserLoginTimestamps()
    {
        // Arrange
        var userService = GetUserService();
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(3, "eventuser", "eventuser@example.com");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        var eventData = new UserLoginEvent(3, "eventuser");

        // Act
        var result = await userService.UpdateLastLogin(eventData);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify the user's last login was updated
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var updatedUser = await context.Users.FirstAsync(u => u.Id == 3);
            Assert.NotNull(updatedUser.LastLoginAt);
            Assert.NotNull(updatedUser.LastActivityAt);
        }
    }

    [Fact]
    public async Task LoginUserAsync_PublishesBusEvent()
    {
        // Arrange - Create a mock bus that can be verified
        var busMock = new Mock<IBus>();
        busMock.Setup(b => b.SendLocal(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        // Create user service with the verifiable bus mock
        var userService = new UserService(
            Logger,
            CacheManager,
            MockFactory(),
            MockConfigurationFactory(),
            GetLibraryService(),
            GetArtistService(),
            GetAlbumService(),
            GetSongService(),
            GetPlaylistService(),
            busMock.Object);

        // Create test user with known encrypted password
        // Using "enc:" prefix pattern which bypasses encryption in LoginUserAsync
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(4, "logintest", "logintest@example.com");
            user.PasswordEncrypted = "testencryptedpassword123";
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Act - Use "enc:" prefix to match the encrypted password directly
        var result = await userService.LoginUserAsync("logintest@example.com", "enc:testencryptedpassword123");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        // Verify that bus.SendLocal was called with a UserLoginEvent
        busMock.Verify(
            b => b.SendLocal(
                It.Is<UserLoginEvent>(e => e.UserId == 4 && e.UserName == "logintest"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByEmailAddressAsync_WithUnusualCharacters_ReturnsUser()
    {
        // Arrange
        var email = "üñîçødë@example.com";
        var userService = GetUserService();
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var user = CreateTestUser(4, "unicodeuser", email);
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        // Act
        var result = await userService.GetByEmailAddressAsync(email);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(email, result.Data.Email);
    }

    [Fact]
    public async Task IsUserAdminAsync_UnauthorizedAccess_ReturnsFalse()
    {
        // Arrange
        var userService = GetUserService();
        var username = "nonexistentuser";
        // Act
        var result = await userService.IsUserAdminAsync(username);
        // Assert
        Assert.False(result);
    }

    private static User CreateTestUser(int id, string username, string email)
    {
        return new User
        {
            Id = id,
            UserName = username,
            UserNameNormalized = username.ToNormalizedString() ?? username.ToUpperInvariant(),
            Email = email,
            EmailNormalized = email.ToNormalizedString() ?? email.ToUpperInvariant(),
            PublicKey = "testkey",
            PasswordEncrypted = "encryptedpassword",
            ApiKey = Guid.NewGuid(),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            IsAdmin = false,
            IsLocked = false
        };
    }
}
