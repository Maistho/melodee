# Message Bus Requirements and Evaluation

## Scope and goals

This document reviews how events are handled today across the solution (Melodee.Blazor and Melodee.Cli), identifies gaps relative to a robust CRUD event system, and evaluates external message bus options. The target state is a durable, external event system that emits events for core data changes (Artist, Album, Song, User, Playlist, and similar entities) while supporting multiple consumers.

Key goals:
- Durable events that survive process restarts and scale across multiple app instances.
- Publish events on core CRUD operations with consistent metadata.
- Support multiple consumers without tight coupling between producer and handler.
- Ensure consistent delivery relative to database changes (outbox/transactional boundaries).

## Current state review

### Rebus usage in Melodee.Blazor

Message bus configuration is in `src/Melodee.Blazor/Program.cs` and uses in-memory transport:
- Rebus transport: `UseInMemoryTransport(new InMemNetwork(), "melodee_bus")`.
- In-memory saga and timeout storage.
- Compression enabled, two workers, max parallelism 20.
- Handlers registered via `AddRebusHandler<>`.

Handlers in `src/Melodee.Common/MessageBus/EventHandlers`:
- `AlbumAddEventHandler` (long-running, writes album + song records based on file system state).
- `AlbumRescanEventHandler` (syncs album songs and metadata against files).
- `ArtistRescanEventHandler` (rescans album directories, calls album handlers directly).
- `MelodeeAlbumReprocessEventHandler` (re-processes album metadata directories).
- `SearchHistoryEventHandler` (persists search history to DB).
- `UserLoginEventHandler` (updates last login).
- `UserStreamEventHandler` (logs stream/download activity).

Events are defined in `src/Melodee.Common/MessageBus/Events`:
- `AlbumAddEvent`, `AlbumRescanEvent`, `ArtistRescanEvent`, `MelodeeAlbumReprocessEvent`.
- `SearchHistoryEvent` (inherits the `SearchHistory` EF model).
- `UserLoginEvent`, `UserStreamEvent`.
- `LibraryUpdatedEvent` (defined but unused).

Event producers are typically `IBus.SendLocal(...)` calls:
- `ArtistService.RescanAsync` sends `ArtistRescanEvent`.
- `AlbumService.RescanAsync` sends `AlbumRescanEvent`.
- `SearchService` sends `SearchHistoryEvent`.
- `UserService` sends `UserLoginEvent` for login/validation.
- `OpenSubsonicApiService` sends `UserStreamEvent` for scrobbling/logging.
- `LibraryInsertJob` sends `MelodeeAlbumReprocessEvent` when metadata validation fails.

Important behavioral details:
- The system uses `SendLocal` (point-to-local queue), not `Publish`, so there is no pub/sub fan-out.
- `ArtistRescanEventHandler` directly invokes `AlbumRescanEventHandler` and `AlbumAddEventHandler` without going through the bus. The bus is therefore not consistently the execution boundary for these operations.
- Event payloads are primarily operational commands (rescan/reprocess) rather than integration events. This is closer to command processing than data change broadcasting.
- Storage for messages is in-memory, so messages do not survive restarts and do not cross process boundaries.

### Rebus usage in Melodee.Cli

Rebus is registered in `src/Melodee.Cli/Command/CommandBase.cs` with in-memory transport:
- `services.AddRebus(configure => configure.Transport(t => t.UseInMemoryTransport(...)))`.
- There are no handler registrations via `AddRebusHandler` in CLI.
- The CLI builds its own `ServiceProvider` directly and does not run a host; no hosted Rebus worker service is started.

Practical effect:
- Rebus in CLI currently provides an `IBus`, but no handlers are registered to receive messages, and there is no hosted worker. In practice, `SendLocal` calls in CLI-only flows will not be handled unless handlers are wired up manually.
- `LibraryScanCommand` passes `IBus` into `LibraryInsertJob`, which issues `MelodeeAlbumReprocessEvent`. In the CLI context this does not result in any processing because no handlers are registered.

### Other event mechanisms

The solution also uses two non-Rebus event mechanisms:

1) SignalR notifications for Party Mode in `src/Melodee.Blazor/Hubs`:
- `PartyHub` and `PartyNotificationService` emit `QueueChanged`, `PlaybackChanged`, and `ParticipantsChanged` events to connected clients.
- These are real-time UI notifications rather than durable system events.

2) Local .NET events/delegates used for progress reporting in CLI:
- `LibraryMoveOkCommand`, `LibraryRebuildCommand`, `LibraryCleanCommand`, and `ProcessInboundCommand` subscribe to events like `OnProcessingProgressEvent`, `OnProcessingStart`, and `OnDirectoryProcessed` from service classes.
- These events are in-process only and support user-facing progress output, not system integration.

## Event flow summary (today)

- User login: `UserService` -> `UserLoginEvent` -> `UserLoginEventHandler` -> `UpdateLastLogin`.
- User stream: `OpenSubsonicApiService` -> `UserStreamEvent` -> `UserStreamEventHandler` logs.
- Search: `SearchService` -> `SearchHistoryEvent` -> `SearchHistoryEventHandler` persists to DB.
- Rescans: `ArtistService` -> `ArtistRescanEventHandler` -> direct `AlbumRescanEventHandler`/`AlbumAddEventHandler`.
- Metadata reprocess: `LibraryInsertJob` -> `MelodeeAlbumReprocessEventHandler`.

These are operational background tasks, not robust CRUD event notifications.

## Gaps relative to target CRUD event system

- No external queue or durable message storage. In-memory transport means events are lost on restart and never leave the process.
- No publish/subscribe semantics. Everything is `SendLocal`, so external consumers cannot receive events.
- No event emission for core CRUD operations (Artist, Album, Song, User, Playlist, etc.).
- Lack of transaction coupling: events are emitted outside of a DB outbox pattern, so failures can produce inconsistent state.
- Some event payloads are tightly coupled to EF models (e.g., `SearchHistoryEvent`). This makes versioning and cross-service consumption harder.
- CLI does not currently participate in the Rebus event flow; it registers no handlers and does not start a bus worker.
- Limited observability (no dead-letter queue, retry policies, or broker-level monitoring).

## Requirements for the desired event system

### Functional requirements

- Emit events for CRUD operations on core entities:
  - Artist, Album, Song, User, Playlist, and related entities (UserSong, PlaylistSong, etc.).
- Include consistent metadata on each event:
  - EventId, Timestamp (UTC), EntityType, EntityId, EntityApiKey, Operation (Created/Updated/Deleted), SourceApp, ActorUserId/ApiKey (if applicable), CorrelationId, CausationId, SchemaVersion.
- Support multiple subscribers (internal services, caches, analytics, integrations).
- Provide at-least-once delivery and allow retry with backoff and dead-lettering.
- Ensure idempotent consumers (deduplicate using EventId + entity key).
- Permit multi-tenant or multi-library routing if needed (future growth).

### Non-functional requirements

- Durable storage (messages survive process restarts and broker outages).
- Scale across multiple application instances.
- Operational safety: monitoring, error queue, and traceability.
- Security: broker authentication, TLS in production, secrets managed via config.
- Event contract versioning strategy for backward compatibility.

## Migration requirements from Rebus in-memory to external bus

At a minimum, moving from in-memory Rebus to an external queue will require:

1) Transport replacement
- Replace `UseInMemoryTransport(...)` with a real transport (RabbitMQ, etc.) in `src/Melodee.Blazor/Program.cs`.
- Align queue naming, error queue, and retry policies.

2) Subscription storage
- Rebus publish/subscribe requires centralized subscription storage. Use `Rebus.PostgreSql` or another shared store.

3) Outbox or transactional publishing
- Introduce an outbox table and a publisher to ensure events are emitted only after DB commits.
- Emit CRUD events in SaveChanges interceptor or service-level transaction wrapper.

4) CLI participation
- Decide if CLI should publish or consume events. If yes, it must start the bus and register handlers explicitly.

5) Event contract redesign
- Separate internal commands (e.g., rescan/reprocess) from integration events (CRUD change events).
- Avoid coupling to EF models in event contracts.

## Options evaluation

### Option A: Keep Rebus, switch to RabbitMQ transport

Pros:
- Lowest migration cost; handlers and bus usage already Rebus based.
- Rebus has mature RabbitMQ support.
- Allows incremental refactor (start with transport change, add outbox next).

Cons:
- Still need to add outbox and subscription storage.
- Some event definitions are not ideal for external consumption.

### Option B: Rebus + RabbitMQ + PostgreSQL outbox and subscription storage

Pros:
- Durable transport + durable subscriptions.
- Fits current PostgreSQL usage; can reuse existing DB for outbox and subscriptions.
- Incremental migration with minimal handler changes.

Cons:
- Requires outbox implementation and background publisher.

### Option C: MassTransit + RabbitMQ

Pros:
- Very mature; built-in outbox support and strong RabbitMQ integration.
- Clear separation between commands and events.

Cons:
- Requires rewriting handlers and bus configuration.
- Larger code churn than keeping Rebus.

### Option D: CAP (with RabbitMQ or Kafka)

Pros:
- Strong outbox semantics with EF Core.
- Good fit for CRUD change events.

Cons:
- Less suitable if you want Rebus-style command processing.
- Requires integration changes and new hosting configuration.

### Option E: MQTT

Pros:
- Good for real-time notifications and IoT-style clients.
- Simple broker setup (e.g., Mosquitto, EMQX).

Cons:
- Lacks strong competing-consumer queue semantics.
- Weak fit for reliable CRUD change events and job orchestration.
- No direct Rebus or MassTransit integration.

### Option F: BlazingMQ

Pros:
- High performance, modern broker.

Cons:
- Limited .NET ecosystem and tooling.
- Higher integration cost and operational risk for this stack.

### Option G: NATS JetStream

Pros:
- Fast, simple; solid .NET support.
- Supports durable streams and consumers.

Cons:
- Additional infrastructure and new integration layer.
- Not currently present in the solution.

## Recommendation

Primary recommendation: keep Rebus and move to RabbitMQ with a durable outbox and shared subscription storage.

Rationale:
- Rebus is already in use; switching transport is the lowest-risk move.
- RabbitMQ has robust tooling, good visibility, and strong .NET client support.
- Rebus + RabbitMQ allows you to keep internal command handlers while introducing a clear event stream for CRUD changes.

A practical phased plan:
1) Replace in-memory transport with RabbitMQ in `Melodee.Blazor` and optionally in `Melodee.Cli`.
2) Add PostgreSQL-backed subscription storage and error queue configuration.
3) Implement an outbox and event dispatcher for CRUD events.
4) Introduce a consistent event contract for entity changes.
5) Add consumers for internal needs (cache invalidation, search index updates, auditing, analytics).

If a larger refactor is acceptable, MassTransit + RabbitMQ is a strong alternative, but it is not required to meet the goal.

## CRUD event design (proposed)

A minimal, durable contract:
- `EntityChangedEvent` (or per-entity events like `ArtistCreated`, `ArtistUpdated`, etc.).
- Required fields:
  - `EventId`, `OccurredAtUtc`, `EntityType`, `EntityId`, `EntityApiKey`, `Operation`, `Source`, `ActorUserId`, `CorrelationId`, `SchemaVersion`.
- Optional fields:
  - `ChangedFields`, `EntitySnapshot` (if snapshots are required), `LibraryId`.

Publishing strategy:
- Capture changes in EF Core via `SaveChanges` interceptor or unit-of-work wrapper.
- Store events in an outbox table as part of the same DB transaction.
- A background publisher reads outbox entries and publishes to the external bus.

Consumer strategy:
- Handlers must be idempotent (use `EventId` and entity keys to deduplicate).
- Use dead-letter queue for failures and maintain retry policies.

## Open questions

- Should CLI publish CRUD events or remain a local tool? If it modifies data, it should publish through the same outbox.
- ANSWER: Publish CRUD events.
- Do consumers need full entity snapshots, or are ID + change metadata sufficient?
- ANSWER: ID + change metadata is sufficient.
- Is per-entity ordering required, or is eventual consistency acceptable?
- ANSWER: Eventual consistency is acceptable.
- Are there external integrations planned that require additional PII or redaction rules?
- ANSWER: There is no PII or redaction required.

