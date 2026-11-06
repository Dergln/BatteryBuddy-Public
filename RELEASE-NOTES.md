# BatteryBuddy 1.0.5

## New

- Optional integration with the separate [BatteryBuddy Stream Deck Plugin](https://github.com/Dergln/BatteryBuddy-Plugin-Elgato-Public).
- Local, same-user, read-only battery snapshots: device names, percentages, charging status and effective threshold colours. No HTTP server or control commands.
- Saving device thresholds immediately republishes the resolved colour, without waiting for another battery scan.

## Reliability

- Reconnect and bounded versioned snapshots support plugin restarts.
- A pipe shutdown cancellation race is fixed and covered by repeated shutdown/disconnect tests.
- Stream Deck Mobile testing confirmed device selection, percentages and per-device threshold colour updates.

## Install or update

Windows 10 or Windows 11 with .NET Framework 4.8 enabled. Extract the entire
`BatteryBuddy-1.0.5-windows.zip` and run `BatteryBuddy.exe`. No administrator rights
are required. When updating, close BatteryBuddy and replace files in the same
folder to preserve Windows tray visibility preferences.

For Stream Deck, also install the plugin from its separate Releases page and
select a device. BatteryBuddy must run as the same Windows user.

The app remains unsigned. Verify `SHA256SUMS.txt`; a checksum is not a digital
signature. Device battery support depends on what the device and Windows expose.

See [README.md](README.md), [LICENSE.md](LICENSE.md) and
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
