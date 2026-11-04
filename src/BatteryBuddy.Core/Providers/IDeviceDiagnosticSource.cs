using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BatteryBuddy.Core.Models;

namespace BatteryBuddy.Core.Providers
{
    public interface IDeviceDiagnosticSource
    {
        string Name { get; }
        Task<IReadOnlyList<DeviceDiagnostic>> ReadAsync(CancellationToken cancellationToken);
    }
}
