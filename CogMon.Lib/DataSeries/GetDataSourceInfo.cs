using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    public class GetDataSeriesInfo
    {
        public string Name { get; set; }
    }

    public class GetDataSeriesList
    {
    }

    public class GetDataSeriesListResponse
    {
        public List<string> DataSeries { get; set; }
    }
}
