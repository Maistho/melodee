namespace Melodee.Blazor.Services.CustomBlocks;

/// <summary>
/// Configuration options for the custom blocks feature.
/// </summary>
public sealed class CustomBlocksOptions
{
    public const string SectionName = "CustomBlocks";

    /// <summary>
    /// Whether custom blocks are enabled. Default: true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum file size in bytes for a custom block. Default: 262144 (256KB)
    /// </summary>
    public int MaxBytes { get; set; } = 262144;

    /// <summary>
    /// Cache duration in seconds. Default: 30
    /// </summary>
    public int CacheSeconds { get; set; } = 30;
}
