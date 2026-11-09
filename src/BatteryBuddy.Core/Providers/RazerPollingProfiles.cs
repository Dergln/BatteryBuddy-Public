using System.Collections.Generic;

namespace BatteryBuddy.Core.Providers
{
    internal static class RazerPollingProfiles
    {
        internal static IEnumerable<CustomPollingProfile> All
        {
            get
            {
                foreach (var group in new (string Name, byte Transaction, ushort[] Products)[]
                {
                    (Name: "Naga Epic", Transaction: (byte)0xFF, Products: new ushort[] { 0x001F }),
                    ("Mamba (2012)", (byte)0xFF, new ushort[] { 0x0024, 0x0025 }),
                    ("Ouroboros", (byte)0xFF, new ushort[] { 0x0032 }),
                    ("Naga Epic Chroma", (byte)0xFF, new ushort[] { 0x003E, 0x003F }),
                    ("Mamba", (byte)0xFF, new ushort[] { 0x0044, 0x0045 }),
                    ("Lancehead", (byte)0x3F, new ushort[] { 0x0059, 0x005A }),
                    ("Atheris", (byte)0x1F, new ushort[] { 0x0062 }),
                    ("Lancehead Wireless", (byte)0x1F, new ushort[] { 0x006F, 0x0070 }),
                    ("Mamba Wireless", (byte)0x3F, new ushort[] { 0x0072, 0x0073 }),
                    ("Pro Click", (byte)0x1F, new ushort[] { 0x0077, 0x0080 }),
                    ("Viper Ultimate", (byte)0xFF, new ushort[] { 0x007A, 0x007B }),
                    ("DeathAdder V2 Pro", (byte)0x3F, new ushort[] { 0x007C, 0x007D }),
                    ("Basilisk X HyperSpeed", (byte)0xFF, new ushort[] { 0x0083 }),
                    ("Basilisk Ultimate", (byte)0x1F, new ushort[] { 0x0086, 0x0088 }),
                    ("Naga Pro", (byte)0x1F, new ushort[] { 0x008F, 0x0090 }),
                    ("Orochi V2", (byte)0x1F, new ushort[] { 0x0094 }),
                    ("Pro Click Mini", (byte)0x1F, new ushort[] { 0x009A }),
                    ("DeathAdder V2 X HyperSpeed", (byte)0x1F, new ushort[] { 0x009C }),
                    ("Viper Mini SE", (byte)0x1F, new ushort[] { 0x009E, 0x009F }),
                    ("Viper V2 Pro", (byte)0x1F, new ushort[] { 0x00A5, 0x00A6 }),
                    ("Naga V2 Pro", (byte)0x1F, new ushort[] { 0x00A7, 0x00A8 }),
                    ("Basilisk V3 Pro", (byte)0x1F, new ushort[] { 0x00AA, 0x00AB }),
                    ("Cobra Pro", (byte)0x1F, new ushort[] { 0x00AF, 0x00B0 }),
                    ("HyperPolling Dongle", (byte)0x1F, new ushort[] { 0x00B3 }),
                    ("Naga V2 HyperSpeed", (byte)0x1F, new ushort[] { 0x00B4 }),
                    ("DeathAdder V3 Pro", (byte)0x1F, new ushort[] { 0x00B6, 0x00B7, 0x00C2, 0x00C3 }),
                    ("Viper V3 HyperSpeed", (byte)0x1F, new ushort[] { 0x00B8 }),
                    ("Basilisk V3 X HyperSpeed", (byte)0x1F, new ushort[] { 0x00B9 }),
                    ("DeathAdder V4 Pro", (byte)0x1F, new ushort[] { 0x00BE, 0x00BF }),
                    ("Viper V3 Pro", (byte)0x1F, new ushort[] { 0x00C0, 0x00C1 }),
                    ("DeathAdder V3 HyperSpeed", (byte)0x1F, new ushort[] { 0x00C4, 0x00C5 }),
                    ("Pro Click V2 Vertical", (byte)0x1F, new ushort[] { 0x00C7, 0x00C8 }),
                    ("Basilisk V3 Pro 35K", (byte)0x1F, new ushort[] { 0x00CC, 0x00CD }),
                    ("Pro Click V2", (byte)0x1F, new ushort[] { 0x00D0, 0x00D1 }),
                    ("Basilisk Mobile", (byte)0x1F, new ushort[] { 0x00D3, 0x00D4 }),
                    ("Basilisk V3 Pro 35K Phantom Green", (byte)0x1F, new ushort[] { 0x00D6, 0x00D7 })
                })
                    foreach (var product in group.Products)
                        yield return Profile(DeviceCatalog.RazerMouse, product, group.Name, group.Transaction, null);

                foreach (var item in new (ushort Product, string Name, byte Transaction, int Interface)[]
                {
                    (Product: (ushort)0x0290, Name: "DeathStalker V2 Pro", Transaction: (byte)0x9F, Interface: 2),
                    ((ushort)0x0292, "DeathStalker V2 Pro", (byte)0x1F, 3),
                    ((ushort)0x0296, "DeathStalker V2 Pro TKL", (byte)0x9F, 2),
                    ((ushort)0x0298, "DeathStalker V2 Pro TKL", (byte)0x1F, 3),
                    ((ushort)0x0271, "BlackWidow V3 Mini HyperSpeed", (byte)0x9F, 3),
                    ((ushort)0x0258, "BlackWidow V3 Mini HyperSpeed", (byte)0x1F, 3),
                    ((ushort)0x02BA, "BlackWidow V4 Mini HyperSpeed", (byte)0x9F, 3),
                    ((ushort)0x02B9, "BlackWidow V4 Mini HyperSpeed", (byte)0x1F, 3),
                    ((ushort)0x02D5, "BlackWidow V4 Tenkeyless HyperSpeed", (byte)0x9F, 2),
                    ((ushort)0x02D7, "BlackWidow V4 Tenkeyless HyperSpeed", (byte)0x1F, 3),
                    ((ushort)0x025A, "BlackWidow V3 Pro", (byte)0x3F, 2),
                    ((ushort)0x025C, "BlackWidow V3 Pro", (byte)0x9F, 2)
                })
                    yield return Profile(DeviceCatalog.RazerKeyboard, item.Product, item.Name, item.Transaction, item.Interface);
            }
        }

        private static CustomPollingProfile Profile(string protocol, ushort product, string name, byte transaction, int? usbInterface)
            => new CustomPollingProfile(protocol, 0x1532, product, null, null, 0, 0, 91,
                usbInterface, "Razer " + name, minimumReportLengths: true, transactionId: transaction, exactFeatureLength: true);
    }
}
