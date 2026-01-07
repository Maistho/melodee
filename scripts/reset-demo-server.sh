#!/usr/bin/env bash
set -euo pipefail

################################################################################
# Demo Server Reset Script
#
# This script resets the Melodee demo server by:
# 1. Deleting all non-admin users and their associated data
# 2. Creating a fresh "demo" user with password "melodee"
# 3. Cleaning up user-specific data (playlists, play history, etc.)
#
# Usage:
#   ./reset-demo-server.sh [OPTIONS]
#
# Options:
#   --dry-run          Show what would be deleted without actually deleting
#   --keep-library     Don't delete user library data (albums, songs, etc.)
#   --connection-string  PostgreSQL connection string (defaults to env var)
#
# Environment Variables:
#   MELODEE_CONNECTION_STRING  PostgreSQL connection string
#   MELODEE_ENCRYPTION_KEY     Encryption key for password storage
#
# Intended for cron job on demo server:
#   0 0 * * * /path/to/reset-demo-server.sh >> /var/log/melodee-reset.log 2>&1
################################################################################

# Color output for terminal
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
DRY_RUN=false
KEEP_LIBRARY=false
CONNECTION_STRING="${MELODEE_CONNECTION_STRING:-}"
ENCRYPTION_KEY="${MELODEE_ENCRYPTION_KEY:-H+Kiik6VMKfTD2MesF1GoMjczTrD5RhuKckJ5+/UQWOdWajGcsEC3yEnlJ5eoy8Y}"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        --keep-library)
            KEEP_LIBRARY=true
            shift
            ;;
        --connection-string)
            CONNECTION_STRING="$2"
            shift 2
            ;;
        -h|--help)
            grep "^#" "$0" | grep -v "#!/usr/bin/env bash" | sed 's/^# //' | sed 's/^#//'
            exit 0
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            exit 1
            ;;
    esac
done

# Validate prerequisites
if [ -z "$CONNECTION_STRING" ]; then
    echo -e "${RED}Error: PostgreSQL connection string not provided${NC}"
    echo "Set MELODEE_CONNECTION_STRING environment variable or use --connection-string option"
    exit 1
fi

if ! command -v psql &> /dev/null; then
    echo -e "${RED}Error: psql command not found. Please install PostgreSQL client.${NC}"
    exit 1
fi

# Extract database connection details from connection string
# Format: Host=localhost;Port=5432;Database=melodee;Username=melodee;Password=melodee
DB_HOST=$(echo "$CONNECTION_STRING" | grep -oP 'Host=\K[^;]+' || echo "localhost")
DB_PORT=$(echo "$CONNECTION_STRING" | grep -oP 'Port=\K[^;]+' || echo "5432")
DB_NAME=$(echo "$CONNECTION_STRING" | grep -oP 'Database=\K[^;]+' || echo "melodee")
DB_USER=$(echo "$CONNECTION_STRING" | grep -oP 'Username=\K[^;]+' || echo "melodee")
DB_PASS=$(echo "$CONNECTION_STRING" | grep -oP 'Password=\K[^;]+' || echo "melodee")

# Set PostgreSQL password for psql commands
export PGPASSWORD="$DB_PASS"

echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Melodee Demo Server Reset Script                         ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${YELLOW}Configuration:${NC}"
echo "  Database: $DB_NAME @ $DB_HOST:$DB_PORT"
echo "  User: $DB_USER"
echo "  Dry Run: $DRY_RUN"
echo "  Keep Library: $KEEP_LIBRARY"
echo ""

# Function to execute SQL with dry-run support
execute_sql() {
    local sql="$1"
    local description="$2"
    
    if [ "$DRY_RUN" = true ]; then
        echo -e "${YELLOW}[DRY RUN]${NC} $description"
        echo "  SQL: $sql"
    else
        echo -e "${GREEN}[EXECUTE]${NC} $description"
        psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "$sql" -q
    fi
}

# Function to get count from SQL
get_count() {
    local sql="$1"
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -t -c "$sql" | tr -d ' '
}

echo -e "${BLUE}Step 1: Analyzing current database state${NC}"
echo "─────────────────────────────────────────────────────────────"

TOTAL_USERS=$(get_count "SELECT COUNT(*) FROM \"Users\";")
ADMIN_USERS=$(get_count "SELECT COUNT(*) FROM \"Users\" WHERE \"IsAdmin\" = true;")
NON_ADMIN_USERS=$(get_count "SELECT COUNT(*) FROM \"Users\" WHERE \"IsAdmin\" = false;")
DEMO_USER_EXISTS=$(get_count "SELECT COUNT(*) FROM \"Users\" WHERE \"UserNameNormalized\" = 'DEMO';")

echo "  Total users: $TOTAL_USERS"
echo "  Admin users: $ADMIN_USERS"
echo "  Non-admin users: $NON_ADMIN_USERS"
echo "  Demo user exists: $DEMO_USER_EXISTS"
echo ""

if [ "$NON_ADMIN_USERS" -eq 0 ] && [ "$DEMO_USER_EXISTS" -eq 1 ]; then
    echo -e "${GREEN}✓ Database is already in clean state (only demo user exists)${NC}"
    exit 0
fi

echo -e "${BLUE}Step 2: Deleting non-admin user data${NC}"
echo "─────────────────────────────────────────────────────────────"

# Get non-admin user IDs for logging
if [ "$DRY_RUN" = false ]; then
    NON_ADMIN_USER_IDS=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -t -c \
        "SELECT STRING_AGG(\"Id\"::text, ', ') FROM \"Users\" WHERE \"IsAdmin\" = false;")
    if [ -n "$NON_ADMIN_USER_IDS" ]; then
        echo "  Deleting data for user IDs: $NON_ADMIN_USER_IDS"
    fi
fi

# Delete user-specific data (cascading deletes will handle most of this, but being explicit)
# The order matters due to foreign key constraints

# User activity and history
execute_sql "DELETE FROM \"UserSongPlayHistories\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user song play history"

execute_sql "DELETE FROM \"SearchHistories\" WHERE \"ByUserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete search history"

execute_sql "DELETE FROM \"JellyfinAccessTokens\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete Jellyfin access tokens"

execute_sql "DELETE FROM \"RefreshTokens\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete refresh tokens"

# User social logins
execute_sql "DELETE FROM \"UserSocialLogins\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user social logins"

# User preferences and settings
execute_sql "DELETE FROM \"UserPlaybackSettings\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user playback settings"

execute_sql "DELETE FROM \"UserEqualizerPresets\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user equalizer presets"

execute_sql "DELETE FROM \"UserPins\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user pins"

# Bookmarks and play queues
execute_sql "DELETE FROM \"Bookmarks\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user bookmarks"

execute_sql "DELETE FROM \"PlayQues\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user play queues"

# Playlists and playlist songs (cascade will handle PlaylistSong)
execute_sql "DELETE FROM \"Playlists\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user playlists"

# Players
execute_sql "DELETE FROM \"Players\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user players"

# Shares and share activity
execute_sql "DELETE FROM \"Shares\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user shares"

# User-specific album, artist, song data
execute_sql "DELETE FROM \"UserAlbums\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user album data"

execute_sql "DELETE FROM \"UserArtists\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user artist data"

execute_sql "DELETE FROM \"UserSongs\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete user song data"

# Request system - only delete if user is involved
execute_sql "DELETE FROM \"RequestParticipants\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete request participants"

execute_sql "DELETE FROM \"RequestUserStates\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete request user states"

# Delete comments created by non-admin users (but keep the request itself if created by admin)
execute_sql "DELETE FROM \"RequestComments\" WHERE \"CreatedByUserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete request comments from non-admin users"

# Delete requests created by non-admin users
execute_sql "DELETE FROM \"Requests\" WHERE \"CreatedByUserId\" IN (SELECT \"Id\" FROM \"Users\" WHERE \"IsAdmin\" = false);" \
    "Delete requests from non-admin users"

echo ""
echo -e "${BLUE}Step 3: Deleting non-admin users${NC}"
echo "─────────────────────────────────────────────────────────────"

execute_sql "DELETE FROM \"Users\" WHERE \"IsAdmin\" = false;" \
    "Delete all non-admin users"

echo ""
echo -e "${BLUE}Step 4: Creating demo user with proper credentials${NC}"
echo "─────────────────────────────────────────────────────────────"

if [ "$DRY_RUN" = false ]; then
    # Get the script directory
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    
    # Check if Python3 is available
    if command -v python3 &> /dev/null; then
        # Use Python script to create demo user with proper encryption
        export MELODEE_CONNECTION_STRING="$CONNECTION_STRING"
        export MELODEE_ENCRYPTION_KEY="${ENCRYPTION_KEY}"
        
        if python3 "$SCRIPT_DIR/create-demo-user.py"; then
            echo -e "${GREEN}✓ Demo user created with proper credentials${NC}"
        else
            echo -e "${YELLOW}⚠️  Failed to create demo user with Python script${NC}"
            echo "You may need to install dependencies: pip3 install psycopg2-binary cryptography"
        fi
    else
        echo -e "${YELLOW}⚠️  Python3 not found - demo user creation skipped${NC}"
        echo "Install Python3 and run: python3 $SCRIPT_DIR/create-demo-user.py"
    fi
else
    echo -e "${YELLOW}[DRY RUN]${NC} Would create demo user with username 'demo' and password 'Mel0deeR0cks!'"
fi

echo ""
echo -e "${BLUE}Step 5: Reset statistics and cleanup${NC}"
echo "─────────────────────────────────────────────────────────────"

# Reset play counts and statistics that may have been affected
execute_sql "UPDATE \"Songs\" SET \"PlayedCount\" = 0 WHERE \"PlayedCount\" > 0;" \
    "Reset song play counts"

execute_sql "UPDATE \"Albums\" SET \"PlayedCount\" = 0 WHERE \"PlayedCount\" > 0;" \
    "Reset album play counts"

execute_sql "UPDATE \"Artists\" SET \"PlayedCount\" = 0 WHERE \"PlayedCount\" > 0;" \
    "Reset artist play counts"

# Cleanup orphaned share activities
execute_sql "DELETE FROM \"ShareActivities\" WHERE \"UserId\" IS NULL;" \
    "Delete orphaned share activities"

echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"

if [ "$DRY_RUN" = true ]; then
    echo -e "${YELLOW}DRY RUN COMPLETE - No changes were made${NC}"
    echo "Run without --dry-run to execute the reset"
else
    echo -e "${GREEN}✓ Demo server reset complete!${NC}"
    echo ""
    echo "Summary:"
    echo "  • Deleted $NON_ADMIN_USERS non-admin users"
    echo "  • Removed all associated user data"
    echo "  • Demo user is ready (requires password reset)"
    echo ""
    echo "Next steps:"
    echo "  1. Set demo user password to 'melodee' via application"
    echo "  2. Verify demo server is accessible at https://demo.melodee.org"
    echo "  3. Test login with demo/melodee credentials"
fi

echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""

# Log completion
echo "Reset completed at $(date -u +"%Y-%m-%d %H:%M:%S UTC")"

# Unset password
unset PGPASSWORD

exit 0
