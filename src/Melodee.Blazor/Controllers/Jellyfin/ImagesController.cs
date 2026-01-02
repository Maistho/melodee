using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Blazor.Controllers.Jellyfin;

/// <summary>
/// Jellyfin-compatible image endpoints.
/// Reuses Melodee's existing image services (AlbumService, ArtistService) which handle
/// caching, resizing, and file management. This controller only provides Jellyfin-style
/// routes and determines entity types from raw GUIDs.
/// </summary>
[ApiController]
[Route("api/jf")]
[ApiExplorerSettings(GroupName = "jellyfin")]
[EnableRateLimiting("jellyfin-api")]
public class ImagesController(
    EtagRepository etagRepository,
    ISerializer serializer,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> dbContextFactory,
    IClock clock,
    AlbumService albumService,
    ArtistService artistService,
    ILogger<ImagesController> logger) : JellyfinControllerBase(etagRepository, serializer, configuration, configurationFactory, dbContextFactory, clock)
{

    /// <summary>
    /// Get item image by type.
    /// </summary>
    [HttpGet("Items/{itemId}/Images/{imageType}")]
    [HttpHead("Items/{itemId}/Images/{imageType}")]
    public async Task<IActionResult> GetItemImageAsync(
        string itemId,
        string imageType,
        [FromQuery] int? maxWidth,
        [FromQuery] int? maxHeight,
        [FromQuery] int? width,
        [FromQuery] int? height,
        [FromQuery] string? tag,
        [FromQuery] string? format,
        CancellationToken cancellationToken)
    {
        return await GetImageInternalAsync(itemId, imageType, 0, maxWidth, maxHeight, cancellationToken);
    }

    /// <summary>
    /// Get item image by type and index.
    /// </summary>
    [HttpGet("Items/{itemId}/Images/{imageType}/{imageIndex:int}")]
    [HttpHead("Items/{itemId}/Images/{imageType}/{imageIndex:int}")]
    public async Task<IActionResult> GetItemImageByIndexAsync(
        string itemId,
        string imageType,
        int imageIndex,
        [FromQuery] int? maxWidth,
        [FromQuery] int? maxHeight,
        [FromQuery] int? width,
        [FromQuery] int? height,
        [FromQuery] string? tag,
        [FromQuery] string? format,
        CancellationToken cancellationToken)
    {
        return await GetImageInternalAsync(itemId, imageType, imageIndex, maxWidth, maxHeight, cancellationToken);
    }

    /// <summary>
    /// Get artist image.
    /// </summary>
    [HttpGet("Artists/{artistId}/Images/{imageType}")]
    [HttpHead("Artists/{artistId}/Images/{imageType}")]
    public async Task<IActionResult> GetArtistImageAsync(
        string artistId,
        string imageType,
        [FromQuery] int? maxWidth,
        [FromQuery] int? maxHeight,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        if (!TryParseJellyfinGuid(artistId, out var apiKey))
        {
            return JellyfinBadRequest("Invalid artist ID format.");
        }

        var size = DetermineSizeFromDimensions(maxWidth, maxHeight);
        var imageResult = await artistService.GetArtistImageBytesAndEtagAsync(apiKey, size, cancellationToken);

        if (imageResult.Bytes == null || imageResult.Bytes.Length == 0)
        {
            return JellyfinNotFound("Artist image not found.");
        }

        return ReturnImageResult(imageResult.Bytes, imageResult.Etag);
    }

    private async Task<IActionResult> GetImageInternalAsync(
        string itemId,
        string imageType,
        int imageIndex,
        int? maxWidth,
        int? maxHeight,
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

        var size = DetermineSizeFromDimensions(maxWidth, maxHeight);

        await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        // Check if this is an artist
        var artist = await dbContext.Artists
            .AsNoTracking()
            .Where(a => a.ApiKey == apiKey && !a.IsLocked)
            .Select(a => new { a.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (artist != null)
        {
            var artistImageResult = await artistService.GetArtistImageBytesAndEtagAsync(apiKey, size, cancellationToken);
            if (artistImageResult.Bytes == null || artistImageResult.Bytes.Length == 0)
            {
                return JellyfinNotFound("Artist image not found.");
            }
            return ReturnImageResult(artistImageResult.Bytes, artistImageResult.Etag);
        }

        // Check if this is an album
        var album = await dbContext.Albums
            .AsNoTracking()
            .Where(a => a.ApiKey == apiKey && !a.IsLocked)
            .Select(a => new { a.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (album != null)
        {
            var albumImageResult = await albumService.GetAlbumImageBytesAndEtagAsync(apiKey, size, cancellationToken);
            if (albumImageResult.Bytes == null || albumImageResult.Bytes.Length == 0)
            {
                return JellyfinNotFound("Album image not found.");
            }
            return ReturnImageResult(albumImageResult.Bytes, albumImageResult.Etag);
        }

        // Check if this is a song (use album art)
        var song = await dbContext.Songs
            .AsNoTracking()
            .Include(s => s.Album)
            .Where(s => s.ApiKey == apiKey && !s.IsLocked)
            .Select(s => new { s.Album.ApiKey })
            .FirstOrDefaultAsync(cancellationToken);

        if (song != null)
        {
            var songAlbumImageResult = await albumService.GetAlbumImageBytesAndEtagAsync(song.ApiKey, size, cancellationToken);
            if (songAlbumImageResult.Bytes == null || songAlbumImageResult.Bytes.Length == 0)
            {
                return JellyfinNotFound("Song album image not found.");
            }
            return ReturnImageResult(songAlbumImageResult.Bytes, songAlbumImageResult.Etag);
        }

        logger.LogDebug("JellyfinImageNotFound ItemId={ItemId} ImageType={ImageType}", itemId, imageType);
        return JellyfinNotFound("Item not found.");
    }

    private IActionResult ReturnImageResult(byte[] imageBytes, string? etag)
    {
        var contentType = DetectContentType(imageBytes);

        if (!string.IsNullOrWhiteSpace(etag) && IsNotModified(etag))
        {
            return NotModified(etag);
        }

        if (!string.IsNullOrWhiteSpace(etag))
        {
            SetETagHeader(etag);
        }

        Response.Headers.CacheControl = "public, max-age=86400";

        if (Request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
        {
            Response.ContentType = contentType;
            Response.ContentLength = imageBytes.Length;
            return Ok();
        }

        return File(imageBytes, contentType);
    }

    private static string DetermineSizeFromDimensions(int? maxWidth, int? maxHeight)
    {
        var maxDimension = Math.Max(maxWidth ?? 0, maxHeight ?? 0);

        return maxDimension switch
        {
            0 => "Large",
            <= 100 => "Small",
            <= 300 => "Medium",
            _ => "Large"
        };
    }

    private static string DetectContentType(byte[] imageBytes)
    {
        if (imageBytes.Length < 8)
        {
            return "image/jpeg";
        }

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
        {
            return "image/png";
        }

        // JPEG: FF D8 FF
        if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
        {
            return "image/jpeg";
        }

        // GIF: 47 49 46 38
        if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x38)
        {
            return "image/gif";
        }

        // WebP: 52 49 46 46 ... 57 45 42 50
        if (imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x46 &&
            imageBytes.Length > 11 && imageBytes[8] == 0x57 && imageBytes[9] == 0x45 && imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
        {
            return "image/webp";
        }

        // BMP: 42 4D
        if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
        {
            return "image/bmp";
        }

        return "image/jpeg";
    }
}

