using System.Net;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

/// <summary>
/// Jukebox control endpoints.
/// As per ADR-0001, Melodee does not implement server-side jukebox playback.
/// All jukebox endpoints return HTTP 410 Gone to indicate intentional non-support.
/// </summary>
public class JukeboxController(ISerializer serializer, EtagRepository etagRepository, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    /// <summary>
    /// Controls the jukebox, i.e., playback directly on the server's audio hardware.
    /// Returns 410 Gone as jukebox functionality is not implemented per ADR-0001.
    /// </summary>
    [HttpGet]
    [HttpPost]
    [Route("/rest/jukeboxControl.view")]
    [Route("/rest/jukeboxControl")]
    public IActionResult JukeboxControlNotSupported()
    {
        HttpContext.Response.Headers.Append("Cache-Control", "no-cache");
        return StatusCode((int)HttpStatusCode.Gone);
    }
}
