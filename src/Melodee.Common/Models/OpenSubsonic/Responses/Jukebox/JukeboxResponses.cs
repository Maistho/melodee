using System.Text.Json.Serialization;

namespace Melodee.Common.Models.OpenSubsonic.Responses.Jukebox;

/// <summary>
/// Response for jukebox status request.
/// </summary>
public sealed record JukeboxStatusResponse(
    [property: JsonPropertyName("currentIndex")] int CurrentIndex,
    [property: JsonPropertyName("playing")] bool Playing,
    [property: JsonPropertyName("position")] double Position,
    [property: JsonPropertyName("gain")] double Gain,
    [property: JsonPropertyName("maxVolume")] int MaxVolume,
    [property: JsonPropertyName("jukeboxPlaylist")] JukeboxPlaylistResponse? Playlist);

/// <summary>
/// Jukebox playlist in response.
/// </summary>
public sealed record JukeboxPlaylistResponse(
    [property: JsonPropertyName("entry")] List<JukeboxEntryResponse> Entries,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("comment")] string? Comment,
    [property: JsonPropertyName("public")] bool IsPublic,
    [property: JsonPropertyName("songCount")] int SongCount,
    [property: JsonPropertyName("duration")] int Duration);

/// <summary>
/// Individual entry in jukebox playlist.
/// </summary>
public sealed record JukeboxEntryResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("parent")] string Parent,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("artist")] string Artist,
    [property: JsonPropertyName("album")] string Album,
    [property: JsonPropertyName("year")] int Year,
    [property: JsonPropertyName("genre")] string? Genre,
    [property: JsonPropertyName("coverArt")] string? CoverArt,
    [property: JsonPropertyName("duration")] int Duration,
    [property: JsonPropertyName("bitRate")] int BitRate,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("transcodedContentType")] string? TranscodedContentType,
    [property: JsonPropertyName("transcodedSuffix")] string? TranscodedSuffix,
    [property: JsonPropertyName("isDir")] bool IsDir,
    [property: JsonPropertyName("isVideo")] bool IsVideo,
    [property: JsonPropertyName("type")] string Type);

/// <summary>
/// Response for jukebox get request (playlist).
/// </summary>
public sealed record JukeboxGetResponse(
    [property: JsonPropertyName("jukeboxPlaylist")] JukeboxPlaylistResponse Playlist);
