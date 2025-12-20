---
title: Backup & Recovery
permalink: /backup/
---

# Backup & Recovery Guide

This guide covers comprehensive backup strategies and disaster recovery procedures for Melodee in homelab environments, ensuring your music library and metadata remain safe.

## Understanding Melodee Data

### Critical Data Components

Melodee stores data across multiple volumes and components:

1. **Database (PostgreSQL)**: Core metadata, user accounts, playlists, ratings, play history
2. **Storage Volume**: Processed and organized music files
3. **User Images**: User avatars and profile images
4. **Playlists**: Custom playlist definitions and configurations
5. **Logs**: System logs for troubleshooting (optional to backup)

### Data Importance Ranking

| Component | Criticality | Backup Frequency | Recovery Priority |
|-----------|-------------|------------------|-------------------|
| Database | Critical | Daily | 1st |
| Storage | Critical | Daily/Incremental | 2nd |
| User Images | Important | Weekly | 3rd |
| Playlists | Important | Weekly | 4th |
| Logs | Optional | As needed | Last |

## Backup Strategies

### Full Backup Approach

A complete backup includes all critical components:

```bash
#!/bin/bash
# full-backup-melodee.sh

BACKUP_ROOT="/backup/melodee"
DATE=$(date +%Y-%m-%d)
BACKUP_DIR="$BACKUP_ROOT/$DATE"
mkdir -p "$BACKUP_DIR"

echo "Starting full backup: $DATE"

# Stop services for consistent backup
echo "Stopping Melodee services..."
docker-compose down

# Export database as SQL dump (more portable than volume export)
echo "Backing up database..."
docker exec melodee-db pg_dump -U melodeeuser -d melodeedb > "$BACKUP_DIR/db_dump.sql"

# Export volumes
echo "Exporting volumes..."
docker run --rm -v melodee_storage:/volume -v "$BACKUP_DIR:/backup" alpine tar czf /backup/storage.tar.gz -C /volume .
docker run --rm -v melodee_user_images:/volume -v "$BACKUP_DIR:/backup" alpine tar czf /backup/user_images.tar.gz -C /volume .
docker run --rm -v melodee_playlists:/volume -v "$BACKUP_DIR:/backup" alpine tar czf /backup/playlists.tar.gz -C /volume .

# Create backup manifest
cat > "$BACKUP_DIR/manifest.txt" << EOF
Backup Date: $DATE
Components: Database, Storage, User Images, Playlists
Database Size: $(du -h "$BACKUP_DIR/db_dump.sql" | cut -f1)
Storage Size: $(du -h "$BACKUP_DIR/storage.tar.gz" | cut -f1)
EOF

# Start services
echo "Starting Melodee services..."
docker-compose up -d

echo "Full backup completed: $BACKUP_DIR"
```

### Incremental Backup Approach

For large libraries, incremental backups are more practical:

```bash
#!/bin/bash
# incremental-backup-melodee.sh

BACKUP_ROOT="/backup/melodee"
DATE=$(date +%Y-%m-%d)
INCREMENTAL_DIR="$BACKUP_ROOT/incremental/$DATE"
mkdir -p "$INCREMENTAL_DIR"

echo "Starting incremental backup: $DATE"

# Backup database (smaller, frequent backups)
docker exec melodee-db pg_dump -U melodeeuser -d melodeedb --format=custom > "$INCREMENTAL_DIR/db_backup.custom"

# Only backup new/modified files in storage volume
# First, create a reference point if this is the first incremental backup
if [ ! -f "$BACKUP_ROOT/latest_timestamp" ]; then
    # Full backup for first run
    docker run --rm -v melodee_storage:/volume -v "$INCREMENTAL_DIR:/backup" alpine tar czf /backup/storage.tar.gz -C /volume .
    touch "$BACKUP_ROOT/latest_timestamp"
else
    # Find files modified since last backup
    LAST_BACKUP_TIME=$(cat "$BACKUP_ROOT/latest_timestamp")
    docker run --rm -v melodee_storage:/volume -v "$INCREMENTAL_DIR:/backup" alpine sh -c "
        find /volume -newer /backup/last_backup_marker 2>/dev/null | 
        tar -czf /backup/storage_incremental.tar.gz -C /volume -T -
    " || 
    # Fallback: backup everything if find fails
    docker run --rm -v melodee_storage:/volume -v "$INCREMENTAL_DIR:/backup" alpine tar czf /backup/storage_incremental.tar.gz -C /volume .
fi

# Update timestamp
date > "$BACKUP_ROOT/latest_timestamp"

echo "Incremental backup completed: $INCREMENTAL_DIR"
```

### Database-Only Backup (Frequent)

For critical metadata protection:

```bash
#!/bin/bash
# db-backup.sh

BACKUP_DIR="/backup/melodee/database"
DATE=$(date +%Y-%m-%d-%H%M%S)
mkdir -p "$BACKUP_DIR"

# Create compressed database dump
docker exec melodee-db pg_dump -U melodeeuser -d melodeedb | gzip > "$BACKUP_DIR/db_backup_$DATE.sql.gz"

# Keep only last 7 days of database backups
find "$BACKUP_DIR" -name "db_backup_*.sql.gz" -mtime +7 -delete

echo "Database backup completed: $BACKUP_DIR/db_backup_$DATE.sql.gz"
```

## Backup Automation

### Cron Scheduling

Add to crontab for automated backups:

```bash
# Edit crontab
crontab -e

# Add backup schedules
# Daily database backup at 2 AM
0 2 * * * /path/to/db-backup.sh

# Weekly full backup on Sundays at 3 AM
0 3 * * 0 /path/to/full-backup-melodee.sh

# Daily incremental backup at 11 PM
0 23 * * * /path/to/incremental-backup-melodee.sh
```

### Docker-based Backup

Create a dedicated backup container:

```yaml
# Add to compose.yml
services:
  melodee-backup:
    image: alpine:latest
    volumes:
      - db_data:/db_data:ro
      - storage:/storage:ro
      - user_images:/user_images:ro
      - playlists:/playlists:ro
      - /backup/melodee:/backup
      - /var/run/docker.sock:/var/run/docker.sock
    environment:
      - BACKUP_SCHEDULE=daily
      - RETENTION_DAYS=30
    command: >
      sh -c "
        apk add --no-cache postgresql-client tar gzip &&
        while true; do
          DATE=$$(date +%Y-%m-%d)
          BACKUP_DIR=/backup/$$DATE
          mkdir -p $$BACKUP_DIR
          
          # Database backup
          pg_dump -h melodee-db -U melodeeuser -d melodeedb | gzip > $$BACKUP_DIR/db_dump.sql.gz
          
          # Volume backups
          tar czf $$BACKUP_DIR/storage.tar.gz -C /storage .
          tar czf $$BACKUP_DIR/user_images.tar.gz -C /user_images .
          tar czf $$BACKUP_DIR/playlists.tar.gz -C /playlists .
          
          # Cleanup old backups
          find /backup -mindepth 1 -maxdepth 1 -type d -mtime +$$RETENTION_DAYS -exec rm -rf {} +
          
          # Wait for next backup
          sleep 86400
        done
      "
    restart: unless-stopped
```

## Off-Site Backup Solutions

### Cloud Storage Integration

**AWS S3:**
```bash
#!/bin/bash
# s3-backup.sh

# Install AWS CLI if not present
# apt-get install awscli

# Sync local backups to S3
aws s3 sync /backup/melodee s3://your-melodee-backup-bucket --delete

# Verify backup integrity
aws s3 ls s3://your-melodee-backup-bucket --recursive
```

**rsync to Remote Server:**
```bash
#!/bin/bash
# remote-backup.sh

# Ensure SSH key authentication is set up
rsync -avz --delete /backup/melodee/ user@remote-server:/path/to/remote/backup/melodee/
```

### Versioned Backup Strategy

```bash
#!/bin/bash
# versioned-backup.sh

BACKUP_ROOT="/backup/melodee"
DATE=$(date +%Y-%m-%d)
WEEK=$(date +%U)

# Daily backups (keep 7 days)
DAILY_DIR="$BACKUP_ROOT/daily"
mkdir -p "$DAILY_DIR"
# Perform daily backup to this location

# Weekly backups (keep 4 weeks)
WEEKLY_DIR="$BACKUP_ROOT/weekly/week_$WEEK"
if [ ! -d "$WEEKLY_DIR" ]; then
    # Only backup if different from last week
    mkdir -p "$WEEKLY_DIR"
    # Perform weekly backup
fi

# Monthly backups (keep 6 months)
MONTH=$(date +%Y-%m)
MONTHLY_DIR="$BACKUP_ROOT/monthly/$MONTH"
if [ ! -d "$MONTHLY_DIR" ]; then
    mkdir -p "$MONTHLY_DIR"
    # Perform monthly backup
fi
```

## Recovery Procedures

### Complete System Recovery

```bash
#!/bin/bash
# recovery-full.sh

RESTORE_DATE="$1"  # Pass date as argument
if [ -z "$RESTORE_DATE" ]; then
    echo "Usage: $0 YYYY-MM-DD"
    exit 1
fi

BACKUP_DIR="/backup/melodee/$RESTORE_DATE"
if [ ! -d "$BACKUP_DIR" ]; then
    echo "Backup directory not found: $BACKUP_DIR"
    exit 1
fi

echo "Starting full recovery from: $RESTORE_DATE"

# Stop services
echo "Stopping Melodee services..."
docker-compose down

# Restore database first
echo "Restoring database..."
docker exec -i melodee-db psql -U melodeeuser -d melodeedb < "$BACKUP_DIR/db_dump.sql"

# Restore volumes
echo "Restoring volumes..."
docker run --rm -v melodee_storage:/volume -v "$BACKUP_DIR:/backup" alpine sh -c "rm -rf /volume/* && tar xzf /backup/storage.tar.gz -C /volume"
docker run --rm -v melodee_user_images:/volume -v "$BACKUP_DIR:/backup" alpine sh -c "rm -rf /volume/* && tar xzf /backup/user_images.tar.gz -C /volume"
docker run --rm -v melodee_playlists:/volume -v "$BACKUP_DIR:/backup" alpine sh -c "rm -rf /volume/* && tar xzf /backup/playlists.tar.gz -C /volume"

# Start services
echo "Starting Melodee services..."
docker-compose up -d

echo "Full recovery completed. Verify system status with: docker-compose ps"
```

### Database-Only Recovery

```bash
#!/bin/bash
# recovery-db.sh

BACKUP_FILE="$1"
if [ ! -f "$BACKUP_FILE" ]; then
    echo "Backup file not found: $BACKUP_FILE"
    exit 1
fi

echo "Restoring database from: $BACKUP_FILE"

# Stop Melodee to prevent database access
docker-compose stop melodee.blazor

# Drop and recreate database
docker exec melodee-db dropdb -U melodeeuser melodeedb
docker exec melodee-db createdb -U melodeeuser melodeedb

# Restore from backup
if [[ "$BACKUP_FILE" == *.gz ]]; then
    gunzip -c "$BACKUP_FILE" | docker exec -i melodee-db psql -U melodeeuser -d melodeedb
elif [[ "$BACKUP_FILE" == *.custom ]]; then
    echo "Custom format backup detected, using pg_restore"
    gunzip -c "$BACKUP_FILE" | docker exec -i melodee-db pg_restore -U melodeeuser -d melodeedb --clean --if-exists
else
    docker exec -i melodee-db psql -U melodeeuser -d melodeedb < "$BACKUP_FILE"
fi

# Restart services
docker-compose start melodee.blazor

echo "Database recovery completed"
```

### Partial Recovery (Individual Volume)

```bash
#!/bin/bash
# recovery-volume.sh

VOLUME_NAME="$1"  # e.g., "storage", "user_images", "playlists"
BACKUP_FILE="$2"  # Path to specific volume backup

if [ -z "$VOLUME_NAME" ] || [ -z "$BACKUP_FILE" ]; then
    echo "Usage: $0 <volume_name> <backup_file>"
    echo "Volume names: storage, user_images, playlists"
    exit 1
fi

VOLUME_MAP="melodee_$VOLUME_NAME"

echo "Restoring volume $VOLUME_NAME from: $BACKUP_FILE"

# Stop services that might be using the volume
docker-compose stop melodee.blazor

# Clear and restore the specific volume
docker run --rm -v "$VOLUME_MAP:/volume" -v "$(dirname "$BACKUP_FILE"):/backup" alpine sh -c "rm -rf /volume/* && tar xzf /backup/$(basename "$BACKUP_FILE") -C /volume"

# Start services
docker-compose start melodee.blazor

echo "Volume $VOLUME_NAME recovery completed"
```

## Backup Verification

### Automated Verification Script

```bash
#!/bin/bash
# verify-backup.sh

BACKUP_DIR="$1"
if [ -z "$BACKUP_DIR" ]; then
    echo "Usage: $0 <backup_directory>"
    exit 1
fi

echo "Verifying backup in: $BACKUP_DIR"

# Check if required backup files exist
REQUIRED_FILES=("db_dump.sql" "storage.tar.gz" "user_images.tar.gz" "playlists.tar.gz")
for file in "${REQUIRED_FILES[@]}"; do
    if [ ! -f "$BACKUP_DIR/$file" ]; then
        echo "ERROR: Missing backup file: $file"
        exit 1
    else
        echo "OK: Found $file ($(du -h "$BACKUP_DIR/$file" | cut -f1))"
    fi
done

# Verify database dump integrity
echo "Checking database dump integrity..."
if head -n 5 "$BACKUP_DIR/db_dump.sql" | grep -q "PostgreSQL"; then
    echo "OK: Database dump appears valid"
else
    echo "WARNING: Database dump may be corrupted"
fi

# Check volume archives
for vol in storage user_images playlists; do
    if tar -tzf "$BACKUP_DIR/$vol.tar.gz" >/dev/null 2>&1; then
        echo "OK: $vol archive is valid"
    else
        echo "ERROR: $vol archive is corrupted"
    fi
done

echo "Backup verification completed"
```

### Integrity Testing

Regularly test recovery procedures:

```bash
#!/bin/bash
# test-recovery.sh

# Create a test environment
TEST_DIR="/tmp/melodee-test-restore"
mkdir -p "$TEST_DIR"

# Use a recent backup for testing
RECENT_BACKUP=$(ls -td /backup/melodee/*/ | head -n1)
echo "Testing recovery from: $RECENT_BACKUP"

# Test database restore in isolation
echo "Testing database restore..."
# This would create a temporary database container and restore to it

# Test volume extraction
echo "Testing volume extraction..."
for vol in storage user_images playlists; do
    mkdir -p "$TEST_DIR/$vol"
    tar xzf "$RECENT_BACKUP/$vol.tar.gz" -C "$TEST_DIR/$vol" --exclude="*.tmp" --exclude="*.tmp/*"
    if [ $? -eq 0 ]; then
        echo "OK: $vol extraction successful"
    else
        echo "ERROR: $vol extraction failed"
    fi
done

# Cleanup
rm -rf "$TEST_DIR"

echo "Recovery test completed"
```

## Disaster Recovery Plan

### Immediate Response Steps

1. **Assess the situation**: Determine what data is lost or corrupted
2. **Stop services**: Prevent further writes to corrupted data
3. **Identify recovery point**: Choose the most recent good backup
4. **Prepare recovery environment**: Ensure backup files are accessible
5. **Execute recovery**: Follow appropriate recovery procedure
6. **Verify operation**: Test that Melodee functions correctly

### Recovery Scenarios

**Database Corruption:**
- Restore database from backup
- Verify music files remain intact
- Re-index if necessary

**Volume Loss:**
- Restore specific volume from backup
- Restart affected services
- Verify file integrity

**Complete System Failure:**
- Set up new system with same configuration
- Restore all components from backup
- Verify all functionality

## Best Practices

### Backup Best Practices
- Test recovery procedures regularly
- Keep multiple backup copies in different locations
- Monitor backup success/failure
- Document your specific backup procedures
- Encrypt sensitive backup data
- Use compression to save space

### Recovery Best Practices
- Maintain a recovery checklist
- Document your specific system configuration
- Keep recovery scripts tested and current
- Have a backup of your backup scripts
- Document any custom configurations

## Monitoring Backup Health

### Backup Monitoring Script

```bash
#!/bin/bash
# monitor-backup.sh

BACKUP_ROOT="/backup/melodee"
ALERT_EMAIL="admin@yourdomain.com"
MAX_AGE_DAYS=2

# Check if recent backup exists
RECENT_BACKUP=$(find "$BACKUP_ROOT" -mindepth 1 -maxdepth 1 -type d -mtime -$MAX_AGE_DAYS | sort | tail -n1)

if [ -z "$RECENT_BACKUP" ]; then
    echo "ALERT: No recent backup found (older than $MAX_AGE_DAYS days)"
    # Send notification (email, push, etc.)
    exit 1
fi

# Check backup size (should be reasonable)
DB_SIZE=$(du -sm "$RECENT_BACKUP/db_dump.sql" 2>/dev/null | cut -f1)
if [ -z "$DB_SIZE" ] || [ "$DB_SIZE" -lt 1 ]; then
    echo "ALERT: Database backup is too small ($DB_SIZE MB)"
    exit 1
fi

echo "Backup monitoring: OK - Recent backup found from $(basename "$RECENT_BACKUP")"
```

This comprehensive backup and recovery guide ensures your Melodee installation remains protected against data loss. Regular testing of recovery procedures is crucial to ensure they work when needed.