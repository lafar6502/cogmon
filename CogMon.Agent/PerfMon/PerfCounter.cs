using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CogMon.Lib.DataSeries;

namespace CogMon.Agent.PerfMon
{
    
    /// <summary>
    /// Single performance counter instance.
    /// </summary>
    public class PerfCounter
    {

        public string Id { get; set; }
        /// <summary>
        /// Max number of events the counter will hold
        /// before auto-resetting
        /// </summary>
        public int MaxUpdates
        {
            get { return _data.Capacity; }
            set { if (value != _data.Capacity) _data = new ConcurrentCircularBuffer<int>(value); }
        }

        public ConcurrentCircularBuffer<int> _data = new ConcurrentCircularBuffer<int>(500);
        private DateTime _lastReset;
        private DateTime _lastUpdate;
        private int _count;
        private long _sum;
        private int _max;
        private int _min;

        public PerfCounter()
        {
            _lastReset = DateTime.Now;
            _lastUpdate = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void Update(int val)
        {
            _data.Enqueue(val);
            _lastUpdate = DateTime.Now;
            Interlocked.Increment(ref _count);
            Interlocked.Add(ref _sum, val);
            var m = _max;
            if (val > m) Interlocked.CompareExchange(ref _max, val, m);
            m = _min;
            if (val < m) Interlocked.CompareExchange(ref _min, val, m);
        }

        
        /// <summary>
        /// Return current perf counter values and optionally reset the counter
        /// </summary>
        /// <param name="reset"></param>
        /// <returns></returns>
        public PerfCounterStats GetCurrentValue(bool reset)
        {
            return GetValues(reset);
        }

        protected PerfCounterStats GetValues(bool reset)
        {
            PerfCounterStats pv = new PerfCounterStats
            {
                Id = this.Id,
                Sum = Interlocked.Exchange(ref _sum, reset ? 0L : _sum),
                Count = Interlocked.Exchange(ref _count, reset ? 0 : _count),
                Min = Interlocked.Exchange(ref _min, reset ? Int32.MaxValue : _min),
                Max = Interlocked.Exchange(ref _max, reset ? Int32.MinValue : _max),
                StartTime = _lastReset,
                EndTime = DateTime.Now
            };
            var dt = _data;
            if (reset)
            {
                var c = MaxUpdates;
                _lastReset = _lastUpdate = DateTime.Now;
                dt = Interlocked.Exchange(ref _data, new ConcurrentCircularBuffer<int>(c));
            }
            int[] dd = dt.ToArray();
            if (dd.Length > 0)
            {
                Array.Sort(dd);
                pv.Median = dd[dd.Length / 2];
                pv.Perc90 = dd[(int)((dd.Length - 1) * 0.90)];
                pv.Perc95 = dd[(int)((dd.Length - 1) * 0.95)];
                pv.Perc98 = dd[(int)((dd.Length - 1) * 0.98)];
            }
            if (pv.Count == 0)
            {
                pv.Min = 0;
                pv.Max = 0;
                pv.Freq = 0;
            }
            else
            {
                pv.Avg = (double) pv.Sum / pv.Count;
                pv.Freq = (double)pv.Count / (pv.EndTime - pv.StartTime).TotalSeconds;
            }
            return pv;
        }

        
        /// <summary>
        /// Resets the counter
        /// </summary>
        protected PerfCounterStats GetValuesAndReset()
        {

            PerfCounterStats pv = new PerfCounterStats
            {
                Id = this.Id,
                Sum = Interlocked.Exchange(ref _sum, 0),
                Count = Interlocked.Exchange(ref _count, 0),
                Min = Interlocked.Exchange(ref _min, Int32.MaxValue),
                Max = Interlocked.Exchange(ref _max, Int32.MinValue)
            };
            pv.Avg = pv.Count == 0 ? 0 : pv.Sum / pv.Count;
            pv.StartTime = this._lastReset;
            pv.EndTime = DateTime.Now;
            var df = (pv.EndTime - pv.StartTime).TotalSeconds;
            pv.Freq = pv.Count == 0 ? 0.0 : df / pv.Count;

            return pv;

            
        }


    }
}
