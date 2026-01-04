# System Codebase Review Log

This document serves as a living record of codebase reviews performed by various coding agents. It tracks findings, risks, and actionable items to improve the Melodee solution.

## 1. Summary of Findings

| ID | Date | Agent | Category | Risk | Status | Description |
|:---|:-----|:------|:---------|:-----|:-------|:------------|
| 001 | 2026-01-04 | Antigravity | Architecture | High | Open | Refactor `OpenSubsonicApiService` (God Class) |
| 002 | 2026-01-04 | Antigravity | Quality | Medium | Open | Extract Manual Object Mapping in Jellyfin Controllers |
| 003 | 2026-01-04 | Antigravity | Testing | Medium | Open | Increase Unit/Integration Test Coverage for Jellyfin |
| 004 | 2026-01-04 | Antigravity | Architecture | Medium | Open | Standardize API Patterns (Service vs Controller) |
| 005 | 2026-01-04 | Antigravity | Security | High | Open | JWT Secret Key Validation and Security Hardening |
| 006 | 2026-01-04 | Antigravity | Security | Medium | Open | Inconsistent Authentication Across APIs |
| 007 | 2026-01-04 | Antigravity | Quality | Medium | Open | Missing Input Validation and Error Handling |
| 008 | 2026-01-04 | Antigravity | Performance | Medium | Open | Database Connection Pool Configuration |
| 009 | 2026-01-04 | Antigravity | Testing | High | Open | Missing OpenSubsonic Controller Tests |
| 010 | 2026-01-04 | Antigravity | Architecture | Low | Open | CORS Configuration Too Permissive |
| 011 | 2026-01-04 | Antigravity | Quality | Low | Open | API Documentation and OpenAPI Specification |
| 012 | 2026-01-04 | Antigravity | Performance | Medium | Open | Jellyfin InstantMix/Similar Items Performance |
| 013 | 2026-01-04 | Antigravity | Quality | Low | Open | Logging and Observability Improvements |

## 2. Detailed Findings

### [001] Refactor `OpenSubsonicApiService` (God Class)
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **High**
- **Status**: Open

#### Concerns
The `Melodee.Common.Services.OpenSubsonicApiService` is currently ~3,500 lines of code. It acts as a "God Class," handling disparate responsibilities such as:
- License validation
- User authentication
- Media retrieval and streaming
- Playlist management (CRUD)
- System settings and shares

This violation of the Single Responsibility Principle (SRP) makes the service fragile. A change in playlist logic might inadvertently break streaming logic. It also makes the file difficult to navigate and maintain.

#### Action Items
1.  **Split the Service**: Decompose `OpenSubsonicApiService` into smaller, domain-specific services:
    -   `OpenSubsonicAuthService`: Handle `getLicense`, `startScan`, authentication.
    -   `OpenSubsonicMediaService`: Handle `stream`, `download`, `getCoverArt`.
    -   `OpenSubsonicPlaylistService`: Handle `getPlaylists`, `createPlaylist`, `updatePlaylist`.
    -   `OpenSubsonicBrowsingService`: Handle `getIndexes`, `getMusicDirectory`.
2.  **Refactor Controllers**: Update `Melodee.Blazor.Controllers.OpenSubsonic` to inject these granular services instead of the monolithic one.

---

### [002] Extract Manual Object Mapping in Jellyfin Controllers
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
The controllers in `Melodee.Blazor.Controllers.Jellyfin` (e.g., `ItemsController.cs`) contain extensive manual code to map internal `Song`, `Album`, and `Artist` entities to Jellyfin's `BaseItemDto`.
-   **Duplication**: Similar mapping logic may be repeated across different endpoints.
-   **Readability**: Controller methods are bloated with property assignments.
-   **Maintainability**: Adding a new field to the internal model requires hunting down every manual mapping instance in the controllers.

#### Action Items
1.  **Create Mappers**: Introduce a `JellyfinMapper` class or use a library like Automapper.
2.  **Centralize Logic**: Move the `MapSong`, `MapAlbum`, `MapArtist` methods out of the controllers and into this shared mapper.
3.  **Simplify Operators**: Refactor controllers to simple call `_mapper.Map<BaseItemDto>(song)` or `_jellyfinMapper.ToDto(song)`.

---

### [003] Increase Unit/Integration Test Coverage for Jellyfin
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
While `OpenSubsonicApiService` has a dedicated test suite in `Melodee.Tests.Common`, the Jellyfin implementation in `Melodee.Blazor` appears to be under-tested.
-   `Melodee.Tests.Blazor.Controllers.Jellyfin` contains very few tests.
-   Logic embedded in controllers is harder to unit test than logic in isolated services.

#### Action Items
1.  **Add Controller Tests**: Create `ItemsControllerTests.cs` using `bunit` or standard `XUnit` with mocked `DbContext` and `HttpContext`.
2.  **Test Scenarios**: Cover critical paths:
    -   Authentication failure (401).
    -   Item not found (404).
    -   Successful retrieval of Song/Album/Artist.
    -   "Similar Items" algorithm correctness.

---

### [004] Standardize API Patterns (Service vs Controller)
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
The solution uses two different patterns for its third-party APIs:
-   **OpenSubsonic**: logic in `Melodee.Common` Service (Service-Heavy).
-   **Jellyfin**: logic in `Melodee.Blazor` Controllers (Controller-Heavy).

This inconsistency increases cognitive load for developers switching between the two integrations.

#### Action Items
1.  **Align Approach**: We recommend moving the Jellyfin logic *out* of the controllers and into a `Melodee.Common.Services.JellyfinApiService`.
2.  **Benefits**:
    -   Consistency with OpenSubsonic implementation.
    -   Better testability (service tests are generally easier/faster than controller tests).
    -   Decoupling of business logic from the ASP.NET Core hosting layer.

