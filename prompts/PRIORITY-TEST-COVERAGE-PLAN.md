# Priority Test Coverage Improvement Plan

## Overview

This document outlines a phased approach to improving test coverage for the Melodee music streaming platform, focusing on critical, performance-sensitive code in `Melodee.Common`. The goal is to increase overall coverage from **52.1%** to **75%+** while prioritizing code with high complexity and risk.

**Current State (as of 2024-12-20):**
- Melodee.Common: 52.1% line coverage
- Total Tests: 2,742 passing, 0 skipped
- Target: 75%+ line coverage for Melodee.Common

---

## Phase Map

- [x] **Phase 1:** Core Services Foundation (LibraryService, UserService) - **BLOCKED** ❌ - PostgreSQL-specific code
- [x] **Phase 2:** Caching & Performance (MemoryCacheManager) - **COMPLETE** ✅ (37.6% → 73.9%)
- [~] **Phase 3:** Data Extensions (FileSystemDirectoryInfoExtensions, AlbumExtensions, SongExtensions) - **DEFERRED** ⏸️ - Complex models
- [x] **Phase 4:** Media Processing (AlbumDiscoveryService, ImageConvertor) - **BLOCKED** ❌ - Test infrastructure needs
- [x] **Phase 5:** Utility & Parsing (SafeParser) - **COMPLETE** ✅ (2.5% → 77.5%)
- [ ] **Phase 6:** Risk Hotspots & CRAP Score Reduction
- [ ] **Future:** Revisit blocked phases with proper infrastructure

## Overall Progress

- **Starting Coverage:** 43.6%
- **Current Coverage:** 44.7% (+1.1%)
- **Successful Phases:** 2 of 5 (Phases 2 & 5)
- **Tests Added:** 6 (Phase 2) + 68 (Phase 5) = 74 new tests

---

## Phase 1: Core Services Foundation - STATUS: PARTIALLY COMPLETE (Blocked)

**Goal:** Establish solid test coverage for the foundational services that all other components depend on.

**Timeline:** 2-3 days

**Current Status:** LibraryService and UserService already have extensive tests (34 and 36 tests respectively), but coverage remains low (17.1% and 21.2%). Investigation revealed PostgreSQL-specific query code (`EF.Functions.ILike`) that cannot be tested with SQLite test database.

**Blocking Issues:**
1. **Database Incompatibility** - ApplyFilters/ApplyOrdering methods use PostgreSQL-specific EF.Functions.ILike which doesn't work in SQLite test database
2. These methods represent 66-91 uncovered lines each (major coverage gaps)
3. Tests for these methods fail with empty results due to database function incompatibility

**Options to Unblock:**
1. Switch test database to PostgreSQL (requires testcontainers or local PostgreSQL)
2. Mock the filtering/ordering logic for tests
3. Refactor LibraryService/UserService to use database-agnostic LINQ
4. Skip filtering/ordering coverage and focus on other methods

**Recommended:** Skip to Phase 2-5 which have simpler targets without database dependencies, then revisit Phase 1 with PostgreSQL testcontainers.

**Next Steps:** Move to Phase 2 (Caching & Performance) which has no database dependencies.

### 1.1 LibraryService (17.1% → 70%) - BLOCKED

**File:** `src/Melodee.Common/Services/LibraryService.cs`
**Coverable Lines:** 374
**Current Coverage:** 17.1%

**Why Critical:**
- All media operations depend on library resolution
- Storage path management affects entire system
- Scan operations are performance-critical

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Common/Services/LibraryServiceTests.cs
```

| Test Method | Description |
|-------------|-------------|
| `GetStorageLibrariesAsync_ReturnsAllStorageLibraries` | Verify storage library retrieval |
| `GetStorageLibrariesAsync_ThrowsWhenNoStorageLibrary` | Error handling when no storage configured |
| `GetStagingLibraryAsync_ReturnsStagingLibrary` | Staging library resolution |
| `GetInboundLibraryAsync_ReturnsInboundLibrary` | Inbound library resolution |
| `GetUserImagesLibraryAsync_ReturnsUserImagesLibrary` | User images path resolution |
| `GetPlaylistLibraryAsync_ReturnsPlaylistLibrary` | Playlist storage resolution |
| `GetByApiKeyAsync_WithValidKey_ReturnsLibrary` | API key lookup |
| `GetByApiKeyAsync_WithInvalidKey_ReturnsNull` | Invalid key handling |
| `GetAsync_WithValidId_ReturnsLibrary` | ID-based lookup |
| `ListAsync_WithPagination_ReturnsCorrectPage` | Pagination support |
| `UpdateAsync_UpdatesLibraryProperties` | Library modification |
| `DeleteAsync_RemovesLibrary` | Library deletion |
| `NeedsScanning_WhenLastScanOld_ReturnsTrue` | Scan detection logic |

**Implementation Notes:**
- Use existing `ServiceTestBase` for mock setup
- Seed test database with multiple library types
- Test caching behavior (cache hits/misses)

### 1.2 UserService (21.2% → 65%)

**File:** `src/Melodee.Common/Services/UserService.cs`
**Coverable Lines:** 320
**Current Coverage:** 21.2%

**Why Critical:**
- Authentication and authorization
- User preferences affect all UI
- Security-critical code path

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Common/Services/UserServiceTests.cs (extend existing)
```

| Test Method | Description |
|-------------|-------------|
| `AuthenticateAsync_WithValidCredentials_ReturnsUser` | Successful auth |
| `AuthenticateAsync_WithInvalidPassword_ReturnsNull` | Failed auth |
| `AuthenticateAsync_WithLockedAccount_ReturnsNull` | Locked account handling |
| `CreateAsync_WithValidData_CreatesUser` | User creation |
| `CreateAsync_WithDuplicateEmail_ThrowsException` | Duplicate detection |
| `UpdateAsync_UpdatesUserProperties` | User modification |
| `UpdateLastLoginAsync_UpdatesTimestamp` | Login tracking |
| `GetByApiKeyAsync_WithValidKey_ReturnsUser` | API key auth |
| `GetByEmailAsync_WithValidEmail_ReturnsUser` | Email lookup |
| `ListAsync_WithFilters_ReturnsFilteredUsers` | User listing |
| `DeleteAsync_RemovesUserAndRelatedData` | Cascade delete |
| `ChangePasswordAsync_UpdatesPassword` | Password change |
| `ImportFromCsvAsync_ImportsUsers` | Bulk import |

**Implementation Notes:**
- Test password hashing/verification
- Verify role assignments
- Test social login integration points

---

## Phase 2: Caching & Performance

**Goal:** Ensure caching layer is robust and performs correctly under various conditions.

**Timeline:** 1-2 days

### 2.1 MemoryCacheManager (37.6% → 80%)

**File:** `src/Melodee.Common/Services/Caching/MemoryCacheManager.cs`
**Coverable Lines:** 446
**Current Coverage:** 37.6%

**Why Critical:**
- All services use caching
- Performance bottleneck if misconfigured
- Memory management critical for long-running server

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Common/Services/Caching/MemoryCacheManagerTests.cs (extend existing)
```

| Test Method | Description |
|-------------|-------------|
| `GetAsync_WhenCached_ReturnsCachedValue` | Cache hit |
| `GetAsync_WhenNotCached_CallsFactory` | Cache miss |
| `GetAsync_WithExpiration_ExpiresCorrectly` | TTL handling |
| `GetAsync_WithRegion_IsolatesData` | Region isolation |
| `Remove_RemovesItem` | Single item removal |
| `ClearRegion_ClearsOnlyRegion` | Region clear |
| `Clear_ClearsAllItems` | Full cache clear |
| `CacheStatistics_ReturnsAccurateStats` | Stats reporting |
| `GetAsync_ConcurrentAccess_IsThreadSafe` | Thread safety |
| `GetAsync_WithNullValue_CachesNull` | Null handling |

**Implementation Notes:**
- Test with realistic cache sizes
- Verify memory is released on clear
- Test concurrent access patterns

### 2.2 StreamingLimiter (33.3% → 75%)

**File:** `src/Melodee.Common/Services/StreamingLimiter.cs`
**Coverable Lines:** ~60
**Current Coverage:** 33.3%

**Why Critical:**
- Rate limiting prevents server overload
- Affects user experience when throttled
- Security: prevents abuse

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Common/Services/StreamingLimiterTests.cs (extend existing)
```

| Test Method | Description |
|-------------|-------------|
| `TryAcquire_UnderLimit_Succeeds` | Normal operation |
| `TryAcquire_AtLimit_Fails` | Limit enforcement |
| `Release_FreesSlot` | Slot release |
| `TryAcquire_AfterRelease_Succeeds` | Slot reuse |
| `GetCurrentCount_ReturnsAccurateCount` | Counter accuracy |
| `ConcurrentAcquire_RespectsLimit` | Thread safety |

---

## Phase 3: Data Extensions

**Goal:** Improve coverage of extension methods used throughout the codebase.

**Timeline:** 2 days

### 3.1 FileSystemDirectoryInfoExtensions (17.6% → 65%)

**File:** `src/Melodee.Common/Models/Extensions/FileSystemDirectoryInfoExtensions.cs`
**Coverable Lines:** 636
**Current Coverage:** 17.6%

**Why Critical:**
- File operations are core to media processing
- Path manipulation affects security
- Used by scanning, importing, and organizing

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Extensions/FileSystemDirectoryInfoExtensionTests.cs (extend existing)
```

| Test Method | Description |
|-------------|-------------|
| `ToDirectorySystemInfo_CreatesCorrectInfo` | Basic conversion |
| `FullName_ReturnsCorrectPath` | Path resolution |
| `AllFileInfos_ReturnsAllFiles` | File enumeration |
| `AllFileInfos_WithPattern_FiltersCorrectly` | Pattern matching |
| `AllFileInfos_NonExistentDir_ReturnsEmpty` | Missing dir handling |
| `AllDirectoryInfos_ReturnsSubdirectories` | Directory enumeration |
| `DeleteAllEmptyDirectories_RemovesEmpty` | Cleanup operation |
| `FindDuplicatesAsync_FindsDuplicates` | Duplicate detection |
| `IsDirectoryNotStudioAlbums_DetectsCorrectly` | Album type detection |
| `IsDirectoryDiscography_DetectsCorrectly` | Discography detection |
| `Parent_ReturnsParentDirectory` | Parent resolution |

### 3.2 AlbumExtensions (30.4% → 65%)

**File:** `src/Melodee.Common/Models/Extensions/AlbumExtensions.cs`
**Coverable Lines:** 776
**Current Coverage:** 30.4%

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Extensions/AlbumExtensionTests.cs (extend existing)
```

| Test Method | Description |
|-------------|-------------|
| `ToAlbumDataInfo_ConvertsCorrectly` | Data transformation |
| `AlbumDirectoryName_FormatsCorrectly` | Directory naming |
| `IsValid_WithValidAlbum_ReturnsTrue` | Validation |
| `MergeWith_CombinesAlbums` | Album merging |
| `ToFileSystemDirectoryInfo_CreatesCorrectPath` | Path generation |

### 3.3 SongExtensions (37.3% → 65%)

**File:** `src/Melodee.Common/Models/Extensions/SongExtensions.cs`
**Coverable Lines:** 584
**Current Coverage:** 37.3%

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Extensions/SongExtensionTests.cs (extend existing)
```

| Test Method | Description |
|-------------|-------------|
| `ToSongDataInfo_ConvertsCorrectly` | Data transformation |
| `SongFileName_FormatsCorrectly` | File naming |
| `Duration_CalculatesCorrectly` | Duration parsing |
| `IsValid_WithValidSong_ReturnsTrue` | Validation |

---

## Phase 4: Media Processing

**Goal:** Ensure media scanning and processing is reliable and well-tested.

**Timeline:** 2 days

### 4.1 AlbumDiscoveryService (71.3% → 85%)

**File:** `src/Melodee.Common/Services/Scanning/AlbumDiscoveryService.cs`
**Coverable Lines:** 244
**Current Coverage:** 71.3%

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Common/Services/Scanning/AlbumDiscoveryServiceTests.cs (extend existing)
```

| Test Method | Description |
|-------------|-------------|
| `DiscoverAlbumsAsync_WithValidDirectory_ReturnsAlbums` | Basic discovery |
| `DiscoverAlbumsAsync_WithNestedDirectories_FindsAll` | Recursive scanning |
| `DiscoverAlbumsAsync_WithMixedFormats_HandlesAll` | Format support |
| `DiscoverAlbumsAsync_WithInvalidFiles_SkipsGracefully` | Error handling |
| `MergeAlbumData_CombinesMetadata` | Metadata merging |

### 4.2 ImageConvertor (1.8% → 50%)

**File:** `src/Melodee.Common/Plugins/Conversion/Image/ImageConvertor.cs`
**Coverable Lines:** 110
**Current Coverage:** 1.8%

**Why Critical:**
- Album art is user-facing
- Performance: image processing is CPU-intensive
- Memory: large images can cause issues

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Plugins/Conversion/ImageConversionTests.cs (extend existing)
```

| Test Method | Description |
|-------------|-------------|
| `ConvertAsync_WithValidImage_Converts` | Basic conversion |
| `ConvertAsync_WithInvalidImage_ReturnsError` | Error handling |
| `ResizeAsync_MaintainsAspectRatio` | Aspect ratio |
| `ConvertAsync_ToJpeg_ProducesJpeg` | Format conversion |
| `ConvertAsync_WithTransparency_HandlesAlpha` | Alpha channel |

---

## Phase 5: Utility & Parsing

**Goal:** Ensure utility classes handle edge cases correctly.

**Timeline:** 1-2 days

### 5.1 SafeParser (55.4% → 80%)

**File:** `src/Melodee.Common/Utility/SafeParser.cs`
**Coverable Lines:** 570
**Current Coverage:** 55.4%

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Utility/SafeParserTests.cs (extend existing)
```

| Test Method | Description |
|-------------|-------------|
| `ToNumber_WithValidInt_ReturnsInt` | Integer parsing |
| `ToNumber_WithInvalidString_ReturnsDefault` | Invalid input |
| `ToNumber_WithNull_ReturnsDefault` | Null handling |
| `ToBoolean_WithTrueString_ReturnsTrue` | Boolean parsing |
| `ToDateTime_WithValidDate_ReturnsDate` | Date parsing |
| `ToEnum_WithValidValue_ReturnsEnum` | Enum parsing |
| `Hash_WithStrings_ReturnsConsistentHash` | Hash generation |

### 5.2 FileHelper (65.5% → 80%)

**File:** `src/Melodee.Common/Utility/FileHelper.cs`
**Coverable Lines:** 232
**Current Coverage:** 65.5%

**Test Cases to Add:**

```
tests/Melodee.Tests.Common/Utility/FileHelperTests.cs (extend existing)
```

| Test Method | Description |
|-------------|-------------|
| `IsFileMediaType_WithMp3_ReturnsTrue` | Media detection |
| `IsFileMediaType_WithTxt_ReturnsFalse` | Non-media detection |
| `GetFileExtension_ReturnsCorrectExtension` | Extension extraction |
| `IsFileImageType_WithJpg_ReturnsTrue` | Image detection |

---

## Phase 6: Risk Hotspots & CRAP Score Reduction

**Goal:** Address methods with high CRAP (Change Risk Anti-Pattern) scores - methods that are complex and poorly tested.

**Timeline:** 2-3 days

### Risk Hotspot Analysis

The following methods have high complexity with low coverage, making them high-risk for bugs:

| Class | Method | Complexity | Coverage | Risk |
|-------|--------|------------|----------|------|
| `AtlMetaTag` | `ReadAsync` | High | 0.5% | **Critical** |
| `LibraryService` | `GetStorageLibrariesAsync` | Medium | 17.1% | High |
| `UserService` | `AuthenticateAsync` | High | 21.2% | High |
| `FileSystemDirectoryInfoExtensions` | `AllFileInfos` | Medium | 17.6% | High |
| `AlbumExtensions` | `ToAlbumDataInfo` | High | 30.4% | High |
| `MemoryCacheManager` | `GetAsync` | Medium | 37.6% | Medium |
| `SongExtensions` | `ToSongDataInfo` | Medium | 37.3% | Medium |
| `ScrobbleService` | `ScrobbleAsync` | Medium | 27.2% | Medium |
| `Mp4TagReader` | `ReadAsync` | High | 14.5% | High |
| `VorbisTagReader` | `ReadAsync` | High | 29.1% | Medium |

### 6.1 High-Risk Methods to Test

**AtlMetaTag.ReadAsync** (0.5% coverage)
- This is the primary metadata reading path
- Complex parsing logic with many branches
- Add tests for various audio formats and edge cases

**LibraryService Path Resolution Methods**
- Test all library type resolutions
- Test error conditions (missing libraries)
- Test caching behavior

**UserService Authentication**
- Test successful/failed login
- Test locked accounts
- Test password hashing

### 6.2 CRAP Score Reduction Strategy

For each high-CRAP method:

1. **Identify branches** - List all conditional paths
2. **Create test matrix** - One test per branch combination
3. **Add edge case tests** - Null inputs, empty collections, boundary values
4. **Verify error paths** - Ensure exceptions are tested

---

## Testing Guidelines for Agents

### Setup Requirements

All tests should:
1. Inherit from `ServiceTestBase` (for service tests) or `TestsBase` (for utility tests)
2. Use the existing mock infrastructure:
   - `MockFactory()` for `IDbContextFactory<MelodeeDbContext>`
   - `MockConfigurationFactory()` for `IMelodeeConfigurationFactory`
   - `MockHttpClientFactory()` for `IHttpClientFactory`
3. Use FluentAssertions for assertions
4. Follow naming convention: `MethodName_Scenario_ExpectedResult`

### Test File Locations

```
tests/
├── Melodee.Tests.Common/
│   ├── Common/
│   │   └── Services/           # Service tests
│   ├── Extensions/             # Extension method tests
│   ├── Plugins/                # Plugin tests
│   └── Utility/                # Utility tests
```

### Example Test Structure

```csharp
public class LibraryServiceTests : ServiceTestBase
{
    [Fact]
    public async Task GetStorageLibrariesAsync_WithValidLibraries_ReturnsLibraries()
    {
        // Arrange
        var service = GetLibraryService();
        await SeedTestLibraries(); // Helper to add test data
        
        // Act
        var result = await service.GetStorageLibrariesAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        result.Data.Should().AllSatisfy(l => l.TypeValue.Should().Be(LibraryType.Storage));
    }
}
```

### Running Coverage

```bash
# Run all tests with coverage
./scripts/run-coverage.sh

# View report
open coverage/report/index.html
```

---

## Success Metrics

| Metric | Current | Phase 1 | Phase 2 | Phase 3 | Phase 4 | Phase 5 | Phase 6 |
|--------|---------|---------|---------|---------|---------|---------|---------|
| Overall Coverage | 52.1% | 58% | 62% | 67% | 71% | 74% | 77% |
| LibraryService | 17.1% | 70% | 70% | 70% | 70% | 70% | 70% |
| UserService | 21.2% | 65% | 65% | 65% | 65% | 65% | 65% |
| MemoryCacheManager | 37.6% | 37.6% | 80% | 80% | 80% | 80% | 80% |
| Test Count | 2,742 | ~2,800 | ~2,850 | ~2,920 | ~2,970 | ~3,020 | ~3,100 |

---

## Completion Checklist

### Phase 1
- [ ] LibraryService tests added (13+ test cases)
- [ ] UserService tests added (13+ test cases)
- [ ] All tests passing
- [ ] Coverage verified at 58%+

### Phase 2
- [ ] MemoryCacheManager tests added (10+ test cases)
- [ ] StreamingLimiter tests added (6+ test cases)
- [ ] All tests passing
- [ ] Coverage verified at 62%+

### Phase 3
- [ ] FileSystemDirectoryInfoExtensions tests added (11+ test cases)
- [ ] AlbumExtensions tests added (5+ test cases)
- [ ] SongExtensions tests added (4+ test cases)
- [ ] All tests passing
- [ ] Coverage verified at 67%+

### Phase 4
- [ ] AlbumDiscoveryService tests extended (5+ test cases)
- [ ] ImageConvertor tests added (5+ test cases)
- [ ] All tests passing
- [ ] Coverage verified at 71%+

### Phase 5
- [ ] SafeParser tests extended (7+ test cases)
- [ ] FileHelper tests extended (4+ test cases)
- [ ] All tests passing
- [ ] Coverage verified at 74%+

### Phase 6
- [ ] AtlMetaTag risk tests added
- [ ] High-CRAP method tests added
- [ ] All tests passing
- [ ] Coverage verified at 77%+
- [ ] No methods with CRAP score > 30 and coverage < 50%
