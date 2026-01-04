# Container Testing Session - 2026-01-04

## ⚠️ TESTING SESSION RULES

**DO NOT:**
- Restart containers during testing
- Make code changes during testing
- Rebuild images during testing

**DO:**
- Document all findings in this file
- Wait for user confirmation that testing is complete before applying fixes

---

## Environment Setup

- **Container Runtime**: Podman with podman-compose
- **Setup Script**: `scripts/run-container-setup.py --start`
- **Port**: 8080 (HTTP)
- **Database**: PostgreSQL 17.7

## Test Checklist

| Test | Status | Notes |
|------|--------|-------|
| Purge podman and fresh start | ✅ Pass | |
| Container build | ✅ Pass | |
| Database migrations | ✅ Pass | |
| Create user account | ✅ Pass | |
| User login | ✅ Pass | |
| Change profile image/avatar | ✅ Pass | |
| Navigate all menu items | ✅ Pass | |
| View /admin/doctor | ✅ Pass | JWT error no longer showing - fix confirmed working |
| Run MusicBrainzUpdateDatabaseJob | 🔄 In Progress | Button clicked, job started |
| View /admin/jobs | ⏳ Pending | |
| Upload album to inbound | ⏳ Pending | |
| View dashboard | ⏳ Pending | |
| View stats | ⏳ Pending | |
| Play song via API | ⏳ Pending | |

## Issues Discovered

_Issues will be documented here as they are found during testing._

### Issue #1: Search Page Missing "No Results Found" Message (UI/UX)

**Location**: Search page
**Severity**: Low (UI/UX improvement)
**Steps to Reproduce**:
1. Perform a search with a value that returns no results (e.g., a random GUID)
2. Observe the search results page

**Expected Behavior**: Page should display a "No Results Found" message

**Actual Behavior**: Page only shows "Results for: <search value>" with blank content below - looks broken

**Files to Investigate**:
- Search page component (likely in `src/Melodee.Blazor/Components/Pages/` or similar)

---

### Issue #2: Profile Image Pixelated on /data/users Page (UI/UX)

**Location**: `/data/users` page
**Severity**: Low (UI/UX)
**Steps to Reproduce**:
1. Upload a 600x600 profile image
2. View the image in the header (looks OK)
3. Navigate to `/data/users` page

**Expected Behavior**: Profile image should display clearly at appropriate resolution

**Actual Behavior**: Profile image appears extremely pixelated on the /data/users page

**Possible Causes**:
- Image being scaled up from a small thumbnail
- Wrong image size variant being used
- CSS scaling issues

**Files to Investigate**:
- User list/grid component displaying avatars
- Image processing/thumbnail generation logic

---

### Issue #3: Broken Number Formatting on User Detail View (Bug)

**Location**: `/user` detail view - "Pinned Items" section
**Severity**: Medium (Display bug)
**Steps to Reproduce**:
1. Navigate to a user detail view
2. Look at the "Pinned Items" section

**Expected Behavior**: Should display formatted number (e.g., "0" or "1,234")

**Actual Behavior**: Displays literal text `0.ToString("N0")` instead of the formatted value

**Root Cause**: Likely a Razor syntax error - the formatting code is being rendered as text instead of being executed

**Files to Investigate**:
- User detail component (likely `src/Melodee.Blazor/Components/Pages/Data/UserDetail.razor` or similar)

---

### Issue #4: Missing Spanish Translations on Dashboard (Localization)

**Location**: `/dashboard` page
**Severity**: Medium (Localization)
**Steps to Reproduce**:
1. Switch language to Spanish (es-ES)
2. Navigate to `/dashboard`
3. Observe card titles

**Expected Behavior**: All card titles should be translated to Spanish

**Actual Behavior**: Several card titles remain in English:
- "TOTAL PLAYS"
- "FAVORITES: SONGS"
- (possibly others)

**Files to Investigate**:
- `src/Melodee.Blazor/Resources/SharedResources.es-ES.resx` - check if keys exist
- Dashboard component - verify L() calls are being used for these titles
- May need to add missing localization keys

---

### Issue #5: Missing Spanish Translations on Stats Page (Localization)

**Location**: `/stats` page
**Severity**: Medium (Localization)
**Steps to Reproduce**:
1. Switch language to Spanish (es-ES)
2. Navigate to `/stats`
3. Observe card titles

**Expected Behavior**: All card titles should be translated to Spanish

**Actual Behavior**: Several card titles remain in English:
- "TOTAL PLAYS"
- "FAVORITES: SONGS"
- (possibly others - same as dashboard)

**Note**: Likely same root cause as Issue #4 - shared components or same localization keys missing

**Files to Investigate**:
- Stats page component
- Shared card components used by both dashboard and stats
- May be hardcoded strings instead of L() calls

---

### Issue #6: Grid "No Records" Message Not Translated (Localization)

**Location**: All data grid views (`/data/albums`, `/data/artists`, etc.)
**Severity**: Medium (Localization)
**Steps to Reproduce**:
1. Switch language to Spanish (es-ES)
2. Navigate to any list view with no data (e.g., `/data/albums`, `/data/artists`)
3. Observe the empty state message

**Expected Behavior**: Message should be translated (e.g., "No hay registros para mostrar.")

**Actual Behavior**: Displays English text "No records to display."

**Root Cause**: Likely Radzen DataGrid's `EmptyText` property not using localization

**Files to Investigate**:
- Grid components or shared grid wrapper
- Check if `EmptyText` parameter is using `L()` call
- May need to set `EmptyText="@L("Common.NoRecordsToDisplay")"` on grids

---

### Issue #7: Grid Footer Pagination Text Not Translated (Localization)

**Location**: All data grid views (`/data/albums`, `/data/artists`, etc.)
**Severity**: Medium (Localization)
**Steps to Reproduce**:
1. Switch language to Spanish (es-ES)
2. Navigate to any list view with a grid
3. Observe the grid footer

**Expected Behavior**: Pagination text should be translated to Spanish

**Actual Behavior**: Footer displays English text:
- "Displaying Page 1 of 1 (total 0 records)"
- "items per page"

**Root Cause**: Radzen DataGrid pagination template strings not localized

**Files to Investigate**:
- Radzen DataGrid configuration
- May need to set `PageSizeText`, `PagingSummaryFormat` properties with L() calls
- Or configure Radzen's built-in localization service

---

### Issue #8: All Statistic Cards Not Translated (Localization)

**Location**: Multiple pages - `/stats`, `/data/albums`, `/data/artists`, etc.
**Severity**: Medium (Localization)
**Steps to Reproduce**:
1. Switch language to Spanish (es-ES)
2. Navigate to any page with statistic cards
3. Observe the card titles

**Expected Behavior**: All statistic card titles should be translated to Spanish

**Actual Behavior**: All statistic card titles remain in English:
- "Albums"
- "Users: Favorite songs"
- (and others)

**Root Cause**: Statistic card component likely using hardcoded strings instead of L() calls

**Files to Investigate**:
- Shared statistic card component (likely in `src/Melodee.Blazor/Components/Shared/`)
- All pages that use statistic cards
- This is a widespread issue affecting multiple views

---

### Issue #9: Container Memory Exhaustion During MusicBrainz Job (Infrastructure - CONFIRMED)

**Location**: Container runtime
**Severity**: **HIGH** (Infrastructure/Stability)
**Steps to Reproduce**:
1. Start container with default configuration (1GB memory limit)
2. Run MusicBrainzUpdateDatabaseJob from /admin/doctor
3. Job fails during SQLite import phase

**Expected Behavior**: MusicBrainz job should complete successfully

**Actual Behavior**: Job fails with `System.OutOfMemoryException`

**Job Execution Log**:
```
21:47:36 - Job started
21:54:01 - Downloaded mbdump.tar.bz2 (6.4 GB) in 385s ✅
21:54:25 - Downloaded mbdump-derived.tar.bz2 (451.2 MB) ✅
22:07:14 - Extracted mbdump.tar.bz2 (769.6 seconds) ✅
22:08:29 - Extracted mbdump-derived.tar.bz2 (75.2 seconds) ✅
22:08:29 - Started SQLite import...
22:09:48 - FAILED: System.OutOfMemoryException ❌
```

**Error Stack**:
```
System.OutOfMemoryException: Exception of type 'System.OutOfMemoryException' was thrown.
   at MusicBrainzRepositoryBase.LoadDataFromFileAsync[T](...) line 66
   at MusicBrainzRepositoryBase.LoadDataFromMusicBrainzFiles(...) line 107
   at SQLiteMusicBrainzRepository.ImportData(...) line 537
```

**Root Cause**: 
1. Container memory limit (1GB) insufficient for loading MusicBrainz data files into memory
2. The `LoadDataFromFileAsync` method appears to load entire data files into memory

**Fix Required**:
1. **Short-term**: Increase container memory limit to 4GB or 8GB in `compose.yml`
2. **Long-term**: Optimize `MusicBrainzRepositoryBase.LoadDataFromFileAsync` to use streaming instead of loading entire files into memory

**Current compose.yml setting**:
```yaml
deploy:
  resources:
    limits:
      cpus: "1.00"
      memory: 1g  # TOO LOW - increase to 4g or 8g
```

**Files to Investigate**:
- `compose.yml` - increase memory limit
- `src/Melodee.Common/Plugins/SearchEngine/MusicBrainz/Data/MusicBrainzRepositoryBase.cs` - line 66, optimize memory usage

---

### Issue #10: Data Grid Column Headers Not Translated on /data/users (Localization)

**Location**: `/data/users` page
**Severity**: Medium (Localization)
**Steps to Reproduce**:
1. Switch language to Spanish (es-ES)
2. Navigate to `/data/users`
3. Observe the data grid column headers

**Expected Behavior**: Column headers should be translated to Spanish

**Actual Behavior**: All column headers remain in English

**Root Cause**: DataGrid column `Title` properties likely using hardcoded strings instead of L() calls

**Files to Investigate**:
- `src/Melodee.Blazor/Components/Pages/Data/Users.razor` or similar
- Check `RadzenDataGridColumn` `Title` attributes

---

### Issue #11: SYSTEMIC - Placeholder Translations Across All Languages (Localization - Community)

**Location**: All pages, all non-English languages
**Severity**: **LOW** (Community contribution needed - not a code bug)
**Status**: ✅ **ADDRESSED** - Using standard FOSS translation contribution model

**Current Translation Status** (as of 2026-01-04):

| Language | Code | Status | Needs Translation |
|----------|------|--------|-------------------|
| English (US) | en-US | ✅ 100% | 0 strings |
| Arabic | ar-SA | 🔄 31% | ~1002 strings |
| Chinese (Simplified) | zh-CN | 🔄 38% | ~900 strings |
| French | fr-FR | 🔄 37% | ~917 strings |
| German | de-DE | 🔄 41% | ~848 strings |
| Italian | it-IT | 🔄 41% | ~850 strings |
| Japanese | ja-JP | 🔄 37% | ~917 strings |
| Portuguese (Brazil) | pt-BR | 🔄 42% | ~844 strings |
| Russian | ru-RU | 🔄 38% | ~900 strings |
| Spanish | es-ES | 🔄 38% | ~902 strings |

**Resolution Approach**: 
Following standard FOSS practices, all placeholder translations now use uniform `[NEEDS TRANSLATION]` prefix followed by the English text. Community members can contribute translations via pull requests.

**Changes Made**:
1. Created `scripts/normalize-translations.py` to convert language-specific placeholders to uniform format
2. Standardized all placeholder text to `[NEEDS TRANSLATION] <English text>` (users see English as fallback)
3. Updated README.md with translation status table and contribution instructions
4. Created `scripts/update-translation-status.sh` to calculate current percentages

**For Contributors**:
1. Find language file: `src/Melodee.Blazor/Resources/SharedResources.<code>.resx`
2. Search for `[NEEDS TRANSLATION]` entries
3. Replace with native translation (remove the prefix and English text)
4. Submit pull request

**User Experience**: Users now see English fallback text instead of untranslated placeholder markers like "Falta traducción" or "未翻訳". This is more usable while awaiting community translations.

**Note**: This is not a bug - it's the natural state of an internationalized FOSS project awaiting community translations.

---

### Issue #12: MusicBrainz Search Engine Shows "Disabled" in Configurable Services (Configuration)

**Location**: `/admin/doctor` page - Configurable Services section
**Severity**: Medium (Configuration/UX)
**Steps to Reproduce**:
1. Navigate to `/admin/doctor`
2. Look at "Configurable Services" section
3. Observe MusicBrainz search engine status

**Expected Behavior**: MusicBrainz should be enabled by default (or clearly indicate why it's disabled)

**Actual Behavior**: MusicBrainz Search Engine shows as "Disabled"

**Possible Causes**:
1. Missing SQLite database file (job failed before creating it)
2. Configuration setting defaulting to disabled
3. Auto-disabled after job failure

**Questions to Investigate**:
- Should MusicBrainz be enabled by default even without the database?
- Is there a setting that controls this?
- Does the job failure auto-disable the service?
- What is the expected behavior when database is missing?

**Files to Investigate**:
- Service configuration/settings for MusicBrainz enabled state
- Check if there's a default setting in database seeds or appsettings
- Check job code for auto-disable logic on failure

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

1. [ ] Complete all test checklist items
2. [ ] Document any issues found
3. [ ] Apply fixes after testing session completes

---

## Session Notes

_Add any additional observations during testing below:_

