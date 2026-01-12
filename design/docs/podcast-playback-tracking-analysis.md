# Podcast Playback Tracking Analysis

## Current State

### Existing Infrastructure for Music
Melodee already has robust playback tracking for music via:

1. **`UserSongPlayHistory`** - Tracks every time a user plays a song
   - `UserId`, `SongId`, `PlayedAt`, `Client`, `SecondsPlayed`, `IsNowPlaying`, `LastHeartbeatAt`
   - Supports "now playing" status with heartbeat
   - Tracks source (Stream, Share, Radio)

2. **`Bookmark`** - Stores resume position for songs
   - `UserId`, `SongId`, `Position`, `Comment`
   - Unique index on `(UserId, SongId)`

### Current Podcast Episode Model
`PodcastEpisode` currently has:
- Download status tracking
- File metadata (LocalPath, LocalFileSize, Duration)
- Basic episode metadata (Title, Description, PublishDate)
- **NO user-specific playback tracking fields**

## The Gap: User Playback Tracking for Podcasts

According to the requirements document (Phase 1, Section 5):
> "Track 'played' events similarly to music (at least last played time; optional play count).
> Support resume position (nice-to-have): store per-user episode bookmark."

### What's Missing

Since podcasts are **per-user** (each user has their own subscriptions), we need:

1. ✅ **Episode download tracking** - Already implemented via `DownloadStatus`
2. ❌ **Last played timestamp per user** - NOT implemented
3. ❌ **Play count per user** - NOT implemented  
4. ❌ **Resume position per user** - NOT implemented
5. ❌ **Completion status per user** - NOT implemented

### Why Current Models Don't Work

**`PodcastEpisode` is shared metadata** - It represents the episode itself, not a user's interaction with it.
- Multiple users can subscribe to the same channel
- Each user needs their own playback state
- Adding playback fields to `PodcastEpisode` would only track ONE user's state

**We need a join table like `UserSongPlayHistory` or `Bookmark` but for podcast episodes.**

## Proposed Solution

### Option 1: Extend Existing Music Tracking (Recommended for MVP)

Treat podcast episodes as "songs" for playback tracking purposes:

**Pros:**
- Reuse existing `UserSongPlayHistory` and `Bookmark` infrastructure
- No new tables needed
- Works with existing streaming/scrobbling code
- Faster to implement

**Cons:**
- Podcast episodes aren't technically "songs" (semantic mismatch)
- Foreign key would be to `SongId` which doesn't exist for podcasts
- Mixing music and podcast play history in same table

**Verdict:** ❌ This won't work without significant refactoring because `UserSongPlayHistory` has FK to `Song` table.

### Option 2: Create Podcast-Specific Tracking Tables (Proper Solution)

Create new tables mirroring the music tracking structure:

#### `UserPodcastEpisodePlayHistory`
```csharp
public class UserPodcastEpisodePlayHistory
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public required int PodcastEpisodeId { get; set; }
    public PodcastEpisode PodcastEpisode { get; set; } = null!;
    
    public Instant PlayedAt { get; set; }
    public string Client { get; set; } = "Melodee";
    public string? ByUserAgent { get; set; }
    public string? IpAddress { get; set; }
    public int? SecondsPlayed { get; set; }
    public bool IsNowPlaying { get; set; }
    public Instant? LastHeartbeatAt { get; set; }
}
```

#### `PodcastEpisodeBookmark` (Phase 2 as noted in requirements)
```csharp
public class PodcastEpisodeBookmark
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public required int PodcastEpisodeId { get; set; }
    public PodcastEpisode PodcastEpisode { get; set; } = null!;
    
    public int PositionSeconds { get; set; }
    public Instant UpdatedAt { get; set; }
}

// Unique index: (UserId, PodcastEpisodeId)
```

**Pros:**
- Clean separation of concerns
- Proper foreign keys
- Can have podcast-specific fields if needed
- Follows DDD principles
- Matches requirements document exactly

**Cons:**
- More tables to maintain
- Duplicate logic for playback tracking

**Verdict:** ✅ This is the correct approach per the requirements.

### Option 3: Generic Playback Tracking (Future-Proof)

Create a polymorphic tracking system for all media types:

```csharp
public enum MediaType { Song, PodcastEpisode, Video, etc. }

public class UserMediaPlayHistory
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public required MediaType MediaType { get; set; }
    public required int MediaId { get; set; } // Polymorphic FK
    public Instant PlayedAt { get; set; }
    // ... other fields
}
```

**Pros:**
- Single table for all media types
- Future-proof for video, audiobooks, etc.
- Unified analytics

**Cons:**
- Violates database normalization (no real FK)
- More complex queries
- Harder to maintain referential integrity
- Overkill for current needs

**Verdict:** ❌ Too complex for Phase 1, save for later if needed.

## Recommendation for Implementation

### Phase 1 (MVP) - Immediate
1. **Track last played only** via existing `PodcastEpisode` fields:
   - Add `LastPlayedAt` (DateTimeOffset?) to `PodcastEpisode`
   - Add `PlayCount` (int) to `PodcastEpisode`
   - **NOTE**: This only tracks the FIRST user to play it, not per-user tracking
   - This is a compromise for MVP to get basic tracking working

### Phase 2 - Proper Implementation
1. Create `UserPodcastEpisodePlayHistory` table
2. Create `PodcastEpisodeBookmark` table
3. Implement services to:
   - Record play events
   - Update resume positions
   - Query user's episode history
4. Integrate with OpenSubsonic scrobbling endpoints

### Why This Approach?

The requirements say: *"Track 'played' events **similarly to music**"*

Looking at music tracking:
- Songs have `Song.LastPlayedAt` (when anyone last played it)
- But `UserSongPlayHistory` tracks individual user plays
- This gives both aggregate stats and per-user history

For podcasts, we should mirror this:
- `PodcastEpisode.LastPlayedAt` - when anyone last played
- `UserPodcastEpisodePlayHistory` - individual user plays
- `PodcastEpisodeBookmark` - resume positions

## Questions for User

1. **MVP vs Complete**: Do you want basic tracking (Option 1) in Phase 1, or should I implement the full solution (Option 2) now?

2. **Multi-user consideration**: Since podcasts are per-user subscriptions, should we track:
   - Just the owner's playback?
   - Any user who has access (if sharing is implemented)?

3. **OpenSubsonic compatibility**: The spec expects podcast episodes to work like songs for playback. Should we:
   - Map episodes to the existing song streaming/scrobbling endpoints?
   - Create separate podcast-specific endpoints?

## Current Status

**What exists:**
- Episode download tracking ✅
- Episode metadata storage ✅
- Refresh/sync infrastructure ✅

**What's missing for playback tracking:**
- Per-user play history ❌
- Resume position bookmarks ❌
- Last played timestamp ❌
- Play count ❌

**Requirements say Phase 1 needs:**
> "Track 'played' events similarly to music (at least last played time; optional play count)."

**Requirements say Phase 2/3 can have:**
> "Optional (Phase 2/3): `PodcastEpisodeBookmark`"

So for Phase 1, we minimally need:
1. Last played time tracking
2. Optionally play count

For Phase 2, we should add:
1. Full play history
2. Resume position bookmarks
