using System;
using System.IO;
using System.Linq;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class AdditionalBatteryProtocols
    {
        internal static HidBatteryStatus ReadAsus(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0, 0x12, 7 },
                reply => HidBatteryExchange.StartsWith(HidBatteryExchange.WithoutZeroReportId(reply), 0x12, 7), ct);
            report = HidBatteryExchange.WithoutZeroReportId(report);
            if (report.Length < 10)
                throw new InvalidDataException("Short ASUS battery status.");
            var charging = report[9] != 0;
            if (report[4] == 0 && !charging)
                return null;
            var discrete = new ushort[] { 0x18E5, 0x18E3, 0x1960, 0x195E, 0x1A59, 0x1A57,
                0x1908, 0x1906, 0x1949, 0x1947 }.Contains(info.ProductId);
            if (discrete && report[4] > 4)
                throw new InvalidDataException("Invalid discrete ASUS battery level.");
            return new HidBatteryStatus(report[4] * (discrete ? 25 : 1), charging);
        }

        internal static HidBatteryStatus ReadAstro(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 2, 0x0C, 3, 0, 6, 0x0C },
                reply => HidBatteryExchange.StartsWith(reply, 2, 0x0C) && reply.Length >= 9
                    && reply[4] == 6 && reply[5] == 0x0C, ct);
            return new HidBatteryStatus(report[6], report[8] != 0);
        }

        internal static HidBatteryStatus ReadKeychron(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var request = new byte[64];
            request[0] = 0xB3;
            request[1] = 6;
            (channel as IHidInputQueue)?.FlushInput();
            SetFeature(channel, request, ct);
            var report = HidBatteryExchange.Receive(channel, reply => HidBatteryExchange.StartsWith(reply, 0xB4, 6), ct);
            if (report.Length <= 20)
                throw new InvalidDataException("Short Keychron status.");
            return new HidBatteryStatus(report[20], false);
        }

        internal static HidBatteryStatus ReadAmInfinity(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var request = new byte[65];
            request[1] = 0xF7;
            SetFeature(channel, request, ct);
            if (ct.WaitHandle.WaitOne(30))
                ct.ThrowIfCancellationRequested();
            var report = HidBatteryExchange.Feature(channel, null, 5, 65, ct);
            if (report.Length < 4 || report[0] != 5 || report[1] != 0 || report[2] != 0)
                throw new InvalidDataException("Invalid AM Infinity status.");
            return report[3] == 0 ? null : new HidBatteryStatus(report[3], false);
        }

        internal static HidBatteryStatus ReadHyperXCloudIII(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            byte[] Query(byte command, byte response)
            {
                ct.ThrowIfCancellationRequested();
                var request = new byte[info.FeatureReportLength >= 62 ? 62 : info.OutputReportLength];
                request[0] = 0x66;
                request[1] = command;
                (channel as IHidInputQueue)?.FlushInput();
                if (info.FeatureReportLength >= 62)
                    SetFeature(channel, request, ct);
                else if (!channel.Write(request))
                    throw new IOException("Unable to request HyperX Cloud III status.");
                if (ct.WaitHandle.WaitOne(100))
                    ct.ThrowIfCancellationRequested();
                return HidBatteryExchange.Receive(channel, reply => reply.Length >= 5
                    && reply[0] == 0x66 && (reply[1] == command || reply[1] == response), ct);
            }
            var level = Query(0x89, 0x0D);
            if (level[2] == 0 && level[3] == 0)
                return null;
            var charging = Query(0x8A, 0x0C);
            if (charging[2] > 2)
                throw new InvalidDataException("Unknown HyperX charging state.");
            return new HidBatteryStatus(level[4], charging[2] != 0);
        }

        internal static HidBatteryStatus ReadHyperXAlpha2(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0x50, 2 },
                reply => HidBatteryExchange.StartsWith(reply, 0x51, 2), ct);
            if (report.Length < 7)
                throw new InvalidDataException("Short HyperX Alpha 2 status.");
            return new HidBatteryStatus(report[2], (report[6] & 0x80) != 0);
        }

        private static void SetFeature(IHidTransport channel, byte[] request, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (!(channel is IHidFeatureTransport feature))
                throw new NotSupportedException("The HID channel does not support feature reports.");
            if (!feature.SetFeature(request, HidBatteryExchange.TimeoutMs))
                throw new IOException("Unable to send HID battery feature request.");
        }
    }
}
