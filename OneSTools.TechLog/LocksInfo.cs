using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace OneSTools.TechLog
{
    public class LocksInfo
    {
        public ReadOnlyCollection<LocksInfoRegion> Regions { get; }

        public LocksInfo(string locks)
        {
            var regions = new List<LocksInfoRegion>();

            var regionsMatch = Regex.Matches(locks, @"\w+\.\w+.*?(?=(,\w+\.\w+|$))", RegexOptions.ExplicitCapture);

            foreach(Match regionMatch in regionsMatch)
            {
                var region = new LocksInfoRegion(regionMatch.Value);
                regions.Add(region);
            }

            Regions = regions.AsReadOnly();
        }
    }
}