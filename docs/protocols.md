# Battery protocol catalog

This table lists protocol families implemented in BatteryBuddy, not a complete list of
compatible models. A family entry is not a hardware test of every model or connection.
Unknown devices may still work through Windows or an implemented protocol.
The catalog in `DeviceCatalog` is the authoritative source for DeviceList; this
published table and exact profile list are maintained to match it.
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
| AJAZZ AJ200P battery | USB / wireless receiver | Models not individually verified | [OpenMouse AJ200P battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/ajazz/aj200p.ts) | Experimental; not hardware-tested in BatteryBuddy. Some models report approximate battery levels or no charging state. |
| AJAZZ NJ07 / NJ08 battery | USB / 2.4 GHz receiver | Models not individually verified | [OpenMouse AJAZZ battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/ajazz), [AJAZZ vendor WebHID configurator (Not stated)](https://nacodex.yjx2012.com/) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| AM Infinity / AJAZZ AJ159 APEX | 8K receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [AJAZZ Control Center (GPL-3.0-or-later)](https://github.com/Aiacos/ajazz-control-center) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Amkette XS battery telemetry | USB / Bluetooth / receiver | Models not individually verified | [OpenMouse Amkette XS telemetry facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/amkette), [Amkette XS web configurator (Not stated)](https://support.amkette.com/webapp/xs_flow_2_speed/) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Astro A50 Gen 5 | USB base station | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| ASUS ROG / TUF wireless mice | USB / receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [G-Helper (GPL-3.0-only)](https://github.com/seerge/g-helper), [OpenMouse ASUS battery codec facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/asus) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| ATK Bitmouse battery | USB / 2.4 GHz receiver | Models not individually verified | [OpenMouse ATK Bitmouse battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/atk) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Attack Shark X11 / Delux M600 Pro passive battery | 2.4 GHz receiver input telemetry | Models not individually verified | [OpenMouse Delux battery report facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src) | Experimental; not hardware-tested in BatteryBuddy. Charging state is unavailable. |
| Audeze Maxwell / Maxwell 2 | USB / receiver | [Audeze Maxwell (USB receiver)](https://www.reddit.com/r/MouseReview/s/XUqP9TRg7v) | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl), [Maxwell firmware research (CC BY 4.0)](https://github.com/kats1123/maxwell-firmware-research) | Experimental. A connected receiver may retain the last battery level after the headset disconnects. Charging state is unavailable. |
| CompX feature mice (LAMZU and compatible models) | USB / receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [lamzu-battery-monitory (MIT)](https://github.com/Sheroune/lamzu-battery-monitory), [OpenMouse Lamzu/CRDRAKO/LunaFury/Vancer/AttackShark/RAWM facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/lamzu/index.ts) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| CompX mouse-local battery | Wired | Models not individually verified | [OpenMouse Lamzu/CompX protocol facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/lamzu) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| CompX power mice (Pulsar and compatible models) | USB / 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [python-pulsar-mouse-tool (MIT)](https://github.com/andrewrabert/python-pulsar-mouse-tool), [hitscan-battery (No license stated)](https://github.com/sopparus/hitscan-battery), [OpenMouse Atlantis/Scyrox/VGN report08 facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/lamzu/atlantis.ts) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Corsair legacy wireless headsets | USB receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Dareu report-8 battery | USB / wireless receiver | Models not individually verified | [OpenMouse Dareu report-8 facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/dareu) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| EWEADN wireless mice | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery/pull/352), [EWEADN H2 Control (GPL-3.0)](https://github.com/MuhtasimJawad/EWEADN-H2-Control), [USBDeviceBattery (No license file stated)](https://github.com/YeLuoYuXiang/USBDeviceBattery), [MouseDock (MIT)](https://github.com/late-autumn-gif/MouseDock), [MyFinder Device Battery (AGPL-3.0)](https://github.com/wushuai0802/myfinder-device-battery/issues/315) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Flydigi Vader 5 Pro | 2.4 GHz receiver | Models not individually verified | [Vader Battery Tray (MIT)](https://github.com/vermi5/VaderBatteryTray) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| G-Wolves legacy XVI battery | USB / wireless receiver | Models not individually verified | [OpenMouse G-Wolves legacy XVI protocol facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/gwolves), [G-Wolves web driver (Not stated)](https://mouse.xyz) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| G-Wolves new-protocol mice | USB / 8K receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [G-Wolves web driver (No license stated)](https://mouse.xyz), [OpenMouse G-Wolves XVI:1 protocol and descriptor fixtures (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/gwolves/xvi-new) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| G-Wolves old-protocol mice | USB / receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [G-Wolves web driver (No license stated)](https://mouse.xyz), [OpenMouse G-Wolves XVI battery protocol (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/gwolves/xvi) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| G-Wolves report-5 battery | USB / wireless receiver | Models not individually verified | [OpenMouse G-Wolves report-5 protocol facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/gwolves), [G-Wolves web driver (Not stated)](https://mouse.xyz) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| GearHub V5 receiver battery | USB receiver / Bluetooth | Models not individually verified | [OpenMouse GearHub V5 protocol facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/gearhub) | Experimental; not hardware-tested in BatteryBuddy. Charging state is unavailable. |
| Glorious classic / O3 battery | USB / 2.4 GHz receiver | Models not individually verified | [OpenMouse Glorious classic battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/vendors.ts), [glorious-ctl (reference cited by OpenMouse) (MIT)](https://github.com/louis4craft/glorious-ctl), [mxw (reference cited by OpenMouse) (MIT)](https://github.com/korkje/mxw), [Glorious-Mouse-Toolkit-Linux (reference cited by OpenMouse) (Unavailable; GitHub REST repository/license returned 404 on 2026-11-09)](https://github.com/RealCrystalNight/Glorious-Mouse-Toolkit-Linux), [GloriousBatteryMonitor (reference cited by OpenMouse) (GPL-3.0)](https://github.com/AwesomeTy18/GloriousBatteryMonitor) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Glorious Core 2 battery | USB / 2.4 GHz receiver | Models not individually verified | [OpenMouse Glorious Core 2 battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/vendors.ts) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Hitscan Xvi battery | USB / 2.4 GHz receiver | Models not individually verified | [OpenMouse Hitscan Xvi battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/hitscan) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| HyperX Cloud Alpha 2 | USB station (Controller) | Models not individually verified | [Halo Battery / user captures (MIT)](https://github.com/HeyOkay/HaloBattery/issues/26), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| HyperX Cloud II Wireless (HP) | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| HyperX Cloud III / Cloud II Core Wireless | USB receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HyperHeadset (MIT)](https://github.com/LennardKittner/HyperHeadset) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| HyperX Pulsefire Haste wireless battery | 2.4 GHz receiver | Models not individually verified | [OpenMouse HyperX battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/hyperx) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Incott 8K wireless battery | 2.4 GHz receiver input telemetry | Models not individually verified | [OpenMouse Incott protocol facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/incott/index.ts), [IncottHIDApp (protocol facts cited by OpenMouse) (MIT)](https://github.com/romkazor/IncottHIDApp), [IncottHub (independent verification/capture) (Unavailable; GitHub REST repository/license returned 404 on 2026-11-09)](https://github.com/vladidraganov/IncottHub) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| IPI / Bytech Float 88 battery | USB / wireless receiver | Models not individually verified | [OpenMouse IPI Float 88 protocol facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/bytech) | Experimental; not hardware-tested in BatteryBuddy. Charging state is unavailable. |
| Jabra Link 380 / 390 / Evolve2 65 Flex | USB / receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| K-snake X11 battery | USB / 2.4 GHz receiver | Models not individually verified | [OpenMouse K-snake X11 protocol facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/ksnake), [K-snake vendor configurator (Not stated)](https://x1a11.yjx2012.com/) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Keychron 1K mouse battery | USB / wireless receiver | Models not individually verified | [OpenMouse Keychron 1K battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/keychron) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Keychron 4K battery | USB / wireless receiver | Models not individually verified | [OpenMouse Keychron 4K battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/keychron) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Keychron 8K M6 battery | USB / Link-KM receiver | Models not individually verified | [OpenMouse Keychron 8K battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/keychron) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Keychron M5 / Ultra-Link 8K | USB / receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [keychron-battery-dkms (GPL-2.0-only)](https://github.com/csutcliff/keychron-battery-dkms) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Keychron Nape battery | USB / Link-KM receiver | Models not individually verified | [OpenMouse Keychron Nape battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/keychron) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Keychron Nordic battery | USB / wireless receiver | Models not individually verified | [OpenMouse Keychron Nordic battery facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/keychron) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Lenovo Wireless VoIP Headset | USB receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Lofree Hyzen (wireless only) | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [Lofree web driver (No license stated)](https://hyzen.lofree.tech) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Logitech G PRO X 2 / X 3 / G522 (Centurion) | USB receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl / iowi479 / thanosk / KarimTayie (GPLv3)](https://github.com/Sapd/HeadsetControl), [Solaar (GPL-2.0-or-later)](https://github.com/pwr-Solaar/Solaar) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Logitech G533 / G535 / G733 / G933 / G935 / G PRO wireless voltage | USB receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl), [g933-utils (MIT)](https://github.com/ashkitten/g933-utils) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| Logitech G930 (voltage estimate) | USB receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl), [g930-battery-percentage (MIT)](https://github.com/Roliga/g930-battery-percentage) | Experimental; please send feedback. A connected receiver may retain the last battery level after the headset disconnects. |
| Logitech HID++ 2.0 | USB / Bluetooth | Models not individually verified | [BatteryBuddy](https://github.com/Dergln/BatteryBuddy-Public), [OpenMouse HID++ ADC measurement facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/logitech/hidpp.ts) | Some models estimate battery percentage from voltage. If the device stops responding (e.g. asleep or disconnected while its receiver stays connected), its last reading is kept for up to three hours. |
| MCHOSE A5 Pro Max | USB / 1K / 4K receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [MCHOSE A5 web driver (No license file stated)](https://github.com/z750sasr/mchose-a5-pro-max-web-driver), [mchose-macos (MIT)](https://github.com/Klegus/mchose-macos) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| MCHOSE G7 | USB / receiver | Models not individually verified | [Halo Battery / kek353 captures (MIT)](https://github.com/HeyOkay/HaloBattery/issues/8), [OpenMouse K-snake battery codec (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/ksnake) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| MCHOSE M7 / L7 / A7 RealTek status | USB / Bluetooth / receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [mchose-linux / alexfrih (MIT)](https://github.com/alexfrih/mchose-linux), [OpenMouse MCHOSE V2 link IDs (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/mchose/index.ts) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| MCHOSE V3 battery | Wireless | Models not individually verified | [OpenMouse MCHOSE V3 M HUB protocol facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/mchose/v3.ts) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| MCHOSE X9 / V9 Pro | 2.4 GHz receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| moddoMOUSE battery | Wireless | Models not individually verified | [OpenMouse moddo protocol / moddoHUB-Web reference (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/docs/moddo-testing.md) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Motospeed X6 battery | Wireless | Models not individually verified | [OpenMouse X6 protocol facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/motospeed/index.ts) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Nintendo Switch Pro Controller / Joy-Con | Bluetooth | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [SDL (Zlib)](https://github.com/libsdl-org/SDL), [Switch reverse-engineering notes (No license stated)](https://github.com/dekuNukem/Nintendo_Switch_Reverse_Engineering) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| NZXT Lift Elite Wireless | USB / 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Orbital DMS battery | Wireless | Models not individually verified | [OpenMouse Orbital host protocol facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/orbital/host-protocol.js) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Pulsar Pro battery | Wireless | Models not individually verified | [OpenMouse Pulsar Pro protocol (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/pulsar/pulsar-pro-hid.ts) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Pulsar XS-1 battery | Wireless | Models not individually verified | [OpenMouse Pulsar XS-1 protocol (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/pulsar/pulsar-xs1-hid.ts) | Experimental; not hardware-tested in BatteryBuddy. Charging state is unavailable. |
| Rapoo VT battery feature | Wireless | Models not individually verified | [OpenMouse Rapoo status collection facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/rapoo/index.ts) | Experimental. Receiver-buffered readings can lag or remain stale while the mouse is idle; they do not prove mouse connectivity. Charging state may be unavailable. |
| Rapoo VT battery GET_REPORT | Wireless | Models not individually verified | [OpenMouse Rapoo VT9 Pro captures and mousectl reference (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/docs/rapoo-vt9-pro-testing.md) | Experimental. Receiver-buffered readings can lag or remain stale while the mouse is idle; they do not prove mouse connectivity. Charging state may be unavailable. |
| Razer wireless keyboards (allowlisted models) | USB / HyperSpeed receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [OpenRazer (GPL-2.0-or-later)](https://github.com/openrazer/openrazer) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Razer wireless mice (allowlisted models) | USB / HyperSpeed receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [OpenRazer (GPL-2.0-or-later)](https://github.com/openrazer/openrazer), [OpenMouse Razer exact device/transaction facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/razer/devices.ts) | Experimental; not hardware-tested in BatteryBuddy. Some models report approximate battery levels or no charging state. |
| Redragon M690 PRO battery | Wireless | Models not individually verified | [OpenMouse M690 protocol and captures (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/redragon/m690-pro.ts), [libratbag driver-sinowealth (MIT)](https://github.com/libratbag/libratbag/blob/master/src/driver-sinowealth.c) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Ryunix KYU PRO MX1 battery | Wireless | Models not individually verified | [OpenMouse KYU PRO MX1 telemetry facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/ryunix/kyu-pro-mx1.ts) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Sony DualShock 4 / DualSense / Edge (passive full reports) | USB / Bluetooth | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [Linux (GPL-2.0-or-later)](https://github.com/torvalds/linux/blob/master/drivers/hid/hid-playstation.c) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| Sony INZONE H5 / H9 II | USB receiver | Models not individually verified | [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| Steam Controller (2025) puck slots | USB puck | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [SDL (Zlib)](https://github.com/libsdl-org/SDL), [sc2-research (MIT)](https://github.com/CouchTurtle/sc2-research) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |
| SteelSeries Aerox 3 / 5 / 9 Wireless | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [rivalcfg (WTFPL)](https://github.com/flozz/rivalcfg), [alloyctl (GPLv2)](https://github.com/szymonwilczek/alloyctl) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| SteelSeries Arctis 1 / 7 / 9 / Nova Pro Wireless | USB receiver / base station | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl), [Linux (GPL-2.0-or-later)](https://github.com/torvalds/linux/blob/master/drivers/hid/hid-steelseries-arctis.c) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| SteelSeries Arctis Nova 7 (1038:22A1) | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl), [Linux (GPL-2.0-or-later)](https://github.com/torvalds/linux/blob/master/drivers/hid/hid-steelseries-arctis.c) | Experimental; please send feedback. A connected receiver may retain the last battery level after the headset disconnects. |
| SteelSeries model-specific battery | Wireless | Models not individually verified | [OpenMouse model-specific SteelSeries facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/steelseries), [rivalcfg protocol reference (WTFPL)](https://github.com/flozz/rivalcfg) | Experimental; not hardware-tested in BatteryBuddy. Some models report approximate battery levels or no charging state. |
| SteelSeries Nova / Arctis 7+ / GameBuds | 2.4 GHz receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [HeadsetControl (GPLv3)](https://github.com/Sapd/HeadsetControl) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| TEEvolution Terra Pro | USB / receiver | [TEEvolution Terra Pro (USB)](https://www.reddit.com/r/MouseReview/s/XUqP9TRg7v) | [OpenMouse mouse-protocol (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/teevolution), [TeevoLink factory protocol reference (No license stated)](https://github.com/xilu7/teevolink/tree/26e0734589c8e09f1757f2cb6c809645fe06f61e/src/sdk) | Experimental. Receiver-buffered readings can lag or remain stale while the mouse is idle; they do not prove mouse connectivity. Charging state may be unavailable. |
| Valkyrie read-only identity battery | Wireless | Models not individually verified | [OpenMouse measured Valkyrie identity protocol (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/docs/valkyrie-m3-series.md) | Experimental; not hardware-tested in BatteryBuddy. Charging state and receiver-side disconnect detection are unavailable. |
| VAXEE battery | Wireless | Models not individually verified | [OpenMouse VAXEE Control Center v3.6 facts (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/vaxee/index.ts) | Experimental; please send feedback. Battery percentage is approximate; not hardware-tested in BatteryBuddy. |
| WLmouse feature-protocol mice | USB / receiver | Models not individually verified | [Halo Battery (MIT)](https://github.com/HeyOkay/HaloBattery), [mouse-battery-tray / len0c (MIT)](https://github.com/incconutwo/mouse-battery-tray), [OpenMouse WLmouse exact runtime table (AGPL-3.0-or-later)](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/vendors.ts) | Experimental; please send feedback. Not hardware-tested in BatteryBuddy. |

Bluetooth depends on a Windows battery property; XInput reports coarse levels.
Logitech probing is limited to the existing HID++ interface allowlist and devices
that respond to its protocol. Neither recognition nor an entry promises a reading
for every device.
Responsive Logitech devices are queried on each refresh. The three-hour limit is a
last-known-value fallback for a device that stops responding, including sleep or a
disconnected wireless device behind a still-connected receiver; it is not a normal
three-hour delay in receiving battery updates. Retained values keep their original
last-contact time.

## Teevolution Terra Pro

The experimental `teevolution-terra-pro` family covers only VID `3554` with PIDs
`F520`, `F523`, `F5BB` and `F522`. Only `F523` is explicitly identified upstream as
the RapidSync 8K dongle; the other product IDs are not assigned invented wired or
wireless labels. No other Teevolution model is claimed supported.

Before any query, Windows descriptor capabilities must prove that the input
and output controls use only report ID `08`, with exactly 17-byte native reports.
The separate RapidSync LCD collection (`FF08:0002`, report `0A`) is excluded.
The main collection's usage page and USB interface number are not established by
the sources, so neither is guessed. Missing report-ID evidence fails closed.

BatteryBuddy opens this collection **exclusively**: close an active TeevoLink
browser tab/configurator if it prevents access. There is no shared-handle
fallback. The adapter flushes old input before each request, sends the factory's
plain online-status GET `03`, and only on an online reply sends battery GET `04`.
Responses must match the report ID, command, status and additive checksum.
Each receive is bounded to 1200 ms, 100-ms read slices and 32 unrelated reports;
cancellation and write/flush failures propagate normally. No host-control
enable/disable, clock, pairing, firmware, configuration or flash writes occur.

A validated 0% is real; values above 100 mean unknown, not zero or a voltage
estimate. Charging is read only from its documented 0/1 field. An offline
receiver suppresses the battery query. These checks do **not** prove a fresh
radio sample: firmware may still buffer old telemetry while idle, report online
incorrectly, or disconnect between queries. Hardware verification remains
pending; the existing localized receiver-buffer caveat is shown in DeviceList.
No software-cached percentage is relabeled as a new reading.

Primary references:
- [OpenMouse protocol and codec](https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/teevolution)
  (AGPL-3.0-or-later); the
  [driver](https://github.com/OpenMouse-Project/mouse-protocol/blob/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/drivers/teevolution/hid.ts)
  also documents the report-ID selection.
- [TeevoLink factory SDK](https://github.com/xilu7/teevolink/blob/26e0734589c8e09f1757f2cb6c809645fe06f61e/src/sdk/dev_HIDHandle_05_27.js)
  (no license stated), whose battery path uses the zero-parameter online getter.
  Public availability is not a source reuse license: only interoperability
  facts inform this independent C# implementation; no factory code is bundled.

OpenMouse's full configurator wraps queries in an online-request enable/disable
pair not corroborated by the factory battery path. That wrapper is deliberately
omitted. A firmware variant requiring it will remain unknown rather than trigger
an automatic mode-changing fallback.

## Exact profiles and important limits

The expanded adapters use allowlisted VID/PID and collection/report profiles, not
device-name guesses or vendor-wide probing. Existing generic Windows, Bluetooth
and XInput providers remain enabled. The original Logitech interface allowlist is
unchanged except that exact headset profiles now own their matching interface;
they are not polled concurrently by the generic HID++ provider.

The profiles below include experimental variants which have not been tested on
BatteryBuddy hardware. Minimum report sizes are bounded at 1024 bytes; feature
lengths marked exact must match exactly. An unrestricted usage page still needs
an explicit product plus a vendor-page/interface or exact feature descriptor,
or Teevolution's additional descriptor-proven report-ID guard described above.
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

Build Core, then run `tools\update-protocol-catalog.ps1` to regenerate the table and
profile list. Tests enforce the table's exact parity with DeviceList.

<details>
<summary>Executable VID/PID, HID collection and length allowlists</summary>

<!-- profile-list:start -->
Exact executable profiles (generated from the same catalog; `*` means not restricted by that field):

- `8bitdo-passive`: `2DC8:6000,6100,6001,6101,6003,6006,6009`; usage `0001:0004`; interface `*`; input/output `15/0`, feature `-` (minimum).
- `8bitdo-passive`: `2DC8:6000,6100,6001,6101,6003,6006,6009`; usage `0001:0005`; interface `*`; input/output `15/0`, feature `-` (minimum).
- `am-infinity`: `3151:5007`; usage `FFFF:0002`; interface `*`; input/output `0/0`, feature `65` (minimum).
- `astro-a50-gen5`: `046D:0B1C`; usage `FF32:0074`; interface `*`; input/output `9/64`, feature `-` (minimum).
- `asus-mouse`: `0B05:1A72,1A70,1B0C,1B0A,197F,197D,1A1A,1A18,1A94,1A92,1A68,1A66,1979,1977,19F4,1A8D,1AF5,1AF3,1C57,1C56,18E5,18E3,1960,195E,1A59,1A57,1908,1906,1949,1947`; usage `*:*`; interface `0`; input/output `10/65`, feature `-` (minimum).
- `audeze-maxwell`: `3329:4B18,4B19`; usage `FF13:0001`; interface `0`; input/output `62/62`, feature `-` (exact).
- `audeze-maxwell`: `3329:4B28,4B29`; usage `FF13:0001`; interface `5`; input/output `62/62`, feature `-` (exact).
- `audeze-maxwell`: `3329:4B1A`; usage `FF13:0001`; interface `*`; input/output `62/62`, feature `-` (exact).
- `corsair-legacy`: `1B1C:1B27,0A14,0A16,0A17,0A1D,0A1A,1B2A,1B23,1B29,0A55,0A51,0A52,0A38,0A4F,0A2B,0A75,0A56`; usage `FFC5:0001`; interface `3`; input/output `5/2`, feature `-` (minimum).
- `eweadn-wireless-mice`: `372E:1026`; usage `FF00:0001`; interface `*`; input/output `0/0`, feature `64` (exact; feature length exact; single feature report ID 03).
- `eweadn-wireless-mice`: `372E:1026`; usage `FF06:0001`; interface `*`; input/output `0/0`, feature `64` (exact; feature length exact; single feature report ID 03).
- `eweadn-wireless-mice`: `089D:062F`; usage `FF01:0010`; interface `2`; input/output `33/33`, feature `-` (exact; single input/output report ID 00).
- `eweadn-wireless-mice`: `A8A5:2255`; usage `FF01:0010`; interface `2`; input/output `65/65`, feature `-` (exact; product string EWEADN X23SE; unique nonzero input/output report IDs).
- `eweadn-wireless-mice`: `A8A5:2255`; usage `FF01:0010`; interface `*`; input/output `65/65`, feature `-` (exact; single input/output report ID 00; product string EWEADN A7).
- `eweadn-wireless-mice`: `A8A5:2255`; usage `FF01:0010`; interface `*`; input/output `65/65`, feature `-` (exact; single input/output report ID 00; product string EWEADN A7Pro).
- `flydigi-vader-5-pro`: `37D7:2401`; usage `FFA0:0001`; interface `1`; input/output `32/32`, feature `-` (exact; single input/output report ID 00).
- `gwolves-new`: `33E4:3854,5418,5419,5219,4718,4719,5618,5619,3619,3519,2719,4219,3808,3817,6808,6817,5608,5617,3608,3617`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact).
- `gwolves-old`: `33E4:3908,3917,7904,7913,3708,3717,5308,5317,7704,7713,3508,3517,7908,7917,5808,5817,5807,2708,2717,5408,5417,5407,5708,5717,5707,5908,5917,5907,7204,7203,7708,7717,5804,5803,5404,5403,5704,5703,5904,5903`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact).
- `hyperx-alpha-2`: `03F0:08BE`; usage `FF13:FF00`; interface `*`; input/output `7/64`, feature `-` (minimum).
- `hyperx-cloud-ii-hp`: `03F0:0696,018B`; usage `FF90:0303`; interface `*`; input/output `20/52`, feature `-` (minimum).
- `hyperx-cloud-iii`: `03F0:05B7,0C9D,0795,0995`; usage `FF13:0001`; interface `*`; input/output `5/0`, feature `62` (minimum).
- `hyperx-cloud-iii`: `03F0:05B7,0C9D,0795,0995`; usage `FF13:0001`; interface `*`; input/output `5/62`, feature `-` (minimum).
- `jabra-gnp`: `0B0E:24C7,2519`; usage `FF00:0001`; interface `3`; input/output `64/64`, feature `-` (minimum).
- `jabra-gnp`: `0B0E:2E57`; usage `FF00:0005`; interface `3`; input/output `64/64`, feature `-` (minimum).
- `keychron-status`: `3434:D028,D048`; usage `*:*`; interface `4`; input/output `21/0`, feature `64` (minimum).
- `lamzu-maya-x`: `373E:0046,0047,0021,0022,003A,003B,006B,0033,0084,0066,0067,0098,0099`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseLZBatteryProtocols.ReadCompx).
- `lamzu-maya-x`: `373E:001E,001C`; usage `FFFF:*`; interface `2`; input/output `0/0`, feature `65` (minimum).
- `lamzu-maya-x`: `37B0:0009,000F,0010,0007,000D,000E,0001,0003`; usage `FFFF:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseLZBatteryProtocols.ReadCompx).
- `lenovo-voip`: `17EF:A07D`; usage `FF07:0222`; interface `3`; input/output `61/0`, feature `61` (minimum).
- `lofree-hyzen`: `388D:0025`; usage `FF1C:0092`; interface `*`; input/output `32/32`, feature `-` (minimum).
- `logitech-centurion`: `046D:0AF7,0B18,0B45`; usage `FFA0:0001`; interface `3`; input/output `64/64`, feature `-` (minimum).
- `logitech-g930`: `046D:0A1F`; usage `FF00:0001`; interface `*`; input/output `0/0`, feature `64` (minimum).
- `logitech-headset-voltage`: `046D:0AC4`; usage `000C:0001`; interface `3`; input/output `20/20`, feature `-` (exact).
- `logitech-headset-voltage`: `046D:0A66,0A5B,0A87,0AB5,0AFE,0B1F,0ABA,0AFB,0AFC`; usage `FF43:0202`; interface `*`; input/output `20/20`, feature `-` (exact).
- `logitech-hidpp`: `046D:*`; usage `FF00:*`; interface `*`; input/output `20/20`, feature `-` (exact).
- `logitech-hidpp`: `046D:*`; usage `FF43:*`; interface `*`; input/output `20/20`, feature `-` (exact).
- `mchose-a5`: `2023:F013,F015,F019`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact).
- `mchose-g7`: `A8A5:2255`; usage `FF01:0010`; interface `*`; input/output `10/65`, feature `-` (minimum; product string YJX-CHIP).
- `mchose-headsets`: `291D:385D`; usage `FF90:0001`; interface `0`; input/output `64/64`, feature `-` (minimum).
- `mchose-headsets`: `3837:6045`; usage `FF90:0001`; interface `0`; input/output `64/64`, feature `-` (minimum).
- `mchose-realtek`: `3837:100A,1020`; usage `FF01:0001`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact).
- `mchose-realtek`: `3837:100B`; usage `FF01:0001`; interface `2`; input/output `0/0`, feature `65` (minimum).
- `mchose-realtek`: `5253:1020,0031`; usage `FF01:*`; interface `*`; input/output `0/0`, feature `65` (minimum).
- `nintendo-status`: `057E:2006,2007,2009`; usage `0001:0004`; interface `*`; input/output `3/49`, feature `-` (minimum).
- `nintendo-status`: `057E:2006,2007,2009`; usage `0001:0005`; interface `*`; input/output `3/49`, feature `-` (minimum).
- `nzxt-lift`: `1E71:2101,2129`; usage `FFCA:0001`; interface `*`; input/output `27/64`, feature `-` (minimum).
- `openmouse-ajazz-aj200p`: `363C:ED03,ED05,ED06,ED00,ED07,ED08,FC08,ED09,ED0A,FC0A,ED0B,ED0C,FC0C,ED1D,ED1E,FC1E,ED1B,ED1C,FC1C,ED25,ED26,FC26`; usage `FFA0:0002`; interface `*`; input/output `0/128`, feature `-` (minimum; reader OpenMouseAKBatteryProtocols.ReadAj200p).
- `openmouse-ajazz-nj07-nj08`: `A8A4:2157,2158,2167,2168,2177,2178`; usage `FF01:0010`; interface `*`; input/output `0/64`, feature `-` (minimum; reader OpenMouseAKBatteryProtocols.ReadAjazz).
- `openmouse-ajazz-nj07-nj08`: `A8A5:2157,2158,2167,2168,2177,2178`; usage `FF01:0010`; interface `*`; input/output `0/64`, feature `-` (minimum; reader OpenMouseAKBatteryProtocols.ReadAjazz).
- `openmouse-amkette-xs`: `04F3:026D,026E,026F`; usage `000A:0000`; interface `*`; input/output `7/0`, feature `1` (minimum; reader OpenMouseAKBatteryProtocols.ReadAmkette).
- `openmouse-atk-bitmouse`: `373B:1154,1155`; usage `FF05:0001`; interface `*`; input/output `0/64`, feature `-` (minimum; reader OpenMouseAKBatteryProtocols.ReadBitmouse).
- `openmouse-attackshark-x11-passive`: `1D57:FA60`; usage `000A:*`; interface `2`; input/output `5/0`, feature `-` (minimum; reader OpenMouseAKBatteryProtocols.ReadAttackSharkX11).
- `openmouse-bytech-float88`: `372E:1014,1015`; usage `FF00:0001`; interface `*`; input/output `0/0`, feature `64` (minimum; feature length exact; reader OpenMouseAKBatteryProtocols.ReadBytech).
- `openmouse-compx-target-zero`: `373E:006A,0032,0054`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseLZBatteryProtocols.ReadCompx).
- `openmouse-dareu-report8`: `260D:1114,1117`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (exact; reader OpenMouseAKBatteryProtocols.ReadDareu).
- `openmouse-gearhub-v5`: `3151:402C`; usage `FF55:0202`; interface `*`; input/output `0/66`, feature `-` (minimum; reader OpenMouseAKBatteryProtocols.ReadGearHub).
- `openmouse-gearhub-v5`: `3151:402D`; usage `FFFF:0002`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseAKBatteryProtocols.ReadGearHub).
- `openmouse-glorious-classic`: `3794:A312,A300`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseAKBatteryProtocols.ReadGloriousClassic).
- `openmouse-glorious-classic`: `258A:2022,2023,2024,2025,2033,2039,2027,2031,2034`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseAKBatteryProtocols.ReadGloriousClassic).
- `openmouse-glorious-core2`: `258A:201B,2035,201C,2036`; usage `FFFF:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseAKBatteryProtocols.ReadGloriousCore2).
- `openmouse-gwolves-report5`: `33E4:5618,3854`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `0` (exact; feature length exact; reader OpenMouseAKBatteryProtocols.ReadGWolvesReport5).
- `openmouse-gwolves-report5`: `33E4:2619`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (exact; reader OpenMouseAKBatteryProtocols.ReadGWolvesReport5).
- `openmouse-gwolves-xvi`: `33E4:001A,00AA`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseAKBatteryProtocols.ReadGWolvesXvi).
- `openmouse-hitscan-xvi`: `3770:0136,0194,0300`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseAKBatteryProtocols.ReadHitscanXvi).
- `openmouse-hyperx-haste`: `03F0:028E`; usage `FF00:0001`; interface `*`; input/output `65/65`, feature `-` (exact; reader OpenMouseAKBatteryProtocols.ReadHyperX).
- `openmouse-incott-8k`: `093A:522C,622C`; usage `FF05:*`; interface `*`; input/output `3/0`, feature `-` (minimum; reader OpenMouseAKBatteryProtocols.ReadIncott).
- `openmouse-keychron-1k`: `3434:D033`; usage `008C:0001`; interface `*`; input/output `0/0`, feature `21` (minimum; feature length exact; reader OpenMouseAKBatteryProtocols.ReadKeychron1k).
- `openmouse-keychron-4k`: `3434:D040,D046,D03C,D037,D041,D045`; usage `FF0A:0001`; interface `*`; input/output `0/65`, feature `-` (exact; reader OpenMouseAKBatteryProtocols.ReadKeychron4k).
- `openmouse-keychron-8k`: `3434:D060,D029`; usage `FFC1:0001`; interface `*`; input/output `0/64`, feature `-` (exact; reader OpenMouseAKBatteryProtocols.ReadKeychron8k).
- `openmouse-keychron-nape`: `3434:0440,D026,D029`; usage `FF60:0061`; interface `*`; input/output `0/33`, feature `-` (exact; reader OpenMouseAKBatteryProtocols.ReadKeychronNape).
- `openmouse-keychron-nordic`: `3434:D077,D05B,D078`; usage `FF0A:0001`; interface `*`; input/output `0/65`, feature `-` (exact; reader OpenMouseAKBatteryProtocols.ReadKeychronNordic).
- `openmouse-ksnake-x11`: `A8A4:2255`; usage `FF01:0010`; interface `*`; input/output `0/65`, feature `-` (minimum; reader OpenMouseAKBatteryProtocols.ReadKsnake).
- `openmouse-mchose-v3`: `3837:4035,4026,4034,4027,4028,4030,4031,4032,4033,4037,4038,4036,402A,4029,1014,1018,1016`; usage `FF01:0001`; interface `*`; input/output `64/64`, feature `-` (exact; reader OpenMouseLZBatteryProtocols.ReadMchoseV3).
- `openmouse-moddo`: `2FE3:0001,0002`; usage `00FF:0001`; interface `*`; input/output `0/0`, feature `12` (minimum; reader OpenMouseLZBatteryProtocols.ReadModdo).
- `openmouse-moddo`: `2FE3:0001,0002`; usage `00FF:0002`; interface `*`; input/output `0/0`, feature `12` (minimum; reader OpenMouseLZBatteryProtocols.ReadModdo).
- `openmouse-motospeed`: `0BDA:FFE0`; usage `FFC1:*`; interface `*`; input/output `21/61`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadMotospeed).
- `openmouse-orbital`: `1915:080B,0746`; usage `FF0A:0001`; interface `*`; input/output `65/65`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadOrbital).
- `openmouse-pulsar-pro`: `3710:5405`; usage `0001:0000`; interface `*`; input/output `8/64`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadPulsarPro).
- `openmouse-pulsar-xs1`: `3710:3404,3409,3410,5402,5403`; usage `FFFF:0001`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseLZBatteryProtocols.ReadPulsarXs1).
- `openmouse-rapoo-config`: `24AE:1205,4405`; usage `FF00:000E`; interface `*`; input/output `32/32`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadRapoo).
- `openmouse-rapoo-feature`: `24AE:1205,4405`; usage `FF0B:0104`; interface `*`; input/output `0/0`, feature `10` (minimum; reader OpenMouseLZBatteryProtocols.ReadRapooFeature).
- `openmouse-redragon-m690`: `258A:002E,002F`; usage `FF00:0001`; interface `1`; input/output `0/0`, feature `520` (minimum; feature length exact; reader OpenMouseLZBatteryProtocols.ReadRedragonM690).
- `openmouse-ryunix`: `04F3:026E,026F`; usage `000A:00C7`; interface `*`; input/output `7/0`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadRyunix).
- `openmouse-steelseries`: `1038:172B,1726`; usage `*:*`; interface `0`; input/output `4/3`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadSteelSeries).
- `openmouse-steelseries`: `1038:184A,1848,1842,1840`; usage `*:*`; interface `3`; input/output `3/2`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadSteelSeries).
- `openmouse-steelseries`: `1038:1830`; usage `*:*`; interface `3`; input/output `4/3`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadSteelSeries).
- `openmouse-steelseries`: `1038:1892,1890`; usage `FFC0:*`; interface `*`; input/output `3/2`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadSteelSeries).
- `openmouse-steelseries`: `1038:183A,187A,1854,185E,1862,185A,1876`; usage `FFC0:*`; interface `3`; input/output `3/2`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadSteelSeries).
- `openmouse-valkyrie`: `248A:5D2E`; usage `0001:0000`; interface `2`; input/output `32/0`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadValkyrie).
- `openmouse-valkyrie`: `249A:5C2F`; usage `0001:0000`; interface `2`; input/output `32/0`, feature `-` (minimum; reader OpenMouseLZBatteryProtocols.ReadValkyrie).
- `openmouse-vaxee`: `3057:1001,1002,1003,1004,1005,1006,1007,1008,1009,1010,1011,1012,1013,0005,2001,1014,2002,1015`; usage `FF05:0001`; interface `*`; input/output `0/0`, feature `64` (minimum; feature length exact; reader OpenMouseLZBatteryProtocols.ReadVaxee).
- `playstation-passive`: `054C:05C4,09CC,05C5,0BA0`; usage `0001:0004`; interface `*`; input/output `31/0`, feature `-` (minimum).
- `playstation-passive`: `054C:0CE6,0DF2`; usage `0001:0004`; interface `*`; input/output `55/0`, feature `-` (minimum).
- `playstation-passive`: `054C:05C4,09CC,05C5,0BA0`; usage `0001:0005`; interface `*`; input/output `31/0`, feature `-` (minimum).
- `playstation-passive`: `054C:0CE6,0DF2`; usage `0001:0005`; interface `*`; input/output `55/0`, feature `-` (minimum).
- `pulsar-power`: `3554:FB56,FB57`; usage `FF02:*`; interface `*`; input/output `17/17`, feature `-` (exact; reader OpenMouseLZBatteryProtocols.ReadCompxPower).
- `pulsar-power`: `3554:F54B,F575,F548,F577,F576,F549,F50F,F50D,F510,F515,F517,F5F7`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (exact; reader OpenMouseLZBatteryProtocols.ReadCompxPower).
- `pulsar-power`: `3554:F508,F507,F58F,F58A,F58C`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (minimum).
- `pulsar-power`: `3710:5406,3414`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (minimum).
- `pulsar-power`: `373B:1085`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (minimum).
- `pulsar-power`: `3770:0200,0100`; usage `FF02:0002`; interface `*`; input/output `17/17`, feature `-` (minimum).
- `razer-keyboard`: `1532:0290,0296,02D5,025C`; usage `*:*`; interface `2`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 9F).
- `razer-keyboard`: `1532:025A`; usage `*:*`; interface `2`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 3F).
- `razer-keyboard`: `1532:0271,02BA`; usage `*:*`; interface `3`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 9F).
- `razer-keyboard`: `1532:0292,0298,0258,02B9,02D7`; usage `*:*`; interface `3`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 1F).
- `razer-mouse`: `1532:001F,0024,0025,0032,003E,003F,0044,0045,007A,007B,0083,0039`; usage `*:*`; interface `*`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction FF).
- `razer-mouse`: `1532:0062,006F,0070,0077,0080,0086,0088,008F,0090,0094,009A,009C,009E,009F,00A5,00A6,00A7,00A8,00AA,00AB,00AF,00B0,00B3,00B4,00B6,00B7,00C2,00C3,00B8,00B9,00BE,00BF,00C0,00C1,00C4,00C5,00C7,00C8,00CC,00CD,00D0,00D1,00D3,00D4,00D6,00D7,00A4,00DE,00DF,00EF,00F0,00E5,00E6`; usage `*:*`; interface `*`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 1F).
- `razer-mouse`: `1532:0059,005A,0072,0073,007C,007D`; usage `*:*`; interface `*`; input/output `0/0`, feature `91` (minimum; feature length exact; transaction 3F).
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
- `teevolution-terra-pro`: `3554:F520,F523,F5BB,F522`; usage `*:*`; interface `*`; input/output `17/17`, feature `-` (exact; single input/output report ID 08).
- `wlmouse-feature`: `36A7:A887,A868,A880`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact).
- `wlmouse-feature`: `36A7:A860,A861,A863,A864,A866,A867,A869,A870,A871,A872,A873,A874,A875,A878,A879,A881,A882,A883,A884,A885,A886`; usage `*:*`; interface `*`; input/output `0/0`, feature `65` (minimum; feature length exact; reader OpenMouseLZBatteryProtocols.ReadCompx).
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
establish BatteryBuddy support. Before adding an adapter, trace and retain its
applicable source and license notices, prove its requests are safe, and test the
exact model and connection. Fragile candidates remain outside this catalog until
their descriptor and response evidence is sufficiently verified.

### Flydigi Vader 5 Pro and EWEADN wireless mice

The Vader 5 Pro reader is limited to `37D7:2401`, USB interface 1,
`FFA0:0001`, 32-byte input/output reports and report ID 0. It sends only the
documented `GET_INFO` battery request; the reply must identify device 130 and
firmware `0x7141` or newer. The controller reports seven ordinal levels mapped to
10, 25, 40, 55, 70, 85 and 100 percent, so those percentages are estimates rather
than precise measurements. No dock, lighting, profile or configuration request
is used.

EWEADN receiver paths are separated by model and descriptor:

- H2 / S9 uses `089D:062F`, interface 2, `FF01:0010` and 33-byte reports.
  The `0x30` device-info reply must identify `M302` (H2) or `M156` (S9), have a
  valid battery/charge field and report the mouse connected. An asleep or
  disconnected mouse yields no current reading.
- A7 / A7 Pro uses `A8A5:2255`, exact product strings `EWEADN A7` /
  `EWEADN A7Pro`, `FF01:0010`, 65-byte reports and report ID 0. The product
  string prevents confusion with the shared-ID MCHOSE G7 and X23SE profiles.
- X23SE requires the exact `EWEADN X23SE` product string, interface 2,
  `FF01:0010`, 65-byte reports and unique, descriptor-derived, nonzero report
  IDs. Two matching battery replies are required. The source documents the
  query layout but no successful X23SE battery response on real hardware; this
  profile is experimental and especially needs user confirmation.
- S9 Pro 8K uses `372E:1026`, `FF00:0001` or `FF06:0001`, and a unique
  descriptor-proven feature report ID 3 with a 64-byte report. Only valid
  16-byte and 64-byte replies with an online device and level 1–100 are accepted.

No Vader or EWEADN model has been tested on BatteryBuddy hardware. EWEADN A5
receiver support and other X23 aliases remain excluded for lack of a sufficiently
verified receiver response.
