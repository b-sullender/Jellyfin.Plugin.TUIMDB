#!/bin/bash

set -e

# Function to safely remove directories
safe_rm_dir() {
    if [ -d "$1" ]; then
        echo "Removing directory: $1"
        rm -rf "$1"
    else
        echo "Directory does not exist, skipping: $1"
    fi
}

PLUGIN_NAME="TUIMDB"
PROJECT_DIR="Jellyfin.Plugin.TUIMDB"
BUILD_DIR="$PROJECT_DIR/bin/Release/net9.0"
PUBLISH_DLL="$BUILD_DIR/Jellyfin.Plugin.TUIMDB.dll"
META_FILE="meta.json"

PACKAGE_DIR="package"

echo "Cleaning old build artifacts..."

# Remove build directories
safe_rm_dir "$PROJECT_DIR/bin"
safe_rm_dir "$PROJECT_DIR/obj"

# Remove old package directory if it exists
safe_rm_dir "$PACKAGE_DIR"

echo "Building plugin..."

# Build plugin
dotnet publish --configuration Release

# Verify DLL exists
if [ ! -f "$PUBLISH_DLL" ]; then
    echo "ERROR: Compiled DLL not found:"
    echo "  $PUBLISH_DLL"
    exit 1
fi

# Verify meta.json exists
if [ ! -f "$META_FILE" ]; then
    echo "ERROR: meta.json not found in current directory."
    exit 1
fi

# Get version from meta.json
VERSION=$(grep -oP '"version"\s*:\s*"\K[^"]+' "$META_FILE")

if [ -z "$VERSION" ]; then
    echo "ERROR: Could not determine version from $META_FILE."
    exit 1
fi

ZIP_NAME="${PLUGIN_NAME}_v${VERSION}.zip"

echo "Plugin version: $VERSION"
echo "Package name: $ZIP_NAME"

# Update timestamp in meta.json
timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
echo "Updating meta.json timestamp: $timestamp"
sed -i "s/\"timestamp\": \"[^\"]*\"/\"timestamp\": \"$timestamp\"/" "$META_FILE"

echo "Creating package structure..."

# Create temporary package directory
mkdir -p "$PACKAGE_DIR"

# Copy files into package directory
cp "$PUBLISH_DLL" "$PACKAGE_DIR/"
cp "$META_FILE" "$PACKAGE_DIR/"

echo "Creating ZIP package..."

# Create zip archive
(
    cd "$PACKAGE_DIR"
    zip -r "../$ZIP_NAME" .
)

# Cleanup temporary package directory
rm -rf "$PACKAGE_DIR"

echo "Package created successfully:"
echo "  $ZIP_NAME"

echo "Checksum for manifest.json:\n"
md5sum "$ZIP_NAME"
