using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace OneSTools.TechLog
{
    public class TechLogFileReader : IDisposable
    {
        private readonly string _fileDateTime;
        private FileStream _fileStream;
        private StreamReader _streamReader;
        private readonly StringBuilder _data = new StringBuilder();
        private long _lastEventEndPosition = -1;
        private bool _disposedValue;

        public string FilePath  { get; }
        public string FolderName { get; }
        public string FileName { get; }
        public long Position
        {
            get
            {
                InitializeStream();
                return _streamReader.GetPosition();
            }
            set
            {
                InitializeStream();
                _streamReader.SetPosition(value);
            }
        }

        public TechLogFileReader(string filePath)
        {
            FilePath = filePath;
            FolderName = Directory.GetParent(FilePath).Name;
            FileName = Path.GetFileName(FilePath);

            _fileDateTime = "20" +
                FileName.Substring(0, 2) +
                "-" +
                FileName.Substring(2, 2) +
                "-" +
                FileName.Substring(4, 2) +
                " " +
                FileName.Substring(6, 2);
        }

        public TechLogItem ReadNextItem(CancellationToken cancellationToken = default)
        {
            InitializeStream();

            var rawItem = ReadRawItem(cancellationToken);

            if (rawItem == null)
                return null;

            var parsed = ParseRawItem(rawItem.Trim(), cancellationToken);

            return parsed;
        }

        private ReadOnlySpan<char> ReadRawItem(CancellationToken cancellationToken = default)
        {
            InitializeStream();

            string line = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                line = _streamReader.ReadLine();

                // This is the end of the event or the end of the stream
                if (line is null || _data.Length > 0 && Regex.IsMatch(line, @"^\d\d:\d\d\.", RegexOptions.Compiled))
                    break;
                if (line.Length > 0)
                    _data.AppendLine(line);

                _lastEventEndPosition = Position;
            }

            if (_data.Length == 0)
                return null;
            var result = $"{_fileDateTime}:{_data}";
            _data.Clear();

            if (line != null)
                _data.AppendLine(line);

            return result.AsSpan();
        }

        private TechLogItem ParseRawItem(ReadOnlySpan<char> rawItem, CancellationToken cancellationToken = default)
        {
            var properties = new Dictionary<string, string>();
            
            var item = new TechLogItem
            {
                FolderName = FolderName,
                FileName = FileName,
                EndPosition = _lastEventEndPosition
            };

            // set event end position no new line
            _lastEventEndPosition = Position;

            var dtd = ReadNextPropertyWithoutName(rawItem);
            var dtEndIndex = dtd.LastIndexOf('-');
            item.DateTime = DateTime.Parse(dtd[..dtEndIndex]);
            item.EndTicks = item.DateTime.Ticks;
            var position = dtEndIndex + 1;

            var duration = ReadNextPropertyWithoutName(rawItem[position..]);
            position += duration.Length + 1;
            item.Duration = long.Parse(duration);
            item.StartTicks = item.EndTicks - item.Duration * (TimeSpan.TicksPerMillisecond / 1000);

            var eventName = ReadNextPropertyWithoutName(rawItem[position..]);
            position += eventName.Length + 1;
            item.EventName = eventName.ToString();

            var level = ReadNextPropertyWithoutName(rawItem[position..]);
            position += level.Length + 1;
            item.Level = int.Parse(level);

            while (!cancellationToken.IsCancellationRequested)
            {
                var propertyName = ReadPropertyName(rawItem[position..]);
                position += propertyName.Length + 1;
                propertyName = propertyName.Trim();

                if (position >= rawItem.Length)
                    break;

                if (propertyName == "")
                    continue;

                var propertyValue = ReadPropertyValue(rawItem[position..]);
                position += propertyValue.Length + 1;
                propertyValue = propertyValue.Trim(new[] { '\'', '"' }).Trim();

                if (!item.AllProperties.TryAdd(propertyName.ToString(), propertyValue.ToString()))
                    item.AllProperties.TryAdd(GetPropertyName(properties, propertyName), propertyValue.ToString());

                if (position >= rawItem.Length)
                    break;
            }

            return item;
        }

        private static ReadOnlySpan<char> ReadPropertyName(ReadOnlySpan<char> strData)
        {
            var endIndex = strData.IndexOf('=');

            return endIndex == -1 ? "" : strData[..endIndex];
        }

        private static ReadOnlySpan<char> ReadPropertyValue(ReadOnlySpan<char> strData)
        {
            var nextChar = strData[0];

            int endIndex;
            if (nextChar == ',')
                endIndex = 0;
            else if (nextChar == '\'')
            {
                var str = strData.ToString();
                str = str.Replace("''", "••");
                endIndex = str[1..].IndexOf('\'') + 2;
            }
            else if (nextChar == '"')
            {
                var str = strData.ToString();
                str = str.Replace("\"\"", "••");
                endIndex = str[1..].IndexOf('"') + 2;
            }
            else
                endIndex = strData.IndexOf(',');

            if (endIndex == -1)
                endIndex = strData.Length;
            else if (endIndex == 0)
                return "";

            return strData[..endIndex];
        }

        private static string GetPropertyName(IReadOnlyDictionary<string, string> properties, ReadOnlySpan<char> name, int number = 0)
        {
            while (true)
            {
                var currentName = $"{name.ToString()}{number}";

                if (!properties.ContainsKey(currentName)) 
                    return currentName;
                
                number += 1;
            }
        }

        private static ReadOnlySpan<char> ReadNextPropertyWithoutName(ReadOnlySpan<char> strData, char delimiter = ',')
        {
            var endPosition = strData.IndexOf(delimiter);
            var data = strData[..endPosition];

            return data;
        }

        private void InitializeStream()
        {
            if (_fileStream != null) 
                return;

            _fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            _streamReader = new StreamReader(_fileStream);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) 
                return;

            _streamReader?.Dispose();
            _disposedValue = true;
        }

        ~TechLogFileReader()
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