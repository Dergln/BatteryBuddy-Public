using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BatteryBuddy.Core.Localization;

namespace BatteryBuddy.App.Services
{
    /// <summary>Zentraler Zugriff auf die Texte der Oberfläche. Sprachdateien liegen als eingebettete Ressourcen in der EXE.</summary>
    public static class Loc
    {
        private const string Prefix = "Languages.";
        private const string Suffix = ".lang";

        private static Localizer _current = new Localizer("en", null, null);

        public static bool IsRtl => _current.IsRtl;
        public static string Code => _current.Code;

        /// <summary>Alle verfügbaren Sprachcodes (aus den eingebetteten Dateien).</summary>
        public static IReadOnlyList<string> Available()
            => Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(n => n.StartsWith(Prefix, StringComparison.Ordinal) && n.EndsWith(Suffix, StringComparison.Ordinal))
                .Select(n => n.Substring(Prefix.Length, n.Length - Prefix.Length - Suffix.Length))
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();

        /// <summary>Wählt die Sprache: Nutzerwunsch, sonst Windows-Sprache, sonst Englisch.</summary>
        public static void Init(string requested)
        {
            var code = LanguageResolver.Resolve(requested, CultureInfo.CurrentUICulture.Name, Available());
            var english = Load("en");
            _current = new Localizer(code, english, code == "en" ? english : Load(code));
        }

        public static string Get(string key) => _current.Get(key);

        public static string Format(string key, params object[] args) => _current.Format(key, args);

        /// <summary>Anzeigename einer Sprache in ihrer eigenen Sprache (für die Auswahlliste).</summary>
        public static string NativeName(string code)
        {
            var name = Load(code).TryGetValue("language.name", out var n) ? n : code;
            return name;
        }

        private static Dictionary<string, string> Load(string code)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Prefix + code + Suffix))
            {
                if (stream == null)
                    return new Dictionary<string, string>();
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    return LangFile.Parse(reader.ReadToEnd());
            }
        }
    }
}
