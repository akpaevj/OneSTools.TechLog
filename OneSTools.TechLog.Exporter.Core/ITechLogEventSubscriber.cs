using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.Core
{
    public interface ITechLogEventSubscriber
    {
        public string Event { get; }
        public Task HandleItemAsync(Dictionary<string, string> item, CancellationToken cancellationToken = default);
    }
}