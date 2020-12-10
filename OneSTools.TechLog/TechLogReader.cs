using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace OneSTools.TechLog
{
    public class TechLogReader : IDisposable
    {
        private readonly TechLogReaderSettings _settings;
        private ActionBlock<string> _readBlock;
        private TransformBlock<TechLogItem, TechLogItem> _tunnelBlock;
        private CancellationToken _cancellationToken;
        private Timer _flushTimer;
        private FileSystemWatcher _logFoldersWatcher;
        private bool disposedValue;

        public TechLogReader(TechLogReaderSettings settings)
        {
            _settings = settings;
        }

        public async Task ReadAsync(Action<TechLogItem[]> processor, CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;

            var processorBlock = new ActionBlock<TechLogItem[]>(processor, new ExecutionDataflowBlockOptions() { BoundedCapacity = _settings.BatchFactor });

            var batchBlock = new BatchBlock<TechLogItem>(_settings.BatchSize);
            batchBlock.LinkTo(processorBlock, new DataflowLinkOptions() { PropagateCompletion = true });

            _readBlock = new ActionBlock<string>(logFolder => ReadLogFolder(logFolder, batchBlock, cancellationToken), new ExecutionDataflowBlockOptions() { CancellationToken = cancellationToken });

            if (_settings.LiveMode)
            {
                var timeoutMs = _settings.ReadingTimeout * 1000;

                _flushTimer = new Timer(_ => batchBlock.TriggerBatch(), null, timeoutMs, Timeout.Infinite);

                _tunnelBlock = new TransformBlock<TechLogItem, TechLogItem>(item =>
                {
                    _flushTimer.Change(timeoutMs, Timeout.Infinite);

                    return item;
                });
                _tunnelBlock.LinkTo(batchBlock, new DataflowLinkOptions() { PropagateCompletion = true });

                _ = _readBlock.Completion.ContinueWith(c => _tunnelBlock.Complete());
            }
            else
                _ = _readBlock.Completion.ContinueWith(c => batchBlock.Complete());

            foreach (var logFolder in GetExistingLogFolders())
                Post(logFolder, _readBlock, cancellationToken);

            if (_settings.LiveMode)
                InitializeWatcher();
            else
                _readBlock.Complete();

            await processorBlock.Completion;
        }

        private void ReadLogFolder(string logFolder, ITargetBlock<TechLogItem> nextBlock, CancellationToken cancellationToken = default)
        {
            var settings = new TechLogFolderReaderSettings
            {
                Folder = logFolder,
                AdditionalProperty = _settings.AdditionalProperty,
                LiveMode = _settings.LiveMode,
                ReadingTimeout = _settings.ReadingTimeout
            };

            using var reader = new TechLogFolderReader(settings);

            TechLogItem item = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    item = reader.ReadNextItem(cancellationToken);
                }
                catch (LogReaderTimeoutException)
                {
                    continue;
                }
                catch
                {
                    throw;
                }

                if (item is null)
                    break;
                else
                    Post(item, nextBlock, cancellationToken);
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

        private string[] GetExistingLogFolders()
            => Directory.GetDirectories(_settings.LogFolder);

        private void InitializeWatcher()
        {
            _logFoldersWatcher = new FileSystemWatcher(_settings.LogFolder)
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName
            };
            _logFoldersWatcher.Created += LogFileWatcherEvent;
            _logFoldersWatcher.EnableRaisingEvents = true;
        }

        private void LogFileWatcherEvent(object sender, FileSystemEventArgs e)
        {
            // new log folder has been created
            if (e.ChangeType == WatcherChangeTypes.Created && File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                Post(e.FullPath, _readBlock, _cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                }

                _logFoldersWatcher?.Dispose();

                disposedValue = true;
            }
        }

        ~TechLogReader()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
