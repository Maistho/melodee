using Microsoft.AspNetCore.Components;

namespace Melodee.Blazor.Services.CustomBlocks;

/// <summary>
/// Result of fetching a custom block.
/// </summary>
public sealed record CustomBlockResult
{
    /// <summary>
    /// Whether the block was found.
    /// </summary>
    public bool Found { get; init; }

    /// <summary>
    /// The sanitized HTML content as a MarkupString, ready for rendering.
    /// </summary>
    public MarkupString Content { get; init; }

    /// <summary>
    /// The key that was requested.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    public static CustomBlockResult NotFound(string key) => new()
    {
        Found = false,
        Key = key,
        Content = new MarkupString(string.Empty)
    };

    public static CustomBlockResult Success(string key, string sanitizedHtml) => new()
    {
        Found = true,
        Key = key,
        Content = new MarkupString(sanitizedHtml)
    };
}
