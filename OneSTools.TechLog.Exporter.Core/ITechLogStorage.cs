using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.Core
{
    public interface ITechLogStorage
    {
        Task WriteItemsAsync(TechLogItem[] items);
    }
}
