using System.Text.Json.Serialization;

namespace Melodee.Common.Plugins.SearchEngine.Brave;

public sealed class BraveImageSearchResponse
{
    [JsonPropertyName("results")]
    public List<BraveImageResult> Results { get; set; } = [];
}

public sealed class BraveImageResult
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("thumbnail")]
    public BraveThumbnail? Thumbnail { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("page_url")]
    public string? PageUrl { get; set; }

    [JsonPropertyName("properties")]
    public BraveImageProperties? Properties { get; set; }
}

public sealed class BraveThumbnail
{
    [JsonPropertyName("src")]
    public string? Src { get; set; }
}

public sealed class BraveImageProperties
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("content_size")]
    public string? ContentSize { get; set; }
}
