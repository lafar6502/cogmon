using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    /// <summary>
    /// Persistent performance counter
    /// Used for calculating online statistics
    /// of irregular events
    /// </summary>
    public class PerfEventAggregator
    {
        public string Id { get; set; }
        public List<int> Data { get; set; }
        public DateTime FrameStart { get; set; }
        public DateTime LastUpdate { get; set; }

        public PerfCounterStats CalculateCurrentStats()
        {
            var pc = new PerfCounterStats();
            pc.Id = this.Id;
            pc.StartTime = this.FrameStart;
            pc.EndTime = DateTime.Now;
            pc.Count = Data.Count;
            if (Data.Count == 0) return pc;
            Data.Sort();
            pc.Min = Data[0];
            pc.Max = Data[Data.Count - 1];
            pc.Median = GetPercentile(Data, 50);
            pc.Perc95 = GetPercentile(Data, 95);
            pc.Perc99 = GetPercentile(Data, 99);
            pc.Perc90 = GetPercentile(Data, 90);
            pc.Sum = Data.Sum();
            return pc;
        }

        private int GetPercentile(List<int> data, int perc)
        {
            double pos = ((double) data.Count - 1) * perc / 100.0;
            return data[(int)Math.Round(pos)];
        }
    }

    /// <summary>
    /// Performance counter values at a point in time
    /// </summary>
    public class PerfCounterStats
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Count { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Sum { get; set; }
        public int Median { get; set; }
        public int Perc99 { get; set; }
        public int Perc95 { get; set; }
        public int Perc90 { get; set; }
    }
}
