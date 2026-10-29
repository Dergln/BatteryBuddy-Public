using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Settings;

namespace BatteryBuddy.Core.Logic
{
    /// <summary>Wandelt einen Prozentwert anhand der Geräte-Schwellen in eine Ampelstufe um.</summary>
    public static class ThresholdEvaluator
    {
        /// <summary>Wert &lt;= CriticalAt → Kritisch, &lt;= RedAt → Rot, &lt;= YellowAt → Gelb, sonst Grün. Ohne Wert: Unknown.</summary>
        public static ChargeLevel Evaluate(int? percent, DeviceSettings settings)
        {
            if (!percent.HasValue)
                return ChargeLevel.Unknown;

            // CriticalAt = 0 steht für "nicht eingestellt" (ältere Einstellungen): dann gibt es die Stufe nicht.
            if (settings.CriticalAt > 0 && percent.Value <= settings.CriticalAt)
                return ChargeLevel.Critical;

            if (percent.Value <= settings.RedAt)
                return ChargeLevel.Red;

            return percent.Value <= settings.YellowAt ? ChargeLevel.Yellow : ChargeLevel.Green;
        }

        /// <summary>Effektive Kategorie: manuelle Zuordnung des Nutzers hat Vorrang vor der automatischen Erkennung.</summary>
        public static DeviceCategory EffectiveCategory(BatteryDevice device, DeviceSettings settings)
            => settings.CategoryOverride ?? device.Category;
    }
}
