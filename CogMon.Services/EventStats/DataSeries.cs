using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services.EventStats
{
    /// <summary>
    /// Data series description for mongodb-based 
    /// reports
    /// </summary>
    public class DataSeries
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public class DataField
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
        public List<DataField> Fields { get; set; }
        public string AggDataCollection { get; set; }
        public List<string> ACL { get; set; }
    }
}
