# TUIMDB Jellyfin Plugin

Official Jellyfin plugin for **TUIMDB** (**The Ultimate Internet Media Database**).

TUIMDB is an open media database focused on providing accurate metadata for media servers, with support for **physical media collections**, **alternate episode orders**, and improved genre organization.

## Features

- Metadata powered by TUIMDB
- Support for alternate episode orders
- Improved genre support
- Free and open API (no API key required)

## Why TUIMDB?

Many existing metadata providers do not properly support alternate episode structures used by physical media releases.

Examples include:

- **Friends** finale episodes split differently between broadcast and home releases
- **Dexter’s Laboratory** segment-based episodes reordered or merged across releases
- Alternate release structures for animated series and collector editions

TUIMDB is designed to support these cases natively, reducing the need for manual episode splitting, merging, or metadata corrections.

## Installation

### Install Through Jellyfin

Add the TUIMDB plugin repository to Jellyfin:

```text
https://tuimdb.com/jellyfin/manifest.json
```

After adding the repository, install the **TUIMDB** plugin from the Jellyfin plugin catalog.

## Episode Order Detection

To allow the plugin to detect your preferred episode order, name your series folders using the following format:

```text
Friends (1994) {Standard Order}
Dexter's Laboratory (1996) {Boxset Order}
The Powerpuff Girls (1998) {Boxset Order}
Spartacus (2010) {Alternative Order}
```

## Building From Source

Run all build commands from the repository root. You need the .NET 8 SDK; package
creation also requires `zip`. Local installation is intended for Debian-based
systems with a `jellyfin` service account.

### Verify a Local Build

Build the plugin and validate its release metadata without changing your Jellyfin
installation:

```bash
bash build-install.sh --dry-run
```

The build is written to a temporary directory and removed afterwards. This is the
recommended first check after changing plugin code or release metadata.

### Install Locally

Build and install the plugin into a local Jellyfin server:

```bash
bash build-install.sh
```

The script asks for `sudo` only when it copies the DLL and `meta.json` to
`/var/lib/jellyfin/plugins/TUIMDB` and restarts Jellyfin. Installing `meta.json`
is required for the dashboard to show the plugin owner and correct version.

### Create a Release Package

Create the next release ZIP while updating the version and changelog everywhere
they are required:

```bash
bash build-package.sh --version 1.2.2.0 --changelog "Describe the release"
```

Versions must contain exactly four numeric segments, such as `1.2.2.0`. Keep the
changelog entry to one line. The command:

1. Updates the version and changelog in `meta.json` and `build.yaml`.
2. Updates `Version`, `AssemblyVersion`, and `FileVersion` in
   `Directory.Build.props` so the DLL reports the released version.
3. Updates the `meta.json` timestamp, builds the plugin, and creates
   `TUIMDB_v<version>.zip` plus its MD5 checksum.

The ZIP contains only `Jellyfin.Plugin.TUIMDB.dll` and `meta.json` at its root.
Review and commit the metadata changes before creating the corresponding Git tag
and publishing the archive.

### Release Metadata Checks

Both scripts stop before building if the release identity is inconsistent. They
verify that the version agrees across `meta.json`, `build.yaml`, and
`Directory.Build.props`; that the GUID agrees with the `Id` in `Plugin.cs`; and
that the project target framework agrees with `build.yaml`.

For changes other than the version and changelog, update the relevant tracked
metadata before running a script. In particular, do not change the plugin GUID
after publishing a release. The repository manifest hosted at
`https://tuimdb.com/jellyfin/manifest.json` must use that same GUID and the
version of the ZIP being published.

## API

TUIMDB provides a free public API with no API key required.

Optional API keys may be used for personalization and preferred metadata settings.

## Repository

GitHub Repository:

https://github.com/b-sullender/Jellyfin.Plugin.TUIMDB

## Feedback & Support

Feedback, feature requests, and metadata discussions are welcome.

- Website: https://tuimdb.com
- Reddit: https://reddit.com/r/TUIMDB

---

<p align="center">
  <img src="https://tuimdb.com/jellyfin/tuimdb.png" alt="TUIMDB Logo" width="500">
</p>
