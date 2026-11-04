using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers.Bluetooth
{
    /// <summary>Ein Windows-Geräteknoten (PnP) mit den für uns relevanten Eigenschaften. Reine Datenklasse, daher testbar.</summary>
    internal sealed class PnpNode
    {
        public string InstanceId { get; set; }
        public string FriendlyName { get; set; }

        /// <summary>Ladestand in Prozent, falls Windows ihn für diesen Knoten kennt.</summary>
        public int? Battery { get; set; }

        /// <summary>Zeitpunkt der letzten Aktualisierung des Ladestands (UTC).</summary>
        public DateTime? BatteryTimeUtc { get; set; }

        public bool? IsConnected { get; set; }

        /// <summary>Bluetooth Class of Device (Gerätetyp-Code).</summary>
        public uint? ClassOfDevice { get; set; }
        public string ClassName { get; set; }
        public string ParentId { get; set; }
        public Guid? ContainerId { get; set; }
        public string BusName { get; set; }
        public string[] CompatibleIds { get; set; } = new string[0];
    }

    /// <summary>Liest PnP-Geräteknoten und deren Eigenschaften über die SetupAPI (nur lesend).</summary>
    internal static class PnpEnumerator
    {
        // Eigenschaftsschlüssel, mit denen Windows selbst den Bluetooth-Akkustand und -Status verwaltet.
        private static SetupApi.DEVPROPKEY _battery = new SetupApi.DEVPROPKEY("104EA319-6EE2-4701-BD47-8DDBF425BBE5", 2);
        private static SetupApi.DEVPROPKEY _batteryTime = new SetupApi.DEVPROPKEY("104EA319-6EE2-4701-BD47-8DDBF425BBE5", 7);
        private static SetupApi.DEVPROPKEY _connected = new SetupApi.DEVPROPKEY("83DA6326-97A6-4088-9453-A1923F573B29", 15);
        private static SetupApi.DEVPROPKEY _classOfDevice = new SetupApi.DEVPROPKEY("2BD67D8B-8BEB-48D5-87E0-6CDA3428040A", 4);
        private static SetupApi.DEVPROPKEY _friendlyName = new SetupApi.DEVPROPKEY("A45C254E-DF1C-4EFD-8020-67D146A850E0", 14);
        private static SetupApi.DEVPROPKEY _deviceDesc = new SetupApi.DEVPROPKEY("A45C254E-DF1C-4EFD-8020-67D146A850E0", 2);
        private static SetupApi.DEVPROPKEY _className = new SetupApi.DEVPROPKEY("A45C254E-DF1C-4EFD-8020-67D146A850E0", 9);
        private static SetupApi.DEVPROPKEY _compatibleIds = new SetupApi.DEVPROPKEY("A45C254E-DF1C-4EFD-8020-67D146A850E0", 4);
        private static SetupApi.DEVPROPKEY _parent = new SetupApi.DEVPROPKEY("4340A6C5-93FA-4706-972C-7B648008A5A7", 8);
        private static SetupApi.DEVPROPKEY _container = new SetupApi.DEVPROPKEY("8C7ED206-3F8A-4827-B3AB-AE9E1FAEFC6C", 2);
        private static SetupApi.DEVPROPKEY _busName = new SetupApi.DEVPROPKEY("540B947E-8B40-45BC-A8A2-6A0B894CBDA2", 4);

        /// <summary>Liefert alle aktuell vorhandenen Geräteknoten eines Enumerators (z. B. "BTHENUM" oder "BTHLE").</summary>
        public static List<PnpNode> Enumerate(string enumerator, CancellationToken cancellationToken = default, bool includeIdentity = false)
        {
            var result = new List<PnpNode>();
            var set = SetupApi.SetupDiGetClassDevsW(IntPtr.Zero, enumerator, IntPtr.Zero, SetupApi.DIGCF_PRESENT | SetupApi.DIGCF_ALLCLASSES);
            if (set == SetupApi.InvalidHandle || set == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows device discovery could not start.");

            try
            {
                for (uint i = 0; ; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var info = SetupApi.NewDevInfo();
                    if (!SetupApi.SetupDiEnumDeviceInfo(set, i, ref info))
                    {
                        var error = Marshal.GetLastWin32Error();
                        if (error == 259) break;
                        throw new Win32Exception(error, "Windows device discovery did not finish.");
                    }

                    var id = new StringBuilder(260);
                    if (!SetupApi.SetupDiGetDeviceInstanceIdW(set, ref info, id, (uint)id.Capacity, out _))
                        continue;

                    var node = new PnpNode { InstanceId = id.ToString() };
                    node.FriendlyName = ReadString(set, ref info, ref _friendlyName) ?? ReadString(set, ref info, ref _deviceDesc);

                    var battery = ReadBytes(set, ref info, ref _battery, SetupApi.DEVPROP_TYPE_BYTE, 1);
                    if (battery != null && battery[0] <= 100)
                        node.Battery = battery[0];

                    var time = ReadBytes(set, ref info, ref _batteryTime, SetupApi.DEVPROP_TYPE_FILETIME, 8);
                    if (time != null)
                    {
                        var fileTime = BitConverter.ToInt64(time, 0);
                        // Gültigen FILETIME-Bereich prüfen, bevor umgerechnet wird (verhindert ArgumentOutOfRange bei Müllwerten).
                        if (fileTime > 0 && fileTime < DateTime.MaxValue.ToFileTimeUtc())
                            node.BatteryTimeUtc = DateTime.FromFileTimeUtc(fileTime);
                    }

                    var connected = ReadBytes(set, ref info, ref _connected, SetupApi.DEVPROP_TYPE_BOOLEAN, 1);
                    if (connected != null)
                        node.IsConnected = connected[0] != 0;

                    var cod = ReadBytes(set, ref info, ref _classOfDevice, SetupApi.DEVPROP_TYPE_UINT32, 4);
                    if (cod != null)
                        node.ClassOfDevice = BitConverter.ToUInt32(cod, 0);

                    if (includeIdentity)
                    {
                        node.ClassName = ReadString(set, ref info, ref _className);
                        node.ParentId = ReadString(set, ref info, ref _parent);
                        node.BusName = ReadString(set, ref info, ref _busName);
                        var container = ReadBytes(set, ref info, ref _container, SetupApi.DEVPROP_TYPE_GUID, 16);
                        if (container != null && new Guid(container) != Guid.Empty)
                            node.ContainerId = new Guid(container);
                        node.CompatibleIds = (ReadText(set, ref info, ref _compatibleIds, SetupApi.DEVPROP_TYPE_STRING_LIST) ?? string.Empty)
                            .Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    }

                    result.Add(node);
                }
            }
            finally
            {
                SetupApi.SetupDiDestroyDeviceInfoList(set);
            }

            return result;
        }

        /// <summary>Liest eine Eigenschaft fester Größe. Gibt null zurück, wenn sie fehlt oder einen unerwarteten Typ/Größe hat.</summary>
        private static byte[] ReadBytes(IntPtr set, ref SetupApi.SP_DEVINFO_DATA info, ref SetupApi.DEVPROPKEY key, uint expectedType, int expectedSize)
        {
            var buffer = new byte[expectedSize];
            if (!SetupApi.SetupDiGetDevicePropertyW(set, ref info, ref key, out var type, buffer, (uint)buffer.Length, out var required, 0))
                return null;

            return type == expectedType && required == expectedSize ? buffer : null;
        }

        private static string ReadString(IntPtr set, ref SetupApi.SP_DEVINFO_DATA info, ref SetupApi.DEVPROPKEY key)
            => ReadText(set, ref info, ref key, SetupApi.DEVPROP_TYPE_STRING);

        private static string ReadText(IntPtr set, ref SetupApi.SP_DEVINFO_DATA info, ref SetupApi.DEVPROPKEY key, uint expectedType)
        {
            var buffer = new byte[4096];
            if (!SetupApi.SetupDiGetDevicePropertyW(set, ref info, ref key, out var type, buffer, (uint)buffer.Length, out var required, 0)
                || type != expectedType || required < 2 || required > buffer.Length || required % 2 != 0)
                return null;

            return Encoding.Unicode.GetString(buffer, 0, (int)required).TrimEnd('\0');
        }
    }
}
