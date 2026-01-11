# Podcast Playback Tracking Implementation Summary

## Session Date: 2026-01-09

## Overview

Implemented proper per-user playback tracking for podcast episodes, following the "do it right" approach (Option B). This mirrors the existing music playback tracking infrastructure but specifically for podcasts.

## Changes Made

### 1. New Data Models Created

#### `UserPodcastEpisodePlayHistory.cs`
- **Purpose**: Tracks every time a user plays a podcast episode
- **Location**: `/src/Melodee.Common/Data/Models/UserPodcastEpisodePlayHistory.cs`
- **Key Fields**:
  - `UserId`, `PodcastEpisodeId` (foreign keys)
  - `PlayedAt` (timestamp using NodaTime Instant)
  - `Client`, `ByUserAgent`, `IpAddress` (tracking metadata)
  - `SecondsPlayed` (for scrobbling logic)
  - `Source` (default = 4 for Podcast)
  - `IsNowPlaying` (tracks currently playing episodes)
  - `LastHeartbeatAt` (detects stale/disconnected clients)

- **Indexes**:
  - `(UserId, PodcastEpisodeId, PlayedAt)` - user's play history
  - `(PodcastEpisodeId, PlayedAt)` - episode play history

#### `PodcastEpisodeBookmark.cs`
- **Purpose**: Stores resume position for podcast episodes per user
- **Location**: `/src/Melodee.Common/Data/Models/PodcastEpisodeBookmark.cs`
- **Key Fields**:
  - `UserId`, `PodcastEpisodeId` (foreign keys)
  - `PositionSeconds` (resume position)
  - `Comment` (optional note)
  - `CreatedAt`, `UpdatedAt` (timestamps)

- **Indexes**:
  - `(UserId, PodcastEpisodeId)` UNIQUE - one bookmark per user per episode

### 2. Database Changes

#### MelodeeDbContext.cs
- Added `DbSet<UserPodcastEpisodePlayHistory> UserPodcastEpisodePlayHistories`
- Added `DbSet<PodcastEpisodeBookmark> PodcastEpisodeBookmarks`

#### Migration: `20260109211500_AddPodcastPlaybackTracking.cs`
- **Creates Two Tables**:
  - `PodcastEpisodeBookmarks` (7 columns)
  - `UserPodcastEpisodePlayHistories` (11 columns)

- **Foreign Keys** (with CASCADE delete):
  - Both tables → `Users` (UserId)
  - Both tables → `PodcastEpisodes` (PodcastEpisodeId)

- **Applied Successfully** to PostgreSQL development database
- **Verified** tables exist with proper schema and indexes

### 3. Bug Fixes (Done Earlier in Session)

#### XML DTD Processing Error - FIXED
- **File**: `/src/Melodee.Common/Services/PodcastService.cs` line 488
- **Change**: `DtdProcessing.Prohibit` → `DtdProcessing.Ignore`
- **Reason**: Many podcast feeds include `<!DOCTYPE>` declarations for compatibility
- **Security**: Still safe - `XmlResolver = null` prevents external entity attacks

#### Missing Localization Key - FIXED
- **Added**: `Podcast.ChannelNotFound` to all 10 language resource files
- **Resource Count**: Now 1,580 keys per file (was 1,579)

### 4. UI Debug Panel

#### Podcasts.razor
- Added temporary debug panel showing:
  - `PodcastEnabled` status
  - `CurrentUsersId` value
  - `Count` (total episodes)
  - `Channels` (loaded count)
  - `IsLoading` state
- **Purpose**: Diagnose why grid was showing empty despite database records
- **Location**: After breadcrumb, before main content

## Design Decisions

### Why Two Separate Tables?

**Following DDD Principles**: Separate concerns and aggregate boundaries

1. **UserPodcastEpisodePlayHistory** - Audit/Analytics
   - Immutable append-only records
   - Never updated, only inserted
   - Used for: play statistics, history, recommendations
   - Multiple records per user per episode

2. **PodcastEpisodeBookmark** - User State
   - Mutable current state
   - Updated on every position change
   - Used for: resume playback, UX continuity
   - One record per user per episode (UNIQUE constraint)

### Why Not Inherit from MetaDataModelBase?

**Bookmarks are Simple State**, not domain entities with full metadata:
- Don't need MusicBrainzId, SpotifyId, etc.
- Don't need ApiKey, Tags, AlternateNames
- Don't need rating calculations
- Keep table lean for frequent updates

### Mirrors Music Tracking

The implementation deliberately mirrors `UserSongPlayHistory` and `Bookmark`:
- Same field names where applicable
- Same index strategy
- Same foreign key patterns
- Same scrobbling concepts
- **Benefit**: Familiar patterns, reusable service logic

## Database Schema

### PodcastEpisodeBookmarks
```sql
Table "public.PodcastEpisodeBookmarks"
      Column      |           Type           | Nullable |
------------------+--------------------------+----------+
 Id               | integer                  | not null |
 UserId           | integer                  | not null |
 PodcastEpisodeId | integer                  | not null |
 PositionSeconds  | integer                  | not null |
 Comment          | character varying(1000)  |          |
 CreatedAt        | timestamp with time zone | not null |
 UpdatedAt        | timestamp with time zone | not null |

Indexes:
    "PK_PodcastEpisodeBookmarks" PRIMARY KEY
    "IX_PodcastEpisodeBookmarks_UserId_PodcastEpisodeId" UNIQUE
    "IX_PodcastEpisodeBookmarks_PodcastEpisodeId"

Foreign-key constraints:
    FK → PodcastEpisodes (ON DELETE CASCADE)
    FK → Users (ON DELETE CASCADE)
```

### UserPodcastEpisodePlayHistories
```sql
Table "public.UserPodcastEpisodePlayHistories"
      Column      |           Type           | Nullable |
------------------+--------------------------+----------+
 Id               | integer                  | not null |
 UserId           | integer                  | not null |
 PodcastEpisodeId | integer                  | not null |
 PlayedAt         | timestamp with time zone | not null |
 Client           | character varying(255)   | not null |
 ByUserAgent      | character varying(255)   |          |
 IpAddress        | character varying(255)   |          |
 SecondsPlayed    | integer                  |          |
 Source           | smallint                 | not null |
 IsNowPlaying     | boolean                  | not null |
 LastHeartbeatAt  | timestamp with time zone |          |

Indexes:
    "PK_UserPodcastEpisodePlayHistories" PRIMARY KEY
    "IX_..._PodcastEpisodeId_PlayedAt"
    "IX_..._UserId_PodcastEpisodeId_PlayedAt"

Foreign-key constraints:
    FK → PodcastEpisodes (ON DELETE CASCADE)
    FK → Users (ON DELETE CASCADE)
```

## Next Steps (TODO)

### Phase 2: Service Implementation (NEXT)
Create `PodcastPlaybackService` with:
- `RecordPlayAsync(userId, episodeId, client, secondsPlayed)`
- `UpdateNowPlayingAsync(userId, episodeId, heartbeat)`
- `GetPlayHistoryAsync(userId, episodeId?, pagination)`
- `SaveBookmarkAsync(userId, episodeId, positionSeconds)`
- `GetBookmarkAsync(userId, episodeId)`
- `DeleteBookmarkAsync(userId, episodeId)`
- Scrobbling logic (mark as played when 50%+ or 240+ seconds)

### Phase 3: API Endpoints
- **OpenSubsonic**:
  - `scrobble.view` (mark episode played)
  - `updateNowPlaying.view` (heartbeat)
  - `getBookmarks.view` (get resume positions)
  - `createBookmark.view` (save position)
  - `deleteBookmark.view` (remove bookmark)

- **Native API** (`/api/v1/podcasts/`):
  - `POST /episodes/{id}/play` (record play)
  - `POST /episodes/{id}/bookmark` (save position)
  - `GET /episodes/{id}/bookmark` (get position)
  - `GET /episodes/{id}/history` (play history)

### Phase 4: UI Integration
- Show "played" indicator on episodes
- Display resume position ("Resume from 12:34")
- "Mark as played/unplayed" buttons
- Play progress bars
- Play history view

### Phase 5: Testing
- Unit tests for service methods
- Integration tests for API endpoints
- Test scrobbling logic thresholds
- Test bookmark upsert behavior

## Testing Performed

1. ✅ **Build**: Full solution builds without errors
2. ✅ **Migration**: Generated and applied successfully
3. ✅ **Database**: Tables created with correct schema, indexes, and FKs
4. ✅ **Verification**: Queried PostgreSQL to confirm table structure
5. ⏸️ **Runtime**: Not yet tested (services not implemented)

## Files Modified/Created

### Created
- `/src/Melodee.Common/Data/Models/UserPodcastEpisodePlayHistory.cs`
- `/src/Melodee.Common/Data/Models/PodcastEpisodeBookmark.cs`
- `/src/Melodee.Common/Migrations/20260109211500_AddPodcastPlaybackTracking.cs`
- `/src/Melodee.Common/Migrations/20260109211500_AddPodcastPlaybackTracking.Designer.cs`
- `/docs/podcast-playback-tracking-analysis.md` (design document)
- `/scripts/test-podcast-feeds.sh` (feed validation script)

### Modified
- `/src/Melodee.Common/Data/MelodeeDbContext.cs` - Added DbSets
- `/src/Melodee.Common/Data/MelodeeDbContextModelSnapshot.cs` - Updated by EF Core
- `/src/Melodee.Common/Services/PodcastService.cs` - Fixed DTD processing (line 488)
- All 10 resource files in `/src/Melodee.Blazor/Resources/` - Added `Podcast.ChannelNotFound` key
- `/src/Melodee.Blazor/Components/Pages/Data/Podcasts.razor` - Added debug panel

## Documentation Created

1. **podcast-playback-tracking-analysis.md** - Comprehensive analysis of options, trade-offs, and design decisions
2. **This summary** - Implementation details and next steps

## Validation Checklist

- [x] Models follow C# conventions and DDD principles
- [x] Models mirror existing music tracking patterns
- [x] Foreign keys properly configured with CASCADE delete
- [x] Indexes match query patterns (user+episode+time)
- [x] Unique constraints prevent duplicate bookmarks
- [x] NodaTime Instant used for timestamps (timezone-aware)
- [x] Migration generated from model (not hand-rolled)
- [x] Migration applied successfully to development DB
- [x] Migration has proper Up() and Down() methods
- [x] Full solution builds without warnings or errors
- [x] Resource files validated and synchronized (1,580 keys)

## References

- **Requirements**: `/design/requirements/podcast-requirements.md` Section 5 (Playback + tracking)
- **Music Tracking Models**:
  - `/src/Melodee.Common/Data/Models/UserSongPlayHistory.cs`
  - `/src/Melodee.Common/Data/Models/Bookmark.cs`
- **EF Core Migrations Guide**: `.github/instructions/ef-core-migrations.instructions.md`

## Notes

- **Per-User Tracking**: Each user has independent play history and bookmarks
- **Scrobbling**: Will use same threshold as music (50%+ duration or 240+ seconds)
- **Now Playing**: Heartbeat mechanism detects stale "now playing" entries
- **Cascade Delete**: When user or episode deleted, all related tracking deleted automatically
- **Performance**: Indexes optimized for common queries (user's history, episode stats)
- **Security**: All fields follow existing validation patterns and max length constraints

---

**Status**: Phase 1 (Data Models) COMPLETE ✅  
**Next**: Phase 2 (Service Implementation) - Ready to implement
**Estimated Effort**: Service layer ~2-4 hours, APIs ~2-3 hours, UI ~1-2 hours
