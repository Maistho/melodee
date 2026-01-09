<!-- markdownlint-disable-file -->

## Podcast Support — Phased Implementation Guide (Melodee)

This document is an implementation guide for coding agents.
It is derived from `design/requirements/podcast-requirements.md`.

### Goals of this guide

- Provide an unambiguous, step-by-step build plan.
- Remove “design choice” decisions from implementation.
- Provide a repeatable template for coding agents to implement a single phase.

### Immutable decisions (do not reinterpret)

1. **Master feature flag**: All podcast functionality is gated by `podcast.enabled` (default `true`). When `false`, **Podcasts must not appear anywhere** (NavMenu, header search results, Advanced search entity dropdown, pages/components) and all podcast handlers should short-circuit as “feature disabled”.
2. **Authorization**: When enabled, all podcast capabilities are gated by `User.HasPodcastRole`.
3. **MVP storage model**: Podcasts are stored **per-user** (no cross-user deduplication in Phase 1).
4. **Storage root and paths**: Podcast media is stored under a dedicated **Podcast Library** root using server-generated IDs:
   - Create a new `Library` with type `Podcast`.
   - All podcast storage uses the library path as the podcast root.
   - Example: `{podcastLibraryPath}/{userId}/{channelId}/{episodeId}.{ext}`
5. **Configuration keys**: All podcast-related configuration uses the `podcast.*` prefix (e.g. `podcast.enabled`). Do **not** hardcode podcast settings (network, storage, limits, schedules); use the existing configuration/options patterns.
6. **Network resiliency**: Feed fetch and enclosure download must use Polly following existing solution patterns (retry/backoff + timeouts) to improve resiliency.
7. **Episode identity**: Episode refresh/upsert uses a stable per-channel key:
   - Preferred: feed item `<guid>`
   - Fallback: stable hash of `(EnclosureUrl + PublishDate + Title)`
8. **OpenSubsonic ID encoding (no collisions with music IDs)**:
   - Channel ID: `podcast:channel:{channelId}`
   - Episode ID: `podcast:episode:{episodeId}`
9. **Streaming**: Podcast episode streaming must support **HTTP Range requests**.
10. **Downloads**:
    - Must be atomic: download to temp file, then move into place.
    - Temporary/failed download files must be cleaned up.
    - Queue ordering is FIFO by enqueue time.
11. **Refresh backoff**:
    - `Backoff = min(InitialInterval * (2^ConsecutiveFailureCount), MaxBackoff)`; reset on success.
12. **Security**:
    - SSRF protection is mandatory (scheme allow-list, port allow-list, private IP blocklist).
    - Must mitigate DNS rebinding: validate resolved IP at connection time (avoid TOCTOU between “check” and “fetch
    - Redirects: max 5 per request chain; re-validate after each redirect.
13. **Channel delete behavior**:
    - Channel delete is soft-delete and enqueues async cleanup of downloaded media (default).

---

## Phase Map (checklist)

- [x] Phase 0 — Repository discovery & scaffolding (no feature behavior)
- [x] Phase 1 — MVP: Data model + jobs + OpenSubsonic endpoints + minimal Blazor UI
- [x] Phase 1.1 — MVP: Cover art caching + cover art serving integration
- [x] Phase 1.2 — MVP: Streaming via existing primitives with Range support verified
- [x] Phase 1.3 — MVP: Search + MQL integration (Podcasts in global search + Advanced mode)
- [x] Phase 2 — Operational hardening (recovery, retention, quotas, better observability)
- [x] Phase 3 — Broader integration (native API if not done earlier; optional Jellyfin mapping)

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

- No DB changes in Phase 0 (Phase 1+ requires EF Core model updates + new migrations).
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
- **Phase 1 deliverable**: add one or more NEW migrations for the podcast tables/indexes/seed data. Do not manually edit the DB.
- Use the standard Melodee context/projects when generating migrations:

```bash
dotnet ef migrations add AddPodcastTables \
  --project src/Melodee.Common/Melodee.Common.csproj \
  --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj \
  --context MelodeeDbContext

# Validate locally (no manual DB edits)
dotnet ef database update \
  --project src/Melodee.Common/Melodee.Common.csproj \
  --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj \
  --context MelodeeDbContext
```

- Commit all generated migration artifacts (migration `.cs`, `.Designer.cs`, and `MelodeeDbContextModelSnapshot.cs`).

### 2) Storage

#### Requirements

- Create a new `Library` entry of type `Podcast`.
  - This is a DB-backed concept in Melodee (`Library` rows are seed data).
  - Implement by adding a new `LibraryType.Podcast` enum value and seeding a `Library` row in `MelodeeDbContext` (similar to Inbound/Staging/Storage/Templates).
  - Create a NEW EF migration for the enum/seed changes (do not manually insert/update the DB).
  - Recommended default path: `/storage/podcasts/`.
  - Recommended env override (follow existing pattern in `StartupMelodeeConfigurationService`): `MELODEE_PODCASTS_PATH`.
- **All podcast media and cover art storage must be rooted at the Podcast Library path** (this is the podcast “storage root”).
- Only server-controlled relative paths are stored (relative to the Podcast Library root).
- Download writes:
  - Download to temp file in same filesystem.
  - Atomically move to final path.
  - Ensure temp/failed files are cleaned up.

### 3) Feed parsing and refresh

#### Requirements

- Fetch and parse RSS/Atom for a channel.
  - Must use Polly-based resiliency policies (follow existing solution patterns) for transient network failures.
- Store/update channel metadata (title/description/site/image link).
- Store/update episodes:
  - Use `EpisodeKey` for idempotent upserts.
  - Cap number of items processed/stored per channel (configurable via `podcast.*` settings).

#### Podcast configuration (DB-backed settings)

Podcast behavior is controlled via Melodee settings (the `Settings` table). For each setting used by code in this phase:

- Add a constant to `SettingRegistry`.
  - Keys must start with `podcast.` (podcast feature/settings) or `jobs.podcast` (podcast job schedules).
- Add seed data in `MelodeeDbContext` (`modelBuilder.Entity<Setting>().HasData(...)`).
- Generate a NEW migration so a clean DB gets defaults without manual edits.

Minimum required keys for Phase 1 (exact names):

- `podcast.enabled` (bool, default `true`)
- `podcast.http.allowHttp` (bool, default `false`)
- `podcast.http.timeoutSeconds` (int, default `30`)
- `podcast.http.maxRedirects` (int, default `5`)
- `podcast.http.maxFeedBytes` (int, default `10485760`)
- `podcast.refresh.maxItemsPerChannel` (int, default `500`)
- `podcast.download.maxConcurrent.global` (int, default `2`)
- `podcast.download.maxConcurrent.perUser` (int, default `1`)
- `podcast.download.maxEnclosureBytes` (long, default `2147483648`)
- `jobs.podcastRefresh.cronExpression` (string, default `"0 */15 * * * ?"`)
- `jobs.podcastDownload.cronExpression` (string, default `"0 */5 * * * ?"`)

#### HTTP caching

- Use `If-None-Match` for stored `ETag`.
- Use `If-Modified-Since` for stored `Last-Modified`.

### 4) Download job

#### Requirements

- `PodcastDownloadJob` processes queued episodes.
- FIFO ordering by enqueue time.
- Download must use Polly-based resiliency policies (follow existing solution patterns) for transient network failures.
- Enforce (all configurable via `podcast.*` settings):
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
- When `podcast.enabled=false`, podcasts are not visible/available anywhere in the application.
- When enabled, a user with `HasPodcastRole=false` cannot create/list/refresh/download podcasts.
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

## Phase 1.3 — MVP: Search + MQL integration

### Scope

Make podcasts discoverable in the global UI search and queryable via MQL (the “Advanced mode” on `/search`).

### Requirements

#### 1) Podcasts should be searchable (header/global search)

- **Header search must include podcasts** (when `podcast.enabled=true`):
  - Header search navigates to `/search/{Query}` (see `MainLayout.razor`) and renders results via `Search.razor`.
  - Update the simple search pipeline so `SearchService.DoSearchAsync(...)` can return podcast results alongside artists/albums/songs.
- Extend search models and include flags:
  - Add `SearchInclude.PodcastChannels` and `SearchInclude.PodcastEpisodes` (or a single `SearchInclude.Podcasts`) to `Melodee.Common.Models.Search.SearchInclude`.
  - Extend `Melodee.Common.Models.Search.SearchResult` to carry podcasts:
    - Recommended: `PodcastChannelDataInfo[] PodcastChannels` + `int TotalPodcastChannels`
    - Recommended: `PodcastEpisodeDataInfo[] PodcastEpisodes` + `int TotalPodcastEpisodes`
- Implement the backend query in `Melodee.Common.Services.SearchService`:
  - Query `PodcastChannel` / `PodcastEpisode` for the current user.
  - Filter by normalized text using the same conventions as music search.
    - Prefer `*Normalized` columns.
    - If the required normalized columns do not exist yet for podcasts, add them (and a NEW migration) as part of Phase 1.3.
  - Ensure podcast search results are **gated**:
    - If `podcast.enabled == false`, podcast arrays must be empty and UI must not render podcast sections.
    - If `User.HasPodcastRole == false`, podcast arrays must be empty regardless of requested include flags.
- Update `src/Melodee.Blazor/Components/Pages/Search.razor` (simple mode):
  - Include podcasts in the “no results found” condition.
  - Add a `RadzenPanel` section for podcasts (channels and/or episodes).
  - Navigation targets:
    - Podcast channel results link to the podcast channel details UI.
    - Podcast episode results link to the channel detail page with episode highlighted (or a dedicated episode view if it exists).
- (Optional but recommended) Update `api/v1/search/suggest`:
  - Add podcast suggestions so future autocomplete UIs can include podcasts.
  - Return `type: "podcast"` (and/or `podcast-episode`) and a thumbnail URL pointing at cached cover art.

#### 2) “Advanced” search option should have a “Podcasts” dropdown

The `/search` page has an Advanced mode that currently exposes MQL with an entity dropdown (`all/songs/albums/artists`).

- Add `podcasts` to the entity dropdown in `Search.razor` and localize the label (only when `podcast.enabled=true`).
- Render results for podcasts in Advanced mode:
  - Recommended: treat “Podcasts” as **PodcastEpisode** results (episodes are the playable/searchable unit).
  - Show at minimum: channel title, episode title, publish date, and download status.

#### 3) MQL should work with Podcasts (parity with artists/albums/songs)

- Add a new MQL entity type (only when `podcast.enabled=true`):
  - Entity name: `podcasts` (recommended) mapped to `PodcastEpisode`.
- Update `Melodee.Mql.MqlFieldRegistry`:
  - Add a new `"podcasts"` entry with fields and EF mappings.
  - Minimum fields (Phase 1.3):
    - `title` (default operator: `contains`) → `PodcastEpisode.TitleNormalized`
    - `channel` (default operator: `contains`) → `PodcastEpisode.PodcastChannel.TitleNormalized`
    - `published` → `PodcastEpisode.PublishDate`
    - `downloaded` (boolean) → derived from `PodcastEpisode.DownloadStatus`
    - `duration` → `PodcastEpisode.Duration`
- Implement a compiler:
  - Add `MqlPodcastEpisodeCompiler` (mirrors `MqlSongCompiler` style) that compiles AST → `Expression<Func<PodcastEpisode,bool>>`.
  - Ensure operator handling matches the existing compilers (equals, comparisons, contains/startsWith/endsWith/wildcard, range).
- Update validation and suggestions:
  - `MqlValidator` already uses `MqlFieldRegistry.GetEntityTypes()`; ensure `podcasts` is included.
  - Update `MqlSuggestionService.DetectContext(...)` to include `podcasts` when deciding whether a token is a field name.
- Extend the Blazor MQL execution pipeline:
  - Add `SearchPodcastsAsync(...)` to `IMqlSearchService` and implement it in `MqlSearchService`.
  - Update `SearchAllAsync(...)` to optionally include podcasts (recommended: include podcasts only when the user has `HasPodcastRole`).
  - Update `Search.razor` advanced mode switch logic + result rendering to show the new podcast results.

### Acceptance checks

- Searching from the header (simple search) returns podcasts alongside artists/albums/songs.
- Advanced mode dropdown includes “Podcasts” and executing MQL queries returns podcast results.
- MQL validation/suggestions work for podcast fields (no “unknown field” errors for valid podcast fields).
- When `podcast.enabled=false`:
  - Podcasts do not appear in NavMenu, search results, or Advanced search dropdown, and
  - all podcast UI/pages/components are effectively unavailable.
- A user without `HasPodcastRole`:
  - does not see podcasts in results, and
  - cannot use the Podcasts dropdown / query podcasts via MQL.

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

`Phase 1.1 — MVP: Cover art caching + cover art serving integration`

### Inputs

- Requirements: `design/requirements/podcast-requirements.md`
- Implementation guide: `design/requirements/podcast-implementation.md`
- Phase to implement: `Phase 1.1` (exact)

### Non-negotiable constraints

- Do not reinterpret requirements or add features not listed in the phase.
- Do not change behavior/decisions listed under “Immutable decisions”.
- Database changes:
  - **Phase 0**: no DB changes.
  - **Phase 1+**: DB changes are expected and must be done **only** via EF Core model changes + NEW migrations.
  - Never manually edit the DB to “make it work” locally.
- EF Core migrations:
  - Do not edit existing migrations.
  - Generate a new migration from model/seed changes.
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
   - If the phase includes DB/model changes:
     - Generate a NEW migration (see Phase 1 migration rules).
     - Apply it locally with `dotnet ef database update` (no manual DB edits).
   - Manual smoke:
     - Add one known feed from the requirements doc
     - Refresh
     - Download one episode
     - Stream with seeking
5. **Produce a completion report**
   - List files modified/added.
   - List acceptance criteria and evidence (logs, endpoint calls, UI screenshots if available).
6. **Report**
   - Update the requirements doc marking the phase, and all phase items as completed.
   
### Output format

- Summary (1–3 bullets)
- Files changed (bulleted)
- Acceptance criteria checklist (copied from phase + checked)
- Follow-ups (if anything blocked)
```
