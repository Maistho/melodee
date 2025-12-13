using System.Text.Json.Serialization;

namespace Melodee.Common.Plugins.SearchEngine.LastFm;

public record LastFmResult(
    Results? Results
);

public record Results(
    Opensearch_Query? OpensearchQuery,
    string? OpensearchTotalResults,
    string? OpensearchStartIndex,
    string? OpensearchItemsPerPage,
    Albummatches? Albummatches
);

public record Opensearch_Query(
    string? _text,
    string? role,
    string? searchTerms,
    string? startPage
);

public record Albummatches(
    Album[]? album
);

public record Album(
    string? name,
    string? artist,
    string? url,
    Image[]? image,
    string? streamable,
    string? mbid
);

public record Image(
    string? _text,
    string? size
);

public record LastFmArtistSearchResult([property: JsonPropertyName("results")] ArtistResults? Results);

public record ArtistResults(
    [property: JsonPropertyName("opensearch:Query")] Opensearch_Query? OpensearchQuery,
    [property: JsonPropertyName("opensearch:totalResults")] string? OpensearchTotalResults,
    [property: JsonPropertyName("opensearch:startIndex")] string? OpensearchStartIndex,
    [property: JsonPropertyName("opensearch:itemsPerPage")] string? OpensearchItemsPerPage,
    [property: JsonPropertyName("artistmatches")] Artistmatches? Artistmatches
);

public record Artistmatches([property: JsonPropertyName("artist")] ArtistResult[]? Artist);

public record ArtistResult(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("mbid")] string? Mbid,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("image")] Image[]? image
);
