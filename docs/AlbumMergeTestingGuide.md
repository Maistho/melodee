# Album Merge Testing Guide

## Test Coverage Required

This document outlines the comprehensive test coverage needed for the Album Merge feature.

### 1. DetectAlbumMergeConflictsAsync Tests

#### Happy Path - No Conflicts
```csharp
[Fact]
public async Task DetectAlbumMergeConflictsAsync_AlbumsWithNoConflicts_ReturnsNoConflicts()
{
    // Create target album and source albums with compatible data
    // Verify HasConflicts is false
}
```

#### Album Field Conflicts
```csharp
[Fact]
public async Task DetectAlbumMergeConflictsAsync_AlbumsWithDifferentYears_DetectsYearConflict()
{
    // Create albums with different years
    // Verify conflict type is AlbumFieldConflict
    // Verify field name is "ReleaseYear"
}

[Fact]
public async Task DetectAlbumMergeConflictsAsync_AlbumsWithDifferentTitles_DetectsTitleConflict()
{
    // Create albums with different titles
    // Verify conflict type is AlbumFieldConflict
    // Verify field name is "Title"
}
```

#### Track Number Collision
```csharp
[Fact]
public async Task DetectAlbumMergeConflictsAsync_SameTrackNumberDifferentSongs_DetectsCollision()
{
    // Create target album with track 1 "Song A"
    // Create source album with track 1 "Song B"
    // Verify conflict type is TrackNumberCollision
}
```

#### Duplicate Title Different Number
```csharp
[Fact]
public async Task DetectAlbumMergeConflictsAsync_SameSongTitleDifferentNumbers_DetectsConflict()
{
    // Create target with "My Song" at track 1
    // Create source with "My Song" at track 5
    // Verify conflict type is DuplicateTitleDifferentNumber
}
```

#### Metadata Conflicts
```csharp
[Fact]
public async Task DetectAlbumMergeConflictsAsync_DifferentGenres_DetectsMetadataConflict()
{
    // Create albums with different genres
    // Verify conflict type is MetadataCollision
    // Verify field name is "Genres"
}

[Fact]
public async Task DetectAlbumMergeConflictsAsync_DifferentMoods_DetectsMetadataConflict()
{
    // Create albums with different moods
    // Verify conflict type is MetadataCollision
    // Verify field name is "Moods"
}
```

### 2. MergeAlbumsAsync Tests

#### Happy Path - No Conflicts
```csharp
[Fact]
public async Task MergeAlbumsAsync_TwoAlbumsNoConflicts_MergesSuccessfully()
{
    // Create target and source albums with compatible data
    // Create merge request with no resolutions needed
    // Verify source albums are deleted
    // Verify songs are moved to target
    // Verify report counts are correct
}
```

#### With Field Resolutions
```csharp
[Fact]
public async Task MergeAlbumsAsync_WithYearConflictResolution_AppliesResolution()
{
    // Create albums with different years
    // Provide resolution to use source year
    // Verify target album has source year after merge
}

[Fact]
public async Task MergeAlbumsAsync_WithTitleConflictResolution_AppliesResolution()
{
    // Create albums with different titles
    // Provide resolution to use source title
    // Verify target album has source title after merge
}
```

#### With Track Resolutions
```csharp
[Fact]
public async Task MergeAlbumsAsync_TrackCollisionKeepTarget_KeepsTargetTrack()
{
    // Create collision at track 1
    // Provide resolution to keep target
    // Verify target track remains, source skipped
}

[Fact]
public async Task MergeAlbumsAsync_TrackCollisionReplaceWithSource_ReplacesTrack()
{
    // Create collision at track 1
    // Provide resolution to replace with source
    // Verify target track is replaced with source track
}

[Fact]
public async Task MergeAlbumsAsync_DuplicateTitleSkipSource_SkipsSourceTrack()
{
    // Create duplicate title at different numbers
    // Provide resolution to skip source
    // Verify source track is not added
}
```

#### Deduplication
```csharp
[Fact]
public async Task MergeAlbumsAsync_IdenticalSongs_DeduplicatesAutomatically()
{
    // Create target and source with same song (same title, duration, hash)
    // Merge without resolution
    // Verify only one copy exists in target
    // Verify report shows songs skipped count
}

[Fact]
public async Task MergeAlbumsAsync_DuplicateImages_DeduplicatesImages()
{
    // Create albums with same image files
    // Merge albums
    // Verify only one copy of image exists
    // Verify report shows images skipped count
}
```

#### Metadata Merge
```csharp
[Fact]
public async Task MergeAlbumsAsync_DifferentGenres_UnionsGenres()
{
    // Create target with ["Rock"] and source with ["Pop"]
    // Merge with KeepBoth resolution
    // Verify target has ["Rock", "Pop"]
}

[Fact]
public async Task MergeAlbumsAsync_DifferentMoods_UnionsMoods()
{
    // Similar to genres test
}
```

#### Atomic Rollback
```csharp
[Fact]
public async Task MergeAlbumsAsync_ErrorDuringMerge_RollsBackChanges()
{
    // Create scenario that will fail mid-merge
    // Verify transaction is rolled back
    // Verify no partial changes occurred
    // Verify source albums still exist
}
```

#### Missing Resolutions
```csharp
[Fact]
public async Task MergeAlbumsAsync_MissingRequiredResolutions_ReturnsError()
{
    // Create albums with conflicts
    // Submit merge request without resolutions
    // Verify error message about missing resolutions
}
```

#### Cache Clearing
```csharp
[Fact]
public async Task MergeAlbumsAsync_AfterSuccessfulMerge_ClearsCaches()
{
    // Merge albums
    // Verify artist cache is cleared
    // Verify album caches are cleared
}
```

### 3. Helper Method Tests

```csharp
[Fact]
public void AreSongsEqual_IdenticalSongs_ReturnsTrue()
{
    // Create two songs with same normalized title and duration
    // Verify they are equal
}

[Fact]
public void AreSongsEqual_DifferentTitles_ReturnsFalse()
{
    // Create two songs with different titles
    // Verify they are not equal
}

[Fact]
public void AreSongsEqual_DifferentDuration_ReturnsFalse()
{
    // Create two songs with same title but different duration (>1s)
    // Verify they are not equal
}
```

### 4. Validation Tests

```csharp
[Fact]
public async Task DetectAlbumMergeConflictsAsync_InvalidArtistId_ThrowsArgumentException()

[Fact]
public async Task DetectAlbumMergeConflictsAsync_InvalidTargetAlbumId_ThrowsArgumentException()

[Fact]
public async Task DetectAlbumMergeConflictsAsync_EmptySourceAlbumIds_ThrowsArgumentException()

[Fact]
public async Task MergeAlbumsAsync_NullRequest_ThrowsArgumentNullException()
```

### 5. Edge Cases

```csharp
[Fact]
public async Task MergeAlbumsAsync_ThreeAlbums_MergesAllSuccessfully()

[Fact]
public async Task MergeAlbumsAsync_SourceAlbumWithNoSongs_MergesSuccessfully()

[Fact]
public async Task MergeAlbumsAsync_AlbumsFromDifferentArtists_ReturnsError()
```

## Test Data Helpers

The following helper methods should be created in the test class:

```csharp
private async Task<Album> CreateTestAlbum(
    Artist artist, 
    string name, 
    int year, 
    Song[]? songs = null)

private Song CreateTestSong(
    Album album, 
    int songNumber, 
    string title, 
    double duration = 180000)

private AlbumMergeRequest CreateMergeRequest(
    int artistId,
    int targetAlbumId,
    int[] sourceAlbumIds,
    AlbumMergeResolution[]? resolutions = null)
```

## Running Tests

To run these tests:
```bash
dotnet test --filter "FullyQualifiedName~AlbumMerge"
```

## Coverage Goals

- Minimum 85% code coverage for merge methods
- All conflict types must have test coverage
- All resolution types must have test coverage
- Atomic rollback must be verified
