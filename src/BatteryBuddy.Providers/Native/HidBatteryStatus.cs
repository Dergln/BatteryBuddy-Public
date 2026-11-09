namespace BatteryBuddy.Providers.Native
{
    internal sealed class HidBatteryStatus
    {
        internal int Percent { get; }
        internal bool Charging { get; }

        internal HidBatteryStatus(int percent, bool charging)
        {
            if (percent < 0 || percent > 100)
                throw new System.IO.InvalidDataException("A HID device returned an invalid battery percentage.");
            Percent = percent;
            Charging = charging;
        }
    }
}
