# BatteryBuddy 1.0.18

## High-DPI settings window

- Settings now fit the available monitor work area when opened and after DPI
  changes, keeping the title bar, tabs and footer reachable.
- Verified on a 3840 x 2160 display at 300% Windows scaling. The taskbar's
  reserved space is respected.

## CHERRY STREAM DESKTOP diagnostic entry

- DeviceList includes keyboard JD-85K, mouse JD-85M and receiver JD-85R,
  USB ID **046A:0115**, clearly marked **Tested: no battery data**.
- The devices provide local low-battery LED warnings. No supported battery
  query is implemented; no guessed commands or fabricated readings are added.
- This records the observed test result, not proof that proprietary battery
  reporting is impossible. The compact status and notes are available in all
  38 interface languages.
- Existing supported protocols and polling profiles are unchanged. See the
  [protocol catalog](docs/protocols.md) for sources and limitations.

## Install or update

Windows 10 or Windows 11 with .NET Framework 4.8 enabled. Extract the entire
`BatteryBuddy-1.0.18-windows.zip` and run `BatteryBuddy.exe`. No administrator rights
are required. When updating, close BatteryBuddy and replace files in the same
folder to preserve Windows tray visibility preferences.

The app remains unsigned. Verify `SHA256SUMS.txt`; a checksum is not a digital
signature. Device battery support depends on what the device and Windows expose.

See [README.md](README.md), [LICENSE.md](LICENSE.md) and
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
