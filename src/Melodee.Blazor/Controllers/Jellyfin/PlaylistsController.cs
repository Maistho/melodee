using System.Security.Cryptography;
using System.Text;
using Melodee.Blazor.Controllers.Jellyfin.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Blazor.Controllers.Jellyfin;

/// <summary>
/// Jellyfin-compatible playlist endpoints.
/// Reuses Melodee's existing PlaylistService for playlist management.
/// </summary>
[ApiController]
[Route("api/jf/[controller]")]
[ApiExplorerSettings(GroupName = "jellyfin")]
[EnableRateLimiting("jellyfin-api")]
public class PlaylistsController(
    EtagRepository etagRepository,
    ISerializer serializer,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> dbContextFactory,
    IClock clock,
    PlaylistService playlistService) : JellyfinControllerBase(etagRepository, serializer, configuration, configurationFactory, dbContextFactory, clock)
{
    /// <summary>
    /// Get all playlists for the authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPlaylistsAsync(
        [FromQuery] string? userId,
        [FromQuery] int? startIndex,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        var skip = Math.Max(0, startIndex ?? 0);
        var take = Math.Clamp(limit ?? 100, 1, 500);

        var userInfo = user.ToUserInfo();
        var pagedRequest = new PagedRequest { Page = 1, PageSize = (short)(take + skip) };

        var playlistResult = await playlistService.ListAsync(userInfo, pagedRequest, cancellationToken);

        var playlists = playlistResult.Data
            .Skip(skip)
            .Take(take)
            .Select(p => MapToJellyfinPlaylist(p, user.HasDownloadRole))
            .ToArray();

        return Ok(new JellyfinItemsResult
        {
            Items = playlists,
            TotalRecordCount = playlistResult.TotalCount,
            StartIndex = skip
        });
    }

    /// <summary>
    /// Get a specific playlist by ID.
    /// </summary>
    [HttpGet("{playlistId}")]
    public async Task<IActionResult> GetPlaylistAsync(string playlistId, CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        if (!TryParseJellyfinGuid(playlistId, out var apiKey))
        {
            return JellyfinBadRequest("Invalid playlist ID format.");
        }

        var userInfo = user.ToUserInfo();
        var playlistResult = await playlistService.GetByApiKeyAsync(userInfo, apiKey, cancellationToken);

        if (!playlistResult.IsSuccess || playlistResult.Data == null)
        {
            return JellyfinNotFound("Playlist not found.");
        }

        var playlist = playlistResult.Data;
        var etag = ComputeEtag(playlist.ApiKey, playlist.LastUpdatedAt ?? playlist.CreatedAt);

        if (IsNotModified(etag))
        {
            return NotModified(etag);
        }

        SetETagHeader(etag);
        return Ok(MapToJellyfinPlaylist(playlist, user.HasDownloadRole));
    }

    /// <summary>
    /// Get items (songs) in a playlist.
    /// </summary>
    [HttpGet("{playlistId}/Items")]
    public async Task<IActionResult> GetPlaylistItemsAsync(
        string playlistId,
        [FromQuery] int? startIndex,
        [FromQuery] int? limit,
        [FromQuery] string? fields,
        [FromQuery] bool? enableUserData,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        if (!TryParseJellyfinGuid(playlistId, out var apiKey))
        {
            return JellyfinBadRequest("Invalid playlist ID format.");
        }

        var skip = Math.Max(0, startIndex ?? 0);
        var take = Math.Clamp(limit ?? 100, 1, 500);

        var userInfo = user.ToUserInfo();
        var pagedRequest = new PagedRequest { Page = 1, PageSize = (short)(take + skip) };

        var songsResult = await playlistService.SongsForPlaylistAsync(apiKey, userInfo, pagedRequest, cancellationToken);

        if (!songsResult.IsSuccess)
        {
            return JellyfinNotFound("Playlist not found.");
        }

        var songs = songsResult.Data
            .Skip(skip)
            .Take(take)
            .Select((s, index) => MapToJellyfinAudioItem(s, user.HasDownloadRole, enableUserData ?? false, skip + index))
            .ToArray();

        return Ok(new JellyfinItemsResult
        {
            Items = songs,
            TotalRecordCount = songsResult.TotalCount,
            StartIndex = skip
        });
    }

    private JellyfinBaseItem MapToJellyfinPlaylist(Common.Data.Models.Playlist playlist, bool canDownload)
    {
        return new JellyfinBaseItem
        {
            Name = playlist.Name,
            ServerId = GetServerId(),
            Id = ToJellyfinId(playlist.ApiKey),
            Etag = ComputeEtag(playlist.ApiKey, playlist.LastUpdatedAt ?? playlist.CreatedAt),
            DateCreated = FormatInstantForJellyfin(playlist.CreatedAt),
            SortName = playlist.Name,
            Overview = playlist.Comment,
            Type = "Playlist",
            IsFolder = true,
            CanDownload = canDownload,
            ChildCount = playlist.SongCount,
            RunTimeTicks = (long)(playlist.Duration * 10_000_000),
            MediaType = "Audio",
            ImageTags = new Dictionary<string, string>(),
            BackdropImageTags = []
        };
    }

    private JellyfinBaseItem MapToJellyfinAudioItem(
        Common.Models.Collection.SongDataInfo song,
        bool canDownload,
        bool enableUserData,
        int playlistIndex)
    {
        var runTimeTicks = (long)(song.Duration * 10_000_000);

        return new JellyfinBaseItem
        {
            Name = song.Title,
            ServerId = GetServerId(),
            Id = ToJellyfinId(song.ApiKey),
            Etag = ComputeEtag(song.ApiKey, song.CreatedAt),
            DateCreated = FormatInstantForJellyfin(song.CreatedAt),
            SortName = song.Title,
            Type = "Audio",
            IsFolder = false,
            CanDownload = canDownload,
            IndexNumber = song.SongNumber,
            ParentIndexNumber = playlistIndex + 1,
            RunTimeTicks = runTimeTicks > 0 ? runTimeTicks : null,
            Album = song.AlbumName,
            AlbumId = ToJellyfinId(song.AlbumApiKey),
            Artists = !string.IsNullOrWhiteSpace(song.ArtistName) ? [song.ArtistName] : null,
            ArtistItems = !string.IsNullOrWhiteSpace(song.ArtistName)
                ? [new JellyfinNameGuidPair { Name = song.ArtistName, Id = ToJellyfinId(song.ArtistApiKey) }]
                : null,
            MediaType = "Audio",
            ImageTags = new Dictionary<string, string>(),
            BackdropImageTags = [],
            UserData = enableUserData ? new JellyfinUserItemData
            {
                Key = song.ApiKey.ToString("N"),
                IsFavorite = song.UserStarred,
                PlayCount = song.PlayedCount,
                Played = song.PlayedCount > 0
            } : null
        };
    }

    private static string ComputeEtag(Guid apiKey, Instant lastUpdated)
    {
        var input = $"{apiKey:N}-{lastUpdated.ToUnixTimeTicks()}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

