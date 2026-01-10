using Melodee.Common.Data.Models;
using Microsoft.AspNetCore.SignalR;

namespace Melodee.Blazor.Hubs;

/// <summary>
/// Service for sending real-time notifications to party session clients via SignalR.
/// </summary>
public interface IPartyNotificationService
{
    Task NotifyQueueChangedAsync(Guid sessionApiKey, long revision, QueueChangeType changeType, Guid? itemApiKey, IEnumerable<PartyQueueItem> items);
    Task NotifyPlaybackChangedAsync(Guid sessionApiKey, PartyPlaybackState playbackState);
    Task NotifyParticipantsChangedAsync(Guid sessionApiKey, IEnumerable<PartySessionParticipant> participants);
    Task NotifySessionEndedAsync(Guid sessionApiKey);
}

public sealed class PartyNotificationService(
    IHubContext<PartyHub> hubContext,
    ILogger<PartyNotificationService> logger) : IPartyNotificationService
{
    public async Task NotifyQueueChangedAsync(
        Guid sessionApiKey,
        long revision,
        QueueChangeType changeType,
        Guid? itemApiKey,
        IEnumerable<PartyQueueItem> items)
    {
        try
        {
            var itemDtos = items.Select(x => new PartyQueueItemDto(x.ApiKey, x.SongApiKey, x.EnqueuedByUserId, x.SortOrder, x.Source));
            await hubContext.Clients.Group($"party:{sessionApiKey}")
                .SendAsync("QueueChanged", new QueueChangedEvent(revision, changeType, itemApiKey, itemDtos));
            logger.LogDebug("Sent QueueChanged event for session {SessionApiKey}, revision {Revision}", sessionApiKey, revision);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send QueueChanged notification for session {SessionApiKey}", sessionApiKey);
        }
    }

    public async Task NotifyPlaybackChangedAsync(Guid sessionApiKey, PartyPlaybackState playbackState)
    {
        try
        {
            var playbackEvent = new PlaybackChangedEvent(
                playbackState.CurrentQueueItemApiKey,
                playbackState.PositionSeconds,
                playbackState.IsPlaying,
                playbackState.Volume);

            await hubContext.Clients.Group($"party:{sessionApiKey}")
                .SendAsync("PlaybackChanged", playbackEvent);
            logger.LogDebug("Sent PlaybackChanged event for session {SessionApiKey}", sessionApiKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send PlaybackChanged notification for session {SessionApiKey}", sessionApiKey);
        }
    }

    public async Task NotifyParticipantsChangedAsync(
        Guid sessionApiKey,
        IEnumerable<PartySessionParticipant> participants)
    {
        try
        {
            var participantDtos = participants.Select(x => new PartyParticipantDto(x.UserId, x.Role.ToString(), x.IsBanned));
            await hubContext.Clients.Group($"party:{sessionApiKey}")
                .SendAsync("ParticipantsChanged", new ParticipantsChangedEvent(participantDtos));
            logger.LogDebug("Sent ParticipantsChanged event for session {SessionApiKey}, {Count} participants",
                sessionApiKey, participants.Count());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send ParticipantsChanged notification for session {SessionApiKey}", sessionApiKey);
        }
    }

    public async Task NotifySessionEndedAsync(Guid sessionApiKey)
    {
        try
        {
            await hubContext.Clients.Group($"party:{sessionApiKey}")
                .SendAsync("SessionEnded");
            logger.LogInformation("Sent SessionEnded event for session {SessionApiKey}", sessionApiKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send SessionEnded notification for session {SessionApiKey}", sessionApiKey);
        }
    }
}
