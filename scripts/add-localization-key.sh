#!/bin/bash

# Helper script to add a new localization key to ALL resource files
# This ensures all language files are updated together to prevent CI/CD failures

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}================================================${NC}"
echo -e "${BLUE} Add Localization Key to All Language Files${NC}"
echo -e "${BLUE}================================================${NC}"
echo ""

# Check if key name is provided
if [ -z "$1" ]; then
    echo -e "${RED}Error: Key name is required${NC}"
    echo ""
    echo "Usage: $0 <key.name> <english-value>"
    echo ""
    echo "Example:"
    echo "  $0 \"Actions.Export\" \"Export\""
    echo "  $0 \"Messages.ExportSuccess\" \"Successfully exported {0} items\""
    echo ""
    exit 1
fi

# Check if English value is provided
if [ -z "$2" ]; then
    echo -e "${RED}Error: English value is required${NC}"
    echo ""
    echo "Usage: $0 <key.name> <english-value>"
    echo ""
    exit 1
fi

KEY_NAME="$1"
ENGLISH_VALUE="$2"
RESOURCE_DIR="src/Melodee.Blazor/Resources"

# Escape special XML characters in value
ESCAPED_VALUE=$(echo "$ENGLISH_VALUE" | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g; s/"/\&quot;/g')

echo -e "${YELLOW}Key name:${NC} $KEY_NAME"
echo -e "${YELLOW}English value:${NC} $ENGLISH_VALUE"
echo ""

# Create XML entry
read -r -d '' ENTRY << EOF || true
  <data name="$KEY_NAME" xml:space="preserve">
    <value>$ESCAPED_VALUE</value>
  </data>
EOF

# Function to add entry to a file
add_to_file() {
    local file=$1
    local lang=$2
    local value=$3
    
    # Check if key already exists
    if grep -q "name=\"$KEY_NAME\"" "$file"; then
        echo -e "${YELLOW}  ⚠ Key already exists in $lang, skipping${NC}"
        return
    fi
    
    # Find position before </root>
    if ! grep -q "</root>" "$file"; then
        echo -e "${RED}  ✗ Error: Could not find </root> in $file${NC}"
        return 1
    fi
    
    # Create entry with appropriate value
    local entry
    if [ "$lang" = "en-US" ]; then
        entry="$ENTRY"
    else
        # For non-English, add [NEEDS TRANSLATION] prefix
        local prefixed_value="[NEEDS TRANSLATION] $ESCAPED_VALUE"
        entry=$(cat << ENTRYEOF
  <data name="$KEY_NAME" xml:space="preserve">
    <value>$prefixed_value</value>
  </data>
ENTRYEOF
)
    fi
    
    # Add entry before </root>
    local temp_file=$(mktemp)
    awk -v entry="$entry" '
        /<\/root>/ { print entry; }
        { print; }
    ' "$file" > "$temp_file"
    
    mv "$temp_file" "$file"
    echo -e "${GREEN}  ✓ Added to $lang${NC}"
}

# Add to base file (en-US)
BASE_FILE="$RESOURCE_DIR/SharedResources.resx"
echo "Adding to base file (en-US)..."
add_to_file "$BASE_FILE" "en-US" "$ENGLISH_VALUE"

# Add to all translation files
LANGUAGES=("de-DE" "es-ES" "fr-FR" "it-IT" "ja-JP" "pt-BR" "ru-RU" "zh-CN" "ar-SA" "nl-NL" "pl-PL" "tr-TR" "id-ID" "ko-KR" "vi-VN" "fa-IR" "uk-UA" "cs-CZ" "sv-SE")

echo ""
echo "Adding to translation files with [NEEDS TRANSLATION] prefix..."
for LANG in "${LANGUAGES[@]}"; do
    LANG_FILE="$RESOURCE_DIR/SharedResources.$LANG.resx"
    if [ -f "$LANG_FILE" ]; then
        add_to_file "$LANG_FILE" "$LANG" "[NEEDS TRANSLATION] $ENGLISH_VALUE"
    else
        echo -e "${RED}  ✗ File not found: $LANG_FILE${NC}"
    fi
done

echo ""
echo -e "${BLUE}================================================${NC}"
echo -e "${GREEN}✓ Key added to all 20 language files!${NC}"
echo -e "${BLUE}================================================${NC}"
echo ""
echo "Next steps:"
echo "1. Review the changes in your editor"
echo "2. Replace [NEEDS TRANSLATION] with proper translations if you know them"
echo "3. Run validation: bash scripts/validate-resources.sh"
echo "4. Commit all 20 resource files together"
echo ""
echo -e "${YELLOW}Note: Translations marked with [NEEDS TRANSLATION] should be updated${NC}"
echo -e "${YELLOW}by native speakers before release.${NC}"
