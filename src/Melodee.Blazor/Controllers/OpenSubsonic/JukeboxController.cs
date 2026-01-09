using System.Net;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

/// <summary>
/// Jukebox control endpoints.
/// Currently returns HTTP 410 Gone because server-side jukebox playback is not enabled by default.
/// See `design/requirements/JUKEBOX-REQUIREMENTS.md` for the intended Party Mode + optional backends approach.
/// </summary>
public class JukeboxController(ISerializer serializer, EtagRepository etagRepository, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    /// <summary>
    /// Controls the jukebox, i.e., playback directly on the server's audio hardware.
    /// Returns 410 Gone because jukebox functionality is not enabled by default.
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
