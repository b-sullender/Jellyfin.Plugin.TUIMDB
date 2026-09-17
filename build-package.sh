#!/bin/bash

set -euo pipefail

metadata_value() {
    grep -oP "\"$1\"\\s*:\\s*\"\\K[^\"]+" meta.json
}

build_value() {
    grep -oP "^$1:\\s*\"\\K[^\"]+" build.yaml
}

escape_metadata_value() {
    printf '%s' "$1" | sed 's/[\\/&]/\\&/g; s/"/\\\\"/g'
}

validate_metadata() {
    local meta_version build_version assembly_version meta_guid build_guid plugin_guid build_framework project_framework
    meta_version="$(metadata_value version)"
    build_version="$(build_value version)"
    assembly_version="$(grep -oP '<AssemblyVersion>\\K[^<]+' Directory.Build.props)"
    meta_guid="$(metadata_value guid)"
    build_guid="$(build_value guid)"
    plugin_guid="$(grep -oP 'Guid\.Parse\("\\K[^\"]+' Jellyfin.Plugin.TUIMDB/Plugin.cs)"
    build_framework="$(build_value framework)"
    project_framework="$(grep -oP '<TargetFramework>\\K[^<]+' Jellyfin.Plugin.TUIMDB/Jellyfin.Plugin.TUIMDB.csproj)"

    if [ "$meta_version" != "$build_version" ] || [ "$meta_version" != "$assembly_version" ]; then
        echo "ERROR: version differs between meta.json, build.yaml, and Directory.Build.props."
        exit 1
    fi

    if [ "$meta_guid" != "$build_guid" ] || [ "$meta_guid" != "$plugin_guid" ]; then
        echo "ERROR: GUID differs between meta.json, build.yaml, and Plugin.cs."
        exit 1
    fi

    if [ "$build_framework" != "$project_framework" ]; then
        echo "ERROR: framework differs between build.yaml and the project file."
        exit 1
    fi
}

# Function to safely remove directories
safe_rm_dir() {
    if [ -d "$1" ]; then
        echo "Removing directory: $1"
        rm -rf "$1"
    else
        echo "Directory does not exist, skipping: $1"
    fi
}

if [ "$#" -gt 0 ] && [ "$#" -ne 4 ]; then
    echo "Usage: $0 [--version 1.2.2.0 --changelog 'Release notes']"
    exit 1
fi

if [ "$#" -eq 4 ] && { [ "$1" != "--version" ] || [ "$3" != "--changelog" ]; }; then
    echo "Usage: $0 [--version 1.2.2.0 --changelog 'Release notes']"
    exit 1
fi

PLUGIN_NAME="TUIMDB"
PROJECT_DIR="Jellyfin.Plugin.TUIMDB"
META_FILE="meta.json"
PACKAGE_DIR="package"
PUBLISH_DIR="$PACKAGE_DIR/publish"
INTERMEDIATE_DIR="$PACKAGE_DIR/obj/"

if [ "$#" -eq 4 ]; then
    if ! [[ "$2" =~ ^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
        echo "ERROR: version must have four numeric segments, for example 1.2.2.0."
        exit 1
    fi

    changelog="$(escape_metadata_value "$4")"
    sed -i -E "s/(\"version\"[[:space:]]*:[[:space:]]*\")[^\"]*(\")/\\1$2\\2/" "$META_FILE"
    sed -i -E "s/^version:.*/version: \"$2\"/" build.yaml
    sed -i -E "s#<(Version|AssemblyVersion|FileVersion)>[^<]*</(Version|AssemblyVersion|FileVersion)>#<\\1>$2</\\2>#" Directory.Build.props
    sed -i -E "s/(\"changelog\"[[:space:]]*:[[:space:]]*\")[^\"]*(\")/\\1$changelog\\2/" "$META_FILE"
    sed -i -E "s/^changelog:.*/changelog: \"$changelog\"/" build.yaml
fi

validate_metadata
VERSION="$(metadata_value version)"
PUBLISH_DLL="$PUBLISH_DIR/Jellyfin.Plugin.TUIMDB.dll"

echo "Cleaning old build artifacts..."

# Remove old package directory if it exists
safe_rm_dir "$PACKAGE_DIR"

mkdir -p "$PUBLISH_DIR"

echo "Building plugin..."

# Build the exact assembly version declared in meta.json.
dotnet publish "$PROJECT_DIR/Jellyfin.Plugin.TUIMDB.csproj" --configuration Release --output "$PUBLISH_DIR" \
    -p:Version="$VERSION" -p:AssemblyVersion="$VERSION" -p:FileVersion="$VERSION" \
    -p:BaseIntermediateOutputPath="$INTERMEDIATE_DIR" -p:BaseOutputPath="$PACKAGE_DIR/bin/"

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

ZIP_NAME="${PLUGIN_NAME}_v${VERSION}.zip"

echo "Plugin version: $VERSION"
echo "Package name: $ZIP_NAME"

timestamp="$(date -u +"%Y-%m-%dT%H:%M:%SZ")"
sed -i -E "s/(\"timestamp\"[[:space:]]*:[[:space:]]*\")[^\"]*(\")/\\1$timestamp\\2/" "$META_FILE"

echo "Creating package structure..."

# Copy files into package directory
cp "$META_FILE" "$PACKAGE_DIR/"

echo "Creating ZIP package..."

# Flatten the two release files at the ZIP root, as Jellyfin expects.
zip -j "$ZIP_NAME" "$PUBLISH_DLL" "$META_FILE"

# Cleanup temporary package directory
rm -rf "$PACKAGE_DIR"

echo "Package created successfully:"
echo "  $ZIP_NAME"

echo "Checksum for manifest.json:"
md5sum "$ZIP_NAME"
