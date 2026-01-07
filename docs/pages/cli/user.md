---
title: CLI - User Commands
permalink: /cli/user/
layout: page
---

# User Commands

The `user` branch provides commands for managing user accounts, including creating, listing, and deleting users from the command line.

## Overview

```bash
mcli user [COMMAND] [OPTIONS]
```

**Available Commands:**

| Command | Description |
|---------|-------------|
| `create` | Create a new user account |
| `delete` | Delete a user from the database |
| `list` | List all users in the system |

---

## user create

Creates a new user account in the system.

### Usage

```bash
mcli user create --username <USERNAME> --email <EMAIL> --password <PASSWORD> [OPTIONS]
```

### Options

| Option | Alias | Required | Default | Description |
|--------|-------|----------|---------|-------------|
| `--username` | `-u` | Yes | | Username for the new account |
| `--email` | `-e` | Yes | | Email address for the new account |
| `--password` | `-p` | Yes | | Password (minimum 8 characters) |
| `--force` | `-f` | No | `false` | Delete existing user with same username or email before creating |
| `--verbose` | | No | `false` | Output verbose debug and timing results |

### Examples

```bash
# Create a new user
./mcli user create --username "john" --email "john@example.com" --password "SecurePass123!"

# Create a user with short options
./mcli user create -u "john" -e "john@example.com" -p "SecurePass123!"

# Force recreate an existing user (deletes existing user first)
./mcli user create -u "demo" -e "demo@melodee.org" -p "Mel0deeR0cks!" --force
```

### Output

```
✓ User 'john' created successfully.
```

### Error Output

```
✗ Failed to create user: User already exists. Use --force to replace.
```

### Docker/Podman Usage

When running in a container, use `exec` to run the CLI:

```bash
# Docker
docker compose exec melodee.blazor /app/cli/mcli user create \
    --username "demo" \
    --email "demo@melodee.org" \
    --password "Mel0deeR0cks!"

# Podman
podman compose exec melodee.blazor /app/cli/mcli user create \
    --username "demo" \
    --email "demo@melodee.org" \
    --password "Mel0deeR0cks!"
```

### Notes

- The `--force` option will delete any existing user with the same username or email address before creating the new user
- Passwords must be at least 8 characters long
- User accounts created via CLI have full administrator privileges

---

## user list

Lists all user accounts in the system.

### Usage

```bash
mcli user list [OPTIONS]
```

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `-n`, `--limit` | | `50` | Maximum number of users to return |
| `--raw` | | `false` | Output results in JSON format |
| `--verbose` | | `false` | Output verbose debug and timing results |

### Examples

```bash
# List first 50 users
./mcli user list

# List up to 100 users
./mcli user list -n 100

# Output as JSON for scripting
./mcli user list --raw
```

### Output

```
╭──────┬──────────────┬───────────────────────┬─────────────────┬───────────╮
│   ID │ Username     │ Email                 │ Created         │  Status   │
├──────┼──────────────┼───────────────────────┼─────────────────┼───────────┤
│    1 │ admin        │ admin@example.com     │ 20240115T103000 │     ✓     │
│    2 │ demo         │ demo@melodee.org      │ 20241230T142300 │     ✓     │
│    3 │ john         │ john@example.com      │ 20241230T150000 │ 🔒 Locked │
╰──────┴──────────────┴───────────────────────┴─────────────────┴───────────╯

Showing 3 of 3 users
```

### JSON Output

```json
[
  {
    "Id": 1,
    "ApiKey": "a1b2c3d4-...",
    "UserName": "admin",
    "Email": "admin@example.com",
    "IsLocked": false,
    "CreatedAt": "2024-01-15T10:30:00Z",
    "LastLoginAt": "2024-12-30T08:15:00Z",
    "LastActivityAt": "2024-12-30T14:23:00Z"
  }
]
```

---

## user delete

Deletes a user account from the system.

### Usage

```bash
mcli user delete <ID> [OPTIONS]
```

### Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `ID` | Yes | User ID to delete |

### Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `-y`, `--yes` | | `false` | Skip confirmation prompt |
| `--verbose` | | `false` | Output verbose debug and timing results |

### Examples

```bash
# Delete user (with confirmation)
./mcli user delete 42

# Delete without confirmation (scripting)
./mcli user delete 42 -y
```

### Output

```
User: john
Email: john@example.com
Created: 2024-12-30T15:00:00Z

Delete user 'john'? [y/n] (n): y

✓ User 'john' deleted successfully.
```

### Safety Notes

- ⚠️ **Locked users cannot be deleted.** Unlock the user first through the web interface.
- ⚠️ **This is a destructive operation.** The command shows details and requires confirmation by default.
- ⚠️ **User data associations** (stars, ratings, playlists) may be affected by deletion.

---

## Workflow Examples

### Setting Up a Demo User

```bash
# Create or replace a demo user
./mcli user create \
    --username "demo" \
    --email "demo@melodee.org" \
    --password "Mel0deeR0cks!" \
    --force
```

### Listing Users for Automation

```bash
#!/bin/bash
# Get user count for monitoring
USER_COUNT=$(./mcli user list --raw | jq 'length')
echo "Total users: $USER_COUNT"
```

### Bulk User Management Script

```bash
#!/bin/bash
# Create multiple users from a file
# users.txt format: username,email,password

while IFS=',' read -r username email password; do
    ./mcli user create -u "$username" -e "$email" -p "$password"
done < users.txt
```

---

## See Also

- [CLI Overview](/cli/) - Main CLI documentation
- [Configuration Commands](/cli/configuration/) - Manage settings
- [Library Commands](/cli/library/) - Library operations
