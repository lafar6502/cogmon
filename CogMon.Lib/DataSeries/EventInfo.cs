using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    public class EventInfo
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Label { get; set; }
        public string Details { get; set; }
        public IdLabel Owner { get; set; }
        public string Category { get; set; }
    }
}
