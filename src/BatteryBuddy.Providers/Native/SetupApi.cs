using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BatteryBuddy.Providers.Native
{
    /// <summary>
    /// Schmale P/Invoke-Hülle um die Windows-SetupAPI (Geräteverwaltung).
    /// P/Invoke = Aufruf von nativen Windows-DLL-Funktionen aus C#. Wir verwenden nur lesende Funktionen.
    /// </summary>
    internal static class SetupApi
    {
        public const uint DIGCF_PRESENT = 0x02;
        public const uint DIGCF_ALLCLASSES = 0x04;
        public const uint DIGCF_DEVICEINTERFACE = 0x10;
        public static readonly IntPtr InvalidHandle = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public uint cbSize;
            public Guid InterfaceClassGuid;
            public uint Flags;
            public IntPtr Reserved;
        }

        /// <summary>Eine Geräteeigenschaft wird über GUID + Nummer identifiziert (DEVPROPKEY).</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVPROPKEY
        {
            public Guid fmtid;
            public uint pid;

            public DEVPROPKEY(string guid, uint pid)
            {
                fmtid = new Guid(guid);
                this.pid = pid;
            }
        }

        public const uint DEVPROP_TYPE_BYTE = 0x03;
        public const uint DEVPROP_TYPE_UINT32 = 0x07;
        public const uint DEVPROP_TYPE_FILETIME = 0x10;
        public const uint DEVPROP_TYPE_BOOLEAN = 0x11;
        public const uint DEVPROP_TYPE_STRING = 0x12;

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevsW(IntPtr classGuid, string enumerator, IntPtr hwndParent, uint flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevsW(ref Guid classGuid, string enumerator, IntPtr hwndParent, uint flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, uint index, ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, IntPtr deviceInfoData, ref Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInstanceIdW(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, StringBuilder deviceInstanceId, uint deviceInstanceIdSize, out uint requiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDevicePropertyW(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, ref DEVPROPKEY propertyKey, out uint propertyType, byte[] propertyBuffer, uint propertyBufferSize, out uint requiredSize, uint flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetailW(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        public static SP_DEVINFO_DATA NewDevInfo() => new SP_DEVINFO_DATA { cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA)) };
    }
}
