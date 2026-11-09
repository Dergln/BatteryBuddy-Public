using System;
using System.IO;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class HeadsetTelemetryProtocols
    {
        private static int sonyTransaction = 1;
        private static int jabraSequence;

        internal static HidBatteryStatus ReadSonyInzone(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            ushort transaction;
            do
            {
                transaction = unchecked((ushort)Interlocked.Increment(ref sonyTransaction));
            } while (transaction < 2); // Transaction 1 belongs to unsolicited dongle notifications.

            var request = new byte[] { 2, 12, 1, 0, 0xFC, 8, 0x96, 0xC3, 0x41, 4, 1,
                (byte)transaction, (byte)(transaction >> 8), 0 };
            request[13] = Sum(request, 6, 12);
            var response = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                report => report.Length >= 13 && report[0] == 2 && report[2] == 4 && report[3] == 0xFF
                    && report[9] == 4 && report[10] == 0x10
                    && report[11] == (byte)transaction && report[12] == (byte)(transaction >> 8), ct);
            return ParseSonyInzone(response, transaction);
        }

        internal static HidBatteryStatus ParseSonyInzone(byte[] report, ushort transaction)
        {
            if (report.Length < 16 || report[0] != 2 || report[1] < 14 || report[1] > 62
                || report.Length <= report[1] + 1 || report[2] != 4 || report[3] != 0xFF
                || report[5] != 0 || report[6] != 0x96 || report[7] != 0xC3 || report[8] != 0x14
                || report[9] != 4 || report[10] != 0x10 || transaction < 2
                || report[11] != (byte)transaction || report[12] != (byte)(transaction >> 8)
                || Sum(report, 5, report[1]) != report[report[1] + 1])
                throw new InvalidDataException("Invalid Sony INZONE battery response.");
            return report[14] == 0xFF ? null : new HidBatteryStatus(report[14], report[13] != 0);
        }

        internal static HidBatteryStatus ReadJabra(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var address = info.ProductId == 0x2519 ? (byte)8 : (byte)4;
            var sequence = unchecked((byte)Interlocked.Increment(ref jabraSequence));
            var request = new byte[] { 5, address, 0, sequence, 0x46, 0x12, 2 };
            var response = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                report => report.Length >= 7 && report[0] == 5 && report[1] == 0 && report[2] == address
                    && report[3] == sequence && (report[4] >> 6) == 3
                    && (report[5] == 0xFE || report[5] == 0xFF || (report[5] == 0x12 && report[6] == 2)), ct);
            return ParseJabra(response, address, sequence);
        }

        internal static HidBatteryStatus ParseJabra(byte[] report, byte address, byte sequence)
        {
            if (report.Length < 7 || report[0] != 5 || report[1] != 0 || report[2] != address
                || report[3] != sequence || (report[4] >> 6) != 3 || (report[4] & 0x3F) < 6
                || (report[4] & 0x3F) + 1 > report.Length
                || (report[5] != 0xFE && report[5] != 0xFF && !(report[5] == 0x12 && report[6] == 2)))
                throw new InvalidDataException("Invalid Jabra battery response.");
            if (report[5] == 0xFE)
            {
                switch (report[6])
                {
                    case 0xF3:
                    case 0xF4:
                    case 0xF5:
                        return null;
                    case 0xF6:
                    case 0xFF:
                        throw new NotSupportedException("The paired Jabra headset does not support battery telemetry.");
                    default:
                        throw new InvalidDataException("Jabra rejected the battery query.");
                }
            }
            if ((report[4] & 0x3F) < 8)
                throw new InvalidDataException("Short Jabra battery payload.");
            return new HidBatteryStatus(report[8], (report[7] & 1) != 0);
        }

        internal static HidBatteryStatus ReadLenovoVoip(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (!(channel is IHidFeatureTransport feature))
                throw new NotSupportedException("The HID channel does not support Lenovo battery feature queries.");
            var request = new byte[61];
            request[0] = 0x24;
            request[1] = 1;
            (channel as IHidInputQueue)?.FlushInput();
            if (!feature.SetFeature(request, HidBatteryExchange.TimeoutMs))
                throw new IOException("Unable to request Lenovo headset status.");
            var response = HidBatteryExchange.Receive(channel,
                report => report.Length >= 2 && report[0] == 0x27 && report[1] == 1, ct);
            return ParseLenovoVoip(response);
        }

        internal static HidBatteryStatus ParseLenovoVoip(byte[] report)
        {
            if (report.Length != 61 || report[0] != 0x27 || report[1] != 1)
                throw new InvalidDataException("Invalid Lenovo headset battery response.");
            return report[2] != 0 ? null : new HidBatteryStatus(report[7], false);
        }

        private static byte Sum(byte[] bytes, int first, int last)
        {
            var sum = 0;
            for (var index = first; index <= last; index++)
                sum += bytes[index];
            return unchecked((byte)sum);
        }
    }
}
