using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services.Database
{
    public class EventMapReduce
    {
        public string Id { get; set; }
        public string Collection { get; set; }
        public int LastProcessedId { get; set; }
        public DateTime? LastRun { get; set; }
        public string MapJs { get; set; }
        public string ReduceJs { get; set; }
        public string OutCollection { get; set; }
        public bool DeleteMappedData { get; set; }

    }
}
