using System.Globalization;
using System.Security.Claims;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;
using NodaTime;

namespace Melodee.Blazor.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static CultureInfo GetCulture(this ClaimsPrincipal principal)
    {
        return CultureInfo.CurrentCulture;
    }

    public static string? FormatNumber(this ClaimsPrincipal principal, short? number)
    {
        return number?.ToStringPadLeft(3) ?? MelodeeConfiguration.DefaultNoValuePlaceHolder;
    }

    public static string? FormatNumber(this ClaimsPrincipal principal, int? number)
    {
        return number?.ToStringPadLeft(5) ?? MelodeeConfiguration.DefaultNoValuePlaceHolder;
    }

    public static string TimeZoneId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypeRegistry.UserTimeZoneId) ?? "UTC";
    }

    public static string? FormatInstant(this ClaimsPrincipal principal, Instant? instant)
    {
        if (instant == null)
        {
            return MelodeeConfiguration.DefaultNoValuePlaceHolder;
        }

        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(principal.TimeZoneId()) ?? DateTimeZone.Utc;
        var localDateTime = instant.Value.InZone(timeZone).LocalDateTime;
        return localDateTime.ToString("yyyy-MM-dd HH:mm:ss", principal.GetCulture());
    }

    public static string? FormatDateTimeOffset(this ClaimsPrincipal principal, DateTimeOffset? dateTime)
    {
        return dateTime?.ToString("yyyy-MM-dd HH:mm:ss", principal.GetCulture()) ?? MelodeeConfiguration.DefaultNoValuePlaceHolder;
    }

    public static string? FormatDuration(this ClaimsPrincipal principal, Duration? duration)
    {
        return duration?.ToString("-H:mm:ss", principal.GetCulture()) ?? MelodeeConfiguration.DefaultNoValuePlaceHolder;
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(RoleNameRegistry.Administrator);
    }

    public static int UserId(this ClaimsPrincipal principal)
    {
        return SafeParser.ToNumber<int?>(principal.FindFirstValue(ClaimTypes.PrimarySid) ?? string.Empty) ?? 0;
    }

    public static Guid ToApiGuid(this ClaimsPrincipal principal)
    {
        return Guid.Parse(principal.FindFirstValue(ClaimTypes.Sid) ?? string.Empty);
    }

    public static string ToApiKey(this ClaimsPrincipal principal)
    {
        return $"user{OpenSubsonicServer.ApiIdSeparator}{principal.FindFirstValue(ClaimTypes.Sid)}";
    }

    public static bool IsEditor(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(RoleNameRegistry.Editor) || principal.IsAdmin();
    }
}
