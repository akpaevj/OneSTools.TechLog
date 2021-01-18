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
using System.Linq;
using System.Collections.ObjectModel;

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

        public string FilePath  { get; private set; } = "";
        public string FolderName { get; private set; } = "";
        public string FileName { get; private set; } = "";
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

        public TechLogFileReader(string filePath, AdditionalProperty additionalProperty)
        {
            FilePath = filePath;
            FolderName = Directory.GetParent(FilePath).Name;
            FileName = Path.GetFileName(FilePath);

            _additionalProperty = additionalProperty;

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

            try
            {
                var parsed = ParseRawItem(rawItem.Trim(), cancellationToken);

                return parsed;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                    if (line.Length > 0)
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

                return result.AsSpan();
            } 
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

            var position = 0;

            var dtd = ReadNextPropertyWithoutName(rawItem, ',');
            var dtdLength = dtd.Length;
            var dtEndIndex = dtd.LastIndexOf('-');
            item.DateTime = DateTime.Parse(dtd[0..dtEndIndex]);
            position = dtEndIndex + 1;

            var duration = ReadNextPropertyWithoutName(rawItem[position..], ',');
            position += duration.Length + 1;
            item.Duration = long.Parse(duration);

            var eventName = ReadNextPropertyWithoutName(rawItem[position..], ',');
            position += eventName.Length + 1;
            item.EventName = eventName.ToString();

            var level = ReadNextPropertyWithoutName(rawItem[position..], ',');
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
                propertyValue = propertyValue.Trim(new char[] { '\'', '"' }).Trim();

                if (!item.AllProperties.TryAdd(propertyName.ToString(), propertyValue.ToString()))
                    item.AllProperties.TryAdd(GetPropertyName(properties, propertyName, 0), propertyValue.ToString());

                if (position >= rawItem.Length)
                    break;
            }

            SetAdditionalProperties(item.AllProperties);

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

        private bool NeedAdditionalProperty(AdditionalProperty additionalProperty)
            => (_additionalProperty & additionalProperty) == additionalProperty;

        private string GetPropertyName(Dictionary<string, string> properties, ReadOnlySpan<char> name, int number = 0)
        {
            var currentName = $"{name.ToString()}{number}";

            if (!properties.ContainsKey(currentName))
                return currentName;
            else
                return GetPropertyName(properties, name, number + 1);
        }

        private ReadOnlySpan<char> ReadNextPropertyWithoutName(ReadOnlySpan<char> strData, char delimiter = ',')
        {
            var endPosition = strData.IndexOf(delimiter);
            var data = strData[..endPosition];

            return data;
        }

        private void SetAdditionalProperties(Dictionary<string, string> properties)
        {
            TrySetCleanSqlProperty(properties);
            TrySetSqlHashProperty(properties);
            TrySetFirstContextLineProperty(properties);
            TrySetLastContextLineProperty(properties);
        }

        private bool TrySetCleanSqlProperty(Dictionary<string, string> properties)
        {
            if (NeedAdditionalProperty(AdditionalProperty.CleanSql) && properties.TryGetValue("Sql", out var sql))
            {
                properties.Add("CleanSql", ClearSql(sql).ToString());

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

        private bool TrySetSqlHashProperty(Dictionary<string, string> properties)
        {
            bool needCalculateHash = NeedAdditionalProperty(AdditionalProperty.SqlHash);

            if (!properties.ContainsKey("CleanSql"))
                needCalculateHash = TrySetCleanSqlProperty(properties);

            if (needCalculateHash && properties.TryGetValue("CleanSql", out var cleanedSql))
                properties.Add("SqlHash", GetSqlHash(cleanedSql));

            return needCalculateHash;
        }

        private string GetSqlHash(ReadOnlySpan<char> cleanedSql)
        {
            using var cp = MD5.Create();
            var src = Encoding.UTF8.GetBytes(cleanedSql.ToString());
            var res = cp.ComputeHash(src);

            return BitConverter.ToString(res).Replace("-", "");
        }

        private bool TrySetFirstContextLineProperty(Dictionary<string, string> properties)
        {
            if (NeedAdditionalProperty(AdditionalProperty.FirstContextLine) && properties.TryGetValue("Context", out var context))
            {
                properties.Add("FirstContextLine", GetFirstContextLine(context).ToString());

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

        private bool TrySetLastContextLineProperty(Dictionary<string, string> properties)
        {
            if (NeedAdditionalProperty(AdditionalProperty.LastContextLine) && properties.TryGetValue("Context", out var context))
            {
                properties.Add("LastContextLine", GetLastContextLine(context).ToString());

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
                _fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
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