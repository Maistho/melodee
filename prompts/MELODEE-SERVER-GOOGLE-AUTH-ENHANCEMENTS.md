# Melodee Server — Google Auth Enhancements

## 0. Strategy B: Access + Refresh Tokens with Rotation

This document captures the architectural decisions for introducing Google Sign-In alongside the existing username/password authentication, and for adopting a short-lived access token plus refresh-token-with-rotation model ("Strategy B").

### 0.1 Token Lifetimes and Session Model

Unless overridden per environment:

- **Access token (JWT) lifetime:** 15 minutes.
- **Refresh token lifetime:** 30 days.
- **Maximum session lifetime per device/session:** 90 days total from first issuance (absolute cap, regardless of individual refresh-token lifetimes).

Behavior:

- Clients receive a short-lived JWT and a long-lived refresh token.
- The access token is used for API requests; refresh is used only with the `/auth/refresh` endpoint.
- After 90 days from first use for a given token family, the backend will no longer issue new tokens for that family and will require full re-auth.

### 0.2 Refresh Tokens, Rotation, and Replay Detection

- Refresh tokens are issued as opaque, unguessable strings.
- On the server, **only a hashed form** of the refresh token is stored, never the raw token.
- Each refresh token record stores at least:
  - `UserId`
  - `TokenId` (stable identifier for this token)
  - `HashedToken`
  - `IssuedAt`
  - `ExpiresAt`
  - `RevokedAt` (nullable)
  - `PreviousTokenId` (nullable, to track rotation chains)

**Rotation rules:**

- On every successful refresh:
  - Validate the current refresh token (user, not expired, not revoked, token family not compromised).
  - Issue a **new** JWT and **new** refresh token.
  - Persist the new refresh token (hashed) and link it back via `PreviousTokenId`.
  - Mark the old refresh token as superseded/revoked so it cannot be reused.

**Replay detection:**

- If a refresh token that should have been superseded is used again, treat this as a **replay** event:
  - Reject the request with a `401` and error code `refresh_token_replayed`.
  - Optionally revoke the entire token family (all descendants/ancestors) according to configuration.
  - Emit a telemetry/security event so operators can investigate.

**Revocation:**

- Admins and security processes can revoke refresh tokens by:
  - `UserId` (revoke all user sessions),
  - `TokenId` (revoke a specific session), or
  - Token-family (via `PreviousTokenId` chain).
- A revoked token or token family must always result in a `401` with `refresh_token_invalid` on refresh attempts.

## 1. Data Model and Relationships

The following entities and relationships are expected. Exact EF Core model names may differ but should preserve these semantics.

### 1.1 UserSocialLogin

Represents a social/third-party login mapping for a user (initially Google; extensible to others later):

- `Id` (primary key)
- `UserId` (FK to `User`)
- `Provider` (e.g., `"Google"`)
- `Subject` (the provider-specific stable subject identifier, e.g. Google `sub` claim)
- `Email` (last known verified email address)
- `LastLoginAt` (timestamp of last login via this provider)

Constraints and behavior:

- `(Provider, Subject)` is unique.
- There is a one-to-many relationship from `User` to `UserSocialLogin`.
- `LastLoginAt` is updated on successful logins via that provider.

### 1.2 RefreshToken (or equivalent)

Represents a single refresh token instance in a rotation chain:

- `Id` / `TokenId` (primary key)
- `UserId` (FK to `User`)
- `HashedToken` (hash of the refresh token string)
- `IssuedAt`
- `ExpiresAt`
- `RevokedAt` (nullable)
- `PreviousTokenId` (nullable FK to another refresh token in the same chain)
- Optional: `DeviceName`, `IpAddress`, `UserAgent` for audits (if stored, treat as PII and protect accordingly).

Constraints and behavior:

- Lookups should be optimized by `UserId` and possibly `TokenId`.
- Business logic must never read or write raw token values from/to the database.
- Rotation and replay detection logic is implemented in a service layer, not in controllers.

## 2. Logging, PII, and Rate Limiting

### 2.1 Logging & PII Policy

Never log:

- Raw Google ID tokens.
- Raw access tokens.
- Raw refresh tokens.
- Full email addresses or other highly identifying PII unless absolutely necessary and guarded (e.g., debug-only logs in non-prod, with strict controls).

Allowed in logs (examples):

- High-level event categories (e.g., `"Auth.Login.Google.Success"`, `"Auth.Refresh.Replayed"`).
- User identifiers that are already internal (e.g., `UserId`, not email) where permitted by policy.
- Error codes without sensitive context (e.g., `invalid_google_token`, `refresh_token_invalid`).
- Truncated/hashed identifiers for correlation (e.g., first 8 characters of a token hash, not the token itself).

Guidelines:

- Prefer **structured logging** with fields like `userId`, `provider`, `authMethod`, and `errorCode`.
- Do not include secrets or tokens in exception messages.
- Ensure log levels are tuned so that auth failures do not generate overly verbose logs in production.

### 2.2 Rate Limiting (High-Level)

Exact implementation can vary, but the following endpoints should be protected with reasonable per-IP and per-user limits:

- `POST /api/v1/Users/auth/google`
- `POST /api/v1/Users/auth/refresh`
- `POST /api/v1/Users/me/link/google`

Example starting points (tunable per deployment):

- `/auth/google`: ~10 requests/minute per IP and per subject/email.
- `/auth/refresh`: ~30 requests/minute per refresh token family.
- `/me/link/google`: ~5 requests/hour per authenticated user.

These values should be surfaced as configuration where practical, and they should be validated in Phase 2/4 integration and load tests.

## 3. Testability Notes (Decision → Test Mapping)

This section ties key security and behavior decisions to specific test plans and projects, aligned with the WBS.

### 3.1 Token Lifetimes & Session Behavior

Decision:

- Access tokens expire after 15 minutes.
- Refresh tokens expire after 30 days.
- Maximum session lifetime of 90 days per device/session.

Tests:

- **Project:** `tests/Melodee.Tests.Common`
- **Areas:** `*.Security.RefreshTokenFlowTests`
- **Phases:** 1, 2
- **Scenarios:**
  - Issuing tokens with the configured lifetimes.
  - Refresh attempts before and after `ExpiresAt`.
  - Enforcing the 90-day absolute max session (even if individual refresh tokens are technically not expired).

### 3.2 Refresh Token Rotation & Replay Detection

Decision:

- Every successful refresh call must rotate the refresh token.
- Old tokens cannot be reused; reuse is treated as a replay and may revoke the token family.

Tests:

- **Project:** `tests/Melodee.Tests.Common`
- **Areas:** `*.Security.RefreshTokenFlowTests`
- **Phases:** 1, 2, 4
- **Scenarios:**
  - Initial issuance of a token pair.
  - Multiple sequential refreshes with proper rotation and persistence of `PreviousTokenId`.
  - Reusing an old refresh token → `401` with `refresh_token_replayed` and appropriate revocation of the chain.
  - Admin-triggered revocation, followed by failed refresh attempts (`refresh_token_invalid`).

### 3.3 Google Token Validation & Hosted-Domain Policies

Decision:

- Google ID tokens are validated using Google’s public keys with caching and clock-skew tolerance.
- Optional restriction by allowed hosted domains (e.g., `Auth:Google:AllowedHostedDomains`).

Tests:

- **Project:** `tests/Melodee.Tests.Common`
- **Areas:** `*.Security.GoogleTokenServiceTests`
- **Phases:** 1, 2
- **Scenarios:**
  - Valid token → accepted, correct claims extracted.
  - Invalid signature/audience/issuer → `invalid_google_token`.
  - Expired token → `expired_google_token`.
  - Hosted domain allowed vs. disallowed → success vs. `forbidden_tenant`.
  - JWKS cache hit vs. miss; failure to fetch keys results in safe failure (no logins, telemetry event).

### 3.4 Account Creation, Auto-Linking, and Manual Linking

Decision:

- New accounts can be created on first Google sign-in when `Auth:SelfRegistrationEnabled` is true.
- Auto-link by email is disabled by default (`Auth:Google:AutoLinkEnabled = false`).
- Manual linking requires a password-authenticated session and a valid Google token.

Tests:

- **Projects:**
  - Server controllers: `Melodee.Tests.Server` (or an equivalent server test project).
  - Blazor UI: `tests/Melodee.Tests.Blazor`.
- **Phases:** 2, 3, 3A
- **Scenarios:**
  - New user via Google when self-registration is enabled vs. disabled (`signup_disabled`).
  - Existing user with `UserSocialLogin` entry logs in via Google successfully.
  - Auto-link enabled + unique verified email → account auto-linked.
  - Auto-link disabled or ambiguous email → `google_account_not_linked` with UI prompting password + manual link.
  - Manual link endpoint requires existing authenticated session and rejects attempts that would hijack another account.

### 3.5 Logging & PII Redaction

Decision:

- No raw tokens or sensitive PII in logs.
- Use structured events with redacted identifiers and error codes.

Tests:

- **Projects:**
  - Core services: `tests/Melodee.Tests.Common` (security and logging-focused tests).
  - Server integration: `Melodee.Tests.Server` for end-to-end failure scenarios.
- **Phases:** 1, 2, 4
- **Scenarios:**
  - Simulate auth failures and verify logs contain error codes and non-sensitive identifiers only.
  - Ensure exceptions and failure paths do not leak tokens or email addresses.

### 3.6 Configuration & Environment Behavior

Decision:

- All auth-related policies (Google enabled, auto-link, self-registration, lifetimes, allowed domains) are configurable via `appsettings.json` + `appsettings.{Environment}.json` + environment variables.

Tests:

- **Projects:** `tests/Melodee.Tests.Common` (configuration binding and validation tests), server integration tests for startup.
- **Phases:** 1, 2, 4
- **Scenarios:**
  - Binding strongly-typed options from different configuration layers.
  - Startup succeeds or fails fast when required Google config is missing or inconsistent.
  - Effective behavior changes when environment-specific settings are toggled.

## 4. Configuration Matrix Template

Use this template to track environment-specific Google Auth configuration and policies. Real secrets should be stored securely (e.g., environment variables, secret stores) and not committed.

| Environment | Auth:Google:Enabled | Auth:Google:ClientId | Auth:Google:AllowedHostedDomains | Auth:Google:AutoLinkEnabled | Auth:SelfRegistrationEnabled | Notes |
|------------|---------------------|-----------------------|----------------------------------|-----------------------------|-----------------------------|-------|
| Development |                     |                       |                                  |                             |                             |       |
| Staging     |                     |                       |                                  |                             |                             |       |
| Production  |                     |                       |                                  |                             |                             |       |

This matrix is referenced by WBS Phase 0 task 0.2 and should be maintained by the team responsible for environment configuration.

