using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Text.RegularExpressions;

namespace OneSTechLog
{
    /// <summary>
    /// Represents methods for the parsing of the 1C technological log
    /// </summary>
    public class TechLogParser
    {
        private string folder;
        private Action<Dictionary<string, string>> eventHandler;
        private ExecutionDataflowBlockOptions readBlockOptions;
        private ExecutionDataflowBlockOptions parseBlockOptions;
        private ExecutionDataflowBlockOptions eventHandlerBlockOptions;

        /// <summary>
        /// Folder of the technological log data
        /// </summary>
        public string Folder { get => folder; private set => folder = value; }
        /// <summary>
        /// Action for the event handling
        /// </summary>
        public Action<Dictionary<string, string>> EventHandler { get => eventHandler; set => eventHandler = value; }

        public TechLogParser(
            string folder, 
            Action<Dictionary<string, string>> eventHandler,
            ExecutionDataflowBlockOptions readBlockOptions = null,
            ExecutionDataflowBlockOptions parseBlockOptions = null,
            ExecutionDataflowBlockOptions eventHandlerBlockOptions = null)
        {
            Folder = folder;
            EventHandler = eventHandler;

            if (readBlockOptions == null)
            {
                this.readBlockOptions = new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
            }
            else
            {
                this.readBlockOptions = readBlockOptions;
            }

            if (parseBlockOptions == null)
            {
                this.parseBlockOptions = new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = 10000
                };
            }
            else
            {
                this.parseBlockOptions = parseBlockOptions;
            }

            if (eventHandlerBlockOptions == null)
            {
                this.eventHandlerBlockOptions = new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = 10000
                };
            }
            else
            {
                this.eventHandlerBlockOptions = eventHandlerBlockOptions;
            }
        }

        /// <summary>
        /// Starts parsing of the technological log data
        /// </summary>
        /// <returns></returns>
        public async Task Parse()
        {
            var eventHandlerBlock = new ActionBlock<Dictionary<string, string>>(EventHandler, eventHandlerBlockOptions);
            var parseEventBlock = new TransformBlock<string, Dictionary<string, string>>(ParseEventData, parseBlockOptions);
            var readFileBlock = new ActionBlock<string>(async (filePath) => await ReadFile(filePath, parseEventBlock), readBlockOptions);

            parseEventBlock.LinkTo(eventHandlerBlock);

            var files = GetTechLogFiles();

            foreach (var filePath in files)
            {
                await SendDataToNextBlock(filePath, readFileBlock);
            }

            var readBlockTask = readFileBlock.Completion.ContinueWith(c => parseEventBlock.Complete());
            var parseEventBlockTask = parseEventBlock.Completion.ContinueWith(c => eventHandlerBlock.Complete());

            readFileBlock.Complete();

            await Task.WhenAll(readBlockTask, parseEventBlockTask, eventHandlerBlock.Completion);
        }

        private async Task ReadFile(string filePath, ITargetBlock<string> nextBlock)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var reader = new StreamReader(stream))
            {
                string fileDateTime = GetFileDateTime(filePath);

                StringBuilder currentEvent = new StringBuilder();
                bool firstEvent = true;

                do
                {
                    var currentLine = reader.ReadLine();

                    if (Regex.IsMatch(currentLine, @"^\d\d:\d\d\.\d+", RegexOptions.Compiled))
                    {
                        if (firstEvent)
                        {
                            firstEvent = false;
                        }
                        else
                        {
                            await SendDataToNextBlock(fileDateTime + ":" + currentEvent.ToString(), nextBlock);

                            currentEvent.Clear();
                        }

                        currentEvent.Append(currentLine);
                    }
                    else
                    {
                        currentEvent.Append(currentLine);
                    }
                }
                while (!reader.EndOfStream);

                await SendDataToNextBlock(fileDateTime + ":" + currentEvent.ToString(), nextBlock);
            }
        }
        private Dictionary<string, string> ParseEventData(string eventData)
        {
            var properties = new Dictionary<string, string>
            {
                ["EventName"] = Regex.Match(eventData, @"(?<=^\d+-\d+-\d+\s\d+:\d+:\d+\.\d+-\d+,)\w+?(?=,)", RegexOptions.IgnoreCase | RegexOptions.Compiled).ToString(),
                ["DateTime"] = Regex.Match(eventData, @"^\d+-\d+-\d+\s\d+:\d+:\d+\.\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled).ToString(),
                ["Duration"] = Regex.Match(eventData, @"(?<=^\d+-\d+-\d+\s\d+:\d+:\d+\.\d+-)\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled).ToString()
            };

            var props = Regex.Matches(eventData, @"(?<=,)[\w:]+=.*?(?=(,[\w:]+=|$))", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

            foreach (var prop in props)
            {
                var propText = prop.ToString();
                var splInd = propText.IndexOf('=');
                var propName = propText.Substring(0, splInd);
                var propVal = propText.Substring(splInd + 1);
                if (propVal.StartsWith("'") || propVal.StartsWith("\"")) propVal = propVal.Substring(1, propVal.Length - 2);

                properties[propName] = propVal;
            }

            return properties;
        }
        private async Task SendDataToNextBlock<T>(T data, ITargetBlock<T> nextBlock)
        {
            while (!await nextBlock.SendAsync(data)) ;
        }
        private string[] GetTechLogFiles()
        {
            return Directory.GetFiles(Folder, "*.log");
        }
        private string GetFileDateTime(string filePath)
        {
            var info = Path.GetFileNameWithoutExtension(filePath);

            return "20" + info.Substring(0, 2) + "-" + info.Substring(2, 2) + "-" + info.Substring(4, 2) + " " + info.Substring(6, 2);
        }
    }
}
