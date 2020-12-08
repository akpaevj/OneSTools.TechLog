using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneSTools.TechLog.Exporter.Core;

namespace OneSTools.TechLog.Exporter.ClickHouse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ITechLogStorage, TechLogStorage>();
                    services.AddSingleton<ITechLogExporter, TechLogExporter>();
                    services.AddHostedService<TechLogExporterService>();
                });
    }
}
