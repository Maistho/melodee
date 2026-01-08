namespace Melodee.Blazor.Controllers.Melodee.Models.ArtistLookup;

/// <summary>
/// Request DTO for artist lookup via third-party search providers.
/// </summary>
public sealed record ArtistLookupRequest
{
    private const int MaxNameLength = 200;
    private const int MinLimit = 1;
    private const int MaxLimit = 50;
    private const int DefaultLimit = 10;
    private const int MaxProviderIds = 20;

    /// <summary>
    /// Name of the artist to search for.
    /// </summary>
    public required string ArtistName { get; init; }

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int Limit { get; init; } = DefaultLimit;

    /// <summary>
    /// Optional filter to restrict search to specific providers.
    /// Only providers that are both in this list AND enabled will be queried.
    /// </summary>
    public string[]? ProviderIds { get; init; }

    public bool Validate(out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(ArtistName))
        {
            errorMessage = "Artist name is required.";
            return false;
        }

        if (ArtistName.Length > MaxNameLength)
        {
            errorMessage = $"Artist name must not exceed {MaxNameLength} characters.";
            return false;
        }

        if (Limit < MinLimit || Limit > MaxLimit)
        {
            errorMessage = $"Limit must be between {MinLimit} and {MaxLimit}.";
            return false;
        }

        if (ProviderIds != null && ProviderIds.Length > MaxProviderIds)
        {
            errorMessage = $"ProviderIds must not exceed {MaxProviderIds} items.";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
