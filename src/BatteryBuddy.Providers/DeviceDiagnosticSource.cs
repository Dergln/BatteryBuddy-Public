using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatteryBuddy.Core.Logic;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;
using BatteryBuddy.Providers.Bluetooth;

namespace BatteryBuddy.Providers
{
    public sealed class DeviceDiagnosticSource : IDeviceDiagnosticSource
    {
        public string Name => "DeviceBuddy";

        public Task<IReadOnlyList<DeviceDiagnostic>> ReadAsync(CancellationToken cancellationToken)
            => Task.Run(() => Build(PnpEnumerator.Enumerate(null, cancellationToken, includeIdentity: true),
                XInputBatteryProvider.ReadConnectedSlots(cancellationToken), cancellationToken), cancellationToken);

        private sealed class UsbCandidate
        {
            public DeviceDiagnostic Diagnostic;
            public bool IsXInput;
        }

        internal static IReadOnlyList<DeviceDiagnostic> Build(IReadOnlyList<PnpNode> nodes,
            IReadOnlyList<XInputBatteryProvider.SlotReading> slots, CancellationToken cancellationToken = default)
        {
            var result = new List<DeviceDiagnostic>();
            var byId = nodes.ToDictionary(n => n.InstanceId, StringComparer.OrdinalIgnoreCase);
            var bluetooth = nodes.Where(n => BluetoothBatteryProvider.ExtractAddress(n.InstanceId) != null)
                .GroupBy(n => BluetoothBatteryProvider.ExtractAddress(n.InstanceId), StringComparer.OrdinalIgnoreCase);
            foreach (var group in bluetooth)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var all = group.ToList();
                if (!all.Any(n => n.IsConnected == true) || all.Any(n => n.Battery.HasValue))
                    continue;
                var root = all.FirstOrDefault(n => n.InstanceId.StartsWith(@"BTHENUM\DEV_", StringComparison.OrdinalIgnoreCase)
                    || n.InstanceId.StartsWith(@"BTHLE\DEV_", StringComparison.OrdinalIgnoreCase));
                var name = BluetoothBatteryProvider.CleanName(root?.FriendlyName ?? all[0].FriendlyName);
                var cod = all.Select(n => n.ClassOfDevice).FirstOrDefault(c => c.HasValue && c != 0);
                var category = CategoryClassifier.FromBluetooth(cod, name);
                if (!IsPeripheral(category) && (!cod.HasValue ||
                    !new uint[] { 2, 4, 5 }.Contains((cod.Value >> 8) & 0x1F))) continue;
                var id = "bt:" + group.Key.ToUpperInvariant();
                result.Add(new DeviceDiagnostic(id, name, category, DeviceConnection.Bluetooth,
                    BatteryDiagnosticReason.NoBluetoothReading, id, all.Select(n => n.InstanceId)));
            }

            var usbNodes = new List<Tuple<PnpNode, PnpNode, DeviceCategory>>();
            foreach (var node in nodes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var chain = Ancestors(node, byId);
                if (chain.Any(n => n.InstanceId.StartsWith("BTH", StringComparison.OrdinalIgnoreCase)))
                    continue;
                var usb = chain.FirstOrDefault(n => n.InstanceId.StartsWith(@"USB\VID_", StringComparison.OrdinalIgnoreCase));
                if (usb == null) continue;
                // Composite interfaces share a container with their device, not with the upstream USB hub.
                if (usb.ContainerId.HasValue)
                    usb = chain.Last(n => n.ContainerId == usb.ContainerId &&
                        n.InstanceId.StartsWith(@"USB\VID_", StringComparison.OrdinalIgnoreCase));
                var category = UsbCategory(node);
                if (!IsPeripheral(category)) continue;
                usbNodes.Add(Tuple.Create(node, usb, category));
            }

            var candidates = new List<UsbCandidate>();
            foreach (var group in usbNodes.GroupBy(n =>
                (n.Item2.ContainerId?.ToString() ?? n.Item2.InstanceId) + ":" + n.Item3, StringComparer.OrdinalIgnoreCase))
            {
                var first = group.First();
                var usb = first.Item2;
                var identities = nodes.Where(n => usb.ContainerId.HasValue && n.ContainerId == usb.ContainerId)
                    .Concat(group.Select(n => n.Item1)).Concat(group.Select(n => n.Item2)).Distinct().ToList();
                if (identities.Any(n => n.Battery.HasValue)) continue;
                var names = group.Select(n => n.Item1.BusName).Concat(group.Select(n => n.Item1.FriendlyName));
                var name = !string.IsNullOrWhiteSpace(usb.BusName) ? usb.BusName :
                    names.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n) && !IsGenericName(n)) ??
                    usb.FriendlyName ?? first.Item1.FriendlyName;
                var receiver = Has(usb.BusName, "receiver", "dongle", "empfänger") ||
                    Has(usb.FriendlyName, "receiver", "dongle", "empfänger");
                candidates.Add(new UsbCandidate
                {
                    Diagnostic = new DeviceDiagnostic("usb:" + group.Key, name, first.Item3,
                        receiver ? DeviceConnection.UsbReceiver : DeviceConnection.Usb,
                        receiver ? BatteryDiagnosticReason.ReceiverInterface : BatteryDiagnosticReason.NoUsbReading,
                        connectionIds: identities.Select(n => n.InstanceId)),
                    IsXInput = group.Any(n => IsXInputNode(n.Item1) || IsXInputNode(n.Item2))
                });
            }

            var xinputUsb = candidates.Where(c => c.IsXInput && c.Diagnostic.Category == DeviceCategory.Gamepad).ToList();
            // XInput exposes slots, not PnP identities. Only join an unambiguous single-controller setup.
            var singleMatch = xinputUsb.Count == 1 && slots.Count == 1 &&
                !nodes.Any(n => n.IsConnected == true &&
                    CategoryClassifier.FromBluetooth(n.ClassOfDevice, n.FriendlyName) == DeviceCategory.Gamepad &&
                    n.InstanceId.StartsWith("BTH", StringComparison.OrdinalIgnoreCase));
            foreach (var candidate in candidates)
            {
                if (candidate.IsXInput && slots.Count > 0)
                {
                    if (singleMatch && ReferenceEquals(candidate, xinputUsb[0]))
                    {
                        var d = candidate.Diagnostic;
                        var slot = slots[0];
                        if (slot.BatteryError == 0 && XInputBatteryProvider.ToPercent(slot.BatteryType, slot.BatteryLevel).HasValue)
                            continue;
                        result.Add(new DeviceDiagnostic(d.Id, d.Name, d.Category,
                            slot.BatteryError == 0 && slot.BatteryType == 1 ? DeviceConnection.UsbXInputWired : d.Connection,
                            Reason(slot), "xinput:" + slot.Slot, d.ConnectionIds));
                    }
                    // Ambiguous XInput identities are represented by their slots below, not guessed names.
                    continue;
                }
                result.Add(candidate.Diagnostic);
            }

            if (!singleMatch)
            {
                foreach (var slot in slots)
                {
                    if (slot.BatteryError == 0 && XInputBatteryProvider.ToPercent(slot.BatteryType, slot.BatteryLevel).HasValue)
                        continue;
                    result.Add(new DeviceDiagnostic("xinput:" + slot.Slot, "Controller " + (slot.Slot + 1),
                        DeviceCategory.Gamepad,
                        slot.BatteryError == 0 && slot.BatteryType == 1 ? DeviceConnection.XInputWired : DeviceConnection.XInput,
                        Reason(slot), "xinput:" + slot.Slot));
                }
            }
            return result;
        }

        private static BatteryDiagnosticReason Reason(XInputBatteryProvider.SlotReading slot)
            => slot.BatteryError != 0 ? BatteryDiagnosticReason.BatteryQueryFailed :
                slot.BatteryType == 1 ? BatteryDiagnosticReason.ReportedWired : BatteryDiagnosticReason.UnknownXInputBattery;

        private static bool IsPeripheral(DeviceCategory category)
            => category != DeviceCategory.Other && category != DeviceCategory.InternalBattery;

        private static bool IsXInputNode(PnpNode node)
            => string.Equals(node.ClassName, "XnaComposite", StringComparison.OrdinalIgnoreCase) ||
                node.CompatibleIds.Any(id => Has(id, "MS_COMP_XUSB"));

        private static DeviceCategory UsbCategory(PnpNode node)
        {
            if (IsXInputNode(node)) return DeviceCategory.Gamepad;
            if (string.Equals(node.ClassName, "Mouse", StringComparison.OrdinalIgnoreCase)) return DeviceCategory.Mouse;
            if (string.Equals(node.ClassName, "Keyboard", StringComparison.OrdinalIgnoreCase)) return DeviceCategory.Keyboard;
            foreach (var id in node.CompatibleIds)
            {
                if (Has(id, "HID_DEVICE_UP:0001_U:0002")) return DeviceCategory.Mouse;
                if (Has(id, "HID_DEVICE_UP:0001_U:0006")) return DeviceCategory.Keyboard;
                if (Has(id, "HID_DEVICE_UP:0001_U:0004", "HID_DEVICE_UP:0001_U:0005")) return DeviceCategory.Gamepad;
                if (Has(id, "HID_DEVICE_UP:000D_U:0002", "HID_DEVICE_UP:000D_U:0020")) return DeviceCategory.Pen;
            }
            if (!Has(node.ClassName, "HIDClass", "MEDIA", "AudioEndpoint", "WPD"))
                return DeviceCategory.Other;
            var category = CategoryClassifier.FromName(node.BusName ?? node.FriendlyName);
            if (category == DeviceCategory.Phone && !string.Equals(node.ClassName, "WPD", StringComparison.OrdinalIgnoreCase))
                return DeviceCategory.Other;
            // RGB/fan controllers also use HID; "controller" alone is not evidence of a gamepad.
            if (category == DeviceCategory.Gamepad && !Has(node.BusName ?? node.FriendlyName,
                "gamepad", "joystick", "xbox", "dualsense", "dualshock", "joy-con"))
                return DeviceCategory.Other;
            return category;
        }

        private static bool IsGenericName(string name)
            => Has(name, "HID-compliant", "HID-konform", "USB Input Device", "USB-Eingabegerät", "USB Composite");

        private static bool Has(string value, params string[] parts)
            => value != null && parts.Any(p => value.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0);

        private static List<PnpNode> Ancestors(PnpNode node, IReadOnlyDictionary<string, PnpNode> byId)
        {
            var result = new List<PnpNode>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (node != null && visited.Add(node.InstanceId) && result.Count < 32)
            {
                result.Add(node);
                node = node.ParentId != null && byId.TryGetValue(node.ParentId, out var parent) ? parent : null;
            }
            return result;
        }
    }
}
