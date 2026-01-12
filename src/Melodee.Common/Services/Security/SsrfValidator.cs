using System.Net;
using System.Net.Sockets;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Utility;
using Serilog;

namespace Melodee.Common.Services.Security;

/// <summary>
/// Validates URLs against SSRF (Server-Side Request Forgery) attacks.
/// Implements scheme allow-list, port allow-list, and private IP blocklist.
/// </summary>
public sealed class SsrfValidator(ILogger logger, IMelodeeConfigurationFactory configurationFactory) : ISsrfValidator
{
    private static readonly HashSet<string> AllowedSchemes = ["https"];
    private static readonly HashSet<string> AllowedSchemesWithHttp = ["https", "http"];
    private static readonly HashSet<int> AllowedPorts = [443, 80];

    /// <inheritdoc />
    public async Task<SsrfValidationResult> ValidateUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return SsrfValidationResult.Invalid("URL is required");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return SsrfValidationResult.Invalid("Invalid URL format");
        }

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var allowHttp = configuration.GetValue<bool>(SettingRegistry.PodcastHttpAllowHttp);

        var schemeResult = ValidateScheme(uri, allowHttp);
        if (!schemeResult.IsValid)
        {
            return schemeResult;
        }

        var portResult = ValidatePort(uri, allowHttp);
        if (!portResult.IsValid)
        {
            return portResult;
        }

        var ipResult = await ValidateHostIpAsync(uri.Host, cancellationToken).ConfigureAwait(false);
        if (!ipResult.IsValid)
        {
            return ipResult;
        }

        return SsrfValidationResult.Valid(ipResult.ResolvedAddresses);
    }

    /// <inheritdoc />
    public async Task<SsrfValidationResult> ValidateRedirectAsync(
        string originalUrl,
        string redirectUrl,
        int redirectCount,
        int maxRedirects,
        CancellationToken cancellationToken = default)
    {
        if (redirectCount >= maxRedirects)
        {
            return SsrfValidationResult.Invalid($"Too many redirects (max {maxRedirects})");
        }

        if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out var redirectUri))
        {
            if (Uri.TryCreate(originalUrl, UriKind.Absolute, out var originalUri))
            {
                if (Uri.TryCreate(originalUri, redirectUrl, out redirectUri))
                {
                    return await ValidateUrlAsync(redirectUri.ToString(), cancellationToken).ConfigureAwait(false);
                }
            }
            return SsrfValidationResult.Invalid("Invalid redirect URL");
        }

        return await ValidateUrlAsync(redirectUrl, cancellationToken).ConfigureAwait(false);
    }

    private static SsrfValidationResult ValidateScheme(Uri uri, bool allowHttp)
    {
        var allowedSchemes = allowHttp ? AllowedSchemesWithHttp : AllowedSchemes;

        if (!allowedSchemes.Contains(uri.Scheme.ToLowerInvariant()))
        {
            var message = allowHttp
                ? "URL scheme must be http or https"
                : "URL scheme must be https (http disabled)";
            return SsrfValidationResult.Invalid(message);
        }

        return SsrfValidationResult.Valid([]);
    }

    private static SsrfValidationResult ValidatePort(Uri uri, bool allowHttp)
    {
        var port = uri.Port;
        if (port == -1)
        {
            port = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80;
        }

        if (!AllowedPorts.Contains(port))
        {
            return SsrfValidationResult.Invalid($"Port {port} is not allowed (allowed: 80, 443)");
        }

        if (port == 80 && !allowHttp)
        {
            return SsrfValidationResult.Invalid("Port 80 requires http to be enabled");
        }

        return SsrfValidationResult.Valid([]);
    }

    private async Task<SsrfValidationResult> ValidateHostIpAsync(string host, CancellationToken cancellationToken)
    {
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false);

            if (addresses.Length == 0)
            {
                return SsrfValidationResult.Invalid("Could not resolve hostname");
            }

            foreach (var address in addresses)
            {
                if (IsPrivateOrReservedAddress(address))
                {
                    logger.Warning("[SsrfValidator] Blocked request to private/reserved IP: {Host} -> {IP}", LogSanitizer.Sanitize(host), address);
                    return SsrfValidationResult.Invalid($"Access to private/reserved IP addresses is not allowed ({address})");
                }
            }

            return SsrfValidationResult.Valid(addresses);
        }
        catch (SocketException ex)
        {
            logger.Warning(ex, "[SsrfValidator] DNS resolution failed for {Host}", LogSanitizer.Sanitize(host));
            return SsrfValidationResult.Invalid($"DNS resolution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if an IP address is private, loopback, link-local, or otherwise reserved.
    /// </summary>
    public static bool IsPrivateOrReservedAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        var bytes = address.GetAddressBytes();

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            // IPv4 private ranges
            // 10.0.0.0/8
            if (bytes[0] == 10)
            {
                return true;
            }

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            {
                return true;
            }

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
            {
                return true;
            }

            // Link-local 169.254.0.0/16
            if (bytes[0] == 169 && bytes[1] == 254)
            {
                return true;
            }

            // Loopback 127.0.0.0/8
            if (bytes[0] == 127)
            {
                return true;
            }

            // Current network 0.0.0.0/8
            if (bytes[0] == 0)
            {
                return true;
            }

            // Multicast 224.0.0.0/4
            if (bytes[0] >= 224 && bytes[0] <= 239)
            {
                return true;
            }

            // Reserved 240.0.0.0/4
            if (bytes[0] >= 240)
            {
                return true;
            }

            // Carrier-grade NAT 100.64.0.0/10
            if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)
            {
                return true;
            }
        }
        else if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // IPv6 loopback ::1
            if (address.Equals(IPAddress.IPv6Loopback))
            {
                return true;
            }

            // IPv6 link-local fe80::/10
            if (bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80)
            {
                return true;
            }

            // IPv6 unique local fc00::/7
            if ((bytes[0] & 0xfe) == 0xfc)
            {
                return true;
            }

            // IPv6 multicast ff00::/8
            if (bytes[0] == 0xff)
            {
                return true;
            }

            // IPv4-mapped IPv6 addresses ::ffff:x.x.x.x
            if (address.IsIPv4MappedToIPv6)
            {
                var ipv4 = address.MapToIPv4();
                return IsPrivateOrReservedAddress(ipv4);
            }
        }

        return false;
    }
}

/// <summary>
/// Interface for SSRF validation.
/// </summary>
public interface ISsrfValidator
{
    /// <summary>
    /// Validates a URL against SSRF attacks.
    /// </summary>
    Task<SsrfValidationResult> ValidateUrlAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a redirect URL, including redirect count limits.
    /// </summary>
    Task<SsrfValidationResult> ValidateRedirectAsync(
        string originalUrl,
        string redirectUrl,
        int redirectCount,
        int maxRedirects,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of SSRF validation.
/// </summary>
public sealed record SsrfValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public IPAddress[] ResolvedAddresses { get; init; } = [];

    public static SsrfValidationResult Valid(IPAddress[] resolvedAddresses) => new()
    {
        IsValid = true,
        ResolvedAddresses = resolvedAddresses
    };

    public static SsrfValidationResult Invalid(string errorMessage) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage
    };
}
