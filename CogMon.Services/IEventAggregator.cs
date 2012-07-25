using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib;
using CogMon.Lib.DataSeries;

namespace CogMon.Services
{
    /// <summary>
    /// mongo map-reduce event aggregator...
    /// </summary>
    public interface IEventAggregator
    {
        void AddEvent(string seriesId, DateTime? tstamp, IEnumerable<int> values);
        void AddEvent(DataRecord dr);
        void RunAggregation(string id);

        TimeSeriesData GetTimeSeries(string seriesId, DateTime start, DateTime end, string step);

    }
}
