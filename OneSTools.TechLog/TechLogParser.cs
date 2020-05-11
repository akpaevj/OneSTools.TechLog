using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Text.RegularExpressions;

namespace OneSTools.TechLog
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

        /// <summary>
        /// Creates a new instance of TechLogParser class
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="eventHandler"></param>
        public TechLogParser(string folder, Action<Dictionary<string, string>> eventHandler)
        {
            Folder = folder;
            EventHandler = eventHandler;

            readBlockOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            parseBlockOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                BoundedCapacity = 10000
            };

            eventHandlerBlockOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                BoundedCapacity = 10000
            };
        }

        /// <summary>
        /// Starts parsing of the technological log data
        /// </summary>
        /// <returns></returns>
        public async Task Parse()
        {
            var eventHandlerBlock = new ActionBlock<Dictionary<string, string>>(EventHandler, eventHandlerBlockOptions);
            var parseEventBlock = new TransformBlock<string, Dictionary<string, string>>(ParseEventData, parseBlockOptions);
            var readFileBlock = new ActionBlock<string>((filePath) => ReadFile(filePath, parseEventBlock), readBlockOptions);

            parseEventBlock.LinkTo(eventHandlerBlock);

            var files = GetTechLogFiles();

            foreach (var filePath in files)
            {
                SendDataToNextBlock(filePath, readFileBlock);
            }

            var readBlockTask = readFileBlock.Completion.ContinueWith(c => parseEventBlock.Complete());
            var parseEventBlockTask = parseEventBlock.Completion.ContinueWith(c => eventHandlerBlock.Complete());

            readFileBlock.Complete();

            await Task.WhenAll(readBlockTask, parseEventBlockTask, eventHandlerBlock.Completion);
        }

        private void ReadFile(string filePath, ITargetBlock<string> nextBlock)
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

                    if (Regex.IsMatch(currentLine, @"^\d\d:\d\d\.", RegexOptions.Compiled))
                    {
                        if (firstEvent)
                        {
                            firstEvent = false;
                        }
                        else
                        {
                            SendDataToNextBlock(fileDateTime + ":" + currentEvent.ToString(), nextBlock);

                            currentEvent.Clear();
                        }

                        currentEvent.AppendLine(currentLine);
                    }
                    else
                    {
                        currentEvent.AppendLine(currentLine);
                    }
                }
                while (!reader.EndOfStream);

                SendDataToNextBlock(fileDateTime + ":" + currentEvent.ToString(), nextBlock);
            }
        }
        private Dictionary<string, string> ParseEventData(string eventData)
        {
            var properties = new Dictionary<string, string>
            {
                ["EventName"] = Regex.Match(eventData, @",.*?,",  RegexOptions.Compiled).ToString().Trim(','),
                ["DateTime"] = Regex.Match(eventData, @"^.*?\.\d+",  RegexOptions.Compiled).ToString(),
                ["Duration"] = Regex.Match(eventData, @"-\d+?,",  RegexOptions.Compiled).ToString().Trim('-', ',')
            };

            var props = Regex.Matches(eventData, @",[\w:]+=.*?(?=(,[\w:]+=|$))", RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.Compiled);

            for (int x = 0; x < props.Count; x++)
            {
                var propText = props[x].ToString();
                var splInd = propText.IndexOf('=');
                var propName = propText.Substring(0, splInd).Trim(',');
                var propVal = propText.Substring(splInd + 1).Trim('\'', '"');

                properties[propName] = propVal;
            }

            return properties;
        }
        private void SendDataToNextBlock<T>(T data, ITargetBlock<T> nextBlock)
        {
            while (!nextBlock.Post(data)) ;
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
