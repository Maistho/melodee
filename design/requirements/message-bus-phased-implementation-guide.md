# Phased Implementation Guide (Wolverine + RabbitMQ + PostgreSQL Durability)

This guide breaks implementation into discrete phases meant for coding agents. Each phase has explicit deliverables and “definition of done” to minimize decision-making during implementation.

> Principle: **Make the bus the boundary**. No “direct call” shortcuts. All cross-component background work goes through commands/events.

---

## Phase Map

- [ ] Phase 0 — Repository prep & baseline tests
- [ ] Phase 1 — Add Wolverine + RabbitMQ transport (Blazor host)
- [ ] Phase 2 — Add PostgreSQL durability (Inbox/Outbox) + EF Core transactional integration
- [ ] Phase 3 — Define message contracts + conventions (commands/events, metadata, routing keys)
- [ ] Phase 4 — Implement CRUD integration events (outbox-backed)
- [ ] Phase 5 — Migrate existing Rebus background flows to Wolverine commands
- [ ] Phase 6 — Add first real subscribers + DLQ/replay workflow
- [ ] Phase 7 — Update CLI to publish through outbox
- [ ] Phase 8 — Observability/ops hardening
- [ ] Phase 9 — Remove Rebus and clean up

---

## Phase 0 — Repository prep & baseline tests

### Deliverables
- A solution-wide “messaging” folder/namespace decision:
  - `Melodee.Messaging` (recommended) OR `Melodee.Common.Messaging`
- Baseline unit/integration test projects compile and run.
- Add a `docs/messaging/` folder and place the strategy document there.

### Definition of done
- `dotnet test` passes.
- No functional changes yet.

---

## Phase 1 — Add Wolverine + RabbitMQ transport (Blazor host)

### Deliverables
- Add Wolverine packages and wire into `Melodee.Blazor` hosting.
- Configure RabbitMQ connection from configuration (appsettings + env vars).
- Create required endpoints:
  - command endpoint for `svc.library.commands` (or equivalent)
  - event subscription endpoint for `svc.<service>.events`
- Create health checks for RabbitMQ connectivity.

### Definition of done
- Blazor app starts with Wolverine enabled.
- A smoke-test message can be sent and handled within the same process using RabbitMQ (loopback endpoint or local command).

---

## Phase 2 — Add PostgreSQL durability (Inbox/Outbox) + EF Core transactional integration

### Deliverables
- Enable Wolverine durable message persistence in PostgreSQL.
- Enable EF Core transaction integration so outbox writes are part of the same DB transaction.
- Ensure durable tables are created/migrated as part of startup or deployment.

### Definition of done
- When RabbitMQ is unavailable, outgoing messages are persisted and later delivered when RabbitMQ returns.
- Durable inbox is enabled (dedupe window configured).
- Integration tests demonstrate persistence across process restart.

---

## Phase 3 — Define message contracts + conventions (commands/events, metadata, routing)

### Deliverables
- Add a common envelope/base types:
  - `MessageEnvelope` or base interface/record containing required metadata
- Add conventions:
  - Routing keys for events: `{domain}.{entity}.{verb}`
  - Queue naming: `svc.<service>.commands`, `svc.<service>.events`
  - Exchange names: `melodee.events`, `melodee.commands`, `melodee.dlx`
- Add serialization settings:
  - stable JSON serialization (camelCase)
  - explicit schema version field

### Definition of done
- A sample command and event compile, serialize, and route correctly.
- Metadata is present on every message.

---

## Phase 4 — Implement CRUD integration events (outbox-backed)

### Deliverables
- Implement EF Core change capture for core entities:
  - Artist, Album, Song, User, Playlist (+ key join entities)
- Implement publishing strategy:
  - Create outbox entries during the same DB transaction as the entity change
  - Publish only after commit (Wolverine outbox)
- Ensure “ID + change metadata” payload rule is followed (no EF models in payload).

### Definition of done
- Create/update/delete for at least 2 entities emits integration events.
- Events are delivered to RabbitMQ and can be consumed by a subscriber queue.
- A test proves that DB commit without publish does not occur, and publish without commit does not occur.

---

## Phase 5 — Migrate existing Rebus background flows to Wolverine commands

### Deliverables
- Convert operational messages into Commands:
  - `RescanArtistDirectory`
  - `RescanAlbumDirectory`
  - `ReprocessAlbumMetadata`
- Replace direct handler-to-handler calls with bus calls.
- Ensure long-running work uses Wolverine handler patterns and preserves correlation ids.

### Definition of done
- Existing rescan/reprocess flows function using Wolverine (no Rebus).
- No direct invocation between handlers for these flows.

---

## Phase 6 — Add first real subscribers + DLQ/replay workflow

### Deliverables
- Implement at least one subscriber as a separate logical endpoint:
  - e.g., Search index updater subscribing to `AlbumUpdated`, `ArtistDeleted`
- Configure error handling:
  - retries with backoff
  - poison message routing to DLQ after max retries
- Document replay workflow and add a CLI/admin command for replay (optional but recommended).

### Definition of done
- Subscriber receives events via pub/sub.
- A forced failure results in retries, then DLQ.
- Replay moves message from DLQ and reprocesses successfully after fix.

---

## Phase 7 — Update CLI to publish through outbox

### Deliverables
- Replace Rebus in CLI with Wolverine configuration (producer-first).
- Ensure CLI DbContext and Wolverine durability share the same PostgreSQL settings.
- Ensure CLI commands that change data emit CRUD events via outbox.

### Definition of done
- A CLI command that changes Album/Artist data triggers an integration event that a subscriber can observe.

---

## Phase 8 — Observability/ops hardening

### Deliverables
- OpenTelemetry tracing with correlation propagation.
- Metrics (outbox pending, handler duration, retries, DLQ counts).
- Health endpoints:
  - PostgreSQL durable store readiness
  - RabbitMQ readiness
- Document operational runbook in `docs/messaging/runbook.md`.

### Definition of done
- Ops runbook exists and is accurate.
- Dashboards/metrics are available in dev.

---

## Phase 9 — Remove Rebus and clean up

### Deliverables
- Remove Rebus packages and configuration from Blazor and CLI.
- Delete old event/handler folders or migrate relevant concepts into the new messaging structure.
- Update docs to remove Rebus references.

### Definition of done
- Solution builds and tests pass.
- No Rebus usage remains in production code paths.

---

## Template prompt for a coding agent (per phase)

```text
You are a coding agent working in this repository.

Goal:
Implement Phase <N> from docs/messaging/message-bus-phased-implementation-guide.md.

Constraints:
- Do not make architectural decisions outside the phase.
- Follow docs/messaging/message-bus-strategy-and-requirements.md exactly.
- Keep changes minimal and incremental.
- Add/modify tests to prove the phase is complete.
- Ensure dotnet format (if configured), build, and tests succeed.

Deliverables:
- Code changes implementing Phase <N>
- Any new docs requested by the phase
- Tests validating behavior

Definition of done:
The phase’s “Definition of done” criteria are met, and you can explain how to verify them with commands.
```

## Template prompt for a code review agent (after all phases)

```text
You are a senior code review agent.

Review the repository for the completed implementation of Wolverine + RabbitMQ + PostgreSQL durability messaging.

Check:
- Messaging strategy adherence (commands vs events, integration events, metadata, routing keys)
- Outbox/inbox correctness (commit/publish coupling)
- Idempotency and handler safety
- Retry/DLQ behavior and replay process
- CLI publishing via outbox
- Observability (logs/metrics/tracing/health checks)
- Removal of Rebus and dead code
- Tests: coverage of durability and failure modes

Output:
- A prioritized list of issues (blockers first)
- Specific file/line references and suggested fixes
- Any missing docs or runbook gaps
```
