using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Microsoft.Extensions.Primitives;

namespace Melodee.Blazor.Middleware;

public class JellyfinRoutingMiddleware(
    RequestDelegate next,
    ILogger<JellyfinRoutingMiddleware> logger,
    IMelodeeConfigurationFactory configurationFactory)
{
    private const string JellyfinInternalPrefix = "/api/jf";
    private const string MediaBrowserSchema = "MediaBrowser";
    private const string JellyfinEnabledCacheKey = "__jellyfin_enabled_cache";

    private static readonly HashSet<string> AllowlistedPreAuthPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/",
        "/System/Info/Public",
        "/System/Ping",
        "/Users/AuthenticateByName"
    };

    private static readonly HashSet<string> ExcludedPathPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/",
        "/rest/",
        "/song/"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (path.StartsWith(JellyfinInternalPrefix, StringComparison.OrdinalIgnoreCase))
        {
            if (!await IsJellyfinEnabledAsync(context))
            {
                logger.LogDebug("JellyfinRouting: Jellyfin disabled, returning 404 for prefixed path {Path}", path);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            await next(context);
            return;
        }

        foreach (var excludedPrefix in ExcludedPathPrefixes)
        {
            if (path.StartsWith(excludedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }
        }

        var (isJellyfin, detectionReason) = IsJellyfinRequestWithReason(context.Request, path);
        if (isJellyfin)
        {
            if (!await IsJellyfinEnabledAsync(context))
            {
                logger.LogWarning("JellyfinRouting: Request detected but Jellyfin API is disabled. Path={Path} DetectedVia={DetectionReason}",
                    path, detectionReason);
                await next(context);
                return;
            }

            var newPath = JellyfinInternalPrefix + path;
            logger.LogDebug("JellyfinRouting: Rewriting {OldPath} -> {NewPath} (detected via {DetectionReason})",
                path, newPath, detectionReason);
            context.Request.Path = newPath;
        }

        await next(context);
    }

    private async Task<bool> IsJellyfinEnabledAsync(HttpContext context)
    {
        if (context.Items.TryGetValue(JellyfinEnabledCacheKey, out var cached) && cached is bool cachedValue)
        {
            return cachedValue;
        }

        try
        {
            var config = await configurationFactory.GetConfigurationAsync(context.RequestAborted);
            var enabled = config.GetValue<bool>(SettingRegistry.JellyfinEnabled);
            context.Items[JellyfinEnabledCacheKey] = enabled;
            return enabled;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "JellyfinRouting: Failed to check JellyfinEnabled setting, defaulting to disabled");
            context.Items[JellyfinEnabledCacheKey] = false;
            return false;
        }
    }

    private static (bool IsJellyfin, string Reason) IsJellyfinRequestWithReason(HttpRequest request, string path)
    {
        if (AllowlistedPreAuthPaths.Contains(path))
        {
            return (true, $"AllowlistedPath:{path}");
        }

        if (request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            if (HasMediaBrowserToken(authHeader))
            {
                return (true, "AuthorizationHeader:MediaBrowser");
            }
        }

        if (request.Headers.TryGetValue("X-Emby-Authorization", out var embyAuthHeader))
        {
            if (embyAuthHeader.ToString().StartsWith(MediaBrowserSchema, StringComparison.OrdinalIgnoreCase))
            {
                return (true, "X-Emby-Authorization");
            }
        }

        if (request.Headers.ContainsKey("X-MediaBrowser-Token"))
        {
            return (true, "X-MediaBrowser-Token");
        }

        if (request.Headers.ContainsKey("X-Emby-Token"))
        {
            return (true, "X-Emby-Token");
        }

        return (false, "NoMatch");
    }

    private static bool IsJellyfinRequest(HttpRequest request, string path)
    {
        return IsJellyfinRequestWithReason(request, path).IsJellyfin;
    }

    private static bool HasMediaBrowserToken(StringValues authHeader)
    {
        var headerValue = authHeader.ToString();
        return headerValue.StartsWith(MediaBrowserSchema, StringComparison.OrdinalIgnoreCase);
    }
}
