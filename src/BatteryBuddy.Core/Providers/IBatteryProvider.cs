using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BatteryBuddy.Core.Models;

namespace BatteryBuddy.Core.Providers
{
    /// <summary>
    /// Erweiterungspunkt für Akku-Quellen (Systemakku, Bluetooth, Hersteller-Protokolle ...).
    /// Neuen Hersteller unterstützen = neue Klasse, die dieses Interface umsetzt, und in der App registrieren.
    /// Regeln für Implementierungen:
    ///  - nur lesen, nie Gerätekonfiguration schreiben,
    ///  - das CancellationToken beachten (der Monitor bricht langsame Provider ab),
    ///  - Ids stabil und mit Präfix vergeben (z. B. "logi:", "bt:"), damit sie nie mit anderen Providern kollidieren.
    /// </summary>
    public interface IBatteryProvider
    {
        /// <summary>Anzeigename des Providers (Diagnose, keine Übersetzung nötig).</summary>
        string Name { get; }

        Task<IReadOnlyList<BatteryDevice>> ReadAsync(CancellationToken cancellationToken);
    }
}
