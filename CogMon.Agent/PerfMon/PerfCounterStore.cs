using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace CogMon.Agent.PerfMon
{
    /// <summary>
    /// In-memory store for perf counters
    /// </summary>
    public class PerfCounterStore
    {
        private ConcurrentDictionary<string, PerfCounter> _counters = new ConcurrentDictionary<string, PerfCounter>();

        public void UpdateCounter(string id, string clientAddress, int val)
        {
            string key = string.IsNullOrEmpty(clientAddress) ? id : string.Format("{0}/{1}", id, clientAddress);
            var pc = GetCachedCounter(key);
            pc.Update(val);
        }

        private PerfCounter GetCachedCounter(string key)
        {
            return _counters.GetOrAdd(key, x => new PerfCounter { Id = x });
        }
    }
}
