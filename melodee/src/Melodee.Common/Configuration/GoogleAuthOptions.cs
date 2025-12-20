using System.ComponentModel.DataAnnotations;

namespace Melodee.Common.Configuration;

/// <summary>
/// Configuration options for Google authentication.
/// </summary>
public class GoogleAuthOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Auth:Google";

    /// <summary>
    /// Whether Google authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Google OAuth client ID used by this server.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Additional client IDs to accept (e.g., for Android, iOS, web clients).
    /// </summary>
    public string[] AdditionalClientIds { get; set; } = [];

    /// <summary>
    /// List of allowed hosted domains (hd claim). Empty means no restriction.
    /// </summary>
    public string[] AllowedHostedDomains { get; set; } = [];

    /// <summary>
    /// Whether to automatically link Google accounts to existing accounts by email.
    /// Default is false for security (requires manual linking).
    /// </summary>
    public bool AutoLinkEnabled { get; set; }

    /// <summary>
    /// Clock skew tolerance in seconds for token validation.
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 300;

    /// <summary>
    /// Returns all valid client IDs (primary + additional).
    /// </summary>
    public IEnumerable<string> GetAllClientIds()
    {
        if (!string.IsNullOrEmpty(ClientId))
        {
            yield return ClientId;
        }

        foreach (var id in AdditionalClientIds)
        {
            if (!string.IsNullOrEmpty(id))
            {
                yield return id;
            }
        }
    }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public IEnumerable<ValidationResult> Validate()
    {
        if (Enabled && string.IsNullOrWhiteSpace(ClientId) && AdditionalClientIds.Length == 0)
        {
            yield return new ValidationResult(
                "At least one ClientId must be configured when Google auth is enabled.",
                [nameof(ClientId)]);
        }
    }
}
