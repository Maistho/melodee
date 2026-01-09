## ADR-0007: Party mode (shared sessions) is the primary "jukebox" feature; server-side playback is optional

- Date: 2026-01-09T00:00:00.000Z
- Status: Accepted

### Context

Melodee users commonly ask for “jukebox” / “party mode”: a shared queue that multiple users can control, with music playing on a designated endpoint (e.g., a browser tab on a TV).

The Subsonic/OpenSubsonic `jukeboxControl` endpoint implies server-side playback and is often incompatible with headless/container deployments unless explicitly configured.

### Decision

- The core feature is **Party Mode**: shared sessions + shared queue + real-time updates, with playback happening on a designated endpoint (initially the existing Blazor `/musicplayer`).
- OpenSubsonic/Subsonic `jukeboxControl` remains **disabled by default** (HTTP `410 Gone`).
- If a deployment explicitly configures a jukebox backend (Snapcast/MPD/etc.), Melodee may enable `jukeboxControl` semantics scoped to that backend.

### Rationale

- Delivers the competitive “jukebox” experience without forcing server-side audio output in the default product.
- Keeps behavior explicit and safe for self-hosted deployments.

### Consequences

- Party mode requires a first-class data model (sessions, participants, queue, playback state) and a real-time update mechanism (SignalR preferred).
- `jukeboxControl` support becomes a compatibility layer, not the primary contract.

### References

- `design/requirements/jukebox-requirements.md`
