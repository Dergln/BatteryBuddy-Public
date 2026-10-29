using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace BatteryBuddy.Core.Settings
{
    /// <summary>
    /// Lädt und speichert <see cref="AppSettings"/> als JSON-Datei.
    /// Sicherheit: Die Datei kann von jedem Programm des Benutzers verändert werden. Deshalb Größenlimit,
    /// strikte Typen (keine freie Objekt-Deserialisierung) und anschließendes Sanitize().
    /// </summary>
    public sealed class SettingsStore
    {
        /// <summary>Größer ist die Datei nie legitim (500 Geräte ≈ 100 KB); schützt vor Speicher-Bomben.</summary>
        public const long MaxFileBytes = 1024 * 1024;

        private readonly string _path;
        private readonly DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(AppSettings));

        public SettingsStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Pfad fehlt.", nameof(path));
            _path = path;
        }

        /// <summary>Liefert die gespeicherten Einstellungen oder Standardwerte, wenn die Datei fehlt oder defekt ist.</summary>
        public AppSettings Load()
        {
            try
            {
                var info = new FileInfo(_path);
                if (!info.Exists)
                    return Defaults();

                if (info.Length > MaxFileBytes)
                {
                    MoveAsideCorruptFile();
                    return Defaults();
                }

                using (var stream = File.OpenRead(_path))
                {
                    var settings = (AppSettings)_serializer.ReadObject(stream);
                    settings.Sanitize();
                    return settings;
                }
            }
            catch (Exception ex) when (ex is System.Runtime.Serialization.SerializationException
                                       || ex is InvalidCastException
                                       || ex is IOException
                                       || ex is UnauthorizedAccessException
                                       || ex is ArgumentException
                                       || ex is FormatException)
            {
                // Defekte Datei nicht löschen, sondern zur Fehlersuche zur Seite legen.
                MoveAsideCorruptFile();
                return Defaults();
            }
        }

        /// <summary>Speichert atomar: erst in eine Temp-Datei schreiben, dann ersetzen. So bleibt bei einem Absturz die alte Datei heil.</summary>
        public void Save(AppSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            settings.Sanitize();
            var directory = Path.GetDirectoryName(Path.GetFullPath(_path));
            Directory.CreateDirectory(directory);

            var temp = _path + ".tmp";
            using (var stream = File.Create(temp))
            {
                _serializer.WriteObject(stream, settings);
            }

            if (File.Exists(_path))
                File.Replace(temp, _path, null);
            else
                File.Move(temp, _path);
        }

        private static AppSettings Defaults()
        {
            var s = new AppSettings();
            s.Sanitize();
            return s;
        }

        private void MoveAsideCorruptFile()
        {
            try
            {
                var target = _path + ".bad";
                if (File.Exists(target))
                    File.Delete(target);
                if (File.Exists(_path))
                    File.Move(_path, target);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // Kein Drama: Dann wird beim nächsten Speichern einfach überschrieben.
            }
        }
    }
}
