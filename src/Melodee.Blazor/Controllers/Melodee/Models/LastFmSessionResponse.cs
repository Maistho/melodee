using System.Text.Json.Serialization;

namespace Melodee.Blazor.Controllers.Melodee.Models;

public record LastFmSessionResponse([property: JsonPropertyName("session")] LastFmSessionData? Session);

public record LastFmSessionData(
    [property: JsonPropertyName("key")] string? Key,
    [property: JsonPropertyName("name")] string? Name);
