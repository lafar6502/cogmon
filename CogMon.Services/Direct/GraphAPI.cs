using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BosonMVC.Services.DirectHandler;
using CogMon.Lib.DataSeries;
using MongoDB.Driver;
using CogMon.Lib;

namespace CogMon.Services.Direct
{
    public class GraphTreeNodeInfo
    {
        public string Id { get; set; }
        public bool IsFolder { get; set; }
        public string Name { get; set; }
    }

    public class GraphAPI : IDirectAction
    {
        public IDataSeriesRepository DSRepo { get; set; }
        public MongoDatabase Db { get; set; }

        [DirectMethod]
        public List<object> GetRRDDataSources()
        {
            return new List<object>(DSRepo.DataSources);
        }

        [DirectMethod]
        public List<DataFieldInfo> GetDataSourceFields(string dsId)
        {
            var ds = DSRepo.GetDataSeries(dsId, false);
            return ds.Fields;
        }

        
    }
}
