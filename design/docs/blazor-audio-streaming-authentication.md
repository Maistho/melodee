# Blazor Audio Streaming Authentication

## Overview

This document explains how audio streaming authentication works in Melodee's Blazor Server application, covering both music playback and podcast playback.

## Authentication Mechanism

### Cookie-Based Authentication

Melodee uses a **cookie-based authentication system** for streaming audio from Blazor Server components:

1. **Cookie Generation** (`MelodeeBlazorCookieMiddleware`)
   - Cookie name: `melodee_blazor_token`
   - Cookie value: SHA256 hash of `{current_date_yyyyMMdd}{encryption_private_key}`
   - Set on every non-API request (Blazor pages)
   - Cookie properties:
     - `HttpOnly: true` (prevents JavaScript access for security)
     - `Secure: true` (requires HTTPS)
     - `SameSite: None` (allows cross-site requests for hosted players)
     - `Expires: 1 day`

2. **Cookie Validation** (OpenSubsonic `ControllerBase`)
   - All OpenSubsonic endpoints (`/rest/*`) check for the `melodee_blazor_token` cookie
   - Cookie is validated by regenerating the hash with current date + encryption key
   - If cookie matches, authentication is bypassed (no username/password required)
   - Localhost requests and requests from the configured base URL also bypass authentication

### Why This Works for HTML5 Audio

When a Blazor component uses JavaScript to load audio via the HTML5 `<audio>` element:

```javascript
const audio = new Audio();
audio.src = '/rest/stream?id=song:123';  // Relative URL
audio.load();
```

The browser **automatically sends all cookies** for the same origin, including `melodee_blazor_token`. This means:
- ✅ No need to manually add authentication headers
- ✅ No need to construct query strings with username/password/token
- ✅ No need for special authentication tokens in the URL
- ✅ Works seamlessly because Blazor Server and OpenSubsonic endpoints are same-origin

## Implementation Pattern

### Music Player (Reference Implementation)

**File**: `/src/Melodee.Blazor/Components/Pages/MusicPlayer.razor`

```csharp
private string GetStreamUrl()
{
    var baseUrl = BaseUrlService.GetBaseUrl();
    if (baseUrl == null)
    {
        throw new Exception("Base URL is not available for generating streaming URLs.");
    }
    
    return $"{baseUrl}/rest/stream?id={CurrentSong.ToApiKey()}";
}
```

**Note**: The MusicPlayer uses `BaseUrlService` for full URLs, but this is **not necessary for authentication**. The full URL is used for other reasons (possibly for sharing or external player support).

### Podcast Player (Correct Implementation)

**File**: `/src/Melodee.Blazor/Components/Pages/Data/PodcastDetail.razor`

```csharp
private async Task PlayEpisode(PodcastEpisode episode)
{
    // Use RELATIVE URL - browser sends cookie automatically
    var audioSource = $"/rest/streamPodcastEpisode?id=podcast:episode:{episode.Id}";
    
    // Load and play episode via JavaScript interop
    await _module.InvokeVoidAsync("loadEpisode", audioSource, startPosition);
}
```

**Why relative URLs work**:
- Browser is already on `https://melodee.example.com/data/podcasts/123`
- Relative URL `/rest/streamPodcastEpisode?id=...` resolves to `https://melodee.example.com/rest/streamPodcastEpisode?id=...`
- Browser sends `melodee_blazor_token` cookie with the request (same origin)
- OpenSubsonic endpoint validates cookie and allows access

## Common Mistakes to Avoid

### ❌ WRONG: Using full URLs unnecessarily

```csharp
var baseUrl = BaseUrlService.GetBaseUrl();
var audioSource = $"{baseUrl}/rest/streamPodcastEpisode?id={id}";  // Unnecessary
```

**Why this is wrong**:
- Adds unnecessary complexity
- Requires injecting `IBaseUrlService`
- Can fail if base URL is not configured
- Still relies on the cookie (full URL doesn't add authentication)

### ❌ WRONG: Trying to add auth parameters to URL

```csharp
var audioSource = $"/rest/stream?id={id}&u={username}&p={password}";  // Not needed
```

**Why this is wrong**:
- Not necessary because cookie authentication works
- Exposes credentials in URLs (security risk if URLs are logged/cached)
- OpenSubsonic endpoints already have cookie auth

### ✅ CORRECT: Use relative URLs

```csharp
var audioSource = $"/rest/stream?id={id}";  // Simple and secure
```

**Why this is correct**:
- Minimal code
- Browser handles authentication automatically via cookie
- Secure (credentials never in URL)
- Works for same-origin requests (which Blazor Server always is)

## OpenSubsonic Endpoint Authentication Flow

### Request Flow

1. **User navigates to Blazor page** (e.g., `/data/podcasts/123`)
   - `MelodeeBlazorCookieMiddleware` intercepts the request
   - Sets `melodee_blazor_token` cookie with today's hash
   - Cookie sent to browser with response

2. **User clicks "Play" button**
   - Blazor component calls JavaScript interop
   - JavaScript sets audio source to `/rest/streamPodcastEpisode?id=podcast:episode:456`

3. **Browser requests audio from `/rest/streamPodcastEpisode`**
   - Browser automatically includes `melodee_blazor_token` cookie
   - Request goes to `PodcastController.StreamPodcastEpisodeAsync()`

4. **OpenSubsonic controller validates authentication**
   - `ControllerBase.OnActionExecutionAsync()` runs before action
   - Checks for `melodee_blazor_token` cookie
   - Regenerates hash: `SHA256({today_yyyyMMdd}{encryption_key})`
   - Compares cookie value to regenerated hash
   - If match: `requiresAuth = false` (authentication bypassed)
   - If no match: `requiresAuth = true` (requires OpenSubsonic username/password/token)

5. **Audio streaming begins**
   - Controller returns audio file with HTTP Range support
   - Browser receives audio data and plays it

### Code References

**Cookie Middleware**: `/src/Melodee.Blazor/Middleware/MelodeeBlazorCookieMiddleware.cs`
```csharp
public const string DateFormat = "yyyyMMdd";
public const string CookieName = "melodee_blazor_token";

// Cookie is set with hash of date + encryption key
context.Response.Cookies.Append(CookieName,
    HashHelper.CreateSha256(DateTime.UtcNow.ToString(DateFormat) + 
        configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)) ?? string.Empty,
    new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = DateTime.UtcNow.AddDays(1)
    });
```

**OpenSubsonic Authentication**: `/src/Melodee.Blazor/Controllers/OpenSubsonic/ControllerBase.cs`
```csharp
// Check if request has valid Blazor cookie
context.HttpContext.Request.Cookies.TryGetValue("melodee_blazor_token", out var melodeeBlazorTokenCookie);
if (!string.IsNullOrWhiteSpace(melodeeBlazorTokenCookie))
{
    var cookieHash = HashHelper.CreateSha256(
        DateTime.UtcNow.ToString(MelodeeBlazorCookieMiddleware.DateFormat) + 
        configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)) ?? string.Empty;
    
    requiresAuth = melodeeBlazorTokenCookie != cookieHash;  // If match, no auth needed
}
```

## Security Considerations

### Cookie Properties

- **HttpOnly**: Prevents JavaScript from reading the cookie (XSS protection)
- **Secure**: Cookie only sent over HTTPS (prevents interception)
- **SameSite=None**: Allows cross-site requests (needed for hosted players like Feishin on Vercel)

### Daily Rotation

- Cookie hash includes current date in `yyyyMMdd` format
- Hash changes at midnight UTC
- Old cookies become invalid automatically (24-hour expiry)
- Provides time-based token rotation without user intervention

### Encryption Key

- Hash includes `system.encryption.privateKey` setting
- Key should be unique per Melodee instance
- Key should be kept secret (never committed to source control)
- Changing the key invalidates all existing cookies

## Troubleshooting

### 403 Forbidden Errors

**Symptom**: Audio playback fails with HTTP 403 Forbidden

**Possible Causes**:
1. **Cookie not being sent** - Check browser DevTools Network tab to verify `melodee_blazor_token` cookie is included in request
2. **Cookie expired** - Cookie has 24-hour expiry; refresh the page to get a new cookie
3. **Clock skew** - Server and client clocks are out of sync (cookie uses date-based hash)
4. **Configuration issue** - `system.encryption.privateKey` setting is missing or changed

**Solution**:
1. Verify cookie exists in browser (F12 → Application → Cookies)
2. Refresh the Blazor page to get a new cookie
3. Check that audio URL is **relative** (starts with `/rest/...`) not full URL
4. Verify `system.encryption.privateKey` is configured

### Cookie Not Sent by Browser

**Symptom**: Cookie exists but not sent with audio requests

**Possible Causes**:
1. **Cross-origin request** - Using full URL with different domain
2. **SameSite restrictions** - Browser blocking cookie for security reasons
3. **Insecure context** - Not using HTTPS (required for `Secure` cookies)

**Solution**:
1. Use **relative URLs** instead of full URLs
2. Ensure application is served over HTTPS
3. Check browser console for cookie warnings

### Localhost Development

When developing locally:
- Localhost requests bypass authentication check (line 118 in ControllerBase.cs)
- Cookie authentication still works but is not strictly required
- Test with production-like setup (HTTPS, non-localhost domain) to verify cookie auth

## Summary

**Key Takeaways**:
- ✅ Blazor Server + OpenSubsonic use **cookie authentication** for audio streaming
- ✅ Use **relative URLs** (`/rest/stream?id=...`) for audio sources
- ✅ Browser **automatically sends cookie** with same-origin requests
- ✅ **No need for BaseUrlService** or manual authentication parameters
- ✅ Cookie is **daily-rotated** SHA256 hash for security
- ❌ **Never** put authentication credentials in URLs
- ❌ **Don't** make HTTP API calls from Blazor components (inject services directly)

**Pattern to Follow**:
```csharp
// In Blazor component
var audioSource = $"/rest/stream?id={apiKey}";
await JsRuntime.InvokeVoidAsync("loadAudio", audioSource);
```

That's it! The browser and OpenSubsonic endpoints handle the rest automatically.
