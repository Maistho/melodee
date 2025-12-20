namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Represents lyrics for a song, supporting both plain text and synchronized (timed) lyrics.
/// </summary>
/// <param name="SongId">The unique identifier of the song.</param>
/// <param name="Language">The language code of the lyrics (e.g., "en", "es").</param>
/// <param name="IsSynced">Whether the lyrics include timing information for synchronized display.</param>
/// <param name="PlainText">The full lyrics as plain text (without timing).</param>
/// <param name="Lines">Individual lines with optional timing information for synced lyrics.</param>
/// <param name="DisplayArtist">Optional display name for the artist.</param>
/// <param name="DisplayTitle">Optional display name for the song title.</param>
/// <param name="Offset">Offset in milliseconds to adjust timing for synced lyrics.</param>
public record Lyrics(
    Guid SongId,
    string Language,
    bool IsSynced,
    string? PlainText,
    LyricsLine[]? Lines,
    string? DisplayArtist,
    string? DisplayTitle,
    double? Offset);

/// <summary>
/// Represents a single line of lyrics with optional timing information.
/// </summary>
/// <param name="Text">The text content of the line.</param>
/// <param name="StartMs">The start time in milliseconds (for synced lyrics).</param>
public record LyricsLine(string Text, long? StartMs);
