using Markdig;

namespace Melodee.Blazor.Services.CustomBlocks;

/// <summary>
/// Service for rendering Markdown to HTML with security constraints.
/// </summary>
public interface IMarkdownRenderer
{
    /// <summary>
    /// Renders Markdown to HTML with raw HTML disabled.
    /// </summary>
    /// <param name="markdown">Markdown source</param>
    /// <returns>HTML output (unsanitized - must be sanitized separately)</returns>
    string RenderToHtml(string markdown);
}

/// <summary>
/// Default implementation of Markdown renderer using Markdig with security-focused configuration.
/// </summary>
public sealed class MarkdownRenderer : IMarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownRenderer()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .DisableHtml() // Critical: Prevent raw HTML from being passed through
            .Build();
    }

    public string RenderToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        return Markdown.ToHtml(markdown, _pipeline);
    }
}
