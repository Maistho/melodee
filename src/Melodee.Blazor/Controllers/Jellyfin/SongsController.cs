using Melodee.Blazor.Controllers.Jellyfin.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Blazor.Controllers.Jellyfin;

/// <summary>
/// Jellyfin-compatible Songs endpoint. Provides InstantMix (radio) functionality for song-based mixes.
/// Used by Feishin and other clients that call /Songs/{itemId}/InstantMix.
/// </summary>
[ApiController]
[Route("api/jf/[controller]")]
[ApiExplorerSettings(GroupName = "jellyfin")]
[EnableRateLimiting("jellyfin-api")]
public class SongsController(
    EtagRepository etagRepository,
    ISerializer serializer,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> dbContextFactory,
    IClock clock,
    ILoggerFactory loggerFactory) : JellyfinControllerBase(etagRepository, serializer, configuration, configurationFactory, dbContextFactory, clock, loggerFactory)
{
    /// <summary>
    /// Gets instant mix (radio) based on a song. Used by Feishin for the "Instant Mix" feature.
    /// Returns a shuffled selection of similar songs based on genre and artist.
    /// </summary>
    [HttpGet("{itemId}/InstantMix")]
    public async Task<IActionResult> GetInstantMixAsync(
        string itemId,
        [FromQuery] string? userId,
        [FromQuery] int? limit,
        [FromQuery] string? fields,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        if (!TryParseJellyfinGuid(itemId, out var apiKey))
        {
            return JellyfinBadRequest("Invalid item ID format.");
        }

        var maxItems = Math.Clamp(limit ?? 200, 1, 300);

        await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var seedSong = await dbContext.Songs
            .AsNoTracking()
            .Include(s => s.Album)
            .Where(s => s.ApiKey == apiKey && !s.IsLocked)
            .FirstOrDefaultAsync(cancellationToken);

        if (seedSong == null)
        {
            return JellyfinNotFound("Song not found.");
        }

        var genres = seedSong.Album?.Genres ?? [];
        var artistId = seedSong.Album?.ArtistId;

        var mixQuery = dbContext.Songs
            .AsNoTracking()
            .Include(s => s.Album)
            .ThenInclude(a => a.Artist)
            .Where(s => !s.IsLocked && !s.Album.IsLocked && s.Id != seedSong.Id);

        if (artistId.HasValue)
        {
            mixQuery = mixQuery.Where(s => s.Album.ArtistId == artistId || 
                (s.Album.Genres != null && genres.Any(g => s.Album.Genres.Contains(g))));
        }

        var songs = await mixQuery
            .OrderBy(s => EF.Functions.Random())
            .Take(maxItems)
            .Select(s => new
            {
                s.ApiKey,
                s.Title,
                s.SongNumber,
                s.Duration,
                s.CreatedAt,
                AlbumApiKey = s.Album.ApiKey,
                AlbumName = s.Album.Name,
                AlbumYear = (int?)s.Album.ReleaseDate.Year,
                ArtistApiKey = s.Album.Artist.ApiKey,
                ArtistName = s.Album.Artist.Name,
                s.Album.Genres
            })
            .ToListAsync(cancellationToken);

        var items = songs.Select(s => new JellyfinBaseItem
        {
            Name = s.Title,
            ServerId = GetServerId(),
            Id = ToJellyfinId(s.ApiKey),
            DateCreated = FormatInstantForJellyfin(s.CreatedAt),
            SortName = s.Title,
            Type = "Audio",
            IsFolder = false,
            CanDownload = user.HasDownloadRole,
            RunTimeTicks = (long)(s.Duration * 10_000_000),
            IndexNumber = s.SongNumber,
            Album = s.AlbumName,
            AlbumId = ToJellyfinId(s.AlbumApiKey),
            AlbumArtist = s.ArtistName,
            Artists = [s.ArtistName],
            ArtistItems = [new JellyfinNameGuidPair { Name = s.ArtistName, Id = ToJellyfinId(s.ArtistApiKey) }],
            AlbumArtists = [new JellyfinNameGuidPair { Name = s.ArtistName, Id = ToJellyfinId(s.ArtistApiKey) }],
            ProductionYear = s.AlbumYear,
            Genres = s.Genres?.ToArray(),
            MediaType = "Audio",
            ImageTags = new Dictionary<string, string> { ["Primary"] = ToJellyfinId(s.AlbumApiKey) },
            BackdropImageTags = []
        }).ToArray();

        return Ok(new JellyfinItemsResult
        {
            Items = items,
            TotalRecordCount = items.Length,
            StartIndex = 0
        });
    }
}
