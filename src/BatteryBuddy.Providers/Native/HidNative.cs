using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace BatteryBuddy.Providers.Native
{
    /// <summary>Beschreibt eine HID-Schnittstelle (ein "Gerätekanal" für Eingabegeräte wie Mäuse/Empfänger).</summary>
    internal sealed class HidInterfaceInfo
    {
        public string Path { get; set; }
        public ushort VendorId { get; set; }
        public ushort ProductId { get; set; }
        public ushort UsagePage { get; set; }
        public ushort Usage { get; set; }
        public int InputReportLength { get; set; }
        public int OutputReportLength { get; set; }
    }

    /// <summary>Schnittstelle für den Datenaustausch mit einem HID-Gerät. Als Interface, damit Tests ein Fake-Gerät nutzen können.</summary>
    internal interface IHidTransport : IDisposable
    {
        /// <summary>Sendet einen Output-Report (erstes Byte = Report-ID). Gibt false bei Fehler zurück.</summary>
        bool Write(byte[] report);

        /// <summary>Wartet bis zu <paramref name="timeoutMs"/> auf einen Input-Report. Null = nichts empfangen.</summary>
        byte[] Read(int timeoutMs);
    }

    /// <summary>Windows-HID-Zugriff über hid.dll/kernel32.dll (P/Invoke).</summary>
    internal static class HidNative
    {
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 1;
        private const uint FILE_SHARE_WRITE = 2;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        private const uint WAIT_OBJECT_0 = 0;
        private const int ERROR_IO_PENDING = 997;

        [StructLayout(LayoutKind.Sequential)]
        private struct HIDD_ATTRIBUTES
        {
            public int Size;
            public ushort VendorID;
            public ushort ProductID;
            public ushort VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            public ushort InputReportByteLength;
            public ushort OutputReportByteLength;
            public ushort FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)] public ushort[] Reserved;
            public ushort NumberLinkCollectionNodes;
            public ushort NumberInputButtonCaps;
            public ushort NumberInputValueCaps;
            public ushort NumberInputDataIndices;
            public ushort NumberOutputButtonCaps;
            public ushort NumberOutputValueCaps;
            public ushort NumberOutputDataIndices;
            public ushort NumberFeatureButtonCaps;
            public ushort NumberFeatureValueCaps;
            public ushort NumberFeatureDataIndices;
        }

        [DllImport("hid.dll")] private static extern void HidD_GetHidGuid(out Guid hidGuid);
        [DllImport("hid.dll", SetLastError = true)] private static extern bool HidD_GetAttributes(SafeFileHandle handle, ref HIDD_ATTRIBUTES attributes);
        [DllImport("hid.dll", SetLastError = true)] private static extern bool HidD_GetPreparsedData(SafeFileHandle handle, out IntPtr preparsedData);
        [DllImport("hid.dll", SetLastError = true)] private static extern bool HidD_FreePreparsedData(IntPtr preparsedData);
        [DllImport("hid.dll")] private static extern int HidP_GetCaps(IntPtr preparsedData, out HIDP_CAPS capabilities);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFileW(string fileName, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(SafeFileHandle handle, IntPtr buffer, uint bytesToRead, IntPtr bytesRead, IntPtr overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(SafeFileHandle handle, IntPtr buffer, uint bytesToWrite, IntPtr bytesWritten, IntPtr overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetOverlappedResult(SafeFileHandle handle, IntPtr overlapped, out uint transferred, bool wait);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CancelIoEx(SafeFileHandle handle, IntPtr overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateEventW(IntPtr attributes, bool manualReset, bool initialState, string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        /// <summary>Listet alle vorhandenen HID-Schnittstellen eines Herstellers (Vendor-ID) auf.</summary>
        public static List<HidInterfaceInfo> Enumerate(ushort vendorId)
        {
            var result = new List<HidInterfaceInfo>();
            HidD_GetHidGuid(out var hidGuid);
            var set = SetupApi.SetupDiGetClassDevsW(ref hidGuid, null, IntPtr.Zero, SetupApi.DIGCF_PRESENT | SetupApi.DIGCF_DEVICEINTERFACE);
            if (set == SetupApi.InvalidHandle || set == IntPtr.Zero)
                return result;

            try
            {
                for (uint i = 0; ; i++)
                {
                    var iface = new SetupApi.SP_DEVICE_INTERFACE_DATA { cbSize = (uint)Marshal.SizeOf(typeof(SetupApi.SP_DEVICE_INTERFACE_DATA)) };
                    if (!SetupApi.SetupDiEnumDeviceInterfaces(set, IntPtr.Zero, ref hidGuid, i, ref iface))
                        break;

                    var path = GetInterfacePath(set, ref iface);
                    if (path == null)
                        continue;

                    var info = Query(path, vendorId);
                    if (info != null)
                        result.Add(info);
                }
            }
            finally
            {
                SetupApi.SetupDiDestroyDeviceInfoList(set);
            }

            return result;
        }

        private static string GetInterfacePath(IntPtr set, ref SetupApi.SP_DEVICE_INTERFACE_DATA iface)
        {
            SetupApi.SetupDiGetDeviceInterfaceDetailW(set, ref iface, IntPtr.Zero, 0, out var required, IntPtr.Zero);
            if (required < 8 || required > 4096)
                return null;

            var buffer = Marshal.AllocHGlobal((int)required);
            try
            {
                // cbSize der Detail-Struktur: 8 auf 64-Bit, 6 auf 32-Bit (Unicode). Das verlangt die Windows-API so.
                Marshal.WriteInt32(buffer, IntPtr.Size == 8 ? 8 : 6);
                if (!SetupApi.SetupDiGetDeviceInterfaceDetailW(set, ref iface, buffer, required, out _, IntPtr.Zero))
                    return null;

                return Marshal.PtrToStringUni(IntPtr.Add(buffer, 4));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>Öffnet die Schnittstelle OHNE Zugriffsrechte (nur zum Abfragen der Eigenschaften; stört keine andere Software).</summary>
        private static HidInterfaceInfo Query(string path, ushort vendorId)
        {
            using (var handle = CreateFileW(path, 0, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero))
            {
                if (handle.IsInvalid)
                    return null;

                var attrs = new HIDD_ATTRIBUTES { Size = Marshal.SizeOf(typeof(HIDD_ATTRIBUTES)) };
                if (!HidD_GetAttributes(handle, ref attrs) || attrs.VendorID != vendorId)
                    return null;

                if (!HidD_GetPreparsedData(handle, out var preparsed))
                    return null;

                try
                {
                    if (HidP_GetCaps(preparsed, out var caps) < 0)
                        return null;

                    return new HidInterfaceInfo
                    {
                        Path = path,
                        VendorId = attrs.VendorID,
                        ProductId = attrs.ProductID,
                        UsagePage = caps.UsagePage,
                        Usage = caps.Usage,
                        InputReportLength = caps.InputReportByteLength,
                        OutputReportLength = caps.OutputReportByteLength
                    };
                }
                finally
                {
                    HidD_FreePreparsedData(preparsed);
                }
            }
        }

        /// <summary>Öffnet eine HID-Schnittstelle zum Lesen/Schreiben. Null, wenn der Zugriff verweigert wird.</summary>
        public static IHidTransport Open(HidInterfaceInfo info)
        {
            var handle = CreateFileW(info.Path, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, IntPtr.Zero);
            if (handle.IsInvalid)
            {
                handle.Dispose();
                return null;
            }

            return new OverlappedStream(handle, info.InputReportLength, info.OutputReportLength);
        }

        /// <summary>
        /// Lesen/Schreiben mit Zeitlimit. "Overlapped" = asynchrones Windows-I/O: Der Aufruf kehrt sofort zurück,
        /// wir warten auf ein Event und können bei Zeitüberschreitung sauber abbrechen (sonst bliebe der Thread hängen).
        /// </summary>
        private sealed class OverlappedStream : IHidTransport
        {
            private readonly SafeFileHandle _handle;
            private readonly int _inLength;
            private readonly int _outLength;
            private readonly IntPtr _event;
            private readonly object _lock = new object();

            public OverlappedStream(SafeFileHandle handle, int inLength, int outLength)
            {
                _handle = handle;
                _inLength = inLength;
                _outLength = outLength;
                _event = CreateEventW(IntPtr.Zero, true, false, null);
            }

            public bool Write(byte[] report)
            {
                if (report == null || report.Length != _outLength)
                    return false; // Falsche Länge würde das Gerät verwirren; lieber nichts senden.

                lock (_lock)
                    return Transfer(report, report.Length, true) != null;
            }

            public byte[] Read(int timeoutMs)
            {
                lock (_lock)
                    return Transfer(new byte[_inLength], _inLength, false, timeoutMs);
            }

            private byte[] Transfer(byte[] data, int length, bool write, int timeoutMs = 1000)
            {
                var buffer = Marshal.AllocHGlobal(length);
                var overlapped = Marshal.AllocHGlobal(32); // OVERLAPPED hat max. 32 Byte (x64)
                try
                {
                    for (var i = 0; i < 32; i++) Marshal.WriteByte(overlapped, i, 0);
                    // OVERLAPPED-Aufbau: Internal, InternalHigh, Offset+OffsetHigh (je Zeigergröße bzw. 8 Byte), danach hEvent.
                    // Das Event liegt daher bei Offset 24 (64-Bit) bzw. 16 (32-Bit).
                    Marshal.WriteIntPtr(overlapped, IntPtr.Size == 8 ? 24 : 16, _event);
                    ResetEvent(_event);

                    if (write)
                        Marshal.Copy(data, 0, buffer, length);

                    var ok = write
                        ? WriteFile(_handle, buffer, (uint)length, IntPtr.Zero, overlapped)
                        : ReadFile(_handle, buffer, (uint)length, IntPtr.Zero, overlapped);

                    if (!ok && Marshal.GetLastWin32Error() != ERROR_IO_PENDING)
                        return null;

                    if (!ok && WaitForSingleObject(_event, (uint)timeoutMs) != WAIT_OBJECT_0)
                    {
                        // Zeitüberschreitung: Auftrag abbrechen und warten, bis Windows ihn wirklich beendet hat,
                        // erst danach darf der Speicher freigegeben werden.
                        CancelIoEx(_handle, overlapped);
                        GetOverlappedResult(_handle, overlapped, out _, true);
                        return null;
                    }

                    if (!GetOverlappedResult(_handle, overlapped, out var transferred, false))
                        return null;

                    if (write)
                        return data;

                    var received = new byte[Math.Min((int)transferred, length)];
                    Marshal.Copy(buffer, received, 0, received.Length);
                    return received;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                    Marshal.FreeHGlobal(overlapped);
                }
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool ResetEvent(IntPtr handle);

            public void Dispose()
            {
                _handle.Dispose();
                if (_event != IntPtr.Zero)
                    CloseHandle(_event);
            }
        }
    }
}
