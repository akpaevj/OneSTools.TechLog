using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneSTools.TechLog.Exporter;
using OneSTools.TechLog.Exporter.ClickHouse;
using OneSTools.TechLog.Exporter.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
                .UseWindowsService()
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    var storageType = hostContext.Configuration.GetValue("Exporter:StorageType", StorageType.None);

                    switch (storageType)
                    {
                        case StorageType.ClickHouse:
                            services.AddSingleton<ITechLogStorage, ClickHouseStorage>();
                            break;
                        default:
                            throw new Exception($"{storageType} is not available value of StorageType enum");
                    }

                    services.AddSingleton<TechLogExporter>();
                    services.AddHostedService<TechLogExporterService>();
                });
    }
}
