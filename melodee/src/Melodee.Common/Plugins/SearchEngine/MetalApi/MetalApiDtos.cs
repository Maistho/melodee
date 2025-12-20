using System.Text.Json.Serialization;

namespace Melodee.Common.Plugins.SearchEngine.MetalApi;

/// <summary>
///     Response for band search from Metal API
/// </summary>
public sealed class MetalBandSearchResult
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("genre")]
    public string? Genre { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }
}

/// <summary>
///     Response for album search from Metal API
/// </summary>
public sealed class MetalAlbumSearchResult
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("band")]
    public MetalBandInfo? Band { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }
}

/// <summary>
///     Band information nested in album search results
/// </summary>
public sealed class MetalBandInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }
}

/// <summary>
///     Full album details from Metal API
/// </summary>
public sealed class MetalAlbum
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("releaseDate")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("catalogId")]
    public string? CatalogId { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("limitations")]
    public string? Limitations { get; set; }

    [JsonPropertyName("reviews")]
    public string? Reviews { get; set; }

    [JsonPropertyName("coverUrl")]
    public string? CoverUrl { get; set; }

    [JsonPropertyName("songs")]
    public MetalSong[]? Songs { get; set; }

    [JsonPropertyName("band")]
    public MetalBandInfo? Band { get; set; }
}

/// <summary>
///     Song information from Metal API album details
/// </summary>
public sealed class MetalSong
{
    [JsonPropertyName("number")]
    public int? Number { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("length")]
    public string? Length { get; set; }
}

/// <summary>
///     Error response from Metal API
/// </summary>
public sealed class MetalApiErrorResponse
{
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("statusCode")]
    public int? StatusCode { get; set; }
}
