# Requests Feature Spec (Performance-First)

**Source Requirements**: `prompts/REQUESTS-REQUIREMENT.md`

This document is the implementation specification for adding **Requests** to Melodee. It is written to optimize for:

1. **Hot-path query performance** (index page, navbar unread indicator, dashboard unread list)
2. **Predictable pagination** (stable sort keys; avoid expensive OFFSET scans where possible)
3. **Low write amplification** (denormalize only what we need, and only when it’s proven hot)

## Coding Agent Template

```
You are a coding agent working in the Melodee repo. Implement EXACTLY ONE phase of the Requests feature per `prompts/REQUESTS-SPEC.md` (source reqs:
   `prompts/REQUESTS-REQUIREMENT.md`).

     PHASE
     - Phase 0: Data model + migrations

     HARD RULES (from spec)
     - Performance-first: normalize on write; sargable indexed filters; deterministic sorts.
     - IDs in URLs/DTOs are `api_key` (uuid) only.
     - REST controllers (`src/Melodee.Blazor/Controllers/Melodee/*`) are for external clients only.
     - Blazor UI MUST use services via DI (must NOT call `/api/...`).
     - Enforce invariants/permissions exactly as specified (creator-only, delete only Pending, etc.).

     DO
     1) Read the spec sections for Phase 0.
     2) Implement the phase using existing Melodee patterns (EF models/migrations, services in `Melodee.Common.Services`, controllers in `Melodee.Blazor` when in scope).
     3) Add/update tests for phase logic (prioritize services layer).
     4) Validate: `dotnet build` and `dotnet test` (fix phase-related failures only).

     OUTPUT
     - Summary of changes (bullets)
     - Files changed (relative paths)
     - Verification commands
     - Any deviations (should be none)

     Start by listing the files you expect to touch and a short checklist for Phase 0.
 
```

---

## Phase Map (Progress)

| Phase | Name | Status | Deliverable |
| ----- | ---- | ------ | ----------- |
| 0 | Data model + migrations | ✅ | Tables + indexes + constraints |
| 1 | Requests API (non-admin) | ✅ | `/api/v1/requests*` CRUD + complete |
| 2 | Comments API (non-admin) | ✅ | `/api/v1/requests/{requestApiKey}/comments*` |
| 3 | Requests UI (index/detail/create/edit) | ✅ | Blazor pages + navbar item |
| 4 | Activity tracking (navbar + dashboard) | ✅ | unread model + endpoints + UI |
| 5 | Auto-completion + system comments | ✅ | event-driven matching |
| 6 | Perf hardening + docs | ✅ | query plans, indexes, README/docs |

---

## Scope

### In scope

- Requests: create/view/update/delete (creator-only), status transitions (creator-complete; admin transitions later)
- Comments: threaded (replies), Markdown, system comments
- Activity indicator: unread detection + navbar dot + dashboard “Request Activity” section
- Auto-completion: strict match on new album/song ingestion

### Out of scope (explicitly not required by this spec)

- Admin-only endpoints (reject/in-progress, edit others) — we will design the model to support them
- Real-time push notifications (SignalR) — polling endpoints are sufficient

---

## Architecture Overview

### Layers / responsibilities

- **Data (EF Core / Postgres)**: tables + indexes optimized for read patterns.
- **Domain-ish services** (in `Melodee.Common.Services`): RequestService, RequestCommentService, RequestActivityService.
- **REST controllers** (in `Melodee.Blazor/Controllers/Melodee`) for non-admin API **for external clients**.
- **Blazor UI (including admin)**: uses the services directly via DI/IOC (in-process) and MUST NOT call `/api/...` endpoints.
- **Event handlers**: listen for “album added” / “song added” events and attempt strict matching.

### Performance principles

- Compute/normalize once on write; avoid expensive normalization in WHERE clauses.
- Prefer indexed filters with highly selective columns first.
- Avoid N+1 user lookups: return a small `UserSummary` payload via join/projection.
- Prefer keyset pagination (cursor) for heavy tables if/when offset paging becomes slow; keep initial API offset-based as required.

---

## Data Model (PostgreSQL)

### New tables

#### `request`

Represents the user-submitted request.

**Core columns**

- `id` (bigint, PK) (internal)
- `api_key` (uuid, required, unique) — this is the only identifier used in URLs/DTOs
- `category` (smallint/int, required) — enum mapping:
  - AddAlbum, AddSong, ArtistCorrection, AlbumCorrection, General
- `status` (smallint/int, required) — enum mapping: Pending, InProgress, Completed, Rejected
- `description` (text, required) — Markdown

**Structured helper columns (nullable)**

- `artist_name` (text)
- `target_artist_api_key` (uuid)
- `album_title` (text)
- `target_album_api_key` (uuid)
- `song_title` (text)
- `target_song_api_key` (uuid, optional for future)
- `release_year` (int)
- `external_url` (text)
- `notes` (text)

**Audit columns**

- `created_at` (timestamp/timestamptz or NodaTime Instant, required)
- `created_by_user_id` (bigint/int, required FK -> user)
- `updated_at` (timestamp/Instant, required)
- `updated_by_user_id` (bigint/int, required FK -> user)

**Activity (denormalized, hot path)**

- `last_activity_at` (timestamp/Instant, required)
- `last_activity_user_id` (bigint/int, nullable FK -> user; null = system)
- `last_activity_type` (smallint/int, required) — enum: UserComment, SystemComment, StatusChanged, Edited

**Search / matching (write-time normalized)**

- `artist_name_normalized` (text)
- `album_title_normalized` (text)
- `song_title_normalized` (text)
- `description_normalized` (text) — optional, depending on search approach

Normalization rules (must be deterministic):

- trim
- lower
- collapse internal whitespace to single spaces
- remove/normalize punctuation
- optionally strip diacritics (Postgres `unaccent` extension or app-level normalization)

> Recommendation: start with **app-level normalization** to avoid relying on DB extensions.

#### `request_comment`

- `id` (bigint, PK) (internal)
- `api_key` (uuid, required, unique) — this is the only identifier used in DTOs for comments
- `request_id` (bigint, required FK -> request)
- `parent_comment_id` (bigint, nullable FK -> request_comment) — when set, this comment is a reply
- `body` (text, required) — Markdown
- `is_system` (bool, required, default false)
- `created_at` (timestamp/Instant, required)
- `created_by_user_id` (bigint/int, nullable for system comments)

Constraint:

- If `parent_comment_id` is set, it must reference a comment on the same request.

#### `request_user_state` (read/unread state)

Stores per-user per-request `last_seen_at` and allows O(1) unread checks.

- `request_id` (bigint, required FK -> request)
- `user_id` (bigint/int, required FK -> user)
- `last_seen_at` (timestamp/Instant, required)
- `created_at` (timestamp/Instant, required)
- `updated_at` (timestamp/Instant, required)

PK: `(request_id, user_id)`

#### `request_participant` (performance denormalization)

Tracks “in-scope” requests for navbar unread indicator: creator OR commenter.

- `request_id` (bigint, required FK -> request)
- `user_id` (bigint/int, required FK -> user)
- `is_creator` (bool, required, default false)
- `is_commenter` (bool, required, default false)
- `created_at` (timestamp/Instant, required)

PK: `(request_id, user_id)`

> We can derive commenters from `request_comment`, but navbar unread is a hot path. This table makes `hasUnread` a cheap indexed join.

### Index strategy (critical)

#### `request`

1. Index for index-page default sort:
   - `(created_at DESC, id DESC)`
2. Filter + sort combos:
   - `(status, created_at DESC, id DESC)`
   - `(created_by_user_id, created_at DESC, id DESC)`
   - `(status, created_by_user_id, created_at DESC, id DESC)`
3. Entity filter (ApiKey-based):
   - `(target_artist_api_key, created_at DESC, id DESC)`
   - `(target_album_api_key, created_at DESC, id DESC)`
4. Activity hot path:
   - `(last_activity_at DESC, id DESC)`
   - Partial index (optional): `WHERE status IN (Pending, InProgress)` for open requests

#### `request_comment`

- `(request_id, created_at ASC, id ASC)` to guarantee stable chronological ordering.
- `(request_id, parent_comment_id, created_at ASC, id ASC)` to efficiently load replies for a parent comment.

#### `request_user_state`

- PK `(request_id, user_id)`
- Secondary index for dashboard list:
  - `(user_id, last_seen_at)`

#### `request_participant`

- `(user_id, request_id)`

### Constraints / invariants

- `description` required.
- Delete only when `status = Pending` and `created_by_user_id = currentUserId`.
- Non-admin updates cannot set `status` other than via `/complete`.
- System comments are immutable.

---

## REST API (Non-admin)

Note: This API surface is for **external non-admin clients**. The Blazor UI (including admin) MUST use services directly via DI/IOC and must not call these endpoints.

Controllers should follow existing conventions in `Melodee.Blazor/Controllers/Melodee/*`:

- Base route: `/api/v{version}` using Asp.Versioning.
- Responses for collections: `{ meta, data }` (`PagedResponse<T>` + `PaginationMetadata`).

### DTOs

Define compact DTOs to avoid overfetching:

- `RequestSummaryDto`: list view fields + `createdByUser` summary (include `apiKey`)
- `RequestDetailDto`: includes all editable fields + activity fields (include `apiKey`)
- `RequestCommentDto`: includes `createdByUser` summary + `apiKey` + `parentCommentApiKey`
- `UserSummaryDto`: `id`, `userName`, `displayName?`, `avatarUrl?` (use whatever is already standard)

### Endpoints

#### Requests

- `GET /api/v1/requests?page=&pageSize=&query=&mine=&status=&artistApiKey=&albumApiKey=&songApiKey=`
  - Query implementation notes:
    - `mine=true` filters for `created_by_user_id == currentUserId`.
    - Always apply a deterministic sort: `created_at DESC, id DESC`.
    - Ensure filters are sargable (no functions on indexed columns).
    - For `query`, Phase 1 uses `ILIKE` against a prebuilt concatenated normalized field OR a limited OR over a few columns; Phase 6 can upgrade to full-text.

- `POST /api/v1/requests`
  - Force `status = Pending`.
  - Set `created_by_user_id` from auth context.
  - Insert creator into `request_participant`.
  - Initialize `request_user_state.last_seen_at = created_at` (creator’s own action should not trigger unread).

- `GET /api/v1/requests/{requestApiKey}`
  - Any authenticated user.

- `PUT /api/v1/requests/{requestApiKey}`
  - Creator only.
  - Reject attempts to set admin-only fields/status.
  - Update `last_activity_*` as type `Edited` (but do **not** create unread for the editor).

- `POST /api/v1/requests/{requestApiKey}/complete`
  - Creator only; idempotent.
  - Transition allowed from Pending/InProgress/Rejected → Completed.
  - Update `last_activity_*` as `StatusChanged`.

- `DELETE /api/v1/requests/{requestApiKey}`
  - Creator only.
  - Allowed only while Pending.

#### Comments

- `GET /api/v1/requests/{requestApiKey}/comments?page=&pageSize=`
  - Returns comments with `apiKey` and `parentCommentApiKey` so clients can build a thread.
  - Stable ordering: `created_at ASC, id ASC`.

- `POST /api/v1/requests/{requestApiKey}/comments`
  - Any authenticated user.
  - Create comment (supports replies via optional `parentCommentApiKey`); update request `last_activity_*` to `UserComment`.
  - Validate `parentCommentApiKey` (when set) exists and belongs to the same request.
  - Upsert `request_participant` for commenter.

#### Activity

- `GET /api/v1/requests/activity`
  - Returns `{ hasUnread: bool }` for in-scope requests (creator OR commenter).
  - Query should be a cheap indexed join between `request_participant`, `request`, `request_user_state`.

- `GET /api/v1/requests/activity/unread?page=&pageSize=`
  - Requests where current user is a participant (Creator OR Commenter) AND has unread activity.
  - Sort: `last_activity_at DESC, id DESC`.

- `POST /api/v1/requests/{requestApiKey}/seen`
  - Allowed only if in-scope (creator OR commenter).
  - Upsert `request_user_state.last_seen_at = now`.

### Error semantics

- `401` missing/invalid auth
- `403` authenticated but forbidden
- `404` not found

---

## Blazor UI

### Pages

- `/requests` index page
  - Filters: query text, mine, status, entity filters
  - Paged list newest-first

### Entity detail integration (Artist/Album)

- Artist detail (`/data/artist/{artistApiKey}`):
  - Remove the "View Requests" button.
  - Add a **Requests** `RadzenTreeItem` immediately after **Relationships**.
  - Selecting this tree item shows request cards filtered by `artistApiKey`.
  - Clicking a request card navigates to `/requests/{requestApiKey}`.

- Album detail (`/data/album/{albumApiKey}`):
  - Remove the "View Requests" button.
  - Add a **Requests** `RadzenTreeItem` immediately after **Images**.
  - Selecting this tree item shows request cards filtered by `albumApiKey`.
  - Clicking a request card navigates to `/requests/{requestApiKey}`.

- `/requests/new` creation page
  - Supports prefill query params:
    - `category`, `targetArtistApiKey`, `artistName`, `targetAlbumApiKey`, `albumTitle`, `songTitle`, `releaseYear`, `fromUrl`

- `/requests/{requestApiKey}` detail page
  - Shows metadata + actions + comments
  - Comments are displayed in a **threaded** manner and support a **Reply** action.
  - On open, mark as seen by calling `RequestActivityService` directly (and/or upsert state server-side) — do not call `/api/...`

- `/requests/{requestApiKey}/edit` edit page

### Navbar

- Add “Requests” nav item.
- Add unread dot by calling `RequestActivityService` directly (no HTTP calls to `/api/...`).
  - Poll frequency should be conservative (e.g., once per minute) and/or only when authenticated and connected.

### Dashboard “Request Activity” section

- Only render if `RequestActivityService` returns at least one unread item (no HTTP calls to `/api/...`).
- Show up to N (e.g., 10), sorted by most recent activity.

---

## Auto-completion / strict matching

### Trigger points

Hook into existing message bus/event handlers used for library changes:

- On album added
- On song added

### Matching algorithm (must be unambiguous)

- Candidate set: open requests only (`status IN (Pending, InProgress)`)
- For AddAlbum:
  - **Priority 1**: Match `target_artist_api_key` (if present in request) == new album's artist API key.
  - **Priority 2**: Match `artist_name_normalized` AND `album_title_normalized`.
  - If request `release_year` is present, require exact year match (for both priorities).

- For AddSong:
  - **Priority 1**: Match `target_artist_api_key` (if present) == new song's artist API key.
  - **Priority 2**: Match `artist_name_normalized` AND `song_title_normalized`.
  - If request `release_year` is present, require exact year match.

### Completion effects

- Set `status = Completed`
- Update `last_activity_*` as `StatusChanged` with `last_activity_user_id = null` (system)
- Insert a **system comment** containing a link to the matched entity using Blazor routes:
  - `/data/artist/{artistApiKey}`
  - `/data/album/{albumApiKey}`
  - `/data/song/{songApiKey}`

> The system comment creation should be part of the same transaction as marking Completed.

---

## Phase Details

### Phase 0 — Data model + migrations

- Add EF Core models and migrations for:
  - `Request`
  - `RequestComment`
  - `RequestUserState`
  - `RequestParticipant`
- Add indexes listed above (starting with the minimal set for Phase 1/4 hot paths).

### Phase 1 — Requests API

- Implement Requests controller and services.
- Ensure all filtering is server-side and indexed.

### Phase 2 — Comments API

- Implement comments endpoints and ensure request `last_activity_*` updates.

### Phase 3 — Requests UI

- Implement index/detail/create/edit pages.
- Add navbar item and basic navigation/prefill.

### Phase 4 — Activity tracking

- Implement `request_user_state` upsert logic:
  - on detail open
  - on status changes by current user
- Implement activity endpoints + navbar dot + dashboard widget.

### Phase 5 — Auto-completion

- Implement event handlers and strict match query.
- Add system comment emission.

### Phase 6 — Performance hardening + docs

- Add high coverage **unit tests** (happy path + edge cases), focusing on the domain/services layer:
  - Request/Comment/Activity services (including reply/thread behavior)
  - Permission checks and status transition rules
  - Auto-completion strict matching + normalization
- Confirm query plans for:
  - `/requests` with status + mine
  - `/requests/activity`
  - `/requests/activity/unread`
- If `query` search is slow, upgrade to Postgres full-text search:
  - add `search_document` tsvector column and a GIN index
  - maintain it on write (EF hook/service)
- Update `README.md` and `docs/` per requirements.
