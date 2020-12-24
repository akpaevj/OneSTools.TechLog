using OneSTools.TechLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OneSTools.TechLogTestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Arrange
            var folderReaderSettings = new TechLogReaderSettings()
            {
                LogFolder = @"C:\Users\akpaev.e.ENTERPRISE\Desktop\TechLog",
                //Properties = new List<string>() { "Sql", "Context" },
                //AdditionalProperty = AdditionalProperty.FirstContextLine,
                BatchSize = 1000,
                BatchFactor = 2,
                LiveMode = false
            };

            using var reader = new TechLogReader(folderReaderSettings);

            var cts = new CancellationTokenSource();

            var stopwatch = Stopwatch.StartNew();

            int count = 0;

            // Act
            await reader.ReadAsync(batch =>
            {
                count += batch.Length;

            }, cts.Token);

            stopwatch.Stop();

            Console.WriteLine($"Read {count} items for {stopwatch.ElapsedMilliseconds} ms.");
            Console.ReadKey();

            // Assign
        }
    }
}
