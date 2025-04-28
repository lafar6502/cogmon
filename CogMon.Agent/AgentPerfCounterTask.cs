using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Data.Common;
using System.Configuration;
using CogMon.Lib;
using CogMon.Lib.DataSeries;

namespace CogMon.Agent
{
    /// <summary>
    /// 
    /// </summary>
    public class AgentPerfCounterTask : JobBase
    {

        public PerfMon.PerfCounterStore Counters { get; set; }
        
        /// <summary>
        /// reset by default
        /// </summary>
        /// <param name="pcid"></param>
        /// <returns></returns>
        private PerfCounterStats GetPerfCounterValues(string pcid)
        {
            var reset = false;
            if (pcid.StartsWith("+"))
            {
                reset = false;
                pcid = pcid.Substring(1);
            }
            else if (pcid.StartsWith("!"))
            {
                reset = true;
                pcid = pcid.Substring(1);
            }
            return Counters.GetPerfCounterValues(pcid, reset);
        }

        protected override void Run()
        {
            //if (VariableRegex == null || VariableRegex.Length == 0) throw new Exception("Performance counter names should be passed in VariableRegex");
            //if (Variables != null && Variables.Length > 0 && Variables.Length != VariableRegex.Length) throw new Exception("Number of Variables does not match the length of VariableRegex");
            DataRecord dr = new DataRecord { Series = this.DataSeries };

            if (Variables == null || Variables.Length == 0)
            {
                if (string.IsNullOrEmpty(ScriptName)) throw new Exception("ScriptName parameter should contain perf counter Id if you are not using Variables");
                var pv = GetPerfCounterValues(this.ScriptName);
                dr.DataMap = new Dictionary<string, double>();
                dr.DataMap[this.ScriptName + "_Count"] = pv.Count;
                dr.DataMap[this.ScriptName + "_Avg"] = pv.Avg;
                dr.DataMap[this.ScriptName + "_Freq"] = pv.Freq;
                dr.DataMap[this.ScriptName + "_Sum"] = pv.Sum;

                if (pv.Min.HasValue) dr.DataMap[this.ScriptName + "_Min"] = pv.Min.Value;
                if (pv.Max.HasValue) dr.DataMap[this.ScriptName + "_Max"] = pv.Max.Value;
                if (pv.Median.HasValue) dr.DataMap[this.ScriptName + "_Median"] = pv.Median.Value;
                if (pv.Perc90.HasValue) dr.DataMap[this.ScriptName + "_Perc90"] = pv.Perc90.Value;
                if (pv.Perc95.HasValue) dr.DataMap[this.ScriptName + "_Perc95"] = pv.Perc95.Value;
                if (pv.Perc98.HasValue) dr.DataMap[this.ScriptName + "_Perc98"] = pv.Perc98.Value;    
            }
            else
            {
                if (VariableRegex == null || VariableRegex.Length == 0)
                {
                    string pcid = this.ScriptName;
                    var pv = GetPerfCounterValues(this.ScriptName);
                    dr.DataMap = new Dictionary<string, double>();
                    for (int i = 0; i < Variables.Length; i++)
                    {
                        var pi = pv.GetType().GetProperty(Variables[i]);
                        if (pi == null)
                        {
                            log.Warn("Invalid variable: {0}/{1} in job {2}", Variables[i], i, this.Id);
                            continue;
                        }
                        var val = pi.GetValue(pv, null);
                        if (val == null) continue;
                        dr.DataMap[Variables[i]] = Convert.ToDouble(val);
                    }
                }
                else if (VariableRegex.Length != Variables.Length)
                {
                    throw new Exception("VariableRegex length invalid");
                }
                else
                {
                    Dictionary<string, PerfCounterStats> d = new Dictionary<string, PerfCounterStats>();
                    dr.DataMap = new Dictionary<string, double>();
                    for (int i = 0; i < Variables.Length; i++)
                    {
                        var vn = VariableRegex[i];
                        int idx = vn.IndexOf('/');
                        string cid = idx < 0 ? this.ScriptName : vn.Substring(0, idx);
                        PerfCounterStats pv;
                        if (cid == null) throw new Exception("Counter name missing - either put it in ScriptName field or in the VariableRegex like 'CounterName/Field'");
                        if (!d.TryGetValue(cid, out pv))
                        {
                            pv = GetPerfCounterValues(cid);
                            d[cid] = pv;
                        }
                        string cv = idx < 0 ? vn : vn.Substring(idx + 1);
                        var pi = typeof(PerfCounterStats).GetProperty(cv);
                        if (pi == null)
                        {
                            log.Warn("Invalid variableregex: {0}/{1} in job {2}", vn, i, this.Id);
                            continue;
                        }
                        var val = pi.GetValue(pv, null);
                        if (val != null)
                        {
                            dr.DataMap[Variables[i]] = Convert.ToDouble(val);
                        }
                        
                    }
                }
            }

            
            UpdateDataSource(dr);
        }
    }
}
