# Melodee Jukebox / Party Mode — Comprehensive Requirements & Implementation Phases

**Document purpose:** Provide a single, complete set of requirements for implementing “Jukebox / Party Mode” in Melodee, including:
- A **core Party Mode** (shared session + shared queue) that does **not** require server-side audio hardware.
- Optional **Jukebox Backends** (server/headless playback targets) that are **explicitly configured**.
- Optional **OpenSubsonic/Subsonic `jukeboxControl` compatibility**, enabled **only** when a backend is configured.

This document is designed to be handed to a coding agent and broken into implementation phases with clear deliverables and acceptance criteria.

---

## 1. Terminology & Concepts

### 1.1 “Jukebox mode” (Subsonic sense)
A remote-control mode where a client issues **transport + queue commands** and playback happens on a **target** (often the server audio device, but in practice any configured backend output).

### 1.2 “Party Mode” (Melodee core)
A **shared session** where multiple users can collaboratively manage a **shared queue**, and one designated **Endpoint** consumes that queue and plays audio.

### 1.3 Endpoint / Player (Target)
A concrete playback target:
- **MVP**: Melodee **Web Player** (existing Blazor `/musicplayer`) attached to a session.
- Future: MPV headless, Snapcast, MPD, Chromecast, etc. via plugins/backends.

### 1.4 Controller vs Listener
- **Controller (DJ/Controller role):** Can modify queue, skip, seek, play/pause, and optionally volume.
- **Listener:** Can observe queue and now playing state only.

---

## 2. Product Intent

Users asking for “jukebox” typically want:
- A **single shared queue**
- A **now playing** view
- **Add/reorder/remove**
- **Start/stop/skip/seek**
- **Anti-griefing controls** (roles, locks, rate limiting, moderation)
- Optional **volume control**

Melodee must provide this without forcing a headless server to become a local audio player by default.

---

## 3. Goals & Non-goals

### 3.1 Goals
1. **Party sessions** with shared queue and multi-user collaboration.
2. **Endpoint abstraction**: Web Player first; optional backend targets next.
3. Strong **authorization + anti-abuse**.
4. Optional, explicit **server/headless playback** behind configuration flags.
5. Optional **OpenSubsonic/Subsonic jukebox compatibility** behind configuration.

### 3.2 Non-goals (initially)
- Supporting every casting ecosystem protocol in core (Sonos/AirPlay/etc.).
- DRM/protected content.
- Replacing Melodee’s native API with Subsonic jukebox as primary contract.

---

## 4. Architecture Principles (Guardrails)

1. **Default install behavior**
    - Party Mode can be enabled/disabled via config.
    - OpenSubsonic `/rest/jukeboxControl(.view)` **returns 410** unless an explicit backend is configured and enabled.

2. **Two-tier design**
    - **Tier A (Core):** Party sessions + shared queue + playback state. Playback is executed by a designated Endpoint (Web Player first).
    - **Tier B (Optional):** Jukebox Backends (MPV/Snapcast/MPD/etc.) plugged in behind feature flags.

---

## 5. User Stories

### 5.1 Party Host / Owner
- Create a party session, set name + join method (link/code/PIN).
- Choose a playback endpoint (or leave unassigned until later).
- Promote a friend to DJ/Controller.
- Lock queue or enable “DJ-only control”.
- Kick/ban a disruptive participant.
- End the session.

### 5.2 DJ / Controller
- Add songs/albums/playlists to the queue.
- Reorder queue items.
- Skip, seek, play/pause.
- Optionally control volume (if enabled).

### 5.3 Listener
- Join session and view queue + now playing.
- See who added each queue item and when.

### 5.4 Endpoint (Web Player)
- Attach to a session and consume its queue.
- Heartbeat to server with current position/state.
- Resume gracefully after refresh or temporary disconnect.

---

## 6. Functional Requirements

### 6.1 Party Session Lifecycle
- **Create session**
    - Fields: name, visibility (private/public), join code/PIN (optional), default role for joiners.
    - Owner selects endpoint (optional).
- **Join session**
    - By link and/or join code.
    - Server assigns role (default Listener) unless owner changes.
- **Leave session**
    - Participant leaves session.
    - If Endpoint leaves/dies, session remains active but endpoint becomes stale.
- **End session**
    - Owner ends; session becomes read-only and playback is stopped on endpoint (best-effort).

### 6.2 Shared Queue
A session has an ordered queue of **QueueItems** with:
- `QueueItemId` (ApiKey)
- `SongApiKey`
- `EnqueuedAt`
- `EnqueuedByUserId`
- `Source` (optional: album/playlist/search)
- `Note` (optional)
- `SortOrder` (or position index)

Operations (minimum):
- Add songs (single/multiple) by song id
- Add album → expands to songs (respect album order)
- Add playlist → expands to songs (playlist order)
- Remove queue item
- Reorder items
- Clear queue
- Set current item (owner/controller)

Concurrency:
- Use optimistic concurrency with a monotonically increasing `QueueRevision` (and/or HTTP `ETag`).
- All queue mutations must validate `expectedRevision` and return conflict if stale.

### 6.3 Playback State Model (Authoritative)
Session tracks:
- `CurrentQueueItemId` (or index)
- `PositionSeconds`
- `IsPlaying`
- `Volume` (nullable; endpoint dependent)
- `LastHeartbeatAt` (from active endpoint)
- `LastUpdatedByUserId` (audit)
- `PlaybackRevision` (optional; similar to queue revision)

Rules:
- Endpoint is the source of truth for **position/time**.
- Controllers are the source of truth for **intent** (play/pause/skip/seek), which the endpoint executes.

### 6.4 Endpoints / Players
Endpoint requirements:
- Identity: `EndpointId` (ApiKey), `Name`, `Type`
- Ownership: `OwnerUserId` or `System`
- Scope: `Room` / `Shared` label (optional)
- Capabilities:
    - `canPlay`, `canPause`, `canSeek`, `canSkip`, `canSetVolume`, `canReportPosition`
- Heartbeat:
    - Endpoint sends heartbeat every N seconds (config default e.g., 5–10s)
    - Heartbeat includes: isPlaying, currentQueueItemId, positionSeconds, volume if applicable

MVP Endpoint:
- Existing Blazor `/musicplayer` becomes an attachable endpoint:
    - Accept `?session=<sessionKey>` (or UI flow) to attach
    - Consumes session queue & plays audio via browser
    - Reports progress + state back to server

### 6.5 Permissions & Anti-abuse
Global gate:
- All Party/Jukebox features require `User.HasJukeboxRole` (or equivalent policy).

Session roles:
- **Owner**
    - Assign endpoint
    - Promote/demote roles
    - Lock/unlock queue
    - Kick/ban participants
    - End session
- **DJ/Controller**
    - Add/remove/reorder queue
    - Skip / set current
    - Seek (if endpoint supports)
    - Volume (if enabled + endpoint supports)
- **Listener**
    - View-only

Anti-abuse:
- Rate limiting per user and per session on:
    - Add-to-queue
    - Skip/seek
    - Volume changes
- Moderation:
    - Queue lock (only owner/DJs can change)
    - “Skip cooldown” (e.g., 10s)
    - “Max adds per minute” (config)
- Audit trail:
    - Log queue mutations and control commands with user identity and timestamp.

### 6.6 Real-time Updates
Preferred:
- SignalR for:
    - `QueueChanged(sessionId, revision, diff)`
    - `PlaybackStateChanged(sessionId, state)`
    - `ParticipantsChanged(sessionId, participants)`
      Fallback:
- Polling with `If-None-Match` / ETag and `revision` endpoints.

---

## 7. API Requirements (Native API is Primary)

> Exact route conventions should align with existing `api/v1/...` patterns.

### 7.1 Party Sessions
- `POST   /api/v1/party-sessions`
- `GET    /api/v1/party-sessions/{id}`
- `POST   /api/v1/party-sessions/{id}/join`
- `POST   /api/v1/party-sessions/{id}/leave`
- `POST   /api/v1/party-sessions/{id}/end`

Participants:
- `GET    /api/v1/party-sessions/{id}/participants`
- `POST   /api/v1/party-sessions/{id}/participants/{userId}/role`
- `POST   /api/v1/party-sessions/{id}/participants/{userId}/kick`
- `POST   /api/v1/party-sessions/{id}/participants/{userId}/ban`

### 7.2 Queue
- `GET    /api/v1/party-sessions/{id}/queue` → includes `revision` + items
- `POST   /api/v1/party-sessions/{id}/queue/items` → add (songs/album/playlist)
- `DELETE /api/v1/party-sessions/{id}/queue/items/{itemId}` → remove
- `POST   /api/v1/party-sessions/{id}/queue/reorder` → move/position
- `POST   /api/v1/party-sessions/{id}/queue/clear`

All mutation endpoints should accept `expectedRevision`.

### 7.3 Playback Control (Intent)
- `GET    /api/v1/party-sessions/{id}/playback`
- `POST   /api/v1/party-sessions/{id}/playback/play`
- `POST   /api/v1/party-sessions/{id}/playback/pause`
- `POST   /api/v1/party-sessions/{id}/playback/skip` (optional: to next, or specify item)
- `POST   /api/v1/party-sessions/{id}/playback/seek` (positionSeconds)
- `POST   /api/v1/party-sessions/{id}/playback/volume` (0.0–1.0 or 0–100, if supported)

### 7.4 Endpoints
- `GET    /api/v1/endpoints` (available endpoints for user)
- `POST   /api/v1/endpoints/register` (for web player + future plugins)
- `POST   /api/v1/endpoints/{endpointId}/attach` (attach endpoint to session)
- `POST   /api/v1/endpoints/{endpointId}/detach`
- `POST   /api/v1/endpoints/{endpointId}/heartbeat` (state + position)

---

## 8. OpenSubsonic/Subsonic Compatibility (Optional, Backend-gated)

### 8.1 Default behavior
- `/rest/jukeboxControl(.view)` returns **410 Gone** unless:
    - `jukebox.enabled=true` AND
    - a backend is configured AND
    - an active endpoint/back-end target is selected.

### 8.2 Supported actions (minimum viable)
Implement the Subsonic jukebox action set when enabled:
- `get`, `status`, `set`
- `start`, `stop`
- `skip` (support `index` and `offset` seconds)
- `add` (multiple `id`)
- `clear`
- `remove` (by index)
- `shuffle`
- `setGain` (volume 0.0–1.0), only if backend supports

### 8.3 Mapping Strategy
- Map Subsonic “jukebox playlist” to Melodee Party Session queue.
- Use a configured “default party session” OR create an internal “Jukebox Session” for that backend.
- Enforce permissions (`HasJukeboxRole`) and apply the same rate limits.

---

## 9. Jukebox Backend Design (Optional Tier)

### 9.1 Backend goals
- Allow headless/server playback or external control targets without coupling Melodee core to specific players.
- Support multiple backend types over time with consistent contracts.

### 9.2 Proposed backend interface
Define a backend contract, e.g. `IPlaybackEndpointBackend` with methods:
- `GetCapabilities()`
- `Play(queueItem, startPositionSeconds?)`
- `Pause()`, `Stop()`
- `Skip(nextQueueItem)`
- `Seek(positionSeconds)`
- `SetVolume(value)` (optional)
- `GetStatus()` (current track id, position, isPlaying, volume)
- Events/callbacks:
    - `OnTrackEnded`, `OnError`, `OnPositionChanged` (optional)

### 9.3 Reference backend: MPV (recommended first backend)
Feature knobs commonly expected by users:
- MPV executable path override
- Audio device selection (MPV audio device name)
- Extra args / command template for integration (e.g., Snapcast scenarios)
- IPC socket for real-time control
- Process supervision (restart on crash)

> Implementation details are flexible; the requirement is that the backend can be configured and can report status reliably.

### 9.4 Operational constraints
- Docker deployments may need:
    - `/dev/snd` device pass-through
    - appropriate group permissions
- Document these clearly when the backend is enabled.

---

## 10. Data Model Requirements

> Names illustrative; align with Melodee conventions.

### 10.1 Core tables
- `PartySession`
    - ApiKey, Name, OwnerUserId, Status (Active/Ended)
    - JoinCodeHash (optional), ActiveEndpointId (nullable)
    - CreatedAt, UpdatedAt
    - QueueRevision (long), PlaybackRevision (long)

- `PartySessionParticipant`
    - PartySessionId, UserId, Role, JoinedAt, LastSeenAt

- `PartyQueueItem`
    - ApiKey, PartySessionId, SongId/SongApiKey
    - EnqueuedByUserId, EnqueuedAt
    - SortOrder, Source, Note

- `PartyPlaybackState`
    - PartySessionId
    - CurrentQueueItemApiKey
    - PositionSeconds
    - IsPlaying
    - Volume (nullable)
    - LastHeartbeatAt
    - UpdatedByUserId

- `Endpoint`
    - ApiKey, OwnerUserId, Name, Type
    - CapabilitiesJson
    - LastSeenAt
    - IsShared (bool), Room (optional)

### 10.2 Reuse / boundaries
- Existing per-user queue (`PlayQues`) remains private playback; do not overload it.
- Existing `Players` may inform endpoint discovery but Party endpoints require capabilities.

---

## 11. UI Requirements (Blazor)

### 11.1 Session management
- Create session, show join link + join code/PIN
- Join by link/code
- Session dashboard:
    - Now playing
    - Queue with add/reorder/remove
    - Participants list + roles
    - Owner controls (lock queue, kick/ban, assign endpoint)

### 11.2 Endpoint selection
- List endpoints:
    - My web players (active browser tabs)
    - Shared room endpoints (future)
- Assign selected endpoint to session

### 11.3 Web Player attachment
- `/musicplayer` can attach to a session:
    - session picker + “Attach”
    - or query string parameter
- Displays “Party Mode” UI state and sends heartbeats.

---

## 12. Security, Privacy, Reliability

Security:
- Hash join codes/PINs; never store raw secrets.
- Prevent IDOR: validate membership + role for every operation.
- Apply rate limiting to control endpoints.
- Audit trail persisted for key actions.

Reliability:
- Heartbeat staleness detection:
    - if `LastHeartbeatAt` exceeds threshold (e.g., 30s), endpoint considered stale
    - controllers can reassign endpoint
- If endpoint dies mid-track:
    - session remains active; state persists; new endpoint can resume from last known position

Performance:
- Queue operations should be O(log n) / O(n) acceptable for typical party sizes (hundreds of tracks).
- Avoid broadcasting full queue on every change; prefer diffs once SignalR exists.

---

## 13. Implementation Phases (Agent-ready)

### Phase 0 — Foundations (DB + Domain + Policies)
**Deliverables**
- New entities/tables + migrations for PartySession, Participants, QueueItems, PlaybackState, Endpoint
- Authorization policies and role model
- Feature flags:
    - `partyMode.enabled`
    - `jukebox.enabled` (default false)
- API scaffolding and DTOs

**Acceptance**
- DB migrates cleanly; endpoints compile; basic auth checks in place.

---

### Phase 1 — Party Sessions + Shared Queue (Polling OK) + Web Player Endpoint MVP
**Deliverables**
- Party session CRUD + join/leave/end
- Shared queue operations with `QueueRevision` optimistic concurrency
- Basic playback intent endpoints (play/pause/skip/seek) storing intent/state
- Web Player attachment:
    - attach to session
    - fetch queue/current item
    - play in browser and heartbeat position/state
- Polling refresh for controllers/listeners

**Acceptance**
- Two users can join same session, add songs, see consistent queue.
- Web player plays from shared queue and advances to next track.
- Unauthorized users cannot control.

---

### Phase 2 — Real-time + Moderation + Anti-abuse
**Deliverables**
- SignalR hub + events for queue/playback/participants
- Owner tools: queue lock, kick/ban, role changes
- Rate limiting for control actions
- Audit trail for queue and playback commands

**Acceptance**
- UI updates instantly (queue + now playing).
- Skip spam is blocked by rate limits/cooldowns.
- Owner can moderate.

---

### Phase 3 — Endpoint Registry & Capability Model (First-class)
**Deliverables**
- Endpoint register/list/attach/detach semantics
- Capability-driven UI (hide/disable unsupported controls)
- Staleness detection + “reassign endpoint” flow

**Acceptance**
- Multiple endpoints show up; owner can switch endpoint mid-session.
- Controls reflect endpoint capabilities.

---

### Phase 4 — Optional MPV Backend (Headless Jukebox)
**Deliverables**
- Backend abstraction + MPV backend implementation
- Config:
    - enabled flag
    - mpv path
    - audio device selection
    - extra args/template
- Observability: backend health + error surfaced in session

**Acceptance**
- In a configured deployment, MPV backend plays session queue headlessly.
- Play/pause/skip/seek work; state remains consistent.

---

### Phase 5 — OpenSubsonic/Subsonic `jukeboxControl` (Backend-gated)
**Deliverables**
- Enable `/rest/jukeboxControl(.view)` only when backend enabled
- Implement action mapping to a dedicated Jukebox Session or configurable session
- Test matrix using at least one Subsonic client that supports jukebox

**Acceptance**
- Default build still returns 410.
- When enabled, `get/status/add/start/stop/skip/setGain` etc. function correctly.

---

### Phase 6 — Additional Backends (Optional roadmap)
**Deliverables**
- Snapcast/MPD plugin(s) (choose one first)
- Multi-room patterns (multiple backend instances/devices)

**Acceptance**
- At least one additional backend works end-to-end and is documented.

---

## 14. Testing Requirements

Unit tests:
- Queue concurrency + revision conflicts
- Role enforcement
- Rate limiting

Integration tests:
- Session lifecycle
- Endpoint heartbeat updates playback state
- Web player plays and advances queue

E2E (smoke):
- Two browsers join a session; one acts as endpoint; other as controller.

---

## 15. Acceptance Criteria Summary (Must-have)
- Multi-user shared queue with consistent ordering and revision-based concurrency.
- Web player can act as a party endpoint (attach + play + heartbeat).
- Role-based permissions and anti-abuse controls exist.
- Default install does NOT enable server-side jukebox and keeps `jukeboxControl` disabled (410).
- Optional backend + Subsonic jukebox compatibility can be enabled explicitly.

---

## Appendix A — Notes for Coding Agents
- Prefer incremental, mergeable PRs per phase.
- Each phase must include:
    - migrations + models
    - API endpoints + tests
    - minimal UI wiring (where applicable)
    - docs updates
- Keep Party Mode (core) and Jukebox Backends (optional) cleanly separated.

