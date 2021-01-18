using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneSTools.TechLog;
using OneSTools.TechLog.Exporter;
using OneSTools.TechLog.Exporter.ClickHouse;
using OneSTools.TechLog.Exporter.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
