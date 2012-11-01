using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using NLog;
using CogMon.Lib.DataSeries;
using CogMon.Services.Database;
using MongoDB.Driver.Builders;

namespace CogMon.Services.EventStats
{
    public class PersistentPerfCounterManager : IPerfCounters
    {
        public MongoDatabase Db { get; set; }
        
        public void AddEvent(string id, int value)
        {
            var dt = DateTime.Now;
            var res = Db.GetCollection<PerfEventAggregator>().Update(
                Query.EQ("_id", id),
                Update.Push("Data", value).Set("LastUpdate", dt),
                UpdateFlags.Upsert);
            if (!res.UpdatedExisting)
            {
                Db.GetCollection<PerfEventAggregator>().Update(Query.EQ("_id", id), Update.Set("FrameStart", dt));
            }
        }


        public PerfCounterStats GetCurrentStats(string counterId, bool reset)
        {
            var res = Db.GetCollection<PerfEventAggregator>().FindAndModify(
                Query.EQ("_id", counterId),
                SortBy.Ascending("_id"),
                Update.Set("Data", BsonArray.Create(new int[] { })).Set("FrameStart", DateTime.Now).Set("LastUpdate", DateTime.Now),
                false);
            var pv = res.GetModifiedDocumentAs<PerfEventAggregator>();
            return pv.CalculateCurrentStats();
        }
    }
}
