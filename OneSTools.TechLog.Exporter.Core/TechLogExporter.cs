using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace OneSTools.TechLog.Exporter.Core
{
    public class TechLogExporter : IDisposable
    {
        private readonly TechLogExporterSettings _settings;
        private readonly ITechLogStorage _storage;
        private readonly ILogger<TechLogExporter> _logger;
        private readonly HashSet<string> _beingReadFiles = new HashSet<string>();

        private ActionBlock<TechLogItem[]> _writeBlock;
        private TransformBlock<TechLogItem[], TechLogItem[]> _analyzerBlock;
        private ActionBlock<string> _readBlock;
        private FileSystemWatcher _logFilesWatcher;
        private CancellationToken _cancellationToken;

        public TechLogExporter(ILogger<TechLogExporter> logger, IConfiguration configuration, ITechLogStorage storage)
        {
            _settings = new TechLogExporterSettings()
            {
                LogFolder = configuration.GetValue("Reader:LogFolder", ""),
                LiveMode = configuration.GetValue("Reader:LiveMode", true),
                BatchSize = configuration.GetValue("Reader:BatchSize", 10000),
                BatchFactor = configuration.GetValue("Reader:BatchFactor", 2),
                ReadingTimeout = configuration.GetValue("Reader:ReadingTimeout", 1)
            };

            if (string.IsNullOrWhiteSpace(_settings.LogFolder))
                throw new Exception("Log folder path is not specified");

            _storage = storage;
            _logger = logger;

            InitializeDataFlow();
        }

        public TechLogExporter(TechLogExporterSettings settings, ITechLogStorage storage, ILogger<TechLogExporter> logger = null)
        {
            _settings = settings;
            _storage = storage;
            _logger = logger;

            InitializeDataFlow();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("Exporter is started");

            _cancellationToken = cancellationToken;
            _cancellationToken.Register(() => _readBlock.Complete());

            var logFiles = Directory.GetFiles(_settings.LogFolder, "*.log", SearchOption.AllDirectories);

            foreach (var logFile in logFiles)
                StartFileReading(logFile);

            if (_settings.LiveMode)
                InitializeWatcher();
            else
                _readBlock.Complete();

            await _writeBlock.Completion;

            _logger?.LogInformation("Exporter is stopped");
        }

        private void InitializeDataFlow(CancellationToken cancellationToken = default)
        {
            var writeBlockOptions = new ExecutionDataflowBlockOptions()
            {
                CancellationToken = cancellationToken,
                BoundedCapacity = _settings.BatchFactor
            };
            _writeBlock = new ActionBlock<TechLogItem[]>(WriteItems, writeBlockOptions);

            var analyzerBlockOptions = new ExecutionDataflowBlockOptions()
            {
                CancellationToken = cancellationToken,
                BoundedCapacity = _settings.BatchFactor
            };
            _analyzerBlock = new TransformBlock<TechLogItem[], TechLogItem[]>(AnalyzeItems, analyzerBlockOptions);
            _analyzerBlock.LinkTo(_writeBlock, new DataflowLinkOptions { PropagateCompletion = true });

            var readBlockOptions = new ExecutionDataflowBlockOptions()
            {
                CancellationToken = cancellationToken,
                BoundedCapacity = DataflowBlockOptions.Unbounded
            };
            _readBlock = new ActionBlock<string>(ReadLogFileAsync, readBlockOptions);
            _ = _readBlock.Completion.ContinueWith(c => _analyzerBlock.Complete(), cancellationToken);
        }

        private async Task WriteItems(TechLogItem[] items)
        {
            try
            {
                await _storage.WriteItemsAsync(items);

                // save the file last position
                var lastItem = items[^1];

                await _storage.WriteLastPositionAsync(lastItem.FolderName, lastItem.FileName, lastItem.EndPosition);

                _logger?.LogDebug($"{DateTime.Now:HH:mm:ss.ffffff} {items.Length} events were being written");
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to write data");
            }
        }

        private static TechLogItem[] AnalyzeItems(TechLogItem[] items)
        {
            return items;

            var setContext = false;
            var context = "";
            int? tClientId = 0;
            var firstEvent = false;
            long startTicks = 0;

            for (var i = items.Length - 1; i >= 0; i--)
            {
                var item = items[i];

                // If it reached Context event then it can set context in the chain of events
                if (item.EventName == "Context")
                {
                    setContext = true;
                    context = item.Context;
                    tClientId = item.TClientID;
                    firstEvent = true;
                    startTicks = 0;

                    continue;
                }

                if (setContext && item.TClientID == tClientId)
                {
                    if (firstEvent)
                    {
                        if (item.EventName == "CALL")
                        {
                            firstEvent = false;
                            item.Context = context;
                            startTicks = item.StartTicks;
                        }
                        else
                        {
                            setContext = false;
                            context = "";
                            tClientId = 0;
                            firstEvent = false;
                            startTicks = 0;
                        }
                    }
                    else
                    {
                        // Stop if it reached an earliest time than the CALL start time
                        if (item.DateTime.Ticks > startTicks)
                            item.Context = context;
                        else
                        {
                            setContext = false;
                            context = "";
                            tClientId = 0;
                            startTicks = 0;
                        }
                    }
                }
            }

            return items;
        }

        private async Task ReadLogFileAsync(string filePath)
        {
            var items = new List<TechLogItem>(_settings.BatchSize);

            try
            {
                using var reader = new TechLogFileReader(filePath);

                var position = await _storage.GetLastPositionAsync(reader.FolderName, reader.FileName, _cancellationToken);
                reader.Position = position;

                var needFlushItems = false;

                while (!_cancellationToken.IsCancellationRequested)
                {
                    var item = reader.ReadNextItem(_cancellationToken);

                    if (item == null)
                    {
                        if (_settings.LiveMode)
                        {
                            if (needFlushItems)
                            {
                                if (items.Count > 0)
                                    Post(items.ToArray(), _analyzerBlock, _cancellationToken);

                                break;
                            }

                            needFlushItems = true;
                            await Task.Delay(_settings.ReadingTimeout * 1000, _cancellationToken);
                            continue;
                        }

                        if (items.Count > 0)
                            Post(items.ToArray(), _analyzerBlock, _cancellationToken);

                        break;
                    }

                    needFlushItems = false;

                    items.Add(item);

                    if (items.Count != items.Capacity) 
                        continue;

                    Post(items.ToArray(), _analyzerBlock, _cancellationToken);
                    items.Clear();
                }
            }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException)
            {
                _logger?.LogDebug($"File \"{filePath}\" was being deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute TechLogExporter");
                throw;
            }
            finally
            {
                StopFileReading(filePath);
            }
        }

        private static void Post<T>(T data, ITargetBlock<T> block, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (block.Post(data))
                    break;
            }
        }

        private void InitializeWatcher()
        {
            _logFilesWatcher = new FileSystemWatcher(_settings.LogFolder, "*.log")
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite
            };
            _logFilesWatcher.Created += LogFileWatcherEvent;
            _logFilesWatcher.Changed += LogFileWatcherEvent;
            _logFilesWatcher.EnableRaisingEvents = true;
        }

        private void LogFileWatcherEvent(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
                StartFileReading(e.FullPath);
            else if (e.ChangeType == WatcherChangeTypes.Changed)
                if (!IsFileBeingReading(e.FullPath))
                    StartFileReading(e.FullPath);
        }

        private bool IsFileBeingReading(string filePath)
        {
            lock (_beingReadFiles)
                return _beingReadFiles.Contains(filePath);
        }

        private void StartFileReading(string filePath)
        {
            lock (_beingReadFiles)
                _beingReadFiles.Add(filePath);

            Post(filePath, _readBlock, _cancellationToken);
        }

        private void StopFileReading(string filePath)
        {
            lock (_beingReadFiles)
                _beingReadFiles.Remove(filePath);
        }

        public void Dispose()
        {
            _logFilesWatcher?.Dispose();
            _storage?.Dispose();
        }
    }
}
