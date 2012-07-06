using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib
{
    public enum ConsolidationFunction
    {
        MIN = 0,
        MAX = 1,
        AVERAGE = 2,
        LAST = 3
    }

    public enum SeriesTypes
    {
        GAUGE = 0,
        COUNTER = 1,
        DERIVE = 2,
        ABSOLUTE = 3,
        COMPUTE = 4
    }


}
