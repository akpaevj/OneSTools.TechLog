using System;
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
        private string _fileName;
        private FileStream _fileStream;
        private StreamReader _streamReader;
        private StringBuilder _currentData = new StringBuilder();

        public TechLogReader(string logPath)
        {
            _logPath = logPath;
            _fileName = Path.GetFileNameWithoutExtension(_logPath);
        }

        public TechLogItem ReadNextItem(CancellationToken cancellationToken = default)
        {
            InitializeStream();

            var itemData = ReadItemData(cancellationToken);

            if (itemData == null)
                return null;

            return ParseItemData(itemData, cancellationToken);
        }

        public string ReadItemData(CancellationToken cancellationToken = default)
        {
            InitializeStream();

            var currentLine = "";

            while (!cancellationToken.IsCancellationRequested)
            {
                currentLine = _streamReader.ReadLine();

                if (currentLine == null)
                {
                    if (_currentData.Length > 0)
                        break;
                    else
                        return null;
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

            return _strData;
        }

        public static TechLogItem ParseItemData(string itemData, CancellationToken cancellationToken = default)
        {
            var data = new TechLogItem();

            int startPosition = 0;

            var dtd = ReadNextPropertyWithoutName(itemData, ref startPosition, ',');
            var dtdLength = dtd.Length;
            var dtEndIndex = dtd.LastIndexOf('-');
            data.DateTime = DateTime.Parse(dtd.Substring(0, dtEndIndex));
            startPosition -= dtdLength - dtEndIndex;

            data.Duration = long.Parse(ReadNextPropertyWithoutName(itemData, ref startPosition, ','));
            data.Event = ReadNextPropertyWithoutName(itemData, ref startPosition, ',');
            data.Level = int.Parse(ReadNextPropertyWithoutName(itemData, ref startPosition, ','));

            while (!cancellationToken.IsCancellationRequested)
            {
                var (Name, Value) = ReadNextProperty(itemData, ref startPosition);

                if (data.Properties.ContainsKey(Name))
                {
                    data.Properties.Add(GetPropertyName(data, Name, 0), Value);
                }
                else
                    data.Properties.Add(Name, Value);

                if (startPosition >= itemData.Length)
                    break;
            }

            return data;
        }

        private static string GetPropertyName(TechLogItem item, string name, int number = 0)
        {
            var currentName = $"{name}{number}";

            if (!item.Properties.ContainsKey(currentName))
                return currentName;
            else
            {
                return GetPropertyName(item, name, number + 1);
            }
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

            return (name, value);
        }

        private void InitializeStream()
        {
            if (_fileStream == null)
            {
                _fileStream = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                _streamReader = new StreamReader(_fileStream);
            }
        }

        public void Dispose()
        {
            if (_fileStream == null)
            {
                _streamReader.Dispose();

                _fileStream = null;
                _streamReader = null;
            }
        }
    }
}
