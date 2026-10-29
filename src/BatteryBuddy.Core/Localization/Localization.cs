using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BatteryBuddy.Core.Localization
{
    /// <summary>
    /// Liest Sprachdateien im einfachen Format "schluessel=Text" (eine Zeile je Eintrag).
    /// Zeilen mit # am Anfang und Leerzeilen werden ignoriert. "\n" im Text wird zu einem Zeilenumbruch.
    /// </summary>
    public static class LangFile
    {
        public const int MaxKeyLength = 80;
        public const int MaxValueLength = 500;

        public static Dictionary<string, string> Parse(string text)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(text))
                return result;

            using (var reader = new StringReader(text.TrimStart('\uFEFF')))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line[0] == '#')
                        continue;

                    var eq = line.IndexOf('=');
                    if (eq <= 0)
                        continue;

                    var key = line.Substring(0, eq).Trim();
                    var value = line.Substring(eq + 1).Trim().Replace("\\n", "\n");
                    if (key.Length > MaxKeyLength || value.Length > MaxValueLength)
                        continue;

                    result[key] = value;
                }
            }

            return result;
        }
    }

    /// <summary>Wählt aus den vorhandenen Sprachdateien die passende aus (Wunsch -> Windows-Sprache -> Englisch).</summary>
    public static class LanguageResolver
    {
        public const string Fallback = "en";

        public static string Resolve(string requested, string osCulture, IEnumerable<string> available)
        {
            var codes = (available ?? Enumerable.Empty<string>()).ToList();
            var wanted = !string.IsNullOrWhiteSpace(requested) ? requested : osCulture;
            if (string.IsNullOrWhiteSpace(wanted))
                return Fallback;

            wanted = Alias(wanted.Trim());

            var exact = codes.FirstOrDefault(c => string.Equals(c, wanted, StringComparison.OrdinalIgnoreCase));
            if (exact != null)
                return exact;

            // "pt-BR" -> "pt", "de-AT" -> "de"
            var dash = wanted.IndexOf('-');
            var neutral = dash > 0 ? wanted.Substring(0, dash) : wanted;
            var byNeutral = codes.FirstOrDefault(c => string.Equals(c, neutral, StringComparison.OrdinalIgnoreCase));
            return byNeutral ?? Fallback;
        }

        /// <summary>Windows nennt Chinesisch und Norwegisch teils anders, als unsere Dateien heißen.</summary>
        private static string Alias(string code)
        {
            switch (code.ToLowerInvariant())
            {
                case "zh":
                case "zh-hans":
                case "zh-sg":
                case "zh-hans-cn":
                    return "zh-CN";
                case "zh-hant":
                case "zh-hk":
                case "zh-mo":
                case "zh-hant-tw":
                    return "zh-TW";
                case "no":
                case "nn":
                case "nn-no":
                case "nb-no":
                    return "nb";
                default:
                    return code;
            }
        }
    }

    /// <summary>Liefert Texte; fehlt ein Schlüssel in der gewählten Sprache, wird Englisch genutzt, zuletzt der Schlüssel selbst.</summary>
    public sealed class Localizer
    {
        private static readonly HashSet<string> RtlLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ar", "he", "fa", "ur" };

        private readonly IDictionary<string, string> _fallback;
        private readonly IDictionary<string, string> _current;

        public string Code { get; }
        public bool IsRtl { get; }

        public Localizer(string code, IDictionary<string, string> english, IDictionary<string, string> current)
        {
            Code = code ?? LanguageResolver.Fallback;
            _fallback = english ?? new Dictionary<string, string>();
            _current = current ?? new Dictionary<string, string>();
            var dash = Code.IndexOf('-');
            IsRtl = RtlLanguages.Contains(dash > 0 ? Code.Substring(0, dash) : Code);
        }

        public string Get(string key)
        {
            if (key != null && _current.TryGetValue(key, out var v))
                return v;
            if (key != null && _fallback.TryGetValue(key, out v))
                return v;
            return key ?? string.Empty;
        }

        public string Format(string key, params object[] args)
        {
            var template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                // Fehlerhafte Übersetzung darf die App nie zum Absturz bringen.
                return template;
            }
        }
    }
}
