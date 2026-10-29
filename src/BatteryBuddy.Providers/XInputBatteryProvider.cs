using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;

namespace BatteryBuddy.Providers
{
    /// <summary>
    /// Xbox-/XInput-Controller (auch viele Drittanbieter-Gamepads). XInput ist ab Windows 8 Teil von Windows.
    /// Der Controller meldet nur vier grobe Stufen (leer/niedrig/mittel/voll), daher sind die Prozentwerte Näherungen.
    /// </summary>
    public sealed class XInputBatteryProvider : IBatteryProvider
    {
        public string Name => "XInput";

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_BATTERY_INFORMATION
        {
            public byte BatteryType;
            public byte BatteryLevel;
        }

        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetBatteryInformation")]
        private static extern uint XInputGetBatteryInformation(uint userIndex, byte devType, out XINPUT_BATTERY_INFORMATION info);

        private const uint ErrorSuccess = 0;
        private const byte BatteryTypeWired = 1;      // Kabelcontroller: kein Akku
        private const byte BatteryTypeDisconnected = 0;
        private const byte BatteryTypeUnknown = 0xFF;

        public Task<IReadOnlyList<BatteryDevice>> ReadAsync(CancellationToken cancellationToken)
        {
            var devices = new List<BatteryDevice>();
            try
            {
                for (uint slot = 0; slot < 4; slot++)
                {
                    if (XInputGetBatteryInformation(slot, 0 /* Gamepad */, out var info) != ErrorSuccess)
                        continue;

                    var percent = ToPercent(info.BatteryType, info.BatteryLevel);
                    if (percent.HasValue)
                        devices.Add(new BatteryDevice("xinput:" + slot, "Controller " + (slot + 1), DeviceCategory.Gamepad, percent, false, Name));
                }
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException)
            {
                // XInput nicht vorhanden (sehr alte/abgespeckte Windows-Version): einfach keine Geräte.
            }

            return Task.FromResult<IReadOnlyList<BatteryDevice>>(devices);
        }

        /// <summary>Wandelt die Stufen 0–3 in grobe Prozentwerte um. Null = kein Akku / unbekannt.</summary>
        internal static int? ToPercent(byte batteryType, byte batteryLevel)
        {
            if (batteryType == BatteryTypeDisconnected || batteryType == BatteryTypeWired || batteryType == BatteryTypeUnknown)
                return null;

            switch (batteryLevel)
            {
                case 0: return 5;    // leer
                case 1: return 25;   // niedrig
                case 2: return 60;   // mittel
                case 3: return 100;  // voll
                default: return null;
            }
        }
    }
}
