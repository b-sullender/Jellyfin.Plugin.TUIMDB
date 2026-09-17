#!/bin/bash

set -euo pipefail

metadata_value() {
    grep -oP "\"$1\"\\s*:\\s*\"\\K[^\"]+" meta.json
}

build_value() {
    grep -oP "^$1:\\s*\"\\K[^\"]+" build.yaml
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

    if [ "$meta_version" != "$build_version" ] || [ "$meta_version" != "$assembly_version" ] \
        || [ "$meta_guid" != "$build_guid" ] || [ "$meta_guid" != "$plugin_guid" ] \
        || [ "$build_framework" != "$project_framework" ]; then
        echo "ERROR: release metadata is out of sync. Run build-package.sh for a release, or update the config files together."
        exit 1
    fi
}

PROJECT_DIR="Jellyfin.Plugin.TUIMDB"
META_FILE="meta.json"
BUILD_OUTPUT_DIR="$(mktemp -d)"
trap 'rm -rf "$BUILD_OUTPUT_DIR"' EXIT

validate_metadata
VERSION="$(metadata_value version)"

# Build the plugin
dotnet publish "$PROJECT_DIR/Jellyfin.Plugin.TUIMDB.csproj" --configuration Release --output "$BUILD_OUTPUT_DIR" \
    -p:Version="$VERSION" -p:AssemblyVersion="$VERSION" -p:FileVersion="$VERSION" \
    -p:BaseIntermediateOutputPath="$BUILD_OUTPUT_DIR/obj/" -p:BaseOutputPath="$BUILD_OUTPUT_DIR/bin/"

PLUGIN_DIR="/var/lib/jellyfin/plugins/TUIMDB"

# Copy both the assembly and its manifest.  Without meta.json Jellyfin creates
# a fallback manifest with an unknown owner and an inferred version.
sudo install -d -o jellyfin -g jellyfin -m 755 "$PLUGIN_DIR"
sudo install -o jellyfin -g jellyfin -m 644 "$BUILD_OUTPUT_DIR/Jellyfin.Plugin.TUIMDB.dll" "$PLUGIN_DIR/Jellyfin.Plugin.TUIMDB.dll"
sudo install -o jellyfin -g jellyfin -m 644 "$META_FILE" "$PLUGIN_DIR/meta.json"

# Restart Jellyfin service
sudo systemctl restart jellyfin

echo "Plugin installed successfully."
