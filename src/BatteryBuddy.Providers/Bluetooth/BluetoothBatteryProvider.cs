using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BatteryBuddy.Core.Logic;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;

namespace BatteryBuddy.Providers.Bluetooth
{
    /// <summary>
    /// Liest den Ladestand von Bluetooth-Geräten genau so, wie Windows ihn selbst verwaltet
    /// (Geräteeigenschaft "Battery" der BT-Geräteknoten; speist u. a. die Anzeige unter Einstellungen → Bluetooth).
    /// Ein BT-Gerät besteht aus mehreren Knoten (klassisch, LE, Freisprech-Profil ...). Deshalb:
    ///  - Knoten werden über die 12-stellige Bluetooth-Adresse zu einem Gerät zusammengefasst,
    ///  - nur VERBUNDENE Geräte zählen (bei getrennten Geräten ist der Wert veraltet),
    ///  - unter mehreren Werten gewinnt der mit dem neuesten Zeitstempel.
    /// </summary>
    public sealed class BluetoothBatteryProvider : IBatteryProvider
    {
        public string Name => "Bluetooth";

        // 12 Hex-Zeichen, die nicht Teil eines längeren Hex-Blocks sind.
        private static readonly Regex Address = new Regex("(?<![0-9A-Fa-f])([0-9A-Fa-f]{12})(?![0-9A-Fa-f])", RegexOptions.Compiled);
        private static readonly Regex ProfileGuid = new Regex(@"\{[0-9A-Fa-f\-]+\}", RegexOptions.Compiled);

        public Task<IReadOnlyList<BatteryDevice>> ReadAsync(CancellationToken cancellationToken)
        {
            // SetupAPI-Aufrufe blockieren kurz; auf einen Pool-Thread auslagern.
            return Task.Run<IReadOnlyList<BatteryDevice>>(() =>
            {
                var nodes = new List<PnpNode>();
                nodes.AddRange(PnpEnumerator.Enumerate("BTHENUM"));
                nodes.AddRange(PnpEnumerator.Enumerate("BTHLE"));
                return Build(nodes);
            }, cancellationToken);
        }

        /// <summary>Reine Zusammenführungslogik (ohne Windows-Aufrufe), damit sie mit Testdaten geprüft werden kann.</summary>
        internal static IReadOnlyList<BatteryDevice> Build(IEnumerable<PnpNode> nodes)
        {
            var result = new List<BatteryDevice>();
            var groups = nodes
                .Select(n => new { Node = n, Addr = ExtractAddress(n.InstanceId) })
                .Where(x => x.Addr != null)
                .GroupBy(x => x.Addr);

            foreach (var group in groups)
            {
                var all = group.Select(x => x.Node).ToList();

                // Verbunden, wenn irgendein Knoten des Geräts "verbunden" meldet.
                if (!all.Any(n => n.IsConnected == true))
                    continue;

                // Frischester Wert gewinnt; ohne Zeitstempel gilt der Wert als ältester.
                var best = all.Where(n => n.Battery.HasValue)
                              .OrderByDescending(n => n.BatteryTimeUtc ?? DateTime.MinValue)
                              .FirstOrDefault();
                if (best == null)
                    continue;

                var rawName = all.Where(n => n.InstanceId.StartsWith("BTHENUM\\DEV_", StringComparison.OrdinalIgnoreCase)
                                             || n.InstanceId.StartsWith("BTHLE\\DEV_", StringComparison.OrdinalIgnoreCase))
                                 .Select(n => n.FriendlyName).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))
                              ?? best.FriendlyName;
                var name = CleanName(rawName);

                var cod = all.Select(n => n.ClassOfDevice).FirstOrDefault(c => c.HasValue && c.Value != 0);
                var category = CategoryClassifier.FromBluetooth(cod, name);

                result.Add(new BatteryDevice("bt:" + group.Key.ToUpperInvariant(), name, category, best.Battery, false, "Bluetooth"));
            }

            return result;
        }

        /// <summary>Holt die Bluetooth-Adresse aus der Geräte-Instanz-ID. Nur BT-Knoten werden berücksichtigt.</summary>
        internal static string ExtractAddress(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId) || !instanceId.StartsWith("BTH", StringComparison.OrdinalIgnoreCase))
                return null;

            // GUIDs (Profil-IDs) enthalten ebenfalls 12 Hex-Zeichen und müssen vorher entfernt werden.
            var m = Address.Match(ProfileGuid.Replace(instanceId, string.Empty));
            return m.Success ? m.Groups[1].Value : null;
        }

        /// <summary>Windows hängt bei LE-Geräten "LE_" vor den Namen; das ist für Menschen nur Rauschen.</summary>
        internal static string CleanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return name.StartsWith("LE_", StringComparison.Ordinal) ? name.Substring(3) : name;
        }
    }
}
