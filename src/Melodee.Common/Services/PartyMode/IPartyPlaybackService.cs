using Melodee.Common.Data.Models;
using Melodee.Common.Models;

namespace Melodee.Common.Services;

/// <summary>
/// Interface for party playback operations.
/// </summary>
public interface IPartyPlaybackService
{
    /// <summary>
    /// Gets the playback state for a session.
    /// </summary>
    Task<OperationResult<PartyPlaybackState?>> GetPlaybackStateAsync(Guid sessionApiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the playback state from an endpoint heartbeat.
    /// </summary>
    Task<OperationResult<PartyPlaybackState>> UpdateFromHeartbeatAsync(
        Guid sessionApiKey,
        Guid? currentQueueItemApiKey,
        double positionSeconds,
        bool isPlaying,
        double? volume01,
        int endpointUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the current queue item.
    /// </summary>
    Task<OperationResult<PartyPlaybackState>> SetCurrentItemAsync(Guid sessionApiKey, Guid? queueItemApiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates playback state from controller intent (play/pause/skip/seek).
    /// </summary>
    Task<OperationResult<PartyPlaybackState>> UpdateIntentAsync(
        Guid sessionApiKey,
        PlaybackIntent intent,
        double? positionSeconds,
        int requestingUserId,
        long expectedRevision,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a playback intent action.
/// </summary>
public enum PlaybackIntent
{
    /// <summary>
    /// Start playback.
    /// </summary>
    Play,

    /// <summary>
    /// Pause playback.
    /// </summary>
    Pause,

    /// <summary>
    /// Skip to next track.
    /// </summary>
    Skip,

    /// <summary>
    /// Seek to position.
    /// </summary>
    Seek
}
