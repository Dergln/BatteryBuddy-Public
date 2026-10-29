using System;
using System.Threading;

namespace BatteryBuddy.App.Services
{
    /// <summary>
    /// Sorgt dafür, dass BatteryBuddy nur einmal pro Benutzer läuft. Ein zweiter Start (z. B. über das Startmenü)
    /// weckt stattdessen die laufende Instanz, damit sie das Konfigurationsfenster öffnet.
    /// </summary>
    public sealed class SingleInstance : IDisposable
    {
        // "Local\" = pro Windows-Sitzung, damit mehrere angemeldete Benutzer sich nicht stören.
        private const string MutexName = @"Local\BatteryBuddy.SingleInstance";
        private const string EventName = @"Local\BatteryBuddy.ShowSettings";

        private readonly Mutex _mutex;
        private readonly EventWaitHandle _showEvent;
        private RegisteredWaitHandle _wait;

        public bool IsFirst { get; }

        public SingleInstance()
        {
            _mutex = new Mutex(true, MutexName, out var created);
            IsFirst = created;
            _showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);
        }

        /// <summary>Erste Instanz: bei jedem Signal einer späteren Instanz wird <paramref name="onShow"/> aufgerufen (Hintergrund-Thread!).</summary>
        public void ListenForSecondStart(Action onShow)
            => _wait = ThreadPool.RegisterWaitForSingleObject(_showEvent, (s, t) => onShow(), null, Timeout.Infinite, false);

        public void SignalFirstInstance() => _showEvent.Set();

        public void Dispose()
        {
            _wait?.Unregister(null);
            if (IsFirst)
                _mutex.ReleaseMutex();
            _mutex.Dispose();
            _showEvent.Dispose();
        }
    }
}
