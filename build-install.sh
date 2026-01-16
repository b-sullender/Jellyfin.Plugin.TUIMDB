#!/bin/bash

# Ensure the script is run as root
if [ "$(id -u)" -ne 0 ]; then
    echo "Please run as root: 'sudo bash build-install.sh'"
    exit 1
fi

# Function to safely remove directories
safe_rm_dir() {
    if [ -d "$1" ]; then
        echo "Removing directory: $1"
        rm -rf "$1"
    else
        echo "Directory does not exist, skipping: $1"
    fi
}

# Remove build directories if they exist
safe_rm_dir "Jellyfin.Plugin.TUIMDB/bin"
safe_rm_dir "Jellyfin.Plugin.TUIMDB/obj"

# Build the plugin
dotnet publish --configuration Release

# Remove existing plugin installation if it exists
PLUGIN_DIR="/var/lib/jellyfin/plugins/TUIMDB"
if [ -d "$PLUGIN_DIR" ]; then
    echo "Removing existing plugin directory: $PLUGIN_DIR"
    rm -rf "$PLUGIN_DIR"
fi

# Recreate plugin directory
mkdir -p "$PLUGIN_DIR"

# Copy plugin DLL
cp Jellyfin.Plugin.TUIMDB/bin/Release/net9.0/publish/Jellyfin.Plugin.TUIMDB.dll "$PLUGIN_DIR/"

# Set correct ownership
chown -R jellyfin:jellyfin "$PLUGIN_DIR"

# Restart Jellyfin service
systemctl restart jellyfin

echo "Plugin installed successfully."
