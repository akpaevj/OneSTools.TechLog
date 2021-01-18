using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using ClickHouse.Client.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneSTools.TechLog.Exporter.ClickHouse.Properties;
using OneSTools.TechLog.Exporter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.ClickHouse
{
    public class ClickHouseStorage : ITechLogStorage
    {
        private readonly ILogger<ClickHouseStorage> _logger;
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly ClickHouseConnection _connection;

        public ClickHouseStorage(ILogger<ClickHouseStorage> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetValue("ClickHouse:ConnectionString", "");

            if (_connectionString == string.Empty)
                throw new Exception("Connection string is not specified");

            _databaseName = Regex.Match(_connectionString, "(?<=Database=).*?(?=(;|$))", RegexOptions.IgnoreCase).Value;
            _connectionString = Regex.Replace(_connectionString, "Database=.*?(;|$)", "");

            if (string.IsNullOrWhiteSpace(_databaseName))
                throw new Exception("Database name is not specified");

            _connection = new ClickHouseConnection(_connectionString);

            EnsureCreated();
        }

        public ClickHouseStorage(string connectionString, ILogger<ClickHouseStorage> logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;

            if (_connectionString == string.Empty)
                throw new Exception("Connection string is not specified");

            _databaseName = Regex.Match(_connectionString, "(?<=Database=).*?(?=(;|$))", RegexOptions.IgnoreCase).Value;
            _connectionString = Regex.Replace(_connectionString, "Database=.*?(;|$)", "");

            if (string.IsNullOrWhiteSpace(_databaseName))
                throw new Exception("Database name is not specified");

            _connection = new ClickHouseConnection(_connectionString);

            EnsureCreated();
        }

        public async Task WriteLastPositionAsync(string folder, string file, long position, CancellationToken cancellationToken = default)
        {
            var cmd = _connection.CreateCommand();

            var lastPosition = await GetLastPositionAsync(folder, file);

            if (lastPosition == 0)
                cmd.CommandText = $"INSERT INTO LastPositions (Folder, File, Position) VALUES ('{folder}', '{file}', {position})";
            else
                cmd.CommandText = $"ALTER TABLE LastPositions UPDATE Position = {position} WHERE Folder = '{folder}' AND File = '{file}'";

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<long> GetLastPositionAsync(string folder, string file, CancellationToken cancellationToken = default)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = $"SELECT Position From LastPositions WHERE Folder='{folder}' AND File='{file}'";

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
                return reader.GetInt64(0);
            else
                return 0;
        }

        public async Task WriteItemsAsync(TechLogItem[] items, CancellationToken cancellationToken = default)
        {
            var data = items.Select(item => new object[] {
                item.DateTime,
                item.EventName,
                item.Duration,
                item.PProcessName,
                item.TClientID,
                item.TApplicationName,
                item.TComputerName,
                item.TConnectID,
                item.Usr,
                item.Sql.Replace('\'', '"'),
                item.SqlHash,
                item.Context.Replace('\'', '"'),
                item.FirstContextLine.Replace('\'', '"'),
                item.LastContextLine.Replace('\'', '"')
            }).ToList();

            await WriteBulkAsync("Items", data, cancellationToken);
        }

        private async Task WriteBulkAsync(string tableName, List<object[]> entities, CancellationToken cancellationToken = default)
        {
            using var copy = new ClickHouseBulkCopy(_connection)
            {
                DestinationTableName = tableName,
                BatchSize = entities.Count
            };

            await copy.WriteToServerAsync(entities, cancellationToken);
        }

        private void EnsureCreated()
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            CreateDatabase();

            CreateTable("LastPositions", Resources.CreateLastPositionsTable);
            CreateTable("Items", Resources.CreateItemsTable);

            _logger?.LogDebug("Creating of the database is completed");
        }

        private void CreateDatabase()
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS {_databaseName}";
            cmd.ExecuteNonQuery();

            _connection.ChangeDatabase(_databaseName);

            _logger?.LogDebug($"Database {_databaseName} is created");
        }

        private void CreateTable(string tableName, string cmdText)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.ExecuteNonQuery();

            _logger?.LogDebug($"Table \"{tableName}\" is created");
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}