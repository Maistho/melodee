# Phase 1 Test Coverage Analysis - Targeted Approach

## Summary

After attempting to add tests, we discovered that **LibraryService** and **UserService** already have comprehensive test suites (34 and 36 tests respectively), yet coverage remains low (17.1% and 21.2%). 

This indicates the issue is NOT a lack of tests, but rather **untested branches within complex methods**.

## Current State

| Service | Tests | Coverage | Lines | Status |
|---------|-------|----------|-------|--------|
| LibraryService | 34 | 17.1% | 374 coverable | Many untested branches |
| UserService | 36 | 21.2% | 320 coverable | Many untested branches |

## Root Cause

Both services have large, complex methods with many conditional branches:
- Error handling paths not tested
- Edge cases not covered
- Validation failures not tested
- Cache hit/miss scenarios not tested
- Different library type combinations not tested

## Recommended Approach

### Step 1: Analyze HTML Coverage Report

Open `/home/steven/source/melodee/coverage/report/index.html` in browser and:

1. Navigate to `Melodee.Common.Services.LibraryService`
2. Review line-by-line highlighting (green = covered, red = uncovered)
3. Identify the largest **red blocks** (uncovered sections)
4. Document uncovered methods and their line ranges

### Step 2: Categorize Uncovered Code

Group uncovered lines into:
- **Critical paths** (main business logic)
- **Error handling** (exception paths, validations)
- **Edge cases** (null checks, empty collections)
- **Performance paths** (cache hits, large datasets)

### Step 3: Create Targeted Tests

For each uncovered section:
1. Understand the conditional logic
2. Write test to exercise that specific branch
3. Verify coverage improves for those lines

## Example: LibraryService.DeleteAsync

**Current test:**
```csharp
[Fact]
public async Task DeleteAsync_WithValidUserIds_ReturnsSuccess()
{
    // Only tests happy path
}
```

**Missing tests based on DeleteAsync code:**
- `DeleteAsync_WithNonExistentLibrary_ReturnsError` 
- `DeleteAsync_WithLibraryContainingArtists_ReturnsValidationFailure`
- `DeleteAsync_WithMultipleIds_OneInvalid_ReturnsError`

## Tools

**View coverage HTML:**
```bash
open coverage/report/index.html  # macOS
xdg-open coverage/report/index.html  # Linux
```

**Navigate to specific class:**
- Click "Melodee.Common" assembly
- Click "Services" namespace
- Click "LibraryService" class
- See line-by-line coverage with highlighting

## Next Steps for Agent

1. Open the HTML coverage report for LibraryService
2. Document uncovered line ranges (e.g., lines 250-280, 310-340)
3. Read those code sections and understand the logic
4. Write tests specifically targeting those branches
5. Re-run coverage to verify improvement
6. Repeat until 70% coverage achieved

## Success Criteria

- LibraryService: 17.1% → 70% (need +52.9%)
- UserService: 21.2% → 65% (need +43.8%)
- All new tests must pass
- Tests must target specific uncovered branches, not duplicate existing tests

## Files

- Coverage report: `coverage/report/index.html`
- Tests: `tests/Melodee.Tests.Common/Common/Services/LibraryServiceTests.cs`
- Tests: `tests/Melodee.Tests.Common/Common/Services/UserServiceTests.cs`
- Source: `src/Melodee.Common/Services/LibraryService.cs`
- Source: `src/Melodee.Common/Services/UserService.cs`
