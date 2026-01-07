using Markdig;

namespace Melodee.Blazor.Services.CustomBlocks;

/// <summary>
/// Service for rendering Markdown to HTML.
/// HTML elements in markdown are allowed and will be passed through,
/// but must be sanitized separately using HtmlSanitizerService.
/// </summary>
public interface IMarkdownRenderer
{
    /// <summary>
    /// Renders Markdown to HTML. Raw HTML in the markdown will be preserved.
    /// </summary>
    /// <param name="markdown">Markdown source</param>
    /// <returns>HTML output (unsanitized - must be sanitized separately with HtmlSanitizerService)</returns>
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
            .UseSoftlineBreakAsHardlineBreak()
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
