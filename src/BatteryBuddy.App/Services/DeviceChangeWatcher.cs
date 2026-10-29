using System;
using System.Windows.Interop;

namespace BatteryBuddy.App.Services
{
    /// <summary>
    /// Lauscht auf Windows-Meldungen über geänderte Geräte (WM_DEVICECHANGE), z. B. wenn ein Headset verbunden wird.
    /// Dafür braucht Windows ein (unsichtbares) Fenster, das die Nachrichten empfängt.
    /// </summary>
    public sealed class DeviceChangeWatcher : IDisposable
    {
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        private readonly HwndSource _source;

        public event EventHandler DevicesChanged;

        public DeviceChangeWatcher()
        {
            _source = new HwndSource(new HwndSourceParameters("BatteryBuddyDeviceWatcher") { Width = 0, Height = 0, WindowStyle = 0 });
            _source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DEVICECHANGE)
            {
                var code = wParam.ToInt32();
                if (code == DBT_DEVNODES_CHANGED || code == DBT_DEVICEARRIVAL || code == DBT_DEVICEREMOVECOMPLETE)
                    DevicesChanged?.Invoke(this, EventArgs.Empty);
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            _source.RemoveHook(WndProc);
            _source.Dispose();
        }
    }
}
