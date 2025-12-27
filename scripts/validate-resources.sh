#!/bin/bash

# Resource Validation Script for Melodee.Blazor Localization
# This script validates that all resource files have consistent keys across all languages

set -e

echo "================================================"
echo " Melodee.Blazor Localization Validation Script"
echo "================================================"
echo ""

RESOURCE_DIR="src/Melodee.Blazor/Resources"
BASE_FILE="$RESOURCE_DIR/SharedResources.resx"
LANGUAGES=("es-ES" "ru-RU" "zh-CN" "fr-FR" "ar-SA")

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if base file exists
if [ ! -f "$BASE_FILE" ]; then
    echo -e "${RED}ERROR: Base resource file not found: $BASE_FILE${NC}"
    exit 1
fi

echo "Base resource file: $BASE_FILE"
echo ""

# Extract keys from base file
echo "Extracting keys from base resource file..."
BASE_KEYS=$(grep -o '<data name="[^"]*"' "$BASE_FILE" | sed 's/<data name="\([^"]*\)"/\1/' | LC_ALL=C sort)
BASE_COUNT=$(echo "$BASE_KEYS" | wc -l)

echo -e "${GREEN}Found $BASE_COUNT keys in base file (en-US)${NC}"
echo ""

# Validate each language file
TOTAL_ERRORS=0
TOTAL_MISSING=0

for LANG in "${LANGUAGES[@]}"; do
    LANG_FILE="$RESOURCE_DIR/SharedResources.$LANG.resx"
    
    echo "Validating $LANG..."
    
    if [ ! -f "$LANG_FILE" ]; then
        echo -e "${RED}  ERROR: Language file not found: $LANG_FILE${NC}"
        ((TOTAL_ERRORS++))
        continue
    fi
    
    # Extract keys from language file
    LANG_KEYS=$(grep -o '<data name="[^"]*"' "$LANG_FILE" | sed 's/<data name="\([^"]*\)"/\1/' | LC_ALL=C sort)
    LANG_COUNT=$(echo "$LANG_KEYS" | wc -l)
    
    # Check if counts match
    if [ "$BASE_COUNT" -ne "$LANG_COUNT" ]; then
        echo -e "${YELLOW}  WARNING: Key count mismatch - Base: $BASE_COUNT, $LANG: $LANG_COUNT${NC}"
    else
        echo -e "${GREEN}  ✓ Key count matches: $LANG_COUNT${NC}"
    fi
    
    # Find missing keys
    MISSING_KEYS=$(comm -23 <(echo "$BASE_KEYS") <(echo "$LANG_KEYS"))
    
    if [ -n "$MISSING_KEYS" ]; then
        MISSING_COUNT=$(echo "$MISSING_KEYS" | wc -l)
        echo -e "${RED}  ERROR: $MISSING_COUNT missing keys in $LANG:${NC}"
        echo "$MISSING_KEYS" | while read -r key; do
            echo -e "${RED}    - $key${NC}"
            ((TOTAL_MISSING++))
        done
        ((TOTAL_ERRORS++))
    else
        echo -e "${GREEN}  ✓ All keys present${NC}"
    fi
    
    # Find extra keys
    EXTRA_KEYS=$(comm -13 <(echo "$BASE_KEYS") <(echo "$LANG_KEYS"))
    
    if [ -n "$EXTRA_KEYS" ]; then
        EXTRA_COUNT=$(echo "$EXTRA_KEYS" | wc -l)
        echo -e "${YELLOW}  WARNING: $EXTRA_COUNT extra keys in $LANG:${NC}"
        echo "$EXTRA_KEYS" | while read -r key; do
            echo -e "${YELLOW}    + $key${NC}"
        done
    fi
    
    echo ""
done

# Summary
echo "================================================"
echo " Validation Summary"
echo "================================================"
echo ""
echo "Total languages checked: ${#LANGUAGES[@]}"
echo "Base key count: $BASE_COUNT"

if [ "$TOTAL_ERRORS" -eq 0 ]; then
    echo -e "${GREEN}✓ All validations passed!${NC}"
    echo -e "${GREEN}✓ All language files have consistent keys.${NC}"
    exit 0
else
    echo -e "${RED}✗ Validation failed with $TOTAL_ERRORS error(s)${NC}"
    echo -e "${RED}✗ Total missing keys: $TOTAL_MISSING${NC}"
    exit 1
fi
