using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatteryBuddy.Core.Logic;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers.Logitech
{
    /// <summary>
    /// Liest Logitech-Akkustände direkt über das herstellereigene Protokoll HID++ 2.0 – OHNE Logi Options+ oder andere Herstellersoftware.
    /// Funktioniert für Geräte hinter einem Unifying-/Bolt-/Lightspeed-Empfänger sowie für direkt per USB/Bluetooth verbundene Geräte,
    /// sofern sie HID++ 2.0 unterstützen.
    /// Sicherheit: nur lesende Anfragen, Antworten werden längengeprüft, Namen bereinigt, jeder Zugriff hat ein Zeitlimit.
    /// </summary>
    public sealed class LogitechBatteryProvider : IBatteryProvider
    {
        private const ushort LogitechVendorId = 0x046D;
        private const int RequestTimeoutMs = 700;
        private const int MaxReceiverSlots = 6;
        private const int MaxChannels = 6;

        // Herstellerspezifische HID-Bereiche, in denen Logitech HID++ anbietet (Empfänger/USB bzw. Bluetooth-LE).
        private const ushort UsagePageVendor = 0xFF00;
        private const ushort UsagePageBluetoothLe = 0xFF43;

        private readonly Func<IReadOnlyList<IHidTransport>> _openChannels;

        public string Name => "Logitech";

        public LogitechBatteryProvider() : this(OpenRealChannels)
        {
        }

        /// <summary>Für Tests: eigene (Fake-)Kanäle einspeisen.</summary>
        internal LogitechBatteryProvider(Func<IReadOnlyList<IHidTransport>> openChannels)
        {
            _openChannels = openChannels;
        }

        public Task<IReadOnlyList<BatteryDevice>> ReadAsync(CancellationToken cancellationToken)
        {
            return Task.Run<IReadOnlyList<BatteryDevice>>(() =>
            {
                var channels = _openChannels();
                var found = new List<Probed>();
                try
                {
                    found = ProbeAll(channels, cancellationToken);

                    lock (_knownLock)
                    {
                        // Nach dem App-Start ist noch nichts bekannt: Bleibt der erste Durchgang leer, schläft das Gerät evtl. nur, also einmal wiederholen.
                        if (found.Count == 0 && _known.Count == 0 && channels.Count > 0)
                            found = ProbeAll(channels, cancellationToken);

                        Remember(found);
                        // Schlafende Geräte gezielt (Kanal + Slot bekannt) mehrfach anstupsen, bevor sie als "weg" gelten.
                        RetryMissing(channels, found, cancellationToken);
                        AddStale(found);
                    }
                }
                finally
                {
                    foreach (var c in channels)
                        c.Dispose();
                }

                return ToDevices(found);
            }, cancellationToken);
        }

        // Ein kompletter Durchgang über alle HID-Kanäle; merkt sich den Kanal als Hinweis fürs spätere gezielte Wecken.
        private static List<Probed> ProbeAll(IReadOnlyList<IHidTransport> channels, CancellationToken ct)
        {
            var result = new List<Probed>();
            for (var i = 0; i < channels.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                foreach (var p in ProbeChannel(channels[i], ct))
                {
                    p.ChannelIndex = i;
                    result.Add(p);
                }
            }

            return result;
        }

        // Wie lange ein zuletzt gesehener Wert angezeigt wird, wenn das Gerät nicht antwortet (Maus schläft/ist knapp außer Reichweite).
        internal static readonly TimeSpan StaleLimit = TimeSpan.FromHours(3);
        private const int WakeAttempts = 4;

        private readonly object _knownLock = new object();
        private readonly Dictionary<string, Probed> _known = new Dictionary<string, Probed>();

        private void Remember(List<Probed> found)
        {
            foreach (var p in found)
            {
                p.SeenAt = DateTime.UtcNow;
                _known[KeyOf(p)] = p;
            }
        }

        private void RetryMissing(IReadOnlyList<IHidTransport> channels, List<Probed> found, CancellationToken ct)
        {
            foreach (var known in _known.Values.ToList())
            {
                if (found.Any(f => KeyOf(f) == KeyOf(known)) || known.ChannelIndex >= channels.Count)
                    continue;
                if (DateTime.UtcNow - known.SeenAt > StaleLimit)
                    continue;

                var device = new HidppDevice(channels[known.ChannelIndex], (byte)known.Slot, RequestTimeoutMs);
                for (var attempt = 0; attempt < WakeAttempts; attempt++)
                {
                    ct.ThrowIfCancellationRequested();
                    if (device.Ping() >= 2)
                    {
                        var fresh = new List<Probed>();
                        AddIfValid(fresh, device, known.Slot);
                        if (fresh.Count > 0)
                        {
                            fresh[0].ChannelIndex = known.ChannelIndex;
                            fresh[0].SeenAt = DateTime.UtcNow;
                            _known[KeyOf(fresh[0])] = fresh[0];
                            found.Add(fresh[0]);
                        }

                        break;
                    }
                }
            }
        }

        // Nicht erreichbare Geräte: letzten Wert weiter anzeigen, bis StaleLimit abgelaufen ist (danach gilt das Gerät als getrennt).
        private void AddStale(List<Probed> found)
        {
            foreach (var key in _known.Keys.ToList())
            {
                var known = _known[key];
                if (DateTime.UtcNow - known.SeenAt > StaleLimit)
                    _known.Remove(key);
                else if (!found.Any(f => KeyOf(f) == key))
                    found.Add(known);
            }
        }

        // Gleiche Idee wie die Geräte-Id: Name + Slot, damit zwei gleich benannte Geräte getrennt bleiben (Kanal ist nur ein Hinweis zum Wiederfinden).
        private static string KeyOf(Probed p) => p.Name + "|" + p.Slot;

        internal sealed class Probed
        {
            public string Name;
            public DeviceCategory Category;
            public BatteryReading Battery;
            public int Slot;
            public int ChannelIndex;
            public DateTime SeenAt;
        }

        /// <summary>Fragt einen HID++-Kanal ab: erst als direktes Gerät, danach (Empfänger) die Slots 1–6.</summary>
        internal static List<Probed> ProbeChannel(IHidTransport channel, CancellationToken ct)
        {
            var result = new List<Probed>();

            var direct = new HidppDevice(channel, HidPp.DirectDeviceIndex, RequestTimeoutMs);
            // Schlafende Geräte verlieren das erste Paket (es weckt sie nur auf). Darum bei Zeitüberschreitung einmal wiederholen.
            var directVersion = direct.Ping() ?? direct.Ping();
            if (directVersion >= 2)
            {
                AddIfValid(result, direct, 0);
                return result;
            }

            // Schlafende Geräte verlieren das erste Paket (es weckt sie nur auf), also sofort erneut fragen:
            // Ein späterer Durchgang käme zu spät, das Gerät schläft nach wenigen Sekunden wieder ein.
            for (var slot = 1; slot <= MaxReceiverSlots; slot++)
            {
                ct.ThrowIfCancellationRequested();
                var device = new HidppDevice(channel, (byte)slot, RequestTimeoutMs);
                var version = device.Ping() ?? device.Ping();
                if (version >= 2)
                    AddIfValid(result, device, slot);
            }

            return result;
        }

        private static void AddIfValid(List<Probed> list, HidppDevice device, int slot)
        {
            var battery = device.ReadBattery();
            if (battery == null)
                return; // Gerät ohne Akku-Feature (z. B. Kabeltastatur): nicht relevant

            var name = device.ReadName();
            var type = device.ReadDeviceType();
            var category = type.HasValue ? HidPp.CategoryFromDeviceType(type.Value) : DeviceCategory.Other;
            if (category == DeviceCategory.Other)
                category = CategoryClassifier.FromName(name);

            list.Add(new Probed { Name = name ?? "Logitech device", Category = category, Battery = battery, Slot = slot });
        }

        /// <summary>
        /// Vergibt stabile Ids. Normalerweise "logi:&lt;Name&gt;". Gibt es zwei gleich benannte Geräte, wird der Empfänger-Slot angehängt,
        /// damit beide einzeln einstellbar bleiben.
        /// </summary>
        private static IReadOnlyList<BatteryDevice> ToDevices(List<Probed> probed)
        {
            var devices = new List<BatteryDevice>();
            foreach (var p in probed)
            {
                var duplicate = probed.Count(x => x.Name == p.Name) > 1;
                var id = "logi:" + p.Name + (duplicate ? ":" + p.Slot : string.Empty);
                devices.Add(new BatteryDevice(id, p.Name, p.Category, p.Battery.Percent, p.Battery.Charging, "Logitech", new DateTimeOffset(DateTime.SpecifyKind(p.SeenAt, DateTimeKind.Utc))));
            }

            return devices;
        }

        private static IReadOnlyList<IHidTransport> OpenRealChannels()
        {
            var channels = new List<IHidTransport>();
            try
            {
                var candidates = HidNative.Enumerate(LogitechVendorId)
                    .Where(i => (i.UsagePage == UsagePageVendor || i.UsagePage == UsagePageBluetoothLe)
                                && i.InputReportLength == HidPp.LongLength
                                && i.OutputReportLength == HidPp.LongLength)
                    .Take(MaxChannels);

                foreach (var info in candidates)
                {
                    var transport = HidNative.Open(info);
                    if (transport != null)
                        channels.Add(transport);
                }
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException)
            {
                // HID-API nicht verfügbar: keine Logitech-Geräte.
            }

            return channels;
        }
    }
}
