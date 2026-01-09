<!-- markdownlint-disable-file -->

## Podcast Support — Phased Implementation Guide (Melodee)

This document is an implementation guide for coding agents.
It is derived from `design/requirements/podcast-requirements.md`.

### Goals of this guide

- Provide an unambiguous, step-by-step build plan.
- Remove “design choice” decisions from implementation.
- Provide a repeatable template for coding agents to implement a single phase.

### Immutable decisions (do not reinterpret)

1. **Authorization**: All podcast capabilities are gated by `User.HasPodcastRole`.
2. **MVP storage model**: Podcasts are stored **per-user** (no cross-user deduplication in Phase 1).
3. **Storage root and paths**: Podcast media is stored under a dedicated podcasts subtree (not inside music library folders) using server-generated IDs:
   - `podcasts/{userId}/{channelId}/{episodeId}.{ext}`
4. **Episode identity**: Episode refresh/upsert uses a stable per-channel key:
   - Preferred: feed item `<guid>`
   - Fallback: stable hash of `(EnclosureUrl + PublishDate + Title)`
5. **OpenSubsonic ID encoding (no collisions with music IDs)**:
   - Channel ID: `podcast:channel:{channelId}`
   - Episode ID: `podcast:episode:{episodeId}`
6. **Streaming**: Podcast episode streaming must support **HTTP Range requests**.
7. **Downloads**:
   - Must be atomic: download to temp file, then move into place.
   - Temporary/failed download files must be cleaned up.
   - Queue ordering is FIFO by enqueue time.
8. **Refresh backoff**:
   - `Backoff = min(InitialInterval * (2^ConsecutiveFailureCount), MaxBackoff)`; reset on success.
9. **Security**:
   - SSRF protection is mandatory (scheme allow-list, port allow-list, private IP blocklist).
   - Must mitigate DNS rebinding: validate resolved IP at connection time (avoid TOCTOU between “check” and “fetch”).
   - Redirects: max 5 per request chain; re-validate after each redirect.
10. **Channel delete behavior**:
    - Channel delete is soft-delete and enqueues async cleanup of downloaded media (default).

---

## Phase Map (checklist)

- [ ] Phase 0 — Repository discovery & scaffolding (no feature behavior)
- [ ] Phase 1 — MVP: Data model + jobs + OpenSubsonic endpoints + minimal Blazor UI
- [ ] Phase 1.1 — MVP: Cover art caching + cover art serving integration
- [ ] Phase 1.2 — MVP: Streaming via existing primitives with Range support verified
- [ ] Phase 2 — Operational hardening (recovery, retention, quotas, better observability)
- [ ] Phase 3 — Broader integration (native API if not done earlier; optional Jellyfin mapping)

> Rule: implement phases in order; do not start Phase N+1 while Phase N has unchecked items.

---

## Phase 0 — Repository discovery & scaffolding

### Deliverables

- Identify the existing controller stubs returning `501 Not Implemented` (OpenSubsonic podcast endpoints).
- Identify existing Quartz job infrastructure and how jobs are registered/tracked.
- Identify existing streaming/download primitives and whether they already support Range requests.
- Identify existing cover art mechanism (`getCoverArt` or equivalent) used by OpenSubsonic.

### Required outputs

- A short note (in PR description or agent notes) listing:
  - Exact file paths for: podcast controller(s), job infrastructure, stream/download endpoints, cover art endpoints.
  - Whether Range requests are already supported; if not, list the minimal change required.

### Non-deliverables

- No DB changes.
- No new endpoints.

---

## Phase 1 — MVP: Data model + jobs + OpenSubsonic endpoints + minimal Blazor UI

### Scope

Implement podcasts end-to-end for a single user:
- Create/list/delete channels.
- Refresh feeds (manual trigger + scheduled job).
- Store episodes.
- Download episode media.
- Stream downloaded episode media.

### 1) Data model (EF Core)

#### Requirements

Create EF Core entities:
- `PodcastChannel`
- `PodcastEpisode`
- Optional later: `PodcastEpisodeBookmark` (NOT in Phase 1 unless explicitly required by UI).

Required fields (Phase 1)
- `PodcastChannel`
  - `Id`, `UserId`, `FeedUrl`, `Title`, `Description` (long text), `SiteUrl`, `ImageUrl`
  - `CoverArtLocalPath` (relative path)
  - `Etag`, `LastModified`
  - `LastSyncAt`, `LastSyncAttemptAt`, `LastSyncError`
  - `ConsecutiveFailureCount`, `NextSyncAt`
  - `IsDeleted`, `CreatedAt`, `UpdatedAt`
- `PodcastEpisode`
  - `Id`, `PodcastChannelId`, `Guid` (optional)
  - `EpisodeKey` (required)
  - `Title`, `Description` (long text), `PublishDate`
  - `EnclosureUrl`, `EnclosureLength` (optional), `MimeType` (optional)
  - `DownloadStatus`, `DownloadError`
  - `LocalPath` (relative path), `LocalFileSize` (optional), `Duration` (optional)
  - `CreatedAt`, `UpdatedAt`

Required indexes
- `PodcastChannel(UserId, FeedUrl)` unique
- `PodcastEpisode(PodcastChannelId, PublishDate)` index
- `PodcastEpisode(PodcastChannelId, EpisodeKey)` unique
- `PodcastEpisode(PodcastChannelId, DownloadStatus)` index

#### Migration rules

- **Never edit existing migrations**.
- Update models/configuration and generate a new migration via EF.

### 2) Storage

#### Requirements

- All podcast media under a dedicated podcast root directory.
- Only server-controlled relative paths are stored.
- Download writes:
  - Download to temp file in same filesystem.
  - Atomically move to final path.
  - Ensure temp/failed files are cleaned up.

### 3) Feed parsing and refresh

#### Requirements

- Fetch and parse RSS/Atom for a channel.
- Store/update channel metadata (title/description/site/image link).
- Store/update episodes:
  - Use `EpisodeKey` for idempotent upserts.
  - Cap number of items processed/stored per channel (configurable).

#### HTTP caching

- Use `If-None-Match` for stored `ETag`.
- Use `If-Modified-Since` for stored `Last-Modified`.

### 4) Download job

#### Requirements

- `PodcastDownloadJob` processes queued episodes.
- FIFO ordering by enqueue time.
- Enforce:
  - global concurrency limit
  - per-user concurrency limit
  - max enclosure size
  - timeouts

#### Metadata inference

- If `MimeType` / `EnclosureLength` missing from feed, infer from HTTP response headers during download where possible.

### 5) Refresh job

#### Requirements

- `PodcastRefreshJob` runs on schedule.
- Refresh uses backoff logic based on `ConsecutiveFailureCount` and `NextSyncAt`.

### 6) OpenSubsonic API endpoints

#### Requirements

Implement the routes currently stubbed in `PodcastController`:

- `getPodcasts`
  - Returns channels for the authenticated user.
  - Supports: `includeEpisodes`, `id`.
- `getNewestPodcasts`
  - Returns newest episodes across user channels.
  - Supports `count` and `offset`.
- `refreshPodcasts`
  - Enqueues refresh job; returns immediately with status ok.
- `createPodcastChannel`
  - Input: `url`.
  - Validates URL (SSRF rules).
  - Fetches and parses feed; creates channel + initial episode list.
- `deletePodcastChannel`
  - Input: `id`.
  - Soft deletes channel and enqueues cleanup.
- `downloadPodcastEpisode`
  - Input: `id`.
  - Enqueues download.
- `deletePodcastEpisode`
  - Input: `id`.
  - Deletes local media; preserves metadata unless OpenSubsonic behavior requires otherwise (document actual behavior in code-level docs).

#### ID mapping

- In responses, use stable, non-colliding ID encoding:
  - `podcast:channel:{channelId}`
  - `podcast:episode:{episodeId}`

#### Response envelope

- Must return standard `subsonic-response` envelope in JSON and XML.

### 7) Blazor UI (minimum)

User-facing:
- Channel list (last refresh time + last error)
- Add channel form (feed URL)
- Channel detail: episode list, download status, download/delete actions

Admin-facing:
- Ensure existing user role management can disable podcast role.
- Ensure Jobs UI shows podcast jobs using existing job history/tracking.

Localization:
- Any new UI strings must be added to all 10 resource files.

### 8) Security implementation (MVP)

- Schemes: allow-list `https` (default), optionally `http` behind explicit config.
- Ports: allow-list default 443; allow 80 only if http enabled.
- DNS/IP checks:
  - Block private, loopback, link-local, multicast.
  - Mitigate DNS rebinding by validating resolved IP at connection time.
- Redirects: max 5 per request chain, re-validate each hop.
- Feed size limit and timeouts.
- Secure XML parsing: disable DTD/external entities.

### Phase 1 acceptance checks

- OpenSubsonic endpoints return `200` and function end-to-end with at least one podcast-capable OpenSubsonic client.
- A user with `HasPodcastRole=false` cannot create/list/refresh/download podcasts.
- Refresh job ingests a real-world feed and populates episodes.
- Downloaded episode can be streamed without arbitrary file access.
- Podcast jobs appear in jobs monitoring UI.

---

## Phase 1.1 — MVP: Cover art caching + cover art serving integration

### Scope

Ensure OpenSubsonic clients can retrieve channel cover art through server-provided art, not remote URLs.

### Requirements

- On channel create (and optionally refresh), download podcast artwork to local storage.
- Store `CoverArtLocalPath` on `PodcastChannel`.
- Serve artwork via existing cover art mechanism (e.g., `getCoverArt` mapping).

### Acceptance checks

- At least one OpenSubsonic client displays channel cover art from the server.
- Cover art requests cannot be used for arbitrary file access.

---

## Phase 1.2 — MVP: Streaming via existing primitives with Range support verified

### Scope

Confirm and, if missing, implement HTTP Range request support for streaming downloaded podcast media.

### Requirements

- Streaming endpoint used by podcast playback must support:
  - `Range` requests
  - correct `Content-Range` responses
  - `Accept-Ranges: bytes`

### Acceptance checks

- A podcast client can seek within an episode without restarting download.

---

## Phase 2 — Operational hardening

### Scope

Add resilience, cleanup, and operational controls.

### Required features

- `PodcastCleanupJob`
  - Retention policies
  - Delete old downloaded media
- `PodcastRecoveryJob`
  - Reset stuck `Downloading` episodes beyond threshold
  - Clean up orphaned temp files
- Quotas
  - per-user and/or per-channel quotas
- Better observability
  - refresh duration
  - refresh success rate
  - download success rate
  - bytes downloaded
- Better error handling
  - retries/backoff improvements for transient failures

### Acceptance checks

- System recovers from interrupted downloads without manual DB edits.
- Retention deletes media as configured without deleting metadata unexpectedly.

---

## Phase 3 — Broader integration

### Scope

Broaden API surfaces beyond OpenSubsonic and optional external mappings.

### Requirements

- Native REST API endpoints (if not already implemented earlier):
  - `GET /api/v1/podcasts/channels`
  - `POST /api/v1/podcasts/channels`
  - `DELETE /api/v1/podcasts/channels/{id}`
  - `POST /api/v1/podcasts/channels/{id}/refresh`
  - `GET /api/v1/podcasts/channels/{id}/episodes`
  - `POST /api/v1/podcasts/episodes/{id}/download`
  - `DELETE /api/v1/podcasts/episodes/{id}`
- Optional Jellyfin mapping only if explicitly requested by product direction.

---

## Coding Agent Template (use for any phase)

Copy/paste the following into a coding agent task.

```aiignore

### Task Title

`Podcasts — Implement Phase X` (replace X)

### Inputs

- Requirements: `design/requirements/podcast-requirements.md`
- Implementation guide: `design/requirements/podcast-implementation.md`
- Phase to implement: `Phase X` (exact)

### Non-negotiable constraints

- Do not reinterpret requirements or add features not listed in the phase.
- Do not change behavior/decisions listed under “Immutable decisions”.
- EF Core migrations:
  - Do not edit existing migrations.
  - Generate a new migration from model changes.
- Blazor localization:
  - Any new UI strings must be added to all 10 resource files.

### Step-by-step plan

1. **Locate existing implementation hooks**
   - Identify the exact files/classes that need changes for this phase.
2. **Implement phase deliverables only**
   - Create/modify only what the phase lists.
3. **Add/adjust configuration knobs**
   - Add config keys only if required by the phase.
4. **Validation**
   - Build: `dotnet build` (solution)
   - Tests: `dotnet test` (only existing tests)
   - Manual smoke:
     - Add one known feed from the requirements doc
     - Refresh
     - Download one episode
     - Stream with seeking
5. **Produce a completion report**
   - List files modified/added.
   - List acceptance criteria and evidence (logs, endpoint calls, UI screenshots if available).

### Output format

- Summary (1–3 bullets)
- Files changed (bulleted)
- Acceptance criteria checklist (copied from phase + checked)
- Follow-ups (if anything blocked)
```
