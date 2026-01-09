using System.Net;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Models.Streaming;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

[ApiController]
[Route("[controller]")]
[OpenSubsonicEndpoint]
public class PodcastController(
    ISerializer serializer,
    EtagRepository etagRepository,
    IMelodeeConfigurationFactory configurationFactory,
    PodcastService podcastService,
    LibraryService libraryService) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    [HttpGet]
    [HttpPost]
    [Route("/rest/getPodcasts.view")]
    [Route("/rest/getPodcasts")]
    public async Task<IActionResult> GetPodcastsAsync(
        string? id = null,
        bool includeEpisodes = true,
        CancellationToken cancellationToken = default)
    {
        var auth = await AuthenticateAsync(cancellationToken);
        if (!auth.IsSuccess)
        {
            return AuthFailed();
        }

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
        {
            return NotSupported();
        }

        if (!auth.UserInfo.User.HasPodcastRole)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, CreateResponse(new Error { Code = 10, Message = "User role not allowed" }));
        }

        try
        {
            List<PodcastChannel> channels;

            if (id.Nullify() != null)
            {
                var channelId = ParsePodcastChannelId(id);
                if (channelId == null)
                {
                    return BadRequest(CreateResponse(new Error { Code = 0, Message = "Invalid channel id" }));
                }

                var channelResult = await podcastService.GetChannelAsync(channelId.Value, auth.UserInfo.User.Id, cancellationToken).ConfigureAwait(false);
                if (!channelResult.IsSuccess || channelResult.Data == null)
                {
                    return NotFound(CreateResponse(new Error { Code = 70, Message = "Podcast not found" }));
                }

                channels = [channelResult.Data];
            }
            else
            {
                var result = await podcastService.ListChannelsAsync(auth.UserInfo.User.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
                channels = result.Data?.ToList() ?? [];
            }

            var podcasts = new List<PodcastChannelResponse>();

            foreach (var channel in channels)
            {
                var podcast = new PodcastChannelResponse
                {
                    Id = $"podcast:channel:{channel.Id}",
                    Title = channel.Title,
                    Description = channel.Description,
                    Url = channel.FeedUrl,
                    CoverArt = channel.CoverArtLocalPath.Nullify() != null ? $"podcast:channel:{channel.Id}" : null,
                    OriginalImageUrl = channel.ImageUrl
                };

                if (includeEpisodes)
                {
                    var episodesResult = await podcastService.ListEpisodesAsync(channel.Id, auth.UserInfo.User.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
                    podcast.Episode = episodesResult.Data?.Select(e => e.ToPodcastEpisodeResponse()).ToList() ?? [];
                }

                podcasts.Add(podcast);
            }

            var response = new PodcastsResponse
            {
                Podcasts = new PodcastsContainer { Channel = podcasts }
            };

            return Ok(CreateResponse(response));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{Controller}] Error in GetPodcasts", nameof(PodcastController));
            return StatusCode((int)HttpStatusCode.InternalServerError, CreateResponse(new Error { Message = "Internal server error" }));
        }
    }

    [HttpGet]
    [HttpPost]
    [Route("/rest/getNewestPodcasts.view")]
    [Route("/rest/getNewestPodcasts")]
    public async Task<IActionResult> GetNewestPodcastsAsync(
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        var auth = await AuthenticateAsync(cancellationToken);
        if (!auth.IsSuccess)
        {
            return AuthFailed();
        }

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
        {
            return NotSupported();
        }

        if (!auth.UserInfo.User.HasPodcastRole)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, CreateResponse(new Error { Code = 10, Message = "User role not allowed" }));
        }

        try
        {
            var result = await podcastService.GetNewestEpisodesAsync(auth.UserInfo.User.Id, count, cancellationToken).ConfigureAwait(false);

            var episodes = result.Data?.Select(e => e.ToPodcastEpisodeResponse()).ToList() ?? [];

            var response = new NewestPodcastsResponse
            {
                NewestPodcasts = new NewestPodcastsContainer { Episode = episodes }
            };

            return Ok(CreateResponse(response));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{Controller}] Error in GetNewestPodcasts", nameof(PodcastController));
            return StatusCode((int)HttpStatusCode.InternalServerError, CreateResponse(new Error { Message = "Internal server error" }));
        }
    }

    [HttpGet]
    [HttpPost]
    [Route("/rest/refreshPodcasts.view")]
    [Route("/rest/refreshPodcasts")]
    public async Task<IActionResult> RefreshPodcastsAsync(CancellationToken cancellationToken = default)
    {
        var auth = await AuthenticateAsync(cancellationToken);
        if (!auth.IsSuccess)
        {
            return AuthFailed();
        }

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
        {
            return NotSupported();
        }

        if (!auth.UserInfo.User.HasPodcastRole)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, CreateResponse(new Error { Code = 10, Message = "User role not allowed" }));
        }

        try
        {
            var result = await podcastService.ListChannelsAsync(auth.UserInfo.User.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            var channels = result.Data?.ToList() ?? [];

            await using var context = await podcastService.CreateDbContextAsync(cancellationToken);
            foreach (var channel in channels)
            {
                channel.NextSyncAt = null;
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Ok(CreateResponse(new StatusResponse { Status = "ok" }));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{Controller}] Error in RefreshPodcasts", nameof(PodcastController));
            return StatusCode((int)HttpStatusCode.InternalServerError, CreateResponse(new Error { Message = "Internal server error" }));
        }
    }

    [HttpGet]
    [HttpPost]
    [Route("/rest/createPodcastChannel.view")]
    [Route("/rest/createPodcastChannel")]
    public async Task<IActionResult> CreatePodcastChannelAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        var auth = await AuthenticateAsync(cancellationToken);
        if (!auth.IsSuccess)
        {
            return AuthFailed();
        }

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
        {
            return NotSupported();
        }

        if (!auth.UserInfo.User.HasPodcastRole)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, CreateResponse(new Error { Code = 10, Message = "User role not allowed" }));
        }

        if (url.Nullify() == null)
        {
            return BadRequest(CreateResponse(new Error { Code = 0, Message = "Missing url parameter" }));
        }

        try
        {
            var result = await podcastService.CreateChannelAsync(auth.UserInfo.User.Id, url, cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                return BadRequest(CreateResponse(new Error { Message = result.Message }));
            }

            var channel = result.Data!;
            var response = new PodcastChannelResponse
            {
                Id = $"podcast:channel:{channel.Id}",
                Title = channel.Title,
                Description = channel.Description,
                Url = channel.FeedUrl
            };

            return Ok(CreateResponse(response));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{Controller}] Error in CreatePodcastChannel", nameof(PodcastController));
            return StatusCode((int)HttpStatusCode.InternalServerError, CreateResponse(new Error { Message = "Internal server error" }));
        }
    }

    [HttpGet]
    [HttpPost]
    [Route("/rest/deletePodcastChannel.view")]
    [Route("/rest/deletePodcastChannel")]
    public async Task<IActionResult> DeletePodcastChannelAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await AuthenticateAsync(cancellationToken);
        if (!auth.IsSuccess)
        {
            return AuthFailed();
        }

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
        {
            return NotSupported();
        }

        if (!auth.UserInfo.User.HasPodcastRole)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, CreateResponse(new Error { Code = 10, Message = "User role not allowed" }));
        }

        var channelId = ParsePodcastChannelId(id);
        if (channelId == null)
        {
            return BadRequest(CreateResponse(new Error { Code = 0, Message = "Invalid channel id" }));
        }

        try
        {
            var result = await podcastService.DeleteChannelAsync(channelId.Value, auth.UserInfo.User.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                return NotFound(CreateResponse(new Error { Message = result.Message }));
            }

            return Ok(CreateResponse(new StatusResponse { Status = "ok" }));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{Controller}] Error in DeletePodcastChannel", nameof(PodcastController));
            return StatusCode((int)HttpStatusCode.InternalServerError, CreateResponse(new Error { Message = "Internal server error" }));
        }
    }

    [HttpGet]
    [HttpPost]
    [Route("/rest/deletePodcastEpisode.view")]
    [Route("/rest/deletePodcastEpisode")]
    public async Task<IActionResult> DeletePodcastEpisodeAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await AuthenticateAsync(cancellationToken);
        if (!auth.IsSuccess)
        {
            return AuthFailed();
        }

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
        {
            return NotSupported();
        }

        if (!auth.UserInfo.User.HasPodcastRole)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, CreateResponse(new Error { Code = 10, Message = "User role not allowed" }));
        }

        var episodeId = ParsePodcastEpisodeId(id);
        if (episodeId == null)
        {
            return BadRequest(CreateResponse(new Error { Code = 0, Message = "Invalid episode id" }));
        }

        try
        {
            var result = await podcastService.DeleteEpisodeAsync(episodeId.Value, auth.UserInfo.User.Id, cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                return NotFound(CreateResponse(new Error { Message = result.Message }));
            }

            return Ok(CreateResponse(new StatusResponse { Status = "ok" }));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{Controller}] Error in DeletePodcastEpisode", nameof(PodcastController));
            return StatusCode((int)HttpStatusCode.InternalServerError, CreateResponse(new Error { Message = "Internal server error" }));
        }
    }

    [HttpGet]
    [HttpPost]
    [Route("/rest/downloadPodcastEpisode.view")]
    [Route("/rest/downloadPodcastEpisode")]
    public async Task<IActionResult> DownloadPodcastEpisodeAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await AuthenticateAsync(cancellationToken);
        if (!auth.IsSuccess)
        {
            return AuthFailed();
        }

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
        {
            return NotSupported();
        }

        if (!auth.UserInfo.User.HasPodcastRole)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, CreateResponse(new Error { Code = 10, Message = "User role not allowed" }));
        }

        var episodeId = ParsePodcastEpisodeId(id);
        if (episodeId == null)
        {
            return BadRequest(CreateResponse(new Error { Code = 0, Message = "Invalid episode id" }));
        }

        try
        {
            var result = await podcastService.QueueDownloadAsync(episodeId.Value, auth.UserInfo.User.Id, cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                return BadRequest(CreateResponse(new Error { Message = result.Message }));
            }

            return Ok(CreateResponse(new StatusResponse { Status = "ok" }));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{Controller}] Error in DownloadPodcastEpisode", nameof(PodcastController));
            return StatusCode((int)HttpStatusCode.InternalServerError, CreateResponse(new Error { Message = "Internal server error" }));
        }
    }

    [HttpGet]
    [HttpPost]
    [Route("/rest/streamPodcastEpisode.view")]
    [Route("/rest/streamPodcastEpisode")]
    public async Task<IActionResult> StreamPodcastEpisodeAsync(
        string id,
        string? format = null,
        string? filename = null,
        CancellationToken cancellationToken = default)
    {
        var auth = await AuthenticateAsync(cancellationToken);
        if (!auth.IsSuccess)
        {
            return AuthFailed();
        }

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.PodcastEnabled))
        {
            return NotSupported();
        }

        if (!auth.UserInfo.User.HasStreamRole)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, CreateResponse(new Error { Code = 10, Message = "User role not allowed" }));
        }

        var episodeId = ParsePodcastEpisodeId(id);
        if (episodeId == null)
        {
            return BadRequest(CreateResponse(new Error { Code = 0, Message = "Invalid episode id" }));
        }

        try
        {
            var episodeResult = await podcastService.GetEpisodeAsync(episodeId.Value, auth.UserInfo.User.Id, cancellationToken).ConfigureAwait(false);

            if (!episodeResult.IsSuccess || episodeResult.Data == null)
            {
                return NotFound(CreateResponse(new Error { Code = 70, Message = "Episode not found" }));
            }

            var episode = episodeResult.Data;

            if (episode.DownloadStatus != PodcastEpisodeDownloadStatus.Downloaded || episode.LocalPath.Nullify() == null)
            {
                return BadRequest(CreateResponse(new Error { Message = "Episode not downloaded" }));
            }

            var podcastLibraryResult = await libraryService.GetPodcastLibraryAsync(cancellationToken).ConfigureAwait(false);
            var podcastLibrary = podcastLibraryResult.Data;
            if (podcastLibrary == null)
            {
                return NotFound(CreateResponse(new Error { Message = "Podcast library not configured" }));
            }

            var filePath = Path.Combine(podcastLibrary.Path, episode.LocalPath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(CreateResponse(new Error { Message = "Episode file not found" }));
            }

            var fileInfo = new FileInfo(filePath);
            var rangeHeader = Request.Headers["Range"].FirstOrDefault();

            StreamingDescriptor descriptor;

            if (rangeHeader.Nullify() != null)
            {
                var range = RangeParser.ParseRange(rangeHeader!, fileInfo.Length);
                if (range == null || !range.IsValidForFileSize(fileInfo.Length))
                {
                    Response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
                    return new EmptyResult();
                }

                descriptor = new StreamingDescriptor
                {
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    ContentType = episode.MimeType ?? "audio/mpeg",
                    ResponseHeaders = new Dictionary<string, StringValues>(),
                    Range = range,
                    FileName = episode.Title
                };
            }
            else
            {
                descriptor = new StreamingDescriptor
                {
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    ContentType = episode.MimeType ?? "audio/mpeg",
                    ResponseHeaders = new Dictionary<string, StringValues>(),
                    FileName = episode.Title
                };
            }

            var statusCode = descriptor.Range != null ? 206 : 200;

            foreach (var header in RangeParser.CreateResponseHeaders(descriptor, statusCode))
            {
                Response.Headers[header.Key] = header.Value;
            }

            Response.StatusCode = statusCode;

            if (descriptor.Range != null)
            {
                var fileStream = new FileStream(descriptor.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: 65536, FileOptions.Asynchronous | FileOptions.SequentialScan);

                fileStream.Seek(descriptor.Range.Start, SeekOrigin.Begin);
                var rangeStream = new BoundedStream(fileStream, descriptor.Range.GetContentLength(descriptor.FileSize));

                return new FileStreamResult(rangeStream, descriptor.ContentType)
                {
                    EnableRangeProcessing = true,
                    FileDownloadName = descriptor.FileName
                };
            }

            return new PhysicalFileResult(descriptor.FilePath, descriptor.ContentType)
            {
                EnableRangeProcessing = true,
                FileDownloadName = descriptor.FileName
            };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{Controller}] Error in StreamPodcastEpisode", nameof(PodcastController));
            return StatusCode((int)HttpStatusCode.InternalServerError, CreateResponse(new Error { Message = "Internal server error" }));
        }
    }

    private static int? ParsePodcastChannelId(string id)
    {
        if (id.StartsWith("podcast:channel:", StringComparison.OrdinalIgnoreCase))
        {
            var idPart = id["podcast:channel:".Length..];
            if (int.TryParse(idPart, out var channelId))
            {
                return channelId;
            }
        }

        return null;
    }

    private static int? ParsePodcastEpisodeId(string id)
    {
        if (id.StartsWith("podcast:episode:", StringComparison.OrdinalIgnoreCase))
        {
            var idPart = id["podcast:episode:".Length..];
            if (int.TryParse(idPart, out var episodeId))
            {
                return episodeId;
            }
        }

        return null;
    }
}

file: /home/steven/source/melodee/src/Melodee.Blazor/Controllers/OpenSubsonic/PodcastController.cs
