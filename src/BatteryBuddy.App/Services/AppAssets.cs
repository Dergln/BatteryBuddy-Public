using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BatteryBuddy.App.Services
{
    /// <summary>Zugriff auf die in die EXE eingebetteten Dateien (Symbol, Standardton). Quellen und Lizenzen: siehe Tab "Beiträge".</summary>
    public static class AppAssets
    {
        private const string IconName = "Assets.batterybuddy.ico";
        private const string SoundName = "Assets.low-battery.wav";

        private static Stream Open(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream(name);

        /// <summary>Symbol für die Fenster-Titelleiste (WPF). Bei einem Fehler null: das Fenster bleibt dann einfach ohne Symbol.</summary>
        public static ImageSource WindowIcon()
        {
            try
            {
                using (var stream = Open(IconName))
                {
                    if (stream == null)
                        return null;
                    // OnLoad liest den Stream sofort komplett; danach darf er geschlossen werden.
                    var frame = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    frame.Freeze();
                    return frame;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Platzhalterbild für "Geräte werden noch gesucht" (null, falls es fehlt: dann zeigt die Oberfläche nur Text).</summary>
        public static ImageSource LoadingImage() => Picture("Assets.loading.png");

        /// <summary>Bild für "keine Akkus gefunden".</summary>
        public static ImageSource ConfusedBatteryImage() => Picture("Assets.battery-confused.png");

        /// <summary>PNG-Original fuer Bildvorschauen; die ICO-Standardebene hat nur 16 Pixel.</summary>
        internal static ImageSource AppIconImage() => RequiredPicture("Assets.batterybuddy.png");

        /// <summary>Plattform-Symbol fuer den Sound-Credit, nicht das Lautsprecher-Geraetebild.</summary>
        internal static ImageSource FreesoundImage() => RequiredPicture("Assets.freesound.png");

        /// <summary>Die drei fest eingebetteten Bilder fuer den Herkunftshinweis.</summary>
        internal static ImageSource OriginImage(string name)
        {
            if (name != "idea" && name != "ai" && name != "teamwork")
                throw new ArgumentOutOfRangeException(nameof(name));
            return RequiredPicture("Assets." + name + ".png");
        }

        /// <summary>Eingebettete Grafiken für Lizenz-/Pro-Tabs, Fußzeile und Beiträge.</summary>
        internal static ImageSource InfoImage(string name)
        {
            switch (name)
            {
                case "gplv3":
                case "copyright":
                case "for-free":
                case "license":
                case "open-source":
                case "joke":
                case "joke-orig":
                case "lol":
                case "pill":
                case "feel-good":
                case "gpl-icon":
                case "paypal":
                case "everybody":
                case "bug":
                    return RequiredPicture("Assets." + name + ".png");
                default:
                    throw new ArgumentOutOfRangeException(nameof(name));
            }
        }

        private static ImageSource RequiredPicture(string name)
        {
            using (var stream = Open(name)
                ?? throw new InvalidOperationException("The embedded image is missing: " + name))
            {
                var frame = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                frame.Freeze();
                return frame;
            }
        }

        /// <summary>Large balloon-tip image. The caller keeps it alive until the notification closes.</summary>
        public static Icon NotificationIcon()
        {
            using (var stream = Open("Assets.battery-confused.png")
                ?? throw new InvalidOperationException("The embedded notification image is missing."))
            using (var image = new Bitmap(stream))
            using (var bitmap = new Bitmap(image, SystemInformation.IconSize))
                return TrayIconRenderer.CreateIcon(bitmap);
        }

        private static ImageSource Picture(string name)
        {
            try
            {
                using (var stream = Open(name))
                {
                    if (stream == null)
                        return null;
                    var frame = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    frame.Freeze();
                    return frame;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Tray-Symbol für den Fall "keine Werte vorhanden" in der passenden Größe (DPI). Der Aufrufer gibt es mit Dispose frei.</summary>
        public static Icon TrayIcon()
        {
            try
            {
                using (var stream = Open(IconName))
                    return stream == null ? null : new Icon(stream, SystemInformation.SmallIconSize);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Der eingebettete Standardton als WAV-Bytes (leer, falls er fehlt).</summary>
        public static byte[] DefaultSound()
        {
            try
            {
                using (var stream = Open(SoundName))
                {
                    if (stream == null)
                        return new byte[0];
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }
    }
}
