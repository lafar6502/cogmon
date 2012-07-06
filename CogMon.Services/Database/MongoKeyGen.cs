using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog;

namespace CogMon.Services.Database
{
    public class MongoKeyGen
    {
        public MongoDatabase Db { get; set; }
        public int Range { get; set; }
        private class KeyRange
        {
            public int Current { get; set; }
            public int Max { get; set; }
        }
        private Dictionary<string, KeyRange> _d = new Dictionary<string, KeyRange>();

        public MongoKeyGen()
        {
            Range = 10;
        }

        public int GetNextValue(string entity)
        {
            lock (_d)
            {
                KeyRange kr;
                if (_d.TryGetValue(entity, out kr))
                {
                    kr.Current++;
                    if (kr.Current < kr.Max)
                    {
                        return kr.Current;
                    }
                    _d.Remove(entity);
                }
                
                var res = Db.GetCollection("hilo").FindAndModify(
                    MongoQueryBuilder.DynQuery(x => x._id == entity),
                    SortBy.Ascending("_id"),
                    Update.Inc("cnt", Range), false, true);
                if (!res.Ok) throw new Exception(res.ErrorMessage);
                var d = res.ModifiedDocument;
                kr = new KeyRange();
                kr.Current = d.GetValue("cnt", MongoDB.Bson.BsonValue.Create(0)).AsInt32;
                kr.Max = kr.Current + Range;
                _d[entity] = kr;
                return kr.Current;
            }
            
            
        }
    }
}
