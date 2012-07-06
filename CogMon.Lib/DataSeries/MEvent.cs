using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    public class MEvent
    {
        public int Id { get; set; }
        public DateTime Ts { get; set; }
        public string P { get; set; }
        public List<int> V { get; set; }
    }
}
