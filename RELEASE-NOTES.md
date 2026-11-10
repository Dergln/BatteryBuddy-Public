# BatteryBuddy 1.0.15

## Expanded experimental battery support

- Independent native battery-only implementations informed by OpenMouse and
  original protocol sources: 35 additional families and extensions to existing
  families, alongside the existing Windows, Bluetooth, XInput and HID++ routes.
- Experimental Audeze Maxwell / Maxwell 2 support through five exact USB/receiver
  profiles, with bounded assembly of typed replies. Charging state is unavailable.
  A receiver may retain its last battery level after the headset disconnects.
- Experimental TEEvolution Terra Pro support, including the RapidSync 8K receiver:
  four exact product profiles, proven report-8 collections, online detection before
  battery/charging queries and validated replies. Close the device configurator
  before polling; exclusive access avoids competing requests.
- No third-party executable or vendor runtime is required. These additions do
  not initialize settings, pairing, firmware, LCD, clock or host-control modes.
- DeviceList and the [protocol catalog](docs/protocols.md) retain source links,
  license labels and practical limitations. The catalog now contains 76 families
  and 573 matching profiles, not that many hardware-verified devices.
- Relevant caveats are available in all 38 interface languages.

These adapters remain experimental: physical hardware validation of the new
models is outstanding. Please report the model, connection, firmware and app
version when sharing results, and omit personal information. Unsafe or ambiguous
protocol paths remain disabled. Unknown readings never override a valid value,
including real 0%.

## Stream Deck category correction

The local status pipe now publishes the effective device category, including a
saved manual override. Resetting to automatic detection is reflected too, without
waiting for another battery query. The separate
[BatteryBuddy Stream Deck Plugin](https://github.com/Dergln/BatteryBuddy-Plugin-Elgato-Public)
can therefore show the matching device category and icon.

## Install or update

Windows 10 or Windows 11 with .NET Framework 4.8 enabled. Extract the entire
`BatteryBuddy-1.0.15-windows.zip` and run `BatteryBuddy.exe`. No administrator rights
are required. When updating, close BatteryBuddy and replace files in the same
folder to preserve Windows tray visibility preferences.

The app remains unsigned. Verify `SHA256SUMS.txt`; a checksum is not a digital
signature. Device battery support depends on what the device and Windows expose.

See [README.md](README.md), [LICENSE.md](LICENSE.md) and
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
