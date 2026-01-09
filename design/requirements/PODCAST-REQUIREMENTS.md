## Podcast Support Requirements (Melodee)

### Status

Melodee currently exposes OpenSubsonic podcast routes (`/rest/*Podcast*.view`) but returns HTTP `501 Not Implemented`.
This document scopes what it would take to implement podcasts in a way that fits Melodee’s architecture (jobs-driven ingestion, multi-user roles, and multiple API surfaces).

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

- Darknet Diaries — `https://darknetdiaries.com/feed.xml`
- Syntax.fm — `https://feed.syntax.fm/rss`
- The Changelog — `https://changelog.com/podcast/feed`
- Talk Python — `https://talkpython.fm/episodes/rss`
- Software Engineering Daily — `https://softwareengineeringdaily.com/feed/podcast/`
- NPR Planet Money — `https://feeds.npr.org/510289/podcast.xml`
- NPR TED Radio Hour — `https://feeds.npr.org/510298/podcast.xml`

## Product scope and rollout plan

### Phase 1 (MVP): OpenSubsonic + basic admin UI

- Create/delete/list channels per user.
- Refresh channels by polling RSS/Atom feeds.
- Display episodes and allow server-side download.
- Stream downloaded episodes via existing streaming/download primitives (or introduce podcast-specific streaming if required).
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
- Delete channel: marks channel deleted and (optionally) schedules cleanup of downloaded episode files.
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

### 4) Episode lifecycle

- Episodes must capture at least: title, publish date, description/summary, enclosure URL, enclosure mime type, enclosure byte length (when known).
- Download episode:
  - Transition state: `Queued` → `Downloading` → `Downloaded` or `Failed`.
  - Store local file metadata and a safe, server-controlled storage path.
  - If an episode is already downloaded, repeated download requests should be idempotent.
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
  - `Description` (string)
  - `SiteUrl` (string?)
  - `ImageUrl` (string?)
  - `Etag` (string?)
  - `LastModified` (DateTimeOffset?)
  - `LastSyncAt` (DateTimeOffset?)
  - `LastSyncAttemptAt` (DateTimeOffset?)
  - `LastSyncError` (string?)
  - `IsDeleted` (bool)
  - `CreatedAt` / `UpdatedAt`

- `PodcastEpisode`
  - `Id` (int)
  - `PodcastChannelId` (FK)
  - `Guid` (string?) (from feed item guid if available)
  - `Title` (string)
  - `Description` (string)
  - `PublishDate` (DateTimeOffset?)
  - `EnclosureUrl` (string)
  - `EnclosureLength` (long?)
  - `MimeType` (string?)
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
- `PodcastEpisode(PodcastChannelId, Guid)` unique when guid exists.

## Storage requirements

- Store podcast files under a dedicated subtree, not inside music library folders.
- Use IDs/guids for path generation (never use user-provided titles as path components).
- Example layout:
  - `podcasts/{userId}/{channelId}/{episodeId}.{ext}`
- Support configurable base directory/volume via existing configuration patterns.

## Background jobs (Quartz)

- `PodcastRefreshJob`
  - Runs periodically (configurable, e.g., every 30–60 minutes).
  - Refreshes channels with backoff for repeated failures.
  - Uses conditional HTTP requests (ETag / Last-Modified).

- `PodcastDownloadJob`
  - Processes queued episode downloads.
  - Enforces concurrency limits (global + per-user).
  - Enforces max file size and timeouts.

- `PodcastCleanupJob` (Phase 2)
  - Applies retention policies and deletes old downloaded media.

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
- Decide how episodes are streamed to OpenSubsonic clients:
  1. **Preferred**: represent episodes as “songs” with ids that work with existing `/rest/stream` and `/rest/download` endpoints.
  2. Alternative: add explicit podcast streaming endpoints (not part of OpenSubsonic spec; avoid unless necessary).

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
  - Block non-standard ports unless allow-listed.
  - Limit redirects (e.g., max 5) and re-validate after each redirect.

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
- Provide configuration knobs:
  - Refresh interval
  - Download concurrency
  - Max feed size
  - Max enclosure size
  - Storage root
  - Retention policy defaults

## Acceptance criteria (Phase 1)

- OpenSubsonic endpoints listed above return `200` and function end-to-end with at least one OpenSubsonic client known to support podcasts.
- A user with `HasPodcastRole=false` cannot create/list/refresh/download podcasts.
- Podcast refresh job can ingest a real-world feed and populate episodes.
- Downloaded episode can be streamed without exposing arbitrary file access.
- Jobs appear in the existing jobs monitoring UI with meaningful status.
