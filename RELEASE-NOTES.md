# BatteryBuddy 1.0.3

The first official 1.0 release brings DeviceBuddy troubleshooting and improved keyboard accessibility.

## New

- DeviceBuddy lists connected peripherals for which no battery level was recognized, with their reported connection type and friendly suggestions. Wireless receivers can appear as wired USB devices; another connection mode may expose more information.
- All DeviceBuddy text is available in all 38 interface languages.
- Version and contributor information is available through real buttons and persistent, selectable, scrollable dialogs, in addition to hover help.
- Improved keyboard navigation, meaningful automation names, stable focus during device refresh and native Windows contrast-theme colors.

## Improved

- Settings controls respond consistently to keyboard and Windows UI Automation changes.
- Footer buttons share the selected light/dark theme. Save confirmation appears only above the footer and disappears after five seconds; errors remain readable.
- Safer confirmation-dialog defaults and keyboard tray-menu positioning.

## Requirements and limitations

Windows 10 or Windows 11 with .NET Framework 4.8 enabled. Extract the entire Windows ZIP and run `BatteryBuddy.exe`. No installer or administrator privileges are required.

Battery readings depend on the device, connection mode and Windows driver. DeviceBuddy offers best guesses, not definitive hardware diagnoses. Translations are best-effort machine translations with editorial review. Accessibility improvements have automated regression coverage but are not screen-reader or voice-control certification.

The application is unsigned. Download only from this repository and compare the ZIP's SHA-256 with `SHA256SUMS.txt`; a checksum is not a digital signature.

See [README.md](README.md), [LICENSE.md](LICENSE.md) and [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for usage, licensing and credits.
