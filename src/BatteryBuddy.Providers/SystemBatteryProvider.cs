using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;

namespace BatteryBuddy.Providers
{
    /// <summary>Interner Akku (Laptop/Tablet). Nutzt die Windows-Stromversorgungs-API; auf Desktop-PCs ohne Akku liefert sie nichts.</summary>
    public sealed class SystemBatteryProvider : IBatteryProvider
    {
        public const string DeviceId = "sys:battery";

        public string Name => "System";

        public Task<IReadOnlyList<BatteryDevice>> ReadAsync(CancellationToken cancellationToken)
        {
            var status = SystemInformation.PowerStatus;
            var devices = new List<BatteryDevice>();

            // NoSystemBattery = Desktop-PC; Unknown = Windows kennt den Zustand nicht. In beiden Fällen kein Gerät melden.
            if (!status.BatteryChargeStatus.HasFlag(BatteryChargeStatus.NoSystemBattery)
                && !status.BatteryChargeStatus.HasFlag(BatteryChargeStatus.Unknown))
            {
                var percent = (int)System.Math.Round(status.BatteryLifePercent * 100f);
                var charging = status.BatteryChargeStatus.HasFlag(BatteryChargeStatus.Charging);
                devices.Add(new BatteryDevice(DeviceId, "Internal battery", DeviceCategory.InternalBattery, percent, charging, Name));
            }

            return Task.FromResult<IReadOnlyList<BatteryDevice>>(devices);
        }
    }
}
