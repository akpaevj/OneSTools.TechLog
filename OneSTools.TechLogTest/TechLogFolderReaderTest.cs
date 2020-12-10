using OneSTools.TechLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneSTools.TechLogTest
{
    public class TechLogFolderReaderTest
    {
        [Fact]
        public async Task ReadAsyncTest()
        {
            // Arrange
            var folderReaderSettings = new TechLogReaderSettings()
            {
                LogFolder = @"C:\Users\akpaev.e.ENTERPRISE\Desktop\TechLog",
                //AdditionalProperty = AdditionalProperty.CleanedSql | AdditionalProperty.FirstContextLine | AdditionalProperty.FirstContextLine | AdditionalProperty.LastContextLine,
                BatchSize = 10,
                BatchFactor = 3,
                LiveMode = false
            };

            using var reader = new TechLogReader(folderReaderSettings);

            var cts = new CancellationTokenSource();

            int count = 0;

            // Act
            await reader.ReadAsync(batch => 
            {
                count += batch.Length;

            }, cts.Token);

            // Assign
        }
    }
}
