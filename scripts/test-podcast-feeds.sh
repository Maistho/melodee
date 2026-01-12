#!/usr/bin/env bash
# Test podcast RSS feeds for XML parsing issues

set -euo pipefail

# Test feeds from requirements document
declare -a feeds=(
    "https://atp.fm/episodes?format=rss|Accidental Tech Podcast"
    "https://www.maximumfun.org/podcasts/adventure-zone/rss|Adventure Zone"
    "https://rss.libsyn.com/shows/332183/destinations/2705240.xml|Brave Technologist"
    "https://www.codenewbie.org/podcast/feed|CodeNewbie"
    "https://www.redhat.com/en/command-line-heroes-podcast/feed|Command Line Heroes"
    "https://www.maximumfun.org/podcasts/dark-matter/rss|Dark Matter"
    "https://darknetdiaries.com/feed.xml|Darknet Diaries"
    "https://feeds.megaphone.fm/dissect|Dissect"
    "https://feeds.npr.org/510019/podcast.xml|NPR All Songs Considered"
    "https://feeds.npr.org/510289/podcast.xml|NPR Planet Money"
    "https://feeds.npr.org/510298/podcast.xml|NPR TED Radio Hour"
    "https://feeds.simplecast.com/W1rB_kgL|Popcast (NYT)"
    "https://feeds.megaphone.fm/rollingstonemusicnow|Rolling Stone Music Now"
    "https://feeds.megaphone.fm/sciencevs|Science Vs"
    "https://softwareengineeringdaily.com/feed/podcast/|Software Engineering Daily"
    "https://feeds.megaphone.fm/switchedonpop|Switched on Pop"
    "https://feed.syntax.fm/rss|Syntax.fm"
    "https://feeds.feedburner.com/TEDTalks_audio|TED Talks Daily"
    "https://talkpython.fm/episodes/rss|Talk Python"
    "https://changelog.com/podcast/feed|The Changelog"
    "https://feeds.simplecast.com/54nAGcIl|The Daily (NYT)"
)

echo "================================================"
echo " Podcast Feed XML Validation Test"
echo "================================================"
echo ""
echo "Testing ${#feeds[@]} feeds for XML parsing issues..."
echo ""

declare -i has_dtd=0
declare -i has_external_entities=0
declare -i has_encoding_issues=0
declare -i successful_parses=0
declare -i failed_parses=0
declare -i timeout_count=0

for feed_info in "${feeds[@]}"; do
    IFS='|' read -r url name <<< "$feed_info"
    
    echo -n "Testing: $name ... "
    
    # Fetch feed with timeout
    temp_file=$(mktemp)
    if ! curl -sS -L --max-time 10 --max-redirs 5 -o "$temp_file" "$url" 2>/dev/null; then
        echo "❌ TIMEOUT/FETCH FAILED"
        timeout_count=$((timeout_count + 1))
        rm -f "$temp_file"
        continue
    fi
    
    # Check for DTD declaration
    if grep -q '<!DOCTYPE' "$temp_file" 2>/dev/null; then
        echo "⚠️  HAS DTD"
        has_dtd=$((has_dtd + 1))
    # Check for external entities
    elif grep -qE '<!ENTITY|<!ELEMENT|<!ATTLIST' "$temp_file" 2>/dev/null; then
        echo "⚠️  HAS EXTERNAL ENTITIES"
        has_external_entities=$((has_external_entities + 1))
    # Try to parse with xmllint
    elif xmllint --noout "$temp_file" 2>/dev/null; then
        echo "✅ OK"
        successful_parses=$((successful_parses + 1))
    else
        echo "❌ PARSE ERROR"
        failed_parses=$((failed_parses + 1))
        # Show first error line
        xmllint --noout "$temp_file" 2>&1 | head -1
    fi
    
    rm -f "$temp_file"
done

echo ""
echo "================================================"
echo " Summary"
echo "================================================"
echo "Total feeds tested: ${#feeds[@]}"
echo "✅ Successful parses: $successful_parses"
echo "⚠️  Feeds with DTD: $has_dtd"
echo "⚠️  Feeds with external entities: $has_external_entities"
echo "❌ Parse errors: $failed_parses"
echo "⏱️  Timeouts/fetch failures: $timeout_count"
echo ""

if [ $has_dtd -gt 0 ]; then
    echo "NOTE: Feeds with DTD declarations will now work with our fix"
    echo "      (changed DtdProcessing.Prohibit -> DtdProcessing.Ignore)"
fi

if [ $has_external_entities -gt 0 ]; then
    echo "WARNING: Feeds with external entities may still fail"
    echo "         XmlResolver is set to null for security, which blocks external entities"
fi

echo ""
