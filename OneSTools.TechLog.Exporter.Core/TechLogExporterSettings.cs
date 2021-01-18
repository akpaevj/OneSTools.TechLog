namespace OneSTools.TechLog.Exporter.Core
{
    public class TechLogExporterSettings
    {
        public string LogFolder { get; set; } = "";
        public bool LiveMode { get; set; } = true;
        public int BatchSize { get; set; } = 10000;
        public int BatchFactor { get; set; } = 2;
        public int ReadingTimeout { get; set; } = 1;
    }
}
