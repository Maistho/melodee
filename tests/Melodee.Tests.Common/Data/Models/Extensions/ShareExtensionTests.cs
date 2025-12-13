using Melodee.Common.Configuration;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Moq;

namespace Melodee.Tests.Common.Data.Models.Extensions;

public class ShareExtensionTests
{
    [Fact]
    public void ToApiKey_ShouldReturnShareApiKeyWithPrefix()
    {
        // Arrange
        var user = new Melodee.Common.Data.Models.User
        {
            UserName = "TestUser",
            UserNameNormalized = "testuser",
            Email = "test@example.com",
            EmailNormalized = "test@example.com",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        var share = new Melodee.Common.Data.Models.Share
        {
            ApiKey = Guid.NewGuid(),
            ShareUniqueId = "test-unique-id",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            UserId = 1,
            ShareId = 1,
            User = user
        };

        // Act
        var result = share.ToApiKey();

        // Assert
        Assert.StartsWith("share_", result);
        Assert.Contains(share.ApiKey.ToString(), result);
    }

    [Fact]
    public void ToUrl_ShouldReturnValidShareUrl()
    {
        // Arrange
        var user = new Melodee.Common.Data.Models.User
        {
            UserName = "TestUser",
            UserNameNormalized = "testuser",
            Email = "test@example.com",
            EmailNormalized = "test@example.com",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        var share = new Melodee.Common.Data.Models.Share
        {
            ShareUniqueId = "test-unique-id",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            UserId = 1,
            ShareId = 1,
            User = user
        };

        var config = new Mock<IMelodeeConfiguration>();
        config.Setup(x => x.GetValue<string>(It.IsAny<string>())).Returns("http://example.com");

        // Act
        var result = share.ToUrl(config.Object);

        // Assert
        Assert.Equal("http://example.com/share/test-unique-id", result);
    }

    [Fact]
    public void ToApiShare_ShouldReturnValidApiShare()
    {
        // Arrange
        var user = new Melodee.Common.Data.Models.User
        {
            UserName = "TestUser",
            UserNameNormalized = "testuser",
            Email = "test@example.com",
            EmailNormalized = "test@example.com",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        var share = new Melodee.Common.Data.Models.Share
        {
            Id = 1,
            ApiKey = Guid.NewGuid(),
            Description = "Test Description",
            User = user,
            UserId = 1,
            ShareId = 1,
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            ExpiresAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow.AddDays(7)),
            LastVisitedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            VisitCount = 5
        };

        var child = new Child[] { };

        // Act
        var result = share.ToApiShare("http://example.com/share/test", child);

        // Assert
        Assert.Contains(share.ApiKey.ToString(), result.Id);
        Assert.Equal("http://example.com/share/test", result.Url);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal("TestUser", result.UserName);
        Assert.Equal(5, result.VisitCount);
        Assert.NotNull(result.Entry);
    }

    [Fact]
    public void ToApiShare_WithNullExpirationAndVisit_ShouldHandleProperly()
    {
        // Arrange
        var user = new Melodee.Common.Data.Models.User
        {
            UserName = "TestUser",
            UserNameNormalized = "testuser",
            Email = "test@example.com",
            EmailNormalized = "test@example.com",
            PublicKey = "testkey",
            PasswordEncrypted = "encrypted",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        var share = new Melodee.Common.Data.Models.Share
        {
            Id = 1,
            ApiKey = Guid.NewGuid(),
            Description = "Test Description",
            User = user,
            UserId = 1,
            ShareId = 1,
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
            ExpiresAt = null,  // No expiration
            LastVisitedAt = null,  // Never visited
            VisitCount = 0
        };

        var child = new Child[] { };

        // Act
        var result = share.ToApiShare("http://example.com/share/test", child);

        // Assert
        Assert.Contains(share.ApiKey.ToString(), result.Id);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal("TestUser", result.UserName);
        Assert.Equal(0, result.VisitCount);
        Assert.Null(result.Expires);
        Assert.Null(result.LastVisited);
    }
}