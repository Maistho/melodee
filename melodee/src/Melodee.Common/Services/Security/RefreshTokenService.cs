using System.Security.Cryptography;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Melodee.Common.Services.Security;

/// <summary>
/// Result of refresh token operations.
/// </summary>
public sealed class RefreshTokenResult
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The new refresh token (raw value) if successful. Only available on creation/rotation.
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// The token family ID for the session.
    /// </summary>
    public string? TokenFamily { get; init; }

    /// <summary>
    /// When the token expires.
    /// </summary>
    public Instant? ExpiresAt { get; init; }

    /// <summary>
    /// The user ID associated with the token.
    /// </summary>
    public int? UserId { get; init; }

    /// <summary>
    /// Error code if operation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Human-readable error message if operation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful result with a new token.
    /// </summary>
    public static RefreshTokenResult Success(string token, string tokenFamily, Instant expiresAt, int userId) =>
        new() { IsSuccess = true, Token = token, TokenFamily = tokenFamily, ExpiresAt = expiresAt, UserId = userId };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static RefreshTokenResult Failure(string errorCode, string errorMessage) =>
        new() { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}

/// <summary>
/// Service for managing refresh tokens with rotation support.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Creates a new refresh token for a user.
    /// </summary>
    Task<RefreshTokenResult> CreateTokenAsync(
        int userId,
        string? deviceId = null,
        string? userAgent = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and rotates a refresh token. Returns a new token pair if valid.
    /// </summary>
    Task<RefreshTokenResult> RotateTokenAsync(
        string refreshToken,
        string? deviceId = null,
        string? userAgent = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    Task<OperationResult<bool>> RevokeTokenAsync(
        string refreshToken,
        string reason = "manual_revoke",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    Task<OperationResult<int>> RevokeAllUserTokensAsync(
        int userId,
        string reason = "logout_all",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all tokens in a token family (useful for replay detection).
    /// </summary>
    Task<OperationResult<int>> RevokeTokenFamilyAsync(
        string tokenFamily,
        string reason = "replay_detected",
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of refresh token management with rotation and replay detection.
/// </summary>
public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IDbContextFactory<MelodeeDbContext> _dbContextFactory;
    private readonly TokenOptions _tokenOptions;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly IClock _clock;

    public RefreshTokenService(
        IDbContextFactory<MelodeeDbContext> dbContextFactory,
        IOptions<TokenOptions> tokenOptions,
        ILogger<RefreshTokenService> logger,
        IClock clock)
    {
        _dbContextFactory = dbContextFactory;
        _tokenOptions = tokenOptions.Value;
        _logger = logger;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<RefreshTokenResult> CreateTokenAsync(
        int userId,
        string? deviceId = null,
        string? userAgent = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var now = _clock.GetCurrentInstant();
        var rawToken = GenerateSecureToken();
        var hashedToken = HashToken(rawToken);
        var tokenFamily = Guid.NewGuid().ToString("N");
        var expiresAt = now.Plus(Duration.FromDays(_tokenOptions.RefreshTokenLifetimeDays));

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            HashedToken = hashedToken,
            TokenFamily = tokenFamily,
            IssuedAt = now,
            ExpiresAt = expiresAt,
            SessionStartedAt = now,
            DeviceId = deviceId,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            CreatedAt = now
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Created refresh token for user {UserId}, family {TokenFamily}.",
            userId, tokenFamily);

        return RefreshTokenResult.Success(rawToken, tokenFamily, expiresAt, userId);
    }

    /// <inheritdoc />
    public async Task<RefreshTokenResult> RotateTokenAsync(
        string refreshToken,
        string? deviceId = null,
        string? userAgent = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return RefreshTokenResult.Failure("refresh_token_invalid", "Refresh token is required.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var hashedToken = HashToken(refreshToken);
        var now = _clock.GetCurrentInstant();

        var existingToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.HashedToken == hashedToken, cancellationToken)
            .ConfigureAwait(false);

        if (existingToken == null)
        {
            _logger.LogWarning("Refresh token not found.");
            return RefreshTokenResult.Failure("refresh_token_invalid", "Refresh token is invalid.");
        }

        // Check if token is revoked (potential replay attack)
        if (existingToken.IsRevoked)
        {
            _logger.LogWarning(
                "Replay detected: revoked token used for user {UserId}, family {TokenFamily}.",
                existingToken.UserId, existingToken.TokenFamily);

            // Revoke entire token family if replay detection is enabled
            if (_tokenOptions.RevokeOnReplay)
            {
                await RevokeTokenFamilyInternalAsync(dbContext, existingToken.TokenFamily, "replay_detected", now, cancellationToken)
                    .ConfigureAwait(false);
            }

            return RefreshTokenResult.Failure("refresh_token_replayed",
                "Refresh token has been revoked. Please re-authenticate.");
        }

        // Check if token is expired
        if (existingToken.ExpiresAt <= now)
        {
            _logger.LogWarning(
                "Expired token used for user {UserId}, family {TokenFamily}.",
                existingToken.UserId, existingToken.TokenFamily);
            return RefreshTokenResult.Failure("refresh_token_invalid", "Refresh token has expired.");
        }

        // Check absolute session lifetime
        var maxSessionEnd = existingToken.SessionStartedAt.Plus(Duration.FromDays(_tokenOptions.MaxSessionDays));
        if (now >= maxSessionEnd)
        {
            _logger.LogInformation(
                "Session max lifetime reached for user {UserId}, family {TokenFamily}.",
                existingToken.UserId, existingToken.TokenFamily);

            // Revoke the token
            existingToken.RevokedAt = now;
            existingToken.RevokedReason = "max_session_reached";
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return RefreshTokenResult.Failure("refresh_token_invalid",
                "Session has expired. Please re-authenticate.");
        }

        // Generate new token
        var newRawToken = GenerateSecureToken();
        var newHashedToken = HashToken(newRawToken);
        var newExpiresAt = now.Plus(Duration.FromDays(_tokenOptions.RefreshTokenLifetimeDays));

        // Cap expiration at max session end
        if (newExpiresAt > maxSessionEnd)
        {
            newExpiresAt = maxSessionEnd;
        }

        // Revoke old token (rotation)
        existingToken.RevokedAt = now;
        existingToken.RevokedReason = "rotated";
        existingToken.ReplacedByToken = newHashedToken;

        // Create new token
        var newToken = new RefreshToken
        {
            UserId = existingToken.UserId,
            HashedToken = newHashedToken,
            TokenFamily = existingToken.TokenFamily,
            IssuedAt = now,
            ExpiresAt = newExpiresAt,
            SessionStartedAt = existingToken.SessionStartedAt,
            DeviceId = deviceId ?? existingToken.DeviceId,
            UserAgent = userAgent ?? existingToken.UserAgent,
            IpAddress = ipAddress ?? existingToken.IpAddress,
            CreatedAt = now
        };

        dbContext.RefreshTokens.Add(newToken);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Rotated refresh token for user {UserId}, family {TokenFamily}.",
            existingToken.UserId, existingToken.TokenFamily);

        return RefreshTokenResult.Success(newRawToken, existingToken.TokenFamily, newExpiresAt, existingToken.UserId);
    }

    /// <inheritdoc />
    public async Task<OperationResult<bool>> RevokeTokenAsync(
        string refreshToken,
        string reason = "manual_revoke",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return new OperationResult<bool>(["Refresh token is required."]) { Data = false };
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var hashedToken = HashToken(refreshToken);
        var now = _clock.GetCurrentInstant();

        var existingToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.HashedToken == hashedToken, cancellationToken)
            .ConfigureAwait(false);

        if (existingToken == null)
        {
            return new OperationResult<bool>(["Refresh token not found."]) { Data = false };
        }

        if (existingToken.IsRevoked)
        {
            return new OperationResult<bool> { Data = true }; // Already revoked
        }

        existingToken.RevokedAt = now;
        existingToken.RevokedReason = reason;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Revoked refresh token for user {UserId}, reason: {Reason}.",
            existingToken.UserId, reason);

        return new OperationResult<bool> { Data = true };
    }

    /// <inheritdoc />
    public async Task<OperationResult<int>> RevokeAllUserTokensAsync(
        int userId,
        string reason = "logout_all",
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var now = _clock.GetCurrentInstant();

        var tokensToRevoke = await dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var token in tokensToRevoke)
        {
            token.RevokedAt = now;
            token.RevokedReason = reason;
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Revoked {Count} refresh tokens for user {UserId}, reason: {Reason}.",
            tokensToRevoke.Count, userId, reason);

        return new OperationResult<int> { Data = tokensToRevoke.Count };
    }

    /// <inheritdoc />
    public async Task<OperationResult<int>> RevokeTokenFamilyAsync(
        string tokenFamily,
        string reason = "replay_detected",
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var now = _clock.GetCurrentInstant();
        var count = await RevokeTokenFamilyInternalAsync(dbContext, tokenFamily, reason, now, cancellationToken)
            .ConfigureAwait(false);
        return new OperationResult<int> { Data = count };
    }

    private async Task<int> RevokeTokenFamilyInternalAsync(
        MelodeeDbContext dbContext,
        string tokenFamily,
        string reason,
        Instant now,
        CancellationToken cancellationToken)
    {
        var tokensToRevoke = await dbContext.RefreshTokens
            .Where(t => t.TokenFamily == tokenFamily && t.RevokedAt == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var token in tokensToRevoke)
        {
            token.RevokedAt = now;
            token.RevokedReason = reason;
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogWarning(
            "Revoked {Count} tokens in family {TokenFamily}, reason: {Reason}.",
            tokensToRevoke.Count, tokenFamily, reason);

        return tokensToRevoke.Count;
    }

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Hashes a token using SHA-256.
    /// </summary>
    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
