namespace OneSTools.TechLog
{
    public class TechLogReaderSettings
    {
        public string LogFolder { get; set; } = "";
        public int BatchSize { get; set; } = 1000;
        public int BatchFactor { get; set; } = 2;
        public AdditionalProperty AdditionalProperty { get; set; } = AdditionalProperty.None;
        public bool LiveMode { get; set; } = false;
        public int ReadingTimeout { get; set; } = 1;
    }
}
