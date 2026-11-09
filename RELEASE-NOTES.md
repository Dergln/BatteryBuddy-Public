# BatteryBuddy 1.0.12

## DeviceList sorting

- Standard Windows system battery, Windows Bluetooth and XInput routes appear
  first, followed by other families A-Z.
- Every column heading sorts the whole table A-Z or Z-A, with a visible direction
  arrow and keyboard-accessible buttons.
- Sorting follows the interface language and retains its column, direction and
  selected row when the language changes. Wrapped narrow and right-to-left layouts
  remain supported.
- The published protocol table uses the same default order. Provider polling
  priority and battery-reading behavior are unchanged by this sorting update.

## Device compatibility and protocol credits

- New **DeviceList** tab: an offline, responsive table of implemented protocol
  families, connection types, hardware-verification status, original contributions
  with source licenses, and practical caveats.
- Expanded experimental native battery queries for allowlisted SteelSeries,
  Logitech, Corsair legacy, HyperX, MCHOSE, Sony INZONE, Jabra, Lenovo and Astro
  headsets, plus Razer, ASUS, LAMZU, WLmouse, G-Wolves and other mice/keyboards.
- Passive PlayStation, 8BitDo and Steam controller battery reports and a bounded
  Nintendo Bluetooth device-info query; no enhanced report-mode switch.
- Shared bounded HID input/output and feature-report exchanges, exact model
  profiles, reply validation and cancellation ownership. No third-party
  executable or additional installed runtime is required.
- [Protocol catalog](docs/protocols.md) contains the same table as DeviceList and
  the precise matching profiles. These are experimental paths, not a claim that
  every listed model has been tested on hardware in BatteryBuddy.

## Clearer receiver behavior

Responsive devices are queried on each refresh. A receiver may retain a value
when the wireless device behind it disconnects or sleeps while the receiver stays
connected. That limitation does not mean receiver values are normally hours old.
Logitech's last-known fallback preserves its original contact timestamp.

Protocols requiring unsafe mode changes, weakly correlated replies, uncertain
presence semantics or a different long-lived event listener remain disabled.
Unknown/offline results never override a valid battery value, including real 0%.

## Install or update

Windows 10 or Windows 11 with .NET Framework 4.8 enabled. Extract the entire
`BatteryBuddy-1.0.12-windows.zip` and run `BatteryBuddy.exe`. No administrator rights
are required. When updating, close BatteryBuddy and replace files in the same
folder to preserve Windows tray visibility preferences.

Existing Stream Deck integration remains available through the separate
[BatteryBuddy Stream Deck Plugin](https://github.com/Dergln/BatteryBuddy-Plugin-Elgato-Public).

The app remains unsigned. Verify `SHA256SUMS.txt`; a checksum is not a digital
signature. Device battery support depends on what the device and Windows expose.

See [README.md](README.md), [LICENSE.md](LICENSE.md) and
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
