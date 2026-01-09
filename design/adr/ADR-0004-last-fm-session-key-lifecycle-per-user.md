## ADR-0004: Last.fm session key lifecycle (per-user)

- Date: 2025-12-13T16:38:33.838Z
- Status: Accepted

### Context

Last.fm scrobbling requires a user-authorized **session key** (`sk`). Session keys typically do not expire, but can be revoked by the user in Last.fm, and any invalidation must be handled gracefully.

Melodee is a server-hosted app; we should not ask users to provide their Last.fm password to Melodee.

### Decision

- Use the **web authentication** flow (`auth.getSession`) to obtain a session key.
  - Do **not** use `auth.getMobileSession` (requires collecting user password).
- Store the session key **per Melodee user** in the database (`User.LastFmSessionKey`).
  - Treat as a secret: never log it and do not expose it via APIs.
- Runtime behavior:
  - If global Last.fm scrobbling is enabled but the user has no session key: return success/no-op (and optionally log once at Debug/Warn).
  - If Last.fm returns an "invalid session" error during scrobble/now-playing: clear `User.LastFmSessionKey` and require re-linking.
- There is no refresh flow; re-authentication is the only recovery after revocation.

### Rationale

- Avoids handling user passwords and matches Last.fm’s recommended OAuth-style flow.
- Keeps scrobbling user-scoped (different Melodee users can link different Last.fm accounts).
- Makes revocation safe and explicit.

### Consequences

- Requires a UI flow to link/unlink Last.fm for a user.
- Some failures will look like silent no-ops (by design) unless surfaced in UI.

