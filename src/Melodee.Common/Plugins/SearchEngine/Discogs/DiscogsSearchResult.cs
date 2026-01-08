using System.Text.Json.Serialization;

namespace Melodee.Common.Plugins.SearchEngine.Discogs;

/// <summary>
///     Discogs search result response DTOs.
/// </summary>
public class DiscogsSearchResult
{
    [JsonPropertyName("pagination")]
    public DiscogsPagination? Pagination { get; set; }

    [JsonPropertyName("results")]
    public List<DiscogsResult>? Results { get; set; }
}

public class DiscogsPagination
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    [JsonPropertyName("per_page")]
    public int PerPage { get; set; }

    [JsonPropertyName("items")]
    public int Items { get; set; }
}

public class DiscogsResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("master_id")]
    public int? MasterId { get; set; }

    [JsonPropertyName("master_url")]
    public string? MasterUrl { get; set; }

    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("thumb")]
    public string? Thumb { get; set; }

    [JsonPropertyName("cover_image")]
    public string? CoverImage { get; set; }

    [JsonPropertyName("resource_url")]
    public string? ResourceUrl { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("year")]
    public string? Year { get; set; }

    [JsonPropertyName("format")]
    public List<string>? Format { get; set; }

    [JsonPropertyName("label")]
    public List<string>? Label { get; set; }

    [JsonPropertyName("genre")]
    public List<string>? Genre { get; set; }

    [JsonPropertyName("style")]
    public List<string>? Style { get; set; }

    [JsonPropertyName("barcode")]
    public List<string>? Barcode { get; set; }

    [JsonPropertyName("catno")]
    public string? CatNo { get; set; }

    [JsonPropertyName("entity_type")]
    public string? EntityType { get; set; }

    [JsonPropertyName("entity_type_name")]
    public string? EntityTypeName { get; set; }
}
