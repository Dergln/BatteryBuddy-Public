using System;
using System.Collections.Generic;
using System.Linq;

namespace BatteryBuddy.Core.Models
{
    public enum DeviceConnection
    {
        Unknown,
        Usb,
        UsbReceiver,
        Bluetooth,
        XInput,
        XInputWired,
        UsbXInputWired
    }

    public enum BatteryDiagnosticReason
    {
        NoBluetoothReading,
        NoUsbReading,
        ReceiverInterface,
        ReportedWired,
        UnknownXInputBattery,
        BatteryQueryFailed
    }

    /// <summary>Read-only discovery evidence, separate from battery values, settings and alerts.</summary>
    public sealed class DeviceDiagnostic
    {
        public string Id { get; }
        public string Name { get; }
        public DeviceCategory Category { get; }
        public DeviceConnection Connection { get; }
        public BatteryDiagnosticReason Reason { get; }
        public string BatteryDeviceId { get; }
        public IReadOnlyList<string> ConnectionIds { get; }

        public DeviceDiagnostic(string id, string name, DeviceCategory category, DeviceConnection connection,
            BatteryDiagnosticReason reason, string batteryDeviceId = null, IEnumerable<string> connectionIds = null)
        {
            Id = BatteryDevice.Sanitize(id, 512);
            if (Id.Length == 0)
                throw new ArgumentException("A diagnostic device needs an identity.", nameof(id));
            Name = BatteryDevice.Sanitize(name, BatteryDevice.MaxNameLength);
            if (Name.Length == 0)
                Name = "Device";
            Category = Enum.IsDefined(typeof(DeviceCategory), category) ? category : DeviceCategory.Other;
            Connection = connection;
            Reason = reason;
            BatteryDeviceId = BatteryDevice.Sanitize(batteryDeviceId, BatteryDevice.MaxIdLength);
            ConnectionIds = Array.AsReadOnly((connectionIds ?? Enumerable.Empty<string>())
                .Select(s => BatteryDevice.Sanitize(s, 512)).Where(s => s.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
        }

        public static IReadOnlyList<DeviceDiagnostic> WithoutBattery(IEnumerable<DeviceDiagnostic> diagnostics,
            IEnumerable<BatteryDevice> batteries)
        {
            var known = batteries.Where(b => b.Level.HasValue).ToList();
            return diagnostics.Where(d => !known.Any(b =>
                    string.Equals(b.Id, d.BatteryDeviceId, StringComparison.OrdinalIgnoreCase) ||
                    ((b.Category == d.Category || d.Connection == DeviceConnection.UsbReceiver) && b.ConnectionId.Length > 0 &&
                     d.ConnectionIds.Contains(b.ConnectionId, StringComparer.OrdinalIgnoreCase))))
                .GroupBy(d => d.Id, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();
        }
    }
}
