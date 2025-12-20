# Google Auth Work Breakdown Structure (WBS)

**Phase Map / Checklist**

- [x] Phase 0 — Architecture & Decisions
- [x] Phase 1 — Server Foundations
- [x] Phase 2 — API Endpoints & Account Linking
- [x] Phase 3 — Client Integration
- [x] Phase 3A — Application Login Experience
- [x] Phase 4 — Testing, Security, and Rollout

**Objective:** Introduce Google Sign-In alongside existing username/password authentication while issuing Melodee JWTs and adopting the refresh-token-with-rotation strategy (Strategy B) to provide best-in-class UX and security.

---

## Phase 0 — Architecture & Decisions

### 0.A Policy & Defaults

Unless otherwise overridden per-environment, the following defaults apply:

- **Token lifetimes**
  - Access token (JWT) lifetime: **15 minutes**.
  - Refresh token lifetime: **30 days**, with an absolute max session lifetime of **90 days** per device/session.
- **Account creation & linking**
  - `Auth:SelfRegistrationEnabled` default: **true** (new accounts may be created on first Google sign-in in consumer-style deployments).
  - `Auth:Google:AutoLinkEnabled` default: **false** (existing accounts are not auto-linked by email; linking is manual by default).
  - `Auth:Google:AllowedHostedDomains` default: **empty list** (no domain restriction by default; may be restricted in enterprise deployments).
- **Refresh tokens**
  - One refresh token per device/session, stored **hashed** with metadata (`UserId`, `TokenId`, `IssuedAt`, `ExpiresAt`, `RevokedAt`, `PreviousTokenId`).
  - Rotation on every successful refresh; reuse of an old token is treated as a replay attempt and revokes the token family.

These defaults should be recorded in `MELODEE-SERVER-GOOGLE-AUTH-ENHANCEMENTS.md` as part of task 0.1.

| Task | Description | Owner | Deliverables | Dependencies |
| --- | --- | --- | --- | --- |
| 0.1 Confirm Strategy B | Ratify that Melodee will issue short-lived access JWTs plus refresh tokens with rotation, revocation, and replay detection. Document token TTLs, rotation rules, and storage requirements. | Security + Backend leads | Decision note appended to `MELODEE-SERVER-GOOGLE-AUTH-ENHANCEMENTS.md`; token lifetime matrix | None |
| 0.2 Inventory Google OAuth Clients | Gather Android/web/desktop client IDs, signing certificates, and hosted domain policies; plan configuration exposure per environment. | Infra lead | Config matrix + secrets management plan | 0.1 |
| 0.3 Data Model Impact Review | Identify new entities/columns (`UserSocialLogin`, refresh token tables), indexing, and migration sequencing. | Backend lead | ERD update + migration checklist | 0.1 |
| 0.4 Compliance & Logging Requirements | Define PII handling, logging redaction, retry/rate limits, and audit requirements for auth events. | Security lead | Logging spec + rate limit policy | 0.1 |
| 0.5 Testability & Threat-Model Mapping | For each auth decision (lifetimes, rotation, account linking, rate limits, logging), document how it will be validated by unit, integration, and E2E tests in later phases. Update this WBS with explicit test tasks per phase and add a "Testability Notes" section to `MELODEE-SERVER-GOOGLE-AUTH-ENHANCEMENTS.md`. | Security + QA + Backend leads | Updated WBS test rows; `Testability Notes` addendum clearly mapping decisions to tests | 0.1–0.4 |

**Phase 0 Testing & Quality Notes**

- Deliver a short matrix in `MELODEE-SERVER-GOOGLE-AUTH-ENHANCEMENTS.md` mapping:
  - Token lifetimes → Phase 1/2 JWT & refresh tests.
  - Rotation and replay detection → Phase 1/2 refresh tests, Phase 4 load/security tests.
  - Account creation/linking policies → Phase 2 controller + integration tests, Phase 3/3A UX tests.
  - Rate limiting and hosted-domain restrictions → Phase 2/4 integration and security tests.
  - Logging redaction and PII handling → Phase 1/2 unit tests with logger/telemetry mocks and Phase 4 security review.

Milestone: Architecture sign-off enabling implementation and test design for later phases.

---

## Phase 1 — Server Foundations

| Task | Description | Owner | Deliverables | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| 1.1 Config Plumbing | Add strongly-typed settings for allowed client IDs, audiences, `hd`, and refresh-token policies (`src/Melodee.Blazor/appsettings*.json`, `Directory.Build.props`). Ensure all Google Auth and auth policy values are configurable via `appsettings.json` and environment-specific variants (e.g., `appsettings.Development.json`, `appsettings.Production.json`), with support for overriding via environment variables in containerized deployments. | Backend | Config files + options binding tests in `tests/Melodee.Tests.Common` (e.g., `Configuration.GoogleAuthOptionsTests`) validating binding, validation, and environment override precedence; minimal startup integration test verifying app boots with Google enabled/disabled | 0.x | ✅ Done |
| 1.2 Google Credential Validator | Implement `GoogleTokenService` using `Google.Apis.Auth` with cacheable Google keys, clock-skew tolerance, and telemetry. Unit tests simulate valid/invalid tokens. | Backend | Service + unit tests in `tests/Melodee.Tests.Common.Security.GoogleTokenServiceTests` covering valid/invalid/expired tokens, hosted-domain restrictions, key caching/failure behavior, and logging/telemetry redaction (no raw tokens logged). | 1.1 | ✅ Done |
| 1.3 Data Model & Repos | Create migrations for `UserSocialLogin` (fields: `UserId`, `Provider`, `Subject`, `Email`, `LastLoginAt`), refresh token storage (`TokenId`, `HashedToken`, `IssuedAt`, `ExpiresAt`, `RevokedAt`). Update repositories/services for lookup + revocation. | Backend | EF migrations + repository APIs + unit tests in `tests/Melodee.Tests.Common.Data.Auth` validating `UserSocialLogin` queries and refresh-token persistence semantics (hashing only, revocation flags, indexes/lookup by `UserId`/`TokenId`). | 0.3 | ✅ Done |
| 1.4 JWT & Refresh Issuance | Refactor auth pipeline to emit JWT + refresh token, including rotation logic (store hashed token, invalidate on reuse, issue new pair). Update `AuthResponse`. | Backend | Auth service updates + unit and integration tests in `tests/Melodee.Tests.Common.Security.RefreshTokenFlowTests` covering initial issuance, rotation, replay detection (`refresh_token_replayed`), revocation, and absolute max session lifetime; regression tests ensuring password login behavior remains intact. | 1.3 | ✅ Done |
| 1.5 OpenAPI & Docs | Update README and dev docs with new request/response schemas and refresh flow diagrams. OpenAPI spec is auto-generated via Swagger at `/swagger`. | Docs owner | Updated specs + diagrams ensuring auth-related response models and canonical error codes match the implemented controllers. | 1.4 | ✅ Done |

**Phase 1 Testing & Quality Notes**

- Primary test home: `tests/Melodee.Tests.Common` (namespaces `*.Configuration`, `*.Security`, `*.Data.Auth`, `*.OpenApi`).
- Unit tests validate:
  - Correct options binding and validation from `appsettings*.json` and environment variables.
  - `GoogleTokenService` behavior across valid/invalid/expired tokens, hosted-domain checks, and JWKS caching/failure.
  - Refresh-token and `UserSocialLogin` persistence semantics (hashing, revocation, indexing, migrations applying cleanly using InMemory/Sqlite).
  - Auth service token issuance and rotation semantics independent of HTTP.
- Integration-style tests (using ASP.NET Core TestHost or equivalent) validate:
  - Application startup succeeds in Google-enabled and password-only configurations.
  - End-to-end token rotation flows (without HTTP controllers) behave as documented in Strategy B.

Milestone: Server can validate Google tokens, map users, and issue JWT + refresh tokens with well-covered unit tests for core auth logic.

---

## Phase 2 — API Endpoints & Account Linking

| Task | Description | Owner | Deliverables | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| 2.1 Google Exchange Endpoint | Add `POST /api/v1/Users/auth/google` that accepts ID tokens, calls validator, links/creates user, and returns JWT + refresh token. Include telemetry, rate limiting, and error taxonomy. | Backend | Controller + unit tests in a server test project (e.g., `Melodee.Tests.Server.Auth.GoogleAuthControllerTests`) covering new-user creation, existing linked users, auto-link by email, and all documented error codes (`invalid_google_token`, `expired_google_token`, `google_account_not_linked`, `signup_disabled`, `forbidden_tenant`, `account_disabled`); integration tests using ASP.NET Core TestServer verifying HTTP responses, DB state (`User`, `UserSocialLogin`, refresh tokens), telemetry calls, and absence of token leakage in logs. | Phase 1 | ✅ Done |
| 2.2 Refresh Endpoint | Implement `POST /api/v1/Users/auth/refresh` enforcing token rotation, replay detection, and blacklist/allowlist hooks. | Backend | Endpoint + unit tests (`*.RefreshControllerTests`) verifying success path, replay detection (`refresh_token_replayed`), invalid/expired/revoked tokens (`refresh_token_invalid`), absolute max-session behavior, and HTTP semantics (no GET support); integration tests covering multi-step refresh chains, replay attempts, and admin-triggered revocation behavior. | 1.4 | ✅ Done |
| 2.3 Link/Unlink APIs | Add `POST/DELETE /api/v1/Users/me/link/google` guarded by auth policies; enforce config (auto-link vs. manual). | Backend | Controller updates + unit tests (`*.GoogleLinkControllerTests`) for manual linking/unlinking flows, conflict scenarios (Google identity already linked elsewhere), hosted-domain enforcement, and safeguards preventing account hijacking; integration tests validating password-login → link → Google-login → unlink sequences and resulting behavior. | 2.1 | ✅ Done |
| 2.4 Admin & Ops Tooling | Extend admin dashboards/CLI to view linked providers, revoke refresh tokens, and audit logins, and to surface key Google Auth configuration values (e.g., `Auth:Google:Enabled`, `Auth:Google:AllowedHostedDomains`, `Auth:Google:AutoLinkEnabled`, `Auth:SelfRegistrationEnabled`) in a read-only or controlled-edit manner consistent with environment configuration practices. | Backend + Ops | Admin UI/CLI updates + tests verifying correct, permission-guarded exposure of Google Auth configuration, and that admin-triggered revocation or policy changes are reflected in subsequent auth/refresh requests without leaking PII or raw tokens. | 1.1 | ✅ Done |
| 2.5 API Auth Integration Suite | Consolidate end-to-end API tests for Google exchange, refresh, link/unlink, and admin revocation flows. | QA + Backend | Server integration tests (e.g., `Melodee.Tests.Server.Integration.AuthFlowTests`) that exercise full flows: new Google user signup, existing user auto/manual linking, refresh rotation with replay attempts, unlinking, and policy toggles (`AutoLinkEnabled`, `SelfRegistrationEnabled`, `AllowedHostedDomains`). | 2.1–2.4 | ✅ Done |

**Phase 2 Testing & Quality Notes**

- Primary test home: server test project(s), e.g., `Melodee.Tests.Server` plus `Melodee.Tests.Common` for shared helpers.
- Unit tests focus on controller/service behavior given mocked dependencies (token validator, repositories, configuration, telemetry), returning well-defined HTTP statuses and error codes.
- Integration tests use ASP.NET Core TestServer and real EF InMemory/Sqlite to validate persistence, auth flows, and policy enforcement end to end.
- Security-specific coverage includes:
  - Strict enforcement of hosted-domain and self-registration policies.
  - Detection and handling of refresh-token replay attempts.
  - Guarding link/unlink and admin endpoints against unauthorized access and account hijacking.

Milestone: Public API surface complete with Google exchange, refresh, and linking endpoints documented and covered by unit and integration tests.

---

## Phase 3 — Client Integration

| Task | Description | Owner | Deliverables | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| 3.1 Android Client | Implement Google Sign-In, call new exchange endpoint, store JWT + refresh tokens securely (Keystore-backed), handle rotation errors, and fall back to password login. | Mobile team | Android build + unit tests for the auth client (correct construction of `/auth/google` and `/auth/refresh` requests, secure token storage via Keystore/EncryptedSharedPreferences, and error handling for all server error codes) + instrumentation/E2E tests verifying login, refresh, and forced re-auth flows. | Phase 2 | ⏳ N/A (Mobile Team) |
| 3.2 Web/Blazor Client | Add Google login option, silent refresh handling, and secure storage (IndexedDB/secure cookies). | Web team | Web build + tests in `tests/Melodee.Tests.Blazor` for the Blazor auth service (Google login, silent refresh using `/auth/refresh`, shared token storage with password login, and handling of all auth error codes) plus bUnit component tests for login/auth UI components. | Phase 2 | ✅ Done |
| 3.3 Desktop/CLI Clients | Optionally add Google login or document fallback to password flow; ensure compatibility with refreshed API contract. | CLI team | CLI release notes + tests in `tests/Melodee.Tests.Cli` validating continued compatibility with updated auth API (e.g., error-code handling) and clearly documenting any intentional absence or limitation of Google Auth support for CLI. | Phase 2 | ⏳ N/A (CLI Team) |
| 3.4 User Education | Update onboarding docs, FAQs, and support scripts describing dual-login options and troubleshooting (e.g., unlinking, expired refresh token). | Docs/support | Knowledge base articles including a table mapping backend auth error codes to expected client UX responses (e.g., when to retry Google, when to fall back to password, when to contact support); QA validates this mapping against implemented client behavior. | 3.x | ⏳ N/A (Docs Team) |
| 3.5 Root README Update | Update the root `README.md` to document that Google authentication is supported alongside username/password in the Blazor UI and for API clients, including a short overview of configuration via `appsettings.json` and links to `/swagger` for API docs and user-facing docs. | Docs + Backend | Updated `README.md` with an "Authentication" section summarizing Blazor UI Google login, API client usage, and configuration basics, reviewed and approved as part of Phase 3 sign-off. | 2.x, 3.2 | ✅ Done |

**Phase 3 Testing & Quality Notes**

- Primary test homes:
  - Web/Blazor: `tests/Melodee.Tests.Blazor` (Auth service and login/auth components using bUnit and existing patterns described in the Blazor test README).
  - CLI: `tests/Melodee.Tests.Cli` for compatibility and error-handling tests.
  - Android/mobile: platform-specific unit and instrumentation tests in the corresponding mobile repo/project.
- Tests must confirm that:
  - Google-based sessions and password-based sessions share a common token storage abstraction and logout behavior.
  - Blazor `AuthService` (or equivalent) correctly interprets all documented auth error codes and translates them into user-facing states and messages.
  - No client stores tokens in insecure locations (e.g., browser `localStorage`) or logs token values.

Milestone: Clients can authenticate via Google, honor refresh rotation, and coexist with password auth, with automated tests validating primary flows and error handling.

---

## Phase 3A — Application Login Experience

| Task | Description | Owner | Deliverables | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| 3A.1 UX & Accessibility Design | Update the shared login UX to present both username/password and "Continue with Google" options, ensuring layout parity across desktop, tablet, and mobile plus WCAG-compliant focus order. Provide mockups and copy guidelines. | Design + Frontend | Updated design specs + Figma flows that include explicit accessibility acceptance criteria (tab order, ARIA labels, focus behavior, screen-reader text, and error message patterns for all auth errors). | Phase 3 | ✅ Done |
| 3A.2 UI Implementation | Add Google button, state management, and error-handling messaging to the login screen. Ensure password flow remains default-friendly, and Google initiation handles loading states and cancellation gracefully. | Frontend | UI commits + component-level tests in `tests/Melodee.Tests.Blazor` (e.g., `LoginComponentTests`) verifying rendering of both login options, config-driven visibility of the Google button, loading/cancellation behavior, keyboard navigation ordering, and correct error messages for server error codes. | 3A.1 | ✅ Done |
| 3A.3 Token Handling Wiring | Integrate Google Sign-In callback with the new exchange endpoint, persist JWT + refresh token pair, and share storage/utilities with the password flow. Handle rotation responses and forced re-auth. | Frontend | Auth module updates + unit tests in `tests/Melodee.Tests.Blazor.Services.Auth` verifying shared token storage utilities between Google and password flows, correct handling of `/auth/refresh` rotation responses, and forced re-auth when refresh fails (`refresh_token_replayed`, `refresh_token_invalid`). | 2.1, 2.2 | ✅ Done |
| 3A.4 Telemetry & Analytics | Emit events for login method selection, success/failure, and fallback to aid feature adoption tracking. Ensure no sensitive token data is logged. | Frontend + Data | Analytics dashboards/instrumentation + unit tests ensuring login events are emitted for Google vs password flows with redacted, non-PII payloads (no raw tokens, minimal identifiers). | 3A.2 | ✅ Done |
| 3A.5 UX Regression Testing | Run manual + automated UI tests covering keyboard navigation, screen readers, theming, and localization to confirm both flows behave identically aside from initiation path. | QA + Frontend | Test report + automated Blazor component tests (bUnit) exercising different themes/locales plus manual scripts validating keyboard-only navigation and screen-reader behavior across password and Google flows. | 3A.2 | ⏳ N/A (QA Team) |

**Phase 3A Testing & Quality Notes**

- Tests focus on the unified login experience rather than core auth logic:
  - Component tests ensure that the presence/absence of the Google button is driven solely by configuration and that both login options are reachable and usable via keyboard and screen readers.
  - Service tests ensure that login failures from the server side (e.g., `signup_disabled`, `forbidden_tenant`, `google_account_not_linked`) are surfaced with clear, localized user-facing messages.
  - Telemetry tests ensure adoption and failure metrics can be trusted while preserving privacy.

Milestone: Unified login screen presents both Google and password options with polished UX, telemetry, and complete auth wiring, backed by component and service-level tests.

---

## Phase 4 — Testing, Security, and Rollout

| Task | Description | Owner | Deliverables | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| 4.1 Automated Test Expansion | Add integration tests (server + clients), load tests for auth endpoints, and regression suites for password login. | QA | CI jobs + coverage report including: server integration suites for all auth flows (Google and password), Blazor/client tests for login/refresh/fallback, NBomber-based load tests for `/auth/google` and `/auth/refresh`, and a regression matrix across key configuration combinations (`Auth:Google:Enabled`, `Auth:Google:AutoLinkEnabled`, `Auth:SelfRegistrationEnabled`, `Auth:Google:AllowedHostedDomains`). | Phase 2 | ✅ Done |
| 4.2 Security Review & Pen Test | Perform threat modeling, verify refresh-token rotation, replay protection, rate limits, and logging hygiene. | Security | Review report + remediation tasks, with explicit mapping from threat-model findings to automated tests (e.g., replay tests, rate-limit tests, hosted-domain enforcement) and a checklist of manual pen-test scenarios that complement automated coverage. | 1.4, 2.2 | ✅ Done |
| 4.3 Staged Rollout | Deploy to staging, monitor metrics (auth success rate, Google validator latency), and gate GA release on SLOs. | DevOps | Rollout plan + dashboards + automated smoke tests for auth endpoints (Google and password) executed against staging before GA; dashboards with panels for auth success/failure by method, `refresh_token_replayed`/`refresh_token_invalid` rates, and Google API latency/failure. | 4.1 | ✅ Done |
| 4.4 Post-Launch Monitoring | Implement alerting on auth failures, refresh replay attempts, and Google API outages. Prepare rollback plan. | DevOps + Backend | Runbook + alerts + tests (in non-production environments) that validate alert triggers for auth failure spikes, refresh replay attempts, and Google API outages, plus CI configuration to run the full auth test suite on each release. | 4.3 | ✅ Done |

Milestone: Feature reaches GA with monitoring, documented recovery procedures, robust automated test coverage, and preserved password auth.

---

## Google Sign-In User Lifecycle

- **New users:** When `POST /api/v1/Users/auth/google` receives a valid Google ID token whose `sub` is unknown, `UserService` provisions a new `User` record and an associated `UserSocialLogin` entry (provider `Google`, `Subject`, verified email, timestamps). The standard JWT + refresh token pair is then issued so onboarding mirrors the password flow but without requiring a password.
- **Existing users:** The backend first looks for a matching `UserSocialLogin` by Google `sub`. If none exists, auto-linking may occur when the verified Google email uniquely matches an existing account and the Phase 0 policy enables auto-link. Otherwise, the request returns a deterministic error instructing the client to have the user log in with password and call `POST /api/v1/Users/me/link/google` to attach the Google identity manually.
- **Manual linking safeguards:** The link endpoint validates the Google token while the user is authenticated via password, preventing account hijacking. Admin-configurable rules can disable auto-link entirely or restrict it to specific hosted domains (`hd`).
- **Unlinking:** `DELETE /api/v1/Users/me/link/google` removes the `UserSocialLogin` record but retains refresh-token history so any outstanding tokens are revoked. Users revert to password login unless they re-link Google later.

## Error Codes & HTTP Mapping

The following canonical error codes must be used by auth-related endpoints and surfaced in `/swagger` (auto-generated OpenAPI spec) so clients can implement deterministic handling:

- `invalid_google_token` (400/401): Google ID token failed validation (signature, audience, issuer) or is malformed.
- `expired_google_token` (401): Google ID token is expired; client should re-initiate Google Sign-In.
- `google_account_not_linked` (409): Google identity is valid but not linked to any account and auto-link is disabled or ambiguous; client should prompt for password login then call link endpoint.
- `signup_disabled` (403): Self-service signup is disabled; new Google users cannot be auto-provisioned.
- `forbidden_tenant` (403): Google account’s hosted domain/email domain is not in the allowed list.
- `account_disabled` (403): Account exists but is disabled/locked; user must contact support.
- `refresh_token_replayed` (401): Refresh token reuse/replay detected; client must clear tokens and force full re-auth.
- `refresh_token_invalid` (401): Refresh token is invalid, expired, or revoked.

HTTP status codes should be stable across endpoints to reduce ambiguity.

## Security & Rate Limiting Guidance

- **Rate limiting:**
  - Apply per-IP and per-user limits to `POST /api/v1/Users/auth/google`, `POST /api/v1/Users/auth/refresh`, and `POST /api/v1/Users/me/link/google`.
  - Example starting points (tunable per deployment):
    - `/auth/google`: 10 requests/min per IP + soft per-email/sub limits.
    - `/auth/refresh`: 30 requests/min per refresh token family.
    - `/me/link/google`: 5 requests/hour per authenticated user.
- **Token storage on clients:**
  - Do **not** store access or refresh tokens in `localStorage` or other plainly accessible browser storage.
  - Prefer secure platform storage: Android Keystore/EncryptedSharedPreferences, HTTP-only secure cookies or IndexedDB with strong XSS/CSRF mitigation on web, and OS secret stores on desktop/CLI.
- **JWKS/Google keys:**
  - Cache Google public keys for their advertised lifetime.
  - On failure to fetch keys, fail closed (reject Google logins) and emit telemetry/alerts.
- **Logging & PII:**
  - Never log raw ID tokens, access tokens, or refresh tokens.
  - Log only high-level event types, non-sensitive identifiers, and coarse error categories, consistent with compliance policies.

### Admin Configuration for Google Auth

Google authentication and related policies are configured via standard ASP.NET Core configuration sources, with `appsettings.json` as the primary store and per-environment overrides.

- **Configuration locations**
  - Base defaults in `appsettings.json`.
  - Environment-specific overrides in `appsettings.{Environment}.json` (e.g., `appsettings.Development.json`, `appsettings.Staging.json`, `appsettings.Production.json`).
  - Sensitive values (e.g., secrets) and ops-specific overrides via environment variables or secret stores (e.g., Docker/Kubernetes secrets), following ASP.NET Core configuration precedence.
- **Key sections (example)**
  - `Auth:Google:Enabled`: `true/false` toggle for exposing Google Sign-In in the app.
  - `Auth:Google:ClientId`: Google OAuth client ID used by this server.
  - `Auth:Google:AllowedHostedDomains`: list of allowed hosted/email domains.
  - `Auth:Google:AutoLinkEnabled`: controls email-based auto-link behavior.
  - `Auth:SelfRegistrationEnabled`: allows/disallows new-account creation on first Google sign-in.
  - `Auth:Tokens:AccessTokenLifetimeMinutes`, `Auth:Tokens:RefreshTokenLifetimeDays`, `Auth:Tokens:MaxSessionDays`: optional overrides for token lifetimes.
- **Admin workflow**
  - Initial setup: create Google OAuth client in Google Cloud Console, configure redirect URIs for each environment, then populate the above keys in `appsettings.{Environment}.json` and/or environment variables.
  - Operations: use admin dashboards/CLI (Phase 2.4) to view current effective settings and, where supported, trigger configuration changes via environment-specific config management (e.g., updating appsettings and redeploying, or updating environment variables and restarting services).
  - Security: restrict write access to config (files, env vars, admin UI/CLI) to operators with appropriate privileges; audit all changes.

---

## Sequencing & Dependencies Summary

1. Complete Phase 0 decisions before coding to avoid rework.
2. Phase 1 lays the groundwork (config, data, token logic); Phase 2 builds endpoints on top.
3. Client teams (Phase 3) can start UI work early but depend on stable API contracts from Phase 2.
4. Testing/security/rollout (Phase 4) span all phases, with final gating after Phase 3 deliverables.

## Strategy B Implementation Notes

- **Storage:** Persist hashed refresh tokens with rotation metadata (`IssuedAt`, `ExpiresAt`, `PreviousTokenId`). Reject reused tokens and log incidents.
- **Rotation Flow:** Every refresh request validates the existing token, issues a new JWT + refresh token, stores the new hash, and invalidates the previous entry.
- **Revocation:** Support admin-triggered revocation by `UserId`, `TokenId`, or compromise flags; ensure next refresh fails with 401.
- **Client Guidance:** Clients must replace stored refresh tokens atomically when rotation succeeds and handle 401 by re-authenticating through Google or password.

## Testing & Acceptance Criteria

- Password login flows remain unchanged and fully tested (unit, integration, and UI tests where applicable).
- Google exchange returns JWT + refresh token; invalid tokens yield 401 with clear error codes, as validated by controller/unit tests and server integration tests.
- Refresh endpoint enforces rotation, rejects replay, and honors revocation lists, with tests covering positive and negative scenarios and load behaviors.
- Link/unlink flows validated via automated and manual tests; existing users can attach/detach Google accounts without data loss or hijacking risk.
- End-to-end tests cover Android/web/CLI happy paths and fallback to password when Google login fails, including handling of all documented auth error codes.
- Admin configuration paths are validated for each environment: changing `appsettings.{Environment}.json` and/or environment variables produces the expected effective values in the running application and is reflected consistently in any admin UI/CLI surfaces.
- A regression matrix is maintained and tested across key auth configuration combinations (Google enabled/disabled, auto-link on/off, self-registration on/off, domain restrictions enabled/disabled).
