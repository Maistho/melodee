# Container Testing Session - 2026-01-04

## Environment Setup

- **Container Runtime**: Podman with podman-compose
- **Setup Script**: `scripts/run-container-setup.py --start`
- **Port**: 8080 (HTTP)
- **Database**: PostgreSQL 17.7

## Test Checklist

| Test | Status | Notes |
|------|--------|-------|
| Purge podman and fresh start | ✅ Pass | Used `podman system prune -a -f --volumes` |
| Container build | ✅ Pass | Multi-stage build completed successfully |
| Database migrations | ✅ Pass | 18+ migrations applied automatically |
| Create user account | ✅ Pass | Registration flow works |
| User login | ✅ Pass | Authentication works |
| Change profile image/avatar | ✅ Pass | Avatar upload functional |
| Navigate all menu items | ✅ Pass | All nav elements accessible |
| View /admin/doctor | ✅ Pass | Doctor page loads |
| Run MusicBrainzUpdateDatabaseJob | 🔄 In Progress | Job started, downloading mbdump.tar.bz2 |
| View /admin/jobs | ✅ Pass | Job status visible |
| Upload album to inbound | ⏳ Pending | Waiting for MusicBrainz job |
| View dashboard | ⏳ Pending | |
| View stats | ⏳ Pending | |
| Play song via API | ⏳ Pending | |

## Issues Discovered

### 1. JWT Configuration Issue (High Priority) ✅ FIXED

**Location**: `/admin/doctor` page - JWT Token Strength check
**Error**: "JWT key is not configured"

**Root Cause**: Two issues found:
1. The `compose.yml` set `MelodeeAuthSettings__Token` but `Program.cs` requires `Jwt:Key`, `Jwt:Issuer`, and `Jwt:Audience` for JWT Bearer authentication setup.
2. The Doctor health check only looked at `Jwt:Key`, not the fallback configuration sources.

**Fixes Applied**:

1. **compose.yml** - Added missing JWT environment variables:
```yaml
- Jwt__Key=${MELODEE_AUTH_TOKEN:-...}
- Jwt__Issuer=${JWT_ISSUER:-MelodeeApi}
- Jwt__Audience=${JWT_AUDIENCE:-MelodeeClient}
```

2. **DoctorService.cs** - Updated `RunJwtTokenStrengthCheck()` to check all configuration sources (container, non-container, and .env):
```csharp
// Priority order: Jwt:Key -> MelodeeAuthSettings:Token -> MELODEE_AUTH_TOKEN env var
var jwtKey = configuration.GetValue<string>("Jwt:Key");

if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = configuration.GetValue<string>("MelodeeAuthSettings:Token");
}

if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = Environment.GetEnvironmentVariable("MELODEE_AUTH_TOKEN");
}
```

---

### 2. Missing Localization Key (Medium Priority)

**Location**: Admin Doctor page
**Key**: `AdminDoctor.MusicBrainzJobDescription`
**Error Log**:
```
[21:22:58 Melodee.Blazor.Services.LocalizationService [Warning] Resource key not found: "AdminDoctor.MusicBrainzJobDescription"
```

**Fix Required**: Add key to all 10 resource files:
- `src/Melodee.Blazor/Resources/SharedResources.resx`
- `src/Melodee.Blazor/Resources/SharedResources.de-DE.resx`
- `src/Melodee.Blazor/Resources/SharedResources.es-ES.resx`
- `src/Melodee.Blazor/Resources/SharedResources.fr-FR.resx`
- `src/Melodee.Blazor/Resources/SharedResources.it-IT.resx`
- `src/Melodee.Blazor/Resources/SharedResources.ja-JP.resx`
- `src/Melodee.Blazor/Resources/SharedResources.pt-BR.resx`
- `src/Melodee.Blazor/Resources/SharedResources.ru-RU.resx`
- `src/Melodee.Blazor/Resources/SharedResources.zh-CN.resx`
- `src/Melodee.Blazor/Resources/SharedResources.ar-SA.resx`

**Suggested Value**: "Downloads and updates the local MusicBrainz database for artist and album metadata lookups."

---

### 2. HTTPS Redirect Warning (Low Priority - Expected)

**Error Log**:
```
[21:19:00 Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware [Warning] Failed to determine the https port for redirect.
```

**Explanation**: Normal behavior when running HTTP-only in container/development mode. Not an issue for local testing.

**Fix**: None required. For production, configure HTTPS properly or use a reverse proxy.

---

### 3. XML Encryptor Warning (Low Priority)

**Error Log**:
```
[21:18:43 Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager [Warning] No XML encryptor configured. Key {4d65fee8-21f2-4a5e-bcb7-f7cfce040bdd} may be persisted to storage in unencrypted form.
```

**Explanation**: Data protection keys stored unencrypted. Acceptable for development.

**Fix for Production**: Configure data protection with encryption:
```csharp
services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/app/keys"))
    .ProtectKeysWithCertificate(certificate);
```

---

### 4. Blazor Circuit Disconnections (Informational - Normal)

**Error Log**:
```
[21:21:18 Melodee.Blazor.Services.MelodeeCircuitHandler [Warning] Connection lost for circuit: "KpYmd49W8YGs4QKNQpTh1QBokyjFZbRaMdV8kiPPo-c". Client may be attempting to reconnect.
```

**Explanation**: Normal Blazor Server SignalR behavior when:
- User navigates between pages
- User refreshes browser
- Network momentarily disconnects

**Fix**: None required. This is expected behavior.

---

### 5. Localization Save Error on Circuit Disconnect (Low Priority)

**Error Log**:
```
[21:23:38 Melodee.Blazor.Services.LocalizationService [Error] Error saving culture to local storage
Microsoft.JSInterop.JSDisconnectedException: JavaScript interop calls cannot be issued at this time. This is because the circuit has disconnected and is being disposed.
```

**Explanation**: Attempting to save user's language preference to localStorage after the SignalR circuit has already disconnected.

**File**: `src/Melodee.Blazor/Services/LocalizationService.cs`

**Potential Fix**: Wrap localStorage calls in try-catch or check circuit state before JS interop:
```csharp
try
{
    await _localStorage.SetItemAsync("culture", culture);
}
catch (JSDisconnectedException)
{
    // Circuit disconnected, ignore - preference will be re-read on reconnect
}
```

---

### 6. Cover Image Request Cancelled (Informational)

**Error Log**:
```
[21:21:38  [Error] Failed to get cover image for requested resource.
System.OperationCanceledException: The operation was canceled.
```

**Explanation**: Image request was cancelled, likely because user navigated away before image loaded.

**Fix**: Consider downgrading log level from Error to Debug/Warning for `OperationCanceledException` in image loading code.

---

### 7. ResponseData Invalid Warning (Needs Investigation)

**Error Log**:
```
[21:21:38  [Warning] ResponseData is invalid for ApiKey ["user_76a86b57-e2bd-4bf8-935e-4637df7a9286"]
```

**Explanation**: API response validation failed. May be related to the cancelled cover image request above.

**Action**: Monitor if this occurs outside of cancellation scenarios.

---

## Container Commands Reference

```bash
# View live logs
podman compose logs -f

# Check container status
podman compose ps

# Stop containers
podman compose down

# Rebuild and restart
podman compose up -d --build

# Check specific service logs
podman compose logs melodee.blazor
podman compose logs melodee-db

# Filter logs for errors
podman compose logs melodee.blazor 2>&1 | grep -iE "(error|exception|fail)"
```

## Files Modified During Setup

- `.env` - Generated with secure secrets (DB_PASSWORD, MELODEE_AUTH_TOKEN)
- Container volumes created for:
  - PostgreSQL data
  - App storage (`/app/storage`)
  - Inbound folder (`/app/inbound`)
  - Staging folder (`/app/staging`)
  - User images (`/app/user-images`)

## Next Steps

1. [ ] Fix missing localization key `AdminDoctor.MusicBrainzJobDescription`
2. [ ] Complete MusicBrainz database download job
3. [ ] Test album upload to inbound
4. [ ] Test album processing pipeline
5. [ ] Test dashboard with processed albums
6. [ ] Test statistics page
7. [ ] Test song playback via OpenSubsonic API
8. [ ] Consider improving error handling for JS interop on circuit disconnect

---

## Session Notes

_Add any additional observations during testing below:_

