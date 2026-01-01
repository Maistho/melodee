---
post_title: "Jellyfin API Emulation Spec (Melodee.Blazor)"
author1: "GitHub Copilot (GPT-5.2)"
post_slug: "jellyfin-spec"
microsoft_alias: "copilot"
featured_image: ""
categories:

  - "prompts"
tags:

  - "melodee"
  - "jellyfin"
  - "api"
  - "emulation"
ai_note: "AI-assisted draft; intended for engineering implementation."
summary: "Phased specification for adding a Jellyfin-compatible API surface to Melodee.Blazor, including header-based routing, authentication, streaming, testing, and security/performance considerations."
post_date: "2025-12-31"
---

## Overview

Melodee currently exposes two API surfaces:

1. Melodee REST API (JWT, versioned routes under `/api/v{version}`)
2. OpenSubsonic/Subsonic emulation (routes under `/rest/*`)

This document specifies a third API surface: **Jellyfin API emulation**.

The primary goal is to allow Jellyfin clients (Desktop, JellyAmp, Tauon, jellycli)
to browse and stream music from Melodee **without client modification**.

Reference OpenAPI spec: `prompts/jellyfin-openapi-stable.json` (Jellyfin 10.11.5).

## Goals

- Provide a Jellyfin-compatible subset focused on **music**:

  - server discovery (`/System/*`)
  - authentication (`/Users/AuthenticateByName`)
  - browsing/library queries (`/UserViews`, `/Items`, `/Artists`, playlists)
  - streaming (`/Audio/{itemId}/stream`, `/Items/{itemId}/File|Download`)
- Use **header-based routing** to route Jellyfin clients into a dedicated namespace

  `/api/jf` in the server implementation.
- Keep all Jellyfin code isolated in a new controller namespace:

  `Melodee.Blazor.Controllers.Jellyfin` (within the `Melodee.Blazor` project).
- Include clear security and performance requirements to avoid unsafe shortcuts.

## Phase map (completion checklist)

Use this as a quick, high-level progress tracker.

- [ ] Phase 0: Plumbing and scaffolding
- [ ] Phase 1: Server discovery and login
- [ ] Phase 2: User views and music browsing primitives
- [ ] Phase 3: Item details, images, and playlists
- [ ] Phase 4: Audio streaming
- [ ] Phase 5: Playback reporting (optional)
- [ ] Phase 6: Token management, hardening, and admin (optional)

## Critical endpoints summary

This table is the minimal contract to get real Jellyfin music clients working.

| Endpoint | Method | Phase | Auth | Expected behavior |
| --- | --- | --- | --- | --- |
| `/System/Info/Public` | GET | 1 | No | Returns server identity + capabilities needed for client onboarding |
| `/System/Ping` | GET/POST | 1 | No | Returns `204` (preferred) or small `200` per schema |
| `/System/Info` | GET | 1 | Yes (recommended) | Returns authenticated server info; can be minimal |
| `/Users/AuthenticateByName` | POST | 1 | No | Validates Melodee credentials; returns Jellyfin `AuthenticationResult` |
| `/UserViews` | GET | 2 | Yes | Returns “Music” view(s) mapped to Melodee roots |
| `/Artists` | GET | 2 | Yes | Returns paged artist list and search support |
| `/Items` | GET | 2 | Yes | Returns paged item list (artists/albums/tracks) with safe param handling |
| `/Audio/{itemId}/stream` | GET/HEAD | 4 | Yes | Serves audio with range support; no full buffering |
| `/Items/{itemId}/File` | GET | 4 | Yes | Direct file response (range aware if supported) |
| `/Items/{itemId}/Download` | GET | 4 | Yes | Same content as File but with download-friendly headers |

## Non-goals

- Implementing the full Jellyfin API surface (video, live TV, DLNA, transcoding).
- Implementing Jellyfin plugins, websocket APIs, or remote discovery protocols.
- Requiring new client-side modifications.

## Routing and Namespace

### External paths vs internal controller paths

Jellyfin clients expect endpoints at **root paths** like `/System/Info/Public`,
`/Users/AuthenticateByName`, `/Audio/{id}/stream`, `/Items`, etc.

To keep Melodee’s routing clean and prevent collisions, internal controller routes
will live under:

- `/api/jf/*`

A **routing middleware** will map Jellyfin client requests to this internal prefix.

### Architecture (high-level)

```text
Jellyfin Client
  |  (Authorization: MediaBrowser ... / X-Emby-Authorization)
  v
ASP.NET Core pipeline
  -> JellyfinRoutingMiddleware (detect + rewrite to /api/jf/*)
  -> Authentication/Authorization/RateLimiter
  -> Jellyfin Controllers (/api/jf/*)
  -> Melodee domain/services (library + streaming)
```

### Jellyfin request detection (header-based routing)

Primary detection:

- `Authorization` header uses schema `MediaBrowser`, e.g.

```http
Authorization: MediaBrowser Token="8ac3a7abaff943ba9adea7f8754da7f8"
```

Secondary detection (needed for real-world Jellyfin clients and pre-auth flows):

- `X-Emby-Authorization` header starts with `MediaBrowser` (common in Jellyfin)
- `X-MediaBrowser-Token` / `X-Emby-Token` present
- Path-based allowlist for pre-auth endpoints that may not have auth headers:

  - `/System/Info/Public`
  - `/System/Ping`
  - `/Users/AuthenticateByName`

### Middleware behavior

Implement a middleware (example name: `JellyfinRoutingMiddleware`) that runs
early enough that the rewritten path is used by endpoint routing.

Placement requirement:

- Path rewriting must happen before endpoint routing selects an endpoint.
- Place `app.UseMiddleware<JellyfinRoutingMiddleware>()` early in `Program.cs`,
  before `app.UseAuthentication()`.

Recommended placement:

- After response compression and Swagger (if enabled), and before any auth.

Rules:

- If request path already starts with `/api/jf`, do nothing.
- If request path starts with `/api/` or `/rest/` or `/song/`, do nothing

  (avoid interfering with existing Melodee/OpenSubsonic behavior).
- If request matches Jellyfin detection (headers or allowlisted paths), rewrite:

  - `context.Request.Path = "/api/jf" + context.Request.Path`

- Preserve query string and method.

Acceptance criteria:

- Jellyfin clients can use base URL `https://host/` (no extra path) and still

  reach Jellyfin endpoints.
- Operators can also optionally use base URL `https://host/api/jf`.

## Controller Namespace and Project Layout

Create a new folder under `src/Melodee.Blazor/Controllers/`:

- `src/Melodee.Blazor/Controllers/Jellyfin/`

Controllers must use namespace:

- `Melodee.Blazor.Controllers.Jellyfin`

Add a base controller patterned after existing `CommonBase` / OpenSubsonic
controllers, but tuned to Jellyfin semantics:

- header parsing (MediaBrowser token)
- common error shaping
- request correlation and logging

## Authentication and Token Model

Jellyfin OpenAPI describes an `apiKey` security scheme in the `Authorization`
header.

### Supported inbound token formats

Requests must accept tokens from (in priority order):

1. `Authorization: MediaBrowser Token="..."`
2. `X-MediaBrowser-Token: ...`
3. `X-Emby-Token: ...`
4. `X-Emby-Authorization: MediaBrowser ... Token="..." ...`

### Token storage and lifecycle

To meet security requirements, implement an explicit Jellyfin token concept
(do not treat the token as an unhashed long-lived password).

Recommended model (new table/entity):

- `JellyfinAccessToken`

  - `Id` (GUID)
  - `UserId` (GUID)
  - `TokenHash` (store only hash)
  - `TokenSalt` (store per-token unique salt)
  - `CreatedAt`, `LastUsedAt`
  - `ExpiresAt` (nullable for non-expiring, but prefer expiry)
  - `RevokedAt` (nullable)
  - `Client`, `Device`, `DeviceId`, `Version` (from headers when available)

Hashing requirements:

- Token generation:

  - tokens must be cryptographically random (e.g., 256-bit) and treated as
    bearer secrets.

- Hashing algorithm:

  - prefer HMAC for clearer cryptographic intent:

    - compute `TokenHash = HMAC-SHA256(PepperKey, TokenSalt || AccessToken)`.

      - `PepperKey` is a server secret (config/env), not stored in the DB.
      - `TokenSalt` is stored per token to ensure uniqueness even if a token is
        reused accidentally.

  - if HMAC is not used, an acceptable fallback is:

    - `SHA-256(TokenSalt || Pepper || AccessToken)`.
  - `TokenSalt` must be unique per token (recommended 16+ bytes from a CSPRNG).

- Comparison:

  - use constant-time comparison when checking hashes.

Optional hardening (recommended if easy):

- Ensure a server-side “pepper” secret exists (config/env) so DB compromise does
  not immediately reveal token-verification capability.

### Issuance endpoint

Implement:

- `POST /Users/AuthenticateByName`

Behavior:

- Validate username/password against Melodee’s user system.
- On success, create a new Jellyfin access token.

Concurrency requirement:

- Token issuance and enforcement of `MaxActiveTokensPerUser` must be
  concurrency-safe.

  - Use a transactional approach (or equivalent) so concurrent logins cannot
    exceed the cap.
- Return a Jellyfin `AuthenticationResult` shape (per OpenAPI schema):

  - include `AccessToken`
  - include `User` object (minimal fields needed by clients)
  - include `SessionInfo` if required by clients (can be minimal/stub initially)

### Revocation / rotation

Implement support for:

- token rotation: issuing multiple tokens per user/device
- revocation: immediate invalidation of a token without changing the user password

Minimum required revocation mechanism:

- server-side check `RevokedAt == null` and `ExpiresAt` not elapsed
- admin or user-driven revoke endpoint (phase later; see Phases)

## Response conventions

- Default to `application/json` responses.
- Support optional Jellyfin “profile” content types if required by clients:

  - `application/json; profile="CamelCase"`
  - `application/json; profile="PascalCase"`

If not fully implemented initially:

- always return `application/json` but accept requests that ask for profile types.

Error responses:

- Prefer Jellyfin-style `ProblemDetails` where OpenAPI indicates.
- For unsupported endpoints, respond with `404` (preferred) or `410` if endpoint is

  explicitly deprecated in OpenAPI and known not to be supported.

### Error handling and mapping (consistency)

Goal: map internal Melodee failures into Jellyfin-friendly responses without
leaking sensitive details, while keeping behavior consistent across all
Jellyfin endpoints.

Guidance:

- Use a single error shaping mechanism (preferred: a centralized exception
  handler for the Jellyfin route group or a shared base controller helper).
- Return `application/json` and a Jellyfin-compatible `ProblemDetails` shape.
- Map errors consistently (suggested baseline mapping):

  - `400 BadRequest`: validation failures (missing required fields, invalid GUID,
    invalid `Range` syntax, etc.)
  - `401 Unauthorized`: missing/invalid token, invalid username/password
  - `403 Forbidden`: authenticated but not permitted (library restrictions,
    disabled account, admin-only endpoint)
  - `404 NotFound`: unknown item id / playlist id / image tag
  - `409 Conflict`: state conflict (duplicate playlist name where prohibited,
    optimistic concurrency mismatch if implemented)
  - `416 Range Not Satisfiable`: invalid byte ranges
  - `429 Too Many Requests`: rate-limited (ensure retry headers as appropriate)
  - `500 Internal Server Error`: unexpected exceptions only

- Include stable, non-sensitive fields:

  - `status`, `title`, `detail` (sanitized)
  - `traceId` (from `HttpContext.TraceIdentifier`) for supportability
  - optionally `errorCode` (Melodee-defined string enum) for client debugging

Examples (recommended response bodies):

- Invalid or missing token (`401`):

```json
{
  "type": "about:blank",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Missing or invalid authentication token.",
  "traceId": "00-..."
}
```

- Rate limited (`429`):

Headers:

```text
Retry-After: 60
```

Body:

```json
{
  "type": "about:blank",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded. Please retry later.",
  "traceId": "00-..."
}
```

Non-goal: attempting to perfectly mirror Jellyfin server error bodies. The
priority is client compatibility and debuggability.

## Phased Implementation Plan

Each phase is meant to be independently shippable. Do not include timeframes.

### Phase 0: Plumbing and scaffolding

Deliverables:

- Add Swagger document for Jellyfin controllers:

  - `SwaggerDoc("jellyfin", ...)`
  - Inclusion predicate by namespace contains `.Controllers.Jellyfin`
- Add Jellyfin routing middleware and unit tests.
- Add rate limiting policy placeholder for Jellyfin:

  - `jellyfin-api`
  - `jellyfin-stream` (optional separate policy)
- Add basic controller base + token parser utilities.
- Add EF Core migration for the new `JellyfinAccessToken` persistence model.

Auth flow verification note:

- If Jellyfin auth is implemented such that `HttpContext.User` is set before
  `app.UseAuthentication()` runs, verify the standard authentication middleware
  does not overwrite the user principal when it fails to authenticate.

  - Typical behavior is safe (auth middleware only sets the principal on
    success), but this should be verified with tests.

Unit tests:

- routing middleware rewrites only expected paths
- routing middleware does not rewrite `/api/*`, `/rest/*`, `/song/*`
- token parser accepts all supported token formats

### Phase 1: Server discovery and login

Endpoints:

- `GET /System/Info/Public`
- `GET /System/Ping`
- `POST /System/Ping` (some clients use POST)
- `GET /System/Info` (may require auth)
- `POST /Users/AuthenticateByName`

Behavior:

- `/System/Info/Public` returns enough fields for clients to display server name

  and decide it is a Jellyfin-compatible server.

CORS decision note:

- Confirm whether any Jellyfin web-based clients (or browser tooling) will call
  these endpoints cross-origin.

  - If yes, define a narrowly scoped CORS policy for the Jellyfin surface.
  - If no, explicitly keep CORS disabled for these endpoints.
- `/System/Ping` returns `204` or a small `200` response per schema.
- `/Users/AuthenticateByName` issues token and returns `AuthenticationResult`.

Unit tests:

- happy path auth returns token + user payload
- wrong password returns `401` (or appropriate Jellyfin error)
- locked/blacklisted user returns `403`
- repeated auth creates new token (rotation)

Edge cases:

- missing username/password
- username casing rules
- brute-force attempts (verify rate limiting)

### Phase 2: User views and music browsing primitives

Endpoints:

- `GET /UserViews`
- `GET /Artists`
- `GET /Items` (filtered for music)

Implementation notes:

- Map Jellyfin “views” to Melodee library roots.

  - At minimum, expose a single “Music” view.

#### Data mapping notes (artists, albums, items)

The Jellyfin API surface is entity-centric (Items with types and IDs). Melodee
has distinct domain entities. The implementation should define and test stable
ID mappings and type projections.

Proposed baseline mapping (minimal subset for music clients):

- Jellyfin `UserView`:

  - `Id` maps to a Melodee library root identifier (or a synthetic ID if Melodee
    does not expose roots). Keep stable across restarts.
  - `Name`: “Music” or per-root name.

- Jellyfin `Artist` items:

  - `Id` maps to Melodee `Artist.Id`.
  - `Name` maps to Melodee artist name.
  - `ImageTags`: derive from artist image last-modified (or a content hash).

- Jellyfin `MusicAlbum` items:

  - `Id` maps to Melodee `Album.Id`.
  - `Name` maps to album title.
  - `AlbumArtist` / `AlbumArtists`: map from Melodee album artist relationships.
  - `ProductionYear`: map if available; otherwise omit.
  - `ImageTags`: derive from album art last-modified.

- Jellyfin `Audio` items (tracks):

  - `Id` maps to Melodee `Song.Id` (or track entity ID).
  - `Name` maps to track title.
  - `AlbumId` maps to Melodee album ID.
  - `ArtistItems` maps to Melodee track artists.
  - `RunTimeTicks`: duration in ticks.
  - `MediaSources`: include container/codec/bitrate metadata when available.

Rules:

- IDs exposed via Jellyfin must be opaque to clients but stable.
- Avoid exposing filesystem paths.
- If a client requests unsupported fields via `fields=...`, ignore unknown
  fields safely (do not error).
- For `/Items` and `/Artists`, support common query params used by clients:

  - `searchTerm`, `startIndex`, `limit`, `parentId`, `includeItemTypes`,
    `fields`, `enableUserData`
- Any unsupported query params must be safely ignored, not cause 500s.

Performance:

- Cache `/UserViews` and `/Artists` per user (short TTL) because clients call

  repeatedly on navigation.

Unit tests:

- paging behavior (`startIndex`, `limit`) and bounds
- searchTerm empty vs non-empty
- unknown query params do not change behavior and do not error

### Phase 3: Item details, images, and playlists (browse completeness)

Endpoints (prioritize by observed client traffic):

- `GET /Items/{id}` (if required by clients)
- `GET /Items/{id}/Images/*` (or equivalent image endpoints per OpenAPI)
- `GET /Playlists`
- `POST /Playlists` (optional; non-MVP)
- `GET /Playlists/{id}` and playlist item listing

Notes:

- Images: serve album art / artist art with correct caching headers.
- If Jellyfin requires image tags, implement stable tags based on last-modified.

Image sizing parameters (common client behavior):

- Many clients call image endpoints with width/height parameters.

  - Support `maxWidth`/`maxHeight` (or equivalent per OpenAPI) if practical.
  - If not supported in MVP, accept and ignore these params (do not error).

Playlist write scope clarification:

- MVP expectation: playlist read/browse only.
- Playlist write operations are explicitly deferred (Phase 3 optional) and are
  not required for initial Jellyfin client playback compatibility.

Unit tests:

- image route returns correct content-type and ETag
- playlist create/update validates input lengths and ownership

### Phase 4: Audio streaming

Endpoints:

- `GET /Audio/{itemId}/stream`
- `HEAD /Audio/{itemId}/stream`
- `GET /Items/{itemId}/File`
- `GET /Items/{itemId}/Download`

Streaming requirements:

- Must support range requests:

  - `Accept-Ranges: bytes`
  - handle `Range: bytes=start-end` and return `206` with `Content-Range`
- Must not buffer entire file into memory.
- Must enforce authorization/ownership rules.

Query parameters handling for `/Audio/{itemId}/stream`:

- Implement (at minimum): `container`, `static`, `startTimeTicks`, `audioBitRate`
- Accept and ignore many other parameters without failure.
- If transcoding is not supported initially:

  - require `static=true` OR respond with a clear error for non-static requests

Performance and safety:

- Add per-user concurrency limits for streaming (reuse/extend `StreamingLimiter`).
- Ensure file handles are disposed promptly.

Resource management requirements:

- Cancellation:

  - All streaming endpoints must honor `HttpContext.RequestAborted`.
  - If using manual stream copy, pass the cancellation token to copy methods.

- Cleanup:

  - Ensure streams are disposed even on partial reads and client disconnects.
  - Avoid holding DB contexts open for the duration of a stream.
  - If temporary files are ever introduced (future transcoding), ensure deletion
    on success, failure, and cancellation.

- Backpressure:

  - Prefer framework streaming primitives (`FileStreamResult`/`PhysicalFileResult`)
    over buffering.
  - Ensure range requests do not result in per-request full file reads.

- Failure behavior:

  - If a stream fails after headers have been sent (disk error, file deleted),
    the response cannot be reshaped into JSON. Prefer:

    - abort the connection
    - log a structured error with `traceId`, userId, itemId

  - If a stream fails before headers are sent, return a safe error response
    (typically `404` if item no longer exists, otherwise `503`).

Unit tests:

- `HEAD` returns headers without body
- full download returns `200` with correct content type
- range request returns `206` and correct `Content-Range`
- invalid range returns `416`

### Phase 5: Playback reporting (optional for client UX)

Many clients report playback state for resume, recently played, and scrobbling.

Endpoints (implement as needed):

- playback/session reporting endpoints (from OpenAPI: `Sessions`, `Playstate`,

  “NowPlaying” style calls)

Behavior:

- Map to existing Melodee “now playing” / scrobble infrastructure where possible.

Unit tests:

- reports are accepted and do not crash for unknown payload fields
- rate-limited to prevent spam

### Phase 6: Token management, hardening, and admin capabilities

Endpoints (OpenAPI includes API key endpoints):

- `GET /Auth/Keys` (admin-only)
- token deletion/revocation endpoints (where present in OpenAPI)

Security:

- add audit logs for token issuance/revocation
- add server-configurable expiry and max tokens per user/device

## Configuration

### Appsettings keys (proposed)

Add configuration under a new section `Jellyfin:` in `appsettings.json`:

- `Jellyfin:Enabled` (bool)
- `Jellyfin:RoutePrefix` (default `/api/jf`)
- `Jellyfin:Token:`

  - `ExpiresAfterHours`
  - `MaxActiveTokensPerUser`
  - `AllowLegacyHeaderTokens` (bool)
- `Jellyfin:RateLimiting:`

  - `ApiRequestsPerPeriod`
  - `ApiPeriodSeconds`
  - `StreamConcurrentPerUser`

### Defaults and validation rules (recommended)

These defaults are intended to be safe and practical for typical home/server
usage while preventing obvious abuse. They should be validated at startup.

- `Jellyfin:Enabled`

  - default: `false`

- `Jellyfin:RoutePrefix`

  - default: `/api/jf`
  - validation: must start with `/` and must not be `/api` or `/rest`

- `Jellyfin:Token:ExpiresAfterHours`

  - default: `168` (7 days)
  - validation: min `1`, max `8760` (1 year)

- `Jellyfin:Token:MaxActiveTokensPerUser`

  - default: `10`
  - validation: min `1`, max `50`

- `Jellyfin:Token:AllowLegacyHeaderTokens`

  - default: `true` (compatibility); allow disabling for stricter deployments

- `Jellyfin:RateLimiting:ApiRequestsPerPeriod` and `Jellyfin:RateLimiting:ApiPeriodSeconds`

  - default: `200` requests per `60` seconds per user (browse + metadata)
  - validation: min `10` per period; max should be capped (e.g., `5000`) to
    avoid accidental misconfiguration

- `Jellyfin:RateLimiting:StreamConcurrentPerUser`

  - default: `2`
  - validation: min `1`, max `10`

Concrete example:

- “200 requests per minute per user” means:

  - `ApiRequestsPerPeriod=200`
  - `ApiPeriodSeconds=60`

### SettingRegistry keys (proposed)

If Melodee prefers database-backed configuration, add keys:

- `SettingRegistry.JellyfinEnabled`
- `SettingRegistry.JellyfinTokenExpiryHours`
- `SettingRegistry.JellyfinMaxTokensPerUser`
- `SettingRegistry.JellyfinRoutePrefix`

All new settings must be added to:

- `MelodeeConfiguration.AllSettings(...)`
- test configuration in `tests/Melodee.Tests.Common/TestsBase.cs`

## Migration and coexistence strategy

Goal: support Jellyfin clients without breaking existing Melodee REST API and
OpenSubsonic users.

Guidance:

- Coexistence:

  - Keep `/api/v{version}` (Melodee) and `/rest/*` (OpenSubsonic) behavior
    unchanged.
  - Jellyfin emulation must only activate when detected via headers or
    allowlisted paths.

- OpenSubsonic users:

  - No required migration; Jellyfin emulation is additive.
  - Users can run both client types against the same Melodee instance.

- Migration path (optional, operator-guided):

  - Authentication:

    - users authenticate with the same Melodee username/password via
      `/Users/AuthenticateByName`.
    - tokens are newly issued Jellyfin tokens; do not reuse OpenSubsonic tokens.

  - Token/config migration:

    - default: no automated token migration (different formats and security
      properties).
    - optional future utility: an admin-only tool to bulk revoke Jellyfin tokens
      or export token issuance audit logs.

- Potential conflicts to avoid:

  - Do not rewrite requests that already target `/api/*` or `/rest/*`.
  - Ensure cookie middleware continues to bypass API routes and does not inject
    UI cookies into Jellyfin responses.
  - Rate limiting partitions and limits should not be shared across emulations
    unless intentionally configured (avoid one client type starving another).

## Jellyfin versioning strategy

Reference schema: Jellyfin 10.11.5 OpenAPI.

Requirements:

- Assume clients may vary and send different query params/headers.
- Prefer “tolerant reader” behavior:

  - ignore unknown query parameters
  - ignore unknown JSON fields in request bodies
  - keep response contracts stable, but omit fields that cannot be computed

- Track compatibility changes explicitly:

  - keep a small allowlist of known client variants (Desktop, JellyAmp, etc.)
  - add contract tests for endpoints known to differ by client version
  - avoid branching behavior on client version unless necessary and tested

## Security Considerations

### Token rotation and revocation

- Do not store tokens in plaintext.
- Support multiple tokens per user (per device) and per-token revocation.
- Support expiry; ensure stale tokens cannot be used indefinitely.

### Edge cases to handle explicitly

- Concurrent token issuance (same user/device):

  - enforce `MaxActiveTokensPerUser` deterministically (transactional check or
    post-insert pruning with a consistent ordering)
  - ensure no race condition allows unbounded token growth

- Expired/revoked token during an active stream:

  - minimum requirement: token must be valid at stream start
  - recommended default: do not revalidate mid-stream (can break playback)

Mid-stream revalidation policy (configurable):

- Add a configuration option for how to handle long-lived streams:

  - `Jellyfin:Token:RevalidateDuringStreamPolicy` (string enum)

    - `Never` (default): validate token only at stream start
    - `StartOnly`: synonym for `Never` (optional)
    - `Periodic`: revalidate at safe boundaries (implementation-defined; avoid
      per-chunk DB lookups)

- Regardless of policy, do not terminate an in-flight response abruptly without
  clear client compatibility testing.

- User deleted/disabled with active tokens:

  - treat as `403` on new requests
  - revoke or hard-invalidate all tokens for that user (prefer cascade revoke)

### Rate limiting

- Apply rate limiting to:

  - auth endpoints (`/Users/AuthenticateByName`) to mitigate brute force
  - browse endpoints to mitigate scraping
  - streaming endpoints to mitigate bandwidth abuse

Partition strategy (recommended):

- primary: by `UserId` (after auth)
- fallback: by remote IP (pre-auth)

### Input validation

- Validate all UUID path parameters.
- Validate query params with patterns (e.g., `container` matches OpenAPI regex).
- Enforce maximum lengths on strings (searchTerm, deviceId, etc.).
- For any filesystem access, prevent path traversal by using IDs only and

  resolving via database.

## Performance Considerations

### Caching strategies

- Cache high-churn browse calls with short TTL:

  - `/UserViews`, `/Artists`, common `/Items` queries
- Use existing cache abstractions (`ICacheManager`) to keep patterns consistent.
- Use ETags for images and for browse responses when stable.

Cache keys and invalidation:

- Cache keys for browse endpoints must include:

  - `UserId` (permissions differ)
  - the effective view/root id (if applicable)
  - paging/sort/search parameters

- Invalidation strategy:

  - TTL-only is acceptable for MVP.
  - If the codebase provides a library-change event signal (scan complete,
    metadata update), use it to evict relevant browse caches.

Conditional request handling (browse endpoints):

- For `/UserViews`, `/Artists`, `/Items` responses where the payload can be
  represented by a stable cache key:

  - emit `ETag` and `Last-Modified` (if a meaningful last-modified is known)
  - honor `If-None-Match` and `If-Modified-Since`
  - return `304 Not Modified` when appropriate

- This is especially valuable for clients that poll browse endpoints frequently.

### Database query optimization

- Use `AsNoTracking()` for read-heavy browse queries.
- Avoid N+1 queries by projecting to DTOs and using includes selectively.
- Ensure pagination is pushed to the database (`Skip/Take`).

### Memory management for streaming

- Stream via `FileStreamResult` / `PhysicalFileResult` with range support.
- Do not load full audio into memory.
- Apply per-user concurrency limits and cancellation token handling.

## Testing Strategy

### Unit tests

Tooling (align with repo conventions):

- Test framework: xUnit
- Mocking: Moq

Mocking strategy:

- Domain/services:

  - mock database abstractions and external services (filesystem, metadata)
    using Moq.

- Streaming:

  - avoid real filesystem dependency in most unit tests by using in-memory
    streams or temp files created within the test scope.

- Middleware:

  - use `DefaultHttpContext` and set headers/paths explicitly.

Where:

- Service-level mapping tests: `tests/Melodee.Tests.Common/Services/`
- Controller/middleware tests: `tests/Melodee.Tests.Blazor/`

Coverage requirements:

- Happy path:

  - successful auth
  - browse returns expected DTO shapes and paging
  - audio stream returns correct headers
- Edge cases:

  - missing/invalid token formats
  - revoked/expired tokens
  - invalid UUIDs
  - invalid range headers
  - very large `limit` values
  - unknown query params

### Performance targets (baseline expectations)

These are targets for engineering and regression detection (not guarantees).

- Browse endpoints (`/UserViews`, `/Artists`, `/Items`):

  - p50 < 200ms, p95 < 1000ms on a typical library size

- Stream start latency (`/Audio/{id}/stream` first byte):

  - p50 < 300ms, p95 < 1500ms (assuming local storage and warm cache)

How to validate:

- Use existing benchmark harness under `benchmarks/` when applicable.
- Add targeted performance regression tests under
  `tests/Melodee.Tests.Common/Performance/` for query shapes and memory usage.

### Integration testing (manual)

Clients:

- Jellyfin Desktop Client: https://github.com/jellyfin/jellyfin-desktop
- JellyAmp: https://github.com/jellyfin-labs/jellyamp
- Tauon: https://github.com/Taiko2k/Tauon
- jellycli: https://github.com/tryffel/jellycli

Manual test checklist:

1. Add server URL and authenticate
2. Browse music library views
3. Search for an artist/album
4. Play a track end-to-end
5. Seek within a track (range requests)
6. Verify that Melodee API (`/api/v1`) and OpenSubsonic (`/rest`) still work

### Coexistence test requirements

These requirements ensure Jellyfin emulation remains additive and does not
regress existing API surfaces.

#### Mandatory regression tests

- All existing tests in `tests/Melodee.Tests.Blazor/` must pass.
- All OpenSubsonic controller tests must pass unchanged.
- All Melodee API controller tests must pass unchanged.

#### Integration verification

Before merging any Jellyfin phase:

1. Authenticate via Melodee JWT API (`/api/v1/auth`).
2. Authenticate via OpenSubsonic (`/rest/ping` with credentials).
3. Authenticate via Jellyfin (`/Users/AuthenticateByName`).
4. Verify all three work for the same user account simultaneously.

### Client compatibility checklist (quirks to watch)

| Client | Headers/auth behavior to verify | Playback behavior to verify |
| --- | --- | --- |
| Jellyfin Desktop | Sends `X-Emby-Authorization` and/or `Authorization: MediaBrowser ...` | Often performs `HEAD` before `GET`; requires range/seek to work |
| JellyAmp | May send device/client fields in auth headers | Rapid browse calls; ensure caching + rate limiting are tuned |
| Tauon | Tends to be strict about content types and metadata fields | Seeks frequently; validate `206` + `Content-Range` correctness |
| jellycli | Simple flows; good for smoke tests | Streams via direct GET; validate auth + 200/206 behavior |

## User management and permissions

Goal: integrate Jellyfin auth and library access with Melodee’s existing user
and permission model.

Guidance:

- Authentication:

  - `/Users/AuthenticateByName` must validate against Melodee users.
  - Jellyfin tokens are separate from JWT and should not grant broader access
    than the authenticated Melodee user.

- Authorization:

  - Enforce library visibility constraints consistently across browse and
    streaming endpoints.
  - Ensure item IDs are checked for ownership/visibility before streaming.

- Future capability (optional):

  - allow admins to view/revoke issued Jellyfin tokens by user and device
  - emit audit logs on token issuance and revocation

## Observability

- Log Jellyfin requests with:

  - correlation id (`HttpContext.TraceIdentifier`)
  - user id (if available)
  - client/device headers (sanitized)

- Add structured logs for:

  - token issuance, revocation, auth failures
  - streaming start/stop and cancellation
  - rate-limit rejections (partition key, endpoint group)

Example log templates (structured logging):

- Token issued:

  - `JellyfinTokenIssued UserId={UserId} TokenId={TokenId} Client={Client} DeviceId={DeviceId}`

- Auth failed:

  - `JellyfinAuthFailed UserName={UserName} RemoteIp={RemoteIp} Reason={Reason}`

- Stream started:

  - `JellyfinStreamStart UserId={UserId} ItemId={ItemId} Range={Range} Static={Static}`

- Stream canceled:

  - `JellyfinStreamCanceled UserId={UserId} ItemId={ItemId} BytesSent={BytesSent} Reason={Reason}`

### Monitoring and metrics (Jellyfin-specific)

Add metrics that are specific to the Jellyfin surface, so operators can
distinguish Jellyfin client load from other APIs.

Suggested metrics:

- request rate and latency by endpoint group (`System`, `Users`, `Items`,
  `Artists`, `Audio`)
- auth failures and rate-limit rejections
- active streams (gauge) and stream start/stop counts
- bytes served (counter) and range request ratio
- cache hit/miss for browse endpoints

Minimum logging for streaming:

- itemId, userId, response status, bytes served (if available)
- cancellation reason (client disconnect vs server cancellation)

## Troubleshooting (client compatibility)

Common issues and what to check:

- Client cannot add server:

  - confirm `/System/Info/Public` is reachable at the base URL (no prefix)
  - verify middleware rewrites correctly when no auth header is present

- Auth succeeds but browsing fails:

  - verify token parsing for `X-Emby-Authorization` and `Authorization` schema
  - check rate limiting for browse endpoints

- Playback starts but seeking fails:

  - confirm `Accept-Ranges: bytes` and `206` responses for valid ranges
  - validate `Content-Range` formatting and `416` behavior for invalid ranges

- High CPU/memory during playback:

  - confirm no full buffering
  - confirm DB context is not held for stream lifetime
  - confirm concurrency limits are applied

## Glossary

- Jellyfin token:

  - A bearer token issued by `/Users/AuthenticateByName` and presented by
    clients via `Authorization: MediaBrowser ...` or related headers.

- MediaBrowser:

  - The authorization scheme string used by Jellyfin/Emby-style clients in the
    `Authorization` header.

- UserView:

  - A top-level “library view” a client navigates into (e.g., “Music”).

- Item:

  - A Jellyfin entity (artist, album, track, playlist, etc.) identified by an
    ID and a type.

- Route prefix:

  - The internal namespace for Jellyfin controllers (default `/api/jf`), reached
    via middleware rewrite from external root paths.