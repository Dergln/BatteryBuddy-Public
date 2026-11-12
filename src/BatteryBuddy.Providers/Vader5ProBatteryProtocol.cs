using System;
using System.IO;
using System.Threading;
using BatteryBuddy.Core.Providers;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class VaderBatteryProtocol
    {
        private const byte ReportId = 0;
        private const byte DeviceId = 130;
        private const ushort MinimumFirmware = 0x7141;
        private static readonly int[] ApproximatePercentages = { 10, 25, 40, 55, 70, 85, 100 };

        internal static HidBatteryStatus Read(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info == null || info.VendorId != 0x37D7 || info.ProductId != 0x2401
                || info.UsagePage != 0xFFA0 || info.Usage != 0x0001
                || info.InputReportLength != 32 || info.OutputReportLength != 32
                || !HidBatteryIdentity.IsUsbInterface(info.Path, 1)
                || !HasReportId(info.InputReportIds) || !HasReportId(info.OutputReportIds))
                throw new NotSupportedException("Unsupported Flydigi Vader 5 Pro HID collection.");

            var report = HidBatteryExchange.Output(channel, info.OutputReportLength,
                new byte[] { ReportId, 0x5A, 0xA5, 0x01, 0x02, 0x00 },
                IsGetInfoReply, ct);
            return ParseGetInfoReply(report);
        }

        internal static HidBatteryStatus ParseGetInfoReply(byte[] report)
        {
            if (report == null || report.Length == 0)
                throw new InvalidDataException("Empty Flydigi GET_INFO reply.");

            var offset = report[0] == 0x5A ? 0 : report[0] == ReportId ? 1 : -1;
            if (offset < 0 || report.Length < offset + 31
                || report[offset] != 0x5A || report[offset + 1] != 0xA5 || report[offset + 2] != 0x01)
                throw new InvalidDataException("Invalid Flydigi GET_INFO reply.");

            if (report[offset + 5] != DeviceId)
                throw new InvalidDataException("Unexpected Flydigi controller identity.");

            var firmware = (ushort)((report[offset + 15] << 8) | report[offset + 16]);
            if (firmware < MinimumFirmware)
                throw new InvalidDataException("Unsupported Flydigi controller firmware.");

            var stateAndLevel = report[offset + 11];
            var state = stateAndLevel >> 4;
            var level = stateAndLevel & 0x0F;
            if (state == 2)
                return new HidBatteryStatus(100, false);
            if (state != 0 && state != 1)
                throw new InvalidDataException("Unknown Flydigi battery status.");
            if (level >= ApproximatePercentages.Length)
                throw new InvalidDataException("Unknown Flydigi battery level.");

            return new HidBatteryStatus(ApproximatePercentages[level], state == 1);
        }

        private static bool IsGetInfoReply(byte[] report)
        {
            if (report == null || report.Length < 3)
                return false;
            var offset = report[0] == 0x5A ? 0 : report[0] == ReportId ? 1 : -1;
            return offset >= 0 && report.Length >= offset + 3
                && report[offset] == 0x5A && report[offset + 1] == 0xA5 && report[offset + 2] == 0x01;
        }

        private static bool HasReportId(byte[] reportIds)
            => reportIds != null && reportIds.Length == 1 && reportIds[0] == ReportId;
    }
}
