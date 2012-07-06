using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    
    public class GraphTemplate
    {

        public string Id { get; set; }
        public string Title { get; set; }
        public string VAxisLabel { get; set; }
        public string Options { get; set; }
        public class DEFTemplate
        {
            public string VariableName { get; set; }
            public string DataSource { get; set; }
            public string Series { get; set; }
            public string Options { get; set; }
        }
        public List<DEFTemplate> DEFs { get; set; }
        public class CDEFTemplate
        {
            public string VariableName { get; set; }
            public string Expression { get; set; }
        }
        public List<CDEFTemplate> CDEFs { get; set; }

    }
}
