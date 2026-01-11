using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums.PartyMode;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic.Responses.Jukebox;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Playback;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services.Jukebox;

/// <summary>
/// Interface for Subsonic jukebox control operations.
/// </summary>
public interface ISubsonicJukeboxService
{
    /// <summary>
    /// Gets the jukebox status.
    /// </summary>
    Task<OperationResult<JukeboxStatusResponse>> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the jukebox playlist.
    /// </summary>
    Task<OperationResult<JukeboxGetResponse>> GetPlaylistAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the jukebox gain (volume).
    /// </summary>
    Task<OperationResult<bool>> SetGainAsync(double gain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts playback.
    /// </summary>
    Task<OperationResult<bool>> StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops playback.
    /// </summary>
    Task<OperationResult<bool>> StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Skips to a specific index or offset.
    /// </summary>
    Task<OperationResult<bool>> SkipAsync(int? index, int? offset, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds songs to the jukebox playlist.
    /// </summary>
    Task<OperationResult<bool>> AddAsync(IEnumerable<string> songIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the jukebox playlist.
    /// </summary>
    Task<OperationResult<bool>> ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a song from the jukebox playlist by index.
    /// </summary>
    Task<OperationResult<bool>> RemoveAsync(int index, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shuffles the jukebox playlist.
    /// </summary>
    Task<OperationResult<bool>> ShuffleAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for Subsonic jukebox control operations.
/// Maps Subsonic jukeboxControl API to party session/queue operations.
/// </summary>
public sealed class SubsonicJukeboxService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IOptions<JukeboxOptions> jukeboxOptions,
    IPartyQueueService partyQueueService,
    IPartyPlaybackService partyPlaybackService)
    : ServiceBase(logger, cacheManager, contextFactory), ISubsonicJukeboxService
{
    private const string JukeboxSessionApiKey = "jukebox-system-session";
    private const int SystemUserId = 0;

    public async Task<OperationResult<JukeboxStatusResponse>> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<JukeboxStatusResponse>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = default!
            };
        }

        var sessionResult = await GetOrCreateJukeboxSessionAsync(cancellationToken).ConfigureAwait(false);
        if (!sessionResult.IsSuccess || sessionResult.Data == null)
        {
            return new OperationResult<JukeboxStatusResponse>("Failed to get jukebox session")
            {
                Type = OperationResponseType.Error,
                Data = default!
            };
        }

        var session = sessionResult.Data;
        var queueResult = await partyQueueService.GetQueueAsync(session.ApiKey, cancellationToken).ConfigureAwait(false);
        var playbackResult = await partyPlaybackService.GetPlaybackStateAsync(session.ApiKey, cancellationToken).ConfigureAwait(false);

        var playlist = await BuildJukeboxPlaylistAsync(session.ApiKey, cancellationToken).ConfigureAwait(false);

        var status = new JukeboxStatusResponse(
            CurrentIndex: playbackResult.Data?.CurrentQueueItemApiKey != null
                ? FindCurrentIndex(playlist.Entries, playbackResult.Data.CurrentQueueItemApiKey.Value)
                : 0,
            Playing: playbackResult.Data?.IsPlaying ?? false,
            Position: playbackResult.Data?.PositionSeconds ?? 0,
            Gain: playbackResult.Data?.Volume ?? 0.8,
            MaxVolume: 100,
            Playlist: playlist);

        return new OperationResult<JukeboxStatusResponse> { Data = status };
    }

    public async Task<OperationResult<JukeboxGetResponse>> GetPlaylistAsync(CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<JukeboxGetResponse>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = default!
            };
        }

        var sessionResult = await GetOrCreateJukeboxSessionAsync(cancellationToken).ConfigureAwait(false);
        if (!sessionResult.IsSuccess || sessionResult.Data == null)
        {
            return new OperationResult<JukeboxGetResponse>("Failed to get jukebox session")
            {
                Type = OperationResponseType.Error,
                Data = default!
            };
        }

        var playlist = await BuildJukeboxPlaylistAsync(sessionResult.Data.ApiKey, cancellationToken).ConfigureAwait(false);

        return new OperationResult<JukeboxGetResponse>
        {
            Data = new JukeboxGetResponse(playlist)
        };
    }

    public async Task<OperationResult<bool>> SetGainAsync(double gain, CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var sessionResult = await GetOrCreateJukeboxSessionAsync(cancellationToken).ConfigureAwait(false);
        if (!sessionResult.IsSuccess || sessionResult.Data == null)
        {
            return new OperationResult<bool>("Failed to get jukebox session")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        var volume01 = Math.Clamp(gain, 0.0, 1.0);
        var playbackResult = await partyPlaybackService.UpdateFromHeartbeatAsync(
            sessionResult.Data.ApiKey,
            null,
            0,
            false,
            volume01,
            SystemUserId,
            cancellationToken
        ).ConfigureAwait(false);

        return new OperationResult<bool> { Data = playbackResult.IsSuccess };
    }

    public async Task<OperationResult<bool>> StartAsync(CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var sessionResult = await GetOrCreateJukeboxSessionAsync(cancellationToken).ConfigureAwait(false);
        if (!sessionResult.IsSuccess || sessionResult.Data == null)
        {
            return new OperationResult<bool>("Failed to get jukebox session")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        var playbackResult = await partyPlaybackService.UpdateIntentAsync(
            sessionResult.Data.ApiKey,
            PlaybackIntent.Play,
            null,
            SystemUserId,
            sessionResult.Data.PlaybackRevision,
            cancellationToken
        ).ConfigureAwait(false);

        return new OperationResult<bool> { Data = playbackResult.IsSuccess };
    }

    public async Task<OperationResult<bool>> StopAsync(CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var sessionResult = await GetOrCreateJukeboxSessionAsync(cancellationToken).ConfigureAwait(false);
        if (!sessionResult.IsSuccess || sessionResult.Data == null)
        {
            return new OperationResult<bool>("Failed to get jukebox session")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        var playbackResult = await partyPlaybackService.UpdateIntentAsync(
            sessionResult.Data.ApiKey,
            PlaybackIntent.Pause,
            null,
            SystemUserId,
            sessionResult.Data.PlaybackRevision,
            cancellationToken
        ).ConfigureAwait(false);

        return new OperationResult<bool> { Data = playbackResult.IsSuccess };
    }

    public async Task<OperationResult<bool>> SkipAsync(int? index, int? offset, CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var sessionResult = await GetOrCreateJukeboxSessionAsync(cancellationToken).ConfigureAwait(false);
        if (!sessionResult.IsSuccess || sessionResult.Data == null)
        {
            return new OperationResult<bool>("Failed to get jukebox session")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        var playbackResult = await partyPlaybackService.UpdateIntentAsync(
            sessionResult.Data.ApiKey,
            PlaybackIntent.Skip,
            null,
            SystemUserId,
            sessionResult.Data.PlaybackRevision,
            cancellationToken
        ).ConfigureAwait(false);

        return new OperationResult<bool> { Data = playbackResult.IsSuccess };
    }

    public async Task<OperationResult<bool>> AddAsync(IEnumerable<string> songIds, CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var sessionResult = await GetOrCreateJukeboxSessionAsync(cancellationToken).ConfigureAwait(false);
        if (!sessionResult.IsSuccess || sessionResult.Data == null)
        {
            return new OperationResult<bool>("Failed to get jukebox session")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        var songApiKeys = songIds
            .Where(id => Guid.TryParse(id, out _))
            .Select(Guid.Parse)
            .ToList();

        if (!songApiKeys.Any())
        {
            return new OperationResult<bool>("No valid song IDs provided")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var queueResult = await partyQueueService.GetQueueAsync(sessionResult.Data.ApiKey, cancellationToken).ConfigureAwait(false);
        var addResult = await partyQueueService.AddItemsAsync(
            sessionResult.Data.ApiKey,
            songApiKeys,
            SystemUserId,
            "subsonic",
            queueResult.Data.Revision,
            cancellationToken
        ).ConfigureAwait(false);

        return new OperationResult<bool> { Data = addResult.IsSuccess };
    }

    public async Task<OperationResult<bool>> ClearAsync(CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var sessionResult = await GetOrCreateJukeboxSessionAsync(cancellationToken).ConfigureAwait(false);
        if (!sessionResult.IsSuccess || sessionResult.Data == null)
        {
            return new OperationResult<bool>("Failed to get jukebox session")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        var queueResult = await partyQueueService.GetQueueAsync(sessionResult.Data.ApiKey, cancellationToken).ConfigureAwait(false);
        var clearResult = await partyQueueService.ClearAsync(
            sessionResult.Data.ApiKey,
            SystemUserId,
            queueResult.Data.Revision,
            cancellationToken
        ).ConfigureAwait(false);

        return new OperationResult<bool> { Data = clearResult.IsSuccess };
    }

    public async Task<OperationResult<bool>> RemoveAsync(int index, CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var sessionResult = await GetOrCreateJukeboxSessionAsync(cancellationToken).ConfigureAwait(false);
        if (!sessionResult.IsSuccess || sessionResult.Data == null)
        {
            return new OperationResult<bool>("Failed to get jukebox session")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        var queueResult = await partyQueueService.GetQueueAsync(sessionResult.Data.ApiKey, cancellationToken).ConfigureAwait(false);
        var items = queueResult.Data.Items.ToList();

        if (index < 0 || index >= items.Count)
        {
            return new OperationResult<bool>("Invalid index")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var itemToRemove = items[index];
        var removeResult = await partyQueueService.RemoveItemAsync(
            sessionResult.Data.ApiKey,
            itemToRemove.ApiKey,
            SystemUserId,
            queueResult.Data.Revision,
            cancellationToken
        ).ConfigureAwait(false);

        return new OperationResult<bool> { Data = removeResult.IsSuccess };
    }

    public async Task<OperationResult<bool>> ShuffleAsync(CancellationToken cancellationToken = default)
    {
        if (!jukeboxOptions.Value.Enabled)
        {
            return new OperationResult<bool>("Jukebox is not enabled")
            {
                Type = OperationResponseType.BadRequest,
                Data = false
            };
        }

        var sessionResult = await GetOrCreateJukeboxSessionAsync(cancellationToken).ConfigureAwait(false);
        if (!sessionResult.IsSuccess || sessionResult.Data == null)
        {
            return new OperationResult<bool>("Failed to get jukebox session")
            {
                Type = OperationResponseType.Error,
                Data = false
            };
        }

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.QueueItems)
            .FirstOrDefaultAsync(x => x.ApiKey == sessionResult.Data.ApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null || !session.QueueItems.Any())
        {
            return new OperationResult<bool> { Data = true };
        }

        var items = session.QueueItems.ToList();
        var random = new Random();
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (items[i].SortOrder, items[j].SortOrder) = (items[j].SortOrder, items[i].SortOrder);
        }

        var newSortOrder = 0;
        foreach (var item in items)
        {
            item.SortOrder = newSortOrder++;
        }

        session.QueueRevision++;
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.Remove($"urn:party:queue:{sessionResult.Data.ApiKey}");

        return new OperationResult<bool> { Data = true };
    }

    private async Task<OperationResult<PartySession>> GetOrCreateJukeboxSessionAsync(CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = await scopedContext.PartySessions
            .Include(x => x.PlaybackState)
            .FirstOrDefaultAsync(x => x.ApiKey == Guid.Parse(JukeboxSessionApiKey), cancellationToken)
            .ConfigureAwait(false);

        if (session != null)
        {
            return new OperationResult<PartySession> { Data = session };
        }

        session = new PartySession
        {
            ApiKey = Guid.Parse(JukeboxSessionApiKey),
            Name = "Subsonic Jukebox",
            OwnerUserId = SystemUserId,
            Status = PartySessionStatus.Active,
            QueueRevision = 0,
            PlaybackRevision = 0,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        scopedContext.PartySessions.Add(session);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Logger.Information("[SubsonicJukeboxService] Created jukebox session");

        return new OperationResult<PartySession> { Data = session };
    }

    private async Task<JukeboxPlaylistResponse> BuildJukeboxPlaylistAsync(Guid sessionApiKey, CancellationToken cancellationToken = default)
    {
        var queueResult = await partyQueueService.GetQueueAsync(sessionApiKey, cancellationToken).ConfigureAwait(false);
        var items = queueResult.Data.Items.ToList();

        var entries = new List<JukeboxEntryResponse>();
        foreach (var item in items.OrderBy(x => x.SortOrder))
        {
            entries.Add(new JukeboxEntryResponse(
                Id: item.SongApiKey.ToString(),
                Parent: item.SongApiKey.ToString(),
                Title: $"Song {item.SongApiKey:N}",
                Artist: "Unknown Artist",
                Album: "Unknown Album",
                Year: 0,
                Genre: null,
                CoverArt: null,
                Duration: 180,
                BitRate: 320,
                Path: $"/song/{item.SongApiKey}",
                TranscodedContentType: null,
                TranscodedSuffix: null,
                IsDir: false,
                IsVideo: false,
                Type: "music"));
        }

        return new JukeboxPlaylistResponse(
            Entries: entries,
            Username: "system",
            Comment: null,
            IsPublic: false,
            SongCount: entries.Count,
            Duration: entries.Sum(e => e.Duration));
    }

    private static int FindCurrentIndex(List<JukeboxEntryResponse> entries, Guid currentItemApiKey)
    {
        var index = entries.FindIndex(e => Guid.TryParse(e.Id, out var id) && id == currentItemApiKey);
        return index >= 0 ? index : 0;
    }
}
