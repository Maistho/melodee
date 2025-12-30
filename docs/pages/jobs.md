---
title: Background Jobs
permalink: /jobs/
---

# Background Jobs

Melodee uses [Quartz.NET](https://www.quartz-scheduler.net/) for background job scheduling. Jobs handle automated tasks like scanning libraries, moving files, updating the database, and maintaining data integrity.

## Job Overview

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  LibraryInbound │     │  StagingAuto    │     │  LibraryInsert  │
│   ProcessJob    │────▶│    MoveJob      │────▶│      Job        │
│  (every 10 min) │     │  (every 15 min) │     │   (chained)     │
└─────────────────┘     └─────────────────┘     └─────────────────┘
         │                                               │
         └───────────── Media Ingestion Chain ───────────┘
```

## Media Ingestion Jobs

These jobs form an automated chain that processes music from inbound through to the playable database.

### LibraryInboundProcessJob

Scans the inbound library for new media files and processes them into staging.

| Property | Value |
|----------|-------|
| **Default Schedule** | Every 10 minutes (`0 */10 * * * ?`) |
| **Setting Key** | `jobs.libraryProcess.cronExpression` |
| **Chains To** | StagingAutoMoveJob (when scheduled) |

**What It Does:**

1. Checks if the inbound library needs scanning (compares timestamps)
2. Scans all subdirectories for supported audio files
3. Parses ID3/Vorbis tags and extracts album artwork
4. Validates and normalizes metadata (artist names, album titles, track numbers)
5. Creates `melodee.json` metadata files for each discovered album
6. Moves processed albums to the staging directory
7. Records scan history for monitoring

**Skip Conditions:**

- Inbound library path not configured
- Library is locked (`IsLocked=true`)
- No changes detected since last scan

### StagingAutoMoveJob

Automatically moves validated albums from staging to storage.

| Property | Value |
|----------|-------|
| **Default Schedule** | Every 15 minutes (`0 */15 * * * ?`) |
| **Setting Key** | `jobs.stagingAutoMove.cronExpression` |
| **Chains To** | LibraryInsertJob (when scheduled) |

**What It Does:**

1. Retrieves the staging library configuration
2. Finds the target storage library (first unlocked storage library)
3. Scans staging for albums with `AlbumStatus.Ok`
4. Moves each qualifying album to the storage library
5. Preserves album directory structure (Artist/Album format)

**Skip Conditions:**

- Staging library is locked
- No storage libraries configured
- All storage libraries are locked
- No albums with "Ok" status in staging

**Why This Matters:**

This job enables truly automatic music ingestion. Well-tagged albums that pass validation flow through the entire pipeline without manual intervention. Albums that need review remain in staging until manually approved.

### StagingAlbumRevalidationJob

Periodically re-validates albums in staging that have invalid or unknown artists.

| Property | Value |
|----------|-------|
| **Default Schedule** | Weekly on Sunday at 3am (`0 0 3 ? * SUN`) |
| **Setting Key** | `jobs.stagingAlbumRevalidation.cronExpression` |
| **Chains To** | None (independent job) |

**What It Does:**

1. Retrieves the staging library configuration
2. Scans for albums with `HasInvalidArtists` or `HasUnknownArtist` status reasons
3. Re-queries the ArtistSearchEngineService for each album's artist
4. If the artist is now found in search engines, updates the album with the result
5. Re-validates the album - if now valid, status becomes `AlbumStatus.Ok`
6. Saves updated albums so StagingAutoMoveJob can move them to storage

**Skip Conditions:**

- Staging library is locked
- No albums with invalid/unknown artist status in staging

**Why This Matters:**

This job provides a self-healing mechanism for albums that were processed before their artist data became available in external sources (MusicBrainz, Spotify, etc.). When an artist is later added to these sources, this job automatically detects and fixes affected albums. This is particularly useful for:

- New or indie artists who may not have immediate search engine coverage
- Albums processed during search engine downtime
- Bulk imports where some artists weren't recognized initially

Albums that become valid through revalidation will automatically flow through to storage via StagingAutoMoveJob, completing the ingestion pipeline without manual intervention.

### LibraryInsertJob

Reads metadata from storage libraries and inserts records into the database, making music playable via API.

| Property | Value |
|----------|-------|
| **Default Schedule** | Daily at midnight (`0 0 0 * * ?`) |
| **Setting Key** | `jobs.libraryInsert.cronExpression` |
| **Chains To** | None (terminal job) |

**What It Does:**

1. Scans all storage libraries for `melodee.json` files
2. Filters to files modified since last scan (unless force mode)
3. Loads and validates album metadata from each file
4. Creates or finds existing Artist records (by name, MusicBrainz ID, or Spotify ID)
5. Creates Album records with metadata (genres, release date, duration, etc.)
6. Creates Song records with media file details (bitrate, duration, file hash)
7. Creates Contributor records for performers, producers, and publishers
8. Updates library aggregates (total albums, songs, duration)
9. Records scan history

**Special Handling:**

- Duplicate albums are prefixed with `__duplicate_` for manual review
- Invalid `melodee.json` files trigger reprocessing back to staging
- Missing media files (referenced in JSON but not on disk) trigger reprocessing

**Configuration Settings:**

- `ProcessingMaximumProcessingCount`: Maximum songs per run (0 = unlimited)
- `ProcessingDuplicateAlbumPrefix`: Prefix for duplicate album directories
- `ProcessingIgnoredPerformers/Publishers/Production`: Names to exclude from contributors

## Housekeeping Jobs

These jobs maintain data integrity and optimize system performance.

### ArtistHousekeepingJob

Performs cleanup and maintenance on artist data.

| Property | Value |
|----------|-------|
| **Default Schedule** | Daily at midnight (`0 0 0 * * ?`) |
| **Setting Key** | `jobs.artistHousekeeping.cronExpression` |

**What It Does:**

1. Removes orphaned artist records (artists with no albums)
2. Updates artist statistics and aggregates
3. Cleans up duplicate or merged artist entries
4. Maintains artist relationship data

### ArtistSearchEngineRepositoryHousekeepingJob

Updates and maintains the artist search index for fast lookups.

| Property | Value |
|----------|-------|
| **Default Schedule** | Daily at midnight (`0 0 0 * * ?`) |
| **Setting Key** | `jobs.artistSearchEngineHousekeeping.cronExpression` |

**What It Does:**

1. Rebuilds artist search indexes
2. Updates search terms and aliases
3. Removes stale search entries for deleted artists
4. Optimizes search performance

### NowPlayingCleanupJob

Cleans up stale "now playing" entries from the database.

| Property | Value |
|----------|-------|
| **Default Schedule** | Every 5 minutes (`0 */5 * * * ?`) |
| **Setting Key** | Not configurable (fixed schedule) |

**What It Does:**

1. Finds "now playing" entries older than the expected duration
2. Removes stale entries that weren't properly cleared
3. Maintains accurate now-playing statistics

## Data Update Jobs

These jobs update external data and maintain integrations.

### ChartUpdateJob

Links chart entries (Billboard, etc.) to albums in the database.

| Property | Value |
|----------|-------|
| **Default Schedule** | Daily at 2 AM (`0 0 2 * * ?`) |
| **Setting Key** | `jobs.chartUpdate.cronExpression` |

**What It Does:**

1. Scans all charts in the database
2. Finds unlinked chart items (entries without album associations)
3. Searches for matching albums by artist name and album title
4. Creates links between chart items and albums
5. Updates chart statistics

**Why This Matters:**

Charts are imported separately from the music library. This job connects chart data to your actual music, enabling features like "show albums from Billboard Top 100" filtering.

### MusicBrainzUpdateDatabaseJob

Updates the local MusicBrainz database cache for artist and album lookups.

| Property | Value |
|----------|-------|
| **Default Schedule** | First of each month at noon (`0 0 12 1 * ?`) |
| **Setting Key** | `jobs.musicbrainzUpdateDatabase.cronExpression` |

**What It Does:**

1. Downloads MusicBrainz database updates
2. Imports new artist and release data
3. Updates existing records with corrections
4. Maintains the local search database

**Configuration Settings:**

- `searchEngine.musicbrainz.storagePath`: Where to store the local database
- `searchEngine.musicbrainz.importMaximumToProcess`: Batch size limit
- `searchEngine.musicbrainz.importBatchSize`: Records per batch

## Job Chaining

When jobs are triggered by the scheduler (not manually), they automatically chain to the next job in sequence:

```
LibraryInboundProcessJob
        │
        ▼ (on success, if work was done)
StagingAutoMoveJob
        │
        ▼ (on success, if albums were moved)
LibraryInsertJob
        │
        ▼ (terminal - no further chaining)
```

### Automatic vs Manual Mode

**Scheduled Triggers (Automatic Chaining):**

- Jobs run via cron schedule
- On success, trigger the next job in chain
- Enables fully automatic media ingestion

**Manual Triggers (No Chaining):**

- Jobs triggered from the admin UI
- Do NOT chain to subsequent jobs
- Allows troubleshooting and selective execution

This design means:
- Drop files in inbound → music is playable within ~20 minutes (if validated)
- Manual triggers give full control for debugging or selective processing

## Managing Jobs

### Admin UI

Access the jobs dashboard at `/admin/jobs` to:

- View all scheduled jobs and their status
- See currently executing jobs
- View last execution time and next scheduled run
- Manually trigger any job
- Pause or resume individual jobs
- Pause or resume the entire scheduler

### Disabling Jobs

To disable a job, set its cron expression to empty in the Settings:

1. Navigate to `/admin/settings`
2. Find the job's cron expression setting
3. Clear the value (leave empty)
4. Save changes

Or set in the database:
```sql
UPDATE "Settings" SET "Value" = '' WHERE "Key" = 'jobs.libraryProcess.cronExpression';
```

### Cron Expression Reference

Quartz cron expressions use 6-7 fields:

```
┌───────────── second (0-59)
│ ┌───────────── minute (0-59)
│ │ ┌───────────── hour (0-23)
│ │ │ ┌───────────── day of month (1-31)
│ │ │ │ ┌───────────── month (1-12)
│ │ │ │ │ ┌───────────── day of week (0-6, SUN-SAT)
│ │ │ │ │ │
* * * * * ?
```

**Common Examples:**

| Expression | Description |
|------------|-------------|
| `0 */10 * * * ?` | Every 10 minutes |
| `0 */15 * * * ?` | Every 15 minutes |
| `0 0 * * * ?` | Every hour |
| `0 0 0 * * ?` | Daily at midnight |
| `0 0 2 * * ?` | Daily at 2 AM |
| `0 0 12 1 * ?` | First of each month at noon |
| `0 0 0 * * 0` | Every Sunday at midnight |

Use [Cron Expression Generator](https://www.freeformatter.com/cron-expression-generator-quartz.html) for complex schedules.

## Troubleshooting

### Job Not Running

1. Check if the cron expression is set (empty = disabled)
2. Check if the scheduler is paused (admin UI shows status)
3. Check if the specific job is paused
4. Review logs for errors during execution

### Job Running But No Effect

1. Check skip conditions (library locked, no changes, etc.)
2. Review job logs for warnings
3. Verify library paths are accessible
4. Check file permissions on library directories

### Jobs Taking Too Long

1. Check `ProcessingMaximumProcessingCount` setting
2. Consider reducing batch sizes
3. Review hardware resources (CPU, I/O)
4. Check for locked files or network issues

### Viewing Job Logs

Job execution is logged to the standard application log:

```bash
# Docker/Podman logs
docker logs melodee-blazor | grep -i "job"

# Or search for specific job
docker logs melodee-blazor | grep "LibraryInboundProcessJob"
```

Log messages include:
- Job start/stop times
- Items processed counts
- Skip reasons
- Errors and warnings
