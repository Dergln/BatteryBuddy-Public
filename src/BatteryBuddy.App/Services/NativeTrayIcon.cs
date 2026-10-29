using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BatteryBuddy.App.Services
{
    /// <summary>Owns the Windows tray entry so balloon tips can use a custom icon without private WinForms reflection.</summary>
    internal sealed class NativeTrayIcon : NativeWindow, IDisposable
    {
        private const int CallbackMessage = 0x8001;
        private const uint NotifyMessage = 1;
        private const uint NotifyIcon = 2;
        private const uint NotifyTip = 4;
        private const uint NotifyInfo = 0x10;
        private readonly int _taskbarCreated = RegisterWindowMessage("TaskbarCreated");
        private readonly Guid _guid;
        internal delegate bool NotifyIconCommand(uint message, ref NotifyIconData data);
        private readonly NotifyIconCommand _notify;
        private bool _added;
        private bool _visible;
        private Icon _icon;
        private string _text = string.Empty;

        public event MouseEventHandler MouseClick;
        public event EventHandler BalloonTipClosed;
        public ContextMenuStrip ContextMenuStrip { get; set; }

        public NativeTrayIcon(string stableKey, NotifyIconCommand notify = null)
        {
            if (string.IsNullOrWhiteSpace(stableKey))
                throw new ArgumentException("A stable tray identity is required.", nameof(stableKey));
            _guid = CreateIdentity(stableKey, Application.ExecutablePath);
            // Tests exercise native windows without adding persistent entries to the user's Explorer history.
            _notify = notify ?? ShellNotifyIcon;
            CreateHandle(new CreateParams { Caption = "BatteryBuddy.Tray" });
        }

        internal static Guid CreateIdentity(string stableKey, string executablePath)
        {
            // Windows bindet unsignierte GUIDs an den EXE-Pfad. Portable/test/main brauchen getrennte Identitaeten.
            var path = System.IO.Path.GetFullPath(executablePath).ToUpperInvariant();
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("BatteryBuddy.Tray." + path + "\0" + stableKey));
                var guidBytes = new byte[16];
                Array.Copy(bytes, guidBytes, guidBytes.Length);
                return new Guid(guidBytes);
            }
        }

        public Icon Icon
        {
            get => _icon;
            set { _icon = value; Update(); }
        }

        public string Text
        {
            get => _text;
            set { _text = value; Update(); }
        }

        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                if (value)
                    Update();
                else
                    Remove();
            }
        }

        private NotifyIconData Data(uint flags) => new NotifyIconData
        {
            Size = (uint)Marshal.SizeOf<NotifyIconData>(),
            Window = Handle,
            Id = 1,
            Flags = flags | (_guid == Guid.Empty ? 0u : 0x20u),
            Guid = _guid,
            Callback = CallbackMessage,
            Icon = (flags & NotifyIcon) != 0 ? _icon?.Handle ?? IntPtr.Zero : IntPtr.Zero,
            Tip = _text,
            Info = string.Empty,
            InfoTitle = string.Empty
        };

        private void Update()
        {
            if (!_visible || _icon == null)
                return;

            var data = Data(NotifyMessage | NotifyIcon | NotifyTip);
            if (!_notify(_added ? 1u : 0u, ref data))
                throw new Win32Exception("Windows could not update the BatteryBuddy tray icon.");
            if (!_added)
            {
                // Version 3 supports keyboard activation while retaining unpacked callback messages.
                data.TimeoutOrVersion = 3;
                if (!_notify(4, ref data))
                {
                    _notify(2, ref data);
                    throw new Win32Exception("Windows could not configure the BatteryBuddy tray icon.");
                }
            }
            _added = true;
        }

        public void ShowBalloonTip(int timeout, string title, string body, Icon icon)
        {
            if (!_added)
                throw new InvalidOperationException("The tray icon is not registered.");

            var data = Data(NotifyInfo);
            data.Info = body;
            data.InfoTitle = title;
            data.TimeoutOrVersion = (uint)timeout;
            // NIIF_USER + NIIF_LARGE_ICON uses BalloonIcon, not the (independently changing) tray image.
            data.InfoFlags = 0x4 | 0x20;
            data.BalloonIcon = icon.Handle;
            if (!_notify(1, ref data))
                throw new Win32Exception("Windows could not show the BatteryBuddy notification.");
        }

        private void Remove()
        {
            if (!_added)
                return;
            var data = Data(0);
            if (!_notify(2, ref data))
                Trace.TraceError("Windows could not remove the BatteryBuddy tray icon.");
            _added = false;
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == _taskbarCreated)
            {
                // Explorer recreates the notification area after a restart; our old registration is gone.
                _added = false;
                Update();
            }
            else if (message.Msg == CallbackMessage)
            {
                switch (message.LParam.ToInt32())
                {
                    case 0x202: // WM_LBUTTONUP
                    case 0x400: // NIN_SELECT (keyboard activation)
                    case 0x401: // NIN_KEYSELECT
                        MouseClick?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                        break;
                    case 0x205: // WM_RBUTTONUP
                    case 0x7B:  // WM_CONTEXTMENU
                        ShowContextMenu();
                        break;
                    case 0x403: // NIN_BALLOONHIDE
                    case 0x404: // NIN_BALLOONTIMEOUT
                    case 0x405: // NIN_BALLOONUSERCLICK
                        BalloonTipClosed?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }
            base.WndProc(ref message);
        }

        private void ShowContextMenu()
        {
            if (ContextMenuStrip == null)
                return;
            // A foreground owner makes an outside click dismiss the menu, just like WinForms NotifyIcon.
            SetForegroundWindow(Handle);
            ContextMenuStrip.Show(Cursor.Position);
            PostMessage(Handle, 0, IntPtr.Zero, IntPtr.Zero);
        }

        public void Dispose()
        {
            _visible = false;
            Remove();
            DestroyHandle();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct NotifyIconData
        {
            public uint Size;
            public IntPtr Window;
            public uint Id;
            public uint Flags;
            public uint Callback;
            public IntPtr Icon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Tip;
            public uint State;
            public uint StateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Info;
            public uint TimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string InfoTitle;
            public uint InfoFlags;
            public Guid Guid;
            public IntPtr BalloonIcon;
        }

        [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIconW", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShellNotifyIcon(uint message, ref NotifyIconData data);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegisterWindowMessage(string message);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr window);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
    }
}
