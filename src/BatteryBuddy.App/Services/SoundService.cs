using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using BatteryBuddy.Core.Settings;

namespace BatteryBuddy.App.Services
{
    /// <summary>
    /// Spielt die Warntöne ab: BatteryBuddy-Standardton (eingebettete Datei), ein Windows-Systemton oder eine eigene WAV/MP3-Datei.
    /// Es klingt immer nur ein Ton.
    /// </summary>
    public static class SoundService
    {
        private const long MaxCustomFileBytes = 20L * 1024 * 1024;
        private const int MaxCustomPlayMs = 10000;

        private static readonly object Gate = new object();
        private static bool _playing;
        private static Task _playback = Task.CompletedTask;
        private static MediaPlayer _media; // Nur im UI-Thread benutzen (MediaPlayer braucht einen Dispatcher).

        /// <summary>Spielt den gewählten Ton ab und liefert einen Task, der nach dem Ton endet.</summary>
        public static Task Play(NotifyMethods methods, string customPath, string windowsSoundName, bool critical)
        {
            if ((methods & AlertProfile.SoundMask) == 0)
                return Task.CompletedTask;
            lock (Gate)
            {
                if (_playing)
                    return _playback; // Simulationen warten auf den Ton, statt den nächsten Schritt zu früh zu starten.
                _playing = true;

                var dispatcher = Application.Current?.Dispatcher;
                _playback = Task.Run(async () =>
                {
                    try
                    {
                        if ((methods & NotifyMethods.SoundCustom) != 0)
                        {
                            if (dispatcher != null)
                                await PlayCustomAsync(dispatcher, customPath);
                        }
                        else if ((methods & NotifyMethods.SoundWindows) != 0)
                            PlayWindowsSound(windowsSoundName);
                        else if ((methods & NotifyMethods.SoundDefault) != 0)
                            PlayTone(critical);
                    }
                    catch (Exception)
                    {
                        // Kein Tonausgang o. Ä.: still ignorieren.
                    }
                    finally
                    {
                        lock (Gate)
                            _playing = false;
                    }
                });
                return _playback;
            }
        }

        /// <summary>Alle Systemtöne (*.wav) aus dem Windows-Ordner "Media", alphabetisch. Leer, falls der Ordner fehlt.</summary>
        public static string[] ListWindowsSounds()
        {
            try
            {
                var dir = WindowsMediaFolder();
                return Directory.Exists(dir)
                    ? Directory.GetFiles(dir, "*.wav").Select(Path.GetFileName).OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase).ToArray()
                    : new string[0];
            }
            catch (Exception)
            {
                return new string[0];
            }
        }

        private static string WindowsMediaFolder() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media");

        /// <summary>Vollständiger Pfad eines Systemtons, aber nur für einen gültigen reinen Dateinamen, der wirklich im Media-Ordner liegt.</summary>
        internal static string ResolveWindowsSound(string name)
        {
            var safe = AppSettings.SanitizeWindowsSoundName(name);
            if (safe.Length == 0)
                return null;
            var path = Path.Combine(WindowsMediaFolder(), safe);
            return File.Exists(path) ? path : null;
        }

        private static void PlayWindowsSound(string name)
        {
            var path = ResolveWindowsSound(name);
            if (path == null)
                return;
            using (var player = new SoundPlayer(path))
                player.PlaySync();
        }

        /// <summary>Spielt den eingebetteten BatteryBuddy-Ton; fehlt er, ersatzweise ein einfacher erzeugter Ton.</summary>
        private static void PlayTone(bool critical)
        {
            var wav = AppAssets.DefaultSound();
            using (var stream = new MemoryStream(wav.Length > 0 ? wav : BuildTone(critical)))
            using (var player = new SoundPlayer(stream))
                player.PlaySync();
        }

        /// <summary>Erzeugt eine kleine WAV-Datei (16 Bit, mono): normal = zwei aufsteigende Töne, kritisch = drei schnelle, höhere.</summary>
        internal static byte[] BuildTone(bool critical)
        {
            const int rate = 22050;
            var notes = critical
                ? new[] { new[] { 988.0, 0.14 }, new[] { 784.0, 0.14 }, new[] { 988.0, 0.14 }, new[] { 784.0, 0.14 } }
                : new[] { new[] { 660.0, 0.16 }, new[] { 880.0, 0.22 } };

            var samples = 0;
            foreach (var n in notes)
                samples += (int)(n[1] * rate);

            var data = new byte[samples * 2];
            var pos = 0;
            foreach (var n in notes)
            {
                var count = (int)(n[1] * rate);
                for (var i = 0; i < count; i++)
                {
                    // Kurze Ein-/Ausblendung gegen Knackser; Lautstärke bewusst moderat (35 %).
                    var envelope = Math.Min(1.0, Math.Min(i, count - i) / (rate * 0.01));
                    var value = (short)(Math.Sin(2 * Math.PI * n[0] * i / rate) * envelope * short.MaxValue * 0.35);
                    data[pos++] = (byte)(value & 0xFF);
                    data[pos++] = (byte)((value >> 8) & 0xFF);
                }
            }

            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(new[] { 'R', 'I', 'F', 'F' });
                w.Write(36 + data.Length);
                w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                w.Write(16);
                w.Write((short)1);      // PCM
                w.Write((short)1);      // mono
                w.Write(rate);
                w.Write(rate * 2);      // Bytes pro Sekunde
                w.Write((short)2);      // Bytes pro Sample
                w.Write((short)16);     // Bits pro Sample
                w.Write(new[] { 'd', 'a', 't', 'a' });
                w.Write(data.Length);
                w.Write(data);
                return ms.ToArray();
            }
        }

        /// <summary>Prüft den Pfad erneut (die Einstellungsdatei könnte von Hand verändert worden sein) und spielt dann ab.</summary>
        private static Task PlayCustomAsync(Dispatcher dispatcher, string path)
        {
            var safe = AppSettings.SanitizeSoundPath(path);
            if (safe.Length == 0)
                return Task.FromResult(0);
            try
            {
                var info = new FileInfo(safe);
                if (!info.Exists || info.Length == 0 || info.Length > MaxCustomFileBytes)
                    return Task.FromResult(0);
            }
            catch (Exception)
            {
                return Task.FromResult(0);
            }

            var done = new TaskCompletionSource<bool>();
            dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _media?.Close();
                    _media = new MediaPlayer();
                    var player = _media;
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(MaxCustomPlayMs) };
                    EventHandler finish = null;
                    finish = (s, e) =>
                    {
                        timer.Stop();
                        player.Close();
                        done.TrySetResult(true);
                    };
                    timer.Tick += finish;
                    player.MediaEnded += (s, e) => finish(s, e);
                    player.MediaFailed += (s, e) => finish(s, e);
                    player.Open(new Uri(safe, UriKind.Absolute));
                    player.Play();
                    timer.Start();
                }
                catch (Exception)
                {
                    done.TrySetResult(false);
                }
            }));
            return done.Task;
        }
    }
}
