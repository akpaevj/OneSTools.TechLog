using System;
using System.Collections.Generic;
using System.Text;

namespace OneSTools.TechLog
{
    public class TechLogItem
    {
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

        public string this[string property]
        {
            get => _properties[property];
            set => _properties[property] = value;
        }

        public long EndPosition { get; set; }

        public IEnumerable<string> Properties => _properties.Keys;

        public bool HasProperty(string property)
            => _properties.ContainsKey(property);

        public bool TryGetPropertyValue(string property, out string value)
            => _properties.TryGetValue(property, out value);

        public bool TrySetPropertyValue(string property, string value)
            => _properties.TryAdd(property, value);
    }
}
