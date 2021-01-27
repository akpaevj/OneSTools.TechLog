using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace OneSTools.TechLog
{
    public class DeadlockConnectionIntersectionsInfo
    {
        public ReadOnlyCollection<DeadlockConnectionIntersectionsPair> Pairs { get; }

        public DeadlockConnectionIntersectionsInfo(string data)
        {
            var pairs = new List<DeadlockConnectionIntersectionsPair>();

            var pairsMatch = Regex.Matches(data, @"\d+ \d+.*?(?=(,\d+ \d+|$))", RegexOptions.ExplicitCapture);

            foreach (Match pairMatch in pairsMatch)
            {
                var pair = new DeadlockConnectionIntersectionsPair(pairMatch.Value);
                pairs.Add(pair);
            }

            Pairs = pairs.AsReadOnly();
        }
    }
}
