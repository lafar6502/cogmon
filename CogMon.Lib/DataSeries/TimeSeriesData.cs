using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    /// <summary>
    /// Class for exporting time series data
    /// </summary>
    public class TimeSeriesData
    {
        public string SeriesId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int StepSecs { get; set; }
        /// <summary>
        /// Column names (not including timestamp/T columns that are always present)
        /// </summary>
        public List<string> DataColumns { get; set; }

        public class Column
        {
            public string Name { get; set; }
            public string Color { get; set; }
        }
        /// <summary>
        /// Todo use instead of DataColumns...
        /// </summary>
        public List<Column> Columns { get; set; }

        public class Row
        {
            /// <summary>
            /// Unix time
            /// </summary>
            public int T { get; set; }
            /// <summary>
            /// UTC timestamp
            /// </summary>
            public DateTime Timestamp { get; set; }
            /// <summary>
            /// Column values
            /// </summary>
            public double[] V { get; set; }
        }

        public List<Row> Rows { get; set; }
    }
}
