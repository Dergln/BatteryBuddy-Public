# Battery protocol catalog

This table lists protocol families implemented in BatteryBuddy, not a complete list of
compatible models. A family entry is not a hardware test of every model or connection.
Unknown devices may still work through Windows or an implemented protocol.
The catalog in `DeviceCatalog` is the authoritative source for the table and DeviceList;
the published table is generated from that catalog.
External contribution labels include the source license in parentheses; BatteryBuddy's
own credits have no suffix. New external sources must record their verified license
in the catalog and retain the applicable full notices in [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md).
DeviceList wraps headings and cell text to the available window width, with the
largest column reserved for comments; resizing the app is not required.
The default order puts Windows system battery, Windows Bluetooth and XInput first,
then all other families A-Z. Activating any column heading sorts the entire table
A-Z; activating it again reverses that column to Z-A. The active heading shows an
arrow. Headings are keyboard-accessible; language changes retain the selected
sort column/direction and re-sort the translated text. The initial grouping is
restored when the settings window is reopened. This changes presentation only,
not provider polling priority. The documentation table uses the default order.

| Device or protocol family | Connection | Hardware verification | Contribution | Comments / caveats |
|---|---|---|---|---|
| Windows system battery | Windows | Models not individually verified | [BatteryBuddy](https://github.com/Dergln/BatteryBuddy-Public) | — |
| Windows Bluetooth | Bluetooth | Models not individually verified | [BatteryBuddy](https://github.com/Dergln/BatteryBuddy-Public) | — |
| XInput | XInput | Models not individually verified | [BatteryBuddy](https://github.com/Dergln/BatteryBuddy-Public) | — |
| 8BitDo Pro / Pro 2 / Pro 3 (passive enhanced reports) | USB / Bluetooth (D-input) | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [SDL (Zlib)](https://github.com/libsdl-org/SDL) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| AM Infinity / AJAZZ AJ159 APEX | 8K receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [AJAZZ Control Center (GPL-3.0-or-later)](https://github.com/Aiacos/ajazz-control-center) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Astro A50 Gen 5 | USB base station | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| ASUS ROG / TUF wireless mice | USB / receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [G-Helper (GPL-3.0-only)](https://github.com/seerge/g-helper) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| Corsair legacy wireless headsets | USB receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| G-Wolves new-protocol mice | USB / 8K receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [G-Wolves web driver (No license stated)](https://mouse.xyz) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| G-Wolves old-protocol mice | USB / receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [G-Wolves web driver (No license stated)](https://mouse.xyz) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| HyperX Cloud Alpha 2 | USB station (Controller) | Models not individually verified | [Halo Battery / user captures (MIT)](https://github.com/HeyOkay/HaloBattery/issues/26), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| HyperX Cloud II Wireless (HP) | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| HyperX Cloud III / Cloud II Core Wireless | USB receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HyperHeadset (MIT)](https://github.com/LennardKittner/HyperHeadset) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Jabra Link 380 / 390 / Evolve2 65 Flex | USB / receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Keychron M5 / Ultra-Link 8K | USB / receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [keychron-battery-dkms (GPL-2.0-only)](https://github.com/csutcliff/keychron-battery-dkms) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| LAMZU Maya X | USB / 8K receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [lamzu-battery-monitory (MIT)](https://github.com/Sheroune/lamzu-battery-monitory) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Lenovo Wireless VoIP Headset | USB receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Lofree Hyzen (wireless only) | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [Lofree web driver (No license stated)](https://hyzen.lofree.tech) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Logitech G PRO X 2 / X 3 / G522 (Centurion) | USB receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl / iowi479 / thanosk / KarimTayie (GPLv3)](https://github.com/Sapd/HeadsetControl), [Solaar (GPL-2.0-or-later)](https://github.com/pwr-Solaar/Solaar) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Logitech G533 / G535 / G733 / G933 / G935 / G PRO wireless voltage | USB receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl), [g933-utils (MIT)](https://github.com/ashkitten/g933-utils) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| Logitech G930 (voltage estimate) | USB receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl), [g930-battery-percentage (MIT)](https://github.com/Roliga/g930-battery-percentage) | Experimental; please send feedback. A connected receiver may retain the last battery level after the headset disconnects. |
| Logitech HID++ 2.0 | USB / Bluetooth | Models not individually verified | [BatteryBuddy](https://github.com/Dergln/BatteryBuddy-Public) | If the device stops responding (e.g. asleep or disconnected while its receiver stays connected), its last reading is kept for up to three hours. |
| MCHOSE A5 Pro Max | USB / 1K / 4K receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [MCHOSE A5 web driver (No license file stated)](https://github.com/z750sasr/mchose-a5-pro-max-web-driver), [mchose-macos (MIT)](https://github.com/Klegus/mchose-macos) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| MCHOSE G7 | USB / receiver | Models not individually verified | [Halo Battery / kek353 captures (MIT)](https://github.com/HeyOkay/HaloBattery/issues/8) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| MCHOSE M7 / L7 / A7 RealTek status | USB / receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [mchose-linux / alexfrih (MIT)](https://github.com/alexfrih/mchose-linux) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| MCHOSE X9 / V9 Pro | 2.4 GHz receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Nintendo Switch Pro Controller / Joy-Con | Bluetooth | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [SDL (Zlib)](https://github.com/libsdl-org/SDL), [Switch reverse-engineering notes (No license stated)](https://github.com/dekuNukem/Nintendo_Switch_Reverse_Engineering) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| NZXT Lift Elite Wireless | USB / 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Pulsar / ATK / VXE / Hitscan power reports | USB / 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [python-pulsar-mouse-tool (MIT)](https://github.com/andrewrabert/python-pulsar-mouse-tool), [hitscan-battery (No license stated)](https://github.com/sopparus/hitscan-battery) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Razer wireless keyboards (allowlisted models) | USB / HyperSpeed receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [OpenRazer (GPL-2.0-or-later)](https://github.com/openrazer/openrazer) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Razer wireless mice (allowlisted models) | USB / HyperSpeed receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [OpenRazer (GPL-2.0-or-later)](https://github.com/openrazer/openrazer) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Sony DualShock 4 / DualSense / Edge (passive full reports) | USB / Bluetooth | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [Linux (GPL-2.0-or-later)](https://github.com/torvalds/linux/blob/master/drivers/hid/hid-playstation.c) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| Sony INZONE H5 / H9 II | USB receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Steam Controller (2025) puck slots | USB puck | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [SDL (Zlib)](https://github.com/libsdl-org/SDL), [sc2-research (MIT)](https://github.com/CouchTurtle/sc2-research) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| SteelSeries Aerox 3 / 5 / 9 Wireless | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [rivalcfg (WTFPL)](https://github.com/flozz/rivalcfg), [alloyctl (GPLv2)](https://github.com/szymonwilczek/alloyctl) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| SteelSeries Arctis 1 / 7 / 9 / Nova Pro Wireless | USB receiver / base station | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl), [Linux (GPL-2.0-or-later)](https://github.com/torvalds/linux/blob/master/drivers/hid/hid-steelseries-arctis.c) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| SteelSeries Arctis Nova 7 (1038:22A1) | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl), [Linux (GPL-2.0-or-later)](https://github.com/torvalds/linux/blob/master/drivers/hid/hid-steelseries-arctis.c) | Experimental; please send feedback. A connected receiver may retain the last battery level after the headset disconnects. |
| SteelSeries Nova / Arctis 7+ / GameBuds | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| WLmouse Beast X / Max / Mini Pro | USB receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [mouse-battery-tray / len0c (MIT)](https://github.com/incconutwo/mouse-battery-tray) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |

Bluetooth depends on a Windows battery property; XInput reports coarse levels.
Logitech probing is limited to the existing HID++ interface allowlist and devices
that respond to its protocol. Neither recognition nor an entry promises a reading
for every device.
Responsive Logitech devices are queried on each refresh. The three-hour limit is a
last-known-value fallback for a device that stops responding, including sleep or a
disconnected wireless device behind a still-connected receiver; it is not a normal
three-hour delay in receiving battery updates. Retained values keep their original
last-contact time.

## Exact profiles and important limits

The expanded adapters use allowlisted VID/PID and collection/report profiles, not
device-name guesses or vendor-wide probing. Existing generic Windows, Bluetooth
and XInput providers remain enabled. The original Logitech interface allowlist is
unchanged except that exact headset profiles now own their matching interface;
they are not polled concurrently by the generic HID++ provider.

The profiles below include experimental variants which have not been tested on
BatteryBuddy hardware. Minimum report sizes are bounded at 1024 bytes; feature
lengths marked exact must match exactly. An unrestricted usage page still needs
an explicit product plus a vendor-page/interface or exact feature descriptor.
No keyboard/mouse controller is switched into an enhanced mode, no lighting,
rumble, firmware, pairing, sidetone or other settings are changed.

- PlayStation and 8BitDo are **listen-only**. Basic Bluetooth/D-input reports may
  contain no battery at all; a full report already enabled by the device/game is
  required (8BitDo Pro 3 reports its battery without a mode switch). Zero padding
  in an ordinary 8BitDo report is not an empty battery. USB DS4 receivers with no
  paired controller are rejected.
- Nintendo is Bluetooth-only. Its read-only device-information subcommand uses
  neutral rumble data, not a mode/USB handshake. USB remains on the existing
  generic routes.
- Steam Controller 2025 uses separate bounded listeners for puck slots 1-4,
  allowing five seconds for its approximately 2.5-second battery-report cadence.
  No lizard-mode or other behavior setting is changed.
- Some sources report coarse steps or voltage estimates, not measured percent.
  The DeviceList marks these approximate; source-specific invalid/offline states
  never become fabricated zero/100-percent readings.
- Receiver/cable paths remain separate unless the existing identity layer can
  establish their identity. Identical model names are not evidence that two
  devices are one device; experimental receiver/cable entries may appear twice.
- Lofree is queried only while its wired counterpart is absent, and only explicit
  command-echo replies are accepted. Firmware returning anonymous command-zero
  replies is deliberately not interpreted.
- G930 does not supply a reliable headset-link flag: its receiver can expose an
  old voltage after the headset disconnects. That limitation is shown in DeviceList.
- The legacy Corsair `C9 64` adapter does not enable Bragi/Slipstream/Void V2.
  The suspect upstream product literal `1B1C` is excluded.

The table and profile appendix are generated from the same catalog used by
DeviceList; no separate vendor-name allowlist is used.

<details>
<summary>Executable VID/PID, HID collection and length allowlists</summary>

<!-- profile-list:start -->
Exact executable profiles (generated from the same catalog; `*` means not restricted by that field):

- `8bitdo-passive`: `2DC8:6000,6100,6001,6101,6003,6006,6009`; usage `0001:0004`; interface `*`; input/output `15/0`, feature `-` (minimum).
- `8bitdo-passive`: `2DC8:6000,6100,6001,6101,6003,6006,6009`; usage `0001:0005`; interface `*`; input/output `15/0`, feature `-` (minimum).
- `am-infinity`: `3151:5007`; usage `FFFF:0002`; interface `*`; input/output `0/0`, feature `65` (minimum).
- `astro-a50-gen5`: `046D:0B1C`; usage `FF32:0074`; interface `*`; input/output `9/64`, feature `-` (minimum).
- `asus-mouse`: `0B05:1A72,1A70,1B0C,1B0A,197F,197D,1A1A,1A18,1A94,1A92,1A68,1A66,1979,1977,19F4,1A8D,1AF5,1AF3,1C57,1C56,18E5,18E3,1960,195E,1A59,1A57,1908,1906,1949,1947`; usage `*:*`; interface `0`; input/output `10/65`, feature `-` (minimum).
- `corsair-legacy`: `1B1C:1B27,0A14,0A16,0A17,0A1D,0A1A,1B2A,1B23,1B29,0A55,0A51,0A52,0A38,0A4F,0A2B,0A75,0A56`; usage `FFC5:0001`; interface `3`; input/output `5/2`, feature `-` (minimum).
- `gwolves-new`: `33E4:3854,5418,5419,5219,4718,4719,5618,5619,3619,3519,2719,4219,3808,3817,6808,6817,5608,5617,3608,3617`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact).
- `gwolves-old`: `33E4:3908,3917,7904,7913,3708,3717,5308,5317,7704,7713,3508,3517,7908,7917,5808,5817,5807,2708,2717,5408,5417,5407,5708,5717,5707,5908,5917,5907,7204,7203,7708,7717,5804,5803,5404,5403,5704,5703,5904,5903`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact).
- `hyperx-alpha-2`: `03F0:08BE`; usage `FF13:FF00`; interface `*`; input/output `7/64`, feature `-` (minimum).
- `hyperx-cloud-ii-hp`: `03F0:0696,018B`; usage `FF90:0303`; interface `*`; input/output `20/52`, feature `-` (minimum).
- `hyperx-cloud-iii`: `03F0:05B7,0C9D,0795,0995`; usage `FF13:0001`; interface `*`; input/output `5/0`, feature `62` (minimum).
- `hyperx-cloud-iii`: `03F0:05B7,0C9D,0795,0995`; usage `FF13:0001`; interface `*`; input/output `5/62`, feature `-` (minimum).
- `jabra-gnp`: `0B0E:24C7,2519`; usage `FF00:0001`; interface `3`; input/output `64/64`, feature `-` (minimum).
- `jabra-gnp`: `0B0E:2E57`; usage `FF00:0005`; interface `3`; input/output `64/64`, feature `-` (minimum).
- `keychron-status`: `3434:D028,D048`; usage `*:*`; interface `4`; input/output `21/0`, feature `64` (minimum).
- `lamzu-maya-x`: `373E:001E,001C`; usage `FFFF:*`; interface `2`; input/output `0/0`, feature `65` (minimum).
- `lenovo-voip`: `17EF:A07D`; usage `FF07:0222`; interface `3`; input/output `61/0`, feature `61` (minimum).
- `lofree-hyzen`: `388D:0025`; usage `FF1C:0092`; interface `*`; input/output `32/32`, feature `-` (minimum).
- `logitech-centurion`: `046D:0AF7,0B18,0B45`; usage `FFA0:0001`; interface `3`; input/output `64/64`, feature `-` (minimum).
- `logitech-g930`: `046D:0A1F`; usage `FF00:0001`; interface `*`; input/output `0/0`, feature `64` (minimum).
- `logitech-headset-voltage`: `046D:0AC4`; usage `000C:0001`; interface `3`; input/output `20/20`, feature `-` (exact).
- `logitech-headset-voltage`: `046D:0A66,0A5B,0A87,0AB5,0AFE,0B1F,0ABA,0AFB,0AFC`; usage `FF43:0202`; interface `*`; input/output `20/20`, feature `-` (exact).
- `logitech-hidpp`: `046D:*`; usage `FF00:*`; interface `*`; input/output `20/20`, feature `-` (exact).
- `logitech-hidpp`: `046D:*`; usage `FF43:*`; interface `*`; input/output `20/20`, feature `-` (exact).
- `mchose-a5`: `2023:F013,F015,F019`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact).
- `mchose-g7`: `A8A5:2255`; usage `FF01:0010`; interface `*`; input/output `10/65`, feature `-` (minimum).
- `mchose-headsets`: `291D:385D`; usage `FF90:0001`; interface `0`; input/output `64/64`, feature `-` (minimum).
- `mchose-headsets`: `3837:6045`; usage `FF90:0001`; interface `0`; input/output `64/64`, feature `-` (minimum).
- `mchose-realtek`: `3837:100B`; usage `FF01:0001`; interface `2`; input/output `0/0`, feature `65` (minimum).
- `mchose-realtek`: `5253:1020,0031`; usage `FF01:*`; interface `*`; input/output `0/0`, feature `65` (minimum).
- `nintendo-status`: `057E:2006,2007,2009`; usage `0001:0004`; interface `*`; input/output `3/49`, feature `-` (minimum).
- `nintendo-status`: `057E:2006,2007,2009`; usage `0001:0005`; interface `*`; input/output `3/49`, feature `-` (minimum).
- `nzxt-lift`: `1E71:2101,2129`; usage `FFCA:0001`; interface `*`; input/output `27/64`, feature `-` (minimum).
- `playstation-passive`: `054C:05C4,09CC,05C5,0BA0`; usage `0001:0004`; interface `*`; input/output `31/0`, feature `-` (minimum).
- `playstation-passive`: `054C:0CE6,0DF2`; usage `0001:0004`; interface `*`; input/output `55/0`, feature `-` (minimum).
- `playstation-passive`: `054C:05C4,09CC,05C5,0BA0`; usage `0001:0005`; interface `*`; input/output `31/0`, feature `-` (minimum).
- `playstation-passive`: `054C:0CE6,0DF2`; usage `0001:0005`; interface `*`; input/output `55/0`, feature `-` (minimum).
- `pulsar-power`: `3554:F508,F507,F58F,F58A,F58C`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (minimum).
- `pulsar-power`: `3710:5406,3414`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (minimum).
- `pulsar-power`: `373B:1085`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (minimum).
- `pulsar-power`: `3770:0200,0100`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (minimum).
- `razer-keyboard`: `1532:025A`; usage `*:*`; interface `2`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 3F).
- `razer-keyboard`: `1532:0290,0296,02D5,025C`; usage `*:*`; interface `2`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 9F).
- `razer-keyboard`: `1532:0292,0298,0258,02B9,02D7`; usage `*:*`; interface `3`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 1F).
- `razer-keyboard`: `1532:0271,02BA`; usage `*:*`; interface `3`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 9F).
- `razer-mouse`: `1532:0062,006F,0070,0077,0080,0086,0088,008F,0090,0094,009A,009C,009E,009F,00A5,00A6,00A7,00A8,00AA,00AB,00AF,00B0,00B3,00B4,00B6,00B7,00C2,00C3,00B8,00B9,00BE,00BF,00C0,00C1,00C4,00C5,00C7,00C8,00CC,00CD,00D0,00D1,00D3,00D4,00D6,00D7`; usage `*:*`; interface `*`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 1F).
- `razer-mouse`: `1532:0059,005A,0072,0073,007C,007D`; usage `*:*`; interface `*`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 3F).
- `razer-mouse`: `1532:001F,0024,0025,0032,003E,003F,0044,0045,007A,007B,0083`; usage `*:*`; interface `*`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction FF).
- `sony-inzone`: `054C:0FA8`; usage `FF04:0001`; interface `5`; input/output `64/64`, feature `-` (minimum).
- `sony-inzone`: `054C:0EBF`; usage `FF04:0002`; interface `*`; input/output `64/64`, feature `-` (minimum).
- `steam-passive`: `28DE:1304,1305`; usage `FF00:0001`; interface `2`; input/output `15/0`, feature `-` (minimum).
- `steam-passive`: `28DE:1304,1305`; usage `FF00:0001`; interface `3`; input/output `15/0`, feature `-` (minimum).
- `steam-passive`: `28DE:1304,1305`; usage `FF00:0001`; interface `4`; input/output `15/0`, feature `-` (minimum).
- `steam-passive`: `28DE:1304,1305`; usage `FF00:0001`; interface `5`; input/output `15/0`, feature `-` (minimum).
- `steelseries-aerox`: `1038:1838,1878,1852,185C,1860,1858,1874`; usage `FFC0:0001`; interface `*`; input/output `2/64`, feature `-` (minimum).
- `steelseries-classic`: `1038:12C2`; usage `*:*`; interface `0`; input/output `5/2`, feature `-` (minimum).
- `steelseries-classic`: `1038:12E0,12E5`; usage `*:*`; interface `3`; input/output `16/2`, feature `-` (minimum).
- `steelseries-classic`: `1038:12E0,12E5`; usage `*:*`; interface `4`; input/output `16/2`, feature `-` (minimum).
- `steelseries-classic`: `1038:12AD,1260,1252`; usage `*:*`; interface `5`; input/output `5/2`, feature `-` (minimum).
- `steelseries-classic`: `1038:12B3,12B6,12D7,12D5`; usage `FF43:*`; interface `3`; input/output `4/2`, feature `-` (minimum).
- `steelseries-nova-status`: `1038:2202,227E,2206,2258,229E,22AD,22A4,22A5,223A,22A9,227A,2232,2253,220A,22A7,2298,2269,226D,220E,2212,2216,2236,230A,2317`; usage `FFC0:0001`; interface `3`; input/output `7/2`, feature `-` (minimum).
- `steelseries-nova7-22a1`: `1038:22A1`; usage `FFC0:0001`; interface `3`; input/output `64/64`, feature `-` (exact).
- `steelseries-nova7-22a1`: `1038:22A1`; usage `FFC0:0001`; interface `3`; input/output `64/65`, feature `-` (exact).
- `steelseries-nova7-22a1`: `1038:22A1`; usage `FFC0:0001`; interface `3`; input/output `65/64`, feature `-` (exact).
- `steelseries-nova7-22a1`: `1038:22A1`; usage `FFC0:0001`; interface `3`; input/output `65/65`, feature `-` (exact).
- `wlmouse-feature`: `36A7:A887,A868,A880`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact).
<!-- profile-list:end -->

</details>

These are independent implementations based on documented protocol behavior,
not bundled external executables. None of these additions has been verified on
physical hardware in BatteryBuddy.

Centurion profiles cover G PRO X 2 `046D:0AF7`, G522 `046D:0B18` and
G PRO X 3 `046D:0B45`, on `FFA0:0001`, interface 3. They require discovered
feature `0104`; the X2 hardcoded legacy fallback is deliberately not used.
The X3 upstream evidence is firmware revision 1.02, not every firmware version.
Original HeadsetControl contributions include [iowi479's G522 work](https://github.com/Sapd/HeadsetControl/pull/501),
[thanosk's Centurion port](https://github.com/Sapd/HeadsetControl/pull/592)
and [KarimTayie's X3 work](https://github.com/Sapd/HeadsetControl/pull/618)
(GPLv3), with the original Centurion reference from Solaar (GPL-2.0-or-later).

The experimental Nova 7 adapter accepts only the specified product, USB interface 3,
`FFC0:0001` collection and 64/65-byte input and output report lengths.
It sends a battery-status request and requires an online link indication; the receiver
can retain an old level when disconnected. If its link indication is wrong or delayed,
the value may still appear current. The adapter does not prove that simultaneous
Bluetooth and receiver readings belong to the same physical headset. Its Windows
transport has not been tested with this hardware in BatteryBuddy. Halo's hardware
report is upstream evidence only. Please report success or problems with the model,
connection, firmware and BatteryBuddy version; omit personal information.

Other Halo Battery or HeadsetControl device-list and hardware-test claims do not
establish BatteryBuddy support.
Before adding an adapter, trace and retain its applicable source and license notices,
prove its requests are safe, and test the exact model and connection.

Paths intentionally not enabled include mode-changing Razer headset routes,
uncorrelated newer Corsair properties, uncertain Maxwell/Nova Elite link-state
behavior, unresolved Windows collection/priming requirements in other HyperX
variants, and event-only devices needing a persistent listener. A matching vendor
name or an upstream support-list entry is not enough to enable a battery command.
