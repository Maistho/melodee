namespace Melodee.Mql.Authorization;

/// <summary>
/// Default implementation of MQL authorization service.
/// Enforces rules for user-scoped field access and cross-user query prevention.
/// </summary>
public sealed class MqlAuthorizationService : IMqlAuthorizationService
{
    private static readonly HashSet<string> UserScopedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        // Songs user-scoped fields
        "songs:rating",
        "songs:plays",
        "songs:starred",
        "songs:starredat",
        "songs:lastplayedat",

        // Albums user-scoped fields
        "albums:rating",
        "albums:plays",
        "albums:starred",
        "albums:starredat",
        "albums:lastplayedat",

        // Artists user-scoped fields
        "artists:rating",
        "artists:starred",
        "artists:starredat"
    };

    private static readonly HashSet<string> ForbiddenFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "userid",
        "user_id",
        "user.id",
        "users.id",
        "apikey",
        "api_key",
        "password",
        "secret",
        "token"
    };

    public AuthorizationResult AuthorizeQuery(
        string query,
        string entityType,
        int? currentUserId,
        int? targetUserId = null)
    {
        if (string.IsNullOrEmpty(query))
        {
            return AuthorizationResult.Success();
        }

        return AuthorizeQueryFields(
            ExtractFieldNamesFromQuery(query),
            entityType,
            currentUserId,
            targetUserId);
    }

    public bool IsUserScopedField(string fieldName, string entityType)
    {
        return UserScopedFields.Contains($"{entityType}:{fieldName}");
    }

    public IEnumerable<string> GetUserScopedFields(string entityType)
    {
        var prefix = $"{entityType}:";
        return UserScopedFields
            .Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(x => x[(prefix.Length)..]);
    }

    public AuthorizationResult AuthorizeFieldAccess(
        string fieldName,
        string entityType,
        int? currentUserId,
        int? targetUserId = null)
    {
        var normalizedField = fieldName.ToLowerInvariant();
        var normalizedEntity = entityType.ToLowerInvariant();

        if (IsForbiddenField(normalizedField))
        {
            return AuthorizationResult.ForbiddenField(fieldName, $"Field '{fieldName}' is not accessible.");
        }

        if (!IsUserScopedField(normalizedField, normalizedEntity))
        {
            return AuthorizationResult.Success();
        }

        if (currentUserId == null)
        {
            return AuthorizationResult.ForbiddenAnonymousAccess(fieldName);
        }

        if (targetUserId != null && targetUserId != currentUserId)
        {
            return AuthorizationResult.CrossUserQueryBlocked(fieldName, currentUserId.Value.ToString(), targetUserId.ToString());
        }

        return AuthorizationResult.Success();
    }

    public AuthorizationResult AuthorizeQueryFields(
        IEnumerable<string> fieldNames,
        string entityType,
        int? currentUserId,
        int? targetUserId = null)
    {
        var normalizedEntity = entityType.ToLowerInvariant();
        var blockedFields = new List<string>();

        foreach (var fieldName in fieldNames)
        {
            var normalizedField = fieldName.ToLowerInvariant();

            if (IsForbiddenField(normalizedField))
            {
                blockedFields.Add(fieldName);
                continue;
            }

            if (!IsUserScopedField(normalizedField, normalizedEntity))
            {
                continue;
            }

            if (currentUserId == null)
            {
                blockedFields.Add(fieldName);
                continue;
            }

            if (targetUserId != null && targetUserId != currentUserId)
            {
                blockedFields.Add(fieldName);
            }
        }

        if (blockedFields.Count == 0)
        {
            return AuthorizationResult.Success();
        }

        if (blockedFields.Count == 1)
        {
            var field = blockedFields[0];

            if (currentUserId == null)
            {
                return AuthorizationResult.ForbiddenAnonymousAccess(field);
            }

            if (targetUserId != null && targetUserId != currentUserId)
            {
                return AuthorizationResult.CrossUserQueryBlocked(field, currentUserId.Value.ToString(), targetUserId.ToString());
            }
        }

        return new AuthorizationResult
        {
            IsAuthorized = false,
            ErrorCode = "MQL_FORBIDDEN_FIELD",
            ErrorMessage = $"Query contains {blockedFields.Count} unauthorized field(s): {string.Join(", ", blockedFields)}",
            BlockedFields = blockedFields
        };
    }

    private static bool IsForbiddenField(string fieldName)
    {
        return ForbiddenFields.Contains(fieldName);
    }

    private static IEnumerable<string> ExtractFieldNamesFromQuery(string query)
    {
        var fieldNames = new HashSet<string>();

        var lowerQuery = query.ToLowerInvariant();
        var words = lowerQuery.Split(new[] { ' ', '(', ')', '\t', '\n', '\r', '[', ']', '{', '}', '+', '-', '*', '/', '<', '>', '=', '!', '&', '|', '^', '~', '?' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (word.Contains(':'))
            {
                var parts = word.Split(':');
                var fieldPart = parts[0].Trim();
                if (!string.IsNullOrEmpty(fieldPart) && char.IsLetter(fieldPart[0]) && fieldPart.All(c => char.IsLetterOrDigit(c)))
                {
                    fieldNames.Add(fieldPart);
                }
            }
            else if (word.StartsWith("not:"))
            {
                var fieldPart = word[4..].Trim();
                if (!string.IsNullOrEmpty(fieldPart) && char.IsLetter(fieldPart[0]) && fieldPart.All(c => char.IsLetterOrDigit(c)))
                {
                    fieldNames.Add(fieldPart);
                }
            }
        }

        var alternativeNotPattern = new System.Text.RegularExpressions.Regex(
            @"\bnot\s*:\s*([a-zA-Z][a-zA-Z0-9]*)",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        foreach (System.Text.RegularExpressions.Match match in alternativeNotPattern.Matches(query))
        {
            if (match.Success && match.Groups.Count > 1)
            {
                fieldNames.Add(match.Groups[1].Value);
            }
        }

        return fieldNames;
    }
}
