using System;
using System.IO;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class MouseAndKeyboardBatteryProtocols
    {
        internal static HidBatteryStatus ReadMchoseRealtek(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var request = new byte[65];
            request[0] = 0x12;
            request[1] = 0xF9;
            for (var index = 2; index < request.Length; index++)
                request[index] = 0xFF;
            for (var attempt = 0; attempt < 4; attempt++)
            {
                var report = HidBatteryExchange.Feature(channel, request, 0x12, 65, ct);
                if (report.Length >= 13 && report[0] == 0x12 && report[1] == 0xF9)
                {
                    var vendor = (report[2] ^ 0xFF) | ((report[3] ^ 0xFF) << 8);
                    if (vendor == 0x5253 || vendor == 0x3837)
                    {
                        var charging = report[12] ^ 0xFF;
                        if (charging > 1)
                            throw new InvalidDataException("Invalid MCHOSE charging state.");
                        return new HidBatteryStatus(report[11] ^ 0xFF, charging == 1);
                    }
                }
                if (ct.WaitHandle.WaitOne(120))
                    ct.ThrowIfCancellationRequested();
            }
            throw new TimeoutException("The MCHOSE mouse returned no active status.");
        }

        internal static HidBatteryStatus ReadMchoseG7(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength,
                new byte[] { 0, 0x55, 0x30, 0xA5, 0x0B, 0x2E, 1, 1, 1 },
                reply => HidBatteryExchange.StartsWith(reply, 0xAA, 0x30), ct);
            if (report.Length < 10 || report[9] > 1)
                throw new InvalidDataException("Invalid MCHOSE G7 battery report.");
            return new HidBatteryStatus(report[8], report[9] == 1);
        }

        internal static HidBatteryStatus ReadLofree(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            byte Command(byte command)
            {
                byte[] Exchange(byte value, Func<byte[], bool> accept, CancellationToken token)
                    => HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 4, 0, 0, value }, accept, token);
                ct.ThrowIfCancellationRequested();
                try
                {
                    Exchange(1, reply => reply.Length >= 4 && reply[0] == 4 && reply[3] == 1, ct);
                    var report = Exchange(command, reply => reply.Length >= 9 && reply[0] == 4
                        && reply[3] == command && reply[5] == 0 && reply[6] == 0, ct);
                    return report[8];
                }
                finally
                {
                    Exchange(2, reply => reply.Length >= 4 && reply[0] == 4 && reply[3] == 2, CancellationToken.None);
                }
            }
            if (Command(0xAA) != 1)
                return null;
            return new HidBatteryStatus(Command(0x1A), false);
        }
    }
}
