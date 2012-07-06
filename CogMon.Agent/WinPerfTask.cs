using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Data.Common;
using System.Configuration;
using CogMon.Lib;

namespace CogMon.Agent
{
    /// <summary>
    /// SqlSelect task runs an sql select query.
    /// Every row returned by the query is treated as a data record
    /// Column names should match data field names in the rrd file.
    /// If there is a column named 'timestamp' it should contain a timestamp for the record.
    /// Otherwise 'now' is assumed.
    /// </summary>
    public class WinPerfTask : JobBase
    {
        public WinPerf PerfCounters { get; set; }

        protected override void Run()
        {
            if (VariableRegex == null || VariableRegex.Length == 0) throw new Exception("Performance counter names should be passed in VariableRegex");
            if (Variables != null && Variables.Length > 0 && Variables.Length != VariableRegex.Length) throw new Exception("Number of Variables does not match the length of VariableRegex");
            DataRecord dr = new DataRecord { Series = this.DataSeries };
            if (Variables != null && Variables.Length > 0)
                dr.DataMap = new Dictionary<string, double>();
            else
                dr.Data = new double[this.VariableRegex.Length];

            for (int i = 0; i < VariableRegex.Length; i++)
            {
                var val = PerfCounters.GetValue(VariableRegex[i]);
                log.Debug("PC[{0}]={1}", VariableRegex[i], val);
                if (dr.Data != null) dr.Data[i] = val;
                if (dr.DataMap != null) dr.DataMap[Variables[i]] = val;
            }
            UpdateDataSource(dr);
        }
    }
}
