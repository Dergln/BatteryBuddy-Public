using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;

namespace BatteryBuddy.Providers.Native
{
    internal sealed class HidBatteryProvider : IBatteryProvider
    {
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private readonly Func<IReadOnlyList<HidInterfaceInfo>> _enumerate;
        private readonly Func<HidInterfaceInfo, IHidTransport> _open;
        private readonly Func<HidInterfaceInfo, bool> _matches;
        private readonly Func<HidInterfaceInfo, IHidTransport, CancellationToken, HidBatteryStatus> _read;
        private readonly string _idPrefix;
        private readonly Func<HidInterfaceInfo, string> _deviceName;
        private readonly DeviceCategory _category;

        public string Name { get; }
        internal string ProtocolId { get; }

        internal HidBatteryProvider(string name, string protocolId, string idPrefix, DeviceCategory category,
            Func<HidInterfaceInfo, string> deviceName, Func<IReadOnlyList<HidInterfaceInfo>> enumerate,
            Func<HidInterfaceInfo, bool> matches,
            Func<HidInterfaceInfo, IHidTransport, CancellationToken, HidBatteryStatus> read,
            Func<HidInterfaceInfo, IHidTransport> open = null)
        {
            Name = name;
            ProtocolId = protocolId;
            _idPrefix = idPrefix;
            _category = category;
            _deviceName = deviceName;
            _enumerate = enumerate;
            _matches = matches;
            _read = read;
            _open = open ?? HidNative.Open;
        }

        public async Task<IReadOnlyList<BatteryDevice>> ReadAsync(CancellationToken cancellationToken)
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await Task.Run<IReadOnlyList<BatteryDevice>>(() =>
                {
                    var matches = _enumerate().Where(_matches).Take(9).ToArray();
                    if (matches.Length > 8)
                        throw new InvalidOperationException("Too many HID battery interfaces for one protocol.");
                    var devices = new List<BatteryDevice>();
                    foreach (var info in matches)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var id = HidBatteryIdentity.Create(_idPrefix, info.Path);
                        using (var channel = _open(info) ?? throw new IOException("Unable to open the HID battery interface."))
                        {
                            var status = _read(info, channel, cancellationToken);
                            if (status != null)
                                devices.Add(new BatteryDevice(id, _deviceName(info), _category, status.Percent, status.Charging,
                                    Name, connectionId: (channel as IHidDeviceIdentity)?.DeviceInstanceId));
                        }
                    }
                    return devices;
                }, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
