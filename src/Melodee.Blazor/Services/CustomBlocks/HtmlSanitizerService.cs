using Ganss.Xss;

namespace Melodee.Blazor.Services.CustomBlocks;

/// <summary>
/// Service for sanitizing HTML content to prevent XSS attacks.
/// </summary>
public interface IHtmlSanitizerService
{
    /// <summary>
    /// Sanitizes HTML using a strict allow-list of safe tags and attributes.
    /// </summary>
    /// <param name="html">HTML to sanitize</param>
    /// <returns>Sanitized HTML safe for rendering</returns>
    string Sanitize(string html);
}

/// <summary>
/// Default implementation of HTML sanitizer using Ganss.Xss with a strict allow-list.
/// </summary>
public sealed class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizerService()
    {
        _sanitizer = new HtmlSanitizer();

        // Clear default allowed tags and start with strict allow-list
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedCssProperties.Clear();

        // Allow-list safe HTML tags for basic formatting
        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("ol");
        _sanitizer.AllowedTags.Add("li");
        _sanitizer.AllowedTags.Add("h1");
        _sanitizer.AllowedTags.Add("h2");
        _sanitizer.AllowedTags.Add("h3");
        _sanitizer.AllowedTags.Add("h4");
        _sanitizer.AllowedTags.Add("h5");
        _sanitizer.AllowedTags.Add("h6");
        _sanitizer.AllowedTags.Add("blockquote");
        _sanitizer.AllowedTags.Add("code");
        _sanitizer.AllowedTags.Add("pre");
        _sanitizer.AllowedTags.Add("hr");
        _sanitizer.AllowedTags.Add("br");
        _sanitizer.AllowedTags.Add("a");

        // Allow href attribute only on <a> tags with safe protocols
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");

        // Disallow dangerous attributes (on*, style, etc.) - already cleared above
        // Disallow script, iframe, object, embed, style tags - not in allow-list
    }

    public string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return _sanitizer.Sanitize(html);
    }
}
