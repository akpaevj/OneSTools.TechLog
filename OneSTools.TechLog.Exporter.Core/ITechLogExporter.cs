using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.Core
{
    public interface ITechLogExporter : IDisposable
    {
        Task StartAsync(CancellationToken cancellationToken = default);
    }

    public class ExcpEventSubscriber : ITechLogEventSubscriber
    {
        public string Event => "EXCP";

        public async Task HandleItemsAsync(Dictionary<string, string> items, CancellationToken cancellationToken = default)
        {
        }
    }
}