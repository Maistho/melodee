# Demo Server Reset - Quick Start

## TL;DR

```bash
# Install dependencies (one-time)
pip3 install psycopg2-binary cryptography

# Set credentials
export MELODEE_CONNECTION_STRING="Host=localhost;Port=5432;Database=melodee;Username=melodee;Password=YOUR_PASSWORD"

# Test with dry run
./scripts/reset-demo-server.sh --dry-run

# Run actual reset
./scripts/reset-demo-server.sh

# Result: All non-admin users deleted, fresh "demo" user created with password "Mel0deeR0cks!"
```

## What This Does

1. **Deletes** all non-admin users and their data (playlists, history, settings, etc.)
2. **Resets** play counts to 0
3. **Creates** demo user (username: `demo`, password: `melodee`)
4. **Preserves** library content and admin users

## Cron Setup (24-hour reset)

```bash
# 1. Create wrapper script with credentials
sudo nano /opt/melodee/reset-wrapper.sh
```

```bash
#!/bin/bash
export MELODEE_CONNECTION_STRING="Host=localhost;Port=5432;Database=melodee;Username=melodee;Password=YOUR_PASSWORD"
/path/to/melodee/scripts/reset-demo-server.sh >> /var/log/melodee-reset.log 2>&1
```

```bash
# 2. Make executable
sudo chmod +x /opt/melodee/reset-wrapper.sh

# 3. Add to crontab (runs daily at midnight UTC)
sudo crontab -e
# Add this line:
0 0 * * * /opt/melodee/reset-wrapper.sh
```

## Files

- `scripts/reset-demo-server.sh` - Main reset script
- `scripts/create-demo-user.py` - User creator (called by main script)
- `scripts/DEMO-SERVER-RESET.md` - Full documentation

## Demo Credentials

After reset:
- **URL:** https://demo.melodee.org
- **Username:** demo
- **Password:** Mel0deeR0cks!
- **Email:** demo@melodee.org

## Support

Full documentation: `scripts/DEMO-SERVER-RESET.md`
