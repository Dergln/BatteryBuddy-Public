// Generated registrations preserve protocol-family ownership and exact catalog profiles.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class OpenMouseBatteryProviders
    {
        internal static IReadOnlyList<IBatteryProvider> Create() => new[]
        {
            ExperimentalBatteryProviders.Create("openmouse-attackshark-x11-passive", "Attack Shark X11 / Delux M600 Pro passive battery", "openmouse-attackshark-x11-passive:", OpenMouseAKBatteryProtocols.ReadAttackSharkX11, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-amkette-xs", "Amkette XS battery telemetry", "openmouse-amkette-xs:", OpenMouseAKBatteryProtocols.ReadAmkette, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-atk-bitmouse", "ATK Bitmouse battery", "openmouse-atk-bitmouse:", OpenMouseAKBatteryProtocols.ReadBitmouse, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-bytech-float88", "IPI / Bytech Float 88 battery", "openmouse-bytech-float88:", OpenMouseAKBatteryProtocols.ReadBytech, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-dareu-report8", "Dareu report-8 battery", "openmouse-dareu-report8:", OpenMouseAKBatteryProtocols.ReadDareu, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-gearhub-v5", "GearHub V5 receiver battery", "openmouse-gearhub-v5:", OpenMouseAKBatteryProtocols.ReadGearHub, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-glorious-classic", "Glorious classic / O3 battery", "openmouse-glorious-classic:", OpenMouseAKBatteryProtocols.ReadGloriousClassic, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-glorious-core2", "Glorious Core 2 battery", "openmouse-glorious-core2:", OpenMouseAKBatteryProtocols.ReadGloriousCore2, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-gwolves-report5", "G-Wolves report-5 battery", "openmouse-gwolves-report5:", OpenMouseAKBatteryProtocols.ReadGWolvesReport5, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-gwolves-xvi", "G-Wolves legacy XVI battery", "openmouse-gwolves-xvi:", OpenMouseAKBatteryProtocols.ReadGWolvesXvi, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-hitscan-xvi", "Hitscan Xvi battery", "openmouse-hitscan-xvi:", OpenMouseAKBatteryProtocols.ReadHitscanXvi, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-hyperx-haste", "HyperX Pulsefire Haste wireless battery", "openmouse-hyperx-haste:", OpenMouseAKBatteryProtocols.ReadHyperX, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-incott-8k", "Incott 8K wireless battery", "openmouse-incott-8k:", OpenMouseAKBatteryProtocols.ReadIncott, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-ajazz-nj07-nj08", "AJAZZ NJ07 / NJ08 battery", "openmouse-ajazz-nj07-nj08:", OpenMouseAKBatteryProtocols.ReadAjazz, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-ajazz-aj200p", "AJAZZ AJ200P battery", "openmouse-ajazz-aj200p:", OpenMouseAKBatteryProtocols.ReadAj200p, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-keychron-1k", "Keychron 1K mouse battery", "openmouse-keychron-1k:", OpenMouseAKBatteryProtocols.ReadKeychron1k, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-keychron-4k", "Keychron 4K battery", "openmouse-keychron-4k:", OpenMouseAKBatteryProtocols.ReadKeychron4k, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-keychron-8k", "Keychron 8K M6 battery", "openmouse-keychron-8k:", OpenMouseAKBatteryProtocols.ReadKeychron8k, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-keychron-nordic", "Keychron Nordic battery", "openmouse-keychron-nordic:", OpenMouseAKBatteryProtocols.ReadKeychronNordic, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-keychron-nape", "Keychron Nape battery", "openmouse-keychron-nape:", OpenMouseAKBatteryProtocols.ReadKeychronNape, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-ksnake-x11", "K-snake X11 battery", "openmouse-ksnake-x11:", OpenMouseAKBatteryProtocols.ReadKsnake, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-compx-target-zero", "CompX mouse-local battery", "openmouse-compx-target-zero:", OpenMouseLZBatteryProtocols.ReadCompx, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-moddo", "moddoMOUSE battery", "openmouse-moddo:", OpenMouseLZBatteryProtocols.ReadModdo, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-motospeed", "Motospeed X6 battery", "openmouse-motospeed:", OpenMouseLZBatteryProtocols.ReadMotospeed, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-pulsar-pro", "Pulsar Pro battery", "openmouse-pulsar-pro:", OpenMouseLZBatteryProtocols.ReadPulsarPro, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-pulsar-xs1", "Pulsar XS-1 battery", "openmouse-pulsar-xs1:", OpenMouseLZBatteryProtocols.ReadPulsarXs1, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-vaxee", "VAXEE battery", "openmouse-vaxee:", OpenMouseLZBatteryProtocols.ReadVaxee, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-orbital", "Orbital DMS battery", "openmouse-orbital:", OpenMouseLZBatteryProtocols.ReadOrbital, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-ryunix", "Ryunix KYU PRO MX1 battery", "openmouse-ryunix:", OpenMouseLZBatteryProtocols.ReadRyunix, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-mchose-v3", "MCHOSE V3 battery", "openmouse-mchose-v3:", OpenMouseLZBatteryProtocols.ReadMchoseV3, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-rapoo-config", "Rapoo VT battery GET_REPORT", "openmouse-rapoo-config:", OpenMouseLZBatteryProtocols.ReadRapoo, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-rapoo-feature", "Rapoo VT battery feature", "openmouse-rapoo-feature:", OpenMouseLZBatteryProtocols.ReadRapooFeature, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-redragon-m690", "Redragon M690 PRO battery", "openmouse-redragon-m690:", OpenMouseLZBatteryProtocols.ReadRedragonM690, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-steelseries", "SteelSeries model-specific battery", "openmouse-steelseries:", OpenMouseLZBatteryProtocols.ReadSteelSeries, category: DeviceCategory.Mouse),
            ExperimentalBatteryProviders.Create("openmouse-valkyrie", "Valkyrie read-only identity battery", "openmouse-valkyrie:", OpenMouseLZBatteryProtocols.ReadValkyrie, category: DeviceCategory.Mouse)
        };

        internal static HidBatteryStatus ReadExtension(string protocol, HidInterfaceInfo info, IHidTransport channel,
            CancellationToken ct, Func<HidInterfaceInfo, IHidTransport, CancellationToken, HidBatteryStatus> fallback)
        {
            var profile = DeviceCatalog.CustomProfiles.FirstOrDefault(candidate =>
                candidate.ProtocolId == protocol && ExperimentalBatteryProviders.Matches(candidate, info));
            switch (profile?.ReaderId)
            {
                case "OpenMouseLZBatteryProtocols.ReadCompx": return OpenMouseLZBatteryProtocols.ReadCompx(info, channel, ct);
                case "OpenMouseLZBatteryProtocols.ReadCompxPower": return OpenMouseLZBatteryProtocols.ReadCompxPower(info, channel, ct);
                case null: return fallback(info, channel, ct);
                default: throw new InvalidOperationException("Unregistered battery reader.");
            }
        }
    }
}
