using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Melodee.Blazor.Controllers.Jellyfin;

public record JellyfinTokenInfo(
    string? Token,
    string? Client,
    string? Device,
    string? DeviceId,
    string? Version);

public static partial class JellyfinTokenParser
{
    [GeneratedRegex(@"Token\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"Client\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ClientRegex();

    [GeneratedRegex(@"Device\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DeviceRegex();

    [GeneratedRegex(@"DeviceId\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DeviceIdRegex();

    [GeneratedRegex(@"Version\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex VersionRegex();

    public static JellyfinTokenInfo ParseFromRequest(HttpRequest request)
    {
        string? token = null;
        string? client = null;
        string? device = null;
        string? deviceId = null;
        string? version = null;

        if (request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var authValue = authHeader.ToString();
            if (authValue.StartsWith("MediaBrowser", StringComparison.OrdinalIgnoreCase))
            {
                var parsed = ParseMediaBrowserHeader(authValue);
                token = parsed.Token;
                client = parsed.Client;
                device = parsed.Device;
                deviceId = parsed.DeviceId;
                version = parsed.Version;
            }
        }

        if (token == null && request.Headers.TryGetValue("X-Emby-Authorization", out var embyAuthHeader))
        {
            var embyAuthValue = embyAuthHeader.ToString();
            if (embyAuthValue.StartsWith("MediaBrowser", StringComparison.OrdinalIgnoreCase))
            {
                var parsed = ParseMediaBrowserHeader(embyAuthValue);
                token ??= parsed.Token;
                client ??= parsed.Client;
                device ??= parsed.Device;
                deviceId ??= parsed.DeviceId;
                version ??= parsed.Version;
            }
        }

        if (token == null && request.Headers.TryGetValue("X-MediaBrowser-Token", out var mbToken))
        {
            token = mbToken.ToString();
        }

        if (token == null && request.Headers.TryGetValue("X-Emby-Token", out var embyToken))
        {
            token = embyToken.ToString();
        }

        if (token == null && request.Query.TryGetValue("api_key", out var apiKeyValue))
        {
            token = apiKeyValue.ToString();
        }

        return new JellyfinTokenInfo(token, client, device, deviceId, version);
    }

    private static JellyfinTokenInfo ParseMediaBrowserHeader(string headerValue)
    {
        var tokenMatch = TokenRegex().Match(headerValue);
        var clientMatch = ClientRegex().Match(headerValue);
        var deviceMatch = DeviceRegex().Match(headerValue);
        var deviceIdMatch = DeviceIdRegex().Match(headerValue);
        var versionMatch = VersionRegex().Match(headerValue);

        return new JellyfinTokenInfo(
            tokenMatch.Success ? tokenMatch.Groups[1].Value : null,
            clientMatch.Success ? clientMatch.Groups[1].Value : null,
            deviceMatch.Success ? deviceMatch.Groups[1].Value : null,
            deviceIdMatch.Success ? deviceIdMatch.Groups[1].Value : null,
            versionMatch.Success ? versionMatch.Groups[1].Value : null
        );
    }

    public static string GenerateToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Extracts the first 8 characters of a token for prefix-based lookup.
    /// </summary>
    public static string GetTokenPrefix(string token)
    {
        return token.Length >= 8 ? token[..8] : token;
    }

    public static string GenerateSalt()
    {
        var bytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public static string HashToken(string token, string salt, string pepper)
    {
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var combined = new byte[saltBytes.Length + tokenBytes.Length];
        Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
        Buffer.BlockCopy(tokenBytes, 0, combined, saltBytes.Length, tokenBytes.Length);

        var pepperBytes = Encoding.UTF8.GetBytes(pepper);
        using var hmac = new HMACSHA256(pepperBytes);
        var hash = hmac.ComputeHash(combined);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyToken(string token, string salt, string pepper, string expectedHash)
    {
        var computedHash = HashToken(token, salt, pepper);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(expectedHash));
    }
}
