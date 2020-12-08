using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OneSTools.TechLog
{
    public class TechLogReader : IDisposable
    {
        private string _logPath;
        private string _fileDateTime;
        private readonly bool _liveMode;
        private FileStream _fileStream;
        private StreamReader _streamReader;
        private StringBuilder _currentData = new StringBuilder();
        private ManualResetEvent _logFileChanged;
        private ManualResetEvent _logFileDeleted;
        private FileSystemWatcher _logFileWatcher;

        public bool Closed { get; private set; } = true;

        public TechLogReader(string logPath, bool liveMode = false)
        {
            _logPath = logPath;
            _liveMode = liveMode;

            var fileName = Path.GetFileNameWithoutExtension(_logPath);
            _fileDateTime = "20" +
                fileName.Substring(0, 2) +
                "-" +
                fileName.Substring(2, 2) +
                "-" +
                fileName.Substring(4, 2) +
                " " +
                fileName.Substring(6, 2);
        }

        public Dictionary<string, string> ReadNextItem(CancellationToken cancellationToken = default)
        {
            Initialize();

            var itemData = ReadItemData(cancellationToken);

            if (itemData == null)
                return null;

            return ParseItemData(itemData, cancellationToken);
        }

        public string ReadItemData(CancellationToken cancellationToken = default)
        {
            Initialize();

            var currentLine = "";

            while (!cancellationToken.IsCancellationRequested)
            {
                currentLine = _streamReader.ReadLine();

                if (currentLine == null)
                {
                    if (_currentData.Length > 0)
                        break;
                    else
                    {
                        if (_liveMode)
                        {
                            var handles = new WaitHandle[]
                            {
                                _logFileChanged,
                                _logFileDeleted,
                                cancellationToken.WaitHandle
                            };

                            var index = WaitHandle.WaitAny(handles);

                            if (index == 1 || index == 2 || index == WaitHandle.WaitTimeout) // File is deleted / reader stopped / timeout
                            {
                                Dispose();
                                Closed = true;
                                return null;
                            }
                            else if (index == 0) // File is changed, continue reading
                            {
                                _logFileChanged.Reset();
                                continue;
                            }
                        }
                    }
                }

                if (currentLine == null || _currentData.Length > 0 && Regex.IsMatch(currentLine, @"^\d\d:\d\d\.", RegexOptions.Compiled))
                    break;
                else
                    _currentData.AppendLine(currentLine);
            }

            var _strData = _currentData.ToString().Trim();

            _currentData.Clear();

            if (currentLine != null)
                _currentData.AppendLine(currentLine);

            return _fileDateTime + ":" + _strData;
        }

        public static Dictionary<string, string> ParseItemData(string itemData, CancellationToken cancellationToken = default)
        {
            var item = new Dictionary<string, string>();

            int startPosition = 0;

            var dtd = ReadNextPropertyWithoutName(itemData, ref startPosition, ',');
            var dtdLength = dtd.Length;
            var dtEndIndex = dtd.LastIndexOf('-');
            item["DateTime"] = dtd.Substring(0, dtEndIndex);
            startPosition -= dtdLength - dtEndIndex;

            item["Duration"] = ReadNextPropertyWithoutName(itemData, ref startPosition, ',');
            item["Event"] = ReadNextPropertyWithoutName(itemData, ref startPosition, ',');
            item["Level"] = ReadNextPropertyWithoutName(itemData, ref startPosition, ',');

            while (!cancellationToken.IsCancellationRequested)
            {
                var (Name, Value) = ReadNextProperty(itemData, ref startPosition);

                if (item.ContainsKey(Name))
                {
                    item.Add(GetPropertyName(item, Name, 0), Value);
                }
                else
                    item.Add(Name, Value);

                if (startPosition >= itemData.Length)
                    break;
            }

            return item;
        }

        private static string GetPropertyName(Dictionary<string, string> item, string name, int number = 0)
        {
            var currentName = $"{name}{number}";

            if (!item.ContainsKey(currentName))
                return currentName;
            else
                return GetPropertyName(item, name, number + 1);
        }

        private static string ReadNextPropertyWithoutName(string strData, ref int startPosition, char delimiter = ',')
        {
            var endPosition = strData.IndexOf(delimiter, startPosition);
            var value = strData.Substring(startPosition, endPosition - startPosition);
            startPosition = endPosition + 1;

            return value;
        }

        private static (string Name, string Value) ReadNextProperty(string strData, ref int startPosition)
        {
            var equalPosition = strData.IndexOf('=', startPosition);
            var name = strData.Substring(startPosition, equalPosition - startPosition);
            startPosition = equalPosition + 1;

            if (startPosition == strData.Length)
                return (name, "");

            var nextChar = strData[startPosition];

            int endPosition;
            switch (nextChar)
            {
                case '\'':
                    endPosition = strData.IndexOf("\',", startPosition);
                    break;
                case ',':
                    startPosition++;
                    return (name, "");
                case '"':
                    endPosition = strData.IndexOf("\",", startPosition);
                    break;
                default:
                    endPosition = strData.IndexOf(',', startPosition);
                    break;
            }

            if (endPosition < 0)
                endPosition = strData.Length;

            var value = strData.Substring(startPosition, endPosition - startPosition);
            startPosition = endPosition + 1;

            return (name, value.Trim(new char[] { '\'', '"' }).Trim());
        }

        private static string GetCleanedSql(string data)
        {
            throw new NotImplementedException();

            //// Remove paramaters
            //int startIndex = data.IndexOf("sp_executesql", StringComparison.OrdinalIgnoreCase);

            //if (startIndex < 0)
            //    startIndex = 0;
            //else
            //    startIndex += 16;

            //int e1 = data.IndexOf("', N'@P", StringComparison.OrdinalIgnoreCase);
            //if (e1 < 0)
            //    e1 = data.Length;

            //var e2 = data.IndexOf("p_0:", StringComparison.OrdinalIgnoreCase);
            //if (e2 < 0)
            //    e2 = data.Length;

            //var endIndex = Math.Min(e1, e2);

            //StringBuilder result = new StringBuilder(data[startIndex..endIndex]);

            //// Remove temp table names
            //while (true)
            //{
            //    var ttsi = result.IndexOf("#tt");

            //    if (ttsi >= 0)
            //    {
            //        var ttsei = ttsi + 2;

            //        // Read temp table number
            //        while (true)
            //        {
            //            if (char.IsDigit(result[ttsei]))
            //                ttsei++;
            //            else
            //                break;
            //        }
            //    }
            //    else
            //        break;
            //}

            //return result.ToString();
        }

        private static string GetSqlHash(string sql)
        {
            throw new NotImplementedException();
        }

        private void Initialize()
        {
            InitializeStream();

            InitializeWatcher();
        }

        private void InitializeStream()
        {
            if (_fileStream == null)
            {
                _fileStream = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                _streamReader = new StreamReader(_fileStream);

                Closed = false;
            }
        }

        private void InitializeWatcher()
        {
            if (_liveMode && _logFileWatcher == null)
            {
                _logFileChanged = new ManualResetEvent(false);
                _logFileDeleted = new ManualResetEvent(false);

                _logFileWatcher = new FileSystemWatcher(Path.GetDirectoryName(_logPath), Path.GetFileName(_logPath))
                {
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName
                };
                _logFileWatcher.Changed += _logFileWatcher_Changed;
                _logFileWatcher.Deleted += _logFileWatcher_Deleted;
                _logFileWatcher.EnableRaisingEvents = true;
            }
        }

        private void _logFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
                _logFileChanged.Set();
        }

        private void _logFileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Deleted)
                _logFileDeleted.Set();
        }

        public void Dispose()
        {
            _streamReader?.Dispose();
            _fileStream = null;
            _logFileWatcher?.Dispose();
            _logFileChanged?.Dispose();
            _logFileDeleted?.Dispose();
        }
    }
}
