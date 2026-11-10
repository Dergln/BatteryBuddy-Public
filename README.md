# BatteryBuddy

A free Windows tray app that monitors device batteries and warns you before they run out.

## Download and run

1. Open the [latest release](https://github.com/Dergln/BatteryBuddy-Public/releases/latest).
2. Download `BatteryBuddy-VERSION-windows.zip` from **Assets** and extract the whole ZIP into a folder.
3. Run `BatteryBuddy.exe`. Keep the accompanying DLLs and configuration file in the same folder.
4. Right-click its tray icon to open the menu or settings.

<img width="400" height="640" alt="overview_1_devices" src="https://github.com/user-attachments/assets/8a937eeb-f599-470f-ba18-33aad84010e7" /> <img width="400" height="640" alt="overview_2_settings" src="https://github.com/user-attachments/assets/8078de1e-a49f-4e43-9d42-3e8781fdd346" />

## Device recognition "best guess" helper

Should a device not show up in the 'Devices' tab, you can check via the 'DeviceBuddy' tab why that may be the case.

<img width="400" height="414" alt="20261104_001212" src="https://github.com/user-attachments/assets/5c3b1776-00ea-4968-b44f-808bfc5c2363" /> <img width="400" height="414" alt="20261104_001416" src="https://github.com/user-attachments/assets/2917bceb-94c5-4b89-be2e-1c4c34a102d3" />

## Specific device/protocol compatibility

A bunch of devices/manufacturers use proprietary protocols/datastreams to map out their internal data.
There's varying degrees of reverse engineered solutions out there - BatteryBuddy will try to integrate them if the solution seems feasible and somewhat verified.
If you find a device not to be recognized -> raise it in the issuetracker. We can then check if someone already tried to decipher its data structure.

<img width="600" height="620" alt="image" src="https://github.com/user-attachments/assets/77b14072-4bd9-4a36-ad0d-533c69e38a74" />

[All releases](https://github.com/Dergln/BatteryBuddy-Public/releases) remain available for previous versions. GitHub's automatic **Source code** downloads are for developers, not the ready-to-run app.

**Requirements:** Windows 10 or Windows 11 with .NET Framework 4.8 enabled. No administrator privileges or Logitech Options+ installation are required. Device support depends on what the device and Windows expose.

The app is currently **not digitally signed**. Windows SmartScreen may show a reputation warning. Only download releases from this repository; a SHA-256 checksum accompanies each Windows ZIP. A checksum detects a changed download but is not a digital signature or a guarantee of trust.

To check the ZIP in PowerShell, compare the result with the release's `SHA256SUMS.txt`:

```powershell
Get-FileHash .\BatteryBuddy-1.0.15-windows.zip -Algorithm SHA256
```

## Features

- Battery monitoring for the computer's internal battery, supported Bluetooth devices, Xbox controllers and supported Logitech Bolt/Unifying HID++ devices.
- Configurable per-device warning thresholds, Windows notifications and optional warning sounds.
- Main tray icon plus optional per-category or per-device battery icons.
- Last-contact indicators for inactive devices, device categories, light/dark themes and optional autostart.
- **DeviceBuddy:** a read-only troubleshooting tab for connected peripherals without a recognized battery level. It shows Windows' reported connection and best-guess explanations, using the same nonblocking scans as battery monitoring. A wireless USB receiver may appear to Windows as a wired controller; this does not prove that the controller has no battery.
- **DeviceList:** a wrapped, offline compatibility table with original protocol sources, license labels and caveats. Additional allowlisted headsets, mice, keyboards and controllers have experimental native status adapters, inspired by Halo Battery, HeadsetControl and their original references. No third-party executable is required. See the [protocol catalog and exact profiles](docs/protocols.md); a family entry is not a hardware test of every model.
- **Expanded experimental protocols:** native battery-only adapters informed by OpenMouse, including Audeze Maxwell / Maxwell 2 and TEEvolution Terra Pro USB/receiver profiles. The catalog contains 76 protocol families and 573 matching profiles, not that many hardware-verified devices. Maxwell charging state is unavailable; a receiver may retain its last value after the headset disconnects. Terra Pro requires exclusive access: close its configurator before polling.
- **DeviceList sorting:** Windows system battery, Windows Bluetooth and XInput appear first by default, followed by the other families A-Z. Activate any column heading to sort the whole table A-Z; activate it again for Z-A. Sorting changes presentation, not battery-provider priority.
- Keyboard-friendly navigation, stable device-list focus during refresh, named controls and Windows contrast-theme colors. Version and contributor buttons open persistent, selectable information dialogs; save confirmation appears separately for five seconds without hiding the footer. Automated accessibility checks do not constitute screen-reader or voice-control certification.
- 38 interface languages, including right-to-left layouts for Arabic and Hebrew.
- All functionality is unlocked. There is no paid Pro version; support is optional.
- **Stream Deck:** display device percentages and threshold colours with the separate [BatteryBuddy Stream Deck Plugin](https://github.com/Dergln/BatteryBuddy-Plugin-Elgato-Public). Status travels through a local, same-user, read-only Windows named pipe, without an HTTP server. Saved category overrides and resets to automatic detection are reflected immediately, without waiting for another battery poll. The plugin also works with Stream Deck Mobile.

Some devices do not report battery percentages, report them only while connected, or require vendor-specific support. BatteryBuddy cannot obtain information that the device does not expose.
Responsive devices are queried on each refresh. If a wireless device sleeps or
disconnects while its receiver remains connected, the receiver may retain its last
reading. Logitech's last-known fallback retains the original contact time for up
to three hours; this is not a normal delay in receiving fresh values.

## Available plugins

Extend BatteryBuddy with these separately installed plugins:

| Plugin | What it does | Repository |
| --- | --- | --- |
| Elgato Stream Deck | Displays device battery percentages, charging status and threshold colours on Stream Deck or Stream Deck Mobile. | [BatteryBuddy Stream Deck Plugin](https://github.com/Dergln/BatteryBuddy-Plugin-Elgato-Public) |

Each plugin's repository contains its requirements, installation instructions and
release downloads.

## Local data and privacy

Settings are stored in `%APPDATA%\BatteryBuddy\settings.json`; the same folder may contain `error.log`. BatteryBuddy does not send telemetry or upload device information. Clicking an external link opens your default browser and visits that external service.

To preserve Windows tray visibility preferences when updating a portable installation, close BatteryBuddy and replace the files in the **same folder**. Do not delete the whole Windows tray cache.

## Build from source

Use Windows with the [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) and the [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48). Building also restores NuGet dependencies and therefore requires internet access on the first build.

From the repository root:

```powershell
dotnet build .\BatteryBuddy.sln -c Release
```

The runnable output is in `src\BatteryBuddy.App\bin\Release\net48`. Distribute the EXE, its `.config`, and the `BatteryBuddy.Core.dll` and `BatteryBuddy.Providers.dll` files together with this README and the license/third-party notices. Images, translations and the default sound are embedded.

This repository contains curated released application source, not internal development history or test infrastructure. Its solution contains only the three product projects. Public builds omit debug symbols and revision suffixes and map local source paths to a neutral build path. Release packages are built from the corresponding public source tag.

## Feedback and translations

Report reproducible bugs, translation problems and feature requests in [Issues](https://github.com/Dergln/BatteryBuddy-Public/issues). Include the app version and relevant steps, but remove personal information from screenshots and logs. A GitHub account is sufficient; do not post your email address.

Translations are a best-effort mixture of machine translation and editorial review, not native-speaker-certified translations. Reports with the language, existing wording and suggested correction are welcome.

For security vulnerabilities, use the [private reporting instructions](SECURITY.md), **not** a public issue.

## License and credits

BatteryBuddy's own program code is licensed under [GNU GPL version 3](LICENSE.md). Copyright remains with [Dergln](https://github.com/Dergln). Private and commercial use are permitted under the applicable license terms.

**Third-party artwork and audio retain their separate licenses; they are not relicensed under the GPL by this repository.** See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) and the app's **Contributions** tab for individual sources, authors and terms. Protocol contributions and their source licenses are listed separately in **DeviceList** and the [protocol catalog](docs/protocols.md). Graphics are included as functional resources of this application, not as a standalone graphics collection or a grant of independent asset-reuse rights.
