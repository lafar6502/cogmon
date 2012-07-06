using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    public class DataFieldInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Value { get; set; }
    }

    public class DataSeriesInfo
    {
        public DataSeriesInfo()
        {
            Fields = new List<DataFieldInfo>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DataFieldInfo> Fields { get; set; }

        public int StepSeconds { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
