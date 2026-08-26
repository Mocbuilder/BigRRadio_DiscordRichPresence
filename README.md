# Big R Radio - Discord Rich Presence

A lightweight desktop application that shows your [Big R Radio](https://www.bigrradio.com/) listening activity in Discord using Rich Presence.



> [!IMPORTANT]
> **Work in Progress:** This project is still under development and may contain bugs.
>
> Currently, only **80s Metal FM** is supported. Support for more Big R Radio stations will be added in future updates.

## Known Issues
### All Platforms
* On Launch of the application, the volume is turned off/muted. Simply click or move the volume slider to correctly initialise the volume.

---
## Getting Started

### Prerequisites

#### Windows
* **Windows 10 / 11** (64-bit)
* **WebView2 Runtime** (pre-installed on modern Windows versions)

#### Linux
* **GTK3**, **WebKit2GTK** (4.0 or 4.1) and **VLC** system libraries:
  * **Fedora / RHEL:** `sudo dnf install gtk3 webkit2gtk4.1 vlc`
  * **Ubuntu / Debian:** `sudo apt install libgtk-3-0 libwebkit2gtk-4.0-37 vlc` (or `libwebkit2gtk-4.1-0`)

#### macOS
* Unsupported and untested, though cross-platform building via Photino is theoretically possible.

#### General
* **Discord Desktop App** (required only for Rich Presence functionality; the audio player functions independently as a standalone player).

---

## Installation

### Windows
1. Download the latest `win-x64` archive from the [Releases](https://github.com/Mocbuilder/BigRRadio_DiscordRichPresence/releases) page.
2. Extract the `.zip` archive to your desired location.
3. Ensure the `wwwroot` folder remains in the same directory as the executable.
4. Run `BigRRadio_DiscordRichPresence.exe`.

### Linux
1. Download the latest `linux-x64` archive from the [Releases](https://github.com/Mocbuilder/BigRRadio_DiscordRichPresence/releases) page.
2. Extract the `.tar.gz` / `.zip` archive.
3. Make the binary executable if needed:
   ```bash
   chmod +x BigRRadio_DiscordRichPresence
	```
4. Run `./BigRRadio_DiscordRichPresence`.

## Building from Source

Requires the .NET 9.0 SDK.

### Build for Windows (Single File):
```Bash

dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```
### Build for Linux (Folder Bundle):
```Bash

dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=false
```
## License

This project is distributed under the **GNU General Public License v3.0**. See [`LICENSE`](LICENSE.txt) for more information.

## Credits

- **Mocbuilder** - Idea & Backend Development
- **Rimolo13** - Frontend Development

## Copyright & Legal Disclaimer

- All Big R Radio logos, names, and related trademarks are the property of **Big R Radio**. This project is open source and is not affiliated with, endorsed by, or associated with Big R Radio.
- The **80s Metal FM** logo asset was created using [Textstudio](https://www.textstudio.com/).

### AI Disclaimer: No AI was used in the development of this Application whatsoever.