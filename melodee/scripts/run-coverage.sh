#!/bin/bash
# Script to run tests with coverage and generate HTML reports
# Requires: dotnet-reportgenerator-globaltool
# Install with: dotnet tool install -g dotnet-reportgenerator-globaltool

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
COVERAGE_DIR="$ROOT_DIR/coverage"

echo "=== Melodee Test Coverage ==="
echo ""

# Clean previous coverage results
echo "Cleaning previous coverage results..."
rm -rf "$COVERAGE_DIR"
mkdir -p "$COVERAGE_DIR"

# Run tests with coverage
echo "Running tests with coverage collection..."
dotnet test "$ROOT_DIR/Melodee.sln" \
    --configuration Debug \
    --collect:"XPlat Code Coverage" \
    --settings "$ROOT_DIR/coverage.runsettings" \
    --results-directory "$COVERAGE_DIR/results"

# Check if ReportGenerator is installed
if ! command -v reportgenerator &> /dev/null; then
    echo ""
    echo "ReportGenerator not found. Installing..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
    export PATH="$PATH:$HOME/.dotnet/tools"
fi

# Generate HTML report
echo ""
echo "Generating HTML coverage report..."
reportgenerator \
    -reports:"$COVERAGE_DIR/results/**/coverage.cobertura.xml" \
    -targetdir:"$COVERAGE_DIR/report" \
    -reporttypes:"Html;HtmlSummary;TextSummary" \
    -assemblyfilters:"+Melodee.*;+server;+mcli;-Melodee.Tests.*" \
    -classfilters:"-*.Migrations.*;-System.Text.RegularExpressions.Generated.*"

echo ""
echo "=== Coverage Summary ==="
cat "$COVERAGE_DIR/report/Summary.txt" 2>/dev/null | head -50 || echo "Summary not available"

echo ""
echo "=== Done ==="
echo "HTML Report: $COVERAGE_DIR/report/index.html"
