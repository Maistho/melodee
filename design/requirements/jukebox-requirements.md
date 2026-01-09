## Jukebox / Party Mode Requirements (Melodee)

### Status

- Melodee exposes OpenSubsonic/Subsonic jukebox route(s) (`/rest/jukeboxControl(.view)`) but intentionally returns HTTP `410 Gone`.
- Melodee keeps “server-side audio output” optional; `jukeboxControl` remains disabled unless a jukebox backend is explicitly configured (ADR-0007).
- Melodee **does** already have primitives that are adjacent to “jukebox-like” experiences:
  - Per-user persisted queue (`PlayQues` table) + `UserQueueService` + `api/v1/queue` endpoints.
  - Client tracking (`Players` table) and “Now Playing” activity.
  - A Blazor `/musicplayer` page that plays audio in the browser.

This document defines a competitive “party mode” / “jukebox” feature set and the guardrails to keep server-side playback explicit and optional (ADR-0007).

### Terms

- **Jukebox (Subsonic sense)**: server-side playback control where audio is emitted from an output accessible to the server process.
- **Party mode**: a shared queue/session where multiple users can add/reorder, and one designated player endpoint consumes the queue.
- **Endpoint / Player**: a concrete playback target (browser tab, Snapcast client, Chromecast, MPD instance, etc.).
- **Controller**: a client that can modify a party session (add songs, skip, volume).
- **Listener**: a client that can observe the session (now playing, queue) but not control.

### Product intent

Deliver a feature that competitors often call “jukebox” (party / shared playback), without forcing Melodee to become an always-on local audio player on headless servers.

## Goals

- Provide a **shared queue** (“party session”) that multiple users can collaboratively manage.
- Provide a **device/endpoint abstraction** so Melodee can target:
  - the web UI music player (browser-based playback), and
  - optionally external playback backends (Snapcast/MPD/etc.) via plugins.
- Provide strong **authorization** and anti-abuse controls for “remote control” features.
- Keep “server-side audio output” optional and explicitly configured (ADR-0007).

## Non-goals (initially)

- Implementing every casting protocol (Sonos, AirPlay, etc.) in core.
- DRM / protected streaming.
- Using OpenSubsonic jukebox as the *only* API surface (Melodee should have a clean native API as the primary contract).

## Competitive baseline (what users expect when they say “jukebox”)

Typical expectations taken from Subsonic ecosystem servers and “party mode” concepts:

- Start/stop/skip and “now playing” view.
- Add to queue (by song/album/playlist) and reorder/remove.
- A single shared queue that is not tied to a single user’s private playback.
- A way to prevent griefing: roles, locks, and moderation.
- Optional volume control for the endpoint.

## Architecture alignment (ADR-0007)

Melodee can satisfy the market need while keeping server-side audio output explicit and optional (ADR-0007) by defining **two tiers**:

1. **Core feature: Party sessions + shared queue**
   - Playback happens on a designated endpoint (initially, the Blazor Music Player in a browser).
   - Melodee stores and serves state; it does not need local audio hardware.

2. **Optional plugin/module: Jukebox backend(s)**
   - If a deployment explicitly wants server-side or headless endpoint playback, implement adapters (e.g., MPD, Snapcast server, PipeWire/ALSA) behind a feature flag.
   - Only when such a backend is configured should Melodee consider implementing OpenSubsonic `jukeboxControl` semantics.

## Functional requirements

### 1) Party session lifecycle

- Create session:
  - Owner (user) creates a party session with a name, optional PIN/join code, and default permissions.
  - Owner selects a target endpoint (see Endpoints) or leaves it unassigned.
- Join session:
  - Users can join by link or join code.
  - Session exposes role/permission model: Owner, DJ, Controller, Listener.
- End session:
  - Owner ends the session; server marks session closed and stops accepting control commands.

### 2) Shared queue

- Session has an ordered queue of songs (by Song ApiKey).
- Required operations:
  - Add: enqueue song(s) (single song, album, playlist).
  - Remove: delete a queued item.
  - Reorder: move items (with concurrency control).
  - Clear: empty queue.
  - Set current: choose the current item.
- Queue items should carry minimal metadata:
  - SongApiKey, enqueuedAt, enqueuedByUserId, optional “note”, and an optional “source” (playlist/album).
- Concurrency:
  - Must be resilient to multiple clients sending changes.
  - Prefer optimistic concurrency using a monotonically increasing `queueRevision` or `ETag`.

### 3) Playback state model

A session must track:

- `currentQueueItemId` (or index)
- `positionSeconds`
- `isPlaying`
- `volume` (optional; endpoint dependent)
- `lastHeartbeatAt` from the active endpoint

The playback state is authoritative for controllers to see “now playing” and for endpoints to resume/continue.

### 4) Endpoints / players

- An endpoint represents “where music is played”.
- MVP endpoint: the existing Blazor `/musicplayer` page acts as a playback endpoint.
  - The page should be able to “attach” to a party session and consume the session queue.
  - It must periodically heartbeat to the server to indicate it is active.
- Future endpoints (plugin-based): Snapcast client group, MPD instance, Chromecast target, etc.

Endpoint requirements:

- Must have a unique identity (`endpointId`) and a display name.
- Must be associated with an owning user (or system) and optionally a shared/room scope.
- Must declare capabilities:
  - canPlay, canPause, canSeek, canSkip, canSetVolume, canReportPosition.

### 5) Permissions and anti-abuse

- Gate all “party/jukebox control” features behind `User.HasJukeboxRole`.
- Session-level permissions:
  - Owner can:
    - assign endpoint
    - promote/demote roles
    - lock/unlock queue
    - kick/ban participants
  - Controllers/DJs can:
    - add/remove/reorder queue
    - skip
    - set current
    - optionally control volume (configurable)
  - Listeners can only view
- Rate limiting:
  - Protect “add to queue”, “skip”, “volume” with rate limits.
- Audit trail:
  - Record “who changed what” for queue mutations and playback control.

### 6) Real-time updates

- Clients need timely updates for queue and now playing.
- Prefer SignalR for session events:
  - `QueueChanged(sessionId, revision, diff)`
  - `PlaybackStateChanged(sessionId, state)`
  - `ParticipantChanged(sessionId, participants)`
- Polling fallback is acceptable for MVP.

## API requirements

### A) Melodee native API (primary)

Proposed endpoints (illustrative; align with existing `api/v{version}/...` patterns):

- `POST /api/v1/party-sessions` create session
- `GET /api/v1/party-sessions/{id}` session details
- `POST /api/v1/party-sessions/{id}/join` join session
- `POST /api/v1/party-sessions/{id}/leave` leave session
- `POST /api/v1/party-sessions/{id}/end` end session

Queue:

- `GET /api/v1/party-sessions/{id}/queue` (includes `revision`)
- `PUT /api/v1/party-sessions/{id}/queue` replace queue (admin/control)
- `POST /api/v1/party-sessions/{id}/queue/items` add
- `DELETE /api/v1/party-sessions/{id}/queue/items/{itemId}` remove
- `POST /api/v1/party-sessions/{id}/queue/reorder` reorder

Playback state:

- `GET /api/v1/party-sessions/{id}/playback`
- `POST /api/v1/party-sessions/{id}/playback/play`
- `POST /api/v1/party-sessions/{id}/playback/pause`
- `POST /api/v1/party-sessions/{id}/playback/seek` (positionSeconds)
- `POST /api/v1/party-sessions/{id}/playback/skip`
- `POST /api/v1/party-sessions/{id}/playback/volume` (if supported)

Endpoints:

- `GET /api/v1/endpoints` list endpoints available to user
- `POST /api/v1/endpoints/heartbeat` (endpointId, sessionId, state)

### B) OpenSubsonic `jukeboxControl` (optional / plugin-gated)

Current behavior should remain:

- Default install: return HTTP `410 Gone` per ADR-0007.

If a deployment enables a jukebox backend:

- Implement `jukeboxControl` with Subsonic semantics **only when**:
  - a backend is configured, and
  - a default target endpoint is selected.

Supported actions should be explicitly documented and tested (the Subsonic ecosystem commonly expects):

- `get` / `status` (queue + current)
- `set` (replace queue)
- `start`, `stop`, `skip`
- `add` (add track(s))
- `clear`
- `remove` (remove by index)
- `setGain` / volume (only if backend supports)

Compatibility note:

- Many jukebox clients assume audio plays on the server itself. For those clients, this only works if the configured backend truly represents an audible output in the user’s environment (server audio hardware, Snapcast output, etc.).

## Data model requirements

### MVP (party sessions in core)

Add new tables/entities (names illustrative; align with existing style):

- `PartySession`
  - ApiKey
  - Name
  - OwnerUserId
  - JoinCodeHash (optional)
  - Status (Active/Ended)
  - ActiveEndpointId (nullable)
  - CreatedAt / LastUpdatedAt

- `PartySessionParticipant`
  - PartySessionId
  - UserId
  - Role (Owner/DJ/Controller/Listener)
  - JoinedAt

- `PartyQueueItem`
  - PartySessionId
  - ApiKey
  - SongId + SongApiKey
  - EnqueuedByUserId
  - EnqueuedAt
  - SortOrder

- `PartyPlaybackState`
  - PartySessionId
  - CurrentQueueItemApiKey
  - PositionSeconds
  - IsPlaying
  - Volume (nullable)
  - UpdatedByUserId (nullable)
  - LastHeartbeatAt

- `Endpoint`
  - ApiKey
  - OwnerUserId
  - Name
  - Type (WebPlayer/Snapcast/MPD/Chromecast/…)
  - Capabilities JSON
  - LastSeenAt

### Reuse opportunities

- Existing per-user `PlayQues` and `QueueController` should remain as “private queues”. Party sessions should be distinct (do not overload per-user queue).
- Existing `Players` table can inform endpoint discovery/last seen, but party endpoints likely need additional capability fields.

## UI requirements (Blazor)

- Session creation/join screen:
  - Create session, show join link / QR.
  - Join a session by link/code.
- Party session view:
  - Now playing + queue.
  - Add songs (search/pick) and reorder.
  - Role/permissions UI for owner.
- Endpoint selection:
  - Choose a target endpoint for the session.
- Web player integration:
  - `/musicplayer` can attach to a session (e.g., `?session=<id>`).
  - Acts as the “active endpoint” by heartbeating and reporting position.

## Security requirements

- Treat join codes as secrets:
  - Store hashed join codes; do not store raw PINs.
- Prevent IDOR:
  - Session and queue operations must verify membership and role.
- Rate limiting:
  - Apply tight rate limits to control actions to prevent denial-of-service and “skip spam”.
- Auditability:
  - Store who made each queue mutation and control command.

## Operational requirements

- Feature flags / configuration:
  - `partyMode.enabled` (core)
  - `jukebox.backends.*` (optional plugin backends)
- Clear documentation for “jukebox backend” deployments:
  - required ports/devices
  - container/VM pass-through
  - expected reliability model

## Telemetry and diagnostics

- Track:
  - active sessions
  - queue mutations per minute
  - active endpoints and heartbeat staleness
  - playback errors by backend type

## Rollout plan (suggested)

### Phase 1: Party sessions + web player endpoint

- Implement party sessions + shared queue + permissions.
- Integrate with `/musicplayer` as the first endpoint.
- Provide polling-based updates (SignalR optional).
- Keep `/rest/jukeboxControl` returning 410.

### Phase 2: Real-time + moderation

- SignalR events for queue/playback updates.
- Owner moderation tools (kick, lock queue, restrict volume).

### Phase 3: Optional backends + OpenSubsonic jukeboxControl (opt-in)

- Add a single well-scoped backend first (e.g., Snapcast or MPD).
- Only then enable OpenSubsonic `jukeboxControl` behind configuration.

## Acceptance criteria

- Multiple users can join a party session, add songs, and see a consistent queue.
- A single web player endpoint can attach to the session and play through the queue.
- Permission rules prevent non-controllers from skipping or griefing.
- Default builds remain aligned with ADR-0007 (no implicit server-side audio output; `jukeboxControl` stays 410 unless explicitly enabled).
