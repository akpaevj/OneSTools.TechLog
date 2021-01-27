using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OneSTools.TechLog
{
    public class DeadlockConnectionIntersectionsPair
    {
        public long Waiter { get; }
        public long Locker { get; }
        public string Region { get; }
        public string BlockingMode { get; }
        public List<KeyValuePair<string, string>> FieldValues { get; } = new List<KeyValuePair<string, string>>();

        public DeadlockConnectionIntersectionsPair(string data)
        {
            Waiter = long.Parse(Regex.Match(data, @"^\d+", RegexOptions.ExplicitCapture).Value);
            Locker = long.Parse(Regex.Match(data, @"(?<=^\d+) \d+", RegexOptions.ExplicitCapture).Value);
            Region = Regex.Match(data, @"(?<=^\d+ \d+ )\w+\.\w+", RegexOptions.ExplicitCapture).Value;
            BlockingMode = Regex.Match(data, @"(?<=^\d+ \d+ \w+\.\w+ )\w+", RegexOptions.ExplicitCapture).Value;

            var fields = Regex.Matches(data, @"\w+=.*?(?=( \w+=|$))", RegexOptions.ExplicitCapture);

            foreach (Match fieldMatch in fields)
            {
                var fieldData = fieldMatch.Value;

                var splitIndex = fieldData.IndexOf('=');
                var field = fieldData[..splitIndex];
                var value = fieldData[(splitIndex + 1)..];

                FieldValues.Add(new KeyValuePair<string, string>(field, value.Trim('"')));
            }
        }
    }
}