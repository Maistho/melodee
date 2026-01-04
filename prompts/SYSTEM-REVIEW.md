# System Codebase Review Log

This document serves as a living record of codebase reviews performed by various coding agents. It tracks findings, risks, and actionable items to improve the Melodee solution.

## 1. Summary of Findings

| ID | Date | Agent | Category | Risk | Status | Description |
|:---|:-----|:------|:---------|:-----|:-------|:------------|
| 001 | 2026-01-04 | Antigravity | Architecture | High | Open | Refactor `OpenSubsonicApiService` (God Class) |
| 002 | 2026-01-04 | Antigravity | Security | High | Open | JWT Secret Key Validation and Security Hardening |
| 003 | 2026-01-04 | Antigravity | Testing | High | Open | Missing OpenSubsonic Controller Tests |
| 004 | 2026-01-04 | opencode | Testing | High | Open | Blazor Component Tests |
| 005 | 2026-01-04 | opencode | Blazor | High | Open | Async Void in Blazor Components (Critical Exception Handling) |
| 006 | 2026-01-04 | opencode | Security | High | Open | File Path Traversal Risk in Audio Streaming |
| 007 | 2026-01-04 | Antigravity | Quality | Medium | Open | Extract Manual Object Mapping in Jellyfin Controllers |
| 008 | 2026-01-04 | Antigravity | Testing | Medium | Open | Increase Unit/Integration Test Coverage for Jellyfin |
| 009 | 2026-01-04 | Antigravity | Architecture | Medium | Open | Standardize API Patterns (Service vs Controller) |
| 010 | 2026-01-04 | Antigravity | Security | Medium | Open | Inconsistent Authentication Across APIs |
| 011 | 2026-01-04 | Antigravity | Quality | Medium | Open | Missing Input Validation and Error Handling |
| 012 | 2026-01-04 | Antigravity | Performance | Medium | Open | Database Connection Pool Configuration |
| 013 | 2026-01-04 | Antigravity | Performance | Medium | Open | Jellyfin InstantMix/Similar Items Performance |
| 014 | 2026-01-04 | opencode | Blazor | Medium | Open | Error Handling in Razor Components |
| 015 | 2026-01-04 | opencode | Blazor | Medium | Open | Performance Optimizations |
| 016 | 2026-01-04 | opencode | API | Medium | Open | Jellyfin Phase 6 Completion |
| 017 | 2026-01-04 | opencode | Performance | Medium | Open | Partitioned Rate Limiting per API |
| 018 | 2026-01-04 | opencode | Blazor | Medium | Open | Missing ConfigureAwait(False) in Blazor Components |
| 019 | 2026-01-04 | opencode | Architecture | Medium | Open | Large Jellyfin ItemsController Violates SRP |
| 020 | 2026-01-04 | opencode | Blazor | Medium | Open | Incomplete Error Handling in Blazor Components |
| 021 | 2026-01-04 | opencode | Security | Medium | Open | Missing Anti-Forgery Validation on State-Changing Endpoints |
| 022 | 2026-01-04 | opencode | API | Medium | Open | Missing Request Validation for Jellyfin InstantMix Parameters |
| 023 | 2026-01-04 | Antigravity | Architecture | Low | Open | CORS Configuration Too Permissive |
| 024 | 2026-01-04 | Antigravity | Quality | Low | Open | API Documentation and OpenAPI Specification |
| 025 | 2026-01-04 | Antigravity | Quality | Low | Open | Logging and Observability Improvements |
| 026 | 2026-01-04 | opencode | Quality | Low | Open | Missing ConfigureAwait(False) in Some Controller Calls |
| 027 | 2026-01-04 | opencode | Security | Low | Open | Inconsistent Response Headers and Security Headers |

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

### [002] JWT Secret Key Validation and Security Hardening
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **High**
- **Status**: Open

#### Concerns
In `Program.cs` (lines 246-269), the application validates that JWT configuration exists but does not enforce strong security requirements:
-   **Weak Key Detection**: No validation that `Jwt:Key` is sufficiently long or random. A weak key compromises the entire authentication system.
-   **Production Requirements**: `options.RequireHttpsMetadata = true` is good, but there's no enforcement that HTTPS is actually enabled in production deployments.
-   **Key Rotation**: No mechanism for JWT key rotation or versioning.
-   **ClockSkew**: Set to `TimeSpan.Zero` which is strict, but could cause issues with minor time sync problems across distributed systems.

#### Action Items
1.  **Validate JWT Key Strength**: Add startup validation to ensure `Jwt:Key` is at least 256 bits (32 bytes) and contains sufficient entropy:
    ```csharp
    if (jwtKey.Length < 32) 
        throw new InvalidOperationException("JWT key must be at least 32 characters");
    ```
2.  **Environment-Specific Validation**: Add a check to ensure HTTPS is enabled in production environments.
3.  **Key Rotation Support**: Implement support for multiple valid signing keys to enable zero-downtime key rotation.
4.  **Documentation**: Document JWT key generation requirements in `example.env` and deployment documentation, including recommended key generation methods (e.g., `openssl rand -base64 32`).
5.  **ClockSkew Adjustment**: Consider allowing a small ClockSkew (e.g., 5 seconds) to handle minor time synchronization issues while still maintaining security.

---

### [003] Missing OpenSubsonic Controller Tests
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **High**
- **Status**: Open

#### Concerns
While there is `Melodee.Tests.Common/Services/OpenSubsonicApiServiceTests.cs` testing the service layer, there are **NO tests** for the OpenSubsonic controllers in `Melodee.Blazor/Controllers/OpenSubsonic/`:
-   Controllers act as thin facades calling `OpenSubsonicApiService`, but they still handle routing, request binding, and response serialization.
-   Critical integration points like authentication, rate limiting, and ETag generation are untested at the controller level.
-   The OpenSubsonic API has complex routing (e.g., `/rest/getAlbum.view` and `/rest/getAlbum` both valid) that requires controller-level testing.

This is particularly concerning given that:
-   The service is 3,494 lines, making comprehensive service-level testing challenging.
-   Client applications rely on exact OpenSubsonic API compatibility.

#### Action Items
1.  **Create Controller Test Project Structure**: Add `Melodee.Tests.Blazor/Controllers/OpenSubsonic/` directory.
2.  **Test Critical Endpoints**: Create tests for high-traffic endpoints:
    -   `SystemControllerTests.cs`: Test ping, getLicense, getOpenSubsonicExtensions
    -   `BrowsingControllerTests.cs`: Test getIndexes, getMusicDirectory, getArtists, getArtist
    -   `MediaRetrievalControllerTests.cs`: Test stream, download, getCoverArt
    -   `PlaylistControllerTests.cs`: Test getPlaylists, getPlaylist, createPlaylist, updatePlaylist
3.  **Test Authentication**: Verify that endpoints properly reject unauthenticated requests and accept valid credentials (username/password, token).
4.  **Test Response Format**: Verify responses match OpenSubsonic XML/JSON schema:
    ```csharp
    [Fact]
    public async Task GetLicense_ReturnsValidOpenSubsonicResponse()
    {
        var result = await controller.GetLicenseAsync();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseModel>(okResult.Value);
        Assert.Equal("ok", response.Status);
        Assert.NotNull(response.ItemResult);
    }
    ```
5.  **Integration Tests**: Create integration tests using `WebApplicationFactory<Program>` to test the full HTTP pipeline including middleware, routing, and serialization.
6.  **Test Coverage Metrics**: Add test coverage reporting to CI/CD pipeline with a minimum threshold (e.g., 70%) for controller coverage.

---

### [004] Blazor Component Tests
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **High**
- **Status**: Open

#### Concerns
No bUnit/XUnit tests for Blazor components/Pages found (e.g., tests/Melodee.Tests.Blazor/Components empty-ish). Controllers have some (JellyfinRouting), but UI logic (image upload, theme switch, search filters) untested. Breaks isolation from API changes.

#### Action Items
1.  **Setup bUnit**: Add `bunit` NuGet to Melodee.Tests.Blazor; create Components/DashboardTests.razor for rendering/interactions.
2.  **Key Tests**: `AlbumDetailTests`: Render, mock HttpClient, assert data binding/ETags. `ThemeSelectorTests`: Toggle themes, verify CSS classes.
3.  **Integration**: Playwright for E2E (existing); snapshot testing for themes/UI states.
4.  **CI Coverage**: Extend coverage.runsettings to 80% for Blazor; run `dotnet test --collect:"XPlat Code Coverage"`.

---

### [005] Async Void in Blazor Components (Critical Exception Handling Issue)
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **High**
- **Status**: Open

#### Concerns
Multiple Blazor components use `async void` event handlers which is a critical anti-pattern:
- **AlbumDetail.razor:668**: `private async void OnShowItemChange(TreeEventArgs arg)`
- **ArtistDetail.razor:725**: `private async void OnShowItemChange(TreeEventArgs arg)`
- **MainLayout.razor:171**: `private async void OnConfigurationChanged(object? sender, EventArgs e)`
- **Songs.razor:332**: `private async void MergeSelectedButtonClick()`
- **Albums.razor:342**: `private async void MergeSelectedButtonClick()`

**Why this is dangerous:**
- Exceptions thrown in `async void` methods cannot be caught by standard try-catch blocks
- Exceptions propagate to synchronization context and can crash entire Blazor circuit
- No way to properly log or handle errors, leading to silent failures
- Creates unpredictable application state when failures occur

#### Action Items
1.  **Refactor All Async Void Methods**: Replace all `async void` with `async Task` in Blazor components:
    ```csharp
    // Before (DANGEROUS)
    private async void OnShowItemChange(TreeEventArgs arg)
    {
        await someOperation();
    }

    // After (CORRECT)
    private async Task OnShowItemChange(TreeEventArgs arg)
    {
        await someOperation();
    }
    ```
2.  **Update Event Handler Bindings**: Ensure all event handler bindings in Razor markup use `await` appropriately:
    ```razor
    <!-- For methods that return Task -->
    <RadzenTree ... OnChange="@(args => OnShowItemChange(args))" />
    ```
3.  **Add Exception Handling**: Wrap async operations in try-catch blocks to handle errors gracefully:
    ```csharp
    private async Task OnShowItemChange(TreeEventArgs arg)
    {
        try
        {
            await someOperation();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OnShowItemChange");
            NotificationService.Notify(NotificationSeverity.Error, "Error", "Operation failed");
        }
    }
    ```
4.  **Code Review**: Audit all 80 Razor components to ensure no `async void` patterns exist.

---

### [006] File Path Traversal Risk in Audio Streaming
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **High**
- **Status**: Open

#### Concerns
In `Jellyfin/AudioController.cs:78`, file paths are constructed from database values without proper validation:
```csharp
var filePath = Path.Combine(song.LibraryPath, song.ArtistDirectory, song.AlbumDirectory, song.FileName);
if (!System.IO.File.Exists(filePath))
{
    return JellyfinNotFound("Audio file not found.");
}
```

**Vulnerabilities:**
- No validation that constructed path stays within expected library directory
- If database values contain path traversal sequences (e.g., `..\..\..\etc\passwd`), files outside the library could be accessed
- Melodee/OpenSubsonic streaming endpoints likely have the same issue

While the database query filters by `Library.Path`, an attacker could:
- Inject malicious data directly into the database (SQL injection or compromised admin account)
- Exploit a data migration or import vulnerability to modify path fields
- Use social engineering to convince an admin to upload malformed library paths

#### Action Items
1.  **Add Path Validation**: Implement path validation before serving files:
    ```csharp
    var filePath = Path.Combine(song.LibraryPath, song.ArtistDirectory, song.AlbumDirectory, song.FileName);
    var fullPath = Path.GetFullPath(filePath);

    // Verify file is within expected library directory
    if (!fullPath.StartsWith(Path.GetFullPath(song.LibraryPath), StringComparison.OrdinalIgnoreCase))
    {
        Logger.LogWarning("Path traversal attempt detected: {FilePath} from Library: {LibraryPath}", fullPath, song.LibraryPath);
        return JellyfinForbidden("Invalid file path.");
    }
    ```
2.  **Apply to All Streaming Endpoints**: Update all audio streaming endpoints:
    -   `Jellyfin/AudioController.cs`
    -   `OpenSubsonic/MediaRetrievalController.cs` (stream/download methods)
    -   Any Melodee native API streaming endpoints
3.  **Database Constraints**: Add database constraints or validation to prevent path traversal sequences in Artist/Album/FileName fields:
    -   Validate on insert/update that paths don't contain `..`, `//`, or other suspicious patterns
    -   Consider storing paths as relative to library root and validating at access time
4.  **Security Testing**: Add security tests to verify path traversal attempts are blocked:
    ```csharp
    [Fact]
    public async Task StreamAudioWithPathTraversal_ShouldReturn403()
    {
        // Test with malicious path data
    }
    ```

---

### [007] Extract Manual Object Mapping in Jellyfin Controllers
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

### [008] Increase Unit/Integration Test Coverage for Jellyfin
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

### [009] Standardize API Patterns (Service vs Controller)
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

---

### [010] Inconsistent Authentication Across APIs
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
The three APIs use different authentication mechanisms:
-   **Melodee Native API**: Uses JWT Bearer authentication with `[Authorize]` attributes (seen in `Controllers/Melodee/AlbumsController.cs`).
-   **Jellyfin API**: Custom authentication in `JellyfinControllerBase.AuthenticateJellyfinAsync()` that validates API tokens from custom headers or query parameters.
-   **OpenSubsonic API**: Custom authentication through `ApiRequest` object in `ControllerBase`.

This creates several issues:
-   **Confusion for Developers**: Different patterns make it harder to understand security boundaries.
-   **Security Risk**: Easier to accidentally omit authentication on new endpoints.
-   **Testing Complexity**: Requires multiple test harnesses for different authentication flows.
-   **Credential Management**: Users must manage multiple sets of credentials.

#### Action Items
1.  **Document Authentication Models**: Create comprehensive documentation (`docs/AUTHENTICATION.md`) explaining:
    -   When each authentication method is used
    -   How to generate and manage credentials for each API
    -   Security considerations for each approach
2.  **Standardize Base Controller Pattern**: Create documentation showing the relationship between `CommonBase`, `JellyfinControllerBase`, and Melodee controllers to clarify the authentication inheritance chain.
3.  **Add Auth Validation Tests**: Create integration tests that verify authentication is properly enforced on all API endpoints, catching any accidentally unprotected routes.
4.  **Consider Unified Auth**: Evaluate whether Jellyfin and OpenSubsonic APIs could optionally support JWT authentication as an alternative to their native auth schemes for easier integration.

---

### [011] Missing Input Validation and Error Handling
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
Reviewing controllers reveals inconsistent input validation:
-   **Jellyfin ItemsController**: Some validation exists (e.g., `Math.Clamp(limit ?? 100, 1, 500)` at line 61), but many query parameters accept raw values without sanitization.
-   **GUID Parsing**: `TryParseJellyfinGuid()` pattern is good but not consistently applied everywhere.
-   **Search Terms**: User-provided search terms are converted to uppercase (`ToUpperInvariant()`) but not sanitized against SQL injection or special characters that might exploit EF Core query translation.
-   **Error Messages**: Some error messages may leak internal implementation details (e.g., stack traces in development mode).

While EF Core protects against SQL injection via parameterization, there are still concerns:
-   **Denial of Service**: Unbounded or extremely large query parameters (e.g., massive `limit` values before clamping) could cause performance issues.
-   **Path Traversal**: File operations (e.g., `GetItemFileAsync` in Jellyfin) rely on database paths but should validate against directory traversal.

#### Action Items
1.  **Add Input Validation Attributes**: Apply `[Range]`, `[StringLength]`, `[RegularExpression]` attributes to controller action parameters.
2.  **Create Validation Filters**: Implement an action filter that validates all incoming parameters and returns consistent error responses:
    ```csharp
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
    ```
3.  **Sanitize Search Input**: Create a helper method to sanitize search terms, removing or escaping potentially problematic characters.
4.  **File Path Validation**: Add explicit validation in file serving endpoints to ensure paths don't escape expected directories:
    ```csharp
    var fullPath = Path.GetFullPath(filePath);
    if (!fullPath.StartsWith(allowedBasePath))
        return Forbid();
    ```
5.  **Standardize Error Responses**: Ensure production error responses don't leak sensitive information. Use `ProblemDetails` consistently across all three APIs.
6.  **Add Rate Limiting**: While rate limiting exists for APIs, consider adding more granular limits on expensive operations like search and instant mix.

---

### [012] Database Connection Pool Configuration
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
In `Program.cs` (lines 105-128), database connection pooling is configured with environment variable overrides:
-   **Default Pool Sizes**: The defaults are controlled by Npgsql, which may not be optimal for high-traffic scenarios.
-   **No Monitoring**: There's no logging or metrics collection for pool exhaustion or connection timeouts.
-   **Multiple DbContexts**: The application uses three separate DbContext factories (Melodee, ArtistSearchEngine, MusicBrainz) each with their own connection pools, which could lead to inefficient resource usage.
-   **SQLite Contexts**: The ArtistSearchEngine and MusicBrainz contexts use SQLite, which has different concurrency characteristics than PostgreSQL.

#### Action Items
1.  **Document Pool Configuration**: Add documentation explaining recommended pool size settings for different deployment scenarios:
    -   Small deployments (1-10 users)
    -   Medium deployments (10-100 users)
    -   Large deployments (100+ users)
2.  **Add Connection Pool Monitoring**: Implement logging to track connection pool metrics:
    ```csharp
    opt.UseNpgsql(effectiveConnString, o => 
    {
        o.UseNodaTime()
         .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
         .EnableRetryOnFailure(maxRetryCount: 3);
    }).LogTo(Console.WriteLine, LogLevel.Information);
    ```
3.  **Set Explicit Defaults**: Instead of relying on Npgsql defaults, set explicit `MinPoolSize` and `MaxPoolSize` values in `appsettings.json`:
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=melodee;Username=melodee;Password=***;MinPoolSize=5;MaxPoolSize=100"
      }
    }
    ```
4.  **SQLite Concurrency**: Document SQLite's `busy_timeout` and `JournalMode=WAL` settings for the read-heavy MusicBrainz database to improve concurrent access.
5.  **Health Checks**: Add database connectivity health checks for all three database contexts to expose in `/health` endpoint.

---

### [013] Jellyfin InstantMix/Similar Items Performance
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
In `Jellyfin/ItemsController.cs`, the `GetInstantMixAsync` (lines 436-582) and `GetSimilarItemsAsync` (lines 324-430) methods use `EF.Functions.Random()` for shuffling:
```csharp
.OrderBy(s => EF.Functions.Random())
.Take(maxItems)
```

While functionally correct, this approach has performance issues:
-   **Full Table Scan**: Using `Random()` in `OrderBy` prevents index usage and requires scanning all matching records.
-   **Database Load**: For large music libraries (100K+ songs), this generates expensive queries.
-   **Scalability**: Performance degrades linearly with library size.
-   **Caching Difficulty**: Random results can't be effectively cached.

Additionally:
-   The "similar items" algorithm is simplistic (same artist OR shared genres), which might not produce high-quality recommendations.
-   Genre matching uses array contains (`genres.Any(g => s.Album.Genres.Contains(g))`), which can be inefficient for large genre arrays.

#### Action Items
1.  **Optimize Random Selection**: Replace `EF.Functions.Random()` with a more efficient approach:
    -   **Option A - Reservoir Sampling**: Fetch more records than needed, then sample in-memory
    -   **Option B - Random ID Selection**: Pre-generate random IDs and query by ID
    ```csharp
    // Fetch candidate IDs
    var candidateIds = await query
        .Select(s => s.Id)
        .Take(maxItems * 5) // Get more than needed
        .ToListAsync();

    // Random sample in memory
    var selectedIds = candidateIds
        .OrderBy(_ => Random.Shared.Next())
        .Take(maxItems);

    // Fetch full objects
    var songs = await dbContext.Songs
        .Where(s => selectedIds.Contains(s.Id))
        .Include(...)
        .ToListAsync();
    ```
2.  **Add Caching**: Cache "instant mix" results for frequently accessed items (popular artists/albums) with a reasonable TTL (e.g., 1 hour).
3.  **Improve Similarity Algorithm**: Consider more sophisticated matching:
    -   Weight matches (same artist > shared genres > same era)
    -   Use audio features if available (tempo, energy, mood)
    -   Incorporate user listening history for personalized mixes
4.  **Add Performance Monitoring**: Add logging to track query execution times:
    ```csharp
    var stopwatch = Stopwatch.StartNew();
    var results = await query.ToListAsync();
    logger.LogInformation("InstantMix generated {Count} songs in {ElapsedMs}ms", 
        results.Count, stopwatch.ElapsedMilliseconds);
    ```
5.  **Database Indexes**: Ensure appropriate indexes exist:
    -   Composite index on `(IsLocked, AlbumId, ArtistId)` for song queries
    -   Index on `Genres` column if supported by PostgreSQL (GIN index for array columns)

---

### [014] Error Handling in Razor Components
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
Blazor components (e.g., AlbumDetail.razor, Login.razor) use injected HttpClient for API calls without explicit try-catch blocks. Unhandled exceptions (network errors, 4xx/5xx) can crash individual components or propagate, degrading UX without user feedback. No global ErrorBoundary observed wrapping pages; relies on NotificationService/DialogService inconsistently.

#### Action Items
1.  **Add ErrorBoundary Components**: Wrap main layout/pages with `<ErrorBoundary>@childContent</ErrorBoundary>` using Radzen or custom, logging exceptions to Serilog.
2.  **Component-Level Handling**: In `@code` blocks, wrap API calls: `try { var response = await http.Get... } catch (HttpRequestException ex) { notificationService.Notify(NotificationSeverity.Error, "Network error", ex.Message); }`.
3.  **Global Handler**: In Program.cs, add `builder.Services.AddScoped<ErrorBoundaryBase>();` and route unhandled to DoctorService or centralized logger.
4.  **Test Coverage**: Add bUnit tests simulating HttpClient failures for key pages (Dashboard, Search).

---

### [015] Blazor Performance Optimizations
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
Blazor Server mode risks SignalR overload for large libraries/charts (no virtualization in Radzen grids visible). Bundle size high (~300 DLLs in bin); no pagination/debouncing in some lists (charts). Themes/CSS per-file bloats transfers; no AOT/WASM hints.

#### Action Items
1.  **Virtualization**: Add `Virtualize` or Radzen DataGrid `Virtualization="true"` to Artist/Album/Song lists; paginate charts.
2.  **Bundle Optimization**: Use `IlcPublishAot` or trim unused assemblies; minify CSS/JS (melodee.css + themes).
3.  **Circuit Handling**: Enhance MelodeeCircuitHandler with disconnect logging; consider Blazor WebAssembly for heavy clients.
4.  **Profiling**: Add `dotnet-trace` scripts for UI perf; monitor SignalR connections in DoctorService.

---

### [016] Jellyfin Phase 6 Completion
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
Per prompts/JELLYFIN-SPEC.md, Phase 6 (Token Mgmt/Admin) partial: No `/Auth/Keys` for admin token create/revoke; mobile clients (Finamp) pending testing; no WS sessions or transcoding stubs. Breaks full Jellyfin compat.

#### Action Items
1.  **Admin Endpoints**: Add `JellyfinUsersController.PostNamedKeysAsync()` mirroring Jellyfin OpenAPI; DB ops on JellyfinAccessToken.
2.  **Mobile Testing**: Run scripts/test-jellyfin-api.sh with Finamp/Streamyfin; stub `/Audio/{id}/universal` if needed.
3.  **Enhance**: Add WS `/WebSocket` forwarder; transcoding flags (FFMpeg integration).
4.  **Verify**: Contract tests vs. jellyfin-10.11.5-openapi.json using OpenAPI validator.

---

### [017] Partitioned Rate Limiting per API
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
RateLimiterService shared across APIs; Jellyfin has dedicated config but overload (e.g., Subsonic floods) impacts native. No per-IP/endpoint bytes/sec for streams.

#### Action Items
1.  **Named Limiters**: In Program.cs, `AddRateLimiter("melodee", opts => ...); "jellyfin", "opensubsonic"`.
2.  **Partitioning**: Use `PartitionedRateLimiter` by path prefix or client ID (X-Forwarded-For).
3.  **Stream Limits**: Add bandwidth quotas (e.g., 10MB/s per stream) via middleware.
4.  **Metrics**: Expose limits in DoctorService; alert on 429 spikes.

---

### [018] Missing ConfigureAwait(False) in Blazor Components
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
Only 15 instances of `ConfigureAwait(false)` found in Blazor component pages across 80 Razor files. While `ConfigureAwait(false)` is less critical in Blazor Server mode (already on a synchronization context), it's still important for:
- Performance optimization by avoiding unnecessary thread marshalling
- Reducing potential for deadlocks in complex async scenarios
- Consistency with backend API code which uses ConfigureAwait(false) extensively (100+ instances)

The inconsistency suggests different developers may be unaware of async best practices.

#### Action Items
1.  **Audit Async Calls**: Review all async method calls in Razor components and identify where `ConfigureAwait(false)` should be added.
2.  **Add ConfigureAwait(false)** to non-UI-critical async operations:
    ```csharp
    // In @code blocks
    private async Task LoadDataAsync()
    {
        _data = await _service.GetDataAsync(cancellationToken).ConfigureAwait(false);
    }
    ```
3.  **Documentation**: Create team guidelines for async/await patterns in Blazor components vs. backend code.
4.  **Static Analysis**: Consider adding a Roslyn analyzer to detect missing `ConfigureAwait(false)` in appropriate contexts.

---

### [019] Large Jellyfin ItemsController Violates SRP
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
`Jellyfin/ItemsController.cs` is 1,090 lines of code, making it difficult to maintain and test. The controller handles multiple responsibilities:
- Item retrieval by ID and query parameters
- InstantMix and SimilarItems generation
- Complex mapping logic for Artist/Album/Song to Jellyfin DTOs
- Multiple endpoint variations (search, browse, recommendations)

This is similar to OpenSubsonicApiService god class issue (Finding #001).

#### Action Items
1.  **Extract Mapping Logic**: Move all mapping methods (MapArtist, MapAlbum, MapSong) to a dedicated `JellyfinMapper` class or use AutoMapper.
2.  **Create Specialized Services**: Extract business logic into services:
    -   `JellyfinItemsQueryService`: Handle search, filtering, and pagination
    -   `JellyfinRecommendationsService`: Handle InstantMix and SimilarItems algorithms
    -   `JellyfinItemMapperService`: Handle DTO transformation
3.  **Controller Simplification**: Reduce controller to thin API layer:
    ```csharp
    [HttpGet("{itemId}")]
    public async Task<IActionResult> GetItemAsync(string itemId, CancellationToken cancellationToken)
    {
        var user = await AuthenticateJellyfinAsync(cancellationToken);
        if (user == null) return JellyfinUnauthorized();

        var result = await _itemsQueryService.GetItemAsync(itemId, user, cancellationToken);
        return result.Match<IActionResult>(
            data => Ok(data),
            error => error switch
            {
                NotFoundError => JellyfinNotFound(error.Message),
                _ => JellyfinBadRequest(error.Message)
            });
    }
    ```
4.  **Test Coverage**: With extracted services, unit tests can target business logic without requiring full HTTP context.

---

### [020] Incomplete Error Handling in Blazor Components
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
Building on Finding #014 (Error Handling in Razor Components), additional analysis reveals:
- **Login.razor** has extensive error handling (good example)
- **AlbumDetail.razor**, **ArtistDetail.razor**, and other data pages lack comprehensive error handling
- API calls in `@code` blocks often lack try-catch
- No global error boundary for unhandled component exceptions
- Exception information may not be logged when components fail silently

#### Action Items
1.  **Wrap All API Calls**: Ensure all service calls in `@code` blocks have try-catch:
    ```csharp
    private async Task LoadAlbumDataAsync()
    {
        try
        {
            _isLoading = true;
            var result = await AlbumService.GetByApiKeyAsync(_apiKey);
            if (!result.IsSuccess || result.Data == null)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Album not found");
                NavigationManager.NavigateTo("/data/albums");
                return;
            }
            _album = result.Data;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load album {ApiKey}", _apiKey);
            NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to load album data");
        }
        finally
        {
            _isLoading = false;
        }
    }
    ```
2.  **Add Global Error Boundary**: Implement a custom ErrorBoundary component at the application root:
    ```razor
    <ErrorBoundary OnErrorAsync="HandleErrorAsync">
        <ChildContent>
            <Router AppAssembly="@typeof(App).Assembly">
                ...
            </Router>
        </ChildContent>
        <ErrorContent>
            <div class="error-fallback">
                <h3>Something went wrong</h3>
                <p>@_errorMessage</p>
                <button @onclick="Reload">Reload</button>
            </div>
        </ErrorContent>
    </ErrorBoundary>
    ```
3.  **User-Friendly Messages**: Translate technical errors to user-friendly messages using LocalizationService.
4.  **Error Logging**: Ensure all caught exceptions are logged to Serilog with context (page, user, operation).

---

### [021] Missing Anti-Forgery Validation on State-Changing Endpoints
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
While anti-forgery is configured in Program.cs (lines 185-189), there's no visible application of `[AutoValidateAntiforgeryToken]` or `[ValidateAntiForgeryToken]` on state-changing API endpoints. This could expose:
- POST/PUT/DELETE requests to CSRF attacks
- OpenSubsonic endpoints (which use query params for auth) may be particularly vulnerable
- API endpoints called from Blazor UI might bypass CSRF checks if using cookie authentication

#### Action Items
1.  **Apply Anti-Forgery Attributes**: Add validation to all state-changing endpoints:
    ```csharp
    [HttpPost]
    [ValidateAntiForgeryToken] // or [AutoValidateAntiforgeryToken] at controller level
    public async Task<IActionResult> CreateAlbum([FromBody] CreateAlbumRequest request)
    {
        // ...
    }
    ```
2.  **Exclude Stateless APIs**: Configure anti-forgery to exclude stateless APIs:
    ```csharp
    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-CSRF-TOKEN";
        // Exclude APIs that don't use cookie auth
        options.SuppressXFrameOptionsHeader = false;
    });
    ```
3.  **Configure for OpenSubsonic/Jellyfin**: These APIs use custom authentication, so anti-forgery doesn't apply. Ensure they're excluded from validation.
4.  **Testing**: Add CSRF tests to verify endpoints reject requests without valid anti-forgery tokens.

---

### [022] Missing Request Validation for Jellyfin InstantMix Parameters
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Medium**
- **Status**: Open

#### Concerns
In `Jellyfin/ItemsController.cs`, the `GetInstantMixAsync` method accepts parameters like `limit`, `userId`, etc. with minimal validation:
- `limit` is clamped but may still accept extreme values before clamping
- No validation on `userId` format or existence
- Missing validation on other query parameters

This could lead to:
- Performance issues with unbounded queries
- Inconsistent error responses
- Potential DoS through parameter manipulation

#### Action Items
1.  **Add Parameter Validation**: Apply validation attributes to action parameters:
    ```csharp
    [HttpGet("{itemId}/instantmix")]
    public async Task<IActionResult> GetInstantMixAsync(
        string itemId,
        [FromQuery] [Range(1, 100)] int? limit = null,
        [FromQuery] [Range(1, 100)] int? userId = null,
        CancellationToken cancellationToken)
    ```
2.  **Custom Validation Filter**: Create a validation filter that returns JellyfinProblemDetails for validation errors:
    ```csharp
    public class JellyfinValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(x => x.Key, x => x.Value!.Errors[0].ErrorMessage);

                context.Result = new BadRequestObjectResult(new JellyfinProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join(", ", errors.Values),
                    ErrorCode = "VALIDATION_ERROR"
                });
            }
        }
    }
    ```
3.  **Test Edge Cases**: Add tests for boundary conditions and invalid parameters.

---

### [023] CORS Configuration Too Permissive
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **Low**
- **Status**: Open

#### Concerns
In `Program.cs` (lines 741-745), CORS is configured to allow all origins:
```csharp
app.UseCors(bb => bb
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("Accept-Ranges", "Content-Range", "Content-Length", "Content-Type"));
```

While this simplifies development and allows browser-based clients from any origin, it has security implications:
-   **CSRF Risk**: Combined with cookie-based authentication, this could enable CSRF attacks from malicious sites.
-   **Credential Leakage**: While `.AllowAnyOrigin()` prevents credentials by design, the overly permissive policy suggests security might not be a priority.
-   **Production Deployment**: This configuration is acceptable for development but dangerous in production environments where the application's origin is known.

#### Action Items
1.  **Environment-Specific CORS**: Implement different CORS policies for development vs. production:
    ```csharp
    if (app.Environment.IsDevelopment())
    {
        app.UseCors(bb => bb.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    }
    else
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
        app.UseCors(bb => bb.WithOrigins(allowedOrigins ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
    }
    ```
2.  **Document CORS Configuration**: Add to deployment documentation explaining how to configure `AllowedOrigins` for production deployments.
3.  **Named CORS Policies**: Consider defining named policies for different client types (web UI, mobile apps, third-party integrations) with appropriate restrictions for each.
4.  **CSRF Protection**: Verify that the antiforgery configuration (lines 185-189) is properly applied to state-changing endpoints that use cookie authentication.

---

### [024] API Documentation and OpenAPI Specification
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **Low**
- **Status**: Open

#### Concerns
The application has **partial** API documentation:
-   **Melodee Native API**: Has OpenAPI document and Scalar UI configured (lines 91-103, 496-507).
-   **OpenSubsonic API**: Controllers use XML documentation comments, but there's no unified OpenAPI spec. Developers must refer to external OpenSubsonic API documentation.
-   **Jellyfin API**: Controllers have `[ApiExplorerSettings(GroupName = "jellyfin")]` but no dedicated documentation UI. Developers must refer to external Jellyfin API documentation.
-   **Inconsistent Documentation**: Some endpoints have detailed XML comments, others have minimal or missing documentation.

This makes it difficult for:
-   **Integration Partners**: Third-party developers need to reverse-engineer or rely on external documentation.
-   **Internal Development**: New team members lack a single source of truth for API capabilities.
-   **Testing**: Incomplete documentation leads to untested edge cases.

#### Action Items
1.  **Complete XML Documentation**: Ensure all public controller methods have complete XML documentation with:
    -   Summary describing the endpoint's purpose
    -   Parameter descriptions for each input
    -   Response descriptions including status codes
    -   Example requests/responses where helpful
2.  **Generate Separate OpenAPI Specs**: Generate separate OpenAPI documents for each API:
    -   Add `[ApiExplorerSettings(GroupName = "melodee")]` to native Melodee controllers
    -   Update OpenAPI configuration to generate documents for all three groups
    ```csharp
    builder.Services.AddOpenApi("melodee", options => { /* config */ });
    builder.Services.AddOpenApi("jellyfin", options => { /* config */ });
    builder.Services.AddOpenApi("opensubsonic", options => { /* config */ });
    ```
3.  **Dedicated Documentation Page**: Create a landing page (`/docs`) that provides:
    -   Links to all three API documentation pages
    -   Quick start guides for each API
    -   Authentication guides
    -   SDKs and client libraries (if available)
4.  **API Versioning Documentation**: Document the API versioning strategy clearly. The application uses `Asp.Versioning` but it's not clear which endpoints are versioned or how clients should specify versions.
5.  **Postman/Insomnia Collections**: Generate and maintain Postman/Insomnia collections for common API workflows to help developers get started quickly.

---

### [025] Logging and Observability Improvements
- **Agent**: Antigravity
- **Date**: 2026-01-04
- **Risk Level**: **Low**
- **Status**: Open

#### Concerns
The application uses Serilog for logging (configured in `Program.cs` lines 67-68), but there are observability gaps:
-   **Inconsistent Log Levels**: Some controllers use `logger.LogDebug()`, others use `logger.LogInformation()`, with no clear guidelines.
-   **Missing Context**: Logs lack correlation IDs to trace requests across the multi-API architecture.
-   **Performance Metrics**: No structured performance logging for slow queries or endpoints.
-   **Security Events**: No dedicated logging for security events (failed authentication, authorization failures, suspicious activity).
-   **External Dependency Failures**: No structured logging for failures with external services (Spotify, MusicBrainz, Last.fm).

The Jellyfin API has `GetCorrelationId()` method in the base controller, but it's not clear if it's consistently used or propagated to logs.

#### Action Items
1.  **Implement Correlation IDs**: Ensure all three APIs generate and propagate correlation IDs:
    ```csharp
    // Middleware to add correlation ID
    app.Use(async (context, next) =>
    {
        if (!context.Request.Headers.ContainsKey("X-Correlation-ID"))
        {
            context.Request.Headers["X-Correlation-ID"] = Guid.NewGuid().ToString();
        }
        context.Response.Headers["X-Correlation-ID"] = context.Request.Headers["X-Correlation-ID"];
        
        using (LogContext.PushProperty("CorrelationId", context.Request.Headers["X-Correlation-ID"]))
        {
            await next();
        }
    });
    ```
2.  **Structured Logging Standard**: Create logging guidelines documenting:
    -   When to use each log level (Debug, Information, Warning, Error, Critical)
    -   Required properties for different event types
    -   Sensitive data handling (never log passwords, tokens, or full file paths)
3.  **Performance Logging**: Add structured performance logging for operations exceeding thresholds:
    ```csharp
    public class PerformanceLoggingFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, 
            ActionExecutionDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();
            var resultContext = await next();
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > 1000) // 1 second threshold
            {
                logger.LogWarning("Slow endpoint: {Controller}.{Action} took {ElapsedMs}ms",
                    context.RouteData.Values["controller"],
                    context.RouteData.Values["action"],
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
    ```
4.  **Security Event Logging**: Create a dedicated `SecurityLogger` class for authentication/authorization events:
    ```csharp
    public class SecurityLogger
    {
        public void LogFailedAuthentication(string apiName, string username, string ipAddress, string reason)
        {
            logger.LogWarning("AUTH_FAILED: API={Api} User={User} IP={IP} Reason={Reason}",
                apiName, username, ipAddress, reason);
        }
        
        public void LogSuspiciousActivity(string activity, string details)
        {
            logger.LogWarning("SUSPICIOUS: Activity={Activity} Details={Details}",
                activity, details);
        }
    }
    ```
5.  **Add Metrics Collection**: Integrate with a metrics system (Prometheus, Application Insights, etc.) to track:
    -   Request rate and latency per API/endpoint
    -   Database connection pool usage
    -   Cache hit/miss rates
    -   Authentication failures
6.  **Distributed Tracing**: For complex requests that span multiple services, consider adding support for OpenTelemetry distributed tracing.

---

### [026] Missing ConfigureAwait(False) in Some Controller Async Calls
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Low**
- **Status**: Open

#### Concerns
Inconsistency in async patterns across controllers. Most controller code properly uses `ConfigureAwait(false)`, but some methods don't. While ASP.NET Core controller execution is always on a synchronization context, consistent use of `ConfigureAwait(false)` is a best practice for:
- Performance (avoids unnecessary thread marshalling)
- Clarity (indicates the method doesn't need the synchronization context)
- Future-proofing (if code is moved to background services)

Examples found with 100+ uses in controllers, but gaps exist in some areas.

#### Action Items
1.  **Audit and Standardize**: Review all controller methods and ensure all `await` calls use `.ConfigureAwait(false)` where appropriate.
2.  **Code Style Guide**: Document async/await pattern for the team:
    -   Controllers/API: Always use `ConfigureAwait(false)` (already on sync context, but for consistency)
    -   Blazor Components: Use `ConfigureAwait(false)` for non-UI-critical operations
    -   Background Services: Always use `ConfigureAwait(false)`
3.  **Consider Analyzer**: Add a Roslyn analyzer like `Microsoft.CodeAnalysis.NetAnalyzers` to detect missing ConfigureAwait calls.

---

### [027] Inconsistent Response Headers and Security Headers
- **Agent**: opencode
- **Date**: 2026-01-04
- **Risk Level**: **Low**
- **Status**: Open

#### Concerns
Security headers are applied inconsistently across the application:
- **Static files** (Program.cs:527): Full CSP, X-Content-Type-Options, X-Frame-Options
- **API responses**: Applied via middleware (Program.cs:766-772), but may not cover all paths
- **OpenSubsonic responses**: Headers set in Controller.OnActionExecuting (lines 60-64), but may be incomplete
- **Jellyfin responses**: No global header configuration visible

This inconsistency could lead to:
- Some endpoints missing important security headers
- Different security postures across APIs
- Confusion for security audits

#### Action Items
1.  **Centralized Header Middleware**: Create a dedicated middleware for security headers:
    ```csharp
    public class SecurityHeadersMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            await next(context);
        }
    }
    ```
2.  **Apply Globally**: Register middleware early in the pipeline (after UseRouting, before UseEndpoints).
3.  **API-Specific Exclusions**: Allow API endpoints to customize or exclude certain headers if needed.
4.  **Content Security Policy**: Consider a more specific CSP that allows only necessary sources for UI and APIs.
