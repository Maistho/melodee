using Melodee.Common.Data.Models;
using Melodee.Common.Enums.PartyMode;

namespace Melodee.Common.Services.PartyMode;

/// <summary>
/// Service for sending real-time notifications to party session clients.
/// </summary>
public interface IPartyNotificationService
{
    Task NotifyQueueChangedAsync(Guid sessionApiKey, long revision, QueueChangeType changeType, Guid? itemApiKey, IEnumerable<PartyQueueItem> items);
    Task NotifyPlaybackChangedAsync(Guid sessionApiKey, PartyPlaybackState playbackState);
    Task NotifyParticipantsChangedAsync(Guid sessionApiKey, IEnumerable<PartySessionParticipant> participants);
    Task NotifySessionEndedAsync(Guid sessionApiKey);
}
