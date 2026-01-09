## ADR-0003: External integrations use Settings + caching

- Date: 2025-12-13T16:35:42.249Z
- Status: Accepted

### Context

Melodee integrates with external services (Spotify, Last.fm, iTunes, etc.) for search/scrobbling/metadata.

We need a consistent strategy for where credentials are stored, what happens when credentials are missing/invalid, and how to limit repetitive API calls.

### Decision

- All external API keys/secrets/tokens are stored in the **Settings** table.
- If credentials are **missing or invalid**, the integration is considered **disabled**.
  - The system should **fail gracefully** (no unhandled exceptions) and return an empty/neutral result.
- External API calls should be cached using the DI-injected `ICacheManager`.

### Rationale

- Centralizes configuration for self-hosted deployments.
- Allows explicit enable/disable behavior without depending on environment variables.
- Reduces rate-limit pressure and improves UI responsiveness.

### Consequences

- Some integrations require UI/admin workflows to populate settings.
- Caching introduces staleness; cache keys and TTLs must be chosen carefully.

