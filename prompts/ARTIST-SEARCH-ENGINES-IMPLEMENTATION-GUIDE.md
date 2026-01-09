# Artist Search Engines – Phased Implementation Guide (Agent Runbook)

**Source Plan**: `prompts/ARTIST-SEARCH-ENGINES.md`

This guide is written to minimize decision-making for coding agents. Follow the phases in order and check off items as you complete them.

---

## Phase Map (Progress)

- [x] Phase 0 — Baseline + conventions alignment
- [x] Phase 1 — Shared infrastructure (validation, HTTP clients, retry, throttling, caching helpers)
- [x] Phase 2 — Provider plugins (Discogs, iTunes, Last.fm, WikiData)
- [x] Phase 3 — AMG lookup UI integration (via injected ArtistSearchEngineService)
- [x] Phase 4 — Settings + admin UI wiring (enable flags + provider settings)
- [x] Phase 5 — Tests (unit + fixtures, no live API in CI)
- [x] Phase 6 — Documentation + final verification

-- 

## Coding agent template
```.aiignore
You are a coding agent working in /home/steven/source/melodee. Your task is to COMPLETE ONE PHASE 
from prompts/ARTIST-SEARCH-ENGINES-IMPLEMENTATION-GUIDE.md: **Phase 6 — Documentation + final verification** (and only that phase), 
meeting its Exit Criteria.

 Requirements (follow exactly; do not bikeshed):
 1) Read prompts/ARTIST-SEARCH-ENGINES-IMPLEMENTATION-GUIDE.md and implement every checklist item in Phase 6 in order.
 2) Follow the guide’s Global Defaults:
    - Timeout: 10s per request
    - Use SettingRegistry.SearchEngineUserAgent (searchEngine.userAgent) as User-Agent on all external provider requests
    - Retry with Polly: retry network exceptions, HTTP 5xx, HTTP 429 (prefer Retry-After), 3 retries, exponential 2^n seconds; never retry other 4xx
    - Per-provider SemaphoreSlim throttling: Discogs 2, iTunes 5, Last.fm 2, WikiData 2
    - Caching: ICacheManager.GetAsync(key, factory, token, duration, region: ServiceBase.CacheName); new providers default TTL 2h; keep existing TTLs
    - Secrets: never log; prefer env var overrides (underscore-to-dot mapping) instead of storing secrets in DB
    - Logging hygiene: sanitize user-controlled strings (LogSanitizer.Sanitize)
    - Validation: reject empty/whitespace; trim; cap length 256; URL/query encode params; AMG ID digits-only
 3) Make the smallest possible code changes; reuse existing patterns and plugins referenced in Phase 0 (AlbumImageSearchEngineService, ITunesSearchEngine, LastFm, ArtistSearchEngineService.InitializeAsync).
 4) If the phase involves settings seed data, modify the EF Core model/seed and generate a NEW migration (never edit existing migrations).
 5) If the phase involves Blazor UI, inject and call services directly (no internal HTTP calls) and localize any new user-facing strings.
 6) Tests:
    - If Phase 6 includes tests, add deterministic tests using mocked HTTP responses; do NOT call live external APIs in CI.

 Validation:
 - Run the relevant verification commands for your phase (at minimum: `dotnet test` for impacted projects).
 - Confirm the phase Exit Criteria items are satisfied, and list them with evidence (files changed, key methods added, tests run + results).

 Deliverable:
 - A brief summary of what changed, with a bullet list of modified/added files and how to manually verify the phase behavior.
 - Update prompts/ARTIST-SEARCH-ENGINES-IMPLEMENTATION-GUIDE.md marking the phase as COMPLETED ('[x]') when satisfied.
```

---

## Global Defaults (Do not bikeshed)

### Networking
- **Timeout (all providers)**: 10 seconds per request.
- **User-Agent**:
  - Use the existing setting `SettingRegistry.SearchEngineUserAgent` (`searchEngine.userAgent`).
  - Send it on all external provider requests; Discogs requires it.

### Retry + rate limits
- Use Polly.
- Retry rules:
  - Retry **network exceptions**, **HTTP 5xx**, and **HTTP 429**.
  - For **HTTP 429**, prefer `Retry-After` when present.
  - Do **not** retry other 4xx.
- Retry count: **3**.
- Backoff: exponential `2^n` seconds (unless `Retry-After`).

### Concurrency throttling (per provider)
Use `SemaphoreSlim` per plugin instance.
- Discogs: max **2** concurrent requests
- iTunes: max **5** concurrent requests
- Last.fm: max **2** concurrent requests
- WikiData: max **2** concurrent requests

### Caching
- Prefer plugin-local caching via `ICacheManager.GetAsync(key, factory, token, duration, region)` (factory-based).
- Use `region: ServiceBase.CacheName` for provider result caching (matches existing plugins like Last.fm).
- Cache key format (match existing convention):
  - `{provider}:artist:{query.NameNormalized}:{providerSpecific}:{maxResults}`
  - Examples:
    - `itunes:artist:{nameNormalized}:{country}:{limit}`
    - `lastfm:artist:{nameNormalized}:{limit}`
- TTL:
  - Default for new providers: **2 hours**.
  - Keep existing TTLs where already implemented (e.g., Last.fm currently uses 6 hours).

### Secrets + environment variable overrides
- Treat API keys/tokens as secrets: never log them; prefer setting them via environment variables instead of DB.
- Env var mapping uses underscore-to-dot and is case-insensitive (see `SettingService.ApplyEnvironmentVariableOverrides`):
  - `searchEngine.discogs.userToken` → `searchEngine_discogs_userToken`

### Logging hygiene
- Sanitize user-controlled strings before logging (see existing `LogSanitizer.Sanitize(...)` usage in search services).

### Input validation
- Reject empty/whitespace queries.
- Trim, cap query length to **256**.
- Use URL/query encoding for parameters (do not HTML encode).
- AMG ID must be digits only.

---

## Phase 0 — Baseline + conventions alignment

- [x] Read the reference services/plugins for patterns:
  - [x] `src/Melodee.Common/Services/SearchEngines/AlbumImageSearchEngineService.cs` (manual plugin list, logging sanitization)
  - [x] `src/Melodee.Common/Plugins/SearchEngine/ITunes/ITunesSearchEngine.cs` (cache keys, query encoding)
  - [x] `src/Melodee.Common/Plugins/SearchEngine/LastFm/LastFm.cs` (cache region + TTL, API key handling)
  - [x] `src/Melodee.Common/Services/SearchEngines/ArtistSearchEngineService.cs` (`InitializeAsync` plugin construction)
- [x] Confirm the canonical plugin interface signature and result types used by `ArtistSearchEngineService`.
- [x] Confirm how settings are read (existing configuration factory / settings registry) and match patterns.

Exit criteria:
- [x] You can implement a new plugin without inventing new abstractions.

---

## Phase 1 — Shared infrastructure

Goal: implement small helper utilities so each plugin remains thin and consistent.

### 1.1 Add a shared validation helper
- [x] Create a small helper (static) in an appropriate common location near other helpers (follow repo conventions), e.g.:
  - [x] `src/Melodee.Common/Utility/SearchEngineQueryNormalization.cs`
- [x] Implement:
  - [x] `NormalizeQuery(string input) -> string` (trim + collapse whitespace optional, cap length)
  - [x] `ValidateAmgId(string amgId)` (digits only)

### 1.2 Add a shared Polly policy factory (optional but recommended)
- [x] Create a helper:
  - [x] `src/Melodee.Common/Plugins/SearchEngine/SearchEnginePolicies.cs`
- [x] Provide a method that returns `ResiliencePipeline<HttpResponseMessage>` implementing the defaults above (using Polly v8+ API).

### 1.3 HttpClient usage (follow repo pattern)
- [x] Use `IHttpClientFactory.CreateClient()` (unnamed/default) in plugins unless there is an established named-client convention for this service.
- [x] Set required headers explicitly (e.g., `User-Agent`, `Authorization`) and never log secrets.

Exit criteria:
- [x] All providers can obtain a `HttpClient` via `IHttpClientFactory`.

---

## Phase 2 — Provider plugins

Follow existing patterns: services construct plugins manually (see `AlbumImageSearchEngineService` and `ArtistSearchEngineService.InitializeAsync`).

### 2.1 Discogs plugin (new)

- [x] File: `src/Melodee.Common/Plugins/SearchEngine/Discogs/DiscogsArtistSearchEnginePlugin.cs`
- [x] Settings (NEW keys):
  - [x] `searchEngine.discogs.enabled` (bool, default false)
  - [x] `searchEngine.discogs.userToken` (string, optional; treat as secret)
- [x] User-Agent: use existing `searchEngine.userAgent` setting (already present in DB seed).

Implementation details:
- Use Discogs search endpoint with `type=artist`.
- If `userToken` is present, send `Authorization: Discogs token=<token>`.
- Always send `User-Agent` (from `searchEngine.userAgent`).
- Apply concurrency limit = 2.
- Cache with `ICacheManager.GetAsync(...)` using:
  - key: `discogs:artist:{query.NameNormalized}:{maxResults}`
  - region: `ServiceBase.CacheName`
  - duration: 2 hours

**Important Code Change**:
- [x] In `ArtistSearchEngineService.cs`, verify the search loop behavior. Currently, it breaks early (`if (pluginsResult.Count > 0) break;`). To support "merging results", **remove the early break** so all enabled providers run and return candidates.

### 2.2 iTunes (reuse existing plugin)

- [x] Reuse existing plugin: `src/Melodee.Common/Plugins/SearchEngine/ITunes/ITunesSearchEngine.cs`.
- [x] Enable key already exists: `SettingRegistry.SearchEngineITunesEnabled` (`searchEngine.itunes.enabled`).

Implementation details:
- `ITunesSearchEngine` already implements `IArtistSearchEnginePlugin` and already:
  - uses `Uri.EscapeDataString(...)` for query parameters
  - caches using key `itunes:artist:{query.NameNormalized}:{query.Country}:{maxResults}`
- [x] Added iTunes to `ArtistSearchEngineService` provider list with required dependencies (`ISerializer`, `IHttpClientFactory`) similar to `AlbumImageSearchEngineService`.

### 2.3 Last.fm (reuse existing plugin)

- [x] Reuse existing plugin: `src/Melodee.Common/Plugins/SearchEngine/LastFm/LastFm.cs`.
- [x] Enable key already exists: `SettingRegistry.SearchEngineLastFmEnabled` (`searchEngine.lastFm.Enabled`).
- [x] API key is already standardized in repo as `SettingRegistry.ScrobblingLastFmApiKey` (`scrobbling.lastFm.apiKey`) and is used by the search engine plugin too.

Implementation details:
- `LastFm` already caches with:
  - key: `lastfm:artist:{query.NameNormalized}:{maxResults}`
  - duration: 6 hours
  - region: `ServiceBase.CacheName`

### 2.4 WikiData plugin (new)

- [x] File: `src/Melodee.Common/Plugins/SearchEngine/WikiData/WikiDataArtistSearchEnginePlugin.cs`
- [x] Settings (NEW key):
  - `searchEngine.wikidata.enabled` (bool, default false)
- [x] User-Agent: use existing `searchEngine.userAgent` setting.

Implementation details:
- Use SPARQL endpoint with `format=json`.
- Substitute `{SEARCH_TERM}` safely:
  - cap length (256)
  - reject empty/whitespace
  - URL-encode any values that flow into mwapi URL parameters (use `Uri.EscapeDataString`)
  - if embedding into a SPARQL string literal, escape `\` and `"` and do not allow multiline input
- Apply concurrency limit = 2.

Exit criteria:
- [x] Any NEW plugins implement `IArtistSearchEnginePlugin` and produce `ArtistSearchResult` consistently.
- [x] Existing iTunes/Last.fm plugins are wired into the relevant provider list(s) without inventing new duplicate implementations.

---

## Phase 3 — AMG lookup UI integration (iTunes only)

- [x] Update `src/Melodee.Blazor/Components/Dialogs/ArtistLookupDialog.razor` to support "Lookup by AMG ID".
- [x] The dialog must use injected services (it already injects `ArtistSearchEngineService`; no internal HTTP calls).

Concrete approach:
- [x] Extend `ArtistSearchEngineService` with an explicit method `LookupByAmgIdAsync(string amgArtistId, ...)`, that:
  - [x] validates digits-only using `SearchEngineQueryNormalization.ValidateAmgIdResult()`
  - [x] calls iTunes lookup endpoint `https://itunes.apple.com/lookup?amgArtistId={ID}`
  - [x] returns `ArtistSearchResult` candidates with `AmgId` and `ItunesId` populated where available

Exit criteria:
- [x] User can enter an AMG ID and get candidates without enabling non-iTunes providers.

---

## Phase 4 — Settings + admin UI

### 4.1 Add settings keys (database-backed)
- [x] Ensure these NEW keys exist in DB seed/config:
  - [x] `searchEngine.discogs.enabled`
  - [x] `searchEngine.discogs.userToken`
  - [x] `searchEngine.wikidata.enabled`
- [x] Reuse existing keys (already present in DB seed / `SettingRegistry`) instead of adding duplicates:
  - [x] `searchEngine.userAgent` (shared UA for external providers)
  - [x] `searchEngine.itunes.enabled`
  - [x] `searchEngine.lastFm.Enabled`
  - [x] `scrobbling.lastFm.apiKey` (used by Last.fm search engine plugin)

Rules:
- This repo uses EF Core seed data for settings in `MelodeeDbContext`: update seed and **generate a new migration** (do not edit existing migrations).
- For secrets (Discogs token, Last.fm API key), prefer leaving DB value blank and setting via env var.
  - Env var mapping uses `_` → `.` and is case-insensitive.

### 4.2 Admin UI
- [x] Prefer using the existing generic settings admin page `src/Melodee.Blazor/Components/Pages/Admin/Settings.razor` (it already lists `SettingRegistry` keys and respects env-var overrides).
- [x] If you add provider-specific UI beyond generic key/value editing, keep it additive and ensure secrets are not displayed.

### 4.3 Localization
- [x] Any new user-facing strings must be localized via existing localization patterns (no hardcoded text).

Exit criteria:
- [x] An admin can enable Discogs/WikiData and configure required settings without restarting the app.

---

## Phase 5 — Tests

### 5.1 Unit tests (required)
- [x] Add unit tests per plugin using mocked HTTP responses.
- [x] Include edge cases:
  - [x] empty/whitespace query
  - [x] very long query
  - [x] unicode/diacritics
  - [x] 429 with Retry-After
  - [x] 5xx
  - [x] malformed JSON

### 5.2 Integration tests (optional/manual)
- [ ] Do not call live external APIs in CI.
- [ ] If adding manual tests, gate behind an explicit flag and provide strong throttling.

Exit criteria:
- [x] Tests are stable and deterministic (17 tests passing: 8 for Discogs, 9 for WikiData).

---

## Phase 6 — Documentation + verification

- [ ] Update `prompts/ARTIST-SEARCH-ENGINES.md` only if implementation required a plan adjustment.
- [ ] Run repo test suite relevant to the changes (`dotnet test`).
- [ ] Confirm:
  - no secrets logged
  - enable flags correctly gate provider execution
  - failure of one provider does not break overall search

---

## Done Definition

- [x] All phases checked off.
- [x] Providers can be enabled/disabled independently.
- [x] Admin settings page controls all required provider settings.
- [x] Search works with multiple providers and merges results without crashing on provider failures.
