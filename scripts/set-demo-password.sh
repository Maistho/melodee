#!/usr/bin/env bash
set -euo pipefail

################################################################################
# Demo User Password Reset Script
#
# This script uses the Melodee CLI to properly set the demo user password
# with correct encryption that matches the application's expectations.
#
# Usage:
#   ./set-demo-password.sh
#
# This script should be run AFTER reset-demo-server.sh to finalize the demo user setup.
################################################################################

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Color output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Setting Demo User Password                               ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Check if we need to build first
if [ ! -f "$PROJECT_ROOT/src/Melodee.Cli/bin/Release/net10.0/Melodee.Cli.dll" ]; then
    echo -e "${YELLOW}Building Melodee CLI...${NC}"
    dotnet build "$PROJECT_ROOT/src/Melodee.Cli/Melodee.Cli.csproj" -c Release
    echo ""
fi

echo -e "${BLUE}Creating demo user with username 'demo' and password 'Mel0deeR0cks!'${NC}"

# Use the CLI to create/reset demo user
# The RegisterAsync method in UserService will handle:
# - Generating proper PublicKey
# - Encrypting password correctly
# - Setting default permissions

dotnet run --project "$PROJECT_ROOT/src/Melodee.Cli/Melodee.Cli.csproj" -- \
    user create \
    --username "demo" \
    --email "demo@melodee.org" \
    --password "Mel0deeR0cks!" \
    --force

echo ""
echo -e "${GREEN}✓ Demo user password set successfully!${NC}"
echo ""
echo "Demo credentials:"
echo "  Username: demo"
echo "  Password: Mel0deeR0cks!"
echo "  Email: demo@melodee.org"
echo ""
