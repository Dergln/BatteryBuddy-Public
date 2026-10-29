# Security

## Report a vulnerability privately

Please **do not open a public issue for a security vulnerability**.

Use [GitHub private vulnerability reporting](https://github.com/Dergln/BatteryBuddy-Public/security/advisories/new) (**Security > Report a vulnerability**). Include the affected version, reproduction steps and expected impact. Remove personal data and credentials before attaching files.

Ordinary bugs and translation corrections belong in [Issues](https://github.com/Dergln/BatteryBuddy-Public/issues).

## Security model

- No application telemetry or remote device-data uploads. External links open in the system browser only when clicked.
- Settings and an error log are stored locally under `%APPDATA%\BatteryBuddy`.
- No administrator privileges are required; autostart uses the current user's registry.
- Battery queries use Windows APIs, Bluetooth and read-only HID++ battery requests, not firmware updates or device configuration commands.
- Settings and embedded language resources are validated. A single-instance guard prevents parallel app instances.

## Releases

Use the latest published version. Downloads are currently unsigned; verify their source and the provided SHA-256 checksum. A checksum does not replace a digital signature.
