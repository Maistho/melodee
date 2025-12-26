# Album Merge Feature Implementation Summary

## Overview
This document summarizes the implementation of the album merge feature for the Melodee music management system.

## What Was Implemented

### 1. Domain Models and DTOs

All models are located in `/src/Melodee.Common/Models/AlbumMerge/` and `/src/Melodee.Common/Enums/`:

#### AlbumMergeConflictType.cs (Enum)
Defines the types of conflicts that can occur during merging:
- `AlbumFieldConflict` - Differences in album-level fields (title, year)
- `TrackNumberCollision` - Same track number with different content
- `DuplicateTitleDifferentNumber` - Same song at different track numbers
- `MetadataCollision` - Conflicts in genres, moods, or other metadata
- `ImageCollision` - Image conflicts

#### AlbumMergeConflict.cs
Represents a detected conflict with:
- Unique conflict ID
- Conflict type and description
- Field name and values from target/sources
- Track-specific information (number, IDs)
- Required vs optional flag

#### AlbumMergeResolution.cs
Represents user's resolution decision with:
- Conflict ID reference
- Resolution action (KeepTarget, ReplaceWithSource, SkipSource, KeepBoth, Renumber)
- Selected values and album IDs
- Track information for track-specific resolutions

#### AlbumMergeRequest.cs
Input model for merge operation containing:
- Artist ID
- Target album ID
- Source album IDs
- Resolution array

#### AlbumMergeReport.cs
Output report summarizing the merge with:
- Target and source album information
- Counts of items moved, skipped
- Applied resolutions
- Detailed action log

#### AlbumMergeConflictDetectionResult.cs
Result of conflict detection with:
- Conflict array
- Summary information
- HasConflicts flag

### 2. Service Layer Implementation

Located in `/src/Melodee.Common/Services/ArtistService.cs`:

#### DetectAlbumMergeConflictsAsync
Analyzes albums before merging to identify conflicts:

**Features:**
- Validates artist and album ownership
- Detects album field conflicts (year, title)
- Detects track conflicts (number collisions, duplicate titles)
- Detects metadata conflicts (genres, moods)
- Returns comprehensive conflict list with context

**Conflict Detection Logic:**
1. **Album Fields**: Compares release years and titles across all albums
2. **Tracks**: 
   - Checks for same track number with different content
   - Checks for same normalized title at different track numbers
   - Uses song equality comparison (title, duration, hash)
3. **Metadata**: Identifies new genres/moods not in target

#### MergeAlbumsAsync
Executes the album merge with conflict resolution:

**Features:**
- Validates all required conflicts have resolutions
- Executes merge in atomic transaction
- Applies field resolutions (year, title)
- Merges songs with deduplication
- Moves physical files (songs, images)
- Merges UserAlbum relationships
- Unions metadata (genres, moods)
- Deletes source albums after successful merge
- Clears all relevant caches
- Generates detailed merge report

**Transaction Safety:**
- Uses database transaction for atomicity
- Rolls back on any error
- No partial merges possible

**File Operations:**
- Moves song files from source to target directories
- Moves image files with deduplication
- Deletes source album directories after merge

#### Helper Methods

**DetectAlbumFieldConflicts**
- Compares years and titles
- Creates conflict objects with source values

**DetectTrackConflicts**
- Identifies track number collisions
- Identifies duplicate titles at different numbers
- Uses AreSongsEqual for comparison

**DetectMetadataConflicts**
- Compares genres and moods
- Marks as non-required (can default to union)

**AreSongsEqual**
- Compares normalized titles
- Compares duration (1 second tolerance)
- Uses file hash if available

**ApplyFieldResolutions**
- Updates target album with selected values
- Logs all changes

**GetTrackResolution**
- Finds resolution for specific track

**MergeMetadata**
- Unions genres and moods (default behavior)
- Respects user resolutions if provided

### 3. Key Features

#### Atomic Operations
- All changes wrapped in database transaction
- Rollback on any error ensures data consistency
- No partial merges possible

#### Deduplication
- Songs: Skips identical songs automatically
- Images: Skips duplicate image files
- Metadata: Unions genres/moods by default

#### Conflict Resolution
- Required conflicts must be resolved before merge
- Optional conflicts have sensible defaults
- All resolutions tracked in merge report

#### Reporting
- Detailed action log of all operations
- Counts of moved vs skipped items
- Complete audit trail

## What Still Needs to Be Done

### 1. Comprehensive Testing
See `/docs/AlbumMergeTestingGuide.md` for complete test specifications.

**Priority Tests:**
- Happy path with no conflicts
- All conflict types
- All resolution types
- Atomic rollback
- Deduplication rules
- Cache clearing

### 2. UI Implementation (Blazor)

**Required Components:**
1. **MergeAlbumsDialog** - Initial selection and target choice
2. **ConflictResolutionDialog** - Interactive conflict resolution
3. **MergeReportDialog** - Display merge results

**Integration Points:**
- Add multi-select to album list in ArtistDetail
- Add "Merge Albums" button (disabled when < 2 selected)
- Wire up service calls
- Handle error states
- Refresh artist detail after merge

### 3. API Endpoints (if needed)

**Recommended endpoints:**
- `POST /api/artists/{id}/merge-albums/detect` - Detect conflicts
- `POST /api/artists/{id}/merge-albums` - Execute merge

### 4. Documentation
- User guide for merge workflow
- API documentation
- Deployment notes

## Design Decisions

### Why ArtistService?
All artist data operations are centralized in ArtistService per the existing pattern. Albums belong to artists, so the merge is an artist operation.

### Why Atomic Transactions?
Partial merges would leave the database in an inconsistent state. Transactions ensure all-or-nothing semantics.

### Why Explicit Resolutions?
Requiring user decisions for conflicts prevents data loss and ensures user intent is captured. Silent conflict resolution could merge the wrong data.

### Why Deduplication?
Prevents duplicate songs/images when merging near-duplicate albums (e.g., different versions of same album).

## Usage Example

```csharp
// 1. Detect conflicts
var detectResult = await artistService.DetectAlbumMergeConflictsAsync(
    artistId: 1,
    targetAlbumId: 10,
    sourceAlbumIds: new[] { 11, 12 });

if (!detectResult.IsSuccess)
{
    // Handle error
    return;
}

// 2. If conflicts exist, user must resolve them
if (detectResult.Data.HasConflicts)
{
    // Show conflict resolution UI
    // User selects resolutions
    var resolutions = GetUserResolutions(detectResult.Data.Conflicts);
    
    // 3. Execute merge with resolutions
    var mergeRequest = new AlbumMergeRequest
    {
        ArtistId = 1,
        TargetAlbumId = 10,
        SourceAlbumIds = new[] { 11, 12 },
        Resolutions = resolutions
    };
    
    var mergeResult = await artistService.MergeAlbumsAsync(mergeRequest);
    
    if (mergeResult.IsSuccess)
    {
        // Show merge report
        ShowMergeReport(mergeResult.Data);
    }
}
else
{
    // No conflicts, merge directly
    var mergeRequest = new AlbumMergeRequest
    {
        ArtistId = 1,
        TargetAlbumId = 10,
        SourceAlbumIds = new[] { 11, 12 }
    };
    
    var mergeResult = await artistService.MergeAlbumsAsync(mergeRequest);
}
```

## Build Status

✅ Code compiles successfully
✅ No build errors
⚠️ Tests not yet implemented
⚠️ UI not yet implemented

## File Inventory

### Created Files
- `src/Melodee.Common/Enums/AlbumMergeConflictType.cs`
- `src/Melodee.Common/Models/AlbumMerge/AlbumMergeConflict.cs`
- `src/Melodee.Common/Models/AlbumMerge/AlbumMergeConflictDetectionResult.cs`
- `src/Melodee.Common/Models/AlbumMerge/AlbumMergeReport.cs`
- `src/Melodee.Common/Models/AlbumMerge/AlbumMergeRequest.cs`
- `src/Melodee.Common/Models/AlbumMerge/AlbumMergeResolution.cs`
- `docs/AlbumMergeTestingGuide.md`

### Modified Files
- `src/Melodee.Common/Services/ArtistService.cs` - Added merge methods

## Next Steps

1. **Implement Priority Tests** - Start with happy path and basic conflict tests
2. **Create UI Components** - Build Blazor dialogs for merge workflow
3. **Integration Testing** - Test end-to-end workflow
4. **Performance Testing** - Test with large albums
5. **User Documentation** - Create user-facing documentation
