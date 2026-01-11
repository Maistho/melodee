# Podcast Playback - Ready for Testing

## Changes Made

### 1. Authentication Fix (PodcastController.cs)

**Problem**: Role check (`HasStreamRole`) was failing for localhost/cookie-authenticated requests because:
- Authentication bypassed → `requiresAuth = false`  
- No username provided → Returns `UserInfo.BlankUserInfo`
- BlankUserInfo has no roles → Role check fails with 403

**Solution**: Skip role check when authentication is bypassed (lines 437-447):

```csharp
// Only check for HasStreamRole if authentication was required (not localhost/cookie auth)
// When authentication is bypassed (localhost or Blazor cookie auth), skip role check
if (ApiRequest.RequiresAuthentication && !(auth.UserInfo.Roles?.Contains("HasStreamRole") ?? false))
{
    Log.Warning("[StreamPodcastEpisode] User {UserId} does not have HasStreamRole. Roles: {Roles}",
        auth.UserInfo.Id,
        string.Join(", ", auth.UserInfo.Roles ?? []));
    return StatusCode((int)HttpStatusCode.Forbidden, CreateResponse(new Error(10, "User role not allowed")));
}
```

This matches the music streaming endpoint behavior (which has no role check at all).

### 2. Audio URL Simplified (PodcastDetail.razor)

**Before**:
```csharp
var baseUrl = BaseUrlService.GetBaseUrl();
var audioSource = $"{baseUrl}/rest/streamPodcastEpisode?id=podcast:episode:{id}";
```

**After**:
```csharp
var audioSource = $"/rest/streamPodcastEpisode?id=podcast:episode:{episode.Id}";
```

Using relative URLs allows browser to automatically send the `melodee_blazor_token` cookie.

### 3. Documentation

Created comprehensive documentation:
- `/docs/blazor-audio-streaming-authentication.md` - How cookie auth works
- `/docs/podcast-playback-auth-fix.md` - Fix summary
- `/docs/podcast-playback-auth-debugging.md` - Debugging approach
- Updated `/design/requirements/podcast-requirements.md` - Documented OpenSubsonic client gaps for Phase 2

## Expected Behavior

When you click play on a podcast episode, you should see:

**Logs**:
```
[OpenSubsonic Auth] Request from localhost or baseUrl, bypassing authentication. Path: /rest/streamPodcastEpisode
[AuthenticateSubsonicApiAsync] Authentication bypassed (cookie auth). Username: null, UserInfo: 0, Roles: 
[StreamPodcastEpisode] Auth result - IsSuccess: True, UserId: 0, Roles: 
[StreamPodcastEpisode] Role check passed. RequiresAuth: False, HasStreamRole: False
HTTP "GET" "/rest/streamPodcastEpisode" responded 200
[PodcastDetail] Episode duration loaded: {X}s
[PodcastDetail] Heartbeat sent for episode {id} at position {X}s
```

**UI Behavior**:
- ✅ Audio player appears
- ✅ Episode starts playing (no 403 error)
- ✅ Progress slider updates
- ✅ Pause/play/seek works
- ✅ Bookmark saves every 10s (check logs)
- ✅ Heartbeat every 30s (check logs)
- ✅ Scrobble at 50% duration (check logs)

## Testing Checklist

### Basic Playback
- [ ] Click play button on any podcast episode
- [ ] Verify HTTP 200 response (not 403) in browser DevTools Network tab
- [ ] Audio starts playing within 2-3 seconds
- [ ] Progress slider shows current position
- [ ] Duration displays correctly

### Playback Controls
- [ ] Click pause → audio pauses
- [ ] Click play → audio resumes from same position
- [ ] Drag progress slider → audio seeks to new position
- [ ] Click stop → audio stops and player UI clears

### Playback Tracking (Check Logs)
- [ ] Heartbeat logs appear every ~30 seconds
- [ ] Bookmark save logs appear every ~10 seconds
- [ ] Scrobble log appears when 50% duration reached (or 240s for long episodes)
- [ ] Episode completion log appears when episode finishes

### Edge Cases
- [ ] Close page during playback → Bookmark saved
- [ ] Reopen same episode → Resumes from bookmarked position
- [ ] Play different episode → Previous episode stops, new one starts
- [ ] Seek near end → Triggers scrobble if threshold reached

## Known Limitations (Phase 1)

### OpenSubsonic Client Playback Tracking

**What Works**:
- ✅ Melodee.Blazor can play and track podcast episodes fully
- ✅ OpenSubsonic clients can stream podcast episodes via `/rest/streamPodcastEpisode`

**What Doesn't Work (Phase 2)**:
- ❌ OpenSubsonic clients cannot track playback (NowPlaying, Scrobble, Bookmarks)
- ❌ `/rest/scrobble.view` only handles music, not podcast episodes
- ❌ `/rest/getNowPlaying.view` doesn't include podcast episodes
- ❌ `/rest/getBookmarks.view`, `createBookmark.view`, `deleteBookmark.view` don't exist for podcasts

**Why**: The `ScrobbleService` is music-specific and only queries the `Songs` table. Podcast episode tracking requires extending the OpenSubsonic API to recognize `podcast:episode:*` IDs.

**Impact**: Users must use Melodee.Blazor for full podcast playback tracking. OpenSubsonic clients can stream but won't show play history or resume positions.

**Documented**: See `/design/requirements/podcast-requirements.md` Phase 2 section for detailed requirements.

## Files Changed

**Modified**:
- `/src/Melodee.Blazor/Controllers/OpenSubsonic/ControllerBase.cs` - Added debug logging
- `/src/Melodee.Blazor/Controllers/OpenSubsonic/PodcastController.cs` - Fixed role check, added logging
- `/src/Melodee.Common/Services/OpenSubsonicApiService.cs` - Added debug logging
- `/src/Melodee.Blazor/Components/Pages/Data/PodcastDetail.razor` - Removed BaseUrlService, use relative URL
- `/design/requirements/podcast-requirements.md` - Documented Phase 2 OpenSubsonic gaps

**Created**:
- `/docs/blazor-audio-streaming-authentication.md`
- `/docs/podcast-playback-auth-fix.md`
- `/docs/podcast-playback-auth-debugging.md`

## Next Steps

1. **Test the fix** - Run application and verify episode playback works
2. **Review logs** - Confirm role check is being skipped correctly
3. **Test edge cases** - Verify bookmark resume, scrobbling, multiple episodes
4. **Remove debug logging** - Clean up verbose debug logs once confirmed working
5. **Mark Phase 1 complete** - Update podcast-requirements.md status
