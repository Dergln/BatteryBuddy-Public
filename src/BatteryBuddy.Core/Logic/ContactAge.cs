using System;

namespace BatteryBuddy.Core.Logic
{
    /// <summary>Wie die Zeit seit dem letzten Kontakt angezeigt wird: "&lt;5m", "&lt;2h" oder "&gt;24h".</summary>
    public enum ContactAgeKind
    {
        LessThanMinutes = 0,
        LessThanHours = 1,
        MoreThanDay = 2
    }

    public struct ContactAgeText
    {
        public ContactAgeKind Kind;

        /// <summary>Die Zahl im Text (Minuten bzw. Stunden); bei <see cref="ContactAgeKind.MoreThanDay"/> ohne Bedeutung.</summary>
        public int Value;
    }

    /// <summary>Übersetzt eine Zeitspanne in grobe Stufen. Die eigentliche Beschriftung (Sprache) erzeugt die Oberfläche.</summary>
    public static class ContactAge
    {
        // Obergrenzen in Minuten für die feinen Stufen; danach geht es in vollen Stunden weiter.
        private static readonly int[] MinuteSteps = { 1, 5, 15, 30, 45 };

        public static ContactAgeText Describe(TimeSpan age)
        {
            // Uhr zurückgestellt o. Ä.: negative Zeiten wie "gerade eben" behandeln.
            if (age < TimeSpan.Zero)
                age = TimeSpan.Zero;

            foreach (var step in MinuteSteps)
            {
                if (age.TotalMinutes < step)
                    return new ContactAgeText { Kind = ContactAgeKind.LessThanMinutes, Value = step };
            }

            // Ab 45 Minuten: "<1h" bis "<24h", danach ">24h".
            var hours = (int)Math.Floor(age.TotalHours) + 1;
            return hours <= 24
                ? new ContactAgeText { Kind = ContactAgeKind.LessThanHours, Value = hours }
                : new ContactAgeText { Kind = ContactAgeKind.MoreThanDay, Value = 24 };
        }
    }
}
