using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CogMon.Agent.PerfMon
{
    /// <summary>
    /// Performance counter statistics
    /// </summary>
    public class PerfValues
    {
        public string InstanceId { get; set; }
        public long Sum { get; set; }
        public int Count { get; set; }
        public double Frequency { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime LastReset { get; set; }
        public int Median { get; set; }
        public int Perc90 { get; set; }
        public int Perc95 { get; set; }
        public int Perc99 { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
    }
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
        public int MaxUpdates { get; set; }

        private DateTime _lastReset;
        private DateTime _lastUpdate;
        private long _sum;
        private int _count;
        private int _min;
        private int _max;

        public PerfCounter()
        {
            GetValuesAndReset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void Update(int val)
        {

        }

        /// <summary>
        /// Return current perf counter values and optionally reset the counter
        /// </summary>
        /// <param name="reset"></param>
        /// <returns></returns>
        public PerfValues GetCurrentValue(bool reset)
        {
            if (reset) return GetValuesAndReset();
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Resets the counter
        /// </summary>
        protected PerfValues GetValuesAndReset()
        {

            PerfValues pv = new PerfValues
            {
                InstanceId = this.Id,
                Sum = Interlocked.Exchange(ref _sum, 0),
                Count = Interlocked.Exchange(ref _count, 0),
                Min = Interlocked.Exchange(ref _min, Int32.MaxValue),
                Max = Interlocked.Exchange(ref _max, Int32.MinValue)
            };
            return pv;

            
        }


    }
}
