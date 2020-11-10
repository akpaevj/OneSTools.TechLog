using System;
using System.Collections.Generic;

namespace OneSTools.TechLog
{
    public class TechLogItem
    {
        public DateTime DateTime { get; set; }
        public long Duration { get; set; }
        public string Event { get; set; }
        public int Level { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
