## ADR-0001: Do not implement OpenSubsonic/Subsonic Jukebox Control

- Date: 2025-12-13T16:17:46.094Z
- Status: Superseded (see ADR-0007)

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

