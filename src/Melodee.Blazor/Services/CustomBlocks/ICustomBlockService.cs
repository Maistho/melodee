namespace Melodee.Blazor.Services.CustomBlocks;

/// <summary>
/// Service for loading and rendering custom blocks.
/// </summary>
public interface ICustomBlockService
{
    /// <summary>
    /// Get a custom block by key.
    /// </summary>
    /// <param name="key">Block key (e.g., "login.top")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CustomBlockResult with Found=false if block doesn't exist or feature is disabled</returns>
    Task<CustomBlockResult> GetAsync(string key, CancellationToken cancellationToken = default);
}
