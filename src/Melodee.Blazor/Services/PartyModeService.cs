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
        _logger.LogDebug("[PartyModeService] CreateSessionAsync: Name={Name}, HasJoinCode={HasJoinCode}", name, joinCode != null);
        try
        {
            var request = new { Name = name, JoinCode = joinCode };
            var response = await _httpClient.PostAsJsonAsync(BasePath, request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OperationResult<PartySessionDto>>();
                _logger.LogDebug("[PartyModeService] CreateSessionAsync succeeded: ApiKey={ApiKey}", result?.Data?.ApiKey);
                return result;
            }
            _logger.LogWarning("[PartyModeService] CreateSessionAsync failed: StatusCode={StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in CreateSessionAsync");
            return null;
        }
    }

    public async Task<IEnumerable<PartySessionDto>?> GetMySessionsAsync()
    {
        _logger.LogDebug("[PartyModeService] GetMySessionsAsync starting");
        try
        {
            var response = await _httpClient.GetAsync($"{BasePath}/my");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<IEnumerable<PartySessionDto>>();
                _logger.LogDebug("[PartyModeService] GetMySessionsAsync succeeded: Count={Count}", result?.Count() ?? 0);
                return result;
            }
            _logger.LogWarning("[PartyModeService] GetMySessionsAsync failed: StatusCode={StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in GetMySessionsAsync");
            return null;
        }
    }

    public async Task<IEnumerable<PartySessionDto>?> GetActiveSessionsAsync()
    {
        _logger.LogDebug("[PartyModeService] GetActiveSessionsAsync starting");
        try
        {
            var response = await _httpClient.GetAsync($"{BasePath}/active");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<IEnumerable<PartySessionDto>>();
                _logger.LogDebug("[PartyModeService] GetActiveSessionsAsync succeeded: Count={Count}", result?.Count() ?? 0);
                return result;
            }
            _logger.LogWarning("[PartyModeService] GetActiveSessionsAsync failed: StatusCode={StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in GetActiveSessionsAsync");
            return null;
        }
    }

    public async Task<OperationResult<PartySessionDto>?> GetSessionAsync(Guid sessionApiKey)
    {
        _logger.LogDebug("[PartyModeService] GetSessionAsync: ApiKey={ApiKey}", sessionApiKey);
        try
        {
            var response = await _httpClient.GetAsync($"{BasePath}/{sessionApiKey}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OperationResult<PartySessionDto>>();
                _logger.LogDebug("[PartyModeService] GetSessionAsync succeeded: Name={Name}", result?.Data?.Name);
                return result;
            }
            _logger.LogWarning("[PartyModeService] GetSessionAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in GetSessionAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartySessionParticipantDto>?> JoinSessionAsync(Guid sessionApiKey, string? joinCode = null)
    {
        _logger.LogDebug("[PartyModeService] JoinSessionAsync: ApiKey={ApiKey}, HasJoinCode={HasJoinCode}", sessionApiKey, joinCode != null);
        try
        {
            var request = new { JoinCode = joinCode };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/join", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OperationResult<PartySessionParticipantDto>>();
                _logger.LogDebug("[PartyModeService] JoinSessionAsync succeeded: ApiKey={ApiKey}", sessionApiKey);
                return result;
            }
            _logger.LogWarning("[PartyModeService] JoinSessionAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in JoinSessionAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<bool> LeaveSessionAsync(Guid sessionApiKey)
    {
        _logger.LogDebug("[PartyModeService] LeaveSessionAsync: ApiKey={ApiKey}", sessionApiKey);
        try
        {
            var response = await _httpClient.PostAsync($"{BasePath}/{sessionApiKey}/leave", null);
            var success = response.IsSuccessStatusCode;
            _logger.LogDebug("[PartyModeService] LeaveSessionAsync: ApiKey={ApiKey}, Success={Success}", sessionApiKey, success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in LeaveSessionAsync: ApiKey={ApiKey}", sessionApiKey);
            return false;
        }
    }

    public async Task<bool> EndSessionAsync(Guid sessionApiKey)
    {
        _logger.LogDebug("[PartyModeService] EndSessionAsync: ApiKey={ApiKey}", sessionApiKey);
        try
        {
            var response = await _httpClient.PostAsync($"{BasePath}/{sessionApiKey}/end", null);
            var success = response.IsSuccessStatusCode;
            _logger.LogDebug("[PartyModeService] EndSessionAsync: ApiKey={ApiKey}, Success={Success}", sessionApiKey, success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in EndSessionAsync: ApiKey={ApiKey}", sessionApiKey);
            return false;
        }
    }

    public async Task<OperationResult<IEnumerable<PartySessionParticipantDto>>?> GetParticipantsAsync(Guid sessionApiKey)
    {
        _logger.LogDebug("[PartyModeService] GetParticipantsAsync: ApiKey={ApiKey}", sessionApiKey);
        try
        {
            var response = await _httpClient.GetAsync($"{BasePath}/{sessionApiKey}/participants");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<IEnumerable<PartySessionParticipantDto>>>();
            }
            _logger.LogWarning("[PartyModeService] GetParticipantsAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in GetParticipantsAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<QueueResponseDto>?> GetQueueAsync(Guid sessionApiKey)
    {
        _logger.LogDebug("[PartyModeService] GetQueueAsync: ApiKey={ApiKey}", sessionApiKey);
        try
        {
            var response = await _httpClient.GetAsync($"{BasePath}/{sessionApiKey}/queue");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<QueueResponseDto>>();
            }
            _logger.LogWarning("[PartyModeService] GetQueueAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in GetQueueAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<AddItemsResponseDto>?> AddToQueueAsync(
        Guid sessionApiKey,
        IEnumerable<Guid> songApiKeys,
        string? source = null,
        long expectedRevision = 1)
    {
        _logger.LogDebug("[PartyModeService] AddToQueueAsync: ApiKey={ApiKey}, SongCount={SongCount}, ExpectedRevision={ExpectedRevision}", 
            sessionApiKey, songApiKeys.Count(), expectedRevision);
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
                _logger.LogWarning("[PartyModeService] AddToQueueAsync conflict (revision mismatch): ApiKey={ApiKey}", sessionApiKey);
            }
            else
            {
                _logger.LogWarning("[PartyModeService] AddToQueueAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in AddToQueueAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<long>?> RemoveFromQueueAsync(Guid sessionApiKey, Guid itemApiKey, long expectedRevision)
    {
        _logger.LogDebug("[PartyModeService] RemoveFromQueueAsync: SessionApiKey={SessionApiKey}, ItemApiKey={ItemApiKey}", sessionApiKey, itemApiKey);
        try
        {
            var response = await _httpClient.DeleteAsync(
                $"{BasePath}/{sessionApiKey}/queue/items/{itemApiKey}?expectedRevision={expectedRevision}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<long>>();
            }
            _logger.LogWarning("[PartyModeService] RemoveFromQueueAsync failed: SessionApiKey={SessionApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in RemoveFromQueueAsync: SessionApiKey={SessionApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<long>?> ReorderQueueItemAsync(
        Guid sessionApiKey,
        Guid itemApiKey,
        int newIndex,
        long expectedRevision)
    {
        _logger.LogDebug("[PartyModeService] ReorderQueueItemAsync: SessionApiKey={SessionApiKey}, ItemApiKey={ItemApiKey}, NewIndex={NewIndex}", 
            sessionApiKey, itemApiKey, newIndex);
        try
        {
            var request = new { NewIndex = newIndex, ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync(
                $"{BasePath}/{sessionApiKey}/queue/items/{itemApiKey}/reorder", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<long>>();
            }
            _logger.LogWarning("[PartyModeService] ReorderQueueItemAsync failed: SessionApiKey={SessionApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in ReorderQueueItemAsync: SessionApiKey={SessionApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<long>?> ClearQueueAsync(Guid sessionApiKey, long expectedRevision)
    {
        _logger.LogDebug("[PartyModeService] ClearQueueAsync: SessionApiKey={SessionApiKey}, ExpectedRevision={ExpectedRevision}", sessionApiKey, expectedRevision);
        try
        {
            var response = await _httpClient.PostAsync(
                $"{BasePath}/{sessionApiKey}/queue/clear?expectedRevision={expectedRevision}", null);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<long>>();
            }
            _logger.LogWarning("[PartyModeService] ClearQueueAsync failed: SessionApiKey={SessionApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in ClearQueueAsync: SessionApiKey={SessionApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> GetPlaybackStateAsync(Guid sessionApiKey)
    {
        _logger.LogDebug("[PartyModeService] GetPlaybackStateAsync: ApiKey={ApiKey}", sessionApiKey);
        try
        {
            var response = await _httpClient.GetAsync($"{BasePath}/{sessionApiKey}/playback");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            _logger.LogWarning("[PartyModeService] GetPlaybackStateAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in GetPlaybackStateAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> PlayAsync(Guid sessionApiKey, double? position = null, long expectedRevision = 0)
    {
        _logger.LogDebug("[PartyModeService] PlayAsync: ApiKey={ApiKey}, Position={Position}", sessionApiKey, position);
        try
        {
            var request = new { PositionSeconds = position, ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/playback/play", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            _logger.LogWarning("[PartyModeService] PlayAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in PlayAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> PauseAsync(Guid sessionApiKey, double? position = null, long expectedRevision = 0)
    {
        _logger.LogDebug("[PartyModeService] PauseAsync: ApiKey={ApiKey}, Position={Position}", sessionApiKey, position);
        try
        {
            var request = new { PositionSeconds = position, ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/playback/pause", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            _logger.LogWarning("[PartyModeService] PauseAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in PauseAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> SkipAsync(Guid sessionApiKey, long expectedRevision)
    {
        _logger.LogDebug("[PartyModeService] SkipAsync: ApiKey={ApiKey}, ExpectedRevision={ExpectedRevision}", sessionApiKey, expectedRevision);
        try
        {
            var request = new { ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/playback/skip", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            _logger.LogWarning("[PartyModeService] SkipAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in SkipAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> SeekAsync(Guid sessionApiKey, double position, long expectedRevision)
    {
        _logger.LogDebug("[PartyModeService] SeekAsync: ApiKey={ApiKey}, Position={Position}", sessionApiKey, position);
        try
        {
            var request = new { PositionSeconds = position, ExpectedRevision = expectedRevision };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/playback/seek", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            _logger.LogWarning("[PartyModeService] SeekAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in SeekAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<OperationResult<PartyPlaybackStateDto>?> SetVolumeAsync(Guid sessionApiKey, double volume)
    {
        _logger.LogDebug("[PartyModeService] SetVolumeAsync: ApiKey={ApiKey}, Volume={Volume}", sessionApiKey, volume);
        try
        {
            var request = new { Volume = volume };
            var response = await _httpClient.PostAsJsonAsync($"{BasePath}/{sessionApiKey}/playback/volume", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<PartyPlaybackStateDto>>();
            }
            _logger.LogWarning("[PartyModeService] SetVolumeAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in SetVolumeAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    // Endpoint Registry Methods

    private const string EndpointsBasePath = "api/v1/endpoints";

    public async Task<OperationResult<IEnumerable<SessionEndpointDto>>?> GetEndpointsForSessionAsync(Guid sessionApiKey)
    {
        _logger.LogDebug("[PartyModeService] GetEndpointsForSessionAsync: ApiKey={ApiKey}", sessionApiKey);
        try
        {
            var response = await _httpClient.GetAsync($"{EndpointsBasePath}/for-session/{sessionApiKey}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperationResult<IEnumerable<SessionEndpointDto>>>();
            }
            _logger.LogWarning("[PartyModeService] GetEndpointsForSessionAsync failed: ApiKey={ApiKey}, StatusCode={StatusCode}", sessionApiKey, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in GetEndpointsForSessionAsync: ApiKey={ApiKey}", sessionApiKey);
            return null;
        }
    }

    public async Task<bool> AttachEndpointAsync(Guid endpointApiKey, Guid sessionApiKey)
    {
        _logger.LogDebug("[PartyModeService] AttachEndpointAsync: EndpointApiKey={EndpointApiKey}, SessionApiKey={SessionApiKey}", endpointApiKey, sessionApiKey);
        try
        {
            var request = new { SessionApiKey = sessionApiKey };
            var response = await _httpClient.PostAsJsonAsync($"{EndpointsBasePath}/{endpointApiKey}/attach", request);
            var success = response.IsSuccessStatusCode;
            _logger.LogDebug("[PartyModeService] AttachEndpointAsync: Success={Success}", success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in AttachEndpointAsync: EndpointApiKey={EndpointApiKey}", endpointApiKey);
            return false;
        }
    }

    public async Task<bool> DetachEndpointAsync(Guid endpointApiKey)
    {
        _logger.LogDebug("[PartyModeService] DetachEndpointAsync: EndpointApiKey={EndpointApiKey}", endpointApiKey);
        try
        {
            var response = await _httpClient.PostAsync($"{EndpointsBasePath}/{endpointApiKey}/detach", null);
            var success = response.IsSuccessStatusCode;
            _logger.LogDebug("[PartyModeService] DetachEndpointAsync: Success={Success}", success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PartyModeService] Exception in DetachEndpointAsync: EndpointApiKey={EndpointApiKey}", endpointApiKey);
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
