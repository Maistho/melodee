# Melodee Jukebox / Party Mode — Phased Implementation Guide (Agent-Ready)

This guide breaks the **Melodee Jukebox / Party Mode** requirements into implementation phases that minimize decision-making for a coding agent. Each phase has:
- concrete deliverables
- explicit design choices (so the agent doesn’t invent them)
- acceptance criteria
- required localization updates (9 language resource files)

> Source requirements: `jukebox-requirements.md` (the canonical spec for this feature set).

---

## Phase map (checklist)

- [x] **Phase 0 — Foundations:** DB, domain models, auth policies, feature flags, scaffolding
- [x] **Phase 1 — Party Sessions + Shared Queue + Web Player Endpoint (MVP):** CRUD/join, queue, playback intent, heartbeat, polling UI
- [x] **Phase 2 — Real-time + Moderation + Anti-abuse:** SignalR events, rate limits, queue lock, kick/ban, audit
- [x] **Phase 3 — Endpoint Registry + Capability Model:** endpoints first-class, attach/detach, staleness, capability-driven UI
- [ ] **Phase 4 — MPV Backend:** backend abstraction + MPV backend, config + health reporting
- [ ] **Phase 5 — Subsonic/OpenSubsonic `jukeboxControl` (Backend-gated):** full mapping, tests, default 410
- [ ] **Phase 6 — Additional Backends:** MPD

---

## Global implementation rules (do not improvise)

### Repo conventions (choose once, use everywhere)
- **API base:** keep existing pattern `api/v1/...`
- **DTO naming:** `{Thing}Dto`, `{Thing}CreateRequest`, `{Thing}UpdateRequest`, `{Thing}Response`
- **IDs:** use the project’s existing `ApiKey`-style string IDs for all new public IDs.
- **Error shape:** use existing API error response type; if none, use RFC 7807 `ProblemDetails`.
- **Authorization:** use policy-based auth with role checks (Owner / DJ / Listener) and *session membership* validation.
- **Optimistic concurrency:** *queue mutations require* `expectedRevision` and return `409 Conflict` if mismatched; response includes current `revision`.
- **Feature flags:** configuration-based (e.g., `IOptions<PartyModeOptions>`), not compile-time switches.
- **Localization:** every new user-facing string must be a localization key; update **all 9 language resource files**.

### Do not create new product decisions
If a choice is not specified below, prefer:
- the **simplest implementation** that matches requirements
- an internal `TODO` comment + issue reference (if your workflow supports it), not a new design

---

## Localization requirements (applies to every phase)

### Where to update
There are **9 language resource files** in the repo. Update all of them for any new UI string.

**Rule:** add a new localization key once, then propagate it to all 9 files (same key, translated value if you have translations; otherwise copy English and mark `TODO: translate`).

### Key naming convention (use this)
Use `PartyMode.*` and `Jukebox.*` namespaces:

Examples:
- `PartyMode.Title`
- `PartyMode.CreateSession`
- `PartyMode.JoinSession`
- `PartyMode.Queue`
- `PartyMode.NowPlaying`
- `PartyMode.Role.Owner`
- `PartyMode.Role.DJ`
- `PartyMode.Role.Listener`
- `PartyMode.Endpoint.Assign`
- `PartyMode.Endpoint.Stale`
- `PartyMode.Moderation.Kick`
- `Jukebox.Disabled`
- `Jukebox.Enabled`
- `Jukebox.Backend.Mpv`

### Mandatory key set (baseline)
Ensure these keys exist by the end of Phase 2 (you can add earlier as needed):
- `PartyMode.Title`
- `PartyMode.Session`
- `PartyMode.Session.Create`
- `PartyMode.Session.Name`
- `PartyMode.Session.Join`
- `PartyMode.Session.JoinCode`
- `PartyMode.Session.Leave`
- `PartyMode.Session.End`
- `PartyMode.Queue.Title`
- `PartyMode.Queue.AddSongs`
- `PartyMode.Queue.AddAlbum`
- `PartyMode.Queue.AddPlaylist`
- `PartyMode.Queue.Clear`
- `PartyMode.Queue.Remove`
- `PartyMode.Queue.Reorder`
- `PartyMode.Playback.Play`
- `PartyMode.Playback.Pause`
- `PartyMode.Playback.Skip`
- `PartyMode.Playback.Seek`
- `PartyMode.Playback.Volume`
- `PartyMode.Participants.Title`
- `PartyMode.Role.Owner`
- `PartyMode.Role.DJ`
- `PartyMode.Role.Listener`
- `PartyMode.Endpoint.Title`
- `PartyMode.Endpoint.Assign`
- `PartyMode.Endpoint.Detach`
- `PartyMode.Endpoint.Stale`
- `PartyMode.Endpoint.Reassign`
- `PartyMode.Moderation.QueueLocked`
- `PartyMode.Moderation.LockQueue`
- `PartyMode.Moderation.UnlockQueue`
- `PartyMode.Moderation.Kick`
- `PartyMode.Moderation.Ban`
- `PartyMode.Errors.NotMember`
- `PartyMode.Errors.Forbidden`
- `PartyMode.Errors.Conflict`
- `Jukebox.Disabled`
- `Jukebox.NotConfigured`

---

## Phase 0 — Foundations (DB + Domain + Policies + Flags + Scaffolding)

### Objective
Introduce core persistence + domain model + policy scaffolding so later phases are pure feature work.

### Deliverables (exact)
1. **Config options**
   - Add options classes:
     - `PartyModeOptions { bool Enabled; int HeartbeatSeconds = 5; int EndpointStaleSeconds = 30; }`
     - `JukeboxOptions { bool Enabled; string? BackendType; }`
   - Bind from configuration (e.g., `appsettings.json`), default `Enabled=false` for Jukebox.

2. **Database entities + migrations**
   Create EF Core entities (align naming with Melodee conventions):
   - `PartySession` (ApiKey, Name, OwnerUserId, Status, JoinCodeHash?, ActiveEndpointId?, QueueRevision, PlaybackRevision, CreatedAt, UpdatedAt)
   - `PartySessionParticipant` (PartySessionId, UserId, Role, JoinedAt, LastSeenAt, IsBanned)
   - `PartyQueueItem` (ApiKey, PartySessionId, SongApiKey, EnqueuedByUserId, EnqueuedAt, SortOrder, Source?, Note?)
   - `PartyPlaybackState` (PartySessionId, CurrentQueueItemApiKey?, PositionSeconds, IsPlaying, Volume?, LastHeartbeatAt?, UpdatedByUserId?)
   - `Endpoint` (ApiKey, OwnerUserId?, Name, Type, CapabilitiesJson, LastSeenAt, IsShared, Room?)

   **Indexes (must)**
   - `PartyQueueItem(PartySessionId, SortOrder)`
   - `PartySessionParticipant(PartySessionId, UserId)` unique
   - `Endpoint(OwnerUserId, LastSeenAt)` where applicable

3. **Enums**
   - `PartySessionStatus { Active, Ended }`
   - `PartyRole { Owner, DJ, Listener }`
   - `EndpointType { WebPlayer, MpvBackend }` (expandable)

4. **Authorization policies**
   Create policies:
   - `PartyModeEnabled` (feature flag)
   - `PartySessionMember`
   - `PartySessionController` (Owner or DJ)
   - `PartySessionOwner` (Owner)

5. **Service layer interfaces**
   Add interfaces only (no heavy logic yet):
   - `IPartySessionService`
   - `IPartyQueueService`
   - `IPartyPlaybackService`
   - `IEndpointRegistryService`

6. **Localization**
   Add baseline keys needed for the first UI page stub:
   - `PartyMode.Title`
   - `Jukebox.Disabled`

### Acceptance criteria
- Migrations apply cleanly.
- App boots with Party Mode feature-flag wiring (even if UI is stub).
- Policies compile and can be referenced by controllers/components.
- Localization keys exist in all 9 resource files.

---

## Phase 1 — Party Sessions + Shared Queue + Web Player Endpoint MVP (Polling OK)

### Objective
End-to-end Party Mode with one endpoint: the **Blazor Web Player** attached to a party session.

### Deliverables (exact)

#### 1) API endpoints (native API is primary)
Implement these routes:

**Sessions**
- `POST   /api/v1/party-sessions`
  - body: `{ name, visibility, joinCode? }`
  - server sets Owner = current user, Status = Active
- `GET    /api/v1/party-sessions/{id}`
- `POST   /api/v1/party-sessions/{id}/join`
  - body: `{ joinCode? }`
  - default role = Listener
- `POST   /api/v1/party-sessions/{id}/leave`
- `POST   /api/v1/party-sessions/{id}/end`

**Queue**
- `GET    /api/v1/party-sessions/{id}/queue` → `{ revision, items[] }`
- `POST   /api/v1/party-sessions/{id}/queue/items`
  - body: `{ expectedRevision, songs:[songApiKey], albumApiKey?, playlistApiKey? }`
  - behavior:
    - if `albumApiKey` provided, expand to songs in album order
    - if `playlistApiKey` provided, expand in playlist order
    - if `songs` provided, add in given order
- `DELETE /api/v1/party-sessions/{id}/queue/items/{itemId}?expectedRevision=...`
- `POST   /api/v1/party-sessions/{id}/queue/reorder`
  - body: `{ expectedRevision, itemId, newIndex }`
- `POST   /api/v1/party-sessions/{id}/queue/clear`
  - body: `{ expectedRevision }`

**Playback intent**
- `GET    /api/v1/party-sessions/{id}/playback`
- `POST   /api/v1/party-sessions/{id}/playback/play`
- `POST   /api/v1/party-sessions/{id}/playback/pause`
- `POST   /api/v1/party-sessions/{id}/playback/skip`
- `POST   /api/v1/party-sessions/{id}/playback/seek`
  - body: `{ positionSeconds }`
- `POST   /api/v1/party-sessions/{id}/playback/volume`
  - body: `{ volume01 }` (0.0–1.0)
  - **MVP:** accept and store but only enforce if endpoint capability says `canSetVolume=true`.

**Endpoint heartbeat**
- `POST /api/v1/endpoints/{endpointId}/heartbeat`
  - body: `{ partySessionId, currentQueueItemId, positionSeconds, isPlaying, volume01? }`
  - update `PartyPlaybackState` + endpoint `LastSeenAt`

> **Decision removed:** Use `expectedRevision` everywhere and return `409` with current revision on mismatch.

#### 2) Core business logic (rules)
- **QueueRevision**
  - Start at 0.
  - Every successful queue mutation increments by 1.
- **Playback rules**
  - Controllers set intent (play/pause/skip/seek).
  - Endpoint sends authoritative position via heartbeat.
- **Auto-advance**
  - Implement auto-advance in the **Web Player**:
    - when track ends, call `skip` intent endpoint (or update state and request next item).
  - On skip:
    - set `CurrentQueueItem` to next queue item (or null if none)
    - set `PositionSeconds=0`

#### 3) Web UI (Blazor) — minimal but complete
Add a new navigation entry:
- “Party Mode” page:
  - Create session form
  - Join session form
  - Session detail:
    - now playing
    - queue list + add/remove/reorder
    - participant list (read-only in Phase 1)
  - Polling every 2–3 seconds is acceptable until Phase 2.

**Web player attachment**
- `/musicplayer` supports attaching to a session:
  - `?partySessionId=<id>` OR a UI picker
  - registers/uses an `EndpointId` stored in browser local storage
  - sends heartbeat every `PartyModeOptions.HeartbeatSeconds`

#### 4) Endpoint registration (MVP)
- Implement `POST /api/v1/endpoints/register`
  - body: `{ name, type="WebPlayer", capabilities:{...} }`
  - server returns `endpointId`
- Web player registers once per browser profile and reuses the ID.

**Capabilities for Web Player (set explicitly)**
- canPlay=true
- canPause=true
- canSkip=true
- canSeek=true
- canSetVolume=true (if you already support it)
- canReportPosition=true

#### 5) Localization updates
Add all strings needed for pages introduced in Phase 1 across **all 9 resource files**.

### Acceptance criteria
- Two different users can:
  - create/join the same session
  - add songs and see consistent queue order
- Web player attaches as endpoint, plays from shared queue, and heartbeats state.
- Controller actions (skip/seek/play/pause) take effect on the endpoint.
- Queue revision conflicts produce 409 and do not corrupt ordering.
- All UI strings are localized (keys in all 9 files).

---

## Phase 2 — Real-time + Moderation + Anti-abuse

### Objective
Make the experience “party-ready”: real-time updates + governance controls.

### Deliverables (exact)

#### 1) SignalR real-time layer
- Add a hub: `PartyHub`
- Groups: `party:{partySessionId}`
- Events:
  - `QueueChanged(revision, diff)`
  - `PlaybackChanged(playbackState)`
  - `ParticipantsChanged(participants)`
  - `SessionEnded()`

**Implementation rule**
- Emit events from services (queue/playback/participants) after DB commit.
- Client subscribes on session view and updates UI immediately.

#### 2) Moderation controls (Owner)
Add endpoints:
- `POST /api/v1/party-sessions/{id}/settings/queue-lock`
  - body: `{ isLocked }`
- `POST /api/v1/party-sessions/{id}/participants/{userId}/role`
  - body: `{ role }`
- `POST /api/v1/party-sessions/{id}/participants/{userId}/kick`
- `POST /api/v1/party-sessions/{id}/participants/{userId}/ban`
- `POST /api/v1/party-sessions/{id}/participants/{userId}/unban`

Rules:
- Kicked user can rejoin.
- Banned user cannot rejoin (403).

UI:
- Owner-only controls in participants list.
- Visual indicator when queue is locked.
- When locked: listeners cannot add/reorder/remove.

#### 3) Rate limiting (anti-abuse)
Use ASP.NET rate limiting middleware and define policies:
- `PartyQueueAddPolicy` (e.g., max 20/min per user per session)
- `PartyPlaybackControlPolicy` (e.g., max 30/min per user per session)
- `PartyVolumePolicy` (e.g., max 20/min per user per session)
- `SkipCooldown` enforced at service-level (e.g., 10 seconds between skips per session)

> **Decision removed:** implement both middleware rate limit and a service-level skip cooldown.

#### 4) Audit trail (persisted)
Create `PartyAuditEvent` table:
- ApiKey, PartySessionId, UserId, EventType, PayloadJson, CreatedAt

Record:
- queue add/remove/reorder/clear
- playback play/pause/skip/seek/volume
- moderation actions (role change/kick/ban/lock)

Expose owner-only:
- `GET /api/v1/party-sessions/{id}/audit?take=100`

#### 5) Localization updates
Add keys for moderation, locked messages, errors, and any new UI text.

### Acceptance criteria
- Two users see queue and now playing update instantly without polling.
- Skip spamming is blocked (cooldown + rate limiting).
- Owner can lock queue, kick, ban, and change roles; effects are immediate.
- Audit log shows actions with correct user attribution.
- Localization fully updated across 9 files.

---

## Phase 3 — Endpoint Registry + Capability Model (First-class)

### Objective
Treat endpoints as first-class, support switching endpoints, and drive UI controls by capabilities.

### Deliverables (exact)

#### 1) Endpoint registry APIs
- `GET /api/v1/endpoints` (returns endpoints visible to user)
- `POST /api/v1/endpoints/{endpointId}/attach`
  - body: `{ partySessionId }`
- `POST /api/v1/endpoints/{endpointId}/detach`

Rules:
- A session can have at most one active endpoint at a time.
- Attaching endpoint updates `PartySession.ActiveEndpointId`.

#### 2) Endpoint staleness detection
- Add job/timer in app (or on-demand calculation):
  - Endpoint is stale if `Now - LastSeenAt > PartyModeOptions.EndpointStaleSeconds`
- If stale:
  - mark session as “Endpoint offline” in UI
  - controllers can reassign endpoint

#### 3) Capability-driven UI
- In session UI, enable controls only when the active endpoint reports support.
- If no endpoint:
  - show banner: “No endpoint attached; attach a player to start playback.”

#### 4) Localization updates
Endpoint assign/reassign/offline banners and buttons.

### Acceptance criteria
- Owner can pick from a list of endpoints and switch active endpoint mid-session.
- UI reflects endpoint capabilities correctly.
- Endpoint staleness is detected and shown; reassignment works.
- Localization updated across 9 files.

---

## Phase 4 — MPV Backend

### Objective
Add a headless/server-side playback backend using MPV, behind explicit config.

### Deliverables (exact)

#### 1) Backend abstraction
Add interface:
- `IPlaybackBackend` (or reuse the earlier requirement name)
Methods:
- `Task<BackendCapabilities> GetCapabilitiesAsync()`
- `Task PlayAsync(QueueItem item, double startPositionSeconds = 0)`
- `Task PauseAsync()`
- `Task StopAsync()`
- `Task SeekAsync(double positionSeconds)`
- `Task SetVolumeAsync(double volume01)`
- `Task<BackendStatus> GetStatusAsync()`

#### 2) MPV backend implementation
Use MPV with IPC socket.
Config:
- `JukeboxOptions.Enabled`
- `JukeboxOptions.BackendType = "mpv"`
- `MpvOptions { string? MpvPath; string? AudioDevice; string? ExtraArgs; string? CmdTemplate; }`

Rules:
- Use a single MPV process per backend instance.
- If process dies, restart and surface a warning in session state.

#### 3) Wiring to Party sessions
When MPV backend is enabled:
- Create a system-owned endpoint of type `MpvBackend`
- It can be attached like any endpoint
- Its heartbeats come from backend status polling (server-side), not browser.

#### 4) Localization updates
Backend status messages and configuration-related UI text (if any).

### Acceptance criteria
- With backend enabled + attached:
  - play/pause/skip/seek work against MPV
  - now playing and position update correctly
- With backend disabled:
  - nothing changes from prior phases.

---

## Phase 5 — Subsonic/OpenSubsonic `jukeboxControl` (Backend-gated)

### Objective
Expose Subsonic jukebox control endpoint only when a jukebox backend is configured.

### Deliverables (exact)

#### 1) Default behavior
If `JukeboxOptions.Enabled` is false OR backend not configured:
- `/rest/jukeboxControl` (and `.view`) returns **410 Gone** with message key `Jukebox.NotConfigured`.

#### 2) Implement actions
Support (minimum viable):
- `get`, `status`, `set`
- `start`, `stop`
- `skip` (support `index` and `offset`)
- `add` (multiple `id`)
- `clear`, `remove`, `shuffle`
- `setGain` (map to volume01)

Mapping rules:
- Use a dedicated “Jukebox Session” per backend instance:
  - session owned by System
  - participants not needed; enforce auth via Subsonic user permission/policy
- Jukebox playlist maps to the session queue.

#### 3) Tests
- Add integration tests for:
  - 410 when disabled
  - add/start/skip flows when enabled

#### 4) Localization updates
Error and enablement messages if surfaced in UI.

### Acceptance criteria
- Disabled by default; returns 410.
- Enabled: Subsonic clients can control playback via jukeboxControl semantics.

---

## Phase 6 — Additional Backends

### Objective
- Implement **MPD backend** next (common in homelabs).

Deliverables:
- MPD backend implementing `IPlaybackBackend` via TCP control.
- Multi-instance config support (e.g., named devices/rooms).
- Endpoint list shows multiple backend endpoints.

Acceptance:
- Two named backends can be configured and selected per session.

---

## Template prompt for a coding agent (per phase)

Use this template verbatim; fill in `{PHASE_NUMBER}` and `{PHASE_NAME}` and paste the “Deliverables” section for that phase.

```.aiignore
You are a coding agent working in the Melodee repo.

Goal: Implement Phase 4 — MPV Backend:** backend abstraction + MPV backend, config + health reporting
- Melodee Jukebox / Party Mode — Phased Implementation Guide (/design/requirements/jukebox-implentation.md)
- Follow “Global implementation rules” exactly. Do not invent new designs.

Constraints:
- No time estimates.
- Minimize decision making: if a choice isn’t specified, choose the simplest approach and document it in code comments.
- Every new user-facing string MUST be added to localization using the `PartyMode.*` / `Jukebox.*` keys and propagated to ALL 9 language resource files.
- All new endpoints must follow existing `api/v1/...` conventions and existing error response patterns.
- All queue mutations MUST enforce `expectedRevision` optimistic concurrency and return 409 with the current revision on mismatch.

Work plan:
1) Implement ALL deliverables listed for Phase.
2) Add/update tests:
   - Unit tests for business rules introduced in this phase
   - Integration tests for API endpoints introduced in this phase (where applicable)
3) Update UI components introduced in this phase.
4) Update localization in all 9 resource files for every new UI string.
5) Run:
   - build
   - unit tests
   - any lint/format steps used in the repo
6) Provide a PR-style summary:
   - What changed (bullets)
   - Endpoints added/changed
   - DB migrations added (if any)
   - Localization keys added
   - How to manually verify

Deliverable: a clean commit series on the branch that compiles and passes tests.
```

---

## Prompt for a code review agent (after all phases complete)

Use this after Phase 1–5 are merged (and Phase 6 if implemented).

```aiignore
You are a strict code review agent for the Melodee repo.

Review scope:
- Party Mode / Jukebox implementation across Phases 0–5 (and Phase 6 if present).
- The requirements are defined in:
  1) Melodee Jukebox / Party Mode — Comprehensive Requirements & Implementation Phases
  2) Melodee Jukebox / Party Mode — Phased Implementation Guide

Review objectives:
1) Requirements compliance
   - Shared session + shared queue works end-to-end
   - Web player can attach as endpoint and heartbeat playback state
   - Role-based permissions enforced everywhere
   - Queue optimistic concurrency implemented with expectedRevision and correct 409 behavior
   - Real-time updates (SignalR) are correct and don’t leak data across sessions
   - Moderation and rate-limiting controls are implemented and effective
   - JukeboxControl returns 410 when disabled; works when enabled and configured

2) Architecture & maintainability
   - Clear separation between core Party Mode and Jukebox Backends
   - Service layer boundaries make sense; no business logic in controllers/components
   - Capability model is consistent and used by UI
   - Error handling is consistent with the rest of Melodee

3) Security & privacy
   - No IDOR: every endpoint verifies session membership/role
   - Join code/PIN stored hashed
   - Rate limiting cannot be bypassed
   - Audit log does not store sensitive tokens

4) Localization
   - Every new UI string is localized
   - All 9 language resource files contain the new keys (no missing keys)
   - Keys follow PartyMode.* / Jukebox.* convention

5) Testing
   - Meaningful unit and integration coverage for queue concurrency, roles, and playback control
   - At least one end-to-end smoke path is testable and documented

Output format:
- Start with a checklist of pass/fail by category.
- Then list prioritized issues (P0/P1/P2) with:
  - file/line references
  - risk
  - concrete fix suggestion
- End with “ready to merge” or “not ready” and the minimum changes needed.
```

