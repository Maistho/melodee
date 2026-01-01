using Microsoft.Extensions.Primitives;

namespace Melodee.Blazor.Middleware;

public class JellyfinRoutingMiddleware(RequestDelegate next, ILogger<JellyfinRoutingMiddleware> logger)
{
    private const string JellyfinInternalPrefix = "/api/jf";
    private const string MediaBrowserSchema = "MediaBrowser";

    private static readonly HashSet<string> AllowlistedPreAuthPaths = new(StringComparer.OrdinalIgnoreCase)
    {
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

        if (IsJellyfinRequest(context.Request, path))
        {
            var newPath = JellyfinInternalPrefix + path;
            logger.LogDebug("JellyfinRoutingMiddleware rewriting path {OldPath} to {NewPath}", path, newPath);
            context.Request.Path = newPath;
        }

        await next(context);
    }

    private static bool IsJellyfinRequest(HttpRequest request, string path)
    {
        if (AllowlistedPreAuthPaths.Contains(path))
        {
            return true;
        }

        if (request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            if (HasMediaBrowserToken(authHeader))
            {
                return true;
            }
        }

        if (request.Headers.TryGetValue("X-Emby-Authorization", out var embyAuthHeader))
        {
            if (embyAuthHeader.ToString().StartsWith(MediaBrowserSchema, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        if (request.Headers.ContainsKey("X-MediaBrowser-Token"))
        {
            return true;
        }

        if (request.Headers.ContainsKey("X-Emby-Token"))
        {
            return true;
        }

        return false;
    }

    private static bool HasMediaBrowserToken(StringValues authHeader)
    {
        var headerValue = authHeader.ToString();
        return headerValue.StartsWith(MediaBrowserSchema, StringComparison.OrdinalIgnoreCase);
    }
}
