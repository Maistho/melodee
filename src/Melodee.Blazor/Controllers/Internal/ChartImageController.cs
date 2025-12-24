using Melodee.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Internal;

/// <summary>
/// Internal controller for chart image handling within the Blazor UI.
/// These endpoints are NOT part of the public API and are only used by the Blazor admin UI.
/// </summary>
[ApiController]
[Route("api/charts")]
public sealed class ChartImageController(ChartService chartService) : Microsoft.AspNetCore.Mvc.ControllerBase
{
    /// <summary>
    /// Get chart image by API key.
    /// </summary>
    [HttpGet("{apiKey:guid}/image")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImageAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        var result = await chartService.GetChartImageBytesAndEtagAsync(apiKey, null, cancellationToken);
        
        if (result.Bytes == null || result.Bytes.Length == 0)
        {
            return NotFound();
        }

        return File(result.Bytes, "image/gif", enableRangeProcessing: true);
    }

    /// <summary>
    /// Upload chart image (admin only).
    /// </summary>
    [HttpPost("{apiKey:guid}/image")]
    [Authorize(Roles = "Administrator")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadImageAsync(Guid apiKey, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);
        var imageBytes = ms.ToArray();

        var result = await chartService.UploadChartImageAsync(apiKey, imageBytes, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Messages?.FirstOrDefault() ?? "Failed to upload image" });
        }

        return Ok(new { success = true });
    }

    /// <summary>
    /// Delete chart image (admin only).
    /// </summary>
    [HttpDelete("{apiKey:guid}/image")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteImageAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        var result = await chartService.DeleteChartImageAsync(apiKey, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Messages?.FirstOrDefault() ?? "Failed to delete image" });
        }

        return Ok(new { success = true });
    }
}
