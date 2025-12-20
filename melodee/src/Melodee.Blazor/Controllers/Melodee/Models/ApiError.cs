namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Standardized error response for the Melodee API.
/// </summary>
/// <param name="Code">Machine-readable error code (e.g., "UNAUTHORIZED", "NOT_FOUND", "VALIDATION_ERROR")</param>
/// <param name="Message">Human-readable error message</param>
/// <param name="CorrelationId">Optional correlation ID for tracing (populated from request context)</param>
public record ApiError(string Code, string Message, string? CorrelationId = null)
{
    public static class Codes
    {
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string NotFound = "NOT_FOUND";
        public const string BadRequest = "BAD_REQUEST";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string TooManyRequests = "TOO_MANY_REQUESTS";
        public const string Blacklisted = "BLACKLISTED";
        public const string UserLocked = "USER_LOCKED";
        public const string InternalError = "INTERNAL_ERROR";

        // Google Auth specific error codes (per WBS Phase 2)
        public const string InvalidGoogleToken = "invalid_google_token";
        public const string ExpiredGoogleToken = "expired_google_token";
        public const string GoogleAccountNotLinked = "google_account_not_linked";
        public const string SignupDisabled = "signup_disabled";
        public const string ForbiddenTenant = "forbidden_tenant";
        public const string AccountDisabled = "account_disabled";
        public const string RefreshTokenReplayed = "refresh_token_replayed";
        public const string RefreshTokenInvalid = "refresh_token_invalid";
        public const string GoogleAlreadyLinked = "google_already_linked";
    }
}
