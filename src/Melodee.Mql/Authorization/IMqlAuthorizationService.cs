namespace Melodee.Mql.Authorization;

/// <summary>
/// Represents the result of an authorization check.
/// </summary>
public sealed record AuthorizationResult
{
    public bool IsAuthorized { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public string? FieldName { get; init; }
    public List<string> BlockedFields { get; init; } = new();

    public static AuthorizationResult Success() => new() { IsAuthorized = true };

    public static AuthorizationResult ForbiddenField(string fieldName, string reason)
    {
        return new AuthorizationResult
        {
            IsAuthorized = false,
            ErrorCode = "MQL_FORBIDDEN_FIELD",
            ErrorMessage = reason,
            FieldName = fieldName
        };
    }

    public static AuthorizationResult ForbiddenUserData(string targetUserId, string reason)
    {
        return new AuthorizationResult
        {
            IsAuthorized = false,
            ErrorCode = "MQL_FORBIDDEN_USER_DATA",
            ErrorMessage = reason,
            AdditionalData = new Dictionary<string, string>
            {
                ["TargetUserId"] = targetUserId
            }
        };
    }

    public static AuthorizationResult ForbiddenAnonymousAccess(string fieldName)
    {
        return new AuthorizationResult
        {
            IsAuthorized = false,
            ErrorCode = "MQL_FORBIDDEN_FIELD",
            ErrorMessage = $"Field '{fieldName}' requires authentication. Anonymous access is not permitted.",
            FieldName = fieldName
        };
    }

    public static AuthorizationResult CrossUserQueryBlocked(string fieldName, string currentUserId, string? targetUserId)
    {
        return new AuthorizationResult
        {
            IsAuthorized = false,
            ErrorCode = "MQL_FORBIDDEN_USER_DATA",
            ErrorMessage = targetUserId != null
                ? $"Cannot query {fieldName} for user {targetUserId}. You can only query your own data."
                : $"Cannot query {fieldName} for other users.",
            FieldName = fieldName,
            AdditionalData = new Dictionary<string, string>
            {
                ["CurrentUserId"] = currentUserId,
                ["TargetUserId"] = targetUserId ?? "unknown"
            }
        };
    }

    public Dictionary<string, string>? AdditionalData { get; init; }
}

/// <summary>
/// Represents an authorization requirement for a specific field.
/// </summary>
public sealed record FieldAuthorizationRequirement
{
    public string FieldName { get; init; } = string.Empty;
    public bool RequiresAuthentication { get; init; }
    public bool RequiresOwner { get; init; }
    public string[] AllowedUserIds { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Interface for authorization service for MQL queries.
/// </summary>
public interface IMqlAuthorizationService
{
    /// <summary>
    /// Authorizes a query against the current user context.
    /// </summary>
    AuthorizationResult AuthorizeQuery(
        string query,
        string entityType,
        int? currentUserId,
        int? targetUserId = null);

    /// <summary>
    /// Checks if a specific field is user-scoped.
    /// </summary>
    bool IsUserScopedField(string fieldName, string entityType);

    /// <summary>
    /// Gets all user-scoped fields for a given entity type.
    /// </summary>
    IEnumerable<string> GetUserScopedFields(string entityType);

    /// <summary>
    /// Validates that a user can access a specific field.
    /// </summary>
    AuthorizationResult AuthorizeFieldAccess(
        string fieldName,
        string entityType,
        int? currentUserId,
        int? targetUserId = null);

    /// <summary>
    /// Checks if a query contains only fields the user is authorized to access.
    /// </summary>
    AuthorizationResult AuthorizeQueryFields(
        IEnumerable<string> fieldNames,
        string entityType,
        int? currentUserId,
        int? targetUserId = null);
}
