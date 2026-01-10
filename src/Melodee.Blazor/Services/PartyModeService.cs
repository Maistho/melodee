using Melodee.Common.Models;

namespace Melodee.Blazor.Services;

/// <summary>
/// Service for interacting with Party Mode API endpoints.
/// </summary>
public class PartyModeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PartyModeService> _logger;

    public PartyModeService(HttpClient httpClient, ILogger<PartyModeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    private const string BasePath = "api/v1/party-sessions";

    public async Task<OperationResult<PartySessionDto>?> CreateSessionAsync(string name, string? joinCode = null)
    {
        try
        {
            var request = new { Name = name, JoinCode = joinCode };
            var response = await _httpClient.PostAsJsonAsync(BasePath, request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartySessionDto>>();
            }
            _logger.LogWarning("Failed to create party session: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating party session");
            return null;
        }
    }

    public async Task<OperationResult<PartySessionDto>?> GetSessionAsync(Guid sessionApiKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BasePath}/{sessionApiKey}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartySessionDto>>();
            }
            _logger.LogWarning("Failed to get party session {ApiKey}: {StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting party session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartySessionParticipantDto>?> JoinSessionAsync(Guid sessionApiKey, string? joinCode = null)
    {
        try
        {
            var request = new { JoinCode = joinCode };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/join", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartySessionParticipantDto>>();
            }
            _logger.LogWarning("Failed to join party session {ApiKey}: {StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining party session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<bool> LeaveSessionAsync(Guid sessionApiKey)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{BasePath}/{sessionApiKey}/leave", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving party session {ApiKey}", sessionApiKey);
            return false;
        }
    }

    public async Task<bool> EndSessionAsync(Guid sessionApiKey)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{BasePath}/{sessionApiKey}/end", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending party session {ApiKey}", sessionApiKey);
            return false;
        }
    }

    public async Task<OperationResult<IEnumerable<PartySessionParticipantDto>>?> GetParticipantsAsync(Guid sessionApiKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BasePath}/{sessionApiKey}/participants");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<IEnumerable<PartySessionParticipantDto>>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participants for session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<QueueResponseDto>?> GetQueueAsync(Guid sessionApiKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BasePath}/{sessionApiKey}/queue");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<QueueResponseDto>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue for session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<AddItemsResponseDto>?> AddToQueueAsync(
        Guid sessionApiKey,
        IEnumerable<Guid> songApiKeys,
        string? source = null,
        long expectedRevision = 1)
    {
        try
        {
            var request = new { SongApiKeys = songApiKeys, Source = source, ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/queue/items", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<AddItemsResponseDto>>();
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.LogWarning("Queue conflict when adding items to session {ApiKey}", sessionApiKey);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding items to queue for session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<long>?> RemoveFromQueueAsync(Guid sessionApiKey, Guid itemApiKey, long expectedRevision)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(
                $"{BasePath}/{sessionApiKey}/queue/items/{itemApiKey}?expectedRevision={expectedRevision}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<long>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from queue for session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<long>?> ReorderQueueItemAsync(
        Guid sessionApiKey,
        Guid itemApiKey,
        int newIndex,
        long expectedRevision)
    {
        try
        {
            var request = new { NewIndex = newIndex, ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync(
                $"{BasePath}/{sessionApiKey}/queue/items/{itemApiKey}/reorder", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<long>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering queue item for session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<long>?> ClearQueueAsync(Guid sessionApiKey, long expectedRevision)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"{BasePath}/{sessionApiKey}/queue/clear?expectedRevision={expectedRevision}", null);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<long>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing queue for session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> GetPlaybackStateAsync(Guid sessionApiKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BasePath}/{sessionApiKey}/playback");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting playback state for session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> PlayAsync(Guid sessionApiKey, double? position = null, long expectedRevision = 0)
    {
        try
        {
            var request = new { PositionSeconds = position, ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/playback/play", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> PauseAsync(Guid sessionApiKey, double? position = null, long expectedRevision = 0)
    {
        try
        {
            var request = new { PositionSeconds = position, ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/playback/pause", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> SkipAsync(Guid sessionApiKey, long expectedRevision)
    {
        try
        {
            var request = new { ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/playback/skip", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping in session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> SeekAsync(Guid sessionApiKey, double position, long expectedRevision)
    {
        try
        {
            var request = new { PositionSeconds = position, ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/playback/seek", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeking in session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> SetVolumeAsync(Guid sessionApiKey, double volume)
    {
        try
        {
            var request = new { Volume = volume };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/playback/volume", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting volume for session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    // Endpoint Registry Methods

    private const string EndpointsBasePath = "api/v1/endpoints";

    public async Task<OperationResult<IEnumerable<SessionEndpointDto>>?> GetEndpointsForSessionAsync(Guid sessionApiKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{EndpointsBasePath}/for-session/{sessionApiKey}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<IEnumerable<SessionEndpointDto>>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting endpoints for session {ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<bool> AttachEndpointAsync(Guid endpointApiKey, Guid sessionApiKey)
    {
        try
        {
            var request = new { SessionApiKey = sessionApiKey };
            var response = await _httpClient.PostAsJsonAsync($"{EndpointsBasePath}/{endpointApiKey}/attach", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attaching endpoint {EndpointApiKey} to session {SessionApiKey}", endpointApiKey, sessionApiKey);
            return false;
        }
    }

    public async Task<bool> DetachEndpointAsync(Guid endpointApiKey)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{EndpointsBasePath}/{endpointApiKey}/detach", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detaching endpoint {EndpointApiKey}", endpointApiKey);
            return false;
        }
    }
}

// DTO classes (simplified versions matching API responses)
public record PartySessionDto(Guid ApiKey, string Name, int OwnerUserId, string Status, long QueueRevision, long PlaybackRevision);
public record PartySessionParticipantDto(int UserId, string Role, string JoinedAt);
public record QueueResponseDto(long Revision, IEnumerable<PartyQueueItemDto> Items);
public record PartyQueueItemDto(Guid ApiKey, Guid SongApiKey, int EnqueuedByUserId, string EnqueuedAt, int SortOrder, string? Source);
public record AddItemsResponseDto(long NewRevision, IEnumerable<PartyQueueItemDto> AddedItems);
public record PartyPlaybackStateDto(Guid? CurrentQueueItemApiKey, double PositionSeconds, bool IsPlaying, double? Volume);
public record SessionEndpointDto(Guid ApiKey, string Name, string Type, bool IsShared, string? Room, string? LastSeenAt, string? CapabilitiesJson, bool IsOwner, bool IsActive, bool IsStale);
