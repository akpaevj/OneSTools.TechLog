using Microsoft.Extensions.Logging;
using Nest;
using OneSTools.TechLog.Exporter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.ElasticSearch
{
    public class TechLogStorage : ITechLogStorage
    {
        private readonly ILogger<TechLogStorage> _logger;
        private string _index;
        private string _separation;
        ElasticClient _client;

        public TechLogStorage(ILogger<TechLogStorage> logger, string host, int port = 9200, string index = "", string separation = "")
        {
            _logger = logger;

            var uri = new Uri($"{host}:{port}");
            _index = $"{index}-tl";

            var settings = new ConnectionSettings(uri);
            settings.DefaultIndex(_index);

            _separation = separation;

            _client = new ElasticClient(settings);
            var response = _client.Ping();

            if (!response.IsValid)
                throw response.OriginalException;
        }

        public async Task WriteItemsAsync(TechLogItem[] items)
        {
            var data = new List<(string IndexName, TechLogItem[] Items)>();

            switch (_separation)
            {
                case "H":
                    var groups = items.GroupBy(c => c.DateTime.ToString("yyyyMMddhh")).OrderBy(c => c.Key);
                    foreach (IGrouping<string, TechLogItem> item in groups)
                        data.Add(($"{_index}-{item.Key}", item.ToArray()));
                    break;
                case "D":
                    groups = items.GroupBy(c => c.DateTime.ToString("yyyyMMdd")).OrderBy(c => c.Key);
                    foreach (IGrouping<string, TechLogItem> item in groups)
                        data.Add(($"{_index}-{item.Key}", item.ToArray()));
                    break;
                case "M":
                    groups = items.GroupBy(c => c.DateTime.ToString("yyyyMM")).OrderBy(c => c.Key);
                    foreach (IGrouping<string, TechLogItem> item in groups)
                        data.Add(($"{_index}-{item.Key}", item.ToArray()));
                    break;
                default:
                    data.Add(($"{_index}-all", items));
                    break;
            }

            foreach ((string IndexName, TechLogItem[] Entities) item in data)
            {
                var responseItems = await _client.IndexManyAsync(item.Entities, item.IndexName);

                if (!responseItems.IsValid)
                {
                    throw responseItems.OriginalException;
                }

                _logger.LogInformation($"{DateTime.Now:hh:mm:ss:fffff} has written {item.Entities.Length}");
            }
        }
    }
}
