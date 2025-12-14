# Melodee Server — Google Auth Enhancements (Client → Server → JWT)

**Last updated:** 2025-12-13
**Status:** Proposed requirements

## 1. Purpose

Melodee’s API is secured with **Bearer JWT** tokens.

This document specifies the **server-side enhancements** required to support **Google Sign-In** as an alternative to username/email + password, while still issuing a **Melodee JWT** that is used for all authenticated API calls.

The intent is that **Android and other clients** can:
- Offer Google Sign-In as a login option
- Exchange a Google credential for a Melodee-issued JWT
- Use the JWT in `Authorization: Bearer <token>` for all protected endpoints

## 2. Background / Current State

The OpenAPI spec currently documents password authentication endpoints:
- `POST /api/v1/Users/authenticate`
- `POST /api/v1/Users/auth` (alias)

Both return `AuthResponse`:
- `user`
- `serverVersion`
- `token` (Melodee JWT)

The spec does **not** currently document a Google credential exchange endpoint, refresh behavior, or token lifetime semantics.

## 3. Design Principles

1. **Google auth is a login mechanism, not an API auth mechanism.**
   - Google tokens should not be sent to every API request.
   - The server issues a Melodee JWT after verifying Google credentials.

2. **One auth model for the rest of the API:** Bearer JWT.

3. **Client portability:** The flow must work for Android, web, desktop, and other clients.

4. **Security-first defaults:**
   - Validate token signatures and claims strictly.
   - Avoid long-lived, non-revocable tokens.

## 4. Required Client Capabilities (Android + other clients)

Clients MUST be able to:
- Configure server base URL (self-hosted) and validate reachability.
- Perform Google Sign-In and obtain one of:
  - a Google **ID token** (preferred for simplicity), OR
  - an OAuth **authorization code** (preferred when you want server-side refresh with Google), depending on server implementation.
- Call the server’s Google exchange endpoint to obtain a Melodee JWT.
- Store Melodee tokens securely (Android: Keystore-backed storage).
- Attach `Authorization: Bearer <melodee_jwt>` on every protected request.
- Handle `401` by re-authenticating (silent Google sign-in) and retrying when safe.

Clients SHOULD be able to:
- Surface actionable error states (e.g., “Google account not linked”, “Server rejected credential”, “Please login again”).

## 5. Server Requirements

### 5.1 Google OAuth client configuration
The server MUST support configuration per deployment:
- Allowed Google OAuth Client IDs (Android/web/desktop)
- Expected `aud` (audience) values for token validation
- Optional allowed `hd` (hosted domain) restrictions

### 5.2 Credential verification
The server MUST validate Google credentials:
- If using **ID token**:
  - verify signature against Google’s published keys
  - verify issuer (`iss`) is Google
  - verify audience (`aud`) matches configured client ID
  - verify token is not expired
  - extract stable subject identifier (`sub`) and optional email

- If using **authorization code**:
  - exchange code at Google token endpoint
  - validate returned tokens equivalently

### 5.3 Account linking / provisioning
The server MUST define behavior for mapping Google identity → Melodee user.

Recommended rules:
- Identify Google account by `sub` (stable subject ID).
- Store linkage: `googleSubject`, `googleEmail`, `lastGoogleLoginAt`.
- If an existing user matches verified email and email is verified:
  - MAY auto-link on first Google login (configurable)
- Otherwise:
  - create a new user (if server allows self-signup), OR
  - reject with a clear error indicating that the account is not linked

### 5.4 JWT issuance
After successful verification and user mapping, the server MUST issue a **Melodee JWT**.

JWT MUST include:
- subject (Melodee user ID)
- issued-at (`iat`), expiration (`exp`)
- roles/claims required by the API

JWT SHOULD include:
- token identifier (`jti`) to support revocation

### 5.5 Token lifetime and refresh
The server MUST define one of these strategies:

**Strategy A — Short-lived JWT + re-exchange Google credential (simplest):**
- JWT expires relatively quickly.
- Client performs silent Google sign-in to obtain a fresh Google ID token and re-exchanges it.
- No refresh token stored by Melodee.

**Strategy B — JWT + refresh token (recommended for best UX):**
- Server issues access JWT + refresh token.
- Client uses refresh token to get new JWT without hitting Google.
- Server supports refresh token rotation and revocation.

This repository’s OpenAPI currently only exposes `token` (single JWT). If Strategy B is adopted, `AuthResponse` must be extended (backwards compat) or a new response schema introduced.

## 6. Proposed API Additions (OpenAPI)

Add a new endpoint:

### 6.1 Exchange Google credential for JWT
**Endpoint:** `POST /api/v1/Users/auth/google`

**Request (example):**
```json
{
  "idToken": "<google_id_token>",
  "provider": "google",
  "clientInfo": {
    "platform": "android",
    "appVersion": "0.1.0"
  }
}
```

**Response:** existing `AuthResponse` (or a new auth response that also includes refresh token if Strategy B is used).

**Errors:**
- `400` invalid request (missing token)
- `401` invalid / expired Google credential
- `403` user locked / blacklisted / signup disabled

### 6.2 Optional: refresh endpoint (Strategy B)
**Endpoint:** `POST /api/v1/Users/auth/refresh`

**Request:**
```json
{ "refreshToken": "<refresh_token>" }
```

**Response:**
```json
{ "token": "<new_jwt>", "refreshToken": "<rotated_refresh_token>" }
```

### 6.3 Optional: link/unlink endpoints
If you do not auto-link by email, consider:
- `POST /api/v1/Users/me/link/google`
- `DELETE /api/v1/Users/me/link/google`

These require the user to be logged in already and enable attaching a Google identity to an existing account.

## 7. Non-Functional Requirements

### 7.1 Security
- Rate limit auth endpoints.
- Log auth failures with safe redaction (no raw tokens).
- Support token revocation (at least server-side invalidation for refresh tokens; ideally `jti` allowlist/denylist).
- Ensure clock skew tolerance when validating tokens.

### 7.2 Privacy
- Store only necessary Google fields (`sub`, email, emailVerified).
- Avoid storing access tokens from Google unless required.

### 7.3 Backwards compatibility
- Password login MUST continue to work.
- Existing JWT-only clients MUST continue to work.

## 8. Acceptance Criteria

- Client can choose either:
  - username/email + password login, OR
  - Google Sign-In
- Google Sign-In results in a valid Melodee JWT, usable on protected endpoints.
- Invalid Google credentials return a clear 401 error.
- Server supports configurable policies (auto-link, self-signup, domain restrictions).

## 9. Open Questions

- Which strategy will Melodee adopt for refresh?
  - A: re-exchange Google credentials (simple)
  - B: refresh tokens (best UX)
- Will self-signup be allowed on self-hosted instances by default?
- Should linking require explicit confirmation even if email matches?

## 10. Strategy B Detailed Design & Testability Notes

For the adopted Strategy B (short-lived access tokens plus refresh tokens with rotation, revocation, and replay detection), see the consolidated architectural and testability details in `MELODEE-SERVER-GOOGLE-AUTH-ENHANCEMENTS.md`. That document describes:

- Token lifetimes, session model, and rotation rules.
- Data model changes for `UserSocialLogin` and refresh-token storage.
- Logging, PII, and rate-limiting guidance.
- Testability mapping across server, Blazor, CLI, and integration tests.
- Environment-specific configuration matrix and rollout considerations.
