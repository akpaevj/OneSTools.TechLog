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
        private IConfiguration _configuration;
        private readonly ILogger<TechLogExporterService> _logger;
        private readonly ITechLogFolderReader _techLogFolderReader;
        private string _logFolder;
        private int _portion;

        public TechLogExporterService(IConfiguration configuration, ILogger<TechLogExporterService> logger, ITechLogFolderReader techLogFolderReader)
        {
            _configuration = configuration;
            _logger = logger;
            _techLogFolderReader = techLogFolderReader;

            _logFolder = configuration.GetValue("Exporter:LogFolder", "");

            if (_logFolder == "")
                throw new Exception("Log folder's path is not set");

            _portion = configuration.GetValue("Exporter", 10000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _techLogFolderReader.StartAsync(_logFolder, _portion, true, stoppingToken);
        }
    }
}
