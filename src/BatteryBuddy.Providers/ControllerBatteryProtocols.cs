using System;
using System.IO;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class ControllerBatteryProtocols
    {
        private static int _nintendoCounter;
        internal static bool IsBluetooth(string path)
            => path.IndexOf("vid&", StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("{00001124-0000-1000-8000-00805f9b34fb}", StringComparison.OrdinalIgnoreCase) >= 0;

        internal static HidBatteryStatus ReadPlayStation(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var dualSense = info.ProductId == 0x0CE6 || info.ProductId == 0x0DF2;
            var bluetooth = IsBluetooth(info.Path);
            var reportId = bluetooth ? (dualSense ? 0x31 : 0x11) : 1;
            var offset = dualSense ? (bluetooth ? 54 : 53) : (bluetooth ? 32 : 30);
            (channel as IHidInputQueue)?.FlushInput();
            var report = HidBatteryExchange.Receive(channel, reply => reply.Length > offset && reply[0] == reportId, ct);
            if (info.ProductId == 0x0BA0)
            {
                if (report.Length <= 31)
                    throw new InvalidDataException("Missing DualShock adapter link state.");
                if ((report[31] & 4) != 0)
                    return null;
            }
            var raw = report[offset];
            var level = raw & 15;
            var state = raw >> 4;
            if (level > (dualSense ? 10 : 11))
                throw new InvalidDataException("Invalid PlayStation battery level.");
            if (dualSense && state != 0 && state != 1 && state != 2)
                throw new InvalidDataException("PlayStation controller reports a charging error.");
            return new HidBatteryStatus(Math.Min(level, 10) * 10,
                dualSense ? state == 1 || state == 2 : (raw & 0x10) != 0);
        }

        internal static HidBatteryStatus ReadEightBitDo(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var reportId = info.ProductId == 0x6009 || !IsBluetooth(info.Path) ? 4 : 1;
            (channel as IHidInputQueue)?.FlushInput();
            var report = HidBatteryExchange.Receive(channel, reply => reply.Length > 14
                && reply[0] == reportId && (reply[14] & 0x7F) != 0, ct);
            return new HidBatteryStatus(report[14] & 0x7F, (report[14] & 0x80) != 0);
        }

        internal static HidBatteryStatus ReadNintendo(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (!IsBluetooth(info.Path))
                throw new InvalidOperationException("Nintendo USB mode is not modified for battery polling.");
            var request = new byte[49];
            request[0] = 1;
            request[1] = (byte)(Interlocked.Increment(ref _nintendoCounter) & 15);
            for (var side = 0; side < 2; side++)
            {
                request[3 + side * 4] = 1;
                request[4 + side * 4] = 0x40;
                request[5 + side * 4] = 0x40;
            }
            request[10] = 2;
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                reply => reply.Length >= 3 && (reply[0] == 0x21 || reply[0] == 0x30 || reply[0] == 0x31), ct);
            var level = (report[2] & 0xE0) >> 4;
            if (level > 8)
                throw new InvalidDataException("Invalid Nintendo battery step.");
            return new HidBatteryStatus(level * 100 / 8, (report[2] & 0x10) != 0);
        }

        internal static HidBatteryStatus ReadSteam(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            (channel as IHidInputQueue)?.FlushInput();
            bool IsStatus(byte[] reply) => reply.Length >= 15 && reply[0] == 0x43
                || reply.Length >= 2 && (reply[0] == 0x79 || reply[0] == 0x46) && reply[1] == 1;
            var report = HidBatteryExchange.Receive(channel, IsStatus, ct, 5000, 4096);
            if (report[0] != 0x43)
                return null;
            if (report[1] != 1 && report[1] != 2 && report[1] != 4)
                throw new InvalidDataException("Invalid Steam Controller charge state.");
            return new HidBatteryStatus(report[2], report[1] != 1);
        }
    }
}
