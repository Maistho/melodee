---
title: API
permalink: /api/
---

# API

This page documents the native Melodee API (Controllers in `src/Melodee.Blazor/Controllers/Melodee/`). For an overview of both API options (Native Melodee API and OpenSubsonic API), see the [API Overview](/apis/) page.

For interactive API documentation and testing, visit the Swagger UI at `/swagger` when Melodee is running.

## Authentication

All native endpoints (unless explicitly noted) require an API key associated with the user making the request. Provide it via header:

```
Authorization: Bearer <user-api-key-guid>
```

If an endpoint returns 401 with `{ error: "Authorization token is invalid" }`, verify the key or that the user is not locked. A 403 indicates the user is locked or blacklisted.

## Versioning

Prefix: `/api/v1/` (future versions will increment the path number; multiple versions may run side‑by‑side).

## Common Response Shape

Paginated list endpoints respond with:

```
{
	"meta": {
		"totalCount": <int>,
		"pageSize": <int>,
		"page": <int>,
		"totalPages": <int>
	},
	"data": [ ... ]
}
```

Errors:

```
{ "error": "Message" }
```

## Endpoints

### System

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | /api/v1/System/info | Server type & semantic version. | No |
| GET | /api/v1/System/stats | Selected system statistics (songs, albums, etc.). | Yes |

#### GET /api/v1/System/info
Returns:
```
{
	"serverType": "Melodee",
	"name": "Melodee API",
	"majorVersion": 1,
	"minorVersion": 0,
	"patchVersion": 0
}
```

### Artists

Base: `/api/v1/artists`

| Method | Path | Query/Body | Description |
|--------|------|------------|-------------|
| GET | /api/v1/artists/{id} | — | Get artist by ID (GUID). |
| GET | /api/v1/artists | q, page, pageSize, orderBy, orderDirection | List artists with pagination and filtering. |
| GET | /api/v1/artists/recent | limit | Get recently added artists. |
| GET | /api/v1/artists/{id}/albums | page, pageSize, orderBy, orderDirection | Get albums for an artist. |
| GET | /api/v1/artists/{id}/songs | q, page, pageSize, orderBy, orderDirection | Get songs for an artist. |
| POST | /api/v1/artists/starred/{apiKey}/{isStarred} | — | Toggle starred status for an artist. |
| POST | /api/v1/artists/setrating/{apiKey}/{rating} | — | Set rating for an artist (0-5). |
| POST | /api/v1/artists/hated/{apiKey}/{isHated} | — | Toggle hated status for an artist. |

### Playlists

Base: `/api/v1/playlists`

| Method | Path | Query/Body | Description |
|--------|------|------------|-------------|
| GET | /api/v1/playlists/{id} | — | Get playlist by ID. |
| GET | /api/v1/playlists | page, pageSize | List all playlists. |
| GET | /api/v1/playlists/{apiKey}/songs | page, pageSize | Get songs in a playlist. |
| POST | /api/v1/playlists | `{ name, comment, isPublic, songIds }` | Create a new playlist. |
| PUT | /api/v1/playlists/{apiKey} | `{ name, comment, isPublic }` | Update playlist metadata. |
| DELETE | /api/v1/playlists/{apiKey} | — | Delete a playlist. |
| POST | /api/v1/playlists/{apiKey}/songs | `[ songId1, songId2, ... ]` | Add songs to a playlist. |
| DELETE | /api/v1/playlists/{apiKey}/songs | `[ songId1, songId2, ... ]` | Remove songs from a playlist. |
| PUT | /api/v1/playlists/{apiKey}/songs/reorder | `{ songIds: [...] }` | Reorder songs (send full list of IDs in new order). |
| POST | /api/v1/playlists/{apiKey}/image | Form File | Upload playlist cover image. |
| DELETE | /api/v1/playlists/{apiKey}/image | — | Delete playlist cover image. |

### User

Base: `/api/v1/user`

Endpoints for the currently authenticated user.

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/v1/user/me | Get current user profile. |
| GET | /api/v1/user/last-played | Get last played songs. |
| GET | /api/v1/user/playlists | Get user's playlists. |
| GET | /api/v1/user/songs/liked | Get liked (starred) songs. |
| GET | /api/v1/user/songs/disliked | Get disliked (hated) songs. |
| GET | /api/v1/user/songs/rated | Get rated songs. |
| GET | /api/v1/user/songs/top-rated | Get top rated songs (4+ stars). |
| GET | /api/v1/user/songs/recently-played | Get recently played songs. |
| GET | /api/v1/user/albums/* | Similar endpoints for albums (liked, disliked, etc.). |
| GET | /api/v1/user/artists/* | Similar endpoints for artists (liked, disliked, etc.). |
| GET | /api/v1/user/me/linked-providers | Get linked social accounts. |
| POST | /api/v1/user/me/link/google | Link Google account. |
| DELETE | /api/v1/user/me/link/google | Unlink Google account. |

### Shares

Base: `/api/v1/shares`

| Method | Path | Query/Body | Description |
|--------|------|------------|-------------|
| GET | /api/v1/shares/{apiKey} | — | Get share details. |
| GET | /api/v1/shares | page, pageSize | List user's shares. |
| POST | /api/v1/shares | `{ shareType, resourceId, description, ... }` | Create a share. |
| PUT | /api/v1/shares/{apiKey} | `{ description, isDownloadable, expiresAt }` | Update a share. |
| DELETE | /api/v1/shares/{apiKey} | — | Delete a share. |
| GET | /api/v1/shares/public/{shareUniqueId} | — | Public access to shared content (No Auth). |

### Requests

Base: `/api/v1/requests`

| Method | Path | Query/Body | Description |
|--------|------|------------|-------------|
| GET | /api/v1/requests | page, pageSize, query, mine, status | List requests. |
| GET | /api/v1/requests/{apiKey} | — | Get request details. |
| POST | /api/v1/requests | `{ category, description, ... }` | Create a new request. |
| PUT | /api/v1/requests/{apiKey} | `{ description, ... }` | Update a request. |
| POST | /api/v1/requests/{apiKey}/complete | — | Mark request as complete. |
| DELETE | /api/v1/requests/{apiKey} | — | Delete a request. |
| GET | /api/v1/requests/{apiKey}/comments | page, pageSize | List comments. |
| POST | /api/v1/requests/{apiKey}/comments | `{ body, parentCommentApiKey }` | Add a comment. |
| GET | /api/v1/requests/activity | — | Check for unread activity. |

### Queue

Base: `/api/v1/queue`

| Method | Path | Query/Body | Description |
|--------|------|------------|-------------|
| GET | /api/v1/queue | — | Get current user's play queue. |
| PUT | /api/v1/queue | `{ songIds, currentSongId, position }` | Save/Update play queue. |
| DELETE | /api/v1/queue | — | Clear play queue. |

### Charts

Base: `/api/v1/charts`

| Method | Path | Query/Body | Description |
|--------|------|------------|-------------|
| GET | /api/v1/charts | page, pageSize, tags, year | List charts. |
| GET | /api/v1/charts/{idOrSlug} | — | Get chart details. |
| GET | /api/v1/charts/{idOrSlug}/playlist | — | Get generated playlist tracks for a chart. |

### Genres

Base: `/api/v1/genres`

| Method | Path | Query/Body | Description |
|--------|------|------------|-------------|
| GET | /api/v1/genres | page, limit, orderBy, orderDirection | List genres. |
| GET | /api/v1/genres/{id}/songs | page, limit | Get songs in a genre. |
| POST | /api/v1/genres/starred/{id}/{isStarred} | — | Toggle starred status. |
| POST | /api/v1/genres/hated/{id}/{isHated} | — | Toggle hated status. |
| GET | /api/v1/genres/starred | — | Get starred genres. |
| GET | /api/v1/genres/hated | — | Get hated genres. |

### Albums

Base: `/api/v1/Albums`

| Method | Path | Query | Description |
|--------|------|-------|-------------|
| GET | /api/v1/Albums/{id} | — | Album by id (GUID). |
| GET | /api/v1/Albums | page, pageSize, orderBy, orderDirection | Paginated list. |
| GET | /api/v1/Albums/recent | limit | Most recently added albums (CreatedAt desc). |
| GET | /api/v1/Albums/{id}/songs | — | All songs for an album (includes user rating if available). |

Notes:

- orderDirection: `asc` or `desc` (default desc).
- orderBy defaults to CreatedAt.

### Songs

Base: `/api/v1/Songs`

| Method | Path | Query/Body | Description |
|--------|------|------------|-------------|
| GET | /api/v1/Songs | page, pageSize, orderBy, orderDirection | Paginated songs (user aware). |
| GET | /api/v1/Songs/recent | limit | Recently added songs. |
| POST | /api/v1/Songs/starred/{songId}/{isStarred} | — | Set or clear starred flag for user. |
| POST | /api/v1/Songs/setrating/{songId}/{rating} | — | Set rating (integer). |
| GET | /song/stream/{songId}/{userId}/{authToken} | Range headers | Stream (supports partial content). |

Streaming:

- This path omits the `/api/v1` version prefix intentionally for compatibility.
- authToken is a time‑limited HMAC token (client generates using user public key) encoded Base64.
- Supports `Range: bytes=...` for efficient seeking & partial delivery.

### Search

Base: `/api/v1/Search`

| Method | Path | Body / Params | Description |
|--------|------|---------------|-------------|
| POST | /api/v1/Search | JSON `SearchRequest` | Multi‑type search (songs, albums, artists). |
| GET | /api/v1/Search/songs | q, page, pageSize, filterByArtistApiKey | Focused song search. |

SearchRequest Fields (selected):

| Field | Type | Notes |
|-------|------|-------|
| query | string | Search text |
| albumPageValue / artistPageValue / songPageValue | short | Per‑type paging |
| pageSize | short? | Default 50 |
| type | string? | CSV of enum flags (Songs, Albums, Artists, Data) |
| filterByArtistId | GUID? | Limit songs by artist |

Response returns aggregated counts + typed collections. User ratings / starred info are included where applicable.

### Scrobble

Base: `/api/v1/Scrobble`

| Method | Path | Body | Description |
|--------|------|------|-------------|
| POST | /api/v1/Scrobble | `{ songId, scrobbleType, playedDuration, playerName }` | Submit now playing or played scrobble event. |

`scrobbleType` values: `NowPlaying`, `Played`. On success returns 200 with empty body.

### Authorization & Blacklists

Several endpoints enforce additional checks:

- User locked -> 403
- Blacklisted IP / Email (Search & Songs modifications, Streaming) -> 403 with `{ error: "User is blacklisted" }`

### Rate / Concurrency Limits

Streaming uses an internal limiter keyed per user to prevent excessive simultaneous streams (configurable in future releases). Exceeding yields HTTP 429.

### Status Codes Summary

| Code | Meaning |
|------|---------|
| 200 | Success |
| 206 | Partial Content (streaming range response) |
| 400 | Bad request / validation failure |
| 401 | Missing/invalid API key or auth token |
| 403 | User locked / blacklisted |
| 404 | Resource not found (album, song) |
| 429 | Too many concurrent streams |

### Example: Paginated Albums

Request:
```
GET /api/v1/Albums?page=1&pageSize=25&orderBy=CreatedAt&orderDirection=desc
Authorization: Bearer <apiKey>
```

Response (truncated):
```
{
	"meta": { "totalCount": 10234, "pageSize": 25, "page": 1, "totalPages": 410 },
	"data": [ { "id": "...", "name": "...", "songCount": 12, ... } ]
}
```

### Example: Stream a Song

1. Obtain user public key & generate an HMAC based timed token (client library forthcoming).
2. Issue request:
```
GET /song/stream/<songId>/<userId>/<authToken>
Range: bytes=0-524287
```
3. Handle 206 partial response & continue ranged requests as needed.

### Recent Refactors

The API layer was re‑architected to delegate operations to consolidated domain services (authentication, ratings, playlists, search, caching) reducing duplication and improving reliability without changing external contracts.

### Official Clients & Ecosystem

| Client | Platform | Highlights | Primary API Surface |
|--------|----------|-----------|---------------------|
| MeloAmp | Desktop (Linux / Win / macOS) | Artist/Album/Song browse, drag‑drop queue, theming, equalizer, starring, scrobbling, cross‑platform packaging | Native JSON (fallback OpenSubsonic possible) |
| Melodee Player | Android + Android Auto | Jetpack Compose UI, Media3 playback, voice commands, playlists, search, now‑playing bar, scrobbling | Native JSON (some legacy compatibility) |

#### MeloAmp Details

- Tech: Electron + React + Material‑UI + TypeScript.
- Auth: JWT (obtained via native login against Melodee API) stored locally.
- Features mapping to endpoints:
	- Albums/Songs browsing -> `Albums`, `Songs`, `Search` endpoints.
	- Star / rating actions -> `Songs/starred` and `Songs/setrating`.
	- Scrobbling (play / complete) -> `Scrobble` endpoint.
	- Queue operations are client‑side; future server playlist sync may use a native playlist endpoint (planned).
- Theming & equalizer are client-resident (no server configuration needed).

#### Melodee Player (Android) Details

- Tech: Kotlin, Jetpack Compose, Clean Architecture (data/domain/presentation), Media3 ExoPlayer, Retrofit.
- Android Auto: MediaBrowserService + MediaSession integration; voice command intents map to search + playback calls using `Search` and `Songs` endpoints.
- Caching: May locally cache minimal metadata for smooth navigation; server remains source of truth.
- Scrobbling: Dispatches NowPlaying then Played events to represent progress and completion.
- Supports pull‑to‑refresh to re-query / invalidate local caches via `If-None-Match` (future optimization with etag headers).

## Homelab API Integration

For homelab environments, the API enables several useful integrations:

**Automation Scripts:**
- Automated library management
- Scheduled metadata updates
- Custom reporting and statistics

**Home Automation:**
- Integration with home automation systems (Home Assistant, OpenHAB)
- Voice control via custom integrations
- Presence detection for automatic playback

**Monitoring:**
- System health checks
- Library statistics tracking
- User activity monitoring

If you build another client (CLI player, iOS app, web SPA, etc.) let us know and we’ll list it here.

---

Missing an endpoint? Open an issue with your use‑case so we can prioritize documenting or adding it.

