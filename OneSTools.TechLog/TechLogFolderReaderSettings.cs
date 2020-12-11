using System.Collections.Generic;

namespace OneSTools.TechLog
{
    public class TechLogFolderReaderSettings
    {
        public string Folder { get; set; } = "";
        public List<string> Properties { get; set; } = new List<string>();
        public AdditionalProperty AdditionalProperty { get; set; } = AdditionalProperty.None;
        public bool LiveMode { get; set; } = false;
        public int ReadingTimeout { get; set; } = 1;
    }
}
