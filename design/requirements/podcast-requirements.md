## Podcast Support Requirements (Melodee)


### Status (Updated: 2026-01-09)

**Phase 1 (MVP) - IN PROGRESS**

✅ **Completed:**
- ✅ Data models for channels and episodes
- ✅ Database migrations applied  
- ✅ RSS/Atom feed parsing with DTD support
- ✅ Feed refresh infrastructure (conditional requests, ETag, Last-Modified)
- ✅ Episode download queue system
- ✅ Channel CRUD operations (create, list, delete)
- ✅ Episode CRUD operations
- ✅ HTTP Range request support for streaming
- ✅ SSRF protection for feed URLs
- ✅ OpenSubsonic endpoints: `getPodcasts`, `getNewestPodcasts`, `refreshPodcasts`, `createPodcastChannel`, `deletePodcastChannel`, `deletePodcastEpisode`, `downloadPodcastEpisode`, `streamPodcastEpisode`
- ✅ Blazor UI: Channel list, add channel dialog, channel detail with episodes
- ✅ Playback tracking data models: `UserPodcastEpisodePlayHistory`, `PodcastEpisodeBookmark`
- ✅ Playback tracking service: `PodcastPlaybackService` (NowPlaying, Scrobble, Bookmarks)
- ✅ **Blazor UI: Episode playback controls with JavaScript interop**
- ✅ **Blazor UI: Progress bar with seek and resume from bookmark**
- ✅ **Blazor UI: Auto-save bookmarks every 10 seconds during playback**
- ✅ **Blazor UI: "Now playing" heartbeat every 30 seconds**
- ✅ **JavaScript module: podcastPlayer.js for HTML5 audio control**

⏳ **Remaining for Phase 1:**
- ⏳ OpenSubsonic: Update `scrobble.view` endpoint to handle podcast episodes
- ⏳ OpenSubsonic: Update `getNowPlaying.view` to include podcast episodes
- ⏳ OpenSubsonic: Implement `getBookmarks.view` for podcast episodes
- ⏳ OpenSubsonic: Implement `createBookmark.view` for podcast episodes  
- ⏳ OpenSubsonic: Implement `deleteBookmark.view` for podcast episodes
- ⏳ Native API: `/api/v1/podcasts/episodes/{id}/play` endpoint
- ⏳ Native API: `/api/v1/podcasts/episodes/{id}/bookmark` endpoints
- ⏳ Blazor UI: Display played/unplayed indicators in episode list
- ⏳ Blazor UI: Show play history in episode detail

### Implementation Notes (2026-01-09)

**Playback Tracking Architecture:**
- Mirrors existing music scrobbling infrastructure (`UserSongPlayHistory` → `UserPodcastEpisodePlayHistory`)
- Separate bookmark table for resume positions per user (`PodcastEpisodeBookmark`)
- "Now playing" uses heartbeat mechanism with `IsNowPlaying` flag and `LastHeartbeatAt` timestamp
- Scrobbling logic: Mark episode as "played" when 50%+ duration or 240+ seconds
- All timestamps use NodaTime `Instant` for timezone-aware storage
- Foreign keys cascade delete (when user or episode deleted, tracking is also deleted)

**Blazor Playback Implementation (✅ COMPLETE):**
- JavaScript interop module (`podcastPlayer.js`) controls HTML5 Audio API
- Direct service injection (NO HTTP calls to backend API from Blazor components)
- Bookmark saves every 10 seconds during playback (timer-based)
- Heartbeat sends "now playing" every 30 seconds (timer-based)
- Scrobble on completion OR when 50%/240s threshold reached (prevents duplicates)
- Resume from bookmark on episode load if bookmark exists
- Proper resource disposal (timers, JS module, .NET object reference)
- Audio streaming via OpenSubsonic endpoint: `/rest/streamPodcastEpisode.view?id=podcast:episode:{id}`
- See `/docs/podcast-playback-blazor-implementation.md` for detailed implementation notes

**OpenSubsonic Integration (NEXT - Priority B):**
- Episode IDs use format: `podcast:episode:{episodeId}`
- `scrobble.view` needs to detect podcast episode IDs and route to `PodcastPlaybackService`
- Bookmarks stored per user, accessible via OpenSubsonic bookmark endpoints
- "Now playing" includes both songs and podcast episodes

---

### Original Status

Melodee exposes OpenSubsonic podcast routes and has implemented most of Phase 1.
This document scopes the complete podcast implementation in a way that fits Melodee's architecture (jobs-driven ingestion, multi-user roles, and multiple API surfaces).
### Goals

- Provide first-class podcast subscriptions (RSS/Atom) and episode playback.
- Implement OpenSubsonic podcast endpoints so Subsonic/OpenSubsonic clients that support podcasts can function.
- Fit Melodee’s job-based processing model (Quartz) and storage approach.
- Keep podcasts optional and isolated from the music library scanner.

### Non-goals (initially)

- Podcast “search/discovery” catalogs (Apple Podcasts, PodcastIndex, etc.).
- Transcripts, chapter markers, and advanced podcast-specific metadata.
- Full OPML import/export (nice-to-have later).
- Video podcasts (unless already supported by Melodee’s general media pipeline later).

### Terms

- **Channel**: a podcast feed subscription (RSS/Atom URL).
- **Episode**: an item in a channel feed.
- **Enclosure**: the media file URL for an episode.

### Example test feeds (free)

These are useful for development/testing because they are publicly accessible RSS feeds (no auth required):
- Accidental Tech Podcast — `https://atp.fm/episodes?format=rss`
- Adventure Zone — `https://www.maximumfun.org/podcasts/adventure-zone/rss`
- Brave Technologist — `https://rss.libsyn.com/shows/332183/destinations/2705240.xml`
- CodeNewbie — `https://www.codenewbie.org/podcast/feed`
- Command Line Heroes — `https://www.redhat.com/en/command-line-heroes-podcast/feed`
- Dark Matter — `https://www.maximumfun.org/podcasts/dark-matter/rss`
- Darknet Diaries — `https://feeds.megaphone.fm/darknetdiaries`
- Dissect — `https://feeds.megaphone.fm/dissect`
- NPR All Songs Considered — `https://feeds.npr.org/510019/podcast.xml`
- NPR Planet Money — `https://feeds.npr.org/510289/podcast.xml`
- NPR TED Radio Hour — `https://feeds.npr.org/510298/podcast.xml`
- Popcast (NYT) — `https://feeds.simplecast.com/W1rB_kgL`
- Rolling Stone Music Now — `https://feeds.megaphone.fm/rollingstonemusicnow`
- Science Vs — `https://feeds.megaphone.fm/sciencevs`
- Software Engineering Daily — `https://softwareengineeringdaily.com/feed/podcast/`
- Switched on Pop — `https://feeds.megaphone.fm/switchedonpop`
- Syntax.fm — `https://feed.syntax.fm/rss`
- TED Talks Daily — `https://feeds.feedburner.com/TEDTalks_audio`
- Talk Python — `https://talkpython.fm/episodes/rss`
- The Changelog — `https://changelog.com/podcast/feed`
- The Daily (NYT) — `https://feeds.simplecast.com/54nAGcIl`

## Product scope and rollout plan

### Phase 1 (MVP): OpenSubsonic + basic admin UI

- Create/delete/list channels per user.
- Refresh channels by polling RSS/Atom feeds.
- Display episodes and allow server-side download.
- Cache channel artwork locally (basic cover art caching so OpenSubsonic clients can consistently render art).
- Stream downloaded episodes via existing streaming/download primitives.
  - Streaming must support HTTP Range requests (seeking) for good podcast client compatibility.
- Minimal Blazor UI for managing channels and viewing download status.

### Phase 2: Quality-of-life and operational hardening

- Retention policies (keep last N episodes, keep for X days, keep unplayed only).
- Per-user / per-channel quotas.
- Better error feedback and retries/backoff.
- Artwork caching and refresh.

### Phase 3: Broader integration

- Native REST API endpoints for non-OpenSubsonic clients.
- Optional Jellyfin API mapping (if a compelling client need exists).

## Functional requirements

### 1) Authorization & visibility

- Gate all podcast capabilities behind `User.HasPodcastRole`.
- Default behavior should match current migrations/config (currently `HasPodcastRole` defaults to `true`), but admins must be able to disable it per user.
- Streaming/download of podcast media should also respect existing streaming/download constraints if Melodee models them separately (e.g., if `HasStreamRole` / `HasDownloadRole` is required in other surfaces, define and document the rule for podcasts).

### 2) Channel management

- Add channel: user provides a feed URL.
  - Validate feed URL (see Security requirements).
  - Fetch and parse feed; store channel metadata (title, description, image, site link).
  - Create initial episode list from feed items (do not auto-download by default).
- Delete channel:
  - Marks channel deleted and enqueues cleanup of downloaded episode files (default; cleanup is async).
- List channels: supports pagination and lightweight summaries.

### 3) Feed refresh

- Support manual refresh for a specific channel and bulk refresh for a user.
- Support automatic refresh by job schedule.
- Refresh must use conditional requests when possible:
  - If feed previously provided `ETag`, use `If-None-Match`.
  - If feed previously provided `Last-Modified`, use `If-Modified-Since`.
- Refresh outcome should record:
  - Last refresh attempt time, last successful refresh time.
  - HTTP status summary and error message (sanitized) for UI.
  - Episode adds/updates/deletes as appropriate.
- Large feeds:
  - The server must cap the number of items processed/stored per channel (configurable) to avoid unbounded growth.

### 4) Episode lifecycle

- Episodes must capture at least: title, publish date, description/summary, enclosure URL, enclosure mime type, enclosure byte length (when known).
- Feeds frequently omit `enclosure` metadata (mime type/length). When missing, infer where possible (e.g., from HTTP response headers during download) and otherwise leave null.
- Download episode:
  - Transition state: `Queued` → `Downloading` → `Downloaded` or `Failed`.
  - Store local file metadata and a safe, server-controlled storage path.
  - If an episode is already downloaded, repeated download requests should be idempotent.
- Episode identity (deduplication within a channel):
  - Episodes must have a stable key used for upserts during refresh.
  - Preferred: feed item `<guid>`.
  - Fallback: a stable hash of `(EnclosureUrl + PublishDate + Title)`.
- Delete episode:
  - Removes local file if downloaded.
  - Keeps episode metadata record (preferred) or deletes record (acceptable) depending on OpenSubsonic semantics; document behavior.

### 5) Playback + tracking

- Episodes must be streamable after download.
- Track “played” events similarly to music (at least last played time; optional play count).
- Support resume position (nice-to-have): store per-user episode bookmark.

## Data model (EF Core)

> Note: Migrations must be generated from model changes (do not hand-edit existing migrations).

### Entities

- `PodcastChannel`
  - `Id` (int)
  - `UserId` (FK → User)
  - `FeedUrl` (string, unique per user)
  - `Title` (string)
  - `Description` (string) (can be large; store as a long text column)
  - `SiteUrl` (string?)
  - `ImageUrl` (string?)
  - `CoverArtLocalPath` (string?) (server-controlled relative path)
  - `Etag` (string?)
  - `LastModified` (DateTimeOffset?)
  - `LastSyncAt` (DateTimeOffset?)
  - `LastSyncAttemptAt` (DateTimeOffset?)
  - `LastSyncError` (string?)
  - `ConsecutiveFailureCount` (int)
  - `NextSyncAt` (DateTimeOffset?) (for backoff)
  - `IsDeleted` (bool)
  - `CreatedAt` / `UpdatedAt`

- `PodcastEpisode`
  - `Id` (int)
  - `PodcastChannelId` (FK)
  - `Guid` (string?) (from feed item guid if available)
  - `Title` (string)
  - `Description` (string) (can be large; store as a long text column)
  - `PublishDate` (DateTimeOffset?)
  - `EnclosureUrl` (string)
  - `EnclosureLength` (long?)
  - `MimeType` (string?)
  - `EpisodeKey` (string) (stable per-channel identity; guid when available, else hash)
  - `DownloadStatus` (enum: None/Queued/Downloading/Downloaded/Failed)
  - `DownloadError` (string?)
  - `LocalPath` (string?) (server-controlled relative path only)
  - `LocalFileSize` (long?)
  - `Duration` (TimeSpan?) (if extracted)
  - `CreatedAt` / `UpdatedAt`

- Optional (Phase 2/3): `PodcastEpisodeBookmark`
  - `UserId`
  - `PodcastEpisodeId`
  - `PositionSeconds`
  - `UpdatedAt`

### Indexing

- `PodcastChannel(UserId, FeedUrl)` unique.
- `PodcastEpisode(PodcastChannelId, PublishDate)` index for newest queries.
- `PodcastEpisode(PodcastChannelId, EpisodeKey)` unique.
- `PodcastEpisode(PodcastChannelId, DownloadStatus)` index for queue/status queries.

## Storage requirements

- Store podcast files under a dedicated subtree, not inside music library folders.
- Use IDs/guids for path generation (never use user-provided titles as path components).
- Example layout:
  - `podcasts/{userId}/{channelId}/{episodeId}.{ext}`
- Downloads must write to a temp file and then atomically move into place to avoid partial/corrupt media.
- Temporary/failed download files must be cleaned up.
- Support configurable base directory/volume via existing configuration patterns.

### Trade-offs (MVP)

- **No cross-user deduplication (initially):** In Phase 1, podcast channels and downloaded media are stored per-user. This keeps the model simple but can duplicate storage/bandwidth when many users subscribe to the same feed. Consider normalizing into shared `PodcastFeed`/`PodcastEpisode` plus user subscriptions as a Phase 2 optimization if this becomes a scaling concern.

## Background jobs (Quartz)

- `PodcastRefreshJob`
  - Runs periodically (configurable, e.g., every 30–60 minutes).
  - Refreshes channels with backoff for repeated failures.
    - Suggested algorithm: `Backoff = min(InitialInterval * (2^ConsecutiveFailureCount), MaxBackoff)`; reset on success.
  - Uses conditional HTTP requests (ETag / Last-Modified).

- `PodcastDownloadJob`
  - Processes queued episode downloads.
  - Queue ordering should be FIFO by enqueue time (fairness).
  - Enforces concurrency limits (global + per-user).
  - Enforces max file size and timeouts.
  - Uses atomic write (temp + move) to avoid corrupt files.

- `PodcastCleanupJob` (Phase 2)
  - Applies retention policies and deletes old downloaded media.

- `PodcastRecoveryJob` (Phase 2)
  - Resets episodes stuck in transient states (e.g., `Downloading`) beyond a configured threshold.
  - Cleans up orphaned temp files.

Jobs must integrate with existing job history/tracking so admins can see status in the Jobs UI.

## API requirements

### A) OpenSubsonic endpoints (must implement)

Implement the routes currently stubbed in `PodcastController`:

- `getPodcasts`
  - Parameters to support (at minimum):
    - `includeEpisodes` (bool; default `true` or match spec)
    - `id` (channel id filter)
  - Returns channels for the authenticated user.

- `getNewestPodcasts`
  - Returns newest episodes across channels for the authenticated user.
  - Support `count` and `offset` (or document limitations).

- `refreshPodcasts`
  - Triggers refresh for user channels (async job enqueue).
  - Return immediately with `status=ok`.

- `createPodcastChannel`
  - Parameters: `url` (feed URL).
  - Creates channel for authenticated user.

- `deletePodcastChannel`
  - Parameters: `id` (channel id).
  - Deletes channel for authenticated user.

- `deletePodcastEpisode`
  - Parameters: `id` (episode id).
  - Deletes local media + metadata handling as defined.

- `downloadPodcastEpisode`
  - Parameters: `id` (episode id).
  - Enqueues download.

#### OpenSubsonic response compatibility

- Must return standard `subsonic-response` envelope in JSON and XML.
- Episode and channel identifiers must be stable and map cleanly to Melodee entities.
- Define an ID encoding scheme that cannot collide with music IDs (examples):
  - Channel ID: `podcast:channel:{channelId}`
  - Episode ID: `podcast:episode:{episodeId}`
- Decide how episodes are streamed to OpenSubsonic clients:
  1. **Preferred**: represent episodes as “songs” with ids that work with existing `/rest/stream` and `/rest/download` endpoints.
  2. Alternative: add explicit podcast streaming endpoints (not part of OpenSubsonic spec; avoid unless necessary).

- Cover art:
  - OpenSubsonic clients frequently expect the server to provide cover art (e.g., via `getCoverArt`), not a remote URL.
  - Channel artwork should be cached locally (Phase 1) and served via existing cover art mechanisms.

### B) Native Melodee API (recommended for Blazor UI)

Add `/api/v1/podcasts/*` endpoints (Phase 1 if UI needs them; otherwise Phase 2):

- `GET /api/v1/podcasts/channels`
- `POST /api/v1/podcasts/channels` (create)
- `DELETE /api/v1/podcasts/channels/{id}`
- `POST /api/v1/podcasts/channels/{id}/refresh`
- `GET /api/v1/podcasts/channels/{id}/episodes`
- `POST /api/v1/podcasts/episodes/{id}/download`
- `DELETE /api/v1/podcasts/episodes/{id}`

All endpoints require authentication and `HasPodcastRole`.

### C) Jellyfin API mapping (optional)

- If implemented, treat podcast episodes as audio items in a separate “Podcasts” library/view.
- Only pursue if there is a specific client compatibility goal; otherwise keep podcasts on OpenSubsonic + native UI.

## Blazor UI requirements (admin + user)

Minimum set:

- User-facing:
  - Channel list with last refresh + error status.
  - Add channel form (feed URL).
  - Channel detail: episodes list with download status, download/delete actions.

- Admin-facing:
  - Ability to disable podcast role per user (already exists in User UI scaffolding).
  - Jobs page should surface podcast jobs like other jobs.

Localization:

- Any new UI strings must be added to all 10 `SharedResources.*.resx` files (use `scripts/add-localization-key.sh`).

## Security requirements (must-have)

### SSRF protection

- Validate feed/enclosure URLs:
  - Allow-list schemes: `https` (default), optionally `http` behind explicit config.
  - Resolve DNS and block private, loopback, link-local, and multicast IP ranges.
    - Mitigate DNS rebinding by validating the resolved IP at connection time (avoid TOCTOU between “check” and “fetch”).
  - Block non-standard ports unless allow-listed (defaults should be 443, plus 80 only if http is enabled).
  - Limit redirects (e.g., max 5 per request chain) and re-validate after each redirect.

### Safe parsing and size limits

- Enforce request timeouts.
- Enforce maximum response size for feeds.
- Use secure XML parsing settings (disable DTD/external entities).

### Path safety

- Never trust user input for file paths.
- Ensure streaming/download endpoints only allow access to paths owned by the podcast episode record and inside the configured podcast root.

## Observability & operations

- Log per-channel refresh outcomes (success/failure) with correlation to job runs.
- Surface last error reason (sanitized) in UI.
- Capture basic operational metrics (at minimum): refresh duration, refresh success rate, download success rate, bytes downloaded.
- Document backup/restore expectations (database rows + podcast storage subtree).
- Provide configuration knobs:
  - Refresh interval (default and min)
  - Download concurrency (global + per-user)
  - Max feed size
  - Max episodes per channel (cap for large feeds)
  - Max enclosure size
  - Timeouts (feed fetch and enclosure download)
  - Redirect limit
  - Allowed ports / scheme allow-list
  - Storage root
  - Retention policy defaults

## Acceptance criteria (Phase 1)

- OpenSubsonic endpoints listed above return `200` and function end-to-end with at least one OpenSubsonic client known to support podcasts.
- A user with `HasPodcastRole=false` cannot create/list/refresh/download podcasts.
- Podcast refresh job can ingest a real-world feed and populate episodes.
- Downloaded episode can be streamed without exposing arbitrary file access.
- Jobs appear in the existing jobs monitoring UI with meaningful status.
