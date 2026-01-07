# Demo Server Reset Scripts

This directory contains scripts for managing the Melodee demo server at https://demo.melodee.org

## Overview

The demo server resets every 24 hours to provide a clean experience for new users. These scripts automate:

1. **Deleting all non-admin users** and their associated data
2. **Creating a fresh "demo" user** with password "melodee"
3. **Resetting statistics** (play counts, etc.)

## Scripts

### 1. `reset-demo-server.sh` (Main Reset Script)

Deletes all non-admin users and their data from the PostgreSQL database.

**Usage:**
```bash
./reset-demo-server.sh [OPTIONS]
```

**Options:**
- `--dry-run` - Show what would be deleted without actually deleting
- `--keep-library` - Don't delete user library data (not currently used)
- `--connection-string STRING` - PostgreSQL connection string
- `--help` - Show help message

**Environment Variables:**
- `MELODEE_CONNECTION_STRING` - PostgreSQL connection string (required)
- `MELODEE_ENCRYPTION_KEY` - Encryption key for password storage (optional, has default)

**Example:**
```bash
export MELODEE_CONNECTION_STRING="Host=localhost;Port=5432;Database=melodee;Username=melodee;Password=melodee"
./reset-demo-server.sh
```

**Dry Run (recommended first run):**
```bash
./reset-demo-server.sh --dry-run
```

### 2. `create-demo-user.py` (User Creation Script)

Python script that creates a demo user with properly encrypted credentials.

**Prerequisites:**
```bash
pip3 install psycopg2-binary cryptography
```

**Usage:**
```bash
python3 create-demo-user.py [--connection-string STRING]
```

**Environment Variables:**
- `MELODEE_CONNECTION_STRING` - PostgreSQL connection string (required)
- `MELODEE_ENCRYPTION_KEY` - Encryption key (optional, has default)

**Note:** This script is automatically called by `reset-demo-server.sh` if Python3 is available.

### 3. `set-demo-password.sh` (Deprecated)

Legacy script for setting demo password via CLI. Not currently functional as CLI lacks user commands.

## Cron Job Setup (Recommended)

To run the reset daily at midnight UTC:

1. **Create a wrapper script** with environment variables:

```bash
#!/bin/bash
# /opt/melodee/reset-demo-wrapper.sh

export MELODEE_CONNECTION_STRING="Host=localhost;Port=5432;Database=melodee;Username=melodee;Password=YOUR_PASSWORD"
export MELODEE_ENCRYPTION_KEY="YOUR_ENCRYPTION_KEY"

/path/to/melodee/scripts/reset-demo-server.sh >> /var/log/melodee-reset.log 2>&1
```

2. **Make it executable:**
```bash
chmod +x /opt/melodee/reset-demo-wrapper.sh
```

3. **Add to crontab:**
```bash
crontab -e
```

Add this line:
```cron
0 0 * * * /opt/melodee/reset-demo-wrapper.sh
```

## What Gets Deleted

The reset script removes ALL data for non-admin users:

### User Data
- User accounts (except admins)
- User profiles and settings
- User social logins

### Activity Data
- Play history
- Search history
- Bookmarks
- Play queues

### User Content
- Playlists (user-created)
- Shares
- Pins
- Comments on requests

### User Preferences
- Playback settings
- Equalizer presets
- API tokens (Jellyfin, OAuth, etc.)

### Statistics Reset
- Song play counts → 0
- Album play counts → 0
- Artist play counts → 0

## What Gets Preserved

- **Admin users** and their data
- **Library content** (artists, albums, songs)
- **System settings**
- **Charts** and chart data
- **Library scan history**
- **Job history**
- **System configuration**

## Demo User Credentials

After reset, the demo server has:

- **Username:** `demo`
- **Password:** `Mel0deeR0cks!`
- **Email:** `demo@melodee.org`

### Demo User Permissions

The demo user has:
- ✅ Settings role
- ✅ Download role
- ✅ Playlist role
- ✅ Cover art role
- ✅ Comment role
- ✅ Podcast role
- ✅ Stream role
- ✅ Jukebox role
- ✅ Share role
- ❌ Upload role (disabled for security)
- ❌ Admin role (disabled)
- ❌ Editor role (disabled)

## Security Considerations

1. **Never commit credentials** - Use environment variables or secure secret management
2. **Restrict script access** - Only root or melodee user should run these scripts
3. **Log rotation** - Set up log rotation for `/var/log/melodee-reset.log`
4. **Monitor execution** - Set up alerts if reset fails
5. **Backup before reset** - Consider backing up database before automated resets

## Troubleshooting

### "PostgreSQL connection failed"

- Verify connection string is correct
- Check PostgreSQL is running: `systemctl status postgresql`
- Verify user has permissions: `psql -h localhost -U melodee -d melodee`

### "Python script failed"

- Install dependencies: `pip3 install psycopg2-binary cryptography`
- Check Python version: `python3 --version` (requires 3.6+)

### "Demo user can't login"

- Verify user was created: `psql -d melodee -c "SELECT * FROM \"Users\" WHERE \"UserNameNormalized\" = 'DEMO';"`
- Check encryption key matches application setting
- Run create-demo-user.py manually to recreate

### "Dry run shows unexpected deletes"

- This is normal - dry run shows what WOULD be deleted
- Review the list carefully before running without `--dry-run`

## Development and Testing

**Test locally:**
```bash
# 1. Dry run first
./reset-demo-server.sh --dry-run

# 2. Run actual reset
./reset-demo-server.sh

# 3. Verify demo user works
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@melodee.org","password":"melodee"}'
```

**Manual user creation:**
```bash
python3 create-demo-user.py --connection-string "Host=localhost;Port=5432;Database=melodee;Username=melodee;Password=melodee"
```

## Files

- `reset-demo-server.sh` - Main bash script (314 lines)
- `create-demo-user.py` - Python user creator (236 lines)
- `set-demo-password.sh` - Deprecated CLI wrapper
- `DEMO-SERVER-RESET.md` - This file

## Support

For issues or questions:
- GitHub Issues: https://github.com/sphildreth/melodee/issues
- Documentation: https://melodee.org/docs
- Demo Server: https://demo.melodee.org

---

**Last Updated:** 2026-01-06
**Melodee Version:** 1.0.0+
