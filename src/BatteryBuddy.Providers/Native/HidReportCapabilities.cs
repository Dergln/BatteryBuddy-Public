using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace BatteryBuddy.Providers.Native
{
    internal static class HidReportCapabilities
    {
        // HIDP_BUTTON_CAPS and HIDP_VALUE_CAPS are both 72 bytes, with ReportID at offset 2.
        [StructLayout(LayoutKind.Explicit, Size = 72)]
        internal struct Capability
        {
            [FieldOffset(2)] public byte ReportId;
        }

        internal delegate int ReadCaps(int reportType, [Out] Capability[] caps, ref ushort count, IntPtr preparsed);

        [DllImport("hid.dll")]
        private static extern int HidP_GetButtonCaps(int reportType, [Out] Capability[] caps, ref ushort count, IntPtr preparsed);

        [DllImport("hid.dll")]
        private static extern int HidP_GetValueCaps(int reportType, [Out] Capability[] caps, ref ushort count, IntPtr preparsed);

        internal static byte[] Read(IntPtr preparsed, int reportType, ushort buttons, ushort values)
            => Read(preparsed, reportType, buttons, values, HidP_GetButtonCaps, HidP_GetValueCaps);

        internal static byte[] Read(IntPtr preparsed, int reportType, ushort buttons, ushort values,
            ReadCaps readButtons, ReadCaps readValues)
        {
            var ids = new HashSet<byte>();
            Add(buttons, readButtons);
            Add(values, readValues);
            return ids.OrderBy(id => id).ToArray();

            void Add(ushort count, ReadCaps read)
            {
                if (count > 1024)
                    throw new InvalidDataException("Too many HID report capabilities.");
                if (count == 0)
                    return;
                var caps = new Capability[count];
                var returned = count;
                var status = read(reportType, caps, ref returned, preparsed);
                if (status != 0x00110000 || returned != count)
                    throw new InvalidDataException($"Unable to inspect HID report IDs (status 0x{status:X8}).");
                foreach (var cap in caps)
                    ids.Add(cap.ReportId);
            }
        }
    }
}
