using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Blazor.Controllers.Jellyfin;

[ApiController]
[Route("api/jf/[controller]")]
[ApiExplorerSettings(GroupName = "jellyfin")]
[EnableRateLimiting("jellyfin-stream")]
public class AudioController(
    EtagRepository etagRepository,
    ISerializer serializer,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> dbContextFactory,
    IClock clock,
    ILoggerFactory loggerFactory,
    ILogger<AudioController> logger) : JellyfinControllerBase(etagRepository, serializer, configuration, configurationFactory, dbContextFactory, clock, loggerFactory)
{
    private const int StreamBufferSize = 65536;

    [HttpGet("{itemId}/stream")]
    [HttpHead("{itemId}/stream")]
    public async Task<IActionResult> StreamAudioAsync(
        string itemId,
        [FromQuery] string? container,
        [FromQuery] bool? @static,
        [FromQuery] long? startTimeTicks,
        [FromQuery] int? audioBitRate,
        CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null)
        {
            return JellyfinUnauthorized();
        }

        if (!user.HasStreamRole)
        {
            return JellyfinForbidden("Streaming permission required.");
        }

        if (!TryParseJellyfinGuid(itemId, out var apiKey))
        {
            return JellyfinBadRequest("Invalid item ID format.");
        }

        await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        var song = await dbContext.Songs
            .AsNoTracking()
            .Include(s => s.Album)
            .ThenInclude(a => a.Artist)
            .ThenInclude(ar => ar.Library)
            .Where(s => s.ApiKey == apiKey && !s.IsLocked)
            .Select(s => new
            {
                s.Title,
                s.FileName,
                s.Duration,
                s.BitRate,
                AlbumDirectory = s.Album.Directory,
                ArtistDirectory = s.Album.Artist.Directory,
                LibraryPath = s.Album.Artist.Library.Path
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (song == null)
        {
            return JellyfinNotFound("Audio item not found.");
        }

        var filePath = Path.Combine(song.LibraryPath, song.ArtistDirectory, song.AlbumDirectory, song.FileName);
        if (!System.IO.File.Exists(filePath))
        {
            logger.LogWarning("JellyfinStreamFileNotFound ItemId={ItemId} FilePath={FilePath}", itemId, filePath);
            return JellyfinNotFound("Audio file not found.");
        }

        if (@static != true && (audioBitRate.HasValue || !string.IsNullOrWhiteSpace(container)))
        {
            logger.LogDebug("JellyfinTranscodingNotSupported ItemId={ItemId} Container={Container} BitRate={BitRate}",
                itemId, container ?? "null", audioBitRate?.ToString() ?? "null");
            return JellyfinBadRequest("Transcoding is not supported. Use static=true for direct streaming.");
        }

        var fileInfo = new FileInfo(filePath);
        var contentType = GetContentTypeForFile(filePath);

        Response.Headers.Append("Accept-Ranges", "bytes");

        logger.LogInformation("JellyfinStreamStart UserId={UserId} ItemId={ItemId} Title={Title}",
            user.Id, itemId, song.Title);

        if (Request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
        {
            Response.ContentType = contentType;
            Response.ContentLength = fileInfo.Length;
            return Ok();
        }

        if (Request.Headers.TryGetValue("Range", out var rangeHeader))
        {
            return await HandleRangeRequestAsync(filePath, fileInfo.Length, rangeHeader.ToString(), contentType, user.Id, itemId, cancellationToken);
        }

        return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
    }

    [HttpGet("{itemId}/stream.{extension}")]
    [HttpHead("{itemId}/stream.{extension}")]
    public Task<IActionResult> StreamAudioWithContainerAsync(
        string itemId,
        string extension,
        [FromQuery] bool? @static,
        [FromQuery] long? startTimeTicks,
        [FromQuery] int? audioBitRate,
        CancellationToken cancellationToken)
    {
        return StreamAudioAsync(itemId, extension, @static, startTimeTicks, audioBitRate, cancellationToken);
    }

    [HttpGet("{itemId}/universal")]
    [HttpHead("{itemId}/universal")]
    public Task<IActionResult> UniversalAudioAsync(
        string itemId,
        [FromQuery] string? container,
        [FromQuery] string? audioCodec,
        [FromQuery] string? transcodingContainer,
        [FromQuery] string? transcodingProtocol,
        [FromQuery] int? maxStreamingBitrate,
        [FromQuery] string? userId,
        [FromQuery(Name = "api_key")] string? apiKey,
        CancellationToken cancellationToken)
    {
        return StreamAudioAsync(itemId, container, true, null, maxStreamingBitrate, cancellationToken);
    }

    /// <summary>
    /// Get lyrics for an audio item.
    /// </summary>
    [HttpGet("{itemId}/Lyrics")]
    public async Task<IActionResult> GetLyricsAsync(string itemId, CancellationToken cancellationToken)
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

        await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        var song = await dbContext.Songs
            .AsNoTracking()
            .Include(s => s.Album)
            .ThenInclude(a => a.Artist)
            .ThenInclude(ar => ar.Library)
            .Where(s => s.ApiKey == apiKey && !s.IsLocked)
            .Select(s => new
            {
                s.FileName,
                AlbumDirectory = s.Album.Directory,
                ArtistDirectory = s.Album.Artist.Directory,
                LibraryPath = s.Album.Artist.Library.Path
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (song == null)
        {
            return JellyfinNotFound("Audio item not found.");
        }

        var audioFilePath = Path.Combine(song.LibraryPath, song.ArtistDirectory, song.AlbumDirectory, song.FileName);
        var lrcFilePath = Path.ChangeExtension(audioFilePath, ".lrc");

        if (!System.IO.File.Exists(lrcFilePath))
        {
            return Ok(new { Lyrics = Array.Empty<object>() });
        }

        var lyrics = new List<object>();
        var lines = await System.IO.File.ReadAllLinesAsync(lrcFilePath, cancellationToken);

        foreach (var line in lines)
        {
            var match = System.Text.RegularExpressions.Regex.Match(line, @"\[(\d{2}):(\d{2})\.(\d{2,3})\](.*)");
            if (match.Success)
            {
                var minutes = int.Parse(match.Groups[1].Value);
                var seconds = int.Parse(match.Groups[2].Value);
                var milliseconds = match.Groups[3].Value;
                if (milliseconds.Length == 2) milliseconds += "0";
                var ms = int.Parse(milliseconds);

                var startTicks = ((minutes * 60L + seconds) * 1000 + ms) * 10000;
                var text = match.Groups[4].Value.Trim();

                if (!string.IsNullOrEmpty(text))
                {
                    lyrics.Add(new { Text = text, Start = startTicks });
                }
            }
            else if (!line.StartsWith("[") && !string.IsNullOrWhiteSpace(line))
            {
                lyrics.Add(new { Text = line.Trim(), Start = (long?)null });
            }
        }

        return Ok(new { Lyrics = lyrics });
    }

    private async Task<IActionResult> HandleRangeRequestAsync(
        string filePath,
        long fileLength,
        string rangeHeader,
        string contentType,
        int userId,
        string itemId,
        CancellationToken cancellationToken)
    {
        if (!rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
        {
            return JellyfinRangeNotSatisfiable();
        }

        var rangeSpec = rangeHeader[6..];
        var dashIndex = rangeSpec.IndexOf('-');
        if (dashIndex < 0)
        {
            return JellyfinRangeNotSatisfiable();
        }

        long start;
        long end;

        var startPart = rangeSpec[..dashIndex];
        var endPart = rangeSpec[(dashIndex + 1)..];

        if (string.IsNullOrEmpty(startPart))
        {
            if (!long.TryParse(endPart, out var suffixLength) || suffixLength <= 0)
            {
                return JellyfinRangeNotSatisfiable();
            }
            start = Math.Max(0, fileLength - suffixLength);
            end = fileLength - 1;
        }
        else
        {
            if (!long.TryParse(startPart, out start) || start < 0 || start >= fileLength)
            {
                Response.Headers.Append("Content-Range", $"bytes */{fileLength}");
                return JellyfinRangeNotSatisfiable();
            }
            end = string.IsNullOrEmpty(endPart) ? fileLength - 1 : long.TryParse(endPart, out var parsedEnd) ? Math.Min(parsedEnd, fileLength - 1) : fileLength - 1;
        }

        if (start > end)
        {
            Response.Headers.Append("Content-Range", $"bytes */{fileLength}");
            return JellyfinRangeNotSatisfiable();
        }

        var length = end - start + 1;
        Response.StatusCode = StatusCodes.Status206PartialContent;
        Response.Headers.Append("Content-Range", $"bytes {start}-{end}/{fileLength}");
        Response.Headers.Append("Content-Length", length.ToString());
        Response.ContentType = contentType;

        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, StreamBufferSize, true);
        fileStream.Seek(start, SeekOrigin.Begin);

        var buffer = new byte[StreamBufferSize];
        var remaining = length;
        var bytesSent = 0L;
        var streamCanceled = false;

        try
        {
            while (remaining > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    streamCanceled = true;
                    break;
                }

                var toRead = (int)Math.Min(buffer.Length, remaining);
                var read = await fileStream.ReadAsync(buffer.AsMemory(0, toRead), cancellationToken);
                if (read == 0) break;

                await Response.Body.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                bytesSent += read;
                remaining -= read;
            }
        }
        catch (OperationCanceledException)
        {
            streamCanceled = true;
        }
        catch (IOException)
        {
            streamCanceled = true;
        }

        if (streamCanceled)
        {
            logger.LogInformation(
                "JellyfinStreamCanceled UserId={UserId} ItemId={ItemId} BytesSent={BytesSent} TotalBytes={TotalBytes} Reason={Reason}",
                userId, itemId, bytesSent, length, "ClientDisconnect");
        }
        else if (bytesSent == length)
        {
            logger.LogDebug(
                "JellyfinStreamComplete UserId={UserId} ItemId={ItemId} BytesSent={BytesSent}",
                userId, itemId, bytesSent);
        }

        return new EmptyResult();
    }

    private static string GetContentTypeForFile(string filePath)
    {
        var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
        return ext switch
        {
            ".mp3" => "audio/mpeg",
            ".flac" => "audio/flac",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            ".aac" => "audio/aac",
            ".wav" => "audio/wav",
            ".opus" => "audio/opus",
            _ => "application/octet-stream"
        };
    }
}
