using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Melodee.Blazor.Controllers.Jellyfin;

/// <summary>
/// Handles root-level Jellyfin API requests for server discovery.
/// Jellyfin clients send HEAD / to check if a server is alive before querying /System/Info/Public.
/// </summary>
[ApiController]
[Route("api/jf")]
[ApiExplorerSettings(GroupName = "jellyfin")]
[EnableRateLimiting("jellyfin-api")]
public class RootController : ControllerBase
{
    /// <summary>
    /// Server discovery endpoint. Jellyfin clients send HEAD / to verify server availability.
    /// </summary>
    [HttpGet("")]
    [HttpHead("")]
    [AllowAnonymous]
    public IActionResult ServerRoot()
    {
        Response.ContentLength = 0;
        return Ok();
    }
}
