namespace OneSTools.TechLog
{
    public class TechLogFolderReaderSettings
    {
        public string Folder { get; set; } = "";
        public AdditionalProperty AdditionalProperty { get; set; } = AdditionalProperty.None;
        public bool LiveMode { get; set; } = false;
        public int ReadingTimeout { get; set; } = 1;
    }
}
