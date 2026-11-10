using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    public static class ExperimentalBatteryProviders
    {
        public static IReadOnlyList<IBatteryProvider> Create()
            => new[]
            {
                CreateTeevolution(),
                Create(DeviceCatalog.AudezeMaxwell, "Audeze Maxwell", "audeze-maxwell:", AudezeMaxwellBatteryProtocol.Read),
                Create(DeviceCatalog.MchoseHeadsets, "MCHOSE", "mchose:", ReadMchose),
                Create(DeviceCatalog.CorsairLegacy, "Corsair legacy", "corsair-legacy:", ReadCorsairLegacy),
                Create(DeviceCatalog.HyperXCloudII, "HyperX Cloud II", "hyperx-cloud-ii:", ReadHyperXCloudII),
                Create(DeviceCatalog.LamzuMayaX, "CompX feature", "lamzu:",
                    (info, channel, ct) => OpenMouseBatteryProviders.ReadExtension(DeviceCatalog.LamzuMayaX, info, channel, ct, ReadLamzu),
                    category: DeviceCategory.Mouse),
                Create(DeviceCatalog.SteelSeriesNova, "SteelSeries status", "steelseries:", ReadSteelSeriesNova),
                Create(DeviceCatalog.Pulsar, "CompX power", "pulsar:",
                    (info, channel, ct) => OpenMouseBatteryProviders.ReadExtension(DeviceCatalog.Pulsar, info, channel, ct, ReadPulsar),
                    category: DeviceCategory.Mouse),
                Create(DeviceCatalog.Nzxt, "NZXT Lift", "nzxt:", ReadNzxt, category: DeviceCategory.Mouse),
                Create(DeviceCatalog.RazerMouse, "Razer mice", "razer-mouse:", RazerBatteryProtocol.Read, category: DeviceCategory.Mouse),
                Create(DeviceCatalog.RazerKeyboard, "Razer keyboards", "razer-keyboard:", RazerBatteryProtocol.Read, category: DeviceCategory.Keyboard),
                Create(DeviceCatalog.Wlmouse, "WLmouse", "wlmouse:",
                    (info, channel, ct) => OpenMouseBatteryProviders.ReadExtension(DeviceCatalog.Wlmouse, info, channel, ct, ReadLamzu),
                    category: DeviceCategory.Mouse),
                Create(DeviceCatalog.GwolvesNew, "G-Wolves new", "gwolves-new:", ReadLamzu, category: DeviceCategory.Mouse),
                Create(DeviceCatalog.GwolvesOld, "G-Wolves old", "gwolves-old:", ReadGwolvesOld, category: DeviceCategory.Mouse),
                Create(DeviceCatalog.AsusMouse, "ASUS mice", "asus:",
                    (info, channel, ct) => OpenMouseBatteryProviders.ReadExtension(DeviceCatalog.AsusMouse, info, channel, ct, AdditionalBatteryProtocols.ReadAsus),
                    category: DeviceCategory.Mouse),
                Create(DeviceCatalog.Astro, "Astro A50 Gen 5", "astro:", AdditionalBatteryProtocols.ReadAstro),
                Create(DeviceCatalog.Keychron, "Keychron", "keychron:", AdditionalBatteryProtocols.ReadKeychron, category: DeviceCategory.Mouse),
                Create(DeviceCatalog.AmInfinity, "AM Infinity", "am-infinity:", AdditionalBatteryProtocols.ReadAmInfinity, category: DeviceCategory.Mouse),
                Create(DeviceCatalog.HyperXCloudIII, "HyperX Cloud III", "hyperx-cloud-iii:", AdditionalBatteryProtocols.ReadHyperXCloudIII),
                Create(DeviceCatalog.HyperXAlpha2, "HyperX Alpha 2", "hyperx-alpha-2:", AdditionalBatteryProtocols.ReadHyperXAlpha2),
                Create(DeviceCatalog.SteelSeriesClassic, "SteelSeries classic", "steelseries-classic:", SteelSeriesBatteryProtocols.ReadClassic),
                Create(DeviceCatalog.SteelSeriesAerox, "SteelSeries Aerox", "steelseries-aerox:", SteelSeriesBatteryProtocols.ReadAerox, category: DeviceCategory.Mouse),
                Create(DeviceCatalog.PlayStation, "PlayStation", "playstation:", ControllerBatteryProtocols.ReadPlayStation, category: DeviceCategory.Gamepad),
                Create(DeviceCatalog.EightBitDo, "8BitDo", "8bitdo:", ControllerBatteryProtocols.ReadEightBitDo, category: DeviceCategory.Gamepad),
                Create(DeviceCatalog.Nintendo, "Nintendo", "nintendo:", ControllerBatteryProtocols.ReadNintendo, category: DeviceCategory.Gamepad),
                CreateSteamSlot(2), CreateSteamSlot(3), CreateSteamSlot(4), CreateSteamSlot(5),
                Create(DeviceCatalog.LogitechHeadsets, "Logitech headset voltage", "logitech-headset:", LogitechHeadsetBatteryProtocol.Read),
                Create(DeviceCatalog.LogitechG930, "Logitech G930", "logitech-g930:", LogitechHeadsetBatteryProtocol.ReadG930),
                Create(DeviceCatalog.SonyInzone, "Sony INZONE", "sony-inzone:", HeadsetTelemetryProtocols.ReadSonyInzone),
                Create(DeviceCatalog.Jabra, "Jabra GNP", "jabra:", HeadsetTelemetryProtocols.ReadJabra),
                Create(DeviceCatalog.LenovoVoip, "Lenovo VoIP", "lenovo-voip:", HeadsetTelemetryProtocols.ReadLenovoVoip),
                Create(DeviceCatalog.MchoseRealtek, "MCHOSE RealTek", "mchose-realtek:", MouseAndKeyboardBatteryProtocols.ReadMchoseRealtek, category: DeviceCategory.Mouse),
                Create(DeviceCatalog.MchoseG7, "MCHOSE G7", "mchose-g7:", MouseAndKeyboardBatteryProtocols.ReadMchoseG7, category: DeviceCategory.Mouse),
                Create(DeviceCatalog.MchoseA5, "MCHOSE A5", "mchose-a5:", ReadLamzu, category: DeviceCategory.Mouse),
                Create(DeviceCatalog.Lofree, "Lofree Hyzen", "lofree:", MouseAndKeyboardBatteryProtocols.ReadLofree,
                    enumerate: () => LofreeCandidates(HidNative.Enumerate(0x388D)), category: DeviceCategory.Keyboard),
                Create(DeviceCatalog.LogitechCenturion, "Logitech Centurion", "logitech-centurion:", LogitechCenturionBatteryProtocol.Read)
            }.Concat(OpenMouseBatteryProviders.Create()).ToArray();

        internal static IReadOnlyList<HidInterfaceInfo> LofreeCandidates(IReadOnlyList<HidInterfaceInfo> interfaces)
            => interfaces.Any(info => info.ProductId == 0x0024) ? Array.Empty<HidInterfaceInfo>() : interfaces;

        internal static bool OwnsLogitechInterface(HidInterfaceInfo info)
            => info.VendorId == 0x046D && DeviceCatalog.CustomProfiles.Any(profile =>
                profile.VendorId == 0x046D && profile.ProtocolId != DeviceCatalog.LogitechHidpp && Matches(profile, info));

        internal static IBatteryProvider CreateTeevolution(Func<IReadOnlyList<HidInterfaceInfo>> enumerate = null,
            Func<HidInterfaceInfo, IHidTransport> open = null)
            => Create(DeviceCatalog.Teevolution, "TEEvolution Terra Pro", "teevolution:",
                TeevolutionBatteryProtocol.Read, enumerate, open, DeviceCategory.Mouse);

        private static IBatteryProvider CreateSteamSlot(int usbInterface)
            => Create(DeviceCatalog.Steam, $"Steam slot {usbInterface - 1}", "steam:",
                ControllerBatteryProtocols.ReadSteam, category: DeviceCategory.Gamepad, interfaceFilter: usbInterface);

        internal static IBatteryProvider Create(string protocol, string name, string prefix,
            Func<HidInterfaceInfo, IHidTransport, CancellationToken, HidBatteryStatus> read,
            Func<IReadOnlyList<HidInterfaceInfo>> enumerate = null, Func<HidInterfaceInfo, IHidTransport> open = null,
            DeviceCategory category = DeviceCategory.Headset, int? interfaceFilter = null)
        {
            var profiles = DeviceCatalog.CustomProfiles.Where(profile => profile.ProtocolId == protocol
                && (!interfaceFilter.HasValue || profile.InterfaceNumber == interfaceFilter)).ToArray();
            return new HidBatteryProvider(name, protocol, prefix, category,
                info => profiles.First(profile => Matches(profile, info)).DeviceName,
                enumerate ?? (() => profiles.Select(profile => profile.VendorId).Distinct()
                    .SelectMany(HidNative.Enumerate).ToArray()),
                info => profiles.Any(profile => Matches(profile, info)), read, open);
        }

        internal static bool Matches(CustomPollingProfile profile, HidInterfaceInfo info)
            => profile.Matches(profile.ProtocolId, info.VendorId, info.ProductId, info.UsagePage, info.Usage,
                info.InputReportLength, info.OutputReportLength, info.FeatureReportLength,
                info.InputReportIds, info.OutputReportIds)
                && (profile.ProtocolId == DeviceCatalog.Teevolution
                    ? TeevolutionBatteryProtocol.IsBatteryInterface(info)
                    : profile.UsagePage.HasValue || profile.FeatureReportLength.HasValue || info.UsagePage >= 0xFF00)
                && (profile.ProtocolId != DeviceCatalog.Nintendo || ControllerBatteryProtocols.IsBluetooth(info.Path))
                && (!profile.InterfaceNumber.HasValue || HidBatteryIdentity.IsUsbInterface(info.Path, profile.InterfaceNumber.Value));

        internal static HidBatteryStatus ReadMchose(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0x55, 0x65, 1 },
                response => HidBatteryExchange.StartsWith(response, 0x55, 0x65) && response.Length >= 4 && response[3] == 2, ct);
            return new HidBatteryStatus(report[2], false);
        }

        internal static HidBatteryStatus ReadCorsairLegacy(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0xC9, 0x64 },
                response => HidBatteryExchange.StartsWith(HidBatteryExchange.WithoutZeroReportId(response), 0x64, 0), ct);
            report = HidBatteryExchange.WithoutZeroReportId(report);
            if (report.Length < 5 || report[3] != 0xB1)
                throw new InvalidDataException("Invalid Corsair battery status report.");
            if (report[4] == 0)
                return null;
            if (report[4] != 1 && report[4] != 2 && report[4] != 4 && report[4] != 5)
                throw new InvalidDataException("Unknown Corsair battery state.");
            return new HidBatteryStatus(report[2] & 0x7F, report[4] == 4 || report[4] == 5);
        }

        internal static HidBatteryStatus ReadHyperXCloudII(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            byte[] Query(byte command)
            {
                var report = HidBatteryExchange.Output(channel, info.OutputReportLength,
                    new byte[] { 6, 0xFF, 0xBB, command },
                    response => HidBatteryExchange.StartsWith(response, 6, 0xFF, 0xBB, command), ct, 100);
                if (report.Length < 20)
                    throw new InvalidDataException("Short HyperX Cloud II battery report.");
                return report;
            }
            var battery = Query(2);
            var charging = Query(3);
            if (charging[4] > 1)
                throw new InvalidDataException("Unknown HyperX Cloud II charging state.");
            return new HidBatteryStatus(battery[7], charging[4] == 1);
        }

        internal static HidBatteryStatus ReadLamzu(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var request = new byte[65];
            Array.Copy(new byte[] { 0, 0, 2, 2, 0, 0x83 }, 0, request, 1, 6);
            return HidBatteryExchange.FeatureStatus(channel, request, ParseLamzu, ct);
        }

        internal static HidBatteryStatus ParseLamzu(byte[] report)
        {
            for (var index = 5; index + 2 < report.Length; index++)
            {
                if ((report[index - 5] != 0xA1 && report[index - 5] != 0xA2)
                    || report[index - 4] != 0 || report[index - 3] != 2 || report[index - 2] != 2
                    || report[index - 1] != 0 || report[index] != 0x83)
                    continue;
                if (report[index + 1] > 1)
                    throw new InvalidDataException("Unknown LAMZU charging state.");
                return new HidBatteryStatus(report[index + 2], report[index + 1] == 1);
            }
            return null;
        }

        internal static HidBatteryStatus ReadGwolvesOld(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var wired = new ushort[] { 0x3908, 0x7904, 0x3708, 0x5308, 0x7704, 0x3508, 0x7908, 0x5808,
                0x2708, 0x5408, 0x5708, 0x5908, 0x7204, 0x7708, 0x5804, 0x5404, 0x5704, 0x5904 }.Contains(info.ProductId);
            var request = new byte[65];
            request[2] = 2;
            request[3] = 0x8F;
            request[4] = wired ? (byte)0 : (byte)1;
            return HidBatteryExchange.FeatureStatus(channel, request, report =>
            {
                report = HidBatteryExchange.WithoutZeroReportId(report);
                if (!HidBatteryExchange.StartsWith(report, 0xA1, 2, 0x8F))
                    return null;
                if (report.Length < 6 || report[4] > 1)
                    throw new InvalidDataException("Invalid G-Wolves old battery reply.");
                return new HidBatteryStatus(report[5], report[4] == 1);
            }, ct);
        }

        internal static HidBatteryStatus ReadSteelSeriesNova(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0, 0xB0 },
                response => HidBatteryExchange.StartsWith(HidBatteryExchange.WithoutZeroReportId(response), 0xB0), ct);
            return ParseSteelSeriesNova(info.ProductId, HidBatteryExchange.WithoutZeroReportId(report));
        }

        internal static HidBatteryStatus ParseSteelSeriesNova(ushort product, byte[] report)
        {
            if (report.Length < 4 || report[0] != 0xB0)
                throw new InvalidDataException("Short SteelSeries status report.");
            if (product == 0x230A || product == 0x2317)
            {
                if (report.Length < 7)
                    throw new InvalidDataException("Short GameBuds status report.");
                var levels = new List<int>();
                for (var index = 0; index < 2; index++)
                    if (report[3 + index] == 3)
                    {
                        if (report[5 + index] > 100)
                            throw new InvalidDataException("Invalid GameBuds battery level.");
                        levels.Add(report[5 + index]);
                    }
                return levels.Count == 0 ? null : new HidBatteryStatus(levels.Min(), false);
            }
            if (product == 0x2232 || product == 0x2253 || product == 0x2269 || product == 0x226D)
            {
                if (report.Length < 5 || report[4] > 1)
                    throw new InvalidDataException("Invalid SteelSeries Nova 5-family status.");
                return report[1] == 2 ? null : new HidBatteryStatus(report[3], report[4] == 1);
            }
            if (product == 0x220E || product == 0x2212 || product == 0x2216 || product == 0x2236)
            {
                if (report[1] == 1)
                    return null;
                if (report[2] > 4 || report[3] > 1)
                    throw new InvalidDataException("Invalid Arctis 7+ battery status.");
                return new HidBatteryStatus(report[2] * 25, report[3] == 1);
            }
            if (report[1] == 2 || report[3] == 0)
                return null;
            if (report[1] != 3 || report[3] > 3)
                throw new InvalidDataException("Unknown SteelSeries Nova 7 state.");
            var discrete = new ushort[] { 0x2202, 0x2206, 0x22A4, 0x223A, 0x227A, 0x220A }.Contains(product);
            if (discrete && report[2] > 4)
                throw new InvalidDataException("Invalid discrete SteelSeries battery level.");
            return new HidBatteryStatus(report[2] * (discrete ? 25 : 1), report[3] == 1 || report[3] == 2);
        }

        internal static HidBatteryStatus ReadPulsar(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var command = new byte[17];
            command[0] = 8;
            command[1] = 4;
            command[16] = 0x49;
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength, command,
                response => HidBatteryExchange.StartsWith(response, 8, 4), ct);
            if (report.Length < 17 || (report.Take(17).Sum(value => (int)value) & 0xFF) != 0x55)
                throw new InvalidDataException("Invalid Pulsar power-report checksum.");
            return new HidBatteryStatus(report[6], report[7] != 0);
        }

        internal static HidBatteryStatus ReadNzxt(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0x4E, 2, 0x81, 0, 0xB0 },
                response => HidBatteryExchange.StartsWith(response, 0x4E, 2, 0x97), ct);
            if (report.Length < 27)
                throw new InvalidDataException("Short NZXT telemetry report.");
            return new HidBatteryStatus(report[17] | (report[18] << 8), (report[22] & 1) != 0);
        }
    }
}
