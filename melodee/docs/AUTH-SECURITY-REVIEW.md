# Authentication Security Review & Threat Model

**Document Version:** 1.0  
**Last Updated:** December 2024  
**Status:** Active  

This document fulfills WBS Phase 4.2 requirements: threat modeling, refresh-token rotation verification, replay protection, rate limits, and logging hygiene.

---

## 1. Threat Model Overview

### 1.1 Assets Under Protection

| Asset | Sensitivity | Impact if Compromised |
|-------|-------------|----------------------|
| User credentials (password hashes) | Critical | Full account takeover |
| Refresh tokens | High | Session hijacking |
| Access tokens (JWTs) | Medium | Short-term unauthorized access |
| Google OAuth tokens | High | Account linkage manipulation |
| User PII (email, name) | High | Privacy violation |

### 1.2 Threat Actors

| Actor | Motivation | Capability |
|-------|------------|------------|
| External attacker | Financial gain, disruption | Network access, credential stuffing |
| Compromised client | Stolen device, malware | Access to local storage |
| Malicious insider | Data theft | Internal system access |
| Automated bots | Credential stuffing, DoS | High volume requests |

---

## 2. Authentication Threats & Mitigations

### 2.1 Token Theft

**Threat:** Attacker obtains refresh token from compromised device or network interception.

**Mitigations Implemented:**
- ✅ Refresh tokens hashed before storage (`SHA-256`)
- ✅ Short-lived access tokens (15 minutes default)
- ✅ Token rotation on every refresh (one-time use)
- ✅ Replay detection revokes entire token family
- ✅ HTTPS enforced for all auth endpoints

**Automated Test Coverage:**
- `RefreshTokenServiceTests.RotateToken_InvalidatesOldToken_Success`
- `AuthFlowIntegrationTests.ReplayAttack_ReusingOldToken_IsDetected`
- `AuthFlowIntegrationTests.ReplayDetection_RevokesTokenFamily`

### 2.2 Replay Attacks

**Threat:** Attacker intercepts a refresh token and attempts reuse.

**Mitigations Implemented:**
- ✅ Single-use refresh tokens with rotation
- ✅ `PreviousTokenId` tracking for chain validation
- ✅ Replay detection triggers family-wide revocation
- ✅ Logged as security event: `refresh_token_replayed`

**Automated Test Coverage:**
- `RefreshTokenServiceTests.RotateToken_ReplayDetection_RevokesFamilyAndReturnsError`
- `AuthEndpointLoadTests.RefreshTokenReplayDetection_LoadTest_DetectsReplays`

### 2.3 Credential Stuffing

**Threat:** Automated attempts using leaked credentials from other sites.

**Mitigations Implemented:**
- ✅ Rate limiting on `/auth/authenticate` (10 req/min per IP)
- ✅ Rate limiting on `/auth/google` (10 req/min per IP)
- ✅ Rate limiting on `/auth/refresh-token` (30 req/min per token family)
- ✅ Account lockout after failed attempts
- ✅ IP and email blacklisting support

**Configuration:**
```json
{
  "RateLimiting": {
    "MelodeeAuth": {
      "PermitLimit": 10,
      "WindowSeconds": 60
    }
  }
}
```

### 2.4 Session Fixation

**Threat:** Attacker sets a known session ID before victim authenticates.

**Mitigations Implemented:**
- ✅ New token pair issued on every successful authentication
- ✅ Old tokens not reusable after rotation
- ✅ Server-side token generation (not client-influenced)

### 2.5 Account Hijacking via Google Linking

**Threat:** Attacker links their Google account to victim's Melodee account.

**Mitigations Implemented:**
- ✅ Link endpoint requires prior password authentication
- ✅ Auto-link disabled by default (`Auth:Google:AutoLinkEnabled = false`)
- ✅ Hosted domain restrictions available (`Auth:Google:AllowedHostedDomains`)
- ✅ Google subject (`sub`) uniqueness enforced

**Automated Test Coverage:**
- `UserControllerTests.LinkGoogleAsync_HasAuthorizeAttribute`
- `UserService.LinkSocialLogin_AlreadyLinkedToOther_ReturnsConflict` (implicit via service tests)

---

## 3. Rate Limiting Configuration

### 3.1 Endpoint Rate Limits

| Endpoint | Limit | Window | Scope |
|----------|-------|--------|-------|
| `POST /auth/authenticate` | 10 | 1 minute | Per IP |
| `POST /auth/google` | 10 | 1 minute | Per IP |
| `POST /auth/refresh-token` | 30 | 1 minute | Per token family |
| `POST /user/me/link/google` | 5 | 1 hour | Per authenticated user |
| `POST /auth/password-reset/*` | 3 | 15 minutes | Per IP + email |

### 3.2 Rate Limit Responses

When rate limited, endpoints return:
```json
{
  "error": "rate_limited",
  "message": "Too many requests. Please try again later.",
  "retryAfter": 60
}
```

HTTP Status: `429 Too Many Requests`  
Header: `Retry-After: <seconds>`

---

## 4. Logging & Telemetry

### 4.1 Auth Events Logged

| Event | Level | Data Included | Data Excluded |
|-------|-------|--------------|---------------|
| Login success | Info | Method, IP, UserID | Token values |
| Login failure | Warning | Method, IP, Identifier | Password, Token |
| Token rotation | Debug | UserID, DeviceID | Token values |
| Replay detected | Warning | UserID, IP | Token values |
| Account locked | Warning | UserID, IP | N/A |
| Google auth | Info | Outcome, IP, Email domain | Full email, Token |

### 4.2 PII Handling

**Never logged:**
- Raw tokens (access, refresh, Google ID)
- Passwords (plain or hashed)
- Full email addresses in production logs
- Google subject identifiers

**Sanitization applied:**
- Email addresses masked: `j***@example.com`
- IP addresses may be truncated for GDPR: `192.168.x.x`
- Correlation IDs included for tracing without PII

### 4.3 Log Example

```
2024-12-19 21:00:00 [INF] Auth event: Method=google, Outcome=success_new_user, ClientIp=192.168.1.x, Identifier=j***@example.com, UserId=42
```

---

## 5. Manual Pen Test Scenarios

These scenarios complement automated tests and should be executed before GA:

### 5.1 Token Security Tests

| # | Scenario | Expected Result | Automated? |
|---|----------|-----------------|------------|
| 1 | Replay refresh token after rotation | 401 + `refresh_token_replayed` | ✅ Yes |
| 2 | Use expired refresh token | 401 + `refresh_token_invalid` | ✅ Yes |
| 3 | Tamper with JWT signature | 401 Unauthorized | ❌ Manual |
| 4 | Modify JWT claims (email, userId) | 401 Unauthorized | ❌ Manual |
| 5 | Use refresh token from different device | Success (device tracking only) | ✅ Yes |

### 5.2 Google Auth Tests

| # | Scenario | Expected Result | Automated? |
|---|----------|-----------------|------------|
| 6 | Invalid Google ID token signature | 400 + `invalid_google_token` | ✅ Yes |
| 7 | Expired Google ID token | 401 + `expired_google_token` | ✅ Yes |
| 8 | Google token from wrong client ID | 400 + `invalid_google_token` | ✅ Yes |
| 9 | Google token with unauthorized domain | 403 + `forbidden_tenant` | ✅ Yes |
| 10 | Link Google to already-linked user | 409 + `google_already_linked` | ✅ Yes |

### 5.3 Rate Limiting Tests

| # | Scenario | Expected Result | Automated? |
|---|----------|-----------------|------------|
| 11 | 15 login attempts in 1 minute | 429 after 10th | ❌ Manual |
| 12 | Distributed attack from multiple IPs | Individual limits apply | ❌ Manual |
| 13 | Rate limit header presence | `Retry-After` header included | ❌ Manual |

### 5.4 Account Security Tests

| # | Scenario | Expected Result | Automated? |
|---|----------|-----------------|------------|
| 14 | Login to locked account | 403 + `account_disabled` | ✅ Yes |
| 15 | Refresh token for locked account | 403 + `account_disabled` | ✅ Yes |
| 16 | Blacklisted email login | 403 + blacklisted response | ✅ Yes |
| 17 | Blacklisted IP login | 403 + blacklisted response | ✅ Yes |

---

## 6. Security Review Checklist

### 6.1 Pre-GA Checklist

- [ ] All automated security tests passing
- [ ] Manual pen test scenarios executed
- [ ] Rate limiting verified in staging
- [ ] Log sanitization confirmed (no PII leaks)
- [ ] HTTPS enforced in production
- [ ] Google OAuth client IDs configured per environment
- [ ] JWT signing key rotated from development value
- [ ] Refresh token storage encryption verified
- [ ] Monitoring alerts configured (see RUNBOOK.md)

### 6.2 Ongoing Security Tasks

- [ ] Quarterly review of token lifetimes
- [ ] Annual pen test by external party
- [ ] Monitor for unusual auth patterns
- [ ] Review rate limit effectiveness
- [ ] Update threat model for new features

---

## 7. Remediation Tracking

| Finding | Severity | Status | Remediation |
|---------|----------|--------|-------------|
| N/A | - | - | No findings from initial review |

*Update this section as security reviews identify issues.*

---

## 8. References

- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [Google Sign-In Security Guidelines](https://developers.google.com/identity/sign-in/web/sign-in)
- [JWT Best Practices (RFC 8725)](https://datatracker.ietf.org/doc/html/rfc8725)
- WBS Document: `prompts/WBS-GOOGLE-AUTH.md`
- Runbook: `docs/RUNBOOK-AUTH.md`
