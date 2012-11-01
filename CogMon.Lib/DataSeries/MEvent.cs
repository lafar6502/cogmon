using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    /// <summary>
    /// Event information record used with mongodb-based reports
    /// </summary>
    public class MEvent
    {
        public int Id { get; set; }
        /// <summary>
        /// timestamp
        /// </summary>
        public DateTime Ts { get; set; }
        /// <summary>
        /// event name (grouping key)
        /// </summary>
        public string P { get; set; }
        /// <summary>
        /// list of event values
        /// </summary>
        public List<int> V { get; set; }
    }
}
