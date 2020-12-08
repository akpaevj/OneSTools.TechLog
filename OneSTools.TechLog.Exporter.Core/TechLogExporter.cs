using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using OneSTools.TechLog;
using Microsoft.Extensions.Configuration;

namespace OneSTools.TechLog.Exporter.Core
{
    public class TechLogExporter : IDisposable, ITechLogExporter
    {
        private readonly ILogger<TechLogExporter> _logger;
        private ITechLogStorage _techLogStorage;
        private string _logFolder;
        private int _portion;
        private int _recordingStreams;
        private bool _liveMode;
        private ActionBlock<TechLogItem[]> _writeBlock;
        private BatchBlock<TechLogItem> _batchBlock;
        private ActionBlock<string> _parseBlock;
        private ActionBlock<string> _readBlock;
        private FileSystemWatcher _logFilesWatcher;
        private HashSet<string> _logFiles = new HashSet<string>();

        public TechLogExporter(ILogger<TechLogExporter> logger, IConfiguration configuration, ITechLogStorage techLogStorage)
        {
            _logger = logger;
            _techLogStorage = techLogStorage;

            _logFolder = configuration.GetValue("Exporter:LogFolder", "");
            if (_logFolder == string.Empty)
                throw new Exception("Log folder is not specified");

            _portion = configuration.GetValue("Exporter:Portion", 10000);
            _recordingStreams = configuration.GetValue("Exporter:RecordingStreams", 1);
            _liveMode = configuration.GetValue("Exporter:LiveMode", true);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var maxDegree = Environment.ProcessorCount;

            _writeBlock = new ActionBlock<TechLogItem[]>(_techLogStorage.WriteItemsAsync, new ExecutionDataflowBlockOptions() 
            { 
                BoundedCapacity = _recordingStreams,
                CancellationToken = cancellationToken,
            });
            _batchBlock = new BatchBlock<TechLogItem>(_portion, new GroupingDataflowBlockOptions() 
            {
                BoundedCapacity = _portion,
                CancellationToken = cancellationToken
            });
            _parseBlock = new ActionBlock<string>(str => ParseItemData(str, _batchBlock), new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = maxDegree,
                BoundedCapacity = _portion / 2,
                CancellationToken = cancellationToken
            });
            _readBlock = new ActionBlock<string>(str => ReadItemsData(str, _parseBlock), new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = maxDegree,
                CancellationToken = cancellationToken
            }); ;

            _batchBlock.LinkTo(_writeBlock);

            var logFiles = GetLogFiles();

            foreach (var logFile in logFiles)
                await StartReaderAsync(logFile, cancellationToken);

            if (_liveMode)
                StartLogFilesWatcher();

            await _writeBlock.Completion;
        }

        private async Task StartReaderAsync(string logPath, CancellationToken cancellationToken = default)
        {
            if (!_logFiles.Contains(logPath))
            {
                await SendDataAsync(_readBlock, logPath);

                _logger.LogInformation($"Log reader for \"{logPath}\" is started");
            }
        }

        private string[] GetLogFiles()
        {
            return Directory.GetFiles(_logFolder, "*.log", SearchOption.AllDirectories);
        }

        private void ReadItemsData(string logPath, ITargetBlock<string> nextblock)
        {
            using var reader = new TechLogReader(logPath, _liveMode);

            do
            {
                var itemData = reader.ReadItemData();

                if (reader.Closed)
                    _logFiles.Remove(logPath);

                if (itemData != null)
                    PostData(nextblock, itemData);
            }
            while (!reader.Closed);

            _logger.LogInformation($"Log reader for \"{logPath}\" is stopped");
        }

        private void PostData<T>(ITargetBlock<T> block, T data)
        {
            while (true)
            {
                if (block.Post(data))
                    break;
            }
        }

        private async Task SendDataAsync<T>(ITargetBlock<T> block, T data)
        {
            while (true)
            {
                if (await block.SendAsync(data))
                    break;
            }
        }

        private void ParseItemData(string itemData, ITargetBlock<TechLogItem> nextblock)
        {
            var item = TechLogReader.ParseItemData(itemData);

            PostData(nextblock, item);
        }

        private void StartLogFilesWatcher()
        {
            _logFilesWatcher = new FileSystemWatcher(_logFolder, "*.log")
            {
                NotifyFilter = NotifyFilters.CreationTime
            };
            _logFilesWatcher.Created += _logFilesWatcher_Created;
            _logFilesWatcher.IncludeSubdirectories = true;
            _logFilesWatcher.EnableRaisingEvents = true;
        }

        private async void _logFilesWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                await StartReaderAsync(e.FullPath);
            }
        }

        public void Dispose()
        {
            _logFilesWatcher?.Dispose();
        }
    }
}
