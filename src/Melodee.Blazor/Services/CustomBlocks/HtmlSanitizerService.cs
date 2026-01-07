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
        _sanitizer.AllowedTags.Add("div");
        _sanitizer.AllowedTags.Add("span");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("b");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("i");
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

        // Allow common styling attributes
        _sanitizer.AllowedAttributes.Add("class");
        _sanitizer.AllowedAttributes.Add("id");
        _sanitizer.AllowedAttributes.Add("style");

        // Allow href attribute only on <a> tags with safe protocols
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");

        // Allow safe CSS properties for styling
        _sanitizer.AllowedCssProperties.Add("background");
        _sanitizer.AllowedCssProperties.Add("background-color");
        _sanitizer.AllowedCssProperties.Add("background-image");
        _sanitizer.AllowedCssProperties.Add("background-size");
        _sanitizer.AllowedCssProperties.Add("background-position");
        _sanitizer.AllowedCssProperties.Add("border");
        _sanitizer.AllowedCssProperties.Add("border-left");
        _sanitizer.AllowedCssProperties.Add("border-right");
        _sanitizer.AllowedCssProperties.Add("border-top");
        _sanitizer.AllowedCssProperties.Add("border-bottom");
        _sanitizer.AllowedCssProperties.Add("border-color");
        _sanitizer.AllowedCssProperties.Add("border-width");
        _sanitizer.AllowedCssProperties.Add("border-style");
        _sanitizer.AllowedCssProperties.Add("border-radius");
        _sanitizer.AllowedCssProperties.Add("padding");
        _sanitizer.AllowedCssProperties.Add("padding-left");
        _sanitizer.AllowedCssProperties.Add("padding-right");
        _sanitizer.AllowedCssProperties.Add("padding-top");
        _sanitizer.AllowedCssProperties.Add("padding-bottom");
        _sanitizer.AllowedCssProperties.Add("margin");
        _sanitizer.AllowedCssProperties.Add("margin-left");
        _sanitizer.AllowedCssProperties.Add("margin-right");
        _sanitizer.AllowedCssProperties.Add("margin-top");
        _sanitizer.AllowedCssProperties.Add("margin-bottom");
        _sanitizer.AllowedCssProperties.Add("color");
        _sanitizer.AllowedCssProperties.Add("font-size");
        _sanitizer.AllowedCssProperties.Add("font-weight");
        _sanitizer.AllowedCssProperties.Add("font-family");
        _sanitizer.AllowedCssProperties.Add("text-align");
        _sanitizer.AllowedCssProperties.Add("line-height");
        _sanitizer.AllowedCssProperties.Add("display");
        _sanitizer.AllowedCssProperties.Add("width");
        _sanitizer.AllowedCssProperties.Add("max-width");
        _sanitizer.AllowedCssProperties.Add("height");
        _sanitizer.AllowedCssProperties.Add("max-height");

        // Disallow dangerous attributes (onclick, onerror, etc.) - not in allow-list
        // Disallow script, iframe, object, embed tags - not in allow-list
        // CSS properties like 'position', 'z-index' are not allowed for security
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
