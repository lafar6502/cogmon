using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using NLog;
using CogMon.Lib.DataSeries;

namespace CogMon.Agent.PerfMon
{
    /// <summary>
    /// In-memory store for perf counters
    /// </summary>
    public class PerfCounterStore
    {
        private ConcurrentDictionary<string, PerfCounter> _counters = new ConcurrentDictionary<string, PerfCounter>();
        private Logger log = LogManager.GetCurrentClassLogger();

        public int MaxSamples { get; set; } = 500;
        /// <summary>
        /// 5 minute agg interval by default
        /// TODO this is not working!
        /// </summary>
        public int DataWindowSec { get; set; } = 300;

        public void UpdateCounter(string id, string clientAddress, int val)
        {
            string key = string.IsNullOrEmpty(clientAddress) ? id : string.Format("{0}/{1}", id, clientAddress);
            var pc = GetCachedCounter(id);
            pc.Update(val);
            if (log.IsDebugEnabled)
            {
                var cv = Newtonsoft.Json.JsonConvert.SerializeObject(pc.GetCurrentValue(false));
                log.Debug("updated {0}: {1}: Values: {2}", id, val, cv);
            }
        }

        private PerfCounter GetCachedCounter(string key)
        {
            return _counters.GetOrAdd(key, x => new PerfCounter2(MaxSamples) { Id = x, MaxSampleAgeSec = DataWindowSec });
        }

        public PerfCounter TryGetCounter(string key)
        {
            PerfCounter pc;
            return _counters.TryGetValue(key, out pc) ? pc : null;
        }

        public PerfCounterStats GetPerfCounterValuesAndReset(string id)
        {
            var pc = GetCachedCounter(id);
            return pc.GetCurrentValue(true);
        }

        public PerfCounterStats GetPerfCounterValues(string id, bool reset)
        {
            var pc = GetCachedCounter(id);
            return pc.GetCurrentValue(reset);
        }

        public IList<string> GetPerfCounterNames()
        {
            return new List<string>(_counters.Keys);
        }
    }
}
