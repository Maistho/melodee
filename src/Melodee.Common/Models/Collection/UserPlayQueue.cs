namespace Melodee.Common.Models.Collection;

/// <summary>
/// Represents a user's play queue with song data.
/// </summary>
/// <param name="Songs">The songs in the queue in order.</param>
/// <param name="CurrentSongApiKey">The API key of the currently playing song, if any.</param>
/// <param name="Position">The playback position in seconds of the current song.</param>
/// <param name="ChangedBy">The client/user that last modified the queue.</param>
/// <param name="LastUpdatedAt">When the queue was last updated.</param>
public record UserPlayQueue(
    SongDataInfo[] Songs,
    Guid? CurrentSongApiKey,
    double Position,
    string ChangedBy,
    string? LastUpdatedAt);
