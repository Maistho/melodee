# Podcast Playback Authentication Debugging

## Current Status

**Problem**: HTTP 403 Forbidden errors when attempting to play podcast episodes, despite:
- Using relative URL (`/rest/streamPodcastEpisode?id=podcast:episode:301`)
- Cookie (`melodee_blazor_token`) being set by middleware
- `-*-> User authenticated` message appearing in logs

## Hypothesis

Based on code review, the likely issue is:

1. **Cookie authentication is working** - The `requiresAuth` flag is being set to `false` correctly
2. **User lookup is failing** - When `requiresAuth = false`, the code tries to get user by username
3. **No username provided** - Cookie auth doesn't provide a username, so it falls back to `UserInfo.BlankUserInfo`
4. **BlankUserInfo has no roles** - The blank user info doesn't have the required `HasStreamRole`
5. **Role check fails** - Line 440 in PodcastController checks for `HasStreamRole` and returns 403

## Debugging Logging Added

### 1. ControllerBase.cs (Cookie Validation)

Added logging to show:
- Whether request is from localhost/baseURL
- Whether `melodee_blazor_token` cookie is present
- Whether cookie hash matches expected value
- Final `requiresAuth` value

```csharp
Log.Debug("[OpenSubsonic Auth] Cookie auth check - Cookie present: {CookiePresent}, Cookie matches: {CookieMatches}, RequiresAuth: {RequiresAuth}, Path: {Path}",
    !string.IsNullOrWhiteSpace(melodeeBlazorTokenCookie),
    melodeeBlazorTokenCookie == cookieHash,
    requiresAuth,
    context.HttpContext.Request.Path);
```

### 2. OpenSubsonicApiService.cs (User Authentication)

Added logging to show:
- Username being used (or "null" if none)
- User ID returned
- Roles assigned to the user

```csharp
Logger.Debug("[{MethodName}] Authentication bypassed (cookie auth). Username: {Username}, UserInfo: {UserInfo}, Roles: {Roles}",
    nameof(AuthenticateSubsonicApiAsync),
    apiRequest.Username ?? "null",
    userInfo.Id,
    string.Join(", ", userInfo.Roles ?? []));
```

### 3. PodcastController.cs (Role Check)

Added logging to show:
- Authentication success/failure
- User ID
- User roles
- Whether `HasStreamRole` check passes

```csharp
Log.Debug("[StreamPodcastEpisode] Auth result - IsSuccess: {IsSuccess}, UserId: {UserId}, Roles: {Roles}",
    auth.IsSuccess,
    auth.UserInfo?.Id ?? 0,
    string.Join(", ", auth.UserInfo?.Roles ?? []));

// If role check fails:
Log.Warning("[StreamPodcastEpisode] User {UserId} does not have HasStreamRole. Roles: {Roles}",
    auth.UserInfo.Id,
    string.Join(", ", auth.UserInfo.Roles ?? []));
```

## Expected Log Output

When you click play on a podcast episode, you should now see logs like:

```
[OpenSubsonic Auth] Cookie auth check - Cookie present: True, Cookie matches: True, RequiresAuth: False, Path: /rest/streamPodcastEpisode
[AuthenticateSubsonicApiAsync] Authentication bypassed (cookie auth). Username: null, UserInfo: 0, Roles: 
[StreamPodcastEpisode] Auth result - IsSuccess: True, UserId: 0, Roles: 
[StreamPodcastEpisode] User 0 does not have HasStreamRole. Roles: 
```

This would confirm that:
1. ✅ Cookie auth is working (`RequiresAuth: False`)
2. ❌ No user is being identified (`UserId: 0`)
3. ❌ No roles are assigned (`Roles: `)
4. ❌ Role check fails (no `HasStreamRole`)

## Likely Root Cause

The issue is that **cookie authentication doesn't provide a username**, so the system can't look up the actual logged-in Blazor user.

### Code Path Analysis

**File**: `/src/Melodee.Common/Services/OpenSubsonicApiService.cs` (lines 1565-1575)

```csharp
if (!apiRequest.RequiresAuthentication)
{
    var user = apiRequest.Username == null
        ? null  // <-- This is the problem!
        : await userService.GetByUsernameAsync(apiRequest.Username, cancellationToken);
    
    return new ResponseModel
    {
        UserInfo = user?.Data?.ToUserInfo() ?? UserInfo.BlankUserInfo,  // <-- Returns blank user!
        ResponseData = await NewApiResponse(true, string.Empty, string.Empty)
    };
}
```

**The problem**:
- `ApiRequest` is built from query parameters and form data (lines 102-111 in ControllerBase.cs)
- Query string is: `?id=podcast:episode:301` (no `u` parameter)
- Therefore: `apiRequest.Username == null`
- Therefore: `user = null`
- Therefore: Returns `UserInfo.BlankUserInfo`
- BlankUserInfo has no roles, including `HasStreamRole`

## Potential Solutions

### Option 1: Get User from Blazor Session/Context

When cookie auth is used, we need to identify the user from the Blazor server session, not from OpenSubsonic parameters.

**Implementation**:
- Check if request has Blazor authentication context
- Extract user identity from `HttpContext.User`
- Look up user by identity/claims
- Return proper UserInfo with roles

### Option 2: Add Username to ApiRequest for Cookie Auth

Modify `ControllerBase.OnActionExecutionAsync` to:
- Detect cookie authentication
- Extract logged-in username from Blazor auth context
- Add it to the `ApiRequest` so user lookup works

### Option 3: Bypass Role Check for Cookie Auth

Since cookie auth implies the user is already authenticated via Blazor:
- Modify `StreamPodcastEpisodeAsync` to skip role check when `requiresAuth = false`
- Trust that Blazor authentication has already verified the user

### Recommended Solution

**Option 1** is the most correct approach because:
- Blazor Server has user authentication context in `HttpContext.User`
- We should leverage the existing authentication
- Maintains security by using actual user identity
- Provides correct user info with proper roles

## Next Steps

1. **Run the application** and click play on a podcast episode
2. **Review logs** to confirm the hypothesis
3. **Implement fix** based on log output
4. **Retest** with logging still enabled
5. **Remove debug logging** once issue is resolved

## Files Modified

- `/src/Melodee.Blazor/Controllers/OpenSubsonic/ControllerBase.cs` - Added cookie auth debug logging
- `/src/Melodee.Common/Services/OpenSubsonicApiService.cs` - Added user lookup debug logging
- `/src/Melodee.Blazor/Controllers/OpenSubsonic/PodcastController.cs` - Added role check debug logging
