# BatteryBuddy 1.0.16

## Experimental wireless-receiver support

- Adds native, battery-only receiver adapters for Flydigi Vader 5 Pro and
  EWEADN H2/S9, A7/A7 Pro and S9 Pro 8K. A strictly profiled EWEADN X23SE
  candidate is also included, but still needs a successful real-device response.
- These protocols are experimental and have not been tested on BatteryBuddy
  hardware. Vader's seven reported levels map to approximate percentages.
  EWEADN A5 receiver support and unverified X23 aliases remain excluded.
- Requests are limited to battery status; no pairing, configuration, lighting,
  firmware or vendor runtime is involved. Exact profiles, source links, license
  labels and limitations are listed in the [protocol catalog](docs/protocols.md).
- The catalog contains 78 protocol families and 580 matching profiles; this is
  not a count of individually verified devices. Relevant caveats are available
  in all 38 interface languages.

## DeviceList hardware confirmations

DeviceList now links to published BatteryBuddy success reports for the specific
Audeze Maxwell USB-receiver and TEEvolution Terra Pro USB configurations. These
reports do not verify other models or connections, including Maxwell 2.

## Install or update

Windows 10 or Windows 11 with .NET Framework 4.8 enabled. Extract the entire
`BatteryBuddy-1.0.16-windows.zip` and run `BatteryBuddy.exe`. No administrator rights
are required. When updating, close BatteryBuddy and replace files in the same
folder to preserve Windows tray visibility preferences.

The app remains unsigned. Verify `SHA256SUMS.txt`; a checksum is not a digital
signature. Device battery support depends on what the device and Windows expose.

See [README.md](README.md), [LICENSE.md](LICENSE.md) and
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
