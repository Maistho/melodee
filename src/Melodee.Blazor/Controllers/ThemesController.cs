using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Blazor.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/themes")]
public class ThemesController(
    EtagRepository etagRepository,
    ISerializer serializer,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory,
    IThemeService themeService,
    IDbContextFactory<MelodeeDbContext> contextFactory) 
    : ControllerBase(etagRepository, serializer, configuration, configurationFactory)
{
    /// <summary>
    /// Get all available theme packs
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ThemePackInfo>>> GetThemes(CancellationToken cancellationToken)
    {
        var themes = await themeService.DiscoverThemePacksAsync(cancellationToken);
        
        var themeInfos = themes.Select(t => new ThemePackInfo
        {
            Id = t.Id,
            Name = t.Name,
            Author = t.Author,
            Version = t.Version,
            Description = t.Description,
            IsBuiltIn = t.IsBuiltIn,
            PreviewImage = t.PreviewImage,
            HasWarnings = t.HasWarnings,
            WarningDetails = t.WarningDetails
        });

        return Ok(themeInfos);
    }

    /// <summary>
    /// Set theme preference for current user
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    public async Task<IActionResult> SetUserTheme([FromBody] SetUserThemeRequest request, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        // Validate theme exists
        var theme = await themeService.GetThemePackAsync(request.ThemeId, cancellationToken);
        if (theme == null)
        {
            return NotFound(new { error = $"Theme '{request.ThemeId}' not found" });
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await context.Users.FindAsync([userId], cancellationToken);
        
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        user.PreferredTheme = request.ThemeId;
        await context.SaveChangesAsync(cancellationToken);

        return Ok(new { themeId = request.ThemeId, message = "Theme preference updated" });
    }

    /// <summary>
    /// Rescan theme library for new theme packs (admin only)
    /// </summary>
    [HttpPost("rescan")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ThemePackInfo>>> RescanThemes(CancellationToken cancellationToken)
    {
        // Discovery happens on each call, so just return the current list
        var themes = await themeService.DiscoverThemePacksAsync(cancellationToken);
        
        var themeInfos = themes.Select(t => new ThemePackInfo
        {
            Id = t.Id,
            Name = t.Name,
            Author = t.Author,
            Version = t.Version,
            Description = t.Description,
            IsBuiltIn = t.IsBuiltIn,
            PreviewImage = t.PreviewImage,
            HasWarnings = t.HasWarnings,
            WarningDetails = t.WarningDetails
        });

        return Ok(themeInfos);
    }

    /// <summary>
    /// Import a theme pack from zip file (admin only)
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(100_000_000)] // 100MB max
    public async Task<IActionResult> ImportTheme(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file provided" });
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "File must be a .zip archive" });
        }

        await using var stream = file.OpenReadStream();
        var (success, themeId, error) = await themeService.ImportThemePackAsync(stream, cancellationToken);

        if (!success)
        {
            return BadRequest(new { error });
        }

        return Ok(new { themeId, message = $"Theme '{themeId}' imported successfully" });
    }

    /// <summary>
    /// Export a theme pack as zip file (admin only)
    /// </summary>
    [HttpGet("{themeId}/export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportTheme(string themeId, CancellationToken cancellationToken)
    {
        var stream = await themeService.ExportThemePackAsync(themeId, cancellationToken);
        if (stream == null)
        {
            return NotFound(new { error = $"Theme '{themeId}' not found or cannot be exported" });
        }

        return File(stream, "application/zip", $"{themeId}.zip");
    }

    /// <summary>
    /// Delete a theme pack (admin only, built-in themes cannot be deleted)
    /// </summary>
    [HttpDelete("{themeId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTheme(string themeId, CancellationToken cancellationToken)
    {
        var (success, error) = await themeService.DeleteThemePackAsync(themeId, cancellationToken);

        if (!success)
        {
            return BadRequest(new { error });
        }

        return Ok(new { message = $"Theme '{themeId}' deleted successfully" });
    }
}
