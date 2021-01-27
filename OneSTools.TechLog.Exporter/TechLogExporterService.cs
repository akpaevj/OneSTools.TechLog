using Microsoft.Extensions.Hosting;
using OneSTools.TechLog.Exporter.Core;
using System.Threading;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter
{
    public class TechLogExporterService : BackgroundService
    {
        private readonly TechLogExporter _exporter;

        public TechLogExporterService(TechLogExporter exporter)
        {
            _exporter = exporter;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _exporter.StartAsync(stoppingToken);
        }
    }
}
