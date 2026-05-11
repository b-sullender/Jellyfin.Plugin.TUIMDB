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

### Debian-Based Systems

Build and install the plugin directly using:

```bash
sudo bash build-install.sh
```

### Build Plugin Package

Build and package the plugin as a ZIP file:

```bash
bash build-package.sh
```

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
