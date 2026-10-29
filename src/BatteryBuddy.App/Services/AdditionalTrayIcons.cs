using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BatteryBuddy.Core.Logic;

namespace BatteryBuddy.App.Services
{
    /// <summary>Ein eigener Windows-Eintrag pro stabilem Kategorie-/Geraeteschluessel.</summary>
    internal sealed class AdditionalTrayIcons : IDisposable
    {
        private sealed class Slot : IDisposable
        {
            public readonly NativeTrayIcon Native;
            public Icon Image;
            public Slot(NativeTrayIcon native) { Native = native; }
            public void Dispose()
            {
                Native.Dispose();
                Image?.Dispose();
            }
        }

        private readonly Dictionary<string, Slot> _slots = new Dictionary<string, Slot>(StringComparer.Ordinal);
        private readonly ContextMenuStrip _menu;
        private readonly Action _open;
        private readonly Func<string, NativeTrayIcon> _createIcon;
        internal int Count => _slots.Count;
        internal IReadOnlyList<IntPtr> Handles => _slots.Values.Select(s => s.Native.Handle).ToList();

        public AdditionalTrayIcons(ContextMenuStrip menu, Action open, Func<string, NativeTrayIcon> createIcon = null)
        {
            _menu = menu;
            _open = open;
            _createIcon = createIcon ?? (key => new NativeTrayIcon(key));
        }

        internal static string Tooltip(TrayEntry entry)
        {
            var text = Loc.Get("category." + entry.Category) + "\n"
                + Loc.Format("tray.line", entry.DeviceName, entry.Percent)
                + (entry.IsCharging ? Loc.Get("tray.charging") : string.Empty);
            return text.Length <= 127 ? text : text.Substring(0, 126) + "…";
        }

        public void Update(IReadOnlyList<TrayEntry> entries, bool light)
        {
            var keys = new HashSet<string>(entries.Select(e => e.Key), StringComparer.Ordinal);
            foreach (var key in _slots.Keys.Where(k => !keys.Contains(k)).ToList())
            {
                _slots[key].Dispose();
                _slots.Remove(key);
            }
            foreach (var entry in entries)
            {
                Slot slot = null;
                var image = TrayIconRenderer.Render(entry, light);
                try
                {
                    if (!_slots.TryGetValue(entry.Key, out slot))
                    {
                        slot = new Slot(_createIcon(entry.Key));
                        slot.Native.ContextMenuStrip = _menu;
                        slot.Native.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) _open(); };
                        _slots.Add(entry.Key, slot);
                    }
                    slot.Native.Icon = image;
                    slot.Native.Text = Tooltip(entry);
                    slot.Native.Visible = true;
                }
                catch
                {
                    // Windows darf keinen Handle auf ein bereits freigegebenes Bild behalten.
                    slot?.Dispose();
                    _slots.Remove(entry.Key);
                    image.Dispose();
                    throw;
                }
                slot.Image?.Dispose();
                slot.Image = image;
            }
        }

        public void Dispose()
        {
            foreach (var slot in _slots.Values)
                slot.Dispose();
            _slots.Clear();
        }
    }
}
