using Google.Apis.Auth;
using Melodee.Common.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Melodee.Common.Services.Security;

/// <summary>
/// Result of Google token validation.
/// </summary>
public sealed class GoogleTokenValidationResult
{
    /// <summary>
    /// Whether the token validation was successful.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// The validated payload if successful, null otherwise.
    /// </summary>
    public GoogleJsonWebSignature.Payload? Payload { get; init; }

    /// <summary>
    /// Error code if validation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Human-readable error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static GoogleTokenValidationResult Success(GoogleJsonWebSignature.Payload payload) =>
        new() { IsValid = true, Payload = payload };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static GoogleTokenValidationResult Failure(string errorCode, string errorMessage) =>
        new() { IsValid = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}

/// <summary>
/// Service for validating Google ID tokens.
/// </summary>
public interface IGoogleTokenService
{
    /// <summary>
    /// Validates a Google ID token and returns the payload if valid.
    /// </summary>
    /// <param name="idToken">The Google ID token to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with payload or error information.</returns>
    Task<GoogleTokenValidationResult> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of Google ID token validation using Google.Apis.Auth.
/// </summary>
public sealed class GoogleTokenService : IGoogleTokenService
{
    private readonly GoogleAuthOptions _options;
    private readonly ILogger<GoogleTokenService> _logger;

    public GoogleTokenService(
        IOptions<GoogleAuthOptions> options,
        ILogger<GoogleTokenService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<GoogleTokenValidationResult> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            return GoogleTokenValidationResult.Failure("invalid_google_token", "ID token is required.");
        }

        if (!_options.Enabled)
        {
            _logger.LogWarning("Google authentication is disabled but token validation was attempted.");
            return GoogleTokenValidationResult.Failure("invalid_google_token", "Google authentication is not enabled.");
        }

        var validAudiences = _options.GetAllClientIds().ToList();
        if (validAudiences.Count == 0)
        {
            _logger.LogError("No Google client IDs configured.");
            return GoogleTokenValidationResult.Failure("invalid_google_token", "Google authentication is not properly configured.");
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = validAudiences,
                // Clock skew tolerance
                IssuedAtClockTolerance = TimeSpan.FromSeconds(_options.ClockSkewSeconds),
                ExpirationTimeClockTolerance = TimeSpan.FromSeconds(_options.ClockSkewSeconds)
            };

            // Add hosted domain restriction if configured
            if (_options.AllowedHostedDomains.Length > 0)
            {
                settings.HostedDomain = _options.AllowedHostedDomains[0]; // Primary domain
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings).ConfigureAwait(false);

            // Validate hosted domain if multiple are configured
            if (_options.AllowedHostedDomains.Length > 1)
            {
                var hostedDomain = payload.HostedDomain;
                if (!string.IsNullOrEmpty(hostedDomain) &&
                    !_options.AllowedHostedDomains.Contains(hostedDomain, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Google token validation failed: hosted domain {HostedDomain} not in allowed list.",
                        hostedDomain);
                    return GoogleTokenValidationResult.Failure("forbidden_tenant",
                        "Google account's hosted domain is not allowed.");
                }
            }

            // Require email to be verified
            if (!payload.EmailVerified)
            {
                _logger.LogWarning("Google token validation failed: email not verified.");
                return GoogleTokenValidationResult.Failure("invalid_google_token",
                    "Google account email is not verified.");
            }

            // Log success without exposing sensitive data
            _logger.LogInformation(
                "Google token validated successfully for subject {Subject}.",
                payload.Subject);

            return GoogleTokenValidationResult.Success(payload);
        }
        catch (InvalidJwtException ex)
        {
            // Determine specific error type
            var errorCode = "invalid_google_token";
            var errorMessage = "Google ID token validation failed.";

            if (ex.Message.Contains("expired", StringComparison.OrdinalIgnoreCase))
            {
                errorCode = "expired_google_token";
                errorMessage = "Google ID token has expired.";
            }
            else if (ex.Message.Contains("audience", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Google ID token audience mismatch.";
            }
            else if (ex.Message.Contains("issuer", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Google ID token issuer invalid.";
            }

            _logger.LogWarning(
                "Google token validation failed: {ErrorMessage}",
                errorMessage);

            return GoogleTokenValidationResult.Failure(errorCode, errorMessage);
        }
        catch (Exception ex)
        {
            // Log error without exposing token content
            _logger.LogError(ex, "Unexpected error during Google token validation.");
            return GoogleTokenValidationResult.Failure("invalid_google_token",
                "An error occurred while validating the Google ID token.");
        }
    }
}
