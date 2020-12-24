using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace OneSTools.TechLog
{
    public class TechLogFolderReader : IDisposable
    {
        private readonly TechLogFolderReaderSettings _settings;
        private TechLogFileReader _logReader;
        private ManualResetEvent _logChangedCreated;
        private FileSystemWatcher _logfilesWatcher;
        private bool disposedValue;

        public TechLogFolderReader(TechLogFolderReaderSettings settings)
        {
            _settings = settings;
        }

        public TechLogItem ReadNextItem(CancellationToken cancellationToken = default)
        {
            if (_logReader is null)
                SetNextLogReader();

            if (_settings.LiveMode && _logfilesWatcher is null)
                StartLogFilesWatcher();

            TechLogItem item = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    item = _logReader.ReadNextItem(cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    item = null;
                    _logReader = null;
                    break;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (item == null)
                {
                    var newReader = SetNextLogReader();

                    if (_settings.LiveMode)
                    {
                        if (!newReader)
                        {
                            _logChangedCreated.Reset();

                            var waitHandle = WaitHandle.WaitAny(new WaitHandle[] { _logChangedCreated, cancellationToken.WaitHandle }, _settings.ReadingTimeout * 1000);

                            if (_settings.ReadingTimeout != Timeout.Infinite && waitHandle == WaitHandle.WaitTimeout)
                                throw new LogReaderTimeoutException();

                            _logChangedCreated.Reset();
                        }
                    }
                    else
                    {
                        if (!newReader)
                            break;
                    }
                }
                else
                    break;
            }

            return item;
        }

        private bool SetNextLogReader()
        {
            var currentReaderLastWriteDateTime = DateTime.MinValue;

            if (_logReader != null)
                currentReaderLastWriteDateTime = new FileInfo(_logReader.LogPath).LastWriteTime;

            var filesDateTime = new List<(string FilePath, DateTime LastWriteTime)>();

            var files = Directory.GetFiles(_settings.Folder, "*.log");

            foreach (var file in files)
            {
                if (_logReader != null)
                {
                    if (_logReader.LogPath != file)
                        filesDateTime.Add((file, new FileInfo(file).LastWriteTime));
                }
                else
                    filesDateTime.Add((file, new FileInfo(file).LastWriteTime));
            }

            var orderedFiles = filesDateTime.OrderBy(c => c.LastWriteTime).ToList();

            var (FilePath, LastWriteTime) = orderedFiles.FirstOrDefault(c => c.LastWriteTime > currentReaderLastWriteDateTime);

            if (string.IsNullOrEmpty(FilePath))
                return false;
            else
            {
                _logReader?.Dispose();

                _logReader = new TechLogFileReader(FilePath, _settings.AdditionalProperty);

                return true;
            }
        }

        private void StartLogFilesWatcher()
        {
            _logChangedCreated = new ManualResetEvent(false);

            _logfilesWatcher = new FileSystemWatcher(_settings.Folder, "*.log")
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite
            };
            _logfilesWatcher.Changed += LgpFilesWatcher_Event;
            _logfilesWatcher.Created += LgpFilesWatcher_Event;
            _logfilesWatcher.EnableRaisingEvents = true;
        }

        private void LgpFilesWatcher_Event(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed)
                _logChangedCreated.Set();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                }

                _logReader?.Dispose();
                _logChangedCreated?.Dispose();
                _logfilesWatcher?.Dispose();

                disposedValue = true;
            }
        }

        ~TechLogFolderReader()
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
