using FluentAssertions;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using NodaTime.Testing;

namespace Melodee.Tests.Common.Security;

/// <summary>
/// Tests for RefreshTokenService with rotation and replay detection.
/// </summary>
public class RefreshTokenServiceTests : IDisposable
{
    private readonly MelodeeDbContext _dbContext;
    private readonly IDbContextFactory<MelodeeDbContext> _dbContextFactory;
    private readonly Mock<ILogger<RefreshTokenService>> _loggerMock;
    private readonly FakeClock _clock;
    private readonly TokenOptions _tokenOptions;

    public RefreshTokenServiceTests()
    {
        var options = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseInMemoryDatabase(databaseName: $"RefreshTokenTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new MelodeeDbContext(options);
        _dbContext.Database.EnsureCreated();

        // Seed a test user
        _dbContext.Users.Add(new User
        {
            Id = 1,
            UserName = "testuser",
            UserNameNormalized = "TESTUSER",
            Email = "test@example.com",
            EmailNormalized = "TEST@EXAMPLE.COM",
            PublicKey = "test-public-key",
            PasswordEncrypted = "test-password-hash",
            CreatedAt = Instant.FromUtc(2024, 1, 1, 0, 0)
        });
        _dbContext.SaveChanges();

        var factoryMock = new Mock<IDbContextFactory<MelodeeDbContext>>();
        factoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MelodeeDbContext(options));
        _dbContextFactory = factoryMock.Object;

        _loggerMock = new Mock<ILogger<RefreshTokenService>>();
        _clock = new FakeClock(Instant.FromUtc(2024, 6, 15, 12, 0));

        _tokenOptions = new TokenOptions
        {
            AccessTokenLifetimeMinutes = 15,
            RefreshTokenLifetimeDays = 30,
            MaxSessionDays = 90,
            RotateRefreshTokens = true,
            RevokeOnReplay = true
        };
    }

    private RefreshTokenService CreateService(TokenOptions? options = null)
    {
        var opts = options ?? _tokenOptions;
        var optionsMock = new Mock<IOptions<TokenOptions>>();
        optionsMock.Setup(x => x.Value).Returns(opts);
        return new RefreshTokenService(_dbContextFactory, optionsMock.Object, _loggerMock.Object, _clock);
    }

    #region CreateTokenAsync Tests

    [Fact]
    public async Task CreateTokenAsync_CreatesNewToken()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.CreateTokenAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.TokenFamily.Should().NotBeNullOrEmpty();
        result.UserId.Should().Be(1);
        result.ExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTokenAsync_StoresHashedToken()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.CreateTokenAsync(1);

        // Assert
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var storedToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == 1);

        storedToken.Should().NotBeNull();
        storedToken!.HashedToken.Should().NotBe(result.Token); // Should be hashed, not raw
        storedToken.TokenFamily.Should().Be(result.TokenFamily);
    }

    [Fact]
    public async Task CreateTokenAsync_SetsCorrectExpiration()
    {
        // Arrange
        var service = CreateService();
        var expectedExpiry = _clock.GetCurrentInstant().Plus(Duration.FromDays(30));

        // Act
        var result = await service.CreateTokenAsync(1);

        // Assert
        result.ExpiresAt.Should().Be(expectedExpiry);
    }

    [Fact]
    public async Task CreateTokenAsync_StoresDeviceMetadata()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.CreateTokenAsync(
            1,
            deviceId: "device-123",
            userAgent: "TestBrowser/1.0",
            ipAddress: "192.168.1.1");

        // Assert
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var storedToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenFamily == result.TokenFamily);

        storedToken.Should().NotBeNull();
        storedToken!.DeviceId.Should().Be("device-123");
        storedToken.UserAgent.Should().Be("TestBrowser/1.0");
        storedToken.IpAddress.Should().Be("192.168.1.1");
    }

    #endregion

    #region RotateTokenAsync Tests

    [Fact]
    public async Task RotateTokenAsync_WithValidToken_ReturnsNewToken()
    {
        // Arrange
        var service = CreateService();
        var createResult = await service.CreateTokenAsync(1);

        // Act
        var rotateResult = await service.RotateTokenAsync(createResult.Token!);

        // Assert
        rotateResult.IsSuccess.Should().BeTrue();
        rotateResult.Token.Should().NotBeNullOrEmpty();
        rotateResult.Token.Should().NotBe(createResult.Token); // New token
        rotateResult.TokenFamily.Should().Be(createResult.TokenFamily); // Same family
    }

    [Fact]
    public async Task RotateTokenAsync_RevokesOldToken()
    {
        // Arrange
        var service = CreateService();
        var createResult = await service.CreateTokenAsync(1);

        // Act
        await service.RotateTokenAsync(createResult.Token!);

        // Assert
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var tokens = await dbContext.RefreshTokens
            .Where(t => t.TokenFamily == createResult.TokenFamily)
            .OrderBy(t => t.IssuedAt)
            .ToListAsync();

        tokens.Should().HaveCount(2);
        tokens[0].RevokedAt.Should().NotBeNull();
        tokens[0].RevokedReason.Should().Be("rotated");
        tokens[1].RevokedAt.Should().BeNull(); // New token is valid
    }

    [Fact]
    public async Task RotateTokenAsync_WithEmptyToken_ReturnsError()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RotateTokenAsync("");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("refresh_token_invalid");
    }

    [Fact]
    public async Task RotateTokenAsync_WithNonExistentToken_ReturnsError()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RotateTokenAsync("non-existent-token");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("refresh_token_invalid");
    }

    [Fact]
    public async Task RotateTokenAsync_WithExpiredToken_ReturnsError()
    {
        // Arrange
        var service = CreateService();
        var createResult = await service.CreateTokenAsync(1);

        // Advance clock past expiration
        _clock.Advance(Duration.FromDays(31));

        // Act
        var result = await service.RotateTokenAsync(createResult.Token!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("refresh_token_invalid");
    }

    #endregion

    #region Replay Detection Tests

    [Fact]
    public async Task RotateTokenAsync_WithRevokedToken_ReturnsReplayError()
    {
        // Arrange
        var service = CreateService();
        var createResult = await service.CreateTokenAsync(1);

        // Use token once (rotate)
        await service.RotateTokenAsync(createResult.Token!);

        // Act - Try to use the old token again (replay attack)
        var replayResult = await service.RotateTokenAsync(createResult.Token!);

        // Assert
        replayResult.IsSuccess.Should().BeFalse();
        replayResult.ErrorCode.Should().Be("refresh_token_replayed");
    }

    [Fact]
    public async Task RotateTokenAsync_WhenReplayDetected_RevokesEntireFamily()
    {
        // Arrange
        var service = CreateService();
        var createResult = await service.CreateTokenAsync(1);
        var rotateResult = await service.RotateTokenAsync(createResult.Token!);

        // Act - Replay attack with original token
        await service.RotateTokenAsync(createResult.Token!);

        // Assert - All tokens in family should be revoked
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var tokens = await dbContext.RefreshTokens
            .Where(t => t.TokenFamily == createResult.TokenFamily)
            .ToListAsync();

        tokens.Should().AllSatisfy(t => t.RevokedAt.Should().NotBeNull());
    }

    #endregion

    #region Max Session Lifetime Tests

    [Fact]
    public async Task RotateTokenAsync_AfterMaxSession_ReturnsError()
    {
        // Arrange
        var service = CreateService();
        var createResult = await service.CreateTokenAsync(1);

        // Advance clock past max session (90 days)
        _clock.Advance(Duration.FromDays(91));

        // Act
        var result = await service.RotateTokenAsync(createResult.Token!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("refresh_token_invalid");
    }

    [Fact]
    public async Task RotateTokenAsync_NearMaxSession_CapsExpiration()
    {
        // Arrange
        var service = CreateService();
        var createResult = await service.CreateTokenAsync(1);

        // Keep rotating to stay within the refresh token lifetime
        // but approach max session
        var currentToken = createResult.Token!;

        // Advance to day 25 and rotate
        _clock.Advance(Duration.FromDays(25));
        var rotate1 = await service.RotateTokenAsync(currentToken);
        rotate1.IsSuccess.Should().BeTrue();
        currentToken = rotate1.Token!;

        // Advance to day 50 and rotate
        _clock.Advance(Duration.FromDays(25));
        var rotate2 = await service.RotateTokenAsync(currentToken);
        rotate2.IsSuccess.Should().BeTrue();
        currentToken = rotate2.Token!;

        // Advance to day 75 and rotate
        _clock.Advance(Duration.FromDays(25));
        var rotate3 = await service.RotateTokenAsync(currentToken);
        rotate3.IsSuccess.Should().BeTrue();

        // Assert - token expiration should be capped at max session (day 90)
        // not 30 days from day 75 (which would be day 105)
        var maxSessionEnd = createResult.ExpiresAt!.Value.Minus(Duration.FromDays(30))
            .Plus(Duration.FromDays(90));
        (rotate3.ExpiresAt!.Value <= maxSessionEnd).Should().BeTrue();
    }

    #endregion

    #region RevokeTokenAsync Tests

    [Fact]
    public async Task RevokeTokenAsync_WithValidToken_Succeeds()
    {
        // Arrange
        var service = CreateService();
        var createResult = await service.CreateTokenAsync(1);

        // Act
        var result = await service.RevokeTokenAsync(createResult.Token!, "test_revoke");

        // Assert
        result.IsSuccess.Should().BeTrue();

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenFamily == createResult.TokenFamily);
        token!.RevokedAt.Should().NotBeNull();
        token.RevokedReason.Should().Be("test_revoke");
    }

    [Fact]
    public async Task RevokeTokenAsync_WithNonExistentToken_ReturnsError()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RevokeTokenAsync("non-existent");

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithAlreadyRevokedToken_Succeeds()
    {
        // Arrange
        var service = CreateService();
        var createResult = await service.CreateTokenAsync(1);
        await service.RevokeTokenAsync(createResult.Token!);

        // Act - Revoke again
        var result = await service.RevokeTokenAsync(createResult.Token!);

        // Assert - Should succeed (idempotent)
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region RevokeAllUserTokensAsync Tests

    [Fact]
    public async Task RevokeAllUserTokensAsync_RevokesAllTokens()
    {
        // Arrange
        var service = CreateService();
        await service.CreateTokenAsync(1);
        await service.CreateTokenAsync(1);
        await service.CreateTokenAsync(1);

        // Act
        var result = await service.RevokeAllUserTokensAsync(1, "logout_all");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(3);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var tokens = await dbContext.RefreshTokens.Where(t => t.UserId == 1).ToListAsync();
        tokens.Should().AllSatisfy(t =>
        {
            t.RevokedAt.Should().NotBeNull();
            t.RevokedReason.Should().Be("logout_all");
        });
    }

    #endregion

    #region RevokeTokenFamilyAsync Tests

    [Fact]
    public async Task RevokeTokenFamilyAsync_RevokesAllInFamily()
    {
        // Arrange
        var service = CreateService();
        var createResult = await service.CreateTokenAsync(1);
        await service.RotateTokenAsync(createResult.Token!);

        // Act
        var result = await service.RevokeTokenFamilyAsync(createResult.TokenFamily!, "admin_revoke");

        // Assert
        result.IsSuccess.Should().BeTrue();

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var tokens = await dbContext.RefreshTokens
            .Where(t => t.TokenFamily == createResult.TokenFamily)
            .ToListAsync();
        tokens.Should().AllSatisfy(t => t.RevokedAt.Should().NotBeNull());
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
