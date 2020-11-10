using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneSTools.TechLog.Exporter.Core
{
    public interface ITechLogFolderReader : IDisposable
    {
        Task StartAsync(string logFolder, int portion, bool liveMode = false, CancellationToken cancellationToken = default);
    }
}