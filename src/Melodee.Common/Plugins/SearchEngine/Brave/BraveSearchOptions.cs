namespace Melodee.Common.Plugins.SearchEngine.Brave;

public sealed class BraveSearchOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.search.brave.com";

    public string ImageSearchPath { get; set; } = "/res/v1/images/search";

    public bool Enabled { get; set; } = true;
}
