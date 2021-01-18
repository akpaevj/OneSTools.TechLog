using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.Core
{
    public interface ITechLogStorage : IDisposable
    {
        Task WriteLastPositionAsync(string folder, string file, long position, CancellationToken cancellationToken = default);
        Task<long> GetLastPositionAsync(string folder, string file, CancellationToken cancellationToken = default);
        Task WriteItemsAsync(TechLogItem[] items, CancellationToken cancellationToken = default);
    }
}
