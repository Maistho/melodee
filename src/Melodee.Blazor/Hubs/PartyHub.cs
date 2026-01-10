using Microsoft.AspNetCore.SignalR;

namespace Melodee.Blazor.Hubs;

/// <summary>
/// SignalR hub for real-time party session updates.
/// </summary>
public class PartyHub : Hub
{
    private readonly ILogger<PartyHub> _logger;

    public PartyHub(ILogger<PartyHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join a party session group to receive real-time updates.
    /// </summary>
    public async Task JoinPartySession(Guid sessionApiKey)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"party:{sessionApiKey}");
        _logger.Information("Client {ConnectionId} joined party session {SessionApiKey}", Context.ConnectionId, sessionApiKey);
    }

    /// <summary>
    /// Leave a party session group.
    /// </summary>
    public async Task LeavePartySession(Guid sessionApiKey)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"party:{sessionApiKey}");
        _logger.Information("Client {ConnectionId} left party session {SessionApiKey}", Context.ConnectionId, sessionApiKey);
    }

    /// <summary>
    /// Called when a client connects.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.Information("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.Information("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Event payload for queue changes.
/// </summary>
public record QueueChangedEvent(long Revision, QueueChangeType ChangeType, Guid? ItemApiKey, IEnumerable<PartyQueueItemDto> Items);

/// <summary>
/// Types of queue changes.
/// </summary>
public enum QueueChangeType
{
    Added = 1,
    Removed = 2,
    Reordered = 3,
    Cleared = 4
}

/// <summary>
/// DTO for queue items in SignalR events.
/// </summary>
public record PartyQueueItemDto(Guid ApiKey, Guid SongApiKey, int EnqueuedByUserId, int SortOrder, string? Source);

/// <summary>
/// Event payload for playback state changes.
/// </summary>
public record PlaybackChangedEvent(Guid? CurrentQueueItemApiKey, double PositionSeconds, bool IsPlaying, double? Volume);

/// <summary>
/// Event payload for participant changes.
/// </summary>
public record ParticipantsChangedEvent(IEnumerable<PartyParticipantDto> Participants);

/// <summary>
/// DTO for participants in SignalR events.
/// </summary>
public record PartyParticipantDto(int UserId, string Role, bool IsBanned);
