using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;

namespace BatteryBuddy.Core.Logic
{
    /// <summary>Ergebnis einer Abfragerunde über alle Provider.</summary>
    public sealed class MonitorSnapshot
    {
        public IReadOnlyList<BatteryDevice> Devices { get; }

        /// <summary>Namen der Provider, die fehlgeschlagen oder zu langsam waren (für die Diagnose, ohne Details).</summary>
        public IReadOnlyList<string> FailedProviders { get; }

        public DateTimeOffset Time { get; }

        public MonitorSnapshot(IReadOnlyList<BatteryDevice> devices, IReadOnlyList<string> failedProviders, DateTimeOffset time)
        {
            Devices = devices;
            FailedProviders = failedProviders;
            Time = time;
        }
    }

    /// <summary>
    /// Fragt alle Provider parallel ab. Ein hängender oder fehlerhafter Provider darf die anderen nie blockieren:
    /// jeder hat ein Zeitlimit, Fehler werden abgefangen und nur als "fehlgeschlagen" vermerkt.
    /// </summary>
    public sealed class BatteryMonitor
    {
        private readonly IReadOnlyList<IBatteryProvider> _providers;
        private readonly TimeSpan _providerTimeout;
        private readonly object _lock = new object();
        private Task<MonitorSnapshot> _running;

        /// <summary>Wird während einer Runde ausgelöst, sobald ein Provider fertig ist, aber noch andere laufen (Aufruf aus einem Hintergrundthread!).</summary>
        public event EventHandler<MonitorSnapshot> PartialSnapshot;

        public BatteryMonitor(IEnumerable<IBatteryProvider> providers, TimeSpan providerTimeout)
        {
            _providers = (providers ?? throw new ArgumentNullException(nameof(providers))).ToList();
            _providerTimeout = providerTimeout > TimeSpan.Zero ? providerTimeout : TimeSpan.FromSeconds(20);
        }

        /// <summary>Startet eine Abfrage. Läuft schon eine, wird deren Ergebnis mitgenutzt (keine parallelen Doppel-Abfragen).</summary>
        public Task<MonitorSnapshot> RefreshAsync(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (_running != null && !_running.IsCompleted)
                    return _running;

                _running = RunAsync(cancellationToken);
                return _running;
            }
        }

        private async Task<MonitorSnapshot> RunAsync(CancellationToken cancellationToken)
        {
            // Fertige Ergebnisse je Provider (Position = Reihenfolge der Provider-Liste, null = noch offen).
            var done = new ProviderResult[_providers.Count];
            var tasks = _providers.Select(async (p, index) =>
            {
                var result = await QueryProviderAsync(p, cancellationToken).ConfigureAwait(false);
                MonitorSnapshot partial = null;
                lock (done)
                {
                    done[index] = result;
                    if (done.Any(r => r == null))
                        partial = Merge(done.Where(r => r != null));
                }

                // Zwischenstand melden, damit schnelle Provider nicht auf den langsamsten (z. B. Logitech) warten müssen.
                if (partial != null)
                    PartialSnapshot?.Invoke(this, partial);
                return result;
            }).ToList();

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return Merge(results);
        }

        // Doppelte Ids zusammenführen: Der erste Provider in der Liste gewinnt.
        private static MonitorSnapshot Merge(IEnumerable<ProviderResult> results)
        {
            var list = results.ToList();
            var byId = new Dictionary<string, BatteryDevice>();
            foreach (var device in list.SelectMany(r => r.Devices))
            {
                if (!byId.ContainsKey(device.Id))
                    byId[device.Id] = device;
            }

            var failed = list.Where(r => r.Failed).Select(r => r.Name).ToList();
            return new MonitorSnapshot(byId.Values.ToList(), failed, DateTimeOffset.Now);
        }

        private sealed class ProviderResult
        {
            public string Name;
            public bool Failed;
            public IReadOnlyList<BatteryDevice> Devices = new BatteryDevice[0];
        }

        private async Task<ProviderResult> QueryProviderAsync(IBatteryProvider provider, CancellationToken cancellationToken)
        {
            var result = new ProviderResult { Name = provider.Name };
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(_providerTimeout);
                try
                {
                    // Task.Run: Auch ein Provider, der synchron blockiert, hält den Aufrufer nicht auf.
                    var work = Task.Run(() => provider.ReadAsync(cts.Token), CancellationToken.None);
                    var finished = await Task.WhenAny(work, Task.Delay(_providerTimeout, cancellationToken)).ConfigureAwait(false);

                    if (finished != work)
                    {
                        cts.Cancel();
                        // Späten Fehler des abgebrochenen Providers "einsammeln", damit er nicht als unbeobachtet gilt.
                        var ignored = work.ContinueWith(t => t.Exception, TaskContinuationOptions.OnlyOnFaulted);
                        cancellationToken.ThrowIfCancellationRequested();
                        result.Failed = true;
                        return result;
                    }

                    result.Devices = (await work.ConfigureAwait(false)) ?? result.Devices;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw; // App wird beendet: nicht als Providerfehler werten
                }
                catch (Exception)
                {
                    // Bewusst alle Fehler: Treiber/WMI/HID können vielfältig scheitern, ein Provider darf nie die App stoppen.
                    result.Failed = true;
                }
            }

            return result;
        }
    }
}
