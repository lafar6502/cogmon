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

        protected override void Run()
        {
            //if (VariableRegex == null || VariableRegex.Length == 0) throw new Exception("Performance counter names should be passed in VariableRegex");
            //if (Variables != null && Variables.Length > 0 && Variables.Length != VariableRegex.Length) throw new Exception("Number of Variables does not match the length of VariableRegex");
            DataRecord dr = new DataRecord { Series = this.DataSeries };

            if (Variables == null || Variables.Length == 0)
            {
                if (string.IsNullOrEmpty(ScriptName)) throw new Exception("ScriptName parameter should contain perf counter Id if you are not using Variables");
                var pv = Counters.GetPerfCounterValuesAndReset(this.ScriptName);
                dr.Data = new double[] { pv.Count, pv.Sum, pv.Min, pv.Max, pv.Median, pv.Perc90, pv.Perc95, pv.Perc98, pv.Avg, pv.Freq };
            }
            else
            {
                if (VariableRegex == null || VariableRegex.Length == 0)
                {
                    string pcid = this.ScriptName;
                    var pv = Counters.GetPerfCounterValuesAndReset(pcid);
                    dr.DataMap = new Dictionary<string, double>();
                    for (int i = 0; i < Variables.Length; i++)
                    {
                        var pi = pv.GetType().GetProperty(Variables[i]);
                        if (pi == null) throw new Exception("Invalid variable: " + Variables[i]);
                        dr.DataMap[Variables[i]] = Convert.ToDouble(pi.GetValue(pv, null));
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
                        if (!d.TryGetValue(cid, out pv))
                        {
                            pv = Counters.GetPerfCounterValuesAndReset(cid);
                            d[cid] = pv;
                        }
                        string cv = idx < 0 ? vn : vn.Substring(idx + 1);
                        var pi = typeof(PerfCounterStats).GetProperty(cv);
                        if (pi == null) throw new Exception("Invalid perf counter statistic: " + vn);
                        dr.DataMap[Variables[i]] = Convert.ToDouble(pi.GetValue(pv, null));
                    }
                }
            }

            
            UpdateDataSource(dr);
        }
    }
}
