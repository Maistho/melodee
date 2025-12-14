namespace Melodee.Common.Plugins.SearchEngine.MetalApi;

/// <summary>
///     Configuration options for Metal API integration
/// </summary>
public sealed class MetalApiOptions
{
    /// <summary>
    ///     Base URL for the Metal API
    /// </summary>
    public string BaseUrl { get; set; } = "https://www.metal-api.dev";

    /// <summary>
    ///     Whether the Metal API integration is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Timeout for HTTP requests to Metal API
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
