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

  `Melodee.Controllers.Jellyfin` (within the `Melodee.Blazor` project).
- Include clear security and performance requirements to avoid unsafe shortcuts.

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

Implement a middleware (example name: `JellyfinRoutingMiddleware`) inserted
**before** `app.MapControllers()`.

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

- `Melodee.Controllers.Jellyfin`

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
  - `TokenHash` (SHA-256 or stronger; store only hash)
  - `CreatedAt`, `LastUsedAt`
  - `ExpiresAt` (nullable for non-expiring, but prefer expiry)
  - `RevokedAt` (nullable)
  - `Client`, `Device`, `DeviceId`, `Version` (from headers when available)

### Issuance endpoint

Implement:

- `POST /Users/AuthenticateByName`

Behavior:

- Validate username/password against Melodee’s user system.
- On success, create a new Jellyfin access token.
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

Unit tests:

- routing middleware rewrites only expected paths
- routing middleware does not rewrite `/api/*`, `/rest/*`, `/song/*`
- token parser accepts all supported token formats

### Phase 1: Server discovery and login

Endpoints:

- `GET /System/Info/Public`
- `GET /System/Ping`
- `GET /System/Info` (may require auth)
- `POST /Users/AuthenticateByName`

Behavior:

- `/System/Info/Public` returns enough fields for clients to display server name

  and decide it is a Jellyfin-compatible server.
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
- `POST /Playlists` (optional)
- `GET /Playlists/{id}` and playlist item listing

Notes:

- Images: serve album art / artist art with correct caching headers.
- If Jellyfin requires image tags, implement stable tags based on last-modified.

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

### SettingRegistry keys (proposed)

If Melodee prefers database-backed configuration, add keys:

- `SettingRegistry.JellyfinEnabled`
- `SettingRegistry.JellyfinTokenExpiryHours`
- `SettingRegistry.JellyfinMaxTokensPerUser`
- `SettingRegistry.JellyfinRoutePrefix`

All new settings must be added to:

- `MelodeeConfiguration.AllSettings(...)`
- test configuration in `tests/Melodee.Tests.Common/TestsBase.cs`

## Security Considerations

### Token rotation and revocation

- Do not store tokens in plaintext.
- Support multiple tokens per user (per device) and per-token revocation.
- Support expiry; ensure stale tokens cannot be used indefinitely.

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
- Use ETags for images and optionally for library responses if stable.

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

## Observability

- Log Jellyfin requests with:

  - correlation id (`HttpContext.TraceIdentifier`)
  - user id (if available)
  - client/device headers (sanitized)
- Add structured logs for:

  - token issuance, revocation, auth failures
  - streaming start/stop and cancellation