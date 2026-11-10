using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    // Wire facts: HeadsetControl 0c8a76f (GPL-3.0) and HaloBattery 2251f05 (MIT).
    // Only the battery query is sent; no initialization or settings commands.
    internal static class AudezeMaxwellBatteryProtocol
    {
        private const int ReportLength = 62;
        private const int MaxMessageLength = 1024;
        private const int PollIntervalMs = 50;

        internal static bool Matches(HidInterfaceInfo info)
        {
            if (info == null || info.VendorId != 0x3329 || info.UsagePage != 0xFF13 || info.Usage != 1
                || info.InputReportLength != ReportLength || info.OutputReportLength != ReportLength)
                return false;
            switch (info.ProductId)
            {
                case 0x4B18:
                case 0x4B19:
                    return HidBatteryIdentity.IsUsbInterface(info.Path, 0);
                case 0x4B28:
                case 0x4B29:
                    return HidBatteryIdentity.IsUsbInterface(info.Path, 5);
                case 0x4B1A:
                    return true; // Cable collection is measured, but its interface number is not established.
                default:
                    return false;
            }
        }

        internal static HidBatteryStatus Read(HidInterfaceInfo info, IHidTransport transport, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (!Matches(info))
                throw new NotSupportedException("Unsupported Audeze Maxwell HID collection.");
            if (transport == null)
                throw new ArgumentNullException(nameof(transport));
            if (!(transport is IHidInputReportTransport input))
                throw new NotSupportedException("Maxwell requires bounded HID GET_INPUT_REPORT support.");

            (transport as IHidInputQueue)?.FlushInput();
            // GET_INPUT_REPORT consumes a separate pending reply stream. Flushing ReadFile alone
            // is not proof that this stream is empty, nor does correlation prove a fresh radio sample.
            var drain = Stopwatch.StartNew();
            var drained = false;
            for (var index = 0; index < 8 && drain.ElapsedMilliseconds < 400; index++)
            {
                ct.ThrowIfCancellationRequested();
                var report = input.GetInputReport(7, ReportLength,
                    (int)Math.Max(1, Math.Min(100, 400 - drain.ElapsedMilliseconds)));
                if (report == null)
                    return null;
                var count = FragmentLength(report);
                if (count < 0)
                    continue;
                if (count == 0)
                {
                    drained = true;
                    break;
                }
            }
            if (!drained)
                return null;

            ct.ThrowIfCancellationRequested();
            var request = new byte[ReportLength];
            var command = new byte[] { 0x06, 0x07, 0x80, 0x05, 0x5A, 0x03, 0x00, 0xD6, 0x0C, 0x00 };
            Array.Copy(command, request, command.Length);
            if (!transport.Write(request))
                throw new IOException("Unable to request Maxwell battery status.");

            var stream = new List<byte>();
            var clock = Stopwatch.StartNew();
            for (var reports = 0; reports < 25 && clock.ElapsedMilliseconds < 1250; reports++)
            {
                var remaining = 1250 - (int)clock.ElapsedMilliseconds;
                if (ct.WaitHandle.WaitOne(Math.Min(PollIntervalMs, remaining)))
                    ct.ThrowIfCancellationRequested();
                remaining = 1250 - (int)clock.ElapsedMilliseconds;
                if (remaining <= 0)
                    break;
                var report = input.GetInputReport(7, ReportLength, Math.Min(100, remaining));
                ct.ThrowIfCancellationRequested();
                if (report == null)
                    continue;
                var length = FragmentLength(report);
                if (length <= 0)
                    continue;
                if (stream.Count + length > MaxMessageLength)
                    throw new InvalidDataException("Maxwell message stream exceeds the battery read budget.");
                for (var index = 0; index < length; index++)
                    stream.Add(report[index + 3]);
                while (TryMessage(stream, out var message))
                {
                    if (message.Length < 6 || message[4] != 0xD6 || message[5] != 0x0C)
                        continue;
                    if (message[1] == 0x5B)
                    {
                        if (message.Length != 7)
                            throw new InvalidDataException("Malformed Maxwell battery acknowledgement.");
                        if (message[6] != 0)
                            return null;
                    }
                    if (message[1] != 0x5D)
                        continue;
                    if (message.Length != 9 || message[6] != 0 || message[7] != 0 || message[8] > 100)
                        throw new InvalidDataException("Invalid Maxwell battery result.");
                    // This protocol has no charging flag. False means not measured, including on USB.
                    return new HidBatteryStatus(message[8], false);
                }
            }
            return null;
        }

        private static int FragmentLength(byte[] report)
        {
            if (report.Length == 0 || report[0] != 7)
                return -1;
            if (report.Length != ReportLength || report[1] > ReportLength - 3)
                throw new InvalidDataException("Malformed Maxwell input report.");
            return report[1];
        }

        private static bool TryMessage(List<byte> stream, out byte[] message)
        {
            message = null;
            while (stream.Count > 0 && stream[0] != 5)
                stream.RemoveAt(0);
            if (stream.Count < 4)
                return false;
            var length = 4 + stream[2] + (stream[3] << 8);
            if (length > MaxMessageLength)
                throw new InvalidDataException("Maxwell message exceeds the battery read budget.");
            if (stream.Count < length)
                return false;
            message = stream.GetRange(0, length).ToArray();
            stream.RemoveRange(0, length);
            return true;
        }
    }
}
