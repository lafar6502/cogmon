using CogMon.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Agent
{
    public interface ITimeSeriesDatabase
    {
        void UpdateDataSource(IEnumerable<DataRecord> batch);
    }
}
