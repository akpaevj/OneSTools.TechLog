using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.Core
{
    public interface ITechLogStorage : IDisposable
    {
        Task WriteLastPositionAsync(string folder, string file, long position);
        Task<long> GetLastPositionAsync(string folder, string file, CancellationToken cancellationToken = default);
        Task WriteItemsAsync(TechLogItem[] items);
    }
}
