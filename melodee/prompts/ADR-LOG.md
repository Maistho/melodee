# Architecture Decision Records (ADR Log)

This file captures significant architectural/product decisions for Melodee.

---

## ADR-0001: Do not implement OpenSubsonic/Subsonic Jukebox Control

- Date: 2025-12-13T16:17:46.094Z
- Status: Accepted

### Context

OpenSubsonic/Subsonic defines a `jukeboxControl` endpoint intended for **server-side playback control** (“jukebox mode”), where the server is responsible for maintaining a playback queue and emitting audio through an audio output accessible to the server process.

Melodee is commonly deployed as a web application in **headless** server environments (e.g., Proxmox) and often within **containers**.

### Decision

We will **not implement** server-side jukebox playback.

Instead, Melodee will implement the `jukeboxControl` endpoint(s) but always respond with **HTTP 410 Gone** ("not supported") to indicate the feature is intentionally unavailable.

### Rationale

- Typical Melodee deployments (including the author’s) run in Proxmox/containerized environments where:
  - There is no reliable, configured audio output device exposed to the application.
  - There is no long-running audio playback engine/process integrated with Melodee.
- Implementing jukebox properly would require additional OS/device integration and persistent playback state management that does not align with Melodee’s primary usage (streaming to clients).

### Consequences

- Clients that attempt to use jukebox control will receive a clear, consistent "not supported" response.
- Melodee remains focused on streaming playback to clients rather than acting as a local player.

### Revisit / Future Work

If a contributor wants to add jukebox support in the future, it should likely be implemented as an optional plugin/module with explicit documentation for:
- required audio backend (ALSA/Pulse/etc.)
- required container/VM device pass-through
- state persistence and concurrency semantics

---

## ADR-0002: Similar Songs is admin-managed (ArtistRelationType.Similar)

- Date: 2025-12-13T16:30:20.883Z
- Status: Accepted

### Context

OpenSubsonic defines `getSimilarSongs` / `getSimilarSongs2`. Many servers implement this by calling third-party services (Last.fm/Spotify/etc.) or by doing behavior-based recommendations.

Melodee already has an `ArtistRelation` table and an `ArtistRelationType.Similar` value, and Melodee supports role-based editing (Admin/Editor).

### Decision

Melodee will compute “similar songs” using **curated, local similarity**:

- Similarity is defined by **Artist → Similar Artists** relationships managed by Admin/Editor users (`ArtistRelationType.Similar`).
- `getSimilarSongs(2)` will return songs drawn from:
  1) the requested song/artist’s own catalog (optional), and
  2) the catalog of related “similar” artists.

### Rationale

- Avoids reliance on third-party APIs/credentials and improves determinism.
- Fits a self-hosted/air-gapped deployment model.
- Allows the library owner to control recommendations and quality.

### Consequences

- Similar songs quality depends on how well similarity relationships are maintained.
- Requires UI/management workflows for Admin/Editor to curate “similar” relationships.

### Revisit / Future Work

Optionally add fallback strategies when there are no curated relationships (e.g., same genre/tags, same contributors, play-history co-occurrence) but keep curated similarity as the primary signal.

---

## ADR-0003: External integrations use Settings + caching

- Date: 2025-12-13T16:35:42.249Z
- Status: Accepted

### Context

Melodee integrates with external services (Spotify, Last.fm, iTunes, etc.) for search/scrobbling/metadata.

We need a consistent strategy for where credentials are stored, what happens when credentials are missing/invalid, and how to limit repetitive API calls.

### Decision

- All external API keys/secrets/tokens are stored in the **Settings** table.
- If credentials are **missing or invalid**, the integration is considered **disabled**.
  - The system should **fail gracefully** (no unhandled exceptions) and return an empty/neutral result.
- External API calls should be cached using the DI-injected `ICacheManager`.

### Rationale

- Centralizes configuration for self-hosted deployments.
- Allows explicit enable/disable behavior without depending on environment variables.
- Reduces rate-limit pressure and improves UI responsiveness.

### Consequences

- Some integrations require UI/admin workflows to populate settings.
- Caching introduces staleness; cache keys and TTLs must be chosen carefully.

---

## ADR-0004: Last.fm session key lifecycle (per-user)

- Date: 2025-12-13T16:38:33.838Z
- Status: Accepted

### Context

Last.fm scrobbling requires a user-authorized **session key** (`sk`). Session keys typically do not expire, but can be revoked by the user in Last.fm, and any invalidation must be handled gracefully.

Melodee is a server-hosted app; we should not ask users to provide their Last.fm password to Melodee.

### Decision

- Use the **web authentication** flow (`auth.getSession`) to obtain a session key.
  - Do **not** use `auth.getMobileSession` (requires collecting user password).
- Store the session key **per Melodee user** in the database (`User.LastFmSessionKey`).
  - Treat as a secret: never log it and do not expose it via APIs.
- Runtime behavior:
  - If global Last.fm scrobbling is enabled but the user has no session key: return success/no-op (and optionally log once at Debug/Warn).
  - If Last.fm returns an "invalid session" error during scrobble/now-playing: clear `User.LastFmSessionKey` and require re-linking.
- There is no refresh flow; re-authentication is the only recovery after revocation.

### Rationale

- Avoids handling user passwords and matches Last.fm’s recommended OAuth-style flow.
- Keeps scrobbling user-scoped (different Melodee users can link different Last.fm accounts).
- Makes revocation safe and explicit.

### Consequences

- Requires a UI flow to link/unlink Last.fm for a user.
- Some failures will look like silent no-ops (by design) unless surfaced in UI.
