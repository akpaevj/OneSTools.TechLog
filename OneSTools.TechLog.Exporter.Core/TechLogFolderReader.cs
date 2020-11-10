using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using OneSTools.TechLog;

namespace OneSTools.TechLog.Exporter.Core
{
    public class TechLogFolderReader : IDisposable, ITechLogFolderReader
    {
        private readonly ILogger<TechLogFolderReader> _logger;
        private ITechLogStorage _techLogStorage;
        private string _folder;
        private bool _liveMode;
        private ActionBlock<TechLogItem[]> _writeBlock;
        private BatchBlock<TechLogItem> _batchBlock;
        private TransformBlock<string, TechLogItem> _parseBlock;
        private ActionBlock<string> _readBlock;

        public TechLogFolderReader(ILogger<TechLogFolderReader> logger, ITechLogStorage techLogStorage)
        {
            _logger = logger;
            _techLogStorage = techLogStorage;
        }

        public async Task StartAsync(string folder, int portion, bool liveMode = false, CancellationToken cancellationToken = default)
        {
            _folder = folder;
            _liveMode = liveMode;

            var maxDegree = Environment.ProcessorCount * 10;

            _writeBlock = new ActionBlock<TechLogItem[]>(_techLogStorage.WriteItemsAsync, new ExecutionDataflowBlockOptions() 
            { 
                BoundedCapacity = 3,
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = maxDegree,
            });
            _batchBlock = new BatchBlock<TechLogItem>(portion, new GroupingDataflowBlockOptions() 
            { 
                CancellationToken = cancellationToken,
                BoundedCapacity = portion
            });
            _parseBlock = new TransformBlock<string, TechLogItem>(ParseItemData, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = maxDegree,
                BoundedCapacity = portion / 2,
                CancellationToken = cancellationToken
            });
            _readBlock = new ActionBlock<string>(str => ReadItemsData(str, _parseBlock), new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = maxDegree,
                CancellationToken = cancellationToken
            }); ;

            _parseBlock.LinkTo(_batchBlock);
            _batchBlock.LinkTo(_writeBlock);

            var logFiles = GetLogFiles();

            foreach (var logFile in logFiles)
            {
                await _readBlock.SendAsync(logFile);
            }

            await _writeBlock.Completion;
        }

        private string[] GetLogFiles()
        {
            return Directory.GetFiles(_folder, "*.log", SearchOption.AllDirectories);
        }

        private void ReadItemsData(string logPath, ITargetBlock<string> nextblock)
        {
            var fileName = Path.GetFileNameWithoutExtension(logPath);

            var fileDateTime = "20" +
                fileName.Substring(0, 2) +
                "-" +
                fileName.Substring(2, 2) +
                "-" +
                fileName.Substring(4, 2) +
                " " +
                fileName.Substring(6, 2);

            using var reader = new TechLogReader(logPath);

            while (true)
            {
                var itemData = reader.ReadItemData();

                if (itemData != null)
                    PostData(nextblock, fileDateTime + ":" + itemData);
            }
        }

        private void PostData<T>(ITargetBlock<T> nextblock, T data)
        {
            while (true)
            {
                if (nextblock.Post(data))
                    break;
            }
        }

        private TechLogItem ParseItemData(string itemData)
        {
            return TechLogReader.ParseItemData(itemData);
        }

        public void Dispose()
        {

        }
    }
}
