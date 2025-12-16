# Melodee API Review (Controllers in `Melodee.Blazor.Controllers.Melodee`)

## Phase Map
- [x] Phase 1 — Critical Security & Stability
- [x] Phase 2 — High-Impact Performance & Correctness
- [x] Phase 3 — Medium Improvements & Maintainability

This document captures security and performance concerns for the native Melodee API controllers. Findings are grouped into phased implementation by criticality to guide remediation.

---

## Scope (what was reviewed)
- Controllers: `AlbumsController`, `ArtistsController`, `PlaylistsController`, `ScrobbleController`, `SearchController`, `SongsController`, `SystemController`, `UsersController`
- Cross-cutting base types: `Melodee.Blazor.Controllers.Melodee.ControllerBase` (JWT parsing/logging), `Melodee.Blazor.Controllers.CommonBase` (base URL + IP detection)
- Note: Song streaming uses the non-versioned route `GET /song/stream/{songApiKey}/{userApiKey}/{authToken}` (outside `api/v{version}/...`) and uses an HMAC token instead of a bearer token.

## Testing guardrails (required for any modifications)
- For any controller/service/refactor touched in this review, add/update tests that lock in the expected behavior and run them **before** the change (must be green) and **after** the change (must still be green) to prove feature parity.
- Prefer “characterization tests” first: write tests that codify current observable behavior (status codes, response shape, headers, pagination metadata, auth behavior) so refactors don’t inadvertently change semantics.
- When the work intentionally changes behaNovior (e.g., fixing an injection vector, tightening auth, enforcing blacklist/roles), keep the characterization suite green by asserting invariants, then add new tests that express the **new** required behavior (these may start red and must be green after the change).
- Place tests alongside existing test projects (likely `tests/Melodee.Tests.Blazor` for API/controller-level tests; `tests/Melodee.Tests.Common` for shared helpers like HMAC/JWT/utility).
- Minimum coverage to add when implementing Phase 1/2 items:
  - Auth: invalid/missing bearer token returns 401 and does not leak sensitive headers in logs.
  - Blacklist: blacklisted email/IP is rejected consistently across all endpoints.
  - Forwarded headers: `X-Forwarded-For` is ignored unless proxy trust is configured.
  - Sorting: `orderBy/orderDirection` reject invalid values; allowed values produce deterministic ordering.
  - Pagination: `meta` fields are consistent (`totalCount`, `pageSize`, `page`, `totalPages`) across list endpoints.
  - Streaming: `Range` handling is correct for valid/invalid ranges; no double-range header issues; buffered mode does not allocate unbounded memory.

## Original Prompt for document creation
> Review the controllers that are located in the Melodee.Blazor.Controllers.Melodee namespace. Analyze the code in place and think about its design and performance. These controllers makeup the API which is used to serve
native Melodee clients (not clients using Subsonic/OpenSubsonic). Ensure that operations by these controllers are performing as optimum as possible. Prepare a review document with all concerns around around security and
performance named @prompts/MELODEE-API-REVIEW.md Include enough details in these findings to have enough clarity that coding agents can implement changes successfully towards resolving the concern. Break these findings out
into implementation phases ranked by critically. Do not include any time estimates.


---

## Phase 1 — Critical Security & Stability

1) **Authorization log leakage**
   - `src/Melodee.Blazor/Controllers/Melodee/ControllerBase.cs:OnActionExecutionAsync` serializes and logs `ApiRequest`, which includes all inbound headers and query string.
   - Risk: bearer tokens, cookies, and other secrets get written to logs; large headers can also cause log amplification / cost.
   - **Recommendation:** Log a minimal, structured subset (request id, route, user id, client ip, UA) and explicitly mask/drop sensitive headers (`Authorization`, `Cookie`, `Set-Cookie`, any `X-Api-Key`).

2) **Token validation looseness**
   - `src/Melodee.Blazor/Controllers/Melodee/ControllerBase.cs:ValidateToken` disables issuer/audience validation and uses a single symmetric secret with no key rotation (`kid`).
   - `src/Melodee.Blazor/Controllers/Melodee/UsersController.cs:AuthenticateUserAsync` *generates* tokens using only `MelodeeAuthSettings:Token` (null-forgiven) while validation accepts `Jwt:Key` as a fallback; these can diverge and/or crash if misconfigured.
   - **Recommendation:** Move to ASP.NET Core JWT auth (`AddAuthentication().AddJwtBearer(...)`) with issuer/audience, consistent key configuration, and (if needed) key rotation via `kid`; fail startup if required JWT settings are missing.

3) **Client IP spoofing via `X-Forwarded-For`**
   - `src/Melodee.Blazor/Controllers/CommonBase.cs:GetRequestIp` prefers `X-Forwarded-For` by default, which is trivially spoofable unless the app is configured to only trust known proxies.
   - Impact: blacklist checks, audit logs, and any per-IP throttling can be bypassed; attackers can frame other IPs in logs.
   - **Recommendation:** Only honor forwarded headers when `ForwardedHeadersOptions` is configured with `KnownProxies/KnownNetworks` (or behind a trusted ingress). Otherwise use `HttpContext.Connection.RemoteIpAddress`.

4) **SQL injection surface via `orderBy`/`orderDirection`**
   - Controllers accept unvalidated `orderBy`/`orderDirection` inputs (`SongsController.ListAsync`, `AlbumsController.ListAsync`, `ArtistsController.ListAsync`).
   - `src/Melodee.Common/Models/PagedRequest.cs:OrderByValue` concatenates the order-by field and direction into SQL with no sanitization/parameterization. A crafted `orderBy` containing quotes can break out of `"..."` and inject SQL.
   - **Recommendation:** Replace free-form `orderBy` with a whitelist enum (or map of allowed client fields → known DB columns) and only accept `ASC|DESC` (case-insensitive). Reject any values containing `"`, `,`, whitespace, or other non-identifier characters.

5) **Blacklist checks are inconsistent across controllers**
   - Blacklist enforcement is present on some endpoints (e.g., `SongsController` star/rating/stream, `SearchController`, parts of `UsersController`) but missing entirely on others (`AlbumsController`, `ArtistsController`, `PlaylistsController`, `SystemController.GetSystemStatsAsync`, `ScrobbleController`, `SongsController.SongById/ListAsync/RecentlyAddedAsync`, `UsersController.AboutMeAsync`).
   - Impact: blacklisted users/IPs can still enumerate library/playlist data, scrobble, and query system stats.
   - **Recommendation:** Centralize checks into a filter/middleware that (a) validates auth, (b) resolves the user once, (c) enforces `IsLocked`, (d) enforces blacklist by email + resolved client IP. Apply globally to this API group and explicitly opt-out for `System/info` and `Users/authenticate`.

6) **External HTTP calls without resilient client**
   - `ScrobbleController.GetLastFmSessionKeyAsync` instantiates `HttpClient` directly per request with no timeout or retries. Can exhaust sockets and hang threads under transient failures.
   - Additional correctness/safety: `GetLastFmAuthUrl` interpolates `callback` directly into a URL without encoding; `GetLastFmSessionKeyAsync` interpolates `token` into the query string without encoding.
   - **Recommendation:** Use `IHttpClientFactory` with explicit timeouts + retry/backoff; URL-encode query parameters and consider restricting/validating the callback URL (allowlist scheme/host) to avoid generating malicious auth links.

7) **Unbounded streaming read fallback**
   - `SongsController.StreamSong` buffered path (`useBuffered`) reads entire file into memory when no range is provided; large files can cause memory spikes.
   - **Recommendation:** Prefer streaming (`FileStreamResult`) always; if buffered mode is required, cap size or stream in chunks with buffering disabled for large files.

8) **Rate limiting gaps (including login)**
   - Concurrency limiting exists only for `SongsController.StreamSong`. Other expensive endpoints (search, paged listings) and the login endpoint (`UsersController.AuthenticateUserAsync`) have no throttling.
   - Impact: brute force login attempts, request floods, and abusive large page requests can starve CPU/DB and degrade service.
   - **Recommendation:** Add global ASP.NET rate limiting with per-IP and per-user buckets; apply stricter limits to authentication and search/list endpoints.

9) **API parameter validation gaps**
   - Paging inputs (`page`, `pageSize`, `limit`) are not consistently bounded and sometimes not normalized to `>= 1` (negative/zero page can produce negative `Skip`).
   - Some endpoints accept nullable/large types (`UsersController.UsersPlaylistsAsync(int? page, short? pageSize)`) without caps; `PagedRequest.PageSizeValue` treats `-1` as “return all” (500), which should not be client-controllable without explicit intent.
   - **Recommendation:** Enforce min/max bounds at model-binding (e.g., `page >= 1`, `1 <= pageSize <= 200`, `1 <= limit <= 200`) and reject invalid values with a consistent 400 response.

10) **HMAC streaming token validity and replay**
   - Stream auth validates only that the timed token is valid; the token payload currently binds to `{userId}:{songApiKey}` (see `UserModelExtensions.CreateAuthUrlFragment`), but not to requester IP/UA and has no replay protection beyond the time window.
   - **Recommendation:** Bind token to user + song + an immutable client attribute (at least IP, optionally UA) and shorten TTL; optionally include a nonce and store one-time-use tokens for the TTL window if replay is a concern.

11) **Authorization/roles are not enforced**
   - User role flags exist (`HasStreamRole`, `HasPlaylistRole`, `IsAdmin`, etc. in `src/Melodee.Common/Data/Models/User.cs`) but controllers generally only check `IsLocked`.
   - Impact: endpoints like `SystemController.GetSystemStatsAsync` and playlist/streaming endpoints may be accessible to users who should not have those capabilities.
   - **Recommendation:** Define an authorization policy per capability and enforce it in a centralized filter/policy (e.g., stats require admin; streaming requires `HasStreamRole`; playlist endpoints require `HasPlaylistRole`; scrobble requires `IsScrobblingEnabled`).

---

## Phase 2 — High-Impact Performance & Correctness

1) **Redundant configuration and user fetches**
   - Each action re-fetches configuration (`GetConfigurationAsync`) and user (`GetByApiKeyAsync`) multiple times per request.
   - **Recommendation:** Cache config per request (e.g., scoped accessor) and reuse the resolved user in the controller/action to cut extra DB/config hits.

2) **N+1/user-song enrichment**
   - `SongsController.SongById` fetches `UserSongsForAlbumAsync` then filters for a single song, pulling the whole album’s user songs.
   - **Recommendation:** Add a direct `GetUserSongAsync(userId, songApiKey)` to avoid album-wide queries.

3) **Search defaults and fan-out**
   - Search POST defaults to page size 50 with no cap and accepts multi-type searches in one call, which can fan out to multiple queries.
   - **Recommendation:** Cap page size, consider per-include limits, and short-circuit when query is empty/too short to reduce load.

4) **Streaming header handling**
   - `SongsController.StreamSong` manually sets `Content-Range`/`Accept-Ranges` via `RangeParser.CreateResponseHeaders` and also sets `EnableRangeProcessing = true` on results.
   - Risk: double range processing / duplicated headers / incorrect status codes depending on how ASP.NET handles the stream/result type; also missing conditional caching (`ETag`/`If-None-Match`) despite having `EtagRepository`.
   - **Recommendation:** Choose one approach: either (a) let ASP.NET handle ranges (`EnableRangeProcessing = true`) and stop manually writing range headers, or (b) fully own range handling and set `EnableRangeProcessing = false`. Add conditional requests (ETag/If-None-Match) where appropriate.

5) **User lock/blacklist branching cost**
   - Many actions repeat the same auth/lock/blacklist checks inline, increasing branching and error-prone omissions.
   - **Recommendation:** Move to filters/middleware to centralize and deduplicate these checks.

6) **Pagination metadata correctness**
   - `PlaylistsController.ListAsync` constructs `PaginationMetadata` with the `page`/`pageSize` arguments swapped compared to other endpoints (`new PaginationMetadata(playlists.TotalCount, page, pageSize, ...)`).
   - Impact: clients can mis-render pagination controls or cache keys; inconsistency undermines the “uniform best practices” goal stated in `src/Melodee.Blazor/Controllers/Melodee/README.md`.
   - **Recommendation:** Standardize a single constructor usage across all endpoints and add a small integration test that asserts pagination metadata shape for each list endpoint.

7) **Scrobbling initialization per request**
   - `ScrobbleController.ScrobbleSong` calls `scrobbleService.InitializeAsync(configuration)` on each request; depending on implementation, this may do expensive work repeatedly.
   - **Recommendation:** Initialize scrobbling once on startup or memoize initialization per configuration snapshot; avoid per-request initialization unless it’s a no-op after first run.

---

## Phase 3 — Medium Improvements & Maintainability

1) **Structured error responses**
   - Error payloads are ad-hoc (anonymous objects). This complicates client handling and observability.
   - **Recommendation:** Standardize an error contract (code, message, correlationId) and use it across controllers.

2) **Logging consistency and noise**
   - Mixed logging styles (Serilog, Trace) and occasional broad object serialization can be noisy and expensive.
   - **Recommendation:** Standardize structured Serilog with event IDs; avoid serializing large objects (e.g., `ApiRequest`) at info level.

3) **Caching opportunities**
   - Frequently requested metadata (recent songs, server info) is re-queried on every request.
   - **Recommendation:** Add short-lived cache for read-heavy endpoints keyed by user and parameters, invalidated on relevant writes.

4) **DTO/model trimming**
   - Models returned by search/list include full nested user info per item, increasing payload size.
   - **Recommendation:** Provide lightweight projections for list/search endpoints to reduce serialization and bandwidth.

5) **Test coverage for edge cases**
   - Missing tests for malformed Range headers, blacklisted user access, and large page sizes across these controllers.
   - **Recommendation:** Add unit/integration tests covering these edge cases and treat them as required pre/post-change guardrails: tests must be added and passing before refactors, then pass again after refactors to prove feature parity.

6) **Route and API surface consistency**
   - The streaming route is non-versioned (`/song/stream/...`) while the rest of the API is versioned (`/api/v{version}/...`), and the `src/Melodee.Blazor/Controllers/Melodee/README.md` references `/user/authenticate` even though the controller route is versioned.
   - **Recommendation:** Document the canonical routes for native clients and consider versioning the stream route (or explicitly declaring it “out of band”) to avoid proxy/cache surprises.

---

## Suggested Next Steps
1) Implement Phase 1 items first (auth hardening, blacklist parity, HTTP client resiliency, bounded paging/streaming, sensitive logging hygiene).
2) Follow with Phase 2 performance fixes (per-request caching, direct user-song lookup, search caps, conditional requests, centralized filters).
3) Execute Phase 3 improvements for consistency, payload efficiency, and test coverage.
