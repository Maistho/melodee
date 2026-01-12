## ADR-0002: Similar Songs is admin-managed (ArtistRelationType.Similar)

- Date: 2025-12-13T16:30:20.883Z
- Status: Accepted

### Context

OpenSubsonic defines `getSimilarSongs` / `getSimilarSongs2`. Many servers implement this by calling third-party services (Last.fm/Spotify/etc.) or by doing behavior-based recommendations.

Melodee already has an `ArtistRelation` table and an `ArtistRelationType.Similar` value, and Melodee supports role-based editing (Admin/Editor).

### Decision

Melodee will compute “similar songs” using **curated, local similarity**:

- Similarity is defined by **Artist → Similar Artists** relationships managed by Admin/Editor users (`ArtistRelationType.Similar`).
- `getSimilarSongs(2)` will return songs drawn from:
  1) the requested song/artist’s own catalog (optional), and
  2) the catalog of related “similar” artists.

### Rationale

- Avoids reliance on third-party APIs/credentials and improves determinism.
- Fits a self-hosted/air-gapped deployment model.
- Allows the library owner to control recommendations and quality.

### Consequences

- Similar songs quality depends on how well similarity relationships are maintained.
- Requires UI/management workflows for Admin/Editor to curate “similar” relationships.

### Revisit / Future Work

Optionally add fallback strategies when there are no curated relationships (e.g., same genre/tags, same contributors, play-history co-occurrence) but keep curated similarity as the primary signal.

