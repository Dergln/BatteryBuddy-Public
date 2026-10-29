using System;
using System.Collections.Generic;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Settings;

namespace BatteryBuddy.Core.Logic
{
    /// <summary>A simulated point in the discharge path; it never represents a real device or changes saved state.</summary>
    public sealed class DischargeSimulationStep
    {
        public ChargeLevel Level { get; }
        public int Percent { get; }
        public bool Notify { get; }

        public DischargeSimulationStep(ChargeLevel level, int percent, bool notify)
        {
            Level = level;
            Percent = percent;
            Notify = notify;
        }
    }

    /// <summary>Builds the high-to-low progression from the global thresholds and notification switches.</summary>
    public static class DischargeSimulationPlan
    {
        public static IReadOnlyList<DischargeSimulationStep> Create(AlertProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            return new[]
            {
                new DischargeSimulationStep(ChargeLevel.Green, 100, false),
                new DischargeSimulationStep(ChargeLevel.Yellow, profile.YellowAt, profile.NotifyYellow),
                new DischargeSimulationStep(ChargeLevel.Red, profile.RedAt, profile.NotifyRed),
                new DischargeSimulationStep(ChargeLevel.Critical, profile.CriticalAt, profile.NotifyCritical)
            };
        }
    }
}
