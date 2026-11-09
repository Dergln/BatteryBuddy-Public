using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers.SteelSeries
{
    public sealed class Nova7BatteryProvider : IBatteryProvider
    {
        private const ushort VendorId = 0x1038;
        private const int TimeoutMs = 1000;
        private const int MaxInterfaces = 8;
        private readonly SemaphoreSlim _pollGate = new SemaphoreSlim(1, 1);
        private readonly Func<IReadOnlyList<HidInterfaceInfo>> _enumerate;
        private readonly Func<HidInterfaceInfo, IHidTransport> _open;

        public string Name => "SteelSeries Nova 7";

        public Nova7BatteryProvider() : this(() => HidNative.Enumerate(VendorId), HidNative.Open) { }

        internal Nova7BatteryProvider(Func<IReadOnlyList<HidInterfaceInfo>> enumerate, Func<HidInterfaceInfo, IHidTransport> open)
        {
            _enumerate = enumerate;
            _open = open;
        }

        public async Task<IReadOnlyList<BatteryDevice>> ReadAsync(CancellationToken cancellationToken)
        {
            await _pollGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await Task.Run(() => Poll(cancellationToken), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _pollGate.Release();
            }
        }

        private IReadOnlyList<BatteryDevice> Poll(CancellationToken cancellationToken)
        {
            var devices = new List<BatteryDevice>();
            var matches = _enumerate().Where(info => DeviceCatalog.AllowsCustomPolling(DeviceCatalog.SteelSeriesNova7,
                info.VendorId, info.ProductId, info.UsagePage, info.Usage, info.InputReportLength, info.OutputReportLength)
                && HidBatteryIdentity.IsUsbInterface(info.Path, 3))
                .Take(MaxInterfaces + 1).ToArray();
            if (matches.Length > MaxInterfaces)
                throw new InvalidOperationException("Too many SteelSeries interfaces to poll safely.");

            foreach (var info in matches)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(info.Path))
                    throw new InvalidOperationException("The SteelSeries interface has no stable identity.");

                using (var channel = _open(info) ?? throw new IOException("Unable to open the SteelSeries receiver."))
                {
                    var request = new byte[info.OutputReportLength];
                    request[1] = 0xB0;
                    if (!channel.Write(request))
                        throw new IOException("Unable to request SteelSeries battery status.");

                    var clock = Stopwatch.StartNew();
                    var statusReceived = false;
                    while (clock.ElapsedMilliseconds < TimeoutMs)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var remaining = (int)Math.Max(1, TimeoutMs - clock.ElapsedMilliseconds);
                        var report = channel.Read(Math.Min(remaining, 100));
                        if (report == null)
                            continue;
                        var offset = report.Length > 0 && report[0] == 0 ? 1 : 0;
                        if (report.Length <= offset || report[offset] != 0xB0)
                            continue;
                        if (report.Length < offset + 4)
                            throw new InvalidDataException("Short SteelSeries status report.");

                        statusReceived = true;
                        var link = report[offset + 1];
                        var level = report[offset + 2];
                        var state = report[offset + 3];
                        if (link == 0x02 || state == 0x00)
                            break;
                        if (link != 0x03 || level > 100 || (state != 0x01 && state != 0x02 && state != 0x03))
                            throw new InvalidDataException("Invalid SteelSeries status report.");

                        devices.Add(new BatteryDevice(HidBatteryIdentity.Create("steelseries:", info.Path), "SteelSeries Arctis Nova 7",
                            DeviceCategory.Headset, level, state == 0x01 || state == 0x02, Name,
                            connectionId: (channel as IHidDeviceIdentity)?.DeviceInstanceId));
                        break;
                    }
                    if (!statusReceived)
                        throw new TimeoutException("The SteelSeries receiver did not return a status report.");
                }
            }
            return devices;
        }

    }
}
