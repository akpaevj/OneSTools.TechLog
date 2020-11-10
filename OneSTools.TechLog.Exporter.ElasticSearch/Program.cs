using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneSTools.TechLog.Exporter.Core;

namespace OneSTools.TechLog.Exporter.ElasticSearch
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
                    var configuration = hostContext.Configuration;
                    var host = configuration.GetValue("ElasticSearch:Host", "");
                    var port = configuration.GetValue("ElasticSearch:Port", 9200);
                    var index = configuration.GetValue("ElasticSearch:Index", "");
                    var separation = configuration.GetValue("ElasticSearch:Separation", "");

                    services.AddSingleton<ITechLogStorage>(sp =>
                    {
                        var logger = sp.GetService<ILogger<TechLogStorage>>();

                        return new TechLogStorage(logger, host, port, index, separation);
                    });
                    services.AddSingleton<ITechLogFolderReader, TechLogFolderReader>();
                    services.AddHostedService<TechLogExporterService>();
                });
    }
}
