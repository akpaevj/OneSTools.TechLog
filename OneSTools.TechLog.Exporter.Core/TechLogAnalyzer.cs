using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace OneSTools.TechLog.Exporter.Core
{
    public class DbmsDeadlockHandlerEventArgs
    {
        private readonly List<TechLogItem> _victims = new List<TechLogItem>();

        public TechLogItem Source { get; private set; }
        public ReadOnlyCollection<TechLogItem> Victims => _victims.AsReadOnly();

        internal DbmsDeadlockHandlerEventArgs(TechLogItem source)
            => Source = source;

        internal void AddVictim(TechLogItem victim)
            => _victims.Add(victim);
    }

    public class TDeadlockHandlerEventArgs
    {

    }

    public class TechLogAnalyzer
    {
        private readonly Dictionary<string, TechLogItem> _deadlockSources = new Dictionary<string, TechLogItem>();
        private readonly Dictionary<string, TechLogItem> _unknownVictims = new Dictionary<string, TechLogItem>();


        public delegate void DbmsDeadlockHandler(object sender, DbmsDeadlockHandlerEventArgs a);
        public event DbmsDeadlockHandler dbmsDeadlock;

        public delegate void TDeadlockHandler(object sender, DbmsDeadlockHandlerEventArgs a);
        public event TDeadlockHandler Deadlock;

        public void HandleItem(TechLogItem item)
        {
            if (item.EventName == "DBMSSQL")
                HandleDbms(item);
            else if (item.EventName == "TDEADLOCK")
                HandleTDeadlock(item);
        }

        private void HandleDbms(TechLogItem item)
        {
            if (item.AllProperties.TryGetValue("lka", out var lka) && lka == "1")
            {
                // check this is a victim of the deadlock
                if (item.AllProperties.TryGetValue("lkp", out var lkp) && lkp == "1")
                {
                    // read the number of the query that blocked this query
                    var lkpid = item.AllProperties["lkpid"];
                    //read the connection number that blocked this query
                    var lksrc = item.AllProperties["lksrc"];

                    // check there is a registered source of the deadblock
                    var deadlockSourceKey = $"{lksrc}|{lkpid}";

                    if (_deadlockSources.ContainsKey(deadlockSourceKey))
                    {
                        // the source of the deadlock is registered
                    }
                    else
                    {
                        // the source of the deadblock is not registered, add this query to "Unknown victims" list
                        _unknownVictims.Add(deadlockSourceKey, item);
                    }
                }
                else
                {
                    // get connection id of the query
                    var connectId = item.AllProperties["t:connectID"];

                    // it looks like this query is a real source of the deadblock, read victims' query numbers
                    var victimQueryNumbers = item.AllProperties["lkaid"];

                    var args = new DbmsDeadlockHandlerEventArgs(item);

                    // try to get victims from "unknown victims" list
                    foreach (var victimNumber in victimQueryNumbers.Split(','))
                    {
                        var key = $"{connectId}|{victimNumber}";

                        if (_unknownVictims.TryGetValue(key, out var victim))
                            args.AddVictim(victim);
                    }
                }
            }
        }
        
        private void HandleTDeadlock(TechLogItem item)
        {

        }
    }
}
