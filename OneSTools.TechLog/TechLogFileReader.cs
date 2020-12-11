using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OneSTools.TechLog
{
    public class TechLogFileReader : IDisposable
    {
        private readonly string _fileDateTime;
        private readonly List<string> _properties;
        private readonly AdditionalProperty _additionalProperty;
        private FileStream _fileStream;
        private StreamReader _streamReader;
        private readonly StringBuilder _data = new StringBuilder();
        private long _lastEventEndPosition = -1;
        private bool disposedValue;

        public string LogPath { get; private set; }
        public long Position
        {
            get => _streamReader.GetPosition();
            set => _streamReader.SetPosition(value);
        }

        public TechLogFileReader(string logPath, List<string> properties, AdditionalProperty additionalProperty)
        {
            LogPath = logPath;
            _properties = properties;
            _additionalProperty = additionalProperty;

            var fileName = Path.GetFileNameWithoutExtension(LogPath);

            _fileDateTime = "20" +
                fileName.Substring(0, 2) +
                "-" +
                fileName.Substring(2, 2) +
                "-" +
                fileName.Substring(4, 2) +
                " " +
                fileName.Substring(6, 2);
        }

        public TechLogItem ReadNextItem(CancellationToken cancellationToken = default)
        {
            InitializeStream();

            var rawItem = ReadRawItem(cancellationToken);

            if (rawItem == null)
                return null;

            return ParseRawItem(rawItem, cancellationToken);
        }

        private string ReadRawItem(CancellationToken cancellationToken = default)
        {
            InitializeStream();

            string line = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                line = _streamReader.ReadLine();

                // This is the end of the event or the end of the stream
                if (line is null || _data.Length > 0 && Regex.IsMatch(line, @"^\d\d:\d\d\.", RegexOptions.Compiled))
                    break;
                else
                {
                    _data.AppendLine(line);
                    _lastEventEndPosition = Position;
                }
            }

            if (_data.Length == 0)
                return null;
            else
            {
                var result = $"{_fileDateTime}:{_data}";
                _data.Clear();

                if (line != null)
                    _data.AppendLine(line);

                return result;
            } 
        }

        private TechLogItem ParseRawItem(string rawItem, CancellationToken cancellationToken = default)
        {
            var item = new TechLogItem();

            int startPosition = 0;

            var dtd = ReadNextPropertyWithoutName(rawItem, ref startPosition, ',');
            var dtdLength = dtd.Length;
            var dtEndIndex = dtd.LastIndexOf('-');
            item.TrySetPropertyValue("DateTime", dtd.Substring(0, dtEndIndex));
            startPosition -= dtdLength - dtEndIndex;

            item.TrySetPropertyValue("Duration", ReadNextPropertyWithoutName(rawItem, ref startPosition, ','));
            item.TrySetPropertyValue("Event", ReadNextPropertyWithoutName(rawItem, ref startPosition, ','));
            item.TrySetPropertyValue("Level", ReadNextPropertyWithoutName(rawItem, ref startPosition, ','));

            if (_properties.Count > 0)
            {
                foreach (var property in _properties)
                {
                    var value = ReadPropertyValue(rawItem, property);

                    if (!(value is null))
                        if (!item.TrySetPropertyValue(property, value))
                            item.TrySetPropertyValue(GetPropertyName(item, property, 0), value);
                }
            }
            else
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var (Name, Value) = ReadNextProperty(rawItem, ref startPosition);

                    if (string.IsNullOrEmpty(Name))
                        break;

                    // Property with the same name already can exist, so we have to get a new name for the value
                    if (!item.TrySetPropertyValue(Name, Value))
                        item.TrySetPropertyValue(GetPropertyName(item, Name, 0), Value);

                    if (startPosition >= rawItem.Length)
                        break;
                }
            }

            SetAdditionalProperties(item);

            return item;
        }

        private bool NeedAdditionalProperty(AdditionalProperty additionalProperty)
            => (_additionalProperty & additionalProperty) == additionalProperty;

        private string GetPropertyName(TechLogItem item, string name, int number = 0)
        {
            var currentName = $"{name}{number}";

            if (!item.HasProperty(currentName))
                return currentName;
            else
                return GetPropertyName(item, name, number + 1);
        }

        private string ReadNextPropertyWithoutName(string strData, ref int startPosition, char delimiter = ',')
        {
            var endPosition = strData.IndexOf(delimiter, startPosition);
            var value = strData[startPosition..endPosition];
            startPosition = endPosition + 1;

            return value;
        }

        private (string Name, string Value) ReadNextProperty(string strData, ref int startPosition)
        {
            var equalPosition = strData.IndexOf('=', startPosition);
            if (equalPosition < 0)
                return ("", "");

            var name = strData[startPosition..equalPosition];
            startPosition = equalPosition + 1;

            if (startPosition == strData.Length)
                return (name, "");

            var value = GetPropertyValue(strData, ref startPosition);

            return (name, value);
        }

        private string GetPropertyValue(string strData, ref int startPosition)
        {
            var nextChar = strData[startPosition];

            int endPosition;
            switch (nextChar)
            {
                case '\'':
                    endPosition = strData.IndexOf('\'', startPosition + 1);
                    break;
                case ',':
                    startPosition++;
                    return "";
                case '"':
                    endPosition = strData.IndexOf('"', startPosition + 1);
                    break;
                default:
                    endPosition = strData.IndexOf(',', startPosition);
                    break;
            }

            if (endPosition < 0)
                endPosition = strData.Length;

            var value = strData[startPosition..endPosition];
            startPosition = endPosition + 1;

            return value.Trim(new char[] { '\'', '"' }).Trim();
        }

        private string ReadPropertyValue(string strData, string name)
        {
            var index = strData.IndexOf($",{name}=");

            if (index >= 0)
            {
                int pos = index + name.Length + 2;

                return GetPropertyValue(strData, ref pos);
            }
            else
                return null;
        }

        private void SetAdditionalProperties(TechLogItem item)
        {
            TrySetCleanSqlProperty(item);
            TrySetSqlHashProperty(item);
            TrySetFirstContextLineProperty(item);
            TrySetLastContextLineProperty(item);

            if (NeedAdditionalProperty(AdditionalProperty.EndPosition))
            {
                item.EndPosition = _lastEventEndPosition;
                _lastEventEndPosition = Position;
            }
        }

        private bool TrySetCleanSqlProperty(TechLogItem item)
        {
            if (NeedAdditionalProperty(AdditionalProperty.CleanedSql) && item.TryGetPropertyValue("Sql", out var sql))
            {
                item["CleanSql"] = ClearSql(sql);

                return true;
            }

            return false;
        }

        private string ClearSql(string data)
        {
            var dataSpan = data.AsSpan();

            // Remove parameters
            int startIndex = dataSpan.IndexOf("sp_executesql", StringComparison.OrdinalIgnoreCase);

            if (startIndex < 0)
                startIndex = 0;
            else
                startIndex += 16;

            int e1 = dataSpan.IndexOf("', N'@P", StringComparison.OrdinalIgnoreCase);
            if (e1 < 0)
                e1 = dataSpan.Length;

            var e2 = dataSpan.IndexOf("p_0:", StringComparison.OrdinalIgnoreCase);
            if (e2 < 0)
                e2 = dataSpan.Length;

            var endIndex = Math.Min(e1, e2);

            // Remove temp table names, parameters and guids
            var result = Regex.Replace(dataSpan[startIndex..endIndex].ToString(), @"(#tt\d+|@P\d+|\d{8}-\d{4}-\d{4}-\d{4}-\d{12})", "{RD}", RegexOptions.ExplicitCapture);

            return result;
        }

        private bool TrySetSqlHashProperty(TechLogItem item)
        {
            bool needCalculateHash = NeedAdditionalProperty(AdditionalProperty.SqlHash);

            if (!item.HasProperty("CleanSql"))
                needCalculateHash = TrySetCleanSqlProperty(item);

            if (needCalculateHash && item.TryGetPropertyValue("CleanSql", out var cleanedSql))
                item["SqlHash"] = GetSqlHash(cleanedSql);

            return needCalculateHash;
        }

        private string GetSqlHash(string cleanedSql)
        {
            using var cp = MD5.Create();
            var src = Encoding.UTF8.GetBytes(cleanedSql);
            var res = cp.ComputeHash(src);

            return BitConverter.ToString(res).Replace("-", "");
        }

        private bool TrySetFirstContextLineProperty(TechLogItem item)
        {
            if (NeedAdditionalProperty(AdditionalProperty.FirstContextLine) && item.TryGetPropertyValue("Context", out var context))
            {
                item["FirstContextLine"] = GetFirstContextLine(context);

                return true;
            }

            return false;
        }

        private string GetFirstContextLine(string context)
        {
            var index = context.IndexOf("\n");

            if (index > 0)
                return context[0..index];
            else
                return context;
        }

        private bool TrySetLastContextLineProperty(TechLogItem item)
        {
            if (NeedAdditionalProperty(AdditionalProperty.LastContextLine) && item.TryGetPropertyValue("Context", out var context))
            {
                item["LastContextLine"] = GetLastContextLine(context);

                return true;
            }

            return false;
        }

        private string GetLastContextLine(string context)
        {
            var index = context.LastIndexOf("\t");

            if (index > 0)
                return context[(index + 1)..];
            else
                return context;
        }

        private void InitializeStream()
        {
            if (_fileStream == null)
            {
                _fileStream = new FileStream(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                _streamReader = new StreamReader(_fileStream);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                }

                _streamReader?.Dispose();

                disposedValue = true;
            }
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