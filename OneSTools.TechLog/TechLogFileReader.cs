using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO.MemoryMappedFiles;

namespace OneSTools.TechLog
{
    public class TechLogFileReader : IDisposable
    {
        private readonly string _fileDateTime;
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

        public TechLogFileReader(string logPath, AdditionalProperty additionalProperty)
        {
            LogPath = logPath;
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

            return ParseRawItem(rawItem.Trim(), cancellationToken);
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
                else
                {
                    _data.AppendLine(line);

                    if (NeedAdditionalProperty(AdditionalProperty.EndPosition))
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

                return result.AsSpan();
            } 
        }

        private TechLogItem ParseRawItem(ReadOnlySpan<char> rawItem, CancellationToken cancellationToken = default)
        {
            var item = new TechLogItem();

            var position = 0;

            var dtd = ReadNextPropertyWithoutName(rawItem, ',');
            var dtdLength = dtd.Length;
            var dtEndIndex = dtd.LastIndexOf('-');
            item.TrySetPropertyValue("DateTime", dtd[0..dtEndIndex].ToString());
            position = dtEndIndex + 1;

            var duration = ReadNextPropertyWithoutName(rawItem[position..], ',');
            position += duration.Length + 1;
            item.TrySetPropertyValue("Duration", duration.ToString());

            var eventName = ReadNextPropertyWithoutName(rawItem[position..], ',');
            position += eventName.Length + 1;
            item.TrySetPropertyValue("Event", eventName.ToString());

            var level = ReadNextPropertyWithoutName(rawItem[position..], ',');
            position += level.Length + 1;
            item.TrySetPropertyValue("Level", level.ToString());

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
                propertyValue = propertyValue.Trim(new char[] { '\'', '"' }).Trim();

                if (!item.TrySetPropertyValue(propertyName.ToString(), propertyValue.ToString()))
                    item.TrySetPropertyValue(GetPropertyName(item, propertyName, 0), propertyValue.ToString());

                if (position >= rawItem.Length)
                    break;
            }

            SetAdditionalProperties(item);

            return item;
        }

        private ReadOnlySpan<char> ReadPropertyName(ReadOnlySpan<char> strData)
        {
            var endIndex = strData.IndexOf('=');

            if (endIndex == -1)
                return "";

            return strData[..endIndex];
        }

        private ReadOnlySpan<char> ReadPropertyValue(ReadOnlySpan<char> strData)
        {
            var nextChar = strData[0];

            int endIndex = nextChar switch
            {
                '\'' => strData[1..].IndexOf('\'') + 2,
                '"' => strData[1..].IndexOf('"') + 2,
                ',' => 0,
                _ => strData.IndexOf(','),
            };

            if (endIndex == -1)
                endIndex = strData.Length;
            else if (endIndex == 0)
                return "";

            return strData[..endIndex];
        }

        private bool NeedAdditionalProperty(AdditionalProperty additionalProperty)
            => (_additionalProperty & additionalProperty) == additionalProperty;

        private string GetPropertyName(TechLogItem item, ReadOnlySpan<char> name, int number = 0)
        {
            var currentName = $"{name.ToString()}{number}";

            if (!item.HasProperty(currentName))
                return currentName;
            else
                return GetPropertyName(item, name, number + 1);
        }

        private ReadOnlySpan<char> ReadNextPropertyWithoutName(ReadOnlySpan<char> strData, char delimiter = ',')
        {
            var endPosition = strData.IndexOf(delimiter);
            var data = strData[..endPosition];

            return data;
        }

        private (string Name, string Value) ReadNextProperty(ReadOnlySpan<char> strData, ref int startPosition)
        {
            var equalPosition = strData[startPosition..].IndexOf('=');
            if (equalPosition < 0)
                return ("", "");

            var name = strData[startPosition..equalPosition];
            startPosition = equalPosition + 1;

            if (startPosition == strData.Length)
                return (name.ToString(), "");

            var value = GetPropertyValue(strData, ref startPosition);

            return (name.ToString(), value.ToString());
        }

        private ReadOnlySpan<char> GetPropertyValue(ReadOnlySpan<char> strData, ref int startPosition)
        {
            var nextChar = strData[startPosition];

            int endPosition;
            switch (nextChar)
            {
                case '\'':
                    endPosition = strData[(startPosition + 1)..].IndexOf('\'');
                    break;
                case ',':
                    startPosition++;
                    return "";
                case '"':
                    endPosition = strData[(startPosition + 1)..].IndexOf('"');
                    break;
                default:
                    endPosition = strData[startPosition..].IndexOf(',');
                    break;
            }

            if (endPosition < 0)
                endPosition = strData.Length;

            var value = strData[startPosition..endPosition];
            startPosition = endPosition + 1 + startPosition;

            return value.Trim(new char[] { '\'', '"' }).Trim();
        }

        private string ReadPropertyValue(ReadOnlySpan<char> strData, string name)
        {
            var index = strData.IndexOf($",{name}=");

            if (index >= 0)
            {
                int pos = index + name.Length + 2;

                return GetPropertyValue(strData, ref pos).ToString();
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
            if (NeedAdditionalProperty(AdditionalProperty.CleanSql) && item.TryGetPropertyValue("Sql", out var sql))
            {
                item["CleanSql"] = ClearSql(sql).ToString();

                return true;
            }

            return false;
        }

        private ReadOnlySpan<char> ClearSql(ReadOnlySpan<char> data)
        {
            // Remove parameters
            int startIndex = data.IndexOf("sp_executesql", StringComparison.OrdinalIgnoreCase);

            if (startIndex < 0)
                startIndex = 0;
            else
                startIndex += 16;

            int e1 = data.IndexOf("', N'@P", StringComparison.OrdinalIgnoreCase);
            if (e1 < 0)
                e1 = data.Length;

            var e2 = data.IndexOf("p_0:", StringComparison.OrdinalIgnoreCase);
            if (e2 < 0)
                e2 = data.Length;

            var endIndex = Math.Min(e1, e2);

            // Remove temp table names, parameters and guids
            var result = Regex.Replace(data[startIndex..endIndex].ToString(), @"(#tt\d+|@P\d+|\d{8}-\d{4}-\d{4}-\d{4}-\d{12})", "{RD}", RegexOptions.ExplicitCapture);

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

        private string GetSqlHash(ReadOnlySpan<char> cleanedSql)
        {
            using var cp = MD5.Create();
            var src = Encoding.UTF8.GetBytes(cleanedSql.ToString());
            var res = cp.ComputeHash(src);

            return BitConverter.ToString(res).Replace("-", "");
        }

        private bool TrySetFirstContextLineProperty(TechLogItem item)
        {
            if (NeedAdditionalProperty(AdditionalProperty.FirstContextLine) && item.TryGetPropertyValue("Context", out var context))
            {
                item["FirstContextLine"] = GetFirstContextLine(context).ToString();

                return true;
            }

            return false;
        }

        private ReadOnlySpan<char> GetFirstContextLine(ReadOnlySpan<char> context)
        {
            var index = context.IndexOf('\n');

            if (index > 0)
                return context[0..index].Trim();
            else
                return context;
        }

        private bool TrySetLastContextLineProperty(TechLogItem item)
        {
            if (NeedAdditionalProperty(AdditionalProperty.LastContextLine) && item.TryGetPropertyValue("Context", out var context))
            {
                item["LastContextLine"] = GetLastContextLine(context).ToString();

                return true;
            }

            return false;
        }

        private ReadOnlySpan<char> GetLastContextLine(ReadOnlySpan<char> context)
        {
            var index = context.LastIndexOf('\t');

            if (index > 0)
                return context[(index + 1)..].Trim();
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