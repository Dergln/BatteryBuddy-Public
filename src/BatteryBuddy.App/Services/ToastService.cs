using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BatteryBuddy.Core.Models;
using Microsoft.Win32;
using Windows.UI.Notifications;

namespace BatteryBuddy.App.Services
{
    /// <summary>
    /// Echte Windows-Benachrichtigungen (Toasts). Anders als die alten Sprechblasen des Tray-Symbols bleiben sie
    /// in der Benachrichtigungszentrale stehen. Voraussetzung für Programme ohne Installer: eine "App-Kennung" (AUMID),
    /// die einmalig unter HKEY_CURRENT_USER mit Name und Symbol eingetragen wird (keine Administratorrechte nötig).
    /// </summary>
    internal sealed class ToastService
    {
        public const string AppId = "Dergln.BatteryBuddy";
        private const string AlertGroup = "alerts";
        private const int MaxPendingAcks = 64;

        private readonly SynchronizationContext _ui;
        private readonly Action<IReadOnlyList<string>> _acknowledge;
        private readonly string _imageFolder;
        private readonly HashSet<string> _writtenImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Der "Gesehen"-Knopf trägt nur ein zufälliges Kürzel, keine Geräte-ID: Die Rückgabe von Windows wird so nie
        // als Geräte-ID interpretiert, sondern nur in dieser Liste nachgeschlagen (unbekannte Kürzel werden ignoriert).
        private readonly Dictionary<string, IReadOnlyList<string>> _pendingAcks = new Dictionary<string, IReadOnlyList<string>>();
        private readonly Queue<string> _ackOrder = new Queue<string>();
        // Toasts mit "Gesehen"-Knopf festhalten: Nur solange das Objekt lebt, kommt ein späterer Klick
        // (z. B. aus der Benachrichtigungszentrale) bei uns an. Begrenzt wie die Kürzel-Liste.
        private readonly Queue<ToastNotification> _aliveToasts = new Queue<ToastNotification>();

        private ToastNotifier _notifier;

        /// <param name="ui">Hierhin werden Klicks zurückgegeben (Windows meldet sie auf einem Hintergrund-Thread).</param>
        /// <param name="acknowledge">Wird auf dem UI-Thread mit den Geräte-IDs einer bestätigten Meldung aufgerufen.</param>
        public ToastService(SynchronizationContext ui, Action<IReadOnlyList<string>> acknowledge)
        {
            _ui = ui;
            _acknowledge = acknowledge;
            _imageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BatteryBuddy", "notification-images");
        }

        /// <summary>True, wenn Windows-Toasts nutzbar sind; sonst bleibt die Sprechblase als Ersatz.</summary>
        public bool TryInitialize()
        {
            try
            {
                Directory.CreateDirectory(_imageFolder);
                string iconPath = null;
                try { iconPath = WriteAppIcon(); }
                catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is NotSupportedException
                    || ex is InvalidOperationException || ex is System.Runtime.InteropServices.ExternalException)
                {
                    // Ohne Symbol funktioniert der Toast trotzdem; Windows zeigt dann ein Standard-Symbol.
                    System.Diagnostics.Trace.TraceWarning("Toast app icon unavailable: " + ex.Message);
                }
                RegisterAppId(iconPath);
                _notifier = ToastNotificationManager.CreateToastNotifier(AppId);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException
                || ex is System.Security.SecurityException || ex is System.Runtime.InteropServices.COMException
                || ex is TypeLoadException || ex is ArgumentException)
            {
                _notifier = null;
                return false;
            }
        }

        public bool IsAvailable => _notifier != null;

        /// <summary>Meldungen sind in den Windows-Einstellungen für BatteryBuddy oder global abgeschaltet.</summary>
        public bool IsDisabledByUser
        {
            get
            {
                try { return _notifier != null && _notifier.Setting != NotificationSetting.Enabled; }
                catch (Exception) { return false; }
            }
        }

        /// <summary>
        /// Zeigt einen Toast und wartet, bis er wieder verschwunden ist (Zeitablauf, Klick, Wegwischen oder Fehler),
        /// höchstens aber <paramref name="maxWait"/>. So überlagern sich Meldungen nicht.
        /// </summary>
        /// <param name="tagSource">Gleiche Quelle = neue Meldung ersetzt die alte in der Benachrichtigungszentrale (kein Stapeln bei Wiederholungen).</param>
        /// <param name="ackDeviceIds">Geräte für den "Gesehen"-Knopf; null = kein Knopf (z. B. Test).</param>
        public async Task ShowAsync(string title, string body, string imagePath, string tagSource,
            IReadOnlyList<string> ackDeviceIds, TimeSpan maxWait)
        {
            var ackToken = ackDeviceIds != null && ackDeviceIds.Count > 0 ? AddPendingAck(ackDeviceIds) : null;
            var xml = new Windows.Data.Xml.Dom.XmlDocument();
            xml.LoadXml(BuildXml(title, body, imagePath, ackToken, Loc.Get("notify.ack")));

            var toast = new ToastNotification(xml)
            {
                Tag = Tag(tagSource),
                Group = AlertGroup,
                ExpirationTime = DateTimeOffset.Now.AddDays(1) // alte Warnungen verschwinden nach einem Tag von selbst
            };

            var closed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            toast.Activated += (s, args) =>
            {
                closed.TrySetResult(true);
                var arguments = (args as ToastActivatedEventArgs)?.Arguments;
                _ui.Post(_ => HandleActivation(arguments), null);
            };
            toast.Dismissed += (s, args) => closed.TrySetResult(true);
            toast.Failed += (s, args) => closed.TrySetResult(false);

            _notifier.Show(toast);
            if (ackToken != null)
                lock (_pendingAcks)
                {
                    _aliveToasts.Enqueue(toast);
                    while (_aliveToasts.Count > MaxPendingAcks)
                        _aliveToasts.Dequeue();
                }
            await Task.WhenAny(closed.Task, Task.Delay(maxWait));
        }

        /// <summary>Bild für den Toast als PNG-Datei (Windows liest Toast-Bilder nur aus Dateien/URLs, nicht aus dem Speicher).</summary>
        public string ImageFile(string name, Func<Bitmap> create)
        {
            var path = Path.Combine(_imageFolder, name + ".png");
            lock (_writtenImages)
            {
                // Einmal pro Programmlauf neu schreiben: so passt die Datei immer zur aktuellen Programmversion.
                if (_writtenImages.Contains(name) && File.Exists(path))
                    return path;
                using (var bitmap = create())
                {
                    if (bitmap == null)
                        return null;
                    bitmap.Save(path, ImageFormat.Png);
                }
                _writtenImages.Add(name);
                return path;
            }
        }

        /// <summary>Baut das Toast-XML. Alle Texte werden über XElement automatisch korrekt maskiert (keine XML-Einschleusung).</summary>
        internal static string BuildXml(string title, string body, string imagePath, string ackToken, string ackText)
        {
            var binding = new XElement("binding", new XAttribute("template", "ToastGeneric"),
                new XElement("text", title ?? string.Empty),
                new XElement("text", body ?? string.Empty));
            if (!string.IsNullOrEmpty(imagePath))
                binding.Add(new XElement("image", new XAttribute("placement", "appLogoOverride"),
                    new XAttribute("src", new Uri(imagePath).AbsoluteUri)));

            var toast = new XElement("toast", new XElement("visual", binding));
            if (ackToken != null)
                toast.Add(new XElement("actions", new XElement("action",
                    new XAttribute("content", ackText ?? string.Empty),
                    new XAttribute("arguments", "ack=" + ackToken),
                    new XAttribute("activationType", "foreground"))));
            // BatteryBuddy spielt seine Töne selbst (eigene Auswahl je Gerät); der Windows-Ton bleibt stumm.
            toast.Add(new XElement("audio", new XAttribute("silent", "true")));
            return toast.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>Liest das Kürzel aus den von Windows zurückgegebenen Argumenten (nur das erwartete Format wird akzeptiert).</summary>
        internal static string ParseAckToken(string arguments)
        {
            const string Prefix = "ack=";
            if (arguments == null || !arguments.StartsWith(Prefix, StringComparison.Ordinal))
                return null;
            var token = arguments.Substring(Prefix.Length);
            if (token.Length != 32)
                return null;
            foreach (var c in token)
                if (!Uri.IsHexDigit(c))
                    return null;
            return token;
        }

        /// <summary>Kurzer, stabiler Tag (max. 16 Zeichen, Grenze älterer Windows-10-Versionen) aus der Geräte-ID.</summary>
        internal static string Tag(string source)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(source ?? string.Empty));
                return BitConverter.ToString(hash, 0, 8).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        private string AddPendingAck(IReadOnlyList<string> deviceIds)
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            var token = BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
            lock (_pendingAcks)
            {
                _pendingAcks[token] = deviceIds;
                _ackOrder.Enqueue(token);
                // Begrenzen: Alte, nie angeklickte Meldungen sollen den Speicher nicht füllen.
                while (_ackOrder.Count > MaxPendingAcks)
                    _pendingAcks.Remove(_ackOrder.Dequeue());
            }
            return token;
        }

        private void HandleActivation(string arguments)
        {
            var token = ParseAckToken(arguments);
            if (token == null)
                return;
            IReadOnlyList<string> ids;
            lock (_pendingAcks)
            {
                if (!_pendingAcks.TryGetValue(token, out ids))
                    return;
                _pendingAcks.Remove(token);
            }
            _acknowledge(ids);
        }

        /// <summary>
        /// Programmsymbol als PNG für die Kopfzeile der Benachrichtigung. Der WPF-Decoder liest auch die
        /// PNG-komprimierte 256-px-Ebene der .ico (System.Drawing.Icon scheitert daran).
        /// </summary>
        private string WriteAppIcon()
            => ImageFile("app", () =>
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Assets.batterybuddy.ico"))
                {
                    if (stream == null)
                        return null;
                    var decoder = new System.Windows.Media.Imaging.IconBitmapDecoder(stream,
                        System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat,
                        System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
                    var largest = decoder.Frames.OrderByDescending(f => f.PixelWidth).First();
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    encoder.Frames.Add(largest);
                    using (var png = new MemoryStream())
                    {
                        encoder.Save(png);
                        png.Position = 0;
                        using (var loaded = new Bitmap(png))
                            return new Bitmap(loaded);
                    }
                }
            });

        /// <summary>Trägt Name und Symbol der App-Kennung ein (nur bei Abweichung, damit nicht bei jedem Start geschrieben wird).</summary>
        private static void RegisterAppId(string iconPath)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\AppUserModelId\" + AppId))
            {
                if (key == null)
                    throw new UnauthorizedAccessException("AUMID registry key not writable.");
                SetIfDifferent(key, "DisplayName", "BatteryBuddy");
                if (iconPath != null)
                    SetIfDifferent(key, "IconUri", iconPath);
            }
        }

        private static void SetIfDifferent(RegistryKey key, string name, string value)
        {
            if (!string.Equals(key.GetValue(name) as string, value, StringComparison.Ordinal))
                key.SetValue(name, value, RegistryValueKind.String);
        }

        /// <summary>Allgemeines Bild für Test und Simulation (verwirrte Batterie).</summary>
        public string ConfusedImage()
            => ImageFile("battery-confused", () =>
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Assets.battery-confused.png"))
                {
                    if (stream == null)
                        return null;
                    using (var loaded = new Bitmap(stream))
                        return new Bitmap(loaded); // Kopie, damit der Stream geschlossen werden darf
                }
            });

        /// <summary>Für die Kategorie-Bilder der Meldungen (quadratisch, damit Windows das Logo-Feld nicht verzerrt).</summary>
        public string CategoryImage(DeviceCategory category)
            => ImageFile("category-" + category.ToString().ToLowerInvariant(), () =>
            {
                using (var trimmed = CategoryImages.LoadTrimmedBitmap(category, 256))
                {
                    if (trimmed == null)
                        return null;
                    var side = Math.Max(trimmed.Width, trimmed.Height);
                    var square = new Bitmap(side, side, PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(square))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(trimmed, (side - trimmed.Width) / 2, (side - trimmed.Height) / 2, trimmed.Width, trimmed.Height);
                    }
                    return square;
                }
            });
    }
}
