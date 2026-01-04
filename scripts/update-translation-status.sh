#!/usr/bin/env bash
# Updates translation status percentages in README.md
# Run this after making significant translation changes

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
RESOURCES_DIR="$PROJECT_ROOT/src/Melodee.Blazor/Resources"

echo "📊 Calculating translation status..."
echo ""

last_needs=0
total_strings=0
for file in "$RESOURCES_DIR"/SharedResources*.resx; do
    filename=$(basename "$file")
    if [[ "$filename" == "SharedResources.resx" ]]; then
        lang="en-US"
    else
        lang=$(echo "$filename" | sed 's/SharedResources\.//' | sed 's/\.resx//')
    fi
    
    total=$(grep -c '<data name=' "$file" 2>/dev/null || echo "0")
    total="${total//[^0-9]/}"  # Strip non-numeric characters
    total_strings=$total
    
    needs=$(grep -c '\[NEEDS TRANSLATION\]' "$file" 2>/dev/null || echo "0")
    needs="${needs//[^0-9]/}"  # Strip non-numeric characters
    
    if [[ "$total" -gt 0 ]]; then
        pct=$(( ((total - needs) * 100) / total ))
    else
        pct=0
    fi
    
    if [[ "$needs" -eq 0 ]]; then
        echo "  $lang: ✅ 100%"
    else
        echo "  $lang: 🔄 $pct% ($needs strings need translation)"
        last_needs=$needs
    fi
done

echo ""
echo "📝 Total strings per language: $total_strings"
echo "📝 Strings needing translation: ~$last_needs per non-English language"
echo ""
echo "To contribute translations:"
echo "  1. Edit src/Melodee.Blazor/Resources/SharedResources.<lang>.resx"
echo "  2. Search for [NEEDS TRANSLATION] entries"
echo "  3. Replace with native translations"
echo "  4. Submit a pull request"
