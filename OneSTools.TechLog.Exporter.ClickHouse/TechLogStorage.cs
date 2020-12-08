using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneSTools.TechLog.Exporter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.ClickHouse
{
    public class TechLogStorage : ITechLogStorage
    {
        private readonly ILogger<TechLogStorage> _logger;
        private readonly ClickHouseConnection _connection;

        public TechLogStorage(ILogger<TechLogStorage> logger, IConfiguration configuration)
        {
            _logger = logger;

            var connectionString = configuration.GetConnectionString("Default");

            _connection = new ClickHouseConnection(connectionString);
            _connection.Open();

            CreateTechLogItemsTable();
        }

        private void CreateTechLogItemsTable()
        {
            var commandText =
                @"CREATE TABLE IF NOT EXISTS TechLogItems
                (
                    DateTime DateTime Codec(Delta, LZ4),
                    Duration Int64 Codec(DoubleDelta, LZ4),
                    Event LowCardinality(String),
                    Level Int64 Codec(DoubleDelta, LZ4)
                )
                engine = MergeTree()
                PARTITION BY (toYYYYMM(DateTime))
                PRIMARY KEY (DateTime)
                ORDER BY (DateTime)
                SETTINGS index_granularity = 8192;";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.ExecuteNonQuery();
        }

        public async Task WriteItemsAsync(TechLogItem[] items)
        {
            using var copy = new ClickHouseBulkCopy(_connection)
            {
                DestinationTableName = "TechLogItems",
                BatchSize = items.Length
            };

            var data = items.Select(item => new object[] {
                item.DateTime,
                item.Duration,
                item.Event,
                item.Level,
                item.Properties.Select(c => c.Key).ToArray(),
                item.Properties.Select(c => c.Value).ToArray()
            }).AsEnumerable();

            try
            {
                await copy.WriteToServerAsync(data);

                _logger.LogInformation($"{DateTime.Now:hh:mm:ss:fffff} TechLogExporter has written {items.Length}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{DateTime.Now:hh:mm:ss:fffff} Failed to write data into database");
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
