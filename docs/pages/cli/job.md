---
title: CLI - Job Commands
permalink: /cli/job/
layout: page
---

# Job Commands

The `job` branch provides commands for viewing and running background maintenance jobs.

## Overview

```bash
mcli job [COMMAND] [OPTIONS]
```

**Available Commands:**

| Command | Description |
|---------|-------------|
| `list` | List all known background jobs with their execution history and statistics |
| `run` | Run a specific background job by name |
| `artistsearchengine-refresh` | Run artist search engine refresh job |
| `musicbrainz-update` | Run MusicBrainz database update job |

---

## job list

Lists all known background jobs with their execution history and statistics.

### Usage

```bash
mcli job list [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--raw` | | `false` | Output results in JSON format |
| `--verbose` | | `true` | Output verbose debug and timing results |

### Examples

```bash
# List all jobs with statistics
./mcli job list

# JSON output for monitoring
./mcli job list --raw
```

### Output

```
╭──────┬────────────┬────────────────┬─────────────╮
│ Jobs │ Total Runs │ Failures (24h) │ Active Jobs │
├──────┼────────────┼────────────────┼─────────────┤
│  9   │     156    │       0        │      4      │
╰──────┴────────────┴────────────────┴─────────────╯

╭─────────────────────────────────────────┬──────┬─────────┬──────────┬─────────────────┬─────────────────┬─────────╮
│ Job Name                                │ Runs │ Success │ Avg Time │    Last Run     │   Next Run *    │ Status  │
├─────────────────────────────────────────┼──────┼─────────┼──────────┼─────────────────┼─────────────────┼─────────┤
│ ArtistHousekeepingJob                   │   45 │    100% │     92ms │ 20241230T130000 │ 20241230T140000 │  ✓ OK   │
│ ArtistSearchEngineHousekeepingJob       │   12 │    100% │   1.2sec │ 20241230T120000 │ 20241231T000000 │  ✓ OK   │
│ ChartUpdateJob                          │    8 │    100% │    450ms │ 20241230T060000 │ 20241231T060000 │  ✓ OK   │
│ LibraryInboundProcessJob                │   56 │     98% │     57ms │ 20241230T130500 │ 20241230T131000 │  ✓ OK   │
│ LibraryProcessJob                       │   23 │    100% │   2.3sec │ 20241230T130000 │ 20241230T140000 │  ✓ OK   │
│ MusicBrainzUpdateDatabaseJob            │    1 │    100% │  45.2min │ 20241228T020000 │ 20250104T020000 │  ✓ OK   │
│ NowPlayingCleanupJob                    │   11 │    100% │     12ms │ 20241230T130000 │ 20241230T130500 │  ✓ OK   │
│ StagingAlbumRevalidationJob             │    2 │    100% │    3.4s  │ 20241230T020000 │ 20250106T020000 │  ✓ OK   │
│ StagingAutoMoveJob                      │   15 │    100% │    890ms │ 20241230T130000 │ 20241230T130500 │  ✓ OK   │
╰─────────────────────────────────────────┴──────┴─────────┴──────────┴─────────────────┴─────────────────┴─────────╯

* Next Run times are approximate, calculated from cron expressions
```

### Summary Statistics

| Metric | Description |
|--------|-------------|
| Jobs | Total number of configured jobs |
| Total Runs | Sum of all job executions |
| Failures (24h) | Jobs that failed in the last 24 hours |
| Active Jobs | Jobs that have run at least once |

### Job Status Indicators

| Status | Description |
|--------|-------------|
| ✓ OK | Job last ran successfully |
| ✗ Failed | Job last run failed |
| No runs | Job has never been executed |

### JSON Output

```json
[
  {
    "JobName": "ArtistHousekeepingJob",
    "RunCount": 45,
    "SuccessRate": 100,
    "AverageRunTimeMs": 92,
    "LastRunAt": "2024-12-30T13:00:00Z",
    "LastRunStatus": "Success"
  }
]
```

---

## job run

Run a specific background job by name.

### Usage

```bash
mcli job run -j <JOB_NAME> [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `-j`, `--job-name` | | **Required** | Name of the job to run |
| `--verbose` | | `true` | Output verbose debug and timing results |

### Available Jobs

| Job Name | Description |
|----------|-------------|
| `ArtistHousekeepingJob` | Cleans up artist data and removes orphaned records |
| `ArtistSearchEngineHousekeepingJob` | Updates artist search engine database |
| `ChartUpdateJob` | Updates music charts |
| `LibraryInboundProcessJob` | Processes files in the inbound library |
| `LibraryProcessJob` | General library processing |
| `MusicBrainzUpdateDatabaseJob` | Downloads and updates MusicBrainz database |
| `NowPlayingCleanupJob` | Cleans up old now-playing records |
| `StagingAlbumRevalidationJob` | Re-validates albums with invalid artists |
| `StagingAutoMoveJob` | Automatically moves OK albums from staging |

### Examples

```bash
# Run artist housekeeping
./mcli job run -j ArtistHousekeepingJob

# Run library processing
./mcli job run -j LibraryProcessJob

# Run MusicBrainz update (long-running)
./mcli job run -j MusicBrainzUpdateDatabaseJob
```

### Output

```
Starting job: ArtistHousekeepingJob
✓ Job completed successfully: ArtistHousekeepingJob
```

### Error Output

```
Starting job: InvalidJobName
✗ Job 'InvalidJobName' not found. Available jobs:
  - ArtistHousekeepingJob
  - ArtistSearchEngineHousekeepingJob
  - ChartUpdateJob
  - LibraryInboundProcessJob
  - LibraryProcessJob
  - MusicBrainzUpdateDatabaseJob
  - NowPlayingCleanupJob
  - StagingAutoMoveJob
```

---

## job artistsearchengine-refresh

Refreshes the artist search engine database by updating local data from external search engines.

### Usage

```bash
mcli job artistsearchengine-refresh [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--verbose` | | `true` | Output verbose debug and timing results |

### What It Does

1. Queries configured search engines for artist information
2. Updates local artist database with new albums and metadata
3. Refreshes artist images and biographical data
4. Records job execution history

### Example

```bash
./mcli job artistsearchengine-refresh
```

### Output

```
Starting Artist Search Engine Refresh...
Processing: 1,234 artists
✓ Updated: 156 artists with new data
✓ Completed in 2m 34s
```

---

## job musicbrainz-update

Downloads and updates the local MusicBrainz database used for metadata enrichment during scanning.

### Usage

```bash
mcli job musicbrainz-update [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--verbose` | | `true` | Output verbose debug and timing results |

### What It Does

1. Downloads the latest MusicBrainz data dump
2. Extracts and processes the data
3. Creates/updates the local SQLite database
4. Enables offline metadata lookups

### Requirements

- Sufficient disk space (several GB for the dump)
- Network access to MusicBrainz servers
- May take 30+ minutes depending on connection speed

### Example

```bash
./mcli job musicbrainz-update
```

### Output

```
Starting MusicBrainz Database Update...
Downloading data dump... 2.3 GB / 2.3 GB (100%)
Extracting archive... Done
Processing data... 
  - Artists: 1,234,567
  - Albums: 2,345,678
  - Recordings: 12,345,678
✓ Database updated successfully
✓ Completed in 45m 12s
```

---

## Scheduling Jobs

Jobs are normally scheduled automatically using cron expressions. The CLI commands allow manual execution for:

- Testing job functionality
- Running jobs outside their normal schedule
- Recovering from failed scheduled runs
- Initial setup and verification

### View Job Schedule

```bash
# Get cron expression for a job
./mcli configuration get jobs.libraryProcess.cronExpression
```

### Modify Job Schedule

```bash
# Run every 5 minutes
./mcli configuration set jobs.libraryProcess.cronExpression "0 */5 * * * ?"

# Run every hour
./mcli configuration set jobs.libraryProcess.cronExpression "0 0 * * * ?"

# Run at 2 AM daily
./mcli configuration set jobs.libraryProcess.cronExpression "0 0 2 * * ?"

# Disable job (empty expression)
./mcli configuration set jobs.someJob.cronExpression ""
```

---

## Scripting and Automation

### Daily Maintenance Script

```bash
#!/bin/bash
# Daily maintenance jobs

export MELODEE_APPSETTINGS_PATH="/etc/melodee/appsettings.json"

echo "Running daily maintenance..."

# Housekeeping
./mcli job run -j ArtistHousekeepingJob
./mcli job run -j NowPlayingCleanupJob

# Process any pending items
./mcli job run -j LibraryInboundProcessJob
./mcli job run -j StagingAutoMoveJob

echo "Daily maintenance complete"
```

### Monitoring Job Status

```bash
#!/bin/bash
# Check for job failures

FAILURES=$(./mcli job list --raw | jq '[.[] | select(.LastRunStatus == "Failed")] | length')

if [ "$FAILURES" -gt 0 ]; then
    echo "WARNING: $FAILURES job(s) failed"
    ./mcli job list --raw | jq '.[] | select(.LastRunStatus == "Failed") | .JobName'
    exit 1
fi

echo "All jobs healthy"
exit 0
```

### Weekly MusicBrainz Update

```bash
#!/bin/bash
# Weekly MusicBrainz database refresh (run Sunday at 2 AM)

export MELODEE_APPSETTINGS_PATH="/etc/melodee/appsettings.json"

echo "Starting weekly MusicBrainz update..."
./mcli job musicbrainz-update

if [ $? -eq 0 ]; then
    echo "MusicBrainz update completed successfully"
else
    echo "MusicBrainz update failed" >&2
    exit 1
fi
```

---

## Troubleshooting

### Job Not Found

```
Error: Job 'InvalidName' not found
```

**Solution:** Check exact job name with `job list`:

```bash
./mcli job list | grep -i "keyword"
```

### Job Fails Immediately

**Possible causes:**
1. Database connection issues
2. Missing configuration
3. Insufficient permissions

**Solution:** Check logs and try with verbose output:

```bash
./mcli job run -j JobName --verbose
```

### Job Runs But No Effect

**Possible causes:**
1. No data to process
2. Job skipped due to recent run
3. Configuration preventing action

**Solution:** Check job conditions and force run if needed.

---

## See Also

- [Background Jobs](/jobs/) - Detailed job documentation
- [Configuration Commands](/cli/configuration/) - Modify job schedules
- [CLI Overview](/cli/) - Main CLI documentation
