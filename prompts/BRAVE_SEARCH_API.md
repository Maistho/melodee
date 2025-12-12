# Brave Search API Image Plugins Specification

This document specifies how to add Brave Search API–backed image search plugins to Melodee. It is written for an automated coding agent: follow the steps and contracts exactly unless the repository conventions clearly require an adjustment.

## High-level goals

Implement Brave Search–based plugins that can fetch artist and album images using the Brave Search Images API and expose them through the existing Melodee plugin and search-engine abstractions.

Outcomes:

1. A reusable Brave Search HTTP client in `Melodee.Common`.
2. Concrete plugin implementations for:
   - `IArtistImageSearchEnginePlugin` (artist images)
   - `IAlbumImageSearchEnginePlugin` (album images; existing interface in repo)
3. Configuration and DI wiring for Brave (API key, base URL, enable/disable flag).
4. DTOs + mappers to convert Brave responses into Melodee `ImageSearchResult` models.
5. Unit tests for the client and plugins, and a simple integration/smoke path.

Keep changes focused and consistent with existing project patterns.

---

## 1. Discover existing patterns and types

Before implementing anything, the agent MUST scan the existing code to mirror conventions.

### 1.1. Identify existing image search plugins and models

1. Search for the following interfaces in `src/Melodee.Common`:
   - `IArtistImageSearchEnginePlugin` (already provided)
   - `IAlbumImageSearchEnginePlugin`
2. Inspect their namespaces and method signatures. Example (already known):

   - File: `src/Melodee.Common/Plugins/SearchEngine/IArtistImageSearchEnginePlugin.cs`
   - Namespace: `Melodee.Common.Plugins.SearchEngine`
   - Contract:
     - `Task<OperationResult<ImageSearchResult[]?>> DoArtistImageSearch(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default);`

3. Locate supporting models in `src/Melodee.Common/Models` (or subfolders):
   - `ArtistQuery`
   - `AlbumQuery`
   - `ImageSearchResult`
   - `OperationResult<T>`

4. Search for **existing search engine plugin implementations** to reuse patterns:
   - Grep for `ImageSearchEnginePlugin` in `src/Melodee.Common/Plugins`.
   - Look especially for classes that implement `IArtistImageSearchEnginePlugin` or `IAlbumImageSearchEnginePlugin` (e.g., Last.fm, Spotify, Deezer, etc.).

5. For at least one existing implementation, examine:
   - How the plugin class is named and where it lives.
   - How it builds search queries from `ArtistQuery` / `AlbumQuery`.
   - How `OperationResult<T>` is created on success/failure.
   - How results are normalized and deduplicated.
   - Any helper classes or base classes it reuses.

**Design rule:** The Brave plugins MUST follow the same structure and error-handling style as the most similar existing image-search plugin in `Melodee.Common`.

---

## 2. Brave Search HTTP client

Add a small, focused HTTP client that wraps Brave Search Images API. This client will be used by both artist and album plugins.

### 2.1. File and namespace

1. Create a new folder if it does not exist:
   - `src/Melodee.Common/SearchEngines/Brave/`
2. Create a new file:
   - `src/Melodee.Common/SearchEngines/Brave/BraveSearchClient.cs`
3. Namespace for all Brave types in `Melodee.Common`:
   - `namespace Melodee.Common.SearchEngines.Brave;`

### 2.2. Configuration model

1. In the same folder, add a configuration class file:
   - `src/Melodee.Common/SearchEngines/Brave/BraveSearchOptions.cs`
2. Define a simple POCO with the following properties and default behavior:

   - `public sealed class BraveSearchOptions`
     - `public string ApiKey { get; set; } = string.Empty;`
     - `public string BaseUrl { get; set; } = "https://api.search.brave.com";`
     - `public string ImageSearchPath { get; set; } = "/res/v1/images/search";`
     - `public bool Enabled { get; set; } = true;`

3. The client MUST be constructed with `IOptions<BraveSearchOptions>` or equivalent, depending on the project-wide configuration pattern (inspect other search engine option classes and copy their approach).

### 2.3. DTOs for Brave image search

1. In `src/Melodee.Common/SearchEngines/Brave/`, create a file:
   - `BraveImageSearchDtos.cs`
2. Define DTOs to match Brave’s image search JSON response.

   - Use Brave’s current documentation as guide; if not accessible, implement a minimal, extensible subset with case-insensitive deserialization.
   - Suggested model (adapt names to actual response fields):

     - `public sealed class BraveImageSearchResponse`
       - `public List<BraveImageResult> Results { get; set; } = new();`

     - `public sealed class BraveImageResult`
       - `public string? Title { get; set; }`
       - `public string? Url { get; set; }`           // direct image URL
       - `public string? ThumbnailUrl { get; set; }` // thumbnail URL if Brave provides one
       - `public string? Source { get; set; }`       // source site or domain
       - `public string? PageUrl { get; set; }`      // page hosting the image
       - Optionally: `Width`, `Height`, `ContentType` if fields exist and are useful.

3. Configure JSON options in the client to be **property name case-insensitive** to protect against minor field name differences.

### 2.4. `BraveSearchClient` design

1. Implement `BraveSearchClient` in `BraveSearchClient.cs` with:

   - Dependencies (constructor parameters):
     - `HttpClient httpClient`
     - `IOptions<BraveSearchOptions>` (or similar configuration object)
   - Private fields:
     - `_httpClient`
     - `_options`
     - `_jsonOptions` (for `System.Text.Json`)

2. Public methods (minimum):

   ```csharp
   Task<BraveImageSearchResponse?> SearchImagesAsync(
       string query,
       int count,
       CancellationToken cancellationToken = default);
   ```

3. Behavior of `SearchImagesAsync`:

   - Pre-conditions:
     - If `!_options.Value.Enabled`, return `null` (caller interprets as disabled/not available).
     - If `string.IsNullOrWhiteSpace(_options.Value.ApiKey)`, throw an `InvalidOperationException` with a clear message or log and return `null` according to the existing pattern for other APIs (check similar clients in the repo).
   - Build the URI:
     - Base URL from `_options.Value.BaseUrl`.
     - Path from `_options.Value.ImageSearchPath`.
     - Query-string params:
       - `q` = `query`
       - `count` = clamped integer:
         - `count = Math.Max(1, Math.Min(count, 50))` or use Brave’s documented max.
       - Optionally: `safe=active` or equivalent for safe search if recommended.
   - Create `HttpRequestMessage`:
     - Method: `HttpMethod.Get`.
     - URL: built URI.
     - Headers:
       - `Accept: application/json`
       - `X-Subscription-Token: <ApiKey>` (per Brave docs).
   - Send request via `_httpClient.SendAsync` with `cancellationToken`.
   - Handle timeout or cancellation exceptions appropriately (consistent with existing HTTP clients in the repo):
     - Let `OperationCanceledException` bubble up.
     - For `HttpRequestException`, log or capture error per existing pattern.
   - On non-success HTTP status codes:
     - Option A (if other clients use `OperationResult` inside client): create a failure structure.
     - Option B (simpler): return `null` and let plugins decide how to surface the problem.
     - Follow the existing pattern of whichever HTTP client is closest in `Melodee.Common`.
   - On success:
     - Use `System.Net.Http.Json` or `JsonSerializer.Deserialize` to parse response content into `BraveImageSearchResponse`.
     - Return the parsed object.

4. The client MUST NOT depend on any Blazor or CLI-specific services; it belongs in pure `Melodee.Common` with only .NET and `Microsoft.Extensions.*` dependencies that the project already uses.

---

## 3. Mapping Brave results to `ImageSearchResult`

Create a mapper that converts raw Brave DTOs into the normalized `ImageSearchResult` model used by Melodee.

### 3.1. Identify `ImageSearchResult`

1. Locate `ImageSearchResult` in `src/Melodee.Common/Models/SearchEngines` (or similar).
2. Inspect its properties and any helper methods (e.g., equality, deduping, ranking hints).

### 3.2. Create mapper class

1. Create a new file:
   - `src/Melodee.Common/SearchEngines/Brave/BraveImageMapper.cs`
2. Namespace: `Melodee.Common.SearchEngines.Brave`.
3. Define a static mapper class with methods:

   ```csharp
   internal static class BraveImageMapper
   {
       public static ImageSearchResult? ToImageSearchResult(BraveImageResult source);

       public static ImageSearchResult[] MapResults(IEnumerable<BraveImageResult> results, int maxResults);
   }
   ```

4. Mapping rules for `ToImageSearchResult`:

   - If `source.Url` is `null` or empty, return `null` (skip entries without a main image URL).
   - Populate `ImageSearchResult` properties as follows (adapt to actual `ImageSearchResult` definition):
     - `Title`  `source.Title` (fallbacks allowed, e.g., from domain and filename if needed).
     - `ImageUrl`  `source.Url`.
     - `ThumbnailUrl`  `source.ThumbnailUrl` if present.
     - `SourceSite`  hostname extracted from `source.PageUrl` or `source.Source` if available.
     - `PageUrl`  `source.PageUrl`.
     - Any other relevant fields (dimensions, mime type) if present in `ImageSearchResult`.
   - Keep mapping purely data-oriented; no HTTP calls.

5. `MapResults` behavior:

   - Take the input `IEnumerable<BraveImageResult>`.
   - Map each through `ToImageSearchResult`.
   - Filter out `null` results.
   - Deduplicate entries based on `ImageUrl` (case-insensitive). Use LINQ `GroupBy` on the normalized URL.
   - Order by heuristics if desired (e.g., keep original order returned by Brave; do not try to be clever unless the repo has a shared ranking strategy; match existing plugin behavior if any).
   - Truncate to `maxResults`.
   - Return as an array `ImageSearchResult[]`.

---

## 4. Brave-based artist image search plugin

Implement a Brave-backed plugin that fulfills `IArtistImageSearchEnginePlugin`.

### 4.1. File and namespace

1. Create a new file:
   - `src/Melodee.Common/SearchEngines/Brave/BraveArtistImageSearchEnginePlugin.cs`
2. Namespace: `Melodee.Common.SearchEngines.Brave`.

### 4.2. Class declaration and dependencies

1. Implement `IArtistImageSearchEnginePlugin` and any base interfaces it requires (e.g., `IPlugin`).
2. Class signature:

   ```csharp
   public sealed class BraveArtistImageSearchEnginePlugin : IArtistImageSearchEnginePlugin
   {
       // ...
   }
   ```

3. Constructor dependencies:

   - `BraveSearchClient braveClient`
   - Optionally: a logger (e.g., `ILogger<BraveArtistImageSearchEnginePlugin>`) if logging is common in other plugins.

4. Required members (based on `IPlugin` conventions):

   - Implement `Name` and `Description` (or similar properties/methods) consistent with `IPlugin`:
     - `Name`  "BraveArtistImageSearch".
     - `Description`  "Uses Brave Search Images API to find artist images.".

### 4.3. Implement `DoArtistImageSearch`

Method signature from interface:

```csharp
public Task<OperationResult<ImageSearchResult[]?>> DoArtistImageSearch(
    ArtistQuery query,
    int maxResults,
    CancellationToken cancellationToken = default);
``

Behavior:

1. Input validation:
   - If `query` is `null`, immediately return a failed `OperationResult` with a clear error message, consistent with existing plugins.
   - If `string.IsNullOrWhiteSpace(query.Name)` (or whichever `ArtistQuery` property holds the artist name), return an `OperationResult` failure stating that the artist name is required.
   - Clamp `maxResults` to a positive range (e.g., 1 to 20) consistent with other plugins.

2. Build Brave search query string:

   - Define a private helper method:

     ```csharp
     private static string BuildArtistSearchText(ArtistQuery query)
     ```

   - Behavior:
     - Start with the artist name.
     - Optionally include other metadata if available (e.g., genre, country), but keep it simple and deterministic.
     - Append words that bias results towards portraits/artist photos, e.g.: `"<ArtistName> musician portrait"` or `"<ArtistName> band photo"`.
     - Ensure the string is trimmed.

3. Call Brave client:

   - Use `BraveSearchClient.SearchImagesAsync(searchText, maxResults, cancellationToken)`.
   - If it returns `null`, interpret it as either an error or disabled state (follow the repo’s conventions):
     - For example, return `OperationResult<ImageSearchResult[]?>.Ok(Array.Empty<ImageSearchResult>())` with optional metadata indicating that the source is unavailable, or a failure `OperationResult` if that is the usual convention for connection problems.
   - If a response is returned but `Results` is empty or `null`, return success with an empty array.

4. Map and return results:

   - Pass `response.Results` and `maxResults` into `BraveImageMapper.MapResults`.
   - Wrap in an `OperationResult` success:

     - `return OperationResult<ImageSearchResult[]?>.Ok(mappedArray);`

   - Ensure that `OperationResult` is used consistently with other plugins; check how success/failure static helpers are named (`Ok`, `Success`, `Fail`, `Error`, etc.) and match.

5. Error handling:

   - Catch transient network exceptions only if other plugins do, and convert to a failure `OperationResult` with a descriptive message and/or error code.
   - Let `OperationCanceledException` propagate.

---

## 5. Brave-based album image search plugin

Add an album plugin whose behavior mirrors the artist implementation but uses album and artist names to drive the search query.

### 5.1. Identify `IAlbumImageSearchEnginePlugin`

1. Locate the interface file in `src/Melodee.Common/Plugins/SearchEngine` (or similar):
   - E.g., `IAlbumImageSearchEnginePlugin.cs`.
2. Confirm its method signature, which should be similar to:

   ```csharp
   Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(
       AlbumQuery query,
       int maxResults,
       CancellationToken cancellationToken = default);
   ```

### 5.2. File and namespace

1. Create a new file:
   - `src/Melodee.Common/SearchEngines/Brave/BraveAlbumImageSearchEnginePlugin.cs`
2. Namespace: `Melodee.Common.SearchEngines.Brave`.

### 5.3. Class declaration and dependencies

1. Implement `IAlbumImageSearchEnginePlugin`.
2. Class signature:

   ```csharp
   public sealed class BraveAlbumImageSearchEnginePlugin : IAlbumImageSearchEnginePlugin
   {
       // ...
   }
   ```

3. Constructor dependencies:

   - `BraveSearchClient braveClient`
   - Optional logger (`ILogger<BraveAlbumImageSearchEnginePlugin>`) if consistent with the codebase.

4. Implement required plugin metadata fields from `IPlugin`.

   - `Name`  "BraveAlbumImageSearch".
   - `Description`  "Uses Brave Search Images API to find album cover images.".

### 5.4. Implement `DoAlbumImageSearch`

Behavior:

1. Input validation:
   - Ensure `query` is non-null.
   - Ensure album title (likely `query.Title` or similar) is non-empty; otherwise return a failure `OperationResult`.
   - Clamp `maxResults` similarly to the artist plugin.

2. Build Brave search query:

   - Add a private method:

     ```csharp
     private static string BuildAlbumSearchText(AlbumQuery query)
     ```

   - Behavior:
     - Start with album title.
     - If artist name is available (e.g., `query.ArtistName` or `query.Artist?.Name`), prepend/append it.
     - Append bias words like `"album cover"` to encourage cover art images.
     - Example patterns:
       - If artist present: `"<ArtistName> <AlbumTitle> album cover"`.
       - Else: `"<AlbumTitle> album cover"`.

3. Invoke Brave:

   - Use `BraveSearchClient.SearchImagesAsync(searchText, maxResults, cancellationToken)`.
   - Handle `null` responses and empty `Results` the same way as in the artist plugin.

4. Map results and return:

   - Use `BraveImageMapper.MapResults(response.Results, maxResults)`.
   - Wrap in a successful `OperationResult`.

5. Ensure behavior (success vs. failure on HTTP issues) mirrors the artist plugin exactly.

---

## 6. Configuration and dependency injection wiring

Wire up Brave Search so that applications (Blazor, CLI, worker, etc.) can opt in easily via configuration and dependency injection.

### 6.1. Configuration sources

1. Identify the central configuration mechanism:
   - Look in `src/Melodee.Blazor`, `src/Melodee.Cli`, and any shared configuration classes for patterns like `services.Configure<SomeOptions>(configuration.GetSection("SomeSection"));`.
2. Decide on a configuration section name for Brave:
   - Recommended: `"BraveSearch"`.

3. Update the relevant appsettings or environment examples:

   - For docs/examples only (do not hard-code secrets):
     - In `example.env` or relevant config docs, add keys:
       - `BRAVE_SEARCH__APIKEY` (or `BraveSearch__ApiKey`, depending on project’s naming convention for env binding).
       - `BRAVE_SEARCH__ENABLED` (optional, default `true`).

### 6.2. DI registration extension

1. In `src/Melodee.Common/SearchEngines/Brave/`, create a registration helper:
   - File: `BraveSearchServiceCollectionExtensions.cs`.
   - Namespace: `Melodee.Common.SearchEngines.Brave`.

2. Define an extension method following the style of other search engine registration helpers:

   ```csharp
   public static class BraveSearchServiceCollectionExtensions
   {
       public static IServiceCollection AddBraveSearch(this IServiceCollection services, IConfiguration configuration)
       {
           // Configure BraveSearchOptions from configuration
           // Register BraveSearchClient (typed HttpClient)
           // Register BraveArtistImageSearchEnginePlugin & BraveAlbumImageSearchEnginePlugin as their respective interfaces
       }
   }
   ```

3. Implementation details:

   - Use `services.Configure<BraveSearchOptions>(configuration.GetSection("BraveSearch"));` or whichever pattern is standard.
   - Register an `HttpClient` for `BraveSearchClient`:

     - Use `services.AddHttpClient<BraveSearchClient>()`.
     - If other clients set a global timeout, match it (e.g., 5–10 seconds).

   - Register plugins:
     - `services.AddSingleton<IArtistImageSearchEnginePlugin, BraveArtistImageSearchEnginePlugin>();`
     - `services.AddSingleton<IAlbumImageSearchEnginePlugin, BraveAlbumImageSearchEnginePlugin>();`

4. In application startup code (Blazor, worker, etc.), call this extension when search/image features are enabled:

   - For each relevant app, locate the main DI configuration point (`Program.cs` or equivalent) and add something like:

     ```csharp
     services.AddBraveSearch(configuration);
     ```

   - Protect this behind configuration or feature flags if the repository uses such patterns.

---

## 7. Testing and validation

### 7.1. Unit tests for `BraveSearchClient`

1. Create test files under `tests/Melodee.Tests.Common` (or appropriate test project):

   - `tests/Melodee.Tests.Common/SearchEngines/Brave/BraveSearchClientTests.cs`

2. Use an HTTP mocking approach consistent with the test project (e.g., `HttpMessageHandler` stubs, `RichardSzalay.MockHttp` if already in use, or a simple in-memory handler implementation).

3. Test scenarios:

   - **Builds correct URL and headers**:
     - Verify `q` and `count` query parameters.
     - Verify `X-Subscription-Token` header is set to the configured API key.
   - **Respects `Enabled` flag**:
     - When `Enabled` is `false`, the method returns `null` without performing a request.
   - **Handles success response**:
     - Mock a `200 OK` response with JSON containing multiple image results.
     - Verify deserialization populates `BraveImageSearchResponse.Results` correctly.
   - **Handles non-success status codes**:
     - Mock `500` or `403` and verify behavior matches the chosen pattern (e.g., `null` result, logged error, etc.).

### 7.2. Unit tests for `BraveImageMapper`

1. Create test file:

   - `tests/Melodee.Tests.Common/SearchEngines/Brave/BraveImageMapperTests.cs`

2. Test cases:

   - **Null/empty URL is skipped**:
     - A `BraveImageResult` without a `Url` yields `null` from `ToImageSearchResult` and is omitted by `MapResults`.
   - **Basic mapping**:
     - A populated result maps to `ImageSearchResult` with correct fields.
   - **Deduplication**:
     - Two Brave results with the same image URL (case-insensitive) yield only one `ImageSearchResult`.
   - **Max results respected**:
     - `MapResults` truncates to `maxResults`.

### 7.3. Unit tests for Brave plugins

1. Create test files:

   - `tests/Melodee.Tests.Common/SearchEngines/Brave/BraveArtistImageSearchEnginePluginTests.cs`
   - `tests/Melodee.Tests.Common/SearchEngines/Brave/BraveAlbumImageSearchEnginePluginTests.cs`

2. Use a mocking framework already present in the test project (e.g., Moq, NSubstitute) to mock `BraveSearchClient`.

3. Scenarios for **artist** plugin:

   - **Null query**: returns failure `OperationResult`.
   - **Empty artist name**: returns failure `OperationResult` with specific message.
   - **Brave disabled or returns `null`**: returns success with empty image list or failure according to convention.
   - **No results**: Brave sends empty result list; plugin returns success with empty array.
   - **Valid results**: Brave returns 3+ items; plugin returns mapped images with length  min(`maxResults`, count of valid images).
   - **Search text composition**: verify `BuildArtistSearchText` is called or test indirectly by asserting the query passed to `BraveSearchClient` matches expected pattern; this may require exposing the search text builder or using test-only injection.

4. Scenarios for **album** plugin (analogous to artist tests):

   - **Null query** / **missing title**  failures.
   - **With and without artist name**: ensure search query differs (artist included when available).
   - **No results** and **valid results** as above.

### 7.4. Optional integration/smoke test

1. If the repo has integration test infrastructure and allows network calls in a specific suite, add an opt-in integration test that:
   - Reads a real Brave API key from a dedicated environment variable (e.g., `MELODEE_E2E_BRAVE_API_KEY`).
   - Skips if the key is not present.
   - Calls the Brave artist plugin for a well-known artist (e.g., "The Beatles") and asserts that at least one image is returned.
2. Keep this test explicitly marked as integration/e2e and not part of the default unit-test run, if that’s the project policy.

---

## 8. Documentation updates

1. Update `README.md` or a relevant docs page (under `docs/` if appropriate) to mention Brave Search as an optional image provider:
   - Briefly describe that Melodee can use Brave Search API for artist and album images.
   - Document the configuration keys (`BraveSearch:ApiKey`, etc.).
   - Mention any rate limits or usage constraints (linking to Brave’s terms of service if appropriate).

2. If there is a central plugin registry or table in docs, add entries:

   - `BraveArtistImageSearchEnginePlugin`  *Artist images via Brave Search*.
   - `BraveAlbumImageSearchEnginePlugin`  *Album cover images via Brave Search*.

---

## 9. Implementation checklist (for the coding agent)

The coding agent MUST implement the following, in roughly this order:

1. **Scan existing code**: identify image search models, plugin interfaces, and a comparable search engine plugin implementation.
2. **Create Brave option class**: `BraveSearchOptions` in `src/Melodee.Common/SearchEngines/Brave/`.
3. **Create Brave DTOs**: `BraveImageSearchResponse`, `BraveImageResult` in `BraveImageSearchDtos.cs`.
4. **Create Brave HTTP client**: `BraveSearchClient` with `SearchImagesAsync`.
5. **Create mapper**: `BraveImageMapper` to convert `BraveImageResult` -> `ImageSearchResult` and perform deduplication.
6. **Implement artist plugin**: `BraveArtistImageSearchEnginePlugin` implementing `IArtistImageSearchEnginePlugin`.
7. **Implement album plugin**: `BraveAlbumImageSearchEnginePlugin` implementing `IAlbumImageSearchEnginePlugin`.
8. **Add DI extension**: `BraveSearchServiceCollectionExtensions.AddBraveSearch` to register options, client, and plugins.
9. **Wire DI in apps**: call `AddBraveSearch` in appropriate startup locations.
10. **Add tests**: client tests, mapper tests, and plugin tests under `tests/Melodee.Tests.Common/.../Brave/`.
11. **Optional**: Add an integration test with a real API key gated by an environment variable.
12. **Update docs**: mention Brave Search in the README or docs and update `example.env` to include configuration examples.

When all of the above are implemented and tests pass, the Brave Search API image plugins will be fully integrated into Melodee.
