using System;
using System.IO;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class SteelSeriesBatteryProtocols
    {
        internal static HidBatteryStatus ReadAerox(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0, 0xD2 },
                reply => HidBatteryExchange.StartsWith(HidBatteryExchange.WithoutZeroReportId(reply), 0xD2), ct);
            report = HidBatteryExchange.WithoutZeroReportId(report);
            if (report.Length < 2)
                throw new InvalidDataException("Short Aerox battery status.");
            var raw = report[1] & 0x7F;
            if (raw == 0)
                return null;
            return new HidBatteryStatus(raw <= 21 ? (raw - 1) * 5 : raw, (report[1] & 0x80) != 0);
        }

        internal static HidBatteryStatus ReadClassic(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            byte[] Query(byte command)
                => HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 6, command },
                    reply => HidBatteryExchange.StartsWith(reply, 6, command), ct);
            byte[] report;
            switch (info.ProductId)
            {
                case 0x12B3:
                case 0x12B6:
                case 0x12D7:
                case 0x12D5:
                    report = Query(0x12);
                    if (report.Length < 4)
                        throw new InvalidDataException("Short Arctis 1-family battery status.");
                    return report[2] == 1 ? null : new HidBatteryStatus(report[3], false);
                case 0x12AD:
                    report = Query(0x14);
                    if (report.Length < 3)
                        throw new InvalidDataException("Short Arctis 7 connection status.");
                    if (report[2] != 3)
                        return null;
                    break;
                case 0x1260:
                case 0x1252:
                    break;
                case 0x12C2:
                    report = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0, 0x20 },
                        reply => reply.Length >= 5 && (reply[0] == 0xAA || reply[0] == 0x55), ct);
                    if (report[0] != 0xAA || report[1] != 1)
                        return null;
                    if (report[3] < 100 || report[3] > 154 || report[4] > 1)
                        throw new InvalidDataException("Invalid Arctis 9 voltage or charging state.");
                    return new HidBatteryStatus((report[3] - 100) * 100 / 54, report[4] == 1);
                case 0x12E0:
                case 0x12E5:
                    report = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 6, 0xB0 },
                        reply => reply.Length >= 16 && (reply[15] == 1 || reply[15] == 2 || reply[15] == 8), ct);
                    if (report[15] == 1)
                        return null;
                    if (report[6] > 8)
                        throw new InvalidDataException("Invalid Nova Pro discrete battery level.");
                    return new HidBatteryStatus(report[6] * 100 / 8, report[15] == 2);
                default: throw new InvalidOperationException("No classic SteelSeries battery decoder for this product.");
            }
            report = Query(0x18);
            if (report.Length < 3)
                throw new InvalidDataException("Short Arctis 7 battery status.");
            return report[2] == 0 && info.ProductId != 0x12AD ? null : new HidBatteryStatus(report[2], false);
        }
    }
}
