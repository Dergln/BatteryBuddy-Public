using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BatteryBuddy.Core.Providers
{
    public sealed class DeviceCatalogEntry
    {
        public string Id { get; }
        public string Name { get; }
        public string Connection { get; }
        public string SourceName { get; }
        public string SourceUrl { get; }
        public IReadOnlyList<DeviceCatalogSource> Sources { get; }
        public string CommentsKey { get; }

        internal DeviceCatalogEntry(string id, string name, string connection, string commentsKey, params DeviceCatalogSource[] sources)
        {
            Id = id;
            Name = name;
            Connection = connection;
            CommentsKey = commentsKey;
            Sources = Array.AsReadOnly(sources);
            SourceName = sources[0].DisplayName;
            SourceUrl = sources[0].Url;
        }
    }

    public sealed class DeviceCatalogSource
    {
        public string Name { get; }
        public string Url { get; }
        public string License { get; }
        public string DisplayName => License == null ? Name : $"{Name} ({License})";

        internal DeviceCatalogSource(string name, string url, string license)
        {
            Name = name;
            Url = url;
            License = license;
        }
    }

    public sealed class CustomPollingProfile
    {
        public string ProtocolId { get; }
        public ushort VendorId { get; }
        public ushort? ProductId { get; }
        public ushort? UsagePage { get; }
        public ushort? Usage { get; }
        public int InputReportLength { get; }
        public int OutputReportLength { get; }
        public int? FeatureReportLength { get; }
        public int? InterfaceNumber { get; }
        public string DeviceName { get; }
        public bool MinimumReportLengths { get; }
        public byte? TransactionId { get; }
        public bool ExactFeatureLength { get; }
        public string ReaderId { get; }
        public byte? ReportId { get; }

        internal CustomPollingProfile(string protocolId, ushort vendorId, ushort? productId, ushort? usagePage,
            ushort? usage, int inputLength, int outputLength, int? featureLength = null, int? interfaceNumber = null,
            string deviceName = null, bool minimumReportLengths = false, byte? transactionId = null,
            bool exactFeatureLength = false, string readerId = null, byte? reportId = null)
        {
            ProtocolId = protocolId;
            VendorId = vendorId;
            ProductId = productId;
            UsagePage = usagePage;
            Usage = usage;
            InputReportLength = inputLength;
            OutputReportLength = outputLength;
            FeatureReportLength = featureLength;
            InterfaceNumber = interfaceNumber;
            DeviceName = deviceName;
            MinimumReportLengths = minimumReportLengths;
            TransactionId = transactionId;
            ExactFeatureLength = exactFeatureLength;
            ReaderId = readerId;
            ReportId = reportId;
        }

        public bool Matches(string protocolId, ushort vendorId, ushort productId, ushort usagePage, ushort usage,
            int inputLength, int outputLength, int featureLength = 0,
            IReadOnlyList<byte> inputReportIds = null, IReadOnlyList<byte> outputReportIds = null)
            => ProtocolId == protocolId && VendorId == vendorId && (!UsagePage.HasValue || UsagePage.Value == usagePage)
                && (!ProductId.HasValue || ProductId.Value == productId) && (!Usage.HasValue || Usage.Value == usage)
                && (MinimumReportLengths
                    ? inputLength >= InputReportLength && outputLength >= OutputReportLength
                        && inputLength <= 1024 && outputLength <= 1024
                    : InputReportLength == inputLength && OutputReportLength == outputLength)
                && (!FeatureReportLength.HasValue || (MinimumReportLengths && !ExactFeatureLength
                    ? featureLength >= FeatureReportLength.Value && featureLength <= 1024
                    : FeatureReportLength.Value == featureLength))
                && (!ReportId.HasValue || (inputReportIds?.Count == 1 && inputReportIds[0] == ReportId.Value
                    && outputReportIds?.Count == 1 && outputReportIds[0] == ReportId.Value));
    }

    /// <summary>Implemented battery sources, not a list of individually verified device models.</summary>
    public static class DeviceCatalog
    {
        public const string LogitechHidpp = "logitech-hidpp";
        public const string SteelSeriesNova7 = "steelseries-nova7-22a1";
        public const string MchoseHeadsets = "mchose-headsets";
        public const string CorsairLegacy = "corsair-legacy";
        public const string HyperXCloudII = "hyperx-cloud-ii-hp";
        public const string LamzuMayaX = "lamzu-maya-x";
        public const string SteelSeriesNova = "steelseries-nova-status";
        public const string Pulsar = "pulsar-power";
        public const string Nzxt = "nzxt-lift";
        public const string RazerMouse = "razer-mouse";
        public const string RazerKeyboard = "razer-keyboard";
        public const string Wlmouse = "wlmouse-feature";
        public const string GwolvesNew = "gwolves-new";
        public const string GwolvesOld = "gwolves-old";
        public const string AsusMouse = "asus-mouse";
        public const string Astro = "astro-a50-gen5";
        public const string Keychron = "keychron-status";
        public const string AmInfinity = "am-infinity";
        public const string HyperXCloudIII = "hyperx-cloud-iii";
        public const string HyperXAlpha2 = "hyperx-alpha-2";
        public const string SteelSeriesClassic = "steelseries-classic";
        public const string SteelSeriesAerox = "steelseries-aerox";
        public const string PlayStation = "playstation-passive";
        public const string EightBitDo = "8bitdo-passive";
        public const string Nintendo = "nintendo-status";
        public const string Steam = "steam-passive";
        public const string LogitechHeadsets = "logitech-headset-voltage";
        public const string LogitechG930 = "logitech-g930";
        public const string SonyInzone = "sony-inzone";
        public const string Jabra = "jabra-gnp";
        public const string LenovoVoip = "lenovo-voip";
        public const string MchoseRealtek = "mchose-realtek";
        public const string MchoseG7 = "mchose-g7";
        public const string MchoseA5 = "mchose-a5";
        public const string Lofree = "lofree-hyzen";
        public const string LogitechCenturion = "logitech-centurion";
        public const string AudezeMaxwell = "audeze-maxwell";
        public const string Teevolution = "teevolution-terra-pro";

        private static readonly DeviceCatalogSource BatteryBuddySource =
            new DeviceCatalogSource("BatteryBuddy", "https://github.com/Dergln/BatteryBuddy-Public", null);

        private static readonly ReadOnlyCollection<DeviceCatalogEntry> Entries =
            Array.AsReadOnly(new[]
            {
                new DeviceCatalogEntry("system", "Windows system battery", "Windows",
                    null, BatteryBuddySource),
                new DeviceCatalogEntry("bluetooth", "Windows Bluetooth", "Bluetooth",
                    null, BatteryBuddySource),
                new DeviceCatalogEntry("xinput", "XInput", "XInput",
                    null, BatteryBuddySource),
                new DeviceCatalogEntry(LogitechHidpp, "Logitech HID++ 2.0", "USB / Bluetooth",
                    "devicelist.comment.logitech", BatteryBuddySource),
                new DeviceCatalogEntry(SteelSeriesNova7, "SteelSeries Arctis Nova 7 (1038:22A1)", "2.4 GHz receiver",
                    "devicelist.comment.nova7",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3"),
                    new DeviceCatalogSource("Linux", "https://github.com/torvalds/linux/blob/master/drivers/hid/hid-steelseries-arctis.c", "GPL-2.0-or-later")),
                new DeviceCatalogEntry(MchoseHeadsets, "MCHOSE X9 / V9 Pro", "2.4 GHz receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3")),
                new DeviceCatalogEntry(CorsairLegacy, "Corsair legacy wireless headsets", "USB receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3")),
                new DeviceCatalogEntry(HyperXCloudII, "HyperX Cloud II Wireless (HP)", "2.4 GHz receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3")),
                new DeviceCatalogEntry(LamzuMayaX, "CompX feature mice (LAMZU and compatible models)", "USB / receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("lamzu-battery-monitory", "https://github.com/Sheroune/lamzu-battery-monitory", "MIT")),
                new DeviceCatalogEntry(SteelSeriesNova, "SteelSeries Nova / Arctis 7+ / GameBuds", "2.4 GHz receiver",
                    "devicelist.comment.coarse",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3")),
                new DeviceCatalogEntry(Pulsar, "CompX power mice (Pulsar and compatible models)", "USB / 2.4 GHz receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("python-pulsar-mouse-tool", "https://github.com/andrewrabert/python-pulsar-mouse-tool", "MIT"),
                    new DeviceCatalogSource("hitscan-battery", "https://github.com/sopparus/hitscan-battery", "No license stated")),
                new DeviceCatalogEntry(Nzxt, "NZXT Lift Elite Wireless", "USB / 2.4 GHz receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT")),
                new DeviceCatalogEntry(RazerMouse, "Razer wireless mice (allowlisted models)", "USB / HyperSpeed receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("OpenRazer", "https://github.com/openrazer/openrazer", "GPL-2.0-or-later")),
                new DeviceCatalogEntry(RazerKeyboard, "Razer wireless keyboards (allowlisted models)", "USB / HyperSpeed receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("OpenRazer", "https://github.com/openrazer/openrazer", "GPL-2.0-or-later")),
                new DeviceCatalogEntry(Wlmouse, "WLmouse feature-protocol mice", "USB / receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("mouse-battery-tray / len0c", "https://github.com/incconutwo/mouse-battery-tray", "MIT")),
                new DeviceCatalogEntry(GwolvesNew, "G-Wolves new-protocol mice", "USB / 8K receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("G-Wolves web driver", "https://mouse.xyz", "No license stated")),
                new DeviceCatalogEntry(GwolvesOld, "G-Wolves old-protocol mice", "USB / receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("G-Wolves web driver", "https://mouse.xyz", "No license stated")),
                new DeviceCatalogEntry(AsusMouse, "ASUS ROG / TUF wireless mice", "USB / receiver",
                    "devicelist.comment.coarse",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("G-Helper", "https://github.com/seerge/g-helper", "GPL-3.0-only")),
                new DeviceCatalogEntry(Astro, "Astro A50 Gen 5", "USB base station",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3")),
                new DeviceCatalogEntry(Keychron, "Keychron M5 / Ultra-Link 8K", "USB / receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("keychron-battery-dkms", "https://github.com/csutcliff/keychron-battery-dkms", "GPL-2.0-only")),
                new DeviceCatalogEntry(AmInfinity, "AM Infinity / AJAZZ AJ159 APEX", "8K receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("AJAZZ Control Center", "https://github.com/Aiacos/ajazz-control-center", "GPL-3.0-or-later")),
                new DeviceCatalogEntry(HyperXCloudIII, "HyperX Cloud III / Cloud II Core Wireless", "USB receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("HyperHeadset", "https://github.com/LennardKittner/HyperHeadset", "MIT")),
                new DeviceCatalogEntry(HyperXAlpha2, "HyperX Cloud Alpha 2", "USB station (Controller)",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery / user captures", "https://github.com/HeyOkay/HaloBattery/issues/26", "MIT"),
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3")),
                new DeviceCatalogEntry(SteelSeriesClassic, "SteelSeries Arctis 1 / 7 / 9 / Nova Pro Wireless", "USB receiver / base station",
                    "devicelist.comment.coarse",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3"),
                    new DeviceCatalogSource("Linux", "https://github.com/torvalds/linux/blob/master/drivers/hid/hid-steelseries-arctis.c", "GPL-2.0-or-later")),
                new DeviceCatalogEntry(SteelSeriesAerox, "SteelSeries Aerox 3 / 5 / 9 Wireless", "2.4 GHz receiver",
                    "devicelist.comment.coarse",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("rivalcfg", "https://github.com/flozz/rivalcfg", "WTFPL"),
                    new DeviceCatalogSource("alloyctl", "https://github.com/szymonwilczek/alloyctl", "GPLv2")),
                new DeviceCatalogEntry(PlayStation, "Sony DualShock 4 / DualSense / Edge (passive full reports)", "USB / Bluetooth",
                    "devicelist.comment.coarse",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("Linux", "https://github.com/torvalds/linux/blob/master/drivers/hid/hid-playstation.c", "GPL-2.0-or-later")),
                new DeviceCatalogEntry(EightBitDo, "8BitDo Pro / Pro 2 / Pro 3 (passive enhanced reports)", "USB / Bluetooth (D-input)",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("SDL", "https://github.com/libsdl-org/SDL", "Zlib")),
                new DeviceCatalogEntry(Nintendo, "Nintendo Switch Pro Controller / Joy-Con", "Bluetooth",
                    "devicelist.comment.coarse",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("SDL", "https://github.com/libsdl-org/SDL", "Zlib"),
                    new DeviceCatalogSource("Switch reverse-engineering notes", "https://github.com/dekuNukem/Nintendo_Switch_Reverse_Engineering", "No license stated")),
                new DeviceCatalogEntry(Steam, "Steam Controller (2025) puck slots", "USB puck",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("SDL", "https://github.com/libsdl-org/SDL", "Zlib"),
                    new DeviceCatalogSource("sc2-research", "https://github.com/CouchTurtle/sc2-research", "MIT")),
                new DeviceCatalogEntry(LogitechHeadsets, "Logitech G533 / G535 / G733 / G933 / G935 / G PRO wireless voltage", "USB receiver",
                    "devicelist.comment.coarse",
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3"),
                    new DeviceCatalogSource("g933-utils", "https://github.com/ashkitten/g933-utils", "MIT")),
                new DeviceCatalogEntry(LogitechG930, "Logitech G930 (voltage estimate)", "USB receiver",
                    "devicelist.comment.nova7",
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3"),
                    new DeviceCatalogSource("g930-battery-percentage", "https://github.com/Roliga/g930-battery-percentage", "MIT")),
                new DeviceCatalogEntry(SonyInzone, "Sony INZONE H5 / H9 II", "USB receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3")),
                new DeviceCatalogEntry(Jabra, "Jabra Link 380 / 390 / Evolve2 65 Flex", "USB / receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3")),
                new DeviceCatalogEntry(LenovoVoip, "Lenovo Wireless VoIP Headset", "USB receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3")),
                new DeviceCatalogEntry(MchoseRealtek, "MCHOSE M7 / L7 / A7 RealTek status", "USB / Bluetooth / receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("mchose-linux / alexfrih", "https://github.com/alexfrih/mchose-linux", "MIT")),
                new DeviceCatalogEntry(MchoseG7, "MCHOSE G7", "USB / receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery / kek353 captures", "https://github.com/HeyOkay/HaloBattery/issues/8", "MIT")),
                new DeviceCatalogEntry(MchoseA5, "MCHOSE A5 Pro Max", "USB / 1K / 4K receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("MCHOSE A5 web driver", "https://github.com/z750sasr/mchose-a5-pro-max-web-driver", "No license file stated"),
                    new DeviceCatalogSource("mchose-macos", "https://github.com/Klegus/mchose-macos", "MIT")),
                new DeviceCatalogEntry(Lofree, "Lofree Hyzen (wireless only)", "2.4 GHz receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("Lofree web driver", "https://hyzen.lofree.tech", "No license stated")),
                new DeviceCatalogEntry(LogitechCenturion, "Logitech G PRO X 2 / X 3 / G522 (Centurion)", "USB receiver",
                    "devicelist.comment.experimental",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("HeadsetControl / iowi479 / thanosk / KarimTayie", "https://github.com/Sapd/HeadsetControl", "GPLv3"),
                    new DeviceCatalogSource("Solaar", "https://github.com/pwr-Solaar/Solaar", "GPL-2.0-or-later")),
                new DeviceCatalogEntry(Teevolution, "TEEvolution Terra Pro", "USB / receiver",
                    "devicelist.comment.receiverbuffer",
                    new DeviceCatalogSource("OpenMouse mouse-protocol", "https://github.com/OpenMouse-Project/mouse-protocol/tree/b62a1cce2b63698ac00595efdb0067ecd54e34a0/src/teevolution", "AGPL-3.0-or-later"),
                    new DeviceCatalogSource("TeevoLink factory protocol reference", "https://github.com/xilu7/teevolink/tree/26e0734589c8e09f1757f2cb6c809645fe06f61e/src/sdk", "No license stated")),
                new DeviceCatalogEntry(AudezeMaxwell, "Audeze Maxwell / Maxwell 2", "USB / receiver",
                    "devicelist.comment.maxwell",
                    new DeviceCatalogSource("Halo Battery", "https://github.com/HeyOkay/HaloBattery", "MIT"),
                    new DeviceCatalogSource("HeadsetControl", "https://github.com/Sapd/HeadsetControl", "GPLv3"),
                    new DeviceCatalogSource("Maxwell firmware research", "https://github.com/kats1123/maxwell-firmware-research", "CC BY 4.0"))
            }.Select(OpenMouseDeviceCatalog.WithAdditionalSources).Concat(OpenMouseDeviceCatalog.Entries).ToArray());

        private static readonly ReadOnlyCollection<DeviceCatalogEntry> OrderedEntries =
            Array.AsReadOnly(Entries.OrderBy(entry => entry.Id == "system" ? 0
                : entry.Id == "bluetooth" ? 1 : entry.Id == "xinput" ? 2 : 3)
                .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray());

        private static readonly ReadOnlyCollection<CustomPollingProfile> Profiles =
            BuildProfiles();

        private static ReadOnlyCollection<CustomPollingProfile> BuildProfiles()
        {
            var profiles = new List<CustomPollingProfile>
            {
                new CustomPollingProfile(Teevolution, 0x3554, 0xF520, null, null, 17, 17,
                    deviceName: "TEEvolution Terra Pro (3554:F520)", reportId: 8),
                new CustomPollingProfile(Teevolution, 0x3554, 0xF523, null, null, 17, 17,
                    deviceName: "TEEvolution Terra Pro (RapidSync 8K)", reportId: 8),
                new CustomPollingProfile(Teevolution, 0x3554, 0xF5BB, null, null, 17, 17,
                    deviceName: "TEEvolution Terra Pro (3554:F5BB)", reportId: 8),
                new CustomPollingProfile(Teevolution, 0x3554, 0xF522, null, null, 17, 17,
                    deviceName: "TEEvolution Terra Pro (3554:F522)", reportId: 8),
                new CustomPollingProfile(LogitechHidpp, 0x046D, null, 0xFF00, null, 20, 20),
                new CustomPollingProfile(LogitechHidpp, 0x046D, null, 0xFF43, null, 20, 20),
                new CustomPollingProfile(SteelSeriesNova7, 0x1038, 0x22A1, 0xFFC0, 0x0001, 64, 64, interfaceNumber: 3),
                new CustomPollingProfile(SteelSeriesNova7, 0x1038, 0x22A1, 0xFFC0, 0x0001, 64, 65, interfaceNumber: 3),
                new CustomPollingProfile(SteelSeriesNova7, 0x1038, 0x22A1, 0xFFC0, 0x0001, 65, 64, interfaceNumber: 3),
                new CustomPollingProfile(SteelSeriesNova7, 0x1038, 0x22A1, 0xFFC0, 0x0001, 65, 65, interfaceNumber: 3),
                new CustomPollingProfile(MchoseHeadsets, 0x3837, 0x6045, 0xFF90, 1, 64, 64,
                    interfaceNumber: 0, deviceName: "MCHOSE X9", minimumReportLengths: true),
                new CustomPollingProfile(MchoseHeadsets, 0x291D, 0x385D, 0xFF90, 1, 64, 64,
                    interfaceNumber: 0, deviceName: "MCHOSE V9 Pro", minimumReportLengths: true)
            };
            foreach (var product in new ushort[] { 0x1B27, 0x0A14, 0x0A16, 0x0A17, 0x0A1D, 0x0A1A,
                0x1B2A, 0x1B23, 0x1B29, 0x0A55, 0x0A51, 0x0A52, 0x0A38, 0x0A4F, 0x0A2B, 0x0A75, 0x0A56 })
                profiles.Add(new CustomPollingProfile(CorsairLegacy, 0x1B1C, product, 0xFFC5, 1, 5, 2,
                    interfaceNumber: 3, deviceName: $"Corsair wireless headset (1B1C:{product:X4})",
                    minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x0696, 0x018B })
                profiles.Add(new CustomPollingProfile(HyperXCloudII, 0x03F0, product, 0xFF90, 0x0303, 20, 52,
                    deviceName: "HyperX Cloud II Wireless", minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x001E, 0x001C })
                profiles.Add(new CustomPollingProfile(LamzuMayaX, 0x373E, product, 0xFFFF, null, 0, 0, 65,
                    interfaceNumber: 2, deviceName: "LAMZU Maya X", minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x2202, 0x227E, 0x2206, 0x2258, 0x229E, 0x22AD,
                0x22A4, 0x22A5, 0x223A, 0x22A9, 0x227A, 0x2232, 0x2253, 0x220A, 0x22A7, 0x2298,
                0x2269, 0x226D, 0x220E, 0x2212, 0x2216, 0x2236, 0x230A, 0x2317 })
                profiles.Add(new CustomPollingProfile(SteelSeriesNova, 0x1038, product, 0xFFC0, 1, 7, 2,
                    interfaceNumber: 3, deviceName: $"SteelSeries headset (1038:{product:X4})", minimumReportLengths: true));
            Add(Pulsar, 0x3554, 0xFF02, 2, 17, 17, new ushort[] { 0xF508, 0xF507, 0xF58F, 0xF58A, 0xF58C });
            Add(Pulsar, 0x373B, 0xFF02, 2, 17, 17, new ushort[] { 0x1085 });
            Add(Pulsar, 0x3770, 0xFF02, 2, 17, 17, new ushort[] { 0x0200, 0x0100 });
            Add(Pulsar, 0x3710, 0xFF02, 2, 17, 17, new ushort[] { 0x5406, 0x3414 });
            Add(Nzxt, 0x1E71, 0xFFCA, 1, 27, 64, new ushort[] { 0x2101, 0x2129 });
            profiles.AddRange(RazerPollingProfiles.All);
            foreach (var product in new ushort[] { 0x1020, 0x0031 })
                profiles.Add(new CustomPollingProfile(MchoseRealtek, 0x5253, product, 0xFF01, null, 0, 0, 65,
                    deviceName: "MCHOSE RealTek mouse", minimumReportLengths: true));
            profiles.Add(new CustomPollingProfile(MchoseRealtek, 0x3837, 0x100B, 0xFF01, 1, 0, 0, 65,
                interfaceNumber: 2, deviceName: "MCHOSE A7 V2 Ultra", minimumReportLengths: true));
            profiles.Add(new CustomPollingProfile(MchoseG7, 0xA8A5, 0x2255, 0xFF01, 0x10, 10, 65,
                deviceName: "MCHOSE G7", minimumReportLengths: true));
            Feature(MchoseA5, 0x2023, new ushort[] { 0xF013, 0xF015, 0xF019 });
            profiles.Add(new CustomPollingProfile(Lofree, 0x388D, 0x0025, 0xFF1C, 0x92, 32, 32,
                deviceName: "Lofree Hyzen", minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x0AF7, 0x0B18, 0x0B45 })
                profiles.Add(new CustomPollingProfile(LogitechCenturion, 0x046D, product, 0xFFA0, 1, 64, 64,
                    interfaceNumber: 3, deviceName: product == 0x0AF7 ? "Logitech G PRO X 2"
                        : product == 0x0B18 ? "Logitech G522" : "Logitech G PRO X 3",
                    minimumReportLengths: true));
            profiles.Add(new CustomPollingProfile(SonyInzone, 0x054C, 0x0EBF, 0xFF04, 2, 64, 64,
                deviceName: "Sony INZONE H5", minimumReportLengths: true));
            profiles.Add(new CustomPollingProfile(SonyInzone, 0x054C, 0x0FA8, 0xFF04, 1, 64, 64,
                interfaceNumber: 5, deviceName: "Sony INZONE H9 II", minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x24C7, 0x2E57, 0x2519 })
                profiles.Add(new CustomPollingProfile(Jabra, 0x0B0E, product, 0xFF00, product == 0x2E57 ? (ushort)5 : (ushort)1, 64, 64,
                    interfaceNumber: 3, deviceName: product == 0x24C7 ? "Jabra Link 380" : product == 0x2E57 ? "Jabra Link 390" : "Jabra Evolve2 65 Flex",
                    minimumReportLengths: true));
            profiles.Add(new CustomPollingProfile(LenovoVoip, 0x17EF, 0xA07D, 0xFF07, 0x0222, 61, 0, 61,
                interfaceNumber: 3, deviceName: "Lenovo Wireless VoIP Headset", minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x0A66, 0x0A5B, 0x0A87, 0x0AB5, 0x0AFE, 0x0B1F,
                0x0ABA, 0x0AFB, 0x0AFC })
                profiles.Add(new CustomPollingProfile(LogitechHeadsets, 0x046D, product, 0xFF43, 0x0202, 20, 20,
                    deviceName: $"Logitech wireless headset (046D:{product:X4})"));
            profiles.Add(new CustomPollingProfile(LogitechHeadsets, 0x046D, 0x0AC4, 0x000C, 1, 20, 20,
                interfaceNumber: 3, deviceName: "Logitech G535"));
            profiles.Add(new CustomPollingProfile(LogitechG930, 0x046D, 0x0A1F, 0xFF00, 1, 0, 0, 64,
                deviceName: "Logitech G930", minimumReportLengths: true));
            Controller(PlayStation, 0x054C, new ushort[] { 0x05C4, 0x09CC, 0x05C5, 0x0BA0, 0x0CE6, 0x0DF2 }, 31);
            Controller(EightBitDo, 0x2DC8, new ushort[] { 0x6000, 0x6100, 0x6001, 0x6101, 0x6003, 0x6006, 0x6009 }, 15);
            Controller(Nintendo, 0x057E, new ushort[] { 0x2006, 0x2007, 0x2009 }, 3);
            foreach (var product in new ushort[] { 0x1304, 0x1305 })
                for (var usbInterface = 2; usbInterface <= 5; usbInterface++)
                    profiles.Add(new CustomPollingProfile(Steam, 0x28DE, product, 0xFF00, 1, 15, 0,
                        interfaceNumber: usbInterface, deviceName: $"Steam Controller (slot {usbInterface - 1})",
                        minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x1838, 0x1878, 0x1852, 0x185C, 0x1860, 0x1858, 0x1874 })
                profiles.Add(new CustomPollingProfile(SteelSeriesAerox, 0x1038, product, 0xFFC0, 1, 2, 64,
                    deviceName: $"SteelSeries Aerox (1038:{product:X4})", minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x12B3, 0x12B6, 0x12D7, 0x12D5 })
                profiles.Add(new CustomPollingProfile(SteelSeriesClassic, 0x1038, product, 0xFF43, null, 4, 2,
                    interfaceNumber: 3, deviceName: $"SteelSeries Arctis (1038:{product:X4})", minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x12AD, 0x1260, 0x1252, 0x12C2, 0x12E0, 0x12E5 })
                foreach (var usbInterface in product == 0x12E0 || product == 0x12E5 ? new[] { 3, 4 } : new[] { product == 0x12C2 ? 0 : 5 })
                    profiles.Add(new CustomPollingProfile(SteelSeriesClassic, 0x1038, product, null, null,
                        product == 0x12E0 || product == 0x12E5 ? 16 : 5, 2,
                        interfaceNumber: usbInterface, deviceName: $"SteelSeries Arctis (1038:{product:X4})", minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x1A72, 0x1A70, 0x1B0C, 0x1B0A, 0x197F, 0x197D,
                0x1A1A, 0x1A18, 0x1A94, 0x1A92, 0x1A68, 0x1A66, 0x1979, 0x1977, 0x19F4, 0x1A8D,
                0x1AF5, 0x1AF3, 0x1C57, 0x1C56, 0x18E5, 0x18E3, 0x1960, 0x195E, 0x1A59, 0x1A57,
                0x1908, 0x1906, 0x1949, 0x1947 })
                profiles.Add(new CustomPollingProfile(AsusMouse, 0x0B05, product, null, null, 10, 65,
                    interfaceNumber: 0, deviceName: $"ASUS wireless mouse (0B05:{product:X4})", minimumReportLengths: true));
            Add(Astro, 0x046D, 0xFF32, 0x74, 9, 64, new ushort[] { 0x0B1C });
            Add(HyperXAlpha2, 0x03F0, 0xFF13, 0xFF00, 7, 64, new ushort[] { 0x08BE });
            foreach (var product in new ushort[] { 0xD028, 0xD048 })
                profiles.Add(new CustomPollingProfile(Keychron, 0x3434, product, null, null, 21, 0, 64,
                    interfaceNumber: 4, deviceName: "Keychron M5 / Ultra-Link 8K", minimumReportLengths: true));
            profiles.Add(new CustomPollingProfile(AmInfinity, 0x3151, 0x5007, 0xFFFF, 2, 0, 0, 65,
                deviceName: "AM Infinity / AJAZZ AJ159 APEX", minimumReportLengths: true));
            foreach (var product in new ushort[] { 0x05B7, 0x0C9D, 0x0795, 0x0995 })
            {
                var name = product == 0x0795 || product == 0x0995 ? "HyperX Cloud II Core Wireless" : "HyperX Cloud III Wireless";
                profiles.Add(new CustomPollingProfile(HyperXCloudIII, 0x03F0, product, 0xFF13, 1, 5, 62,
                    deviceName: name, minimumReportLengths: true));
                profiles.Add(new CustomPollingProfile(HyperXCloudIII, 0x03F0, product, 0xFF13, 1, 5, 0, 62,
                    deviceName: name, minimumReportLengths: true));
            }
            Feature(Wlmouse, 0x36A7, new ushort[] { 0xA887, 0xA868, 0xA880 });
            Feature(GwolvesNew, 0x33E4, new ushort[] { 0x3854, 0x5418, 0x5419, 0x5219, 0x4718, 0x4719,
                0x5618, 0x5619, 0x3619, 0x3519, 0x2719, 0x4219, 0x3808, 0x3817, 0x6808, 0x6817,
                0x5608, 0x5617, 0x3608, 0x3617 });
            Feature(GwolvesOld, 0x33E4, new ushort[] { 0x3908, 0x3917, 0x7904, 0x7913, 0x3708, 0x3717,
                0x5308, 0x5317, 0x7704, 0x7713, 0x3508, 0x3517, 0x7908, 0x7917, 0x5808, 0x5817,
                0x5807, 0x2708, 0x2717, 0x5408, 0x5417, 0x5407, 0x5708, 0x5717, 0x5707, 0x5908,
                0x5917, 0x5907, 0x7204, 0x7203, 0x7708, 0x7717, 0x5804, 0x5803, 0x5404, 0x5403,
                0x5704, 0x5703, 0x5904, 0x5903 });
            profiles.Add(new CustomPollingProfile(AudezeMaxwell, 0x3329, 0x4B18, 0xFF13, 1, 62, 62,
                interfaceNumber: 0, deviceName: "Audeze Maxwell (Xbox receiver)"));
            profiles.Add(new CustomPollingProfile(AudezeMaxwell, 0x3329, 0x4B19, 0xFF13, 1, 62, 62,
                interfaceNumber: 0, deviceName: "Audeze Maxwell (PlayStation/PC receiver)"));
            profiles.Add(new CustomPollingProfile(AudezeMaxwell, 0x3329, 0x4B1A, 0xFF13, 1, 62, 62,
                deviceName: "Audeze Maxwell (USB cable)"));
            profiles.Add(new CustomPollingProfile(AudezeMaxwell, 0x3329, 0x4B28, 0xFF13, 1, 62, 62,
                interfaceNumber: 5, deviceName: "Audeze Maxwell 2 (Xbox receiver)"));
            profiles.Add(new CustomPollingProfile(AudezeMaxwell, 0x3329, 0x4B29, 0xFF13, 1, 62, 62,
                interfaceNumber: 5, deviceName: "Audeze Maxwell 2 (PlayStation/PC receiver)"));
            profiles.AddRange(OpenMouseDeviceCatalog.Profiles);
            return profiles.AsReadOnly();

            void Controller(string protocol, ushort vendor, ushort[] products, int input)
            {
                foreach (var product in products)
                    foreach (var usage in new ushort[] { 4, 5 })
                        profiles.Add(new CustomPollingProfile(protocol, vendor, product, 1, usage,
                            protocol == PlayStation && (product == 0x0CE6 || product == 0x0DF2) ? 55 : input,
                            protocol == Nintendo ? 49 : 0,
                            deviceName: protocol == PlayStation
                                ? product == 0x0DF2 ? "Sony DualSense Edge" : product == 0x0CE6 ? "Sony DualSense" : "Sony DualShock 4"
                                : protocol == EightBitDo
                                    ? product == 0x6009 ? "8BitDo Pro 3" : product == 0x6003 || product == 0x6006 ? "8BitDo Pro 2"
                                        : product == 0x6000 || product == 0x6100 ? "8BitDo SF30 Pro" : "8BitDo SN30 Pro"
                                    : product == 0x2006 ? "Nintendo Joy-Con (L)" : product == 0x2007 ? "Nintendo Joy-Con (R)" : "Nintendo Switch Pro Controller",
                            minimumReportLengths: true));
            }

            void Feature(string protocol, ushort vendor, ushort[] products)
            {
                foreach (var product in products)
                    profiles.Add(new CustomPollingProfile(protocol, vendor, product, null, null, 0, 0, 65,
                        deviceName: $"{Entries.First(entry => entry.Id == protocol).Name} ({vendor:X4}:{product:X4})",
                        minimumReportLengths: true, exactFeatureLength: true));
            }

            void Add(string protocol, ushort vendor, ushort page, ushort usage, int input, int output, ushort[] products)
            {
                foreach (var product in products)
                    profiles.Add(new CustomPollingProfile(protocol, vendor, product, page, usage, input, output,
                        deviceName: $"{Entries.First(entry => entry.Id == protocol).Name} ({vendor:X4}:{product:X4})",
                        minimumReportLengths: true));
            }
        }

        public static IReadOnlyList<DeviceCatalogEntry> All => OrderedEntries;
        public static IReadOnlyList<CustomPollingProfile> CustomProfiles => Profiles;

        /// <summary>Matches an explicitly allowlisted protocol and HID collection.</summary>
        public static bool AllowsCustomPolling(string protocolId, ushort vendorId, ushort productId, ushort usagePage,
            ushort usage, int inputReportLength, int outputReportLength)
        {
            foreach (var profile in Profiles)
                if (profile.Matches(protocolId, vendorId, productId, usagePage, usage, inputReportLength, outputReportLength))
                    return true;
            return false;
        }
    }
}
