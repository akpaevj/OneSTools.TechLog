using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OneSTools.TechLog
{
    /// <summary>
    /// Watch tech log folder and raise created/changed events
    /// </summary>
    class TechLogFolderWatcher: IDisposable
    {
        private FileSystemWatcher _watcher;
        private bool disposedValue;

        public string Folder { get; private set; }
        public HashSet<string> ExceceptionPaths { get; private set; } = new HashSet<string>();

        delegate void FileCreatedHandler(string path);
        event FileCreatedHandler FileCreated;

        delegate void FileChangedHandler(string path);
        event FileChangedHandler FileChanged;

        public TechLogFolderWatcher(string folder)
        {
            Folder = folder;
        }

        public void StartWatching()
        {
            _watcher = new FileSystemWatcher(Folder, "*.log")
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite
            };
            _watcher.IncludeSubdirectories = true;
            _watcher.Changed += Watcher_Changed;
            _watcher.Created += Watcher_Created;
            _watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
            => FileCreated?.Invoke(e.FullPath);

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (ExceceptionPaths.Contains(e.FullPath))
                return;

            FileChanged?.Invoke(e.FullPath);
        }

        public void StopWatching()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= Watcher_Changed;
            _watcher.Created -= Watcher_Created;
            _watcher.Dispose();
            _watcher = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ExceceptionPaths.Clear();
                }

                StopWatching();
                disposedValue = true;
            }
        }

        ~TechLogFolderWatcher()
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
