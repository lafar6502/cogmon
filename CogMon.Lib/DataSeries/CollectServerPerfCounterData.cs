using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    /// <summary>
    /// Collect data from server-side performance counter
    /// Used by CogMon agent for periodically invoking performance counter data collection at the server
    /// </summary>
    public class CollectServerPerfCounterData
    {
        public string JobId { get; set; }
    }
}
