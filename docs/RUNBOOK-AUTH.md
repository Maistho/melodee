# Authentication Operations Runbook

**Document Version:** 1.0  
**Last Updated:** December 2024  
**Status:** Active  

This document fulfills WBS Phase 4.3/4.4 requirements: staged rollout guidance, monitoring dashboards, alerting configuration, and incident response procedures.

---

## 1. Staged Rollout Plan

### 1.1 Environment Progression

```
Development → Staging → Production (Canary 5%) → Production (25%) → Production (100%)
```

### 1.2 Gate Criteria

Before promoting to next stage, verify:

| Metric | Threshold | Tool |
|--------|-----------|------|
| Auth success rate | ≥ 99% | Prometheus/Grafana |
| P95 latency `/auth/*` | < 500ms | Prometheus/Grafana |
| Error rate (5xx) | < 0.1% | Application logs |
| Replay detection rate | < 0.01% (expected baseline) | Custom metric |
| Google API latency | < 2s | External monitoring |

### 1.3 Rollback Triggers

**Automatic rollback if:**
- Auth success rate drops below 95%
- Error rate exceeds 1% for 5 minutes
- P95 latency exceeds 2 seconds for 10 minutes

**Manual rollback if:**
- Replay detection rate spikes unexpectedly
- Security incident detected
- Google API unavailable > 5 minutes

---

## 2. Monitoring Configuration

### 2.1 Prometheus Metrics

Add to application metrics (already instrumented via ASP.NET Core):

```yaml
# Custom auth metrics
melodee_auth_attempts_total{method, outcome, error_code}
melodee_auth_refresh_rotations_total{outcome}
melodee_auth_replay_detections_total
melodee_google_token_validations_total{outcome}
melodee_google_api_latency_seconds{quantile}
```

### 2.2 Grafana Dashboard Panels

**Authentication Overview Dashboard:**

| Panel | Query | Visualization |
|-------|-------|---------------|
| Auth Success Rate | `sum(rate(melodee_auth_attempts_total{outcome="success"}[5m])) / sum(rate(melodee_auth_attempts_total[5m]))` | Gauge |
| Auth by Method | `sum by (method) (rate(melodee_auth_attempts_total[5m]))` | Pie chart |
| Error Distribution | `sum by (error_code) (rate(melodee_auth_attempts_total{outcome!="success"}[5m]))` | Bar chart |
| Token Rotations | `rate(melodee_auth_refresh_rotations_total[5m])` | Time series |
| Replay Detections | `rate(melodee_auth_replay_detections_total[5m])` | Time series |
| Google API Latency | `histogram_quantile(0.95, melodee_google_api_latency_seconds)` | Time series |

### 2.3 Log Queries (for ELK/Loki)

**Auth failures by type:**
```
{app="melodee"} |= "Auth event" | json | outcome != "success" | __error__="" | count by (outcome, error_code)
```

**Replay attack attempts:**
```
{app="melodee"} |= "refresh_token_replayed" | count_over_time([1h])
```

**Google auth issues:**
```
{app="melodee"} |= "Auth event" |= "google" | json | outcome =~ "invalid.*|expired.*|forbidden.*"
```

---

## 3. Alert Configuration

### 3.1 Critical Alerts (PagerDuty/OpsGenie)

```yaml
# Auth success rate critical
- alert: AuthSuccessRateCritical
  expr: |
    (sum(rate(melodee_auth_attempts_total{outcome="success"}[5m])) / 
     sum(rate(melodee_auth_attempts_total[5m]))) < 0.95
  for: 5m
  labels:
    severity: critical
  annotations:
    summary: "Auth success rate below 95%"
    description: "Authentication success rate is {{ $value | humanizePercentage }}"
    runbook: "https://docs.melodee.app/runbook#auth-success-rate-low"

# Replay attack spike
- alert: ReplayAttackSpike
  expr: rate(melodee_auth_replay_detections_total[5m]) > 10
  for: 2m
  labels:
    severity: critical
  annotations:
    summary: "Unusual replay attack activity detected"
    description: "{{ $value }} replay attempts per second"
    runbook: "https://docs.melodee.app/runbook#replay-attack-response"

# Google API unavailable
- alert: GoogleAPIUnavailable
  expr: |
    sum(rate(melodee_google_token_validations_total{outcome="error"}[5m])) / 
    sum(rate(melodee_google_token_validations_total[5m])) > 0.5
  for: 5m
  labels:
    severity: critical
  annotations:
    summary: "Google token validation failing"
    description: "Google API error rate is {{ $value | humanizePercentage }}"
    runbook: "https://docs.melodee.app/runbook#google-api-outage"
```

### 3.2 Warning Alerts (Slack/Email)

```yaml
# Auth latency degraded
- alert: AuthLatencyDegraded
  expr: histogram_quantile(0.95, melodee_auth_latency_seconds) > 1
  for: 10m
  labels:
    severity: warning
  annotations:
    summary: "Auth endpoint latency elevated"
    description: "P95 latency is {{ $value }}s"

# Elevated error rate
- alert: AuthErrorRateElevated
  expr: |
    sum(rate(melodee_auth_attempts_total{outcome!="success"}[5m])) / 
    sum(rate(melodee_auth_attempts_total[5m])) > 0.05
  for: 10m
  labels:
    severity: warning
  annotations:
    summary: "Auth error rate above 5%"
```

### 3.3 Alert Testing

Run these in non-production to verify alert triggers:

```bash
# Simulate auth failures (staging only)
for i in {1..100}; do
  curl -X POST https://staging.melodee.app/api/v1/auth/authenticate \
    -H "Content-Type: application/json" \
    -d '{"email":"bad@example.com","password":"wrong"}'
done

# Verify alert fired in alertmanager
```

---

## 4. Incident Response Procedures

### 4.1 Auth Success Rate Low

**Symptoms:**
- Users unable to log in
- `AuthSuccessRateCritical` alert firing
- Elevated 401/403 responses

**Investigation Steps:**

1. **Check error distribution:**
   ```bash
   # Identify which error codes are elevated
   curl -s "http://prometheus:9090/api/v1/query?query=sum%20by%20(error_code)%20(rate(melodee_auth_attempts_total{outcome!=%22success%22}[5m]))"
   ```

2. **Check recent deployments:**
   ```bash
   kubectl rollout history deployment/melodee-api
   ```

3. **Check database connectivity:**
   ```bash
   kubectl exec -it deployment/melodee-api -- curl -s http://localhost:8080/health/ready
   ```

4. **Check JWT signing key:**
   ```bash
   # Verify key is set
   kubectl get secret melodee-jwt-key -o jsonpath='{.data.key}' | base64 -d | head -c 10
   ```

**Resolution:**

| Root Cause | Resolution |
|------------|------------|
| Database unreachable | Restore DB connectivity, check connection pool |
| JWT key missing/changed | Restore from backup, redeploy |
| Bad deployment | `kubectl rollout undo deployment/melodee-api` |
| Rate limiting too aggressive | Temporarily increase limits |

### 4.2 Replay Attack Response

**Symptoms:**
- `ReplayAttackSpike` alert firing
- Elevated `refresh_token_replayed` logs
- User complaints about forced re-login

**Investigation Steps:**

1. **Identify affected users:**
   ```sql
   SELECT DISTINCT rt.UserId, u.Email, COUNT(*) as replays
   FROM RefreshTokens rt
   JOIN Users u ON rt.UserId = u.Id
   WHERE rt.RevokedAt IS NOT NULL 
     AND rt.RevokedReason = 'replay_detected'
     AND rt.RevokedAt > NOW() - INTERVAL 1 HOUR
   GROUP BY rt.UserId, u.Email
   ORDER BY replays DESC
   LIMIT 20;
   ```

2. **Check for patterns:**
   - Multiple users from same IP?
   - Single user across many IPs?
   - Specific device types?

3. **Review IP addresses:**
   ```bash
   grep "refresh_token_replayed" /var/log/melodee/*.log | \
     awk '{print $NF}' | sort | uniq -c | sort -rn | head -20
   ```

**Resolution:**

| Scenario | Action |
|----------|--------|
| Targeted user attack | Contact user, force password reset |
| Widespread bot attack | Add suspect IPs to blacklist |
| Client bug (double-submit) | Work with client team to fix |
| Network issue causing retries | Investigate infrastructure |

### 4.3 Google API Outage

**Symptoms:**
- `GoogleAPIUnavailable` alert firing
- Google logins failing with `invalid_google_token`
- Password logins still working

**Investigation Steps:**

1. **Check Google status:**
   - https://www.google.com/appsstatus/dashboard/
   - https://status.cloud.google.com/

2. **Test Google connectivity:**
   ```bash
   kubectl exec -it deployment/melodee-api -- \
     curl -s "https://www.googleapis.com/oauth2/v3/certs" | head
   ```

3. **Check JWKS cache:**
   ```bash
   # Check if keys are cached
   kubectl exec -it deployment/melodee-api -- \
     cat /tmp/google-jwks-cache.json
   ```

**Resolution:**

| Scenario | Action |
|----------|--------|
| Google outage | Wait, direct users to password login |
| Network block | Check firewall, allow googleapis.com |
| Certificate issue | Update CA certificates |
| Client ID revoked | Regenerate in Google Cloud Console |

---

## 5. Smoke Tests

Run before and after deployments:

```bash
#!/bin/bash
# auth-smoke-tests.sh

BASE_URL="${1:-https://staging.melodee.app}"

echo "=== Auth Smoke Tests ==="

# Test 1: Password login endpoint available
echo -n "Password login endpoint... "
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" \
  -X POST "$BASE_URL/api/v1/auth/authenticate" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"wrong"}')
if [ "$HTTP_CODE" = "401" ]; then echo "PASS"; else echo "FAIL ($HTTP_CODE)"; fi

# Test 2: Google auth endpoint available
echo -n "Google auth endpoint... "
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" \
  -X POST "$BASE_URL/api/v1/auth/google" \
  -H "Content-Type: application/json" \
  -d '{"idToken":"invalid"}')
if [ "$HTTP_CODE" = "400" ]; then echo "PASS"; else echo "FAIL ($HTTP_CODE)"; fi

# Test 3: Refresh endpoint available
echo -n "Refresh token endpoint... "
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" \
  -X POST "$BASE_URL/api/v1/auth/refresh-token" \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"invalid"}')
if [ "$HTTP_CODE" = "401" ]; then echo "PASS"; else echo "FAIL ($HTTP_CODE)"; fi

# Test 4: Health check
echo -n "Health check... "
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/health")
if [ "$HTTP_CODE" = "200" ]; then echo "PASS"; else echo "FAIL ($HTTP_CODE)"; fi

# Test 5: Rate limiting header present
echo -n "Rate limit headers... "
HEADERS=$(curl -s -I -X POST "$BASE_URL/api/v1/auth/authenticate" \
  -H "Content-Type: application/json" \
  -d '{}' 2>&1)
if echo "$HEADERS" | grep -qi "x-rate-limit"; then echo "PASS"; else echo "FAIL (no rate limit header)"; fi

echo "=== Complete ==="
```

---

## 6. Configuration Reference

### 6.1 Auth Settings Location

```
appsettings.json
├── Auth
│   ├── Google
│   │   ├── Enabled: true/false
│   │   ├── ClientId: "xxx.apps.googleusercontent.com"
│   │   ├── AllowedHostedDomains: ["example.com"]
│   │   └── AutoLinkEnabled: false
│   ├── Tokens
│   │   ├── AccessTokenLifetimeMinutes: 15
│   │   ├── RefreshTokenLifetimeDays: 30
│   │   └── MaxSessionDays: 90
│   └── SelfRegistrationEnabled: true
└── Jwt
    ├── Key: "your-256-bit-secret"
    ├── Issuer: "melodee"
    └── Audience: "melodee-clients"
```

### 6.2 Environment Variable Overrides

```bash
# Production overrides via environment
Auth__Google__Enabled=true
Auth__Google__ClientId=xxx.apps.googleusercontent.com
Auth__Tokens__AccessTokenLifetimeMinutes=15
Jwt__Key=<from secret manager>
```

---

## 7. Rollback Procedure

### 7.1 Quick Rollback

```bash
# Kubernetes
kubectl rollout undo deployment/melodee-api

# Docker Compose
docker-compose pull melodee-api:previous
docker-compose up -d melodee-api
```

### 7.2 Database Rollback (if migration applied)

```bash
# List migrations
dotnet ef migrations list --project src/Melodee.Common

# Rollback specific migration
dotnet ef database update <previous-migration-name> --project src/Melodee.Common
```

### 7.3 Feature Flag Disable

```bash
# Disable Google auth without rollback
kubectl set env deployment/melodee-api Auth__Google__Enabled=false
```

---

## 8. Contacts

| Role | Contact | Escalation |
|------|---------|------------|
| On-call engineer | via PagerDuty | Automatic |
| Security team | security@melodee.app | Critical only |
| Google Cloud support | via GCP Console | API outages |

---

## 9. Change Log

| Date | Version | Changes |
|------|---------|---------|
| 2024-12 | 1.0 | Initial document for Google Auth GA |
