using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace BatteryBuddy.Providers.Native
{
    internal static class HidBatteryExchange
    {
        internal const int TimeoutMs = 1000;

        internal static byte[] Output(IHidTransport channel, int reportLength, byte[] command,
            Func<byte[], bool> isResponse, CancellationToken cancellationToken, int settleMs = 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (reportLength <= 0 || command.Length > reportLength || reportLength > 1024)
                throw new InvalidOperationException("The HID battery request does not fit its report descriptor.");
            var report = new byte[reportLength];
            Array.Copy(command, report, command.Length);
            (channel as IHidInputQueue)?.FlushInput();
            if (!channel.Write(report))
                throw new IOException("Unable to request HID battery status.");
            if (settleMs > 0 && cancellationToken.WaitHandle.WaitOne(settleMs))
                cancellationToken.ThrowIfCancellationRequested();
            return Receive(channel, isResponse, cancellationToken);
        }

        internal static byte[] Receive(IHidTransport channel, Func<byte[], bool> isResponse,
            CancellationToken cancellationToken, int timeoutMs = TimeoutMs, int maxReports = 128,
            Func<long> elapsedMilliseconds = null)
        {
            if (timeoutMs <= 0 || timeoutMs > 5000 || maxReports <= 0 || maxReports > 4096)
                throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Invalid HID receive budget.");
            var clock = Stopwatch.StartNew();
            // Per-call clock injection lets transport fixtures advance time without scheduler-dependent sleeps.
            var elapsed = elapsedMilliseconds ?? (() => clock.ElapsedMilliseconds);
            var reports = 0;
            while (elapsed() < timeoutMs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var report = channel.Read((int)Math.Max(1, Math.Min(100, timeoutMs - elapsed())));
                if (report == null)
                    continue;
                if (isResponse(report))
                    return report;
                if (++reports >= maxReports)
                    throw new InvalidDataException("Too much unrelated HID traffic while reading battery status.");
            }
            throw new TimeoutException("The HID device did not return the requested battery status.");
        }

        internal static byte[] Feature(IHidTransport channel, byte[] request, byte reportId, int responseLength,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!(channel is IHidFeatureTransport feature))
                throw new NotSupportedException("The HID channel does not support feature reports.");
            if (request != null && !feature.SetFeature(request, TimeoutMs))
                throw new IOException("Unable to request HID feature battery status.");
            cancellationToken.ThrowIfCancellationRequested();
            return feature.GetFeature(reportId, responseLength, TimeoutMs)
                ?? throw new IOException("Unable to read HID feature battery status.");
        }

        internal static HidBatteryStatus FeatureStatus(IHidTransport channel, byte[] request,
            Func<byte[], HidBatteryStatus> parse, CancellationToken ct)
        {
            var clock = Stopwatch.StartNew();
            for (var attempt = 0; attempt < 15 && clock.ElapsedMilliseconds < TimeoutMs; attempt++)
            {
                var report = Feature(channel, attempt == 0 ? request : null, 0, request.Length, ct);
                var status = parse(report);
                if (status != null)
                    return status;
                if (ct.WaitHandle.WaitOne(50))
                    ct.ThrowIfCancellationRequested();
            }
            throw new TimeoutException("No active device battery reply to the HID feature request.");
        }

        internal static bool StartsWith(byte[] report, params byte[] prefix)
        {
            if (report.Length < prefix.Length)
                return false;
            for (var index = 0; index < prefix.Length; index++)
                if (report[index] != prefix[index])
                    return false;
            return true;
        }

        internal static byte[] WithoutZeroReportId(byte[] report)
        {
            if (report.Length == 0 || report[0] != 0)
                return report;
            var payload = new byte[report.Length - 1];
            Array.Copy(report, 1, payload, 0, payload.Length);
            return payload;
        }
    }
}
