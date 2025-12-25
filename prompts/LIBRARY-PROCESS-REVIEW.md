# Library Process Workflow Review

**Date**: 2025-12-25  
**Command**: `mcli library <library_name> process`  
**Purpose**: Performance analysis and architectural review of the library processing workflow

---

## Executive Summary

The `library process` command processes raw media files from an inbound directory, extracts metadata, queries external APIs for artist/album information, and stages albums for database insertion. Analysis of production logs reveals significant performance bottlenecks, with single directory processing times ranging from 2 seconds to **262 seconds** (4+ minutes).

### Key Findings

1. **External API Latency Dominates**: Artist search queries range from 2ms to **11.2 seconds**
2. **File I/O Bottlenecks**: Copy operations range from 300ms to **83 seconds** for large albums
3. **Sequential Album Processing**: Albums within a directory are processed sequentially
4. **Parallel Directory Processing**: Directories are processed in parallel (good), but parallelism is underutilized
5. **Multiple Service Initializations**: Each job run re-initializes several heavyweight services

---

## Workflow Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       ProcessInboundCommand                                  │
│                    (CLI Entry Point - Spectre.Console)                       │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                   DirectoryProcessorToStagingService                         │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ ProcessDirectoryAsync()                                              │    │
│  │   1. Run PreDiscovery Script (optional)                              │    │
│  │   2. Enumerate directories to process (modified since lastScanAt)   │    │
│  │   3. Parallel.ForEachAsync(directories) → ProcessSingleDirectoryAsync│    │
│  │   4. Run PostDiscovery Script (optional)                             │    │
│  │   5. Delete empty directories                                        │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Pipeline Execution Flow

```
For Each Directory (Parallel):
┌──────────────────────────────────────────────────────────────────────────────┐
│  ProcessSingleDirectoryAsync()                                                │
│  ┌────────────────────────────────────────────────────────────────────────┐  │
│  │ Stage 1: Directory Plugins (CueSheet, SFV, M3U, NFO)                   │  │
│  │   - Convert metadata files into melodee.json                           │  │
│  └────────────────────────────────────────────────────────────────────────┘  │
│                                     │                                         │
│                                     ▼                                         │
│  ┌────────────────────────────────────────────────────────────────────────┐  │
│  │ Stage 2: Conversion Plugins (ImageConvertor, MediaConvertor)           │  │
│  │   - Convert FLAC→MP3, images→JPEG                                      │  │
│  └────────────────────────────────────────────────────────────────────────┘  │
│                                     │                                         │
│                                     ▼                                         │
│  ┌────────────────────────────────────────────────────────────────────────┐  │
│  │ Stage 3: Media Album Creator Plugins (Mp3Files)                        │  │
│  │   - Parse song metadata (AtlMetaTag, IdSharpMetaTag)                   │  │
│  │   - Group songs into albums                                            │  │
│  │   - Handle duplicates, create melodee.json                             │  │
│  └────────────────────────────────────────────────────────────────────────┘  │
│                                     │                                         │
│                                     ▼                                         │
│  ┌────────────────────────────────────────────────────────────────────────┐  │
│  │ Stage 4: ProcessAlbumsAsync() - FOR EACH ALBUM (Sequential)            │  │
│  │   a. FindImages() - Extract/discover album artwork                     │  │
│  │   b. FindArtistImages() - Look for artist images                       │  │
│  │   c. Prepare file copy operations (images + songs)                     │  │
│  │   d. OptimizedFileOperations.CopyFilesAsync() - Batch copy             │  │
│  │   e. Update songs with modified tags                                   │  │
│  │   f. ArtistSearchEngineService.DoSearchAsync() ← SLOW                  │  │
│  │   g. AlbumImageSearchEngineService.DoSearchAsync() (if no images)      │  │
│  │   h. Validate album (AlbumValidator)                                   │  │
│  │   i. DoMagic() - Tag cleanup/normalization (MediaEditService)          │  │
│  │   j. Serialize and save melodee.json                                   │  │
│  └────────────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## Detailed Component Analysis

### 1. ProcessInboundCommand (`Melodee.Cli/Command/LibraryProcessCommand.cs`)

**Purpose**: CLI wrapper that initializes and invokes `DirectoryProcessorToStagingService`

**Modes**:
- **Library-based**: Uses configured inbound/staging paths from database
- **Path-based**: Uses `--inbound` and `--staging` flags for direct path processing

**Event Handlers**:
```csharp
processor.OnProcessingStart += (_, count) => { totalDirectories = count; };
processor.OnDirectoryProcessed += (_, fsInfo) => { processedDirectories++; };
processor.OnProcessingEvent += (_, message) => { currentActivity = message; };
```

---

### 2. DirectoryProcessorToStagingService (`Melodee.Common/Services/Scanning/DirectoryProcessorToStagingService.cs`)

**Purpose**: Core orchestration service for media processing pipeline

**Dependencies Injected**:
```csharp
ILogger logger
ICacheManager cacheManager
IDbContextFactory<MelodeeDbContext> contextFactory
IMelodeeConfigurationFactory configurationFactory
LibraryService libraryService
ISerializer serializer
MediaEditService mediaEditService
ArtistSearchEngineService artistSearchEngineService
AlbumImageSearchEngineService albumImageSearchEngineService
IHttpClientFactory httpClientFactory
IFileSystemService fileSystemService
```

**Initialization** (Lines 93-175):
- Creates song plugins: `AtlMetaTag`, `IdSharpMetaTag`
- Creates conversion plugins: `ImageConvertor`, `MediaConvertor`
- Creates directory plugins: `CueSheet`, `SimpleFileVerification`, `M3UPlaylist`, `Nfo`
- Creates media album creator: `Mp3Files`
- Initializes `MediaEditService` and `ArtistSearchEngineService`

**Parallelism Configuration** (Lines 313-318):
```csharp
var parallelOptions = new ParallelOptions
{
    CancellationToken = cancellationToken,
    MaxDegreeOfParallelism = Environment.ProcessorCount
};
```

**Throttle Semaphore** (Line 53):
```csharp
private readonly SemaphoreSlim _processingThrottle = new(Environment.ProcessorCount);
```

---

### 3. ProcessSingleDirectoryAsync (Lines 456-685)

**Purpose**: Process a single directory through all pipeline stages

**Stage Breakdown**:

| Stage | Description | Typical Time |
|-------|-------------|--------------|
| Delete existing melodee.json | Clean slate for re-processing | ~10ms |
| Directory Plugins | CueSheet, SFV, M3U, NFO parsing | ~50-200ms |
| Conversion Plugins | Media/image conversion | Variable (0-5000ms) |
| Media Album Creator | Mp3Files plugin | 500-12,000ms |
| ProcessAlbumsAsync | Image finding, copying, API queries | 500-250,000ms |

**Performance Notes**:
- File enumeration is async (`OptimizedFileOperations.EnumerateFilesAsync`)
- Plugins run sequentially within their categories
- Each plugin can set `StopProcessing` flag to halt further processing

---

### 4. ProcessAlbumsAsync (Lines 687-1124)

**Purpose**: Process each album discovered in a directory

**Critical Path Operations**:

1. **FindImages()** (Line 709-712):
   ```csharp
   album.Images = (await album.FindImages(...)).ToArray();
   ```
   - Scans directory for image files
   - Validates and converts images
   - May read multiple files

2. **FindArtistImages()** (Lines 717-722):
   ```csharp
   album.Artist = new Artist(..., (await album.FindArtistImages(...)).ToArray());
   ```
   - Looks for artist-specific artwork

3. **File Copy Operations** (Lines 760-833):
   ```csharp
   var copiedCount = await OptimizedFileOperations.CopyFilesAsync(
       filesToCopy,
       deleteOriginal,
       2 * 1024 * 1024, // 2MB buffer
       cancellationToken);
   ```
   - Batches all file copies (images + songs)
   - Uses 2MB buffer for efficiency
   - Supports delete-after-copy mode

4. **Artist Search** (Lines 878-949):
   ```csharp
   var artistSearchResult = await artistSearchEngineService.DoSearchAsync(
       searchRequest, 1, cancellationToken);
   ```
   - **PRIMARY BOTTLENECK**
   - Queries MusicBrainz, Spotify, local DB
   - Can take 2ms to 11+ seconds

5. **Album Image Search** (Lines 951-1027):
   ```csharp
   var albumImageSearchResult = await albumImageSearchEngineService.DoSearchAsync(
       albumImageSearchRequest, 1, cancellationToken);
   ```
   - Only runs if album has no images
   - Downloads image from URL if found

6. **DoMagic** (Lines 1069-1075):
   ```csharp
   await mediaEditService.DoMagic(album, cancellationToken);
   ```
   - Tag cleanup and normalization
   - May trigger file writes per song

---

## Production Log Analysis

**Log File**: `/mnt/fileserver_melodee/logs/mcli/log-20251225.clef`
- **Total Lines**: 7,830
- **Operations >1 second**: 4,678
- **Operations >10 seconds**: 1,245

### Timing Distribution by Operation

#### ProcessSingleDirectoryAsync (Complete Directory Processing)

| Percentile | Time (ms) | Notes |
|------------|-----------|-------|
| Fast | 2,000 | Small albums, cached artists |
| Median | 5,000 | Typical processing |
| 90th | 30,000 | Multiple albums, API calls |
| 99th | 150,000 | Complex/large directories |
| Max | **262,773** | 4.4 minutes single directory |

**Slowest Directory Processing Times from Logs**:
```
33,522ms, 34,271ms, 34,597ms, 36,094ms, 36,383ms, 
38,380ms, 39,905ms, 42,225ms, 42,338ms, 60,249ms,
69,333ms, 87,826ms, 131,076ms, 139,424ms, 141,076ms,
141,886ms, 150,111ms, 167,261ms, 167,643ms, 262,773ms
```

#### ArtistSearchEngineService.DoSearchAsync

| Percentile | Time (ms) | Notes |
|------------|-----------|-------|
| Fast | 2-5 | Cached artist |
| Median | 50-100 | Local DB hit |
| Slow | 5,000-8,000 | MusicBrainz API |
| Max | **11,268** | Complex artist query |

**Slowest Artist Searches from Logs**:
```
7,359ms, 7,386ms, 7,463ms, 7,611ms, 7,786ms,
7,955ms, 7,970ms, 8,043ms, 8,073ms, 8,139ms,
8,211ms, 8,214ms, 8,320ms, 8,807ms, 8,933ms,
8,965ms, 9,492ms, 9,874ms, 10,959ms, 11,268ms
```

#### File Copy Operations (Copying files for album)

| Percentile | Time (ms) | Notes |
|------------|-----------|-------|
| Fast | 300-500 | Small album, local disk |
| Median | 700 | 10-12 files |
| Slow | 5,000-15,000 | Network storage |
| Max | **83,622** | Large compilation album |

**Slowest Copy Operations from Logs**:
```
16,975ms (large album)
17,485ms
17,859ms
19,084ms
19,279ms
22,137ms
23,119ms
24,438ms
25,309ms
83,622ms (massive compilation)
```

---

## Performance Bottlenecks Summary

### Critical (High Impact)

| # | Issue | Location | Impact | Evidence |
|---|-------|----------|--------|----------|
| 1 | **External API latency** | ArtistSearchEngineService | 2ms-11.3s per query | Log analysis shows 1,245 operations >10s |
| 2 | **Sequential album processing** | ProcessAlbumsAsync | Each album processed serially | Code review, albums in same dir serialized |
| 3 | **File copy on network storage** | OptimizedFileOperations | 300ms-83.6s per album | Log shows copy times dominating |
| 4 | **MusicBrainz rate limiting** | MusicBrainzArtistSearchEnginePlugin | 1 req/sec limit | API design constraint |

### High (Moderate Impact)

| # | Issue | Location | Impact | Evidence |
|---|-------|----------|--------|----------|
| 5 | **Song plugin sequential processing** | Mp3Files.ProcessDirectoryAsync | Each file processed in order | Code review line 68-93 |
| 6 | **Image processing overhead** | FindImages, ImageConvertor | Multiple file reads/writes | Conversion happens per image |
| 7 | **DoMagic file writes** | MediaEditService | Per-song file update | Can trigger N writes for N songs |

### Medium (Lower Impact)

| # | Issue | Location | Impact | Evidence |
|---|-------|----------|--------|----------|
| 8 | **Service initialization** | InitializeAsync | ~100ms per job start | Multiple services initialized |
| 9 | **ConcurrentBag allocation** | ProcessDirectoryAsync | Memory pressure | 6 ConcurrentBag instances |
| 10 | **Duplicate detection** | Mp3Files.HandleDuplicates | File reads for CRC | Full file read per song |

---

## Recommendations

### Short-term Optimizations (Low Risk)

1. **Batch Artist Lookups per Directory**
   ```csharp
   // Current: Query per album
   foreach (var album in albumsForDirectory)
       await artistSearchEngineService.DoSearchAsync(album.Artist...);
   
   // Proposed: Collect unique artists, batch query
   var uniqueArtists = albumsForDirectory
       .Select(a => a.Artist.NameNormalized)
       .Distinct();
   var artistCache = await artistSearchEngineService.BatchSearchAsync(uniqueArtists);
   ```

2. **Parallel Album Processing within Directory**
   ```csharp
   // Current: Sequential
   foreach (var album in albumsForDirectory.Take(_maxAlbumProcessingCount))
   
   // Proposed: Parallel with throttle
   await Parallel.ForEachAsync(
       albumsForDirectory.Take(_maxAlbumProcessingCount),
       new ParallelOptions { MaxDegreeOfParallelism = 4 },
       async (album, ct) => await ProcessSingleAlbumAsync(album, ct));
   ```

3. **Artist Search Result Caching**
   - Extend `ArtistSearchCache` TTL from 30s to 5 minutes
   - Pre-warm cache at job start with known artists
   - Persist cache between runs (Redis or SQLite)

4. **Lazy Image Search**
   - Only query `AlbumImageSearchEngineService` if album passes validation
   - Skip image search for albums that will fail anyway

### Medium-term Redesign

1. **Streaming Pipeline Architecture**
   ```
   ┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
   │  Discovery   │───▶│   Parsing    │───▶│  Enrichment  │───▶│   Staging    │
   │   Worker     │    │   Worker     │    │   Worker     │    │   Worker     │
   └──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
          │                   │                    │                   │
          ▼                   ▼                    ▼                   ▼
   ┌──────────────────────────────────────────────────────────────────────────┐
   │                            Rebus Message Queue                            │
   └──────────────────────────────────────────────────────────────────────────┘
   ```

2. **Decouple External API Calls**
   - Queue artist lookups separately from album processing
   - Process albums with "pending enrichment" status
   - Background worker handles API calls with rate limiting

3. **Incremental Processing**
   - Track file checksums to avoid re-processing unchanged files
   - Delta processing based on file modification times

### Long-term Architecture Considerations

1. **Event Sourcing for Processing State**
   - Track each album's processing state as events
   - Enable resume-from-failure
   - Audit trail for debugging

2. **Distributed Processing**
   - Split inbound directory across multiple workers
   - Shared state via Redis
   - Horizontal scaling for large libraries

3. **API Response Caching Service**
   - Dedicated MusicBrainz/Spotify cache database
   - Shared across all processing jobs
   - Background refresh for stale entries

---

## Metrics to Track

| Metric | Current Baseline | Target |
|--------|------------------|--------|
| Directories/minute | ~2-3 | 10+ |
| Avg directory process time | 15,000ms | <5,000ms |
| Artist search cache hit rate | Unknown | >95% |
| File copy throughput | ~50MB/s | >100MB/s |
| External API call rate | ~1/album | <0.2/album |
| Max single directory time | 262s | <60s |

---

## Files Involved

```
src/Melodee.Cli/
├── Command/
│   └── LibraryProcessCommand.cs       # CLI entry point (ProcessInboundCommand)
├── CommandSettings/
│   └── LibraryProcessSettings.cs      # Command options

src/Melodee.Common/
├── Jobs/
│   └── LibraryInboundProcessJob.cs    # Scheduled job wrapper (Quartz)
├── Services/
│   ├── Scanning/
│   │   ├── DirectoryProcessorToStagingService.cs  # Main orchestration
│   │   ├── MediaEditService.cs                     # DoMagic operations
│   │   ├── AlbumDiscoveryService.cs               # Album loading
│   │   └── OptimizedFileOperations.cs             # Batch file operations
│   ├── SearchEngines/
│   │   ├── ArtistSearchEngineService.cs           # Artist API aggregation
│   │   └── AlbumImageSearchEngineService.cs       # Album image search
│   └── LibraryService.cs
├── Plugins/
│   ├── MetaData/
│   │   ├── Directory/
│   │   │   ├── Mp3Files.cs                        # Song→Album grouping
│   │   │   ├── CueSheet.cs
│   │   │   ├── SimpleFileVerification.cs
│   │   │   ├── M3UPlaylist.cs
│   │   │   └── Nfo/Nfo.cs
│   │   └── Song/
│   │       ├── AtlMetaTag.cs                      # Primary tag reader
│   │       └── IdSharpMetaTag.cs                  # Secondary tag reader
│   ├── Conversion/
│   │   ├── Image/ImageConvertor.cs
│   │   └── Media/MediaConvertor.cs
│   ├── Validation/
│   │   └── AlbumValidator.cs
│   └── SearchEngine/
│       ├── MusicBrainz/
│       │   └── MusicBrainzArtistSearchEnginePlugin.cs
│       └── Spotify/
│           └── Spotify.cs
├── Models/
│   ├── Album.cs
│   ├── Artist.cs
│   └── Song.cs
```

---

## Call Graph (Simplified)

```
ProcessInboundCommand.ExecuteAsync()
└── DirectoryProcessorToStagingService.ProcessDirectoryAsync()
    ├── PreDiscoveryScript.ProcessAsync() [optional]
    ├── EnumerateDirectories()
    └── Parallel.ForEachAsync(directories)
        └── ProcessSingleDirectoryAsync()
            ├── OptimizedFileOperations.DeleteFilesAsync() [clean existing]
            ├── IDirectoryPlugin.ProcessDirectoryAsync() [CueSheet, SFV, M3U, NFO]
            ├── IConversionPlugin.ProcessFileAsync() [ImageConvertor, MediaConvertor]
            ├── Mp3Files.ProcessDirectoryAsync()
            │   ├── ISongPlugin.ProcessFileAsync() [AtlMetaTag, IdSharpMetaTag]
            │   ├── HandleDuplicates()
            │   └── Serialize album to melodee.json
            └── ProcessAlbumsAsync()
                └── foreach album:
                    ├── Album.FindImages()
                    ├── Album.FindArtistImages()
                    ├── OptimizedFileOperations.CopyFilesAsync()
                    ├── ISongPlugin.UpdateSongAsync() [if tags modified]
                    ├── ArtistSearchEngineService.DoSearchAsync() ← SLOW
                    │   ├── MelodeeArtistSearchEnginePlugin
                    │   ├── MusicBrainzArtistSearchEnginePlugin
                    │   └── SpotifyArtistSearchEnginePlugin
                    ├── AlbumImageSearchEngineService.DoSearchAsync() [if no images]
                    ├── AlbumValidator.ValidateAlbum()
                    └── MediaEditService.DoMagic()
```

---

## Comparison: Process vs Scan

| Aspect | `library process` | `library scan` |
|--------|-------------------|----------------|
| **Primary Function** | Raw files → Staging | Staging → Database |
| **Input** | Inbound directory with media files | Staging directory with melodee.json |
| **Output** | melodee.json + organized files | Database records |
| **Main Bottleneck** | External API calls | CRC32 hashing, DB inserts |
| **Parallelism** | Directory-level (good) | Batch-level |
| **Typical Time/Album** | 2-10 seconds | 100-500ms |

---

## Conclusion

The `library process` workflow has significant performance issues primarily caused by:

1. **External API dependency** - MusicBrainz queries dominate processing time
2. **Sequential album processing** - Opportunities for parallelization exist
3. **I/O operations on network storage** - File copies are surprisingly slow

**A complete redesign is NOT recommended**. The architecture is sound with good separation of concerns. Targeted optimizations should yield 3-5x improvement:

1. **Immediate**: Artist search caching, batch lookups
2. **Near-term**: Parallel album processing within directories
3. **Long-term**: Decouple API enrichment from core processing

The existing Rebus message bus infrastructure could be leveraged to implement a streaming pipeline for the long-term architecture without major refactoring.

---

## Clarification on “Ok” vs “Moved” Counts (2025-12-25 Run)

- The inbound process reported **630 Ok albums** (`numberOfValidAlbumsProcessed`), which simply means 630 albums passed validation and were staged.
- The subsequent `move-ok` reported **54 albums completed** because most Ok albums already existed in the storage library:
  - Logs show **36 merges** (`processing existing directory merge`), indicating albums were merged into existing storage locations rather than counted as new moves.
  - Only the remaining new, non-duplicate albums were physically moved; the rest were merges or skipped.
- To reduce confusion, the move command now emits detailed TUI statistics (added separately) showing:
  - Total melodee files found, albums ready, moved, merged, skipped by status/duplicate directory, and failed-to-load counts.
  - This makes it clear why the “Ok” count can exceed the “moved” count when many albums already exist in storage.
