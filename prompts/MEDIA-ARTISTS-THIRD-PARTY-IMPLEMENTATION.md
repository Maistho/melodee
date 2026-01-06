# Implementation Plan: Third-Party Artist Search Integration

## Overview
This plan outlines the phases and tasks required to implement a reusable search dialog for third-party artist identification on the `/media/artist` page. The solution will enable users to search for artists across multiple search providers (AMG, MusicBrainz, Discogs, etc.) and link their identifiers to artists in the Melodee database.

**Key constraint (reuse-first):** Melodee already has an `ArtistSearchEngineService` with a plugin model (`IArtistSearchEnginePlugin`) and existing provider implementations (e.g., MusicBrainz, Spotify, and Melodee). This feature should **extend and reuse** that search engine rather than introducing a parallel “third-party search” stack.

### Goals / Non-Goals
- **Goal:** Provide UX to help admins/operators pick the correct third-party identifier(s) for an artist and persist them.
- **Goal:** Reuse `ArtistSearchEngineService` orchestration, normalization, caching/coalescing, and provider implementations.
- **Goal:** Ensure the `/media/artist` UI only allows lookups against providers that are enabled by configuration (disabled providers must have their lookup UI disabled/hidden).
- **Non-goal (initial):** Bulk matching, scheduled sync, or full “rich profile” ingestion.

### Design contract (minimal, UI-friendly)
- **Input:** artist name (string) + optional provider filter.
- **Output:** list of candidates, each with explicit provenance (`Provider`) and IDs (`MusicBrainzId`, `DiscogsId`, etc.).
- **Behavior:** partial failures should not fail the entire dialog; providers can be enabled/disabled via existing configuration.
- **Behavior:** partial failures should not fail the entire dialog.
- **Behavior (provider enablement):** all provider selection is **deny-by-default**; only providers enabled via configuration may be queried or shown.
- **Behavior (UI):** the `/media/artist` page must disable (or hide) provider-specific lookup buttons when the corresponding provider/search engine is disabled.

---

## Phase 1: Backend – Extend the existing search engine (no new parallel interfaces)

### Task 1.1: Use existing `ArtistSearchEngineService` and plugin model
**Goal:** Route dialog searches through `ArtistSearchEngineService` and existing plugins.

**What exists today (reuse):**
- `ArtistSearchEngineService.InitializeAsync(...)` wires up engine plugins (Melodee + MusicBrainz + Spotify).
- Plugin contract: `IArtistSearchEnginePlugin.DoArtistSearchAsync(ArtistQuery, int maxResults, CancellationToken)`.
- Plugins implement `IPlugin` which already provides stable identifiers:
  - `Id` (stable key)
  - `DisplayName` (human-friendly)
- Engine performs normalization and merges IDs across providers (`ArtistSearchResult` already carries `AmgId`, `DiscogsId`, `MusicBrainzId`, `SpotifyId`, etc.).
- Results already include UI-friendly fields you can reuse:
  - provenance: `ArtistSearchResult.FromPlugin`
  - `ImageUrl` / `ThumbnailUrl`

**Plan update (replace prior “IThirdPartyArtistSearchService/Aggregator”):**
- Do **not** create `IThirdPartyArtistSearchService`, `SearchRequest`, `ArtistMatch`, or `ThirdPartySearchAggregator`.
- Instead, add a small API-layer DTO that adapts existing `ArtistSearchResult` into a stable response for the dialog.

**Deliverables (API DTOs only; no new domain abstractions):**
```csharp
public sealed class ArtistLookupRequest
{
    public required string ArtistName { get; init; }
    public int Limit { get; init; } = 10;

    // Optional: if provided, filter the engine to a subset of providers.
    // Prefer using IPlugin.Id values for stability (DisplayName can change).
    public string[]? ProviderIds { get; init; }
}

public sealed class ArtistLookupCandidate
{
    // Provenance is required so the UI can show where a match came from.
    // This should be derived from ArtistSearchResult.FromPlugin (typically plugin DisplayName).
    public required string ProviderDisplayName { get; init; }

    // Optional but recommended for round-tripping provider filters.
    public string? ProviderId { get; init; }

    public required string Name { get; init; }
    public string? SortName { get; init; }

    // Optional, UI-only convenience.
    public string? ImageUrl { get; init; }
    public string? ThumbnailUrl { get; init; }

    // Known IDs that can be linked to the Artist entity.
    public Guid? MusicBrainzId { get; init; }
    public string? SpotifyId { get; init; }
    public string? DiscogsId { get; init; }
    public string? AmgId { get; init; }
    public string? WikiDataId { get; init; }
    public string? ItunesId { get; init; }
    public string? LastFmId { get; init; }
}
```

> NOTE: Keep the DTOs tight and typed. Avoid `Dictionary<string, object>` for metadata. When implementing, follow self-explanatory code commenting guidelines: explain WHY for complex logic, avoid obvious comments.

### Task 1.2: Provider coverage – add plugins only if missing
**Goal:** Only implement new provider integrations if they don’t already exist as `IArtistSearchEnginePlugin` implementations.

**Tasks:**
- Confirm which providers already exist:
  - ✅ MusicBrainz is already integrated via `MusicBrainzArtistSearchEnginePlugin`.
  - ✅ Spotify is already integrated via the `Spotify` plugin.
  - ✅ The local Melodee plugin exists (`MelodeeArtistSearchEnginePlugin`).
- Before implementation, verify existing plugins via code search to avoid assumptions.
- For any missing providers (e.g., Discogs/AMG if not already implemented as plugins):
  - Add them as new `IArtistSearchEnginePlugin` implementations.
  - Respect provider policy (timeouts, rate limiting, user-agent, retries).

**Security/OWASP requirements (provider HTTP):**
- No secrets in code; use configuration/env vars.
- Enforce HTTPS.
- Set strict timeouts.
- Cap response sizes.
- Avoid server-side fetching of arbitrary URLs returned by providers (SSRF hardening).

### Task 1.3: Engine orchestration tweaks (only if needed for the dialog)
**Goal:** Ensure the existing engine can support “show candidates” scenarios.

The current engine has a “stop early when a higher-priority provider returns results” optimization to avoid rate-limited calls.
That’s good for background enrichment, but the dialog may want either:
- **Mode A (fastest):** stop early and show candidates from the first successful provider.
- **Mode B (compare):** query multiple providers and show provenance per candidate.

**Recommendation:** Add a dialog-oriented mode that still reuses plugins but can optionally continue past the first hit.
- Default to **Mode A**.
- Add a UI toggle for “Search other providers” later if needed.

Do this by extending `ArtistSearchEngineService` (or adding a new method) rather than creating an external aggregator.

**Performance Note:** Tie into performance optimization instructions by avoiding N+1 queries in persistence; monitor for memory leaks in long-running searches; ensure efficient algorithms for result merging.

### Task 1.4: API endpoint (thin wrapper over the engine)
**Goal:** Provide a controller endpoint the UI can call.

**Tasks:**
- Add a controller under an existing API area used by `/media/artist`.
- Validate inputs (trim, max length, limit range).
- Authorize access (admin/editor role as appropriate).
- Call `ArtistSearchEngineService.DoSearchAsync(new ArtistQuery { Name = request.ArtistName }, request.Limit, ct)`.
- If `ProviderIds` are provided, treat them strictly as a *narrowing filter* over the **enabled** provider set (intersection). A request must not be able to force-enable or query a disabled provider.
- If provider filtering is required, implement it by filtering the plugin set inside `ArtistSearchEngineService` (preferred), rather than filtering results after the fact.
- Map search results into `ArtistLookupCandidate[]`:
  - `ProviderDisplayName` from `ArtistSearchResult.FromPlugin`
  - `ImageUrl` / `ThumbnailUrl` from the result
- Handle errors gracefully: Return 500 for engine failures with logging (no sensitive details); handle timeouts and partial failures without failing the entire request.

**API Design Note:** POST is acceptable for the request body, but consider GET with query params for simplicity if the payload is small. Ensure the route aligns with existing API conventions (e.g., versioning).

**Deliverable (shape only):**
```csharp
[ApiController]
[Route("api/artist-lookup")]
public sealed class ArtistLookupController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ArtistLookupCandidate[]>> Lookup([FromBody] ArtistLookupRequest request,
        CancellationToken cancellationToken)
    {
        // Authorization
        // Validation
        // Call ArtistSearchEngineService
        // Map to DTOs
        // Return
    }
}
```

> If you prefer to keep the old path style, use `api/media/artist/lookup` but keep the implementation a thin engine wrapper.

---

## Phase 2: Frontend – Reusable dialog (UX improvements + provenance)

### Task 2.1: Create reusable search dialog component
**Goal:** Build a dialog that calls the new engine-backed lookup endpoint and lets the user select one candidate.

**Key updates vs prior draft:**
- **Do not trigger network calls on `onBlur`.** Blur happens during selection and can cause accidental re-search.
- Use *debounce on input* + *explicit Search button* + *Enter key triggers search*.
- Cancel in-flight requests via `AbortController` to avoid out-of-order updates.
- Show provenance:
  - `providerDisplayName` (e.g., “Spotify Service”, “MusicBrainz …”)
  - which IDs will be set (`musicBrainzId`, `discogsId`, etc.)
- Ensure accessibility: Follow markdown instructions for ARIA labels, keyboard navigation, semantic HTML.

**Deliverable (high-level React/TS sketch):**
```typescript
interface ArtistLookupCandidate {
  provider: string;
  name: string;
  sortName?: string;
  musicBrainzId?: string;
  spotifyId?: string;
  discogsId?: string;
  amgId?: string;
  wikiDataId?: string;
  itunesId?: string;
  lastFmId?: string;
}

// UI should display the provider and the ID(s) that would be applied.
```

### Task 2.2: Integrate dialog into `/media/artist`
**Goal:** Add a lookup button next to third-party ID fields.

**Provider enablement requirement (UI):**
- Each provider-specific lookup button must reflect the provider’s enabled/disabled state from configuration.
- If the underlying provider/search engine is disabled, the corresponding button must be disabled (or not rendered).
- The dialog’s provider filter options must only include enabled providers.

**Important reuse note:** The engine already merges IDs across providers. Selecting a single candidate might provide multiple IDs.
- The UI should show which fields will be populated if the candidate is chosen.
- The save action should update the relevant artist fields consistently (do not silently overwrite unrelated IDs).

---

## Phase 3: Database & persistence (verify what already exists)

### Task 3.1: Verify Artist model already supports third-party IDs
**Goal:** Avoid schema changes if the IDs already exist.

In `ArtistSearchEngineService`’s internal model we already see fields like:
- `AmgId`, `DiscogsId`, `MusicBrainzId`, `SpotifyId`, `WikiDataId`, `ItunesId`, `LastFmId`

**Tasks:**
- Confirm the main Melodee `Artist` entity already has these fields.
- If not, add only the missing fields (avoid introducing a JSON blob unless there’s a clear requirement).

### Task 3.2: Persist updates safely
**Goal:** Provide/update the existing endpoint used by `/media/artist` editing.

**Requirements:**
- Validate uniqueness constraints where required (e.g., a `MusicBrainzId` shouldn’t link to multiple artists).
- Use optimistic concurrency if supported by the existing API.
- Audit/log changes.
- Check for optimistic concurrency tokens to prevent race conditions during edits.

---

## Phase 4: Testing & validation

### Task 4.1: Test all provider integrations
**Goal:** Ensure all configured providers return results as expected.

**Tasks:**
- For each provider:
  - Validate the search dialog shows the provider in the UI.
  - Confirm results include expected IDs (e.g., `MusicBrainzId` from MusicBrainz).
  - Check that disabled providers do not appear in the results.
  - Check that `/media/artist` provider-specific lookup buttons are disabled/hidden when the provider is disabled.
  - Verify error handling: Introduce timeouts or errors for a provider to ensure the UI handles it gracefully.

### Task 4.2: Test persistence and UI updates
**Goal:** Validate that selecting a candidate and saving updates the artist entity correctly.

**Tasks:**
- Search for an artist with known IDs across providers.
- Select a candidate and save.
- Confirm the artist entity has the correct IDs populated.
- Check that other unrelated fields are not modified.

---

## Phase 5: Metrics & monitoring

### Task 5.1: Add metrics for the lookup endpoint
**Goal:** Track usage and performance of the new lookup feature.

**Additions:**
- Log the lookup endpoint usage (count, latency, provider filter used).
- Track failures per provider.
- Add metrics for success rates per provider and average response times.

### Task 5.2: Monitor and optimize
**Goal:** Ensure the feature performs well in production and identify any issues.

**Tasks:**
- Monitor logs for errors or performance issues.
- Optimize any slow-performing queries or integrations.
- Review metric trends and adjust resources or configurations as needed.

---

## Phase 6: Documentation & training

### Task 6.1: Update system documentation
**Goal:** Ensure all changes are reflected in the system documentation.

**Tasks:**
- Update architecture diagrams to include the new dialog and API endpoint.
- Document the configuration options for enabling/disabling providers.
- Ensure API documentation is up-to-date with the new endpoint and request/response formats.

### Task 6.2: Train support and operations teams
**Goal:** Ensure teams are aware of the new feature and how to support it.

**Tasks:**
- Conduct a walkthrough of the new feature for relevant teams.
- Provide documentation and training materials.
- Update any operational runbooks or monitoring dashboards.
