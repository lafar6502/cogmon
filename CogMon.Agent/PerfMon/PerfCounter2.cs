using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CogMon.Lib.DataSeries;
using InfluxData.Net.Common.Helpers;

namespace CogMon.Agent.PerfMon
{
    
    /// <summary>
    /// Single performance counter instance.
    /// </summary>
    public class PerfCounter2 : PerfCounter
    {
        
        struct VRecord
        {
            public int Time { get; set; }
            public int Value { get; set; }
        }

        private VRecord[] _data = null;
        private int _head = -1;

        public override int MaxUpdates
        {
            get => _data.Length;
            set
            {
                _head = -1;
                _data = new VRecord[value];
            }
        }

        public PerfCounter2(int capacity)
        {
            _data = new VRecord[capacity];
        }
        public PerfCounter2()
        {
            _data = new VRecord[500];
        }

        private static readonly DateTime StartTime = new DateTime(2010, 1, 1);

        private int GetTimestamp(DateTime dt)
        {
            return (int) (dt - StartTime).TotalSeconds;
        }

        private static int mod(int x, int len)
        {
            var m = x % len;
            if (m < 0) m += len;
            return m;
        }

        public override void Update(int val)
        {
            var nhd = Interlocked.Increment(ref _head);
            nhd = mod(nhd, _data.Length);
            _data[nhd].Value = val;
            _data[nhd].Time = GetTimestamp(DateTime.Now);
        }

        private List<VRecord> GetData(bool reset)
        {
            var hd = _head;
            var buf = _data;

            if (reset)
            {
                _data = new VRecord[buf.Length];
                _head = -1;
            }
            var ret = new List<VRecord>(buf.Length);

            var cnt = 0;
            var cutoff = GetTimestamp(DateTime.Now.AddSeconds(-MaxSampleAgeSec)); //skip everything before this tstamp
            var t0 = buf[mod(hd, buf.Length)];
            while(cnt < buf.Length)
            {
                var idx = mod(hd - cnt, buf.Length);
                
                var vr = buf[idx];
                cnt++;
                if (vr.Time < cutoff || vr.Time > t0.Time)
                {
                    break;
                }
                ret.Add(vr);
            }
            return ret;
        }

        int CompareValues(VRecord a, VRecord b)
        {
            if (a.Value < b.Value)
                return -1;
            else if (a.Value > b.Value)
                return 1;
            else return 0;
        }
        public override PerfCounterStats GetCurrentValue(bool reset)
        {
            var buf = GetData(reset);
             ;
            var ret = new PerfCounterStats
            {
                Id = this.Id,
                Count = 0
            };
            if (buf.Count == 0) return ret;
            //we go backwards
            var bf = buf[0];
            ret.EndTime = StartTime.AddSeconds(bf.Time);
            foreach(var r in buf)
            {
                if (r.Time == 0 || r.Time > bf.Time) throw new Exception("not possible!");
                ret.Count++;
                ret.Sum += r.Value;
                ret.StartTime = StartTime.AddSeconds(r.Time);
            }
            
            ret.Avg = ret.Sum / ret.Count;
            
            buf.Sort(CompareValues);
            ret.Min = buf[0].Value;
            ret.Max = buf[buf.Count - 1].Value;
            ret.Median = buf[buf.Count / 2].Value;
            ret.Perc90 = buf[(int)((buf.Count - 1) * 0.90)].Value;
            ret.Perc95 = buf[(int)((buf.Count - 1) * 0.95)].Value;
            ret.Perc98 = buf[(int)((buf.Count - 1) * 0.98)].Value;
            if (ret.EndTime != ret.StartTime)
            {
                ret.Freq = (double)ret.Count / (ret.EndTime - ret.StartTime).TotalSeconds;
            }
            
            return ret;
        }

    }
}
