using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NLog;

namespace CogMon.Agent
{
    public class PerfCounter
    {
        
    }

    public class WinPerf : IDisposable
    {
        private Dictionary<string, PerformanceCounter> _pcache = new Dictionary<string, PerformanceCounter>();
        private Logger log = LogManager.GetCurrentClassLogger();
        

        private string GetPerfCounterKey(string categoryName, string counterName, string counterInstance, string machineName)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(categoryName)) sb.AppendFormat("category={0};", categoryName);
            if (!string.IsNullOrEmpty(counterName)) sb.AppendFormat("counter={0};", counterName);
            if (!string.IsNullOrEmpty(counterInstance)) sb.AppendFormat("instance={0};", counterInstance);
            if (!string.IsNullOrEmpty(machineName)) sb.AppendFormat("machine={0};", machineName);
            return sb.ToString();
        }

        private bool ParsePerfCounterKey(string pcref, out string categoryName, out string counterName, out string instanceName, out string machineName)
        {
            categoryName = counterName = instanceName = machineName = null;
            string[] elems = pcref.Split(';');
            foreach (string el in elems)
            {
                
                int idx = el.IndexOf('=');
                if (idx < 0) throw new Exception("Invalid element syntax: " + el);
                string n = el.Substring(0, idx);
                string v = el.Substring(idx + 1);
                if (n.Equals("category", StringComparison.InvariantCultureIgnoreCase))
                    categoryName = v;
                else if (n.Equals("counter", StringComparison.InvariantCultureIgnoreCase))
                    counterName = v;
                else if (n.Equals("instance", StringComparison.InvariantCultureIgnoreCase))
                    instanceName = v;
                else if (n.Equals("machine", StringComparison.InvariantCultureIgnoreCase))
                    machineName = v;
                else throw new Exception("Invalid element: " + el);
            }
            if (string.IsNullOrEmpty(counterName)) return false;
            return true;
        }

        private string NormalizePCRef(string pcref)
        {
            string category, machine, counter, instance;
            if (!ParsePerfCounterKey(pcref, out category, out counter, out instance, out machine)) throw new Exception("Invalid perf counter reference: " + pcref);
            return GetPerfCounterKey(category, counter, instance, machine);
        }

        private PerformanceCounter GetPerfCounter(string pcref)
        {
            string category, machine, counter, instance;
            if (!ParsePerfCounterKey(pcref, out category, out counter, out instance, out machine)) throw new Exception("Invalid perf counter reference: " + pcref);
            string k = GetPerfCounterKey(category, counter, instance, machine);
            PerformanceCounter pc = null;
            lock (_pcache)
            {
                if (_pcache.TryGetValue(k, out pc)) return pc;
                pc = new PerformanceCounter();
                if (!string.IsNullOrEmpty(category)) pc.CategoryName = category;
                if (!string.IsNullOrEmpty(counter)) pc.CounterName = counter;
                if (!string.IsNullOrEmpty(instance)) pc.InstanceName = instance;
                if (!string.IsNullOrEmpty(machine)) pc.MachineName = machine;
                pc.ReadOnly = true;
                _pcache[k] = pc;
                return pc;
            }
        }

        public float GetValue(string pcref)
        {
            var pc = this.GetPerfCounter(pcref);
            return pc.NextValue();
        }

        public long GetRawValue(string pcref)
        {
            var pc = GetPerfCounter(pcref);
            return pc.RawValue;
        }

        public void Dispose()
        {
            var d = _pcache;
            _pcache = null;
            if (d != null)
            {
                foreach (PerformanceCounter pc in d.Values)
                {
                    pc.Dispose();
                }
            }
        }
    }
}
