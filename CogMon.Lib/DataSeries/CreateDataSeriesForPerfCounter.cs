using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    /// <summary>
    /// Create a data series for a client or server-side perf counter
    /// </summary>
    public class CreateDataSeriesForPerfCounter
    {
        public string CounterId { get; set; }
        public string JobGroup { get; set; }
        public int UpdateIntervalSec { get; set; }
        public bool ServerSide { get; set; }
        public string Description { get; set; }
    }

    public class CreateDataSeriesResponse
    {
        public string SeriesId { get; set; }
        public string JobId { get; set; }
    }
}
