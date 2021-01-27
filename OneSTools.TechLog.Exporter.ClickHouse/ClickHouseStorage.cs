using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneSTools.TechLog.Exporter.ClickHouse.Properties;
using OneSTools.TechLog.Exporter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace OneSTools.TechLog.Exporter.ClickHouse
{
    public class ClickHouseStorage : ITechLogStorage
    {
        private readonly ILogger<ClickHouseStorage> _logger;
        private readonly string _databaseName;
        private readonly ClickHouseConnection _connection;

        public ClickHouseStorage(ILogger<ClickHouseStorage> logger, IConfiguration configuration)
        {
            _logger = logger;
            var connectionString = configuration.GetValue("ClickHouse:ConnectionString", "");

            if (connectionString == string.Empty)
                throw new Exception("Connection string is not specified");

            _databaseName = Regex.Match(connectionString, "(?<=Database=).*?(?=(;|$))", RegexOptions.IgnoreCase).Value;
            connectionString = Regex.Replace(connectionString, "Database=.*?(;|$)", "");

            if (string.IsNullOrWhiteSpace(_databaseName))
                throw new Exception("Database name is not specified");

            _connection = new ClickHouseConnection(connectionString);

            EnsureCreated();
        }

        public ClickHouseStorage(string connectionString, ILogger<ClickHouseStorage> logger = null)
        {
            var s = connectionString;
            _logger = logger;

            if (s == string.Empty)
                throw new Exception("Connection string is not specified");

            _databaseName = Regex.Match(s, "(?<=Database=).*?(?=(;|$))", RegexOptions.IgnoreCase).Value;
            s = Regex.Replace(s, "Database=.*?(;|$)", "");

            if (string.IsNullOrWhiteSpace(_databaseName))
                throw new Exception("Database name is not specified");

            _connection = new ClickHouseConnection(s);

            EnsureCreated();
        }

        public async Task WriteLastPositionAsync(string folder, string file, long position)
        {
            var cmd = _connection.CreateCommand();

            var lastPosition = await GetLastPositionAsync(folder, file);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (lastPosition == 0)
                cmd.CommandText = $"INSERT INTO LastPositions (Folder, File, Position) VALUES ('{folder}', '{file}', {position})";
            else
                cmd.CommandText = $"ALTER TABLE LastPositions UPDATE Position = {position} WHERE Folder = '{folder}' AND File = '{file}'";

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<long> GetLastPositionAsync(string folder, string file, CancellationToken cancellationToken = default)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = $"SELECT Position From LastPositions WHERE Folder='{folder}' AND File='{file}'";

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
                return reader.GetInt64(0);
            else
                return 0;
        }

        public async Task WriteItemsAsync(TechLogItem[] items)
        {
            var data = items.Select(item => new object[] {
                item.DateTime,
                item.StartTicks,
                item.EndTicks,
                item.EventName,
                item.Duration,
                item.PProcessName ?? string.Empty,
                item.SessionID ?? string.Empty,
                item.TClientID ?? 0,
                item.TApplicationName ?? string.Empty,
                item.TComputerName ?? string.Empty,
                item.TConnectID ?? 0,
                item.Usr ?? string.Empty,
                item.Sql?.Replace('\'', '"') ?? string.Empty,
                item.SqlHash ?? string.Empty,
                item.WaitConnections ?? string.Empty,
                item.LocksInfo?.Regions.Select(c => c.Hash).ToArray() ?? new string[0],
                item.DeadlockConnectionIntersectionsInfo is null ? string.Empty : JsonSerializer.Serialize(item.DeadlockConnectionIntersectionsInfo),
                item.Context?.Replace('\'', '"') ?? string.Empty,
                item.FirstContextLine?.Replace('\'', '"') ?? string.Empty,
                item.LastContextLine?.Replace('\'', '"') ?? string.Empty,
                item.Descr?.Replace('\'', '"') ?? string.Empty
            }).ToList();

            await WriteBulkAsync("Items", data);
        }

        private async Task WriteBulkAsync(string tableName, IReadOnlyCollection<object[]> entities)
        {
            using var copy = new ClickHouseBulkCopy(_connection)
            {
                DestinationTableName = tableName,
                BatchSize = entities.Count
            };

            await copy.WriteToServerAsync(entities);
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