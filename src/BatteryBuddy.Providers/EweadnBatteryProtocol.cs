using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BatteryBuddy.Core.Providers;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class EweadnBatteryProtocol
    {
        private const int ReceiverReportLength = 33;
        private const int FreqReportLength = 65;
        private const byte FreqBatteryCommand = 0x30;

        internal static HidBatteryStatus Read(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            if (info.VendorId == 0x089D && info.ProductId == 0x062F)
                return ReadH2S9Receiver(info, channel, ct);
            if (info.VendorId == 0xA8A5 && info.ProductId == 0x2255)
            {
                if (string.Equals(info.ProductName, "EWEADN A7", StringComparison.OrdinalIgnoreCase))
                    return ReadFreqMouse(info, channel, ct, 0, 0, 1);
                if (string.Equals(info.ProductName, "EWEADN A7Pro", StringComparison.OrdinalIgnoreCase))
                    return ReadFreqMouse(info, channel, ct, 0, 0, 1);
                if (string.Equals(info.ProductName, "EWEADN X23SE", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryGetUniqueNonzeroReportId(info.InputReportIds, out var inputId)
                        || !TryGetUniqueNonzeroReportId(info.OutputReportIds, out var outputId))
                        throw new NotSupportedException("The EWEADN X23SE report IDs are ambiguous.");
                    return ReadFreqMouse(info, channel, ct, inputId, outputId, 2);
                }
            }
            if (info.VendorId == 0x372E && info.ProductId == 0x1026)
                return ReadS9Pro8K(channel, ct);

            throw new NotSupportedException("Unsupported EWEADN receiver HID collection.");
        }

        private static HidBatteryStatus ReadH2S9Receiver(HidInterfaceInfo info, IHidTransport channel,
            CancellationToken ct)
        {
            if (info.InputReportLength != ReceiverReportLength || info.OutputReportLength != ReceiverReportLength
                || !HidBatteryIdentity.IsUsbInterface(info.Path, 2))
                throw new NotSupportedException("Unsupported EWEADN H2 / S9 receiver collection.");

            var request = new byte[ReceiverReportLength];
            request[1] = FreqBatteryCommand;
            request[ReceiverReportLength - 1] = Sum(request, 5, 31);
            var response = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                IsReceiverReply, ct);
            var body = ReceiverBody(response);

            if (body[1] == 0x02)
                return null;
            if (body[1] != 0 || body[2] != 1 || body[3] != 0x0B)
                throw new InvalidDataException("Invalid EWEADN device-info status.");

            var model = Encoding.ASCII.GetString(body, 4, 4);
            if (model != "M156" && model != "M302")
                throw new InvalidDataException("Unexpected EWEADN receiver model identity.");
            if (body[12] > 1 || body[13] > 100)
                throw new InvalidDataException("Invalid EWEADN receiver battery fields.");
            if (body[14] == 0)
                return null;
            if (body[14] != 1)
                throw new InvalidDataException("Unknown EWEADN receiver connection state.");

            return new HidBatteryStatus(body[13], body[12] == 1);
        }

        private static HidBatteryStatus ReadFreqMouse(HidInterfaceInfo info, IHidTransport channel,
            CancellationToken ct, byte inputId, byte outputId, int queries)
        {
            if (info.InputReportLength != FreqReportLength || info.OutputReportLength != FreqReportLength)
                throw new NotSupportedException("Unsupported EWEADN FREQ report lengths.");

            HidBatteryStatus first = null;
            for (var attempt = 0; attempt < queries; attempt++)
            {
                var request = new byte[]
                {
                    outputId, 0x55, 0x30, 0x01, 0x0B, 0x2E, 0x01, 0x01, 0x01, 0x00, 0x00
                };
                var response = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                    report => IsFreqBatteryFrame(report, inputId), ct);
                var status = ParseFreqFrame(response, inputId);
                if (status == null)
                    throw new InvalidDataException("Invalid EWEADN FREQ battery response.");
                if (first == null)
                    first = status;
                else if (first.Percent != status.Percent || first.Charging != status.Charging)
                    return null;
            }

            return first;
        }

        internal static HidBatteryStatus ParseFreqFrame(byte[] report, byte inputId)
        {
            if (report == null || report.Length != FreqReportLength
                || (inputId != 0 && report[0] != inputId))
                return null;

            var offset = inputId == 0 ? 0 : 1;
            var bodyLength = report.Length - offset;
            var found = -1;
            for (var index = 0; index <= 1; index++)
            {
                if (index + 10 > bodyLength)
                    continue;
                if ((report[offset + index] == 0xAA || report[offset + index] == 0xF0)
                    && report[offset + index + 1] == FreqBatteryCommand)
                {
                    if (found >= 0)
                        return null;
                    found = index;
                }
            }
            if (found < 0)
                return null;

            var level = report[offset + found + 8];
            var charging = report[offset + found + 9];
            if (level > 100 || charging > 1)
                throw new InvalidDataException("Invalid EWEADN FREQ battery fields.");
            return new HidBatteryStatus(level, charging == 1);
        }

        private static HidBatteryStatus ReadS9Pro8K(IHidTransport channel, CancellationToken ct)
        {
            var request = new byte[64];
            request[0] = 0x03;
            request[1] = 0x20;
            request[2] = 0x50;
            request[5] = 0x4F;
            request[6] = 0x81;
            var report = HidBatteryExchange.Feature(channel, request, 0x03, 64, ct);
            return ParseS9Pro8KReply(report);
        }

        internal static HidBatteryStatus ParseS9Pro8KReply(byte[] report)
        {
            if (report == null || (report.Length != 16 && report.Length != 64)
                || report[0] != 0x03 || report[1] != 0x50)
                throw new InvalidDataException("Invalid EWEADN S9 Pro 8K feature response.");
            if (report[2] != 0 || report[5] != 1)
                return null;

            var levelAndCharging = report[6];
            var level = levelAndCharging & 0x7F;
            if (level < 1 || level > 100)
                throw new InvalidDataException("Invalid EWEADN S9 Pro 8K battery level.");
            return new HidBatteryStatus(level, (levelAndCharging & 0x80) != 0);
        }

        private static bool IsReceiverReply(byte[] report)
            => report != null && ((report.Length > 1 && report[0] == 0 && report[1] == FreqBatteryCommand)
                || (report.Length > 0 && report[0] == FreqBatteryCommand));

        private static byte[] ReceiverBody(byte[] report)
        {
            if (report.Length == ReceiverReportLength && report[0] == 0)
                return report.Skip(1).ToArray();
            if (report.Length == ReceiverReportLength - 1)
                return report;
            throw new InvalidDataException("Invalid EWEADN receiver report length or ID.");
        }

        private static bool IsFreqBatteryFrame(byte[] report, byte inputId)
        {
            if (report == null || report.Length != FreqReportLength
                || (inputId != 0 && report[0] != inputId))
                return false;
            var offset = inputId == 0 ? 0 : 1;
            return (report[offset] == 0xAA || report[offset] == 0xF0)
                    && report[offset + 1] == FreqBatteryCommand
                || (report[offset + 1] == 0xAA || report[offset + 1] == 0xF0)
                    && report[offset + 2] == FreqBatteryCommand;
        }

        private static bool TryGetUniqueNonzeroReportId(byte[] reportIds, out byte reportId)
        {
            reportId = 0;
            if (reportIds == null || reportIds.Length != 1 || reportIds[0] == 0)
                return false;
            reportId = reportIds[0];
            return true;
        }

        private static byte Sum(byte[] bytes, int first, int last)
        {
            var sum = 0;
            for (var index = first; index <= last; index++)
                sum += bytes[index];
            return (byte)sum;
        }
    }
}
