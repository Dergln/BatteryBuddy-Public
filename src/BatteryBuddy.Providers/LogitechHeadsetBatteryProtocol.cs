using System;
using System.IO;
using System.Linq;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class LogitechHeadsetBatteryProtocol
    {
        internal static HidBatteryStatus Read(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var g533 = info.ProductId == 0x0A66;
            var g535 = info.ProductId == 0x0AC4;
            var gpro = new ushort[] { 0x0ABA, 0x0AFB, 0x0AFC }.Contains(info.ProductId);
            var feature = g533 ? (byte)7 : g535 ? (byte)5 : gpro ? (byte)6 : (byte)8;
            var function = g533 ? (byte)1 : g535 || gpro ? (byte)0x0D : (byte)0x0A;
            var report = HidBatteryExchange.Output(channel, info.OutputReportLength,
                new byte[] { 0x11, 0xFF, feature, function },
                reply => HidBatteryExchange.StartsWith(reply, 0x11, 0xFF, feature, function)
                    || reply.Length >= 7 && (reply[0] == 0x11 || reply[0] == 0x10)
                        && reply[1] == 0xFF && reply[2] == 0xFF && reply[3] == feature && reply[4] == function, ct);
            if (report[2] == 0xFF)
                throw new InvalidDataException($"Logitech headset battery request rejected (error {report[5]:X2}).");
            if (report.Length < 7 || report[6] != 1 && report[6] != 3)
                throw new InvalidDataException("Invalid Logitech headset battery state.");
            var voltage = (report[4] << 8) | report[5];
            var curve = g533 ? new[] { 4200, 3850, 3790, 3750, 3680, 3330 }
                : g535 ? new[] { 4175, 3817, 3766, 3730, 3664, 3310 }
                : gpro ? new[] { 4150, 3830, 3780, 3740, 3670, 3320 }
                : new[] { 4100, 3950, 3850, 3750, 3650, 3500, 3300, 3150 };
            if (voltage < curve.Last())
                return null;
            if (voltage > 4500)
                throw new InvalidDataException("Invalid Logitech headset battery voltage.");
            var percentages = curve.Length == 6 ? new[] { 100, 50, 30, 20, 5, 0 }
                : new[] { 100, 80, 60, 40, 20, 10, 5, 0 };
            var percent = 100;
            for (var index = 1; index < curve.Length; index++)
                if (voltage < curve[index - 1] && voltage >= curve[index])
                {
                    percent = percentages[index] + (voltage - curve[index])
                        * (percentages[index - 1] - percentages[index]) / (curve[index - 1] - curve[index]);
                    break;
                }
            return new HidBatteryStatus(percent, report[6] == 3);
        }

        internal static HidBatteryStatus ReadG930(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var request = new byte[64];
            new byte[] { 0xFF, 9, 0, 0xFD, 0xF4, 0x10, 5, 0xB1, 0xBF, 0xA0, 4 }.CopyTo(request, 0);
            byte[] report = null;
            for (var index = 0; index < 3; index++)
            {
                if (index != 0 && ct.WaitHandle.WaitOne(100))
                    ct.ThrowIfCancellationRequested();
                report = HidBatteryExchange.Feature(channel, index == 0 ? request : null, 0xFF, 64, ct);
            }
            if (report.Length < 14 || report[0] != 0xFF || report[13] < 44 || report[13] > 91)
                throw new InvalidDataException("Invalid Logitech G930 battery feature report.");
            return new HidBatteryStatus((report[13] - 44) * 100 / 47, false);
        }
    }
}
