using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.Core
{
    public class TechLogEventSubscriber : ITechLogEventSubscriber
    {
        public string Event => "EXCP";

        public async Task HandleItemAsync(Dictionary<string, string> item, CancellationToken cancellationToken = default)
        {
            
        }
    }
}