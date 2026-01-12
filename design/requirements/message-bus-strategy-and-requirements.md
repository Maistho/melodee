# Message Bus Strategy & Requirements (Wolverine + RabbitMQ + PostgreSQL)

## Purpose

This document defines the **standard messaging strategy** for this solution and the **requirements** for implementing robust, durable, event-driven messaging across the Melodee ecosystem.

**Decision (locked):** Standardize on **Wolverine + RabbitMQ + Durable Inbox/Outbox backed by PostgreSQL**.

This is a MIT FOSS project; therefore **commercial** messaging platforms are **out of scope**.

---

## Current State Summary (Why change)

Today the solution uses **Rebus** with **in-memory transport** in the Blazor host and the CLI. This makes messaging **non-durable** and **single-process**. It is also used primarily for operational background work rather than durable integration events.

Key issues:
- In-memory transport means messages do **not survive restarts** and do not cross process boundaries.
- Usage is mostly `SendLocal` (point-to-local), so there is **no fan-out pub/sub**.
- Some handlers invoke other handlers **directly** (bypassing the bus), so the bus is not the real execution boundary.
- The CLI registers a bus but does **not** host workers/handlers, so messages are typically **never handled** in CLI-only flows.
- There is no durable outbox for CRUD changes, and limited observability (DLQ, retries, dashboards).

---

## Target Architecture (High Level)

### Core properties we must achieve
1. **Durable delivery**: events and commands survive process restarts and temporary broker outages.
2. **Consistent publish** relative to database state: CRUD integration events are published **only after** successful DB commit.
3. **At-least-once** delivery with **idempotent** consumers to achieve “effectively once” outcomes.
4. **Multiple consumers**: events can be independently consumed by multiple services/components without tight coupling.
5. **Operationally safe**: observability, DLQ, and replay processes exist and are documented.

### Components
- **RabbitMQ**: external broker for distributed messaging between processes/services.
- **PostgreSQL**: durability store for Wolverine’s transactional **inbox/outbox** (and any saga/state storage needed later).
- **Wolverine**: message bus framework providing:
  - local + distributed messaging
  - durable inbox/outbox (store-and-forward)
  - retries and error handling patterns
  - transport integration with RabbitMQ

---

## Message Taxonomy (Required)

We standardize message intent and routing using these definitions:

### Commands
A **Command** requests an action: “Do X”.
- Exactly one owning handler (one bounded context / service).
- **Point-to-point** delivery.
- Examples:
  - `RescanArtistDirectory`
  - `RescanAlbumDirectory`
  - `ReprocessAlbumMetadata`
  - `AddArtistRequest` (workflow request)

### Events
An **Event** is a fact: “X happened”.
- Zero or many subscribers.
- **Publish/subscribe** fan-out.
- Examples:
  - `ArtistCreated`, `ArtistUpdated`, `ArtistDeleted`
  - `AlbumUpdated`
  - `UserCreated`
  - `ArtistAddRequestCompleted` (workflow completion event)

### Domain vs Integration events
- **Domain Events**: internal to the owning bounded context (may be in-process or bus).
- **Integration Events**: durable contracts intended for cross-service ecosystem use. These are the “backbone.”

**Requirement:** All CRUD notifications that other services may depend on are **Integration Events** and must follow contract/versioning rules.

---

## Event Contracts & Versioning

### Contract stability requirements (Integration Events)
Integration Events are treated like public APIs:
- **Additive changes only** in existing schema versions (add fields; do not rename/remove).
- Breaking changes require a **new event type** (e.g., `AlbumUpdatedV2`) OR a new `SchemaVersion` with parallel publishing during transition.
- Do not publish EF models or persistence entities as payloads.

### Base message envelope metadata (Required)
Every message (command or event) MUST contain:

- `MessageId` (UUID)
- `OccurredAtUtc` (UTC timestamp)
- `MessageType` (string; stable)
- `SchemaVersion` (int; starts at 1)
- `SourceApp` (e.g., `Melodee.Blazor`, `Melodee.Cli`, `Melodee.Worker`)
- `CorrelationId` (UUID/string; propagated across calls)
- `CausationId` (UUID/string; parent message id)
- `ActorUserId` (optional)
- `TenantId` / `LibraryId` (optional; reserved for future)

### CRUD integration event payload rule
By default: **ID + minimal change metadata** is sufficient.
- Always include: `EntityType`, `EntityId`, `Operation` (`Created|Updated|Deleted`)
- Include optional `ChangedFields` only when you can do it reliably (otherwise omit).
- Snapshots are **not required** (unless later decided for specific events).

---

## Delivery Semantics (Reliability)

### Standard semantic model
- **At-least-once delivery** is expected across the bus.
- Consumers MUST be **idempotent**.

### Idempotency requirements
- All handlers must tolerate duplicate delivery of the same message.
- Wolverine durable inbox provides built-in deduplication windows, but handlers must still be safe:
  - Upserts rather than inserts where possible
  - Unique constraints for “apply once” side effects
  - Use `MessageId` / `CorrelationId` stored in a consumer-side table for additional dedupe where needed

### Ordering expectations
- Global ordering is not required.
- Per-entity ordering is not guaranteed unless explicitly implemented.
- Eventual consistency is acceptable.
- If a workflow requires ordering, design it as:
  - a single command handler owning the process, or
  - a saga / stateful workflow (future)

---

## Transactional Outbox/Inbox (PostgreSQL)

### Requirement: transactional publish for CRUD events
All CRUD integration events MUST be published via a transactional outbox mechanism such that:
- The DB transaction commits first.
- Events are then delivered from outbox to RabbitMQ.
- If RabbitMQ is unavailable, events remain in outbox and are retried until delivered.

### EF Core transaction integration
- The primary app persistence uses EF Core.
- Wolverine MUST be configured to use EF Core transactions so that:
  - outgoing messages are persisted in the same transaction
  - incoming message handling can participate in a DB transaction where appropriate

### Durable tables management
- Wolverine durable message tables in PostgreSQL must be created and migrated as part of deployment.
- Retention policies must be configured for:
  - processed inbox records (for dedupe window)
  - delivered outbox records
  - dead-letter/error records

---

## RabbitMQ Requirements

### Environments
- Development: RabbitMQ may be started via Docker/Compose.
- Production: RabbitMQ may be containerized **or** hosted externally.
- The application MUST be configurable for both.

### Connection + authentication
- Use per-environment configuration:
  - `RABBITMQ__HOST`, `RABBITMQ__PORT`, `RABBITMQ__VHOST`
  - `RABBITMQ__USERNAME`, `RABBITMQ__PASSWORD` (secrets)
  - `RABBITMQ__TLS__ENABLED`, `RABBITMQ__TLS__CA`, `RABBITMQ__TLS__CERT`, `RABBITMQ__TLS__KEY` as needed
- Never hardcode secrets; support env vars and appsettings overrides.

### Topology & naming conventions (Standard)
We standardize topology for clarity and interoperability:

**Exchanges**
- `melodee.events` (topic exchange) — integration events
- `melodee.commands` (direct or topic exchange) — commands (optional; point-to-point queues may be declared directly)

**Routing keys**
- Events: `{domain}.{entity}.{verb}`  
  Examples:
  - `library.album.updated`
  - `library.artist.deleted`
  - `identity.user.created`
  - `requests.artist-add.completed`
- Commands: `{service}.{command}`  
  Examples:
  - `library.rescan-artist-directory`
  - `library.rescan-album-directory`

**Queues**
- Each service owns its own command queue:
  - `svc.<service>.commands` (e.g., `svc.library.commands`)
- Each subscriber owns its own event queue:
  - `svc.<service>.events` (e.g., `svc.search.events`, `svc.analytics.events`)

### Durability
- Exchanges and queues MUST be durable.
- Messages MUST be persistent.
- Prefer quorum queues for high reliability where available.

### Retries, DLQ, poison messages
- Retry policy is implemented in Wolverine (application-level), but RabbitMQ DLQ is still required:
  - Dead-letter exchange: `melodee.dlx`
  - Dead-letter queue: `melodee.dlq` (or per-service DLQ)
- All poison messages MUST be moved to DLQ after configured retry attempts.
- A documented replay mechanism is required (move from DLQ back to live queue after remediation).

---

## Hosting Model (Docker and Non-Docker)

### General rule
Messaging and durability configuration must not assume Docker.
- Docker is an **option** for local/dev and some prod installs.
- The same settings must work when running as:
  - a container
  - a systemd service
  - a manually started process

### Required application behaviors
- On startup:
  - validate connectivity to PostgreSQL and RabbitMQ
  - validate required exchanges/queues (or auto-provision using Wolverine’s resource management approach)
- Provide health checks:
  - liveness: process up
  - readiness: can connect to PostgreSQL and RabbitMQ; durability agent is running
- Provide graceful shutdown:
  - stop accepting work
  - drain in-flight handlers up to a timeout
  - flush/persist outgoing messages

---

## CLI Participation

**Decision:** The CLI MUST publish CRUD integration events when it performs data changes.

Rules:
- The CLI should generally act as a **producer** of CRUD integration events via the same transactional outbox rules.
- The CLI SHOULD NOT run long-lived consumers by default (unless a specific “worker mode” is implemented).
- Any CLI command that performs DB writes must ensure:
  - it uses the same EF Core DbContext configuration
  - the Wolverine outbox is enabled and messages are persisted before commit completes

---

## Observability & Operations

### Logging
- Log each message handling attempt with:
  - `MessageId`, `MessageType`, `CorrelationId`, `CausationId`
  - success/failure
  - duration
- Logs MUST be structured.

### Metrics
Expose metrics for:
- queue backlog (where possible)
- outbox pending count
- inbox retention counts
- handler success/failure/retry counts
- DLQ size

### Tracing
Use OpenTelemetry tracing and propagate correlation ids.

### Operational runbook (Required)
Document:
- how to inspect queues
- how to view DLQ
- how to replay messages
- how to purge poison messages safely
- how to rebuild projections/caches if consumers fall behind

---

## Migration Strategy (from Rebus to Wolverine)

This is a controlled migration with these non-negotiables:
- No in-memory transport remains in production paths.
- Existing operational tasks (rescan/reprocess) become explicit **commands** handled via Wolverine.
- CRUD integration events are introduced with outbox publish semantics.
- Direct handler-to-handler calls are removed; the bus becomes the boundary.

---

## Acceptance Criteria

The messaging system is considered complete when:
1. RabbitMQ is used for distributed messaging in the Blazor host.
2. PostgreSQL-backed durability is enabled (transactional outbox/inbox).
3. CRUD integration events are emitted for core entities with standardized metadata.
4. At least one additional consumer (e.g., search indexing or caching) receives events via pub/sub.
5. DLQ exists and a replay process is documented and tested.
6. CLI publishes events via outbox for DB changes.
7. In-memory Rebus transport and Rebus handlers are removed or isolated to dev-only experiments (prefer removed).

---

## Appendix: Core entities that MUST emit CRUD events

At minimum:
- Artist
- Album
- Song
- User
- Playlist
- PlaylistSong / UserSong / other join entities that materially affect behavior

Optional (future):
- Scrobble/Stream events (if they become part of analytics stream)
- Search history events (if retained as analytics; do not ship EF model payloads)
