using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CogMon.Lib;
using System.Collections.Concurrent;

namespace CogMon.Agent
{
    public class LastValueTsDatabase : ITimeSeriesDatabase
    {
        private ConcurrentDictionary<string, DataRecord> _data = new ConcurrentDictionary<string, DataRecord>();
        public void UpdateDataSource(IEnumerable<DataRecord> batch)
        {
            foreach(var dr in batch)
            {
                if (string.IsNullOrEmpty(dr.Series)) continue;
                _data[dr.Series] = dr;
            }
        }

        public DataRecord GetLastStatus(string series)
        {
            DataRecord dr;
            if (_data.TryGetValue(series, out dr)) return dr;
            return null;
        }

        public IEnumerable<string> DataSources => _data.Keys;
    }
}
