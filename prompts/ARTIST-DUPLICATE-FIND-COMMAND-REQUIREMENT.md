---
post_title: 'Artist duplicate detection CLI requirements'
author1: 'GitHub Copilot CLI'
post_slug: 'artist-duplicate-find-command-requirements'
microsoft_alias: 'copilot-cli'
featured_image: ''
categories:
  - 'engineering'
tags:
  - 'cli'
  - 'data-quality'
  - 'music-metadata'
ai_note: 'Drafted with assistance from GitHub Copilot CLI based on high-level user requirements.'
summary: 'Requirements for implementing an mcli artist find-duplicates CLI command to detect duplicate artists using external IDs, fuzzy name matching, and album overlap, with a focus on scalable, human-reviewed deduplication workflows.'
post_date: '2025-12-30'
---

## Overview

This document proposes a design for an `mcli artist find-duplicates` command that identifies likely duplicate artist records
based on external IDs (e.g., MusicBrainz, Spotify) and fuzzy name similarity, and returns a ranked list of candidate
matches for human review.

Goals:

- Provide a repeatable, scriptable way to surface likely duplicate artists.
- Combine strong signals (shared external IDs) with name-based similarity scoring.
- Output enough detail (score and reasons) for safe, manual deduplication workflows.

Out of scope for this iteration:

- Automatically merging or deleting artists.
- UI flows for managing duplicates.

## User stories and CLI UX

### Primary user stories

- As a curator, I want to run `mcli artist find-duplicates` so I can discover duplicate artists that need cleanup.
- As a curator, I want to see a confidence score and explanation so I understand why two artists are considered
  duplicates.
- As a curator, I want to filter by minimum score or by external ID source so I can focus on the highest-value work
  first.

### CLI command shape

Proposed syntax:

- `mcli artist find-duplicates [options]`

Key options:

- `--min-score <0-1>`: Only return pairs/groups with score greater than or equal to this value (default: `0.7`).
- `--limit <n>`: Limit the number of duplicate groups returned (e.g., top N by maximum score).
- `--source <name>`: Limit to artists whose external IDs include a given source (e.g., `musicbrainz`, `spotify`).
- `--artist-id <id>`: Restrict search to duplicates of a single artist (useful for focused checks).
- `--format <table|json>`: Control output shape; `table` for human reading, `json` for scripting.
- `--include-low-confidence`: Include low-scoring candidates that would normally be filtered out.

Example invocations:

- `mcli artist find-duplicates` (default run, top candidates)
- `mcli artist find-duplicates --min-score 0.9` (only high-confidence duplicate groups)
- `mcli artist find-duplicates --artist-id 12345` (only duplicates for a specific artist)
- `mcli artist find-duplicates --format json > duplicates.json` (for downstream tooling)

### Output model

Each result should group together all artists believed to be potential duplicates of one another (not just simple pairs):

```jsonc
{
  "groups": [
    {
      "groupId": "artist-dup-0001",
      "maxScore": 0.98,
      "artists": [
        {
          "artistId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
          "name": "Elton John",
          "externalIds": {
            "spotify": "spid-1",
            "discogs": "discogs-1234"
          }
        },
        {
          "artistId": "0f9e8d7c-6b5a-4321-9abc-def012345678",
          "name": "John, Elton",
          "externalIds": {
            "spotify": "spid-1"
          }
        }
      ],
      "pairs": [
        {
          "leftArtistId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
          "rightArtistId": "0f9e8d7c-6b5a-4321-9abc-def012345678",
          "score": 0.98,
          "reasons": [
            "SharedMusicBrainzId",
            "NameFirstLastReversal",
            "HighTokenSimilarity"
          ]
        }
      ]
    }
  ]
}
```

For `table` output, this can be rendered as one row per pair, with group ID, both names, IDs, score, and key reasons.

## Data sources and assumptions

We assume an `Artist` model roughly like:

- `Id`: internal GUID identifier (consistent with existing artist IDs in the codebase).
- `Name`: primary display name (e.g., `Elton John`).
- `SortName` or `SortableName` (optional): a normalized name used for sorting (e.g., `John, Elton`).
- `ExternalIds`: a collection or map of `(source, value)` pairs for known providers.
- Optional metadata (country, type, aliases) that might later refine matching but is not required for v1.

External ID providers considered in matching:

- `ItunesId`
- `AmgId`
- `DiscogsId`
- `WikiDataId`
- `LastFmId`
- `SpotifyId`
- `DeezerId`

Only valid, non-default external IDs should participate in matching (not `null`, empty, or obvious default/zero
sentinel values).

We also assume an `Album`/`Release` model that can be associated to artists, at least with the following fields:

- `Id`: internal identifier.
- `Title`: album or release title.
- `Year` or `ReleaseDate`: coarse-grained release year or date.

The implementation should:

- Treat shared external IDs from trustworthy sources as a very strong duplicate signal.
- Treat overlapping albums (same normalized title and year) as a strong supporting signal, especially for common names.
- Rely on name-based similarity only when external IDs and album overlap are missing, sparse, or conflicting.
- Avoid logging full artist names or raw external IDs in plain text during matching; when logging is necessary for
  diagnostics, prefer structured logs that include only opaque identifiers (e.g., GUIDs) and/or hashed/truncated forms
  of external IDs rather than full values, to reduce privacy and leakage risk.
- Be implemented in the application/domain layer (e.g., an `IArtistDuplicateFinder` service) and consumed by the CLI.

## Matching strategy

### 1. Strong matches via external IDs

Effects:

- Any two artists that share the same `(provider, value)` external ID (e.g., same `SpotifyId` or `DiscogsId`) are almost
  certainly duplicates.
- If multiple external IDs match across different providers, this should drive the score very close to 1.0.
- External IDs are only considered if they are valid (non-null, non-empty, and not a default/zero sentinel value).
  Invalid values to exclude include `null`, empty strings, `"0"`, all-zeros GUIDs, and provider-specific sentinels
  such as `"-1"` or `"unknown"`.

**Implementation sketch**

- Build an index keyed by external ID value per provider, restricted to valid values:
  - `Dictionary<(provider, value), List<ArtistId>>`.
- For each key that maps to more than one artist, treat all artist IDs in the list as belonging to the same initial
  duplicate cluster.

**Scoring contribution** (tunable per provider):

- Shared `SpotifyId`: base external ID score `0.95`.
- Shared `DiscogsId`, `ItunesId`, `DeezerId`, `LastFmId`, `AmgId`, or `WikiDataId`: base external ID score around
  `0.9–0.95` depending on how trustworthy each provider is considered.
- Multiple shared IDs across providers: boost score up to `1.0`.

### 2. Name-based similarity

Name-based matching is used both to rank within external-ID-based clusters and to discover duplicates where external IDs
are missing.

#### 2.1 Name normalization

Normalization should never destroy the original signal we need for heuristics like first/last reversal. The
`ArtistReadModel` should therefore expose both raw and normalized fields.

Define a normalization pipeline that produces separate normalized fields from `Name` and `SortName` (where available):

- Convert to lowercase.
- Remove leading and trailing whitespace.
- Remove diacritics (e.g., `Björk` → `bjork`).
- Remove punctuation (commas, periods, quotes, etc.).
- Normalize whitespace (collapse multiple spaces to single spaces).
- Remove leading articles and common prefixes: `the`, `a`, `an`, optionally `dj`, `mc` when they appear as loose
  prefixes.

The raw fields (`Name`, `SortName`) are preserved for heuristics that rely on punctuation or original ordering (e.g.,
`"John, Elton"`). The normalized fields are used for most fuzzy matching and indexing.

This yields one or more normalized forms per artist:

- `normalizedName` (derived from `Name`).
- `normalizedSortName` (derived from `SortName`, if present).
- `tokenSortedName`: tokens sorted alphabetically from the normalized form for order-insensitive comparisons.

#### 2.2 Tokenization and first/last reversal detection

For each name, compute from the normalized form:

- Token list: split on spaces (`["elton", "john"]`).
- A reversed form for two-token names: `"john elton"`.

Heuristics for first/last reversal (e.g., `"Elton John"` vs `"John, Elton"`) operate on both raw and normalized data:

- Use the raw `Name`/`SortName` to detect patterns like `"Last, First"` while commas are still present.
- Use `tokenSortedName` from the normalized form to check that both names contain the same token set.
- If both conditions hold (same tokens and a `Last, First` pattern in at least one raw name), mark reason
  `NameFirstLastReversal` and assign a high name similarity score (e.g., `0.9+`).

#### 2.3 String similarity metrics

Use one or more fuzzy string similarity metrics to compute a name similarity score in `[0, 1]`:

- Candidate algorithms:
  - Jaro–Winkler similarity (good for short names and transpositions).
  - Normalized Levenshtein distance.
  - Token-based similarity (e.g., token set ratio) to ignore word order.

Implementation notes:

- Prefer a library already used in the codebase for string similarity; otherwise introduce a small, well-maintained
  dependency or implement a simple Jaro–Winkler.
- Compute multiple scores if cheap and combine them with weights.

Example rules:

- Exact normalized name match: `nameScore = 0.9` (no external IDs needed).
- Token-equivalent and high char-based similarity (e.g., > 0.95): `nameScore = 0.9`.
- Substantial but not perfect match (e.g., alias or minor spelling variants): `nameScore` in `0.7–0.85`.

### 3. Candidate generation (avoiding O(n^2))

Naive pairwise comparison across all artists is `O(n^2)` and will not scale, so we need candidate pruning.

For catalogs larger than roughly 100k artists, the implementation should:

- Stream artists via `IAsyncEnumerable<ArtistReadModel>` rather than loading all artists into memory at once.
- Be mindful of memory usage; indices for external IDs, names, and albums add overhead, so fully materializing 1M artists
  plus indices can exceed ~1 GB of RAM and should be avoided where possible.

Proposed indexing strategy:

1. External ID buckets (as above) to form strong-signal clusters.
2. Name-based buckets:
   - Bucket by first letter or first trigram of the normalized name.
   - Within each bucket, further partition by token count (e.g., 1-word, 2-word, 3+ word names).
   - Only compare pairs within the same bucket.

Alternate/extension:

- Use a trigram or n-gram index to find near neighbors for each name (more advanced, likely v2).

### 4. Album overlap signal

For common-name artists where names alone are ambiguous, shared discs/releases are a strong additional indicator.

Per artist:

- Build a lightweight release index: a set of `(normalizedTitle, year)` tuples for albums/releases associated to the
  artist.
- Normalize titles similarly to artist names (lowercase, trim, remove diacritics and punctuation, collapse whitespace).

For a candidate pair `(A, B)`:

- Compute the intersection and union of their album sets.
- Derive an `albumOverlapScore` using an overlap measure such as `|A ∩ B| / min(|A|, |B|)` so small catalogs are not
  overly penalized; Jaccard (`|A ∩ B| / |A ∪ B|`) can also be considered with weighting if needed.
- Treat multiple shared, non-generic albums (e.g., 2+ overlapping titles with same year) as a strong supporting signal.
- Down-weight very common titles (e.g., `Greatest Hits`, or any `(title, year)` that appears under many different
  artists) to avoid false positives.

This album signal should not override clearly conflicting signals on its own, but when combined with neutral or weak
name similarity, it can lift a candidate into the "review" range.

Edge cases to keep in mind for album signals:

- Collaborations/featured artists (e.g., "X feat. Y", "X & Y") might legitimately share albums while not being the same
  primary artist; rules may need to down-weight overlap driven only by collaborations.
- "Various Artists" and similar catch-all artists should be treated specially (e.g., excluded from duplicate search or
  heavily down-weighted) since they naturally share many albums.
- Artists with 0–1 albums will have little or no album overlap signal; their matching must rely more heavily on names and
  external IDs.

### 5. Score computation and categories

For each candidate pair `(A, B)`, compute:

- `externalIdScore`: `0` if no shared IDs, else based on sources shared.
- `nameScore`: result of the name similarity heuristic.
- `albumOverlapScore`: `0` if no album overlap data or no shared albums, else based on shared `(title, year)` tuples.
- `penalties`: e.g., for obviously conflicting signals (different external IDs that usually mean distinct artists).

In addition to these raw scores, a later calibration phase should adjust thresholds (`minScore`, category boundaries)
using real-world labeled data (e.g., a set of manually confirmed duplicates and non-duplicates) so that defaults like
`0.7` and `0.9` are empirically grounded rather than arbitrary.

Combine into a final score in `[0, 1]`:

- Example formula:

  ```text
  base = max(externalIdScore, nameScore, albumOverlapScore)
  bonus = 0
  - if two or more signals are >= 0.9, bonus += 0.05 (capped at 1.0)
  - if first/last reversal detected, bonus += 0.05
  - if albumOverlapScore >= 0.8 and name is otherwise common, bonus += 0.05
  finalScore = min(1.0, base + bonus)
  ```

Define coarse categories to help users interpret results:

- `High` (>= 0.9): very likely duplicates; safe to review for merge.
- `Medium` (0.75–0.9): possible duplicates; requires human judgement.
- `Low` (< 0.75): weak matches; normally filtered unless `--include-low-confidence` is set.

## Domain and application design

### Service placement

The duplicate detection logic is conceptually an artist-centric concern and should live behind a dedicated
`IArtistDuplicateFinder`/`ArtistService` abstraction. To support reuse, that service can then be injected into
`AlbumService` and the Blazor UI so both the CLI and UI call the same underlying logic for discovery and resolution
workflows.

Concretely, an artist-focused application service will expose a method such as `FindArtistDuplicatesAsync` that wraps the
duplicate finder described below, and `AlbumService` can act as a façade where it already orchestrates artist/album
metadata.

### New application service

Introduce an application-layer service responsible for the detection logic:

- Interface: `IArtistDuplicateFinder`.

Example signature:

```csharp
public interface IArtistDuplicateFinder
{
    Task<IReadOnlyList<ArtistDuplicateGroup>> FindDuplicatesAsync(
        ArtistDuplicateSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
```

Supporting models:

```csharp
public sealed record ArtistDuplicateSearchCriteria(
    double MinScore,
    int? Limit,
    string? Source,
    Guid? ArtistId);

public sealed record ArtistDuplicateGroup(
    string GroupId,
    double MaxScore,
    IReadOnlyList<ArtistDuplicateCandidate> Artists,
    IReadOnlyList<ArtistDuplicatePair> Pairs);

public sealed record ArtistDuplicateCandidate(
    Guid ArtistId,
    string Name,
    IReadOnlyDictionary<string, string> ExternalIds);

public sealed record ArtistDuplicatePair(
    Guid LeftArtistId,
    Guid RightArtistId,
    double Score,
    IReadOnlyCollection<string> Reasons);
```

The implementation will:

- Query artists from the repository (batched or streaming to avoid loading everything into memory at once for large
  catalogs).
- Build external ID and name indices.
- Generate candidate pairs and compute scores.
- Build connected components (clusters) from high-scoring pairs to form `ArtistDuplicateGroup` instances; edges are
  defined between any two artists whose pairwise score is above a chosen grouping threshold. This transitive grouping
  means that if A–B and B–C are both strong matches (e.g., 0.95) but A–C is weaker (e.g., 0.60), all three still belong
  to one group, while the per-pair scores remain available for the UI to show nuanced confidence within the group.

### Repository and data access

The service should depend on a repository abstraction rather than EF Core directly:

```csharp
public interface IArtistReadRepository
{
    IAsyncEnumerable<ArtistReadModel> GetAllArtistsAsync(CancellationToken cancellationToken = default);
    // Implementations should page/batch underlying database queries to avoid long-lived transactions and timeouts.
}

public sealed record ArtistReadModel(
    Guid ArtistId,
    string Name,
    string? SortName,
    IReadOnlyDictionary<string, string> ExternalIds,
    IReadOnlyCollection<AlbumStub> Albums);

public sealed record AlbumStub(
    Guid AlbumId,
    string Title,
    int? Year);
```

`ArtistReadModel` is a lightweight projection including only fields relevant for matching (IDs, names, external IDs,
minimal album info) for performance.

Implementation details:

- Use efficient queries and projections to minimize memory overhead.
- Consider a configurable upper bound for the number of artists processed in one run or use pagination/batching.

## CLI integration design

Assuming `mcli` already has a command/handler infrastructure, we can add:

- Command: `ArtistFindDuplicatesCommand` with properties mirroring the CLI options.
- Handler: `ArtistFindDuplicatesCommandHandler` that calls `IArtistDuplicateFinder` and formats output.

Responsibilities:

- Parse CLI options into `ArtistDuplicateSearchCriteria` and validate them (e.g., `--min-score` must be between `0` and
  `1`).
- Invoke `FindDuplicatesAsync`.
- Apply further client-side filtering or sorting if needed.
- Render results as either a human-readable table or JSON.
- Handle exceptions from `IArtistDuplicateFinder` by logging an appropriate error and returning a non-zero exit code so
  automation can detect failures.

Example exception types (for documentation and consistent handling):

```csharp
public class ArtistDuplicateSearchException : Exception
{
    public ArtistDuplicateSearchException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}

public class ArtistRepositoryUnavailableException : ArtistDuplicateSearchException
{
    public ArtistRepositoryUnavailableException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
```

Table output example (one row per pair):

- Columns: `GroupId`, `Score`, `ArtistIdA`, `NameA`, `ArtistIdB`, `NameB`, `SharedExternalIds`, `Reasons`.

## Testing strategy

### Unit tests

Key units to cover (including negative cases):

- Name normalization:
  - `"Elton John"` vs `"elton   john"` → identical normalized form.
  - Diacritic removal (`"Björk"` → `"bjork"`).
  - Article stripping (`"The Beatles"` → `"beatles"`).
- First/last reversal detection:
  - `"Elton John"` vs `"John, Elton"` should yield high similarity and include
    `NameFirstLastReversal` reason.
- String similarity scoring:
  - Exact matches, near matches, clearly different names.
  - Similar/common names with clearly different external IDs should stay low-scoring.
  - Translation/transliteration variants (e.g., "BTS" vs "Bangtan Sonyeondan") may require stronger reliance on
    external IDs or future specialized normalization.
- Score composition:
  - Shared external ID + high name match yields score near `1.0`.
  - No external IDs but identical normalized name yields strong score.

### Integration tests

- End-to-end tests from the CLI (or command handler) down to the in-memory repository:
  - Seed a catalog with at least 100 artists, including 10 known duplicate groups (e.g., same external IDs but different
    names, and pairs with strong album overlap).
  - Verify that `find-duplicates --min-score 0.8` returns exactly those known duplicate groups and that scores fall into
    intended categories.
- Edge-case scenarios:
  - Artists with no external IDs but identical normalized names should still be surfaced with appropriate scores.
  - Artists with shared albums but low name similarity (e.g., collaborations) should be evaluated and, if needed,
    down-weighted by additional business rules.
  - Common-name collisions (e.g., multiple "John Smith" artists with distinct discographies) should be present in test
    data to ensure the algorithm does not over-link them.
  - Artists with conflicting external IDs should not be merged without strong supporting signals.
- Tests for filters:
  - `--min-score` correctly filters low-confidence pairs.
  - `--artist-id` restricts results to a single artist’s cluster.

### Performance/scale testing (later phase)

- Benchmark duplicate detection on catalogs of approximately 10k, 100k, and 1M artists, measuring runtime and memory
  usage, including the cost of external ID indices, name buckets, and album overlap structures.
- Target: on standard hardware, 100k artists should complete within roughly 5 minutes for the CLI workflow while staying
  within acceptable memory limits; for 1M+ artists, consider chunked or multi-pass processing to bound memory.

## Future enhancements (beyond v1)

- Use additional metadata signals:
  - Country, disambiguation strings, or life dates to avoid merging genuinely distinct artists with similar names.
  - Aliases and alternate names.
- Persist duplicate groups and curator decisions so future runs can:
  - Skip already-reviewed pairs.
  - Learn from accept/reject decisions to tune thresholds.
- Provide optional auto-merge suggestions for trivially safe cases (multiple records with identical external IDs and very
  high name similarity), guarded behind explicit flags and review workflows.
- Explore more advanced approximate nearest-neighbor search (e.g., using embeddings or locality-sensitive hashing) for
  very large catalogs.
