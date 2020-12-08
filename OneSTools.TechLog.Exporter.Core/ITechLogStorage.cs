using System;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.Core
{
    public interface ITechLogStorage : IDisposable
    {
        Task WriteItemsAsync(TechLogItem[] items);
    }
}
