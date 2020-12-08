using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.Core
{
    public class TechLogExporterService : BackgroundService
    {
        private readonly ILogger<TechLogExporterService> _logger;
        private readonly ITechLogExporter _techLogExporter;

        public TechLogExporterService(ILogger<TechLogExporterService> logger, ITechLogExporter techLogExporter)
        {
            _logger = logger;
            _techLogExporter = techLogExporter;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _techLogExporter.StartAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to execute TechLogExporter");
            }
        }

        public override void Dispose()
        {
            _techLogExporter?.Dispose();

            base.Dispose();
        }
    }
}
