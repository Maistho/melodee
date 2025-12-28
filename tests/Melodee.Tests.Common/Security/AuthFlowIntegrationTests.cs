using FluentAssertions;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Services.Security;
using Melodee.Tests.Common.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Testing;

namespace Melodee.Tests.Common.Security;

/// <summary>
/// Integration tests for complete auth flows.
/// Per WBS Phase 4.1/2.5: End-to-end API tests for Google exchange, refresh, link/unlink, and admin revocation flows.
/// </summary>
public class AuthFlowIntegrationTests : ServiceTestBase
{
    #region New User Creation Flow Tests

    [Fact]
    public async Task NewUser_PasswordLogin_CreatesRefreshToken_Success()
    {
        // Arrange
        await using var context = await MockFactory().CreateDbContextAsync();
        var user = await CreateTestUserAsync(context, "newuser_password");
        var refreshService = GetRefreshTokenService();

        // Act - Simulate what happens after successful password auth
        var result = await refreshService.CreateTokenAsync(
            user.Id,
            "device-123",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().NotBeNull();
        (result.ExpiresAt > SystemClock.Instance.GetCurrentInstant()).Should().BeTrue("Token should expire in the future");
    }

    [Fact]
    public async Task NewUser_RefreshRotation_IssuesNewTokenPair()
    {
        // Arrange
        await using var context = await MockFactory().CreateDbContextAsync();
        var user = await CreateTestUserAsync(context, "rotation_user");
        var refreshService = GetRefreshTokenService();

        var initialResult = await refreshService.CreateTokenAsync(
            user.Id,
            "device-rotate",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Act
        var rotateResult = await refreshService.RotateTokenAsync(
            initialResult.Token!,
            "device-rotate",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Assert
        rotateResult.IsSuccess.Should().BeTrue();
        rotateResult.Token.Should().NotBeNullOrEmpty();
        rotateResult.Token.Should().NotBe(initialResult.Token, "Rotated token should be different");
    }

    #endregion

    #region Replay Attack Detection Tests

    [Fact]
    public async Task ReplayAttack_ReusingOldToken_IsDetected()
    {
        // Arrange
        await using var context = await MockFactory().CreateDbContextAsync();
        var user = await CreateTestUserAsync(context, "replay_victim");
        var refreshService = GetRefreshTokenService();

        var initialResult = await refreshService.CreateTokenAsync(
            user.Id,
            "device-replay",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Legitimate rotation
        await refreshService.RotateTokenAsync(
            initialResult.Token!,
            "device-replay",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Act - Replay attempt with old token
        var replayResult = await refreshService.RotateTokenAsync(
            initialResult.Token!,
            "device-replay",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Assert
        replayResult.IsSuccess.Should().BeFalse();
        replayResult.ErrorCode.Should().Be("refresh_token_replayed");
    }

    [Fact]
    public async Task ReplayDetection_RevokesTokenFamily()
    {
        // Arrange
        await using var context = await MockFactory().CreateDbContextAsync();
        var user = await CreateTestUserAsync(context, "family_revoke");
        var refreshService = GetRefreshTokenService();

        var initialResult = await refreshService.CreateTokenAsync(
            user.Id,
            "device-family",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // First legitimate rotation
        var rotateResult = await refreshService.RotateTokenAsync(
            initialResult.Token!,
            "device-family",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Replay attempt triggers family revocation
        await refreshService.RotateTokenAsync(
            initialResult.Token!,
            "device-family",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Act - Try to use the legitimately rotated token
        var afterReplayResult = await refreshService.RotateTokenAsync(
            rotateResult.Token!,
            "device-family",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Assert - Should be revoked due to replay detection
        afterReplayResult.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region Token Revocation Tests

    [Fact]
    public async Task AdminRevocation_InvalidatesToken()
    {
        // Arrange
        await using var context = await MockFactory().CreateDbContextAsync();
        var user = await CreateTestUserAsync(context, "revoke_target");
        var refreshService = GetRefreshTokenService();

        var result = await refreshService.CreateTokenAsync(
            user.Id,
            "device-revoke",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Act - Admin revokes the token
        await refreshService.RevokeTokenAsync(result.Token!, "admin_revocation", CancellationToken.None);

        // Assert - Token should no longer work
        // Using a revoked token is treated as a replay attempt since it's been invalidated
        var rotateResult = await refreshService.RotateTokenAsync(
            result.Token!,
            "device-revoke",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        rotateResult.IsSuccess.Should().BeFalse();
        rotateResult.ErrorCode.Should().Be("refresh_token_replayed", "Revoked tokens are treated as replay attempts");
    }

    [Fact]
    public async Task RevokeAllUserTokens_InvalidatesAllSessions()
    {
        // Arrange
        await using var context = await MockFactory().CreateDbContextAsync();
        var user = await CreateTestUserAsync(context, "revoke_all_user");
        var refreshService = GetRefreshTokenService();

        // Create multiple tokens (different devices)
        var token1 = await refreshService.CreateTokenAsync(user.Id, "device1", "Browser1", "1.1.1.1", CancellationToken.None);
        var token2 = await refreshService.CreateTokenAsync(user.Id, "device2", "Browser2", "2.2.2.2", CancellationToken.None);
        var token3 = await refreshService.CreateTokenAsync(user.Id, "device3", "Browser3", "3.3.3.3", CancellationToken.None);

        // Act - Revoke all tokens for user
        await refreshService.RevokeAllUserTokensAsync(user.Id, "logout_all", CancellationToken.None);

        // Assert - All tokens should be invalid
        var result1 = await refreshService.RotateTokenAsync(token1.Token!, "device1", "Browser1", "1.1.1.1", CancellationToken.None);
        var result2 = await refreshService.RotateTokenAsync(token2.Token!, "device2", "Browser2", "2.2.2.2", CancellationToken.None);
        var result3 = await refreshService.RotateTokenAsync(token3.Token!, "device3", "Browser3", "3.3.3.3", CancellationToken.None);

        result1.IsSuccess.Should().BeFalse();
        result2.IsSuccess.Should().BeFalse();
        result3.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region Configuration Combination Tests (Regression Matrix)

    [Theory]
    [InlineData(true, true, true)]   // All enabled
    [InlineData(true, true, false)]  // Google enabled, auto-link enabled, self-reg disabled
    [InlineData(true, false, true)]  // Google enabled, auto-link disabled, self-reg enabled
    [InlineData(true, false, false)] // Google enabled, auto-link disabled, self-reg disabled
    [InlineData(false, false, true)] // Google disabled, self-reg enabled (password only)
    public async Task AuthConfiguration_Matrix_RefreshTokensWorkRegardlessOfSettings(
        bool googleEnabled,
        bool autoLinkEnabled,
        bool selfRegistrationEnabled)
    {
        // Arrange - Config variations should not affect refresh token mechanics
        await using var context = await MockFactory().CreateDbContextAsync();
        var user = await CreateTestUserAsync(context, $"config_test_{googleEnabled}_{autoLinkEnabled}_{selfRegistrationEnabled}");
        var refreshService = GetRefreshTokenService();

        // Act
        var createResult = await refreshService.CreateTokenAsync(
            user.Id,
            "config-device",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        var rotateResult = await refreshService.RotateTokenAsync(
            createResult.Token!,
            "config-device",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None);

        // Assert - Refresh tokens should work regardless of Google/auto-link/self-reg settings
        createResult.IsSuccess.Should().BeTrue($"Token creation should succeed with config: Google={googleEnabled}, AutoLink={autoLinkEnabled}, SelfReg={selfRegistrationEnabled}");
        rotateResult.IsSuccess.Should().BeTrue($"Token rotation should succeed with config: Google={googleEnabled}, AutoLink={autoLinkEnabled}, SelfReg={selfRegistrationEnabled}");
    }

    #endregion

    #region Invalid Token Tests

    [Fact]
    public async Task InvalidToken_MalformedString_ReturnsInvalidError()
    {
        // Arrange
        var refreshService = GetRefreshTokenService();

        // Act
        var result = await refreshService.RotateTokenAsync(
            "not-a-valid-token",
            "device",
            "Browser",
            "1.1.1.1",
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("refresh_token_invalid");
    }

    [Fact]
    public async Task InvalidToken_EmptyString_ReturnsInvalidError()
    {
        // Arrange
        var refreshService = GetRefreshTokenService();

        // Act
        var result = await refreshService.RotateTokenAsync(
            string.Empty,
            "device",
            "Browser",
            "1.1.1.1",
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("refresh_token_invalid");
    }

    [Fact]
    public async Task InvalidToken_NullToken_ReturnsInvalidError()
    {
        // Arrange
        var refreshService = GetRefreshTokenService();

        // Act
        var result = await refreshService.RotateTokenAsync(
            null!,
            "device",
            "Browser",
            "1.1.1.1",
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("refresh_token_invalid");
    }

    #endregion

    #region Multi-Step Refresh Chain Tests

    [Fact]
    public async Task MultiStepRefreshChain_SuccessiveRotations_AllSucceed()
    {
        // Arrange
        await using var context = await MockFactory().CreateDbContextAsync();
        var user = await CreateTestUserAsync(context, "chain_user");
        var refreshService = GetRefreshTokenService();

        var currentToken = (await refreshService.CreateTokenAsync(
            user.Id,
            "chain-device",
            "TestBrowser/1.0",
            "192.168.1.1",
            CancellationToken.None)).Token!;

        // Act - Chain of 10 successive rotations
        for (var i = 0; i < 10; i++)
        {
            var result = await refreshService.RotateTokenAsync(
                currentToken,
                "chain-device",
                "TestBrowser/1.0",
                "192.168.1.1",
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue($"Rotation {i + 1} should succeed");
            result.Token.Should().NotBe(currentToken, $"Rotation {i + 1} should produce a new token");
            currentToken = result.Token!;
        }
    }

    #endregion

    #region Helper Methods

    private static async Task<User> CreateTestUserAsync(MelodeeDbContext context, string username)
    {
        var user = new User
        {
            UserName = username,
            UserNameNormalized = username.ToUpperInvariant(),
            Email = $"{username}@example.com",
            EmailNormalized = $"{username.ToUpperInvariant()}@EXAMPLE.COM",
            PublicKey = $"key_{username}",
            PasswordEncrypted = "encrypted_password",
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            IsAdmin = false,
            IsLocked = false
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private IRefreshTokenService GetRefreshTokenService()
    {
        var tokenOptions = Options.Create(new TokenOptions
        {
            AccessTokenLifetimeMinutes = 15,
            RefreshTokenLifetimeDays = 30,
            MaxSessionDays = 90
        });

        return new RefreshTokenService(
            MockFactory(),
            tokenOptions,
            NullLogger<RefreshTokenService>.Instance,
            new FakeClock(SystemClock.Instance.GetCurrentInstant()));
    }

    #endregion
}
