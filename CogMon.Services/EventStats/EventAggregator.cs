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
    /// <summary>
    /// EventAggregator 
    /// This class manages MongoDB event aggregation
    /// using map-reduce.
    /// </summary>
    public class SimpleEventAggregator : IEventAggregator
    {
        public MongoDatabase Db { get; set; }
        public Database.MongoKeyGen KeyGen { get; set; }

        private Logger log = LogManager.GetCurrentClassLogger();

        public void AddEvent(MEvent mev, string collection)
        {
            Db.GetCollection(collection).Insert(mev);
        }

        

        public void AddEvent(string id, DateTime ts, Dictionary<string, double> values)
        {
            
        }

        public void RunAggregation(string id)
        {
            var er = Db.GetCollection<EventMapReduce>().FindOneById(id);
            if (er == null) throw new Exception("Invalid id: " + id);
            var col = Db.GetCollection(er.Collection);
            var bl = col.Find(Query.GT("_id", er.LastProcessedId)).SetSortOrder(SortBy.Descending("_id")).SetLimit(1).ToList();
            int maxid = 0, minid = er.LastProcessedId;
            if (bl.Count > 0)
            {
                maxid = bl[0].GetValue("_id").AsInt32;
                log.Debug("Col {0}: Max id is {1}", er.Collection, maxid);
                var res = col.MapReduce(
                    Query.And(Query.GT("_id", minid), Query.LTE("_id", maxid)),
                    BsonJavaScript.Create(er.MapJs),
                    BsonJavaScript.Create(er.ReduceJs),
                    MapReduceOptions.SetOutput(MapReduceOutput.Reduce(er.OutCollection)));
                if (!res.Ok) throw new Exception("MapReduce error: " + res.ErrorMessage);
                er.LastProcessedId = maxid;
            }
            er.LastRun = DateTime.Now;
            Db.GetCollection<EventMapReduce>().Save(er);
            if (er.DeleteMappedData && maxid > 0)
            {
                Db.GetCollection(er.Collection).Remove(Query.And(Query.GT("_id", minid), Query.LTE("_id", maxid)));
            }
            
        }

        public void AddEvent(string seriesId, DateTime? tstamp, IEnumerable<int> values)
        {
            int idx = seriesId.IndexOf('/');
            string sid = seriesId;
            string cname = "mevents";
            if (idx > 0)
            {
                cname = seriesId.Substring(0, idx);
                sid = seriesId.Substring(idx + 1);
            }
            var mev = new MEvent
            {
                Id = KeyGen.GetNextValue(cname),
                P = sid,
                Ts = tstamp.HasValue ? tstamp.Value : DateTime.Now,
                V = new List<int>(values)
            };
            var r = Db.GetCollection(cname).Insert(mev);
        }

        /// <summary>
        /// M - month
        /// D - day
        /// H - hour
        /// A - 5-minutes
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        private char CalcDefaultStep(TimeSpan ts, DataSeries ds)
        {
            if (ts.TotalDays > 90) return 'M';
            if (ts.TotalDays > 3) return 'D';
            if (ts.TotalHours > 3) return 'H';
            return 'A';
        }

        private static DateTime RoundToStepBoundary(DateTime dt, char step, bool upper)
        {
            switch (step)
            {
                case 'M':
                    return new DateTime(dt.Year, dt.Month, 1);
                case 'D':
                    return dt.Date;
                case 'H':
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
                case 'A':
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute - (dt.Minute % 5), 0);
                default:
                    throw new NotImplementedException();
            }
        }

        private static DateTime Increment(DateTime dt, char step, int steps)
        {
            switch (step)
            {
                case 'M':
                    return dt.AddMonths(steps);
                case 'D':
                    return dt.AddDays(steps);
                case 'H': return dt.AddHours(steps);
                case 'A': return dt.AddMinutes(steps * 5);
                default: throw new NotImplementedException();
            }
        }

        private static string FormatDatePartForStep(DateTime dt, char step)
        {
            switch (step)
            {
                case 'M' : return dt.ToString("yyyyMM");
                case 'D' : return dt.ToString("yyyyMMdd");
                case 'H': return dt.ToString("yyyyMMddHH");
                case 'A':
                    return dt.ToString("yyyyMMddHHmm");
                default:
                    throw new Exception("step: " + step);
            }
        }

        private static string FormatLabelForTimestamp(DateTime dt, char step)
        {
            switch (step)
            {
                case 'A':
                    return dt.ToString("HH:mm");
                case 'D':
                    return dt.ToString("yyyy-MM-dd");
                case 'H':
                    return dt.ToString("HH");
                case 'M':
                    return dt.ToString("yyyy-MM");
            }
            throw new Exception();
        }

        private DateTime ParseDateTimeFromAggId(string id)
        {
            int idx = id.IndexOf('#');
            if (idx >= 0) id = id.Substring(idx + 1);
            if (id.Length == 4) return new DateTime(int.Parse(id), 0, 0);
            if (id.Length == 6) return DateTime.ParseExact(id, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture);
            if (id.Length == 8) return DateTime.ParseExact(id, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            if (id.Length == 10) return DateTime.ParseExact(id, "yyyyMMddHH", System.Globalization.CultureInfo.InvariantCulture);
            if (id.Length == 12) return DateTime.ParseExact(id, "yyyyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture);
            throw new Exception("Invalid date :" + id);
        }

        internal class reduce_sums
        {
            public string _id { get; set; }
            public reduce_sums_val value { get; set; }
            internal class reduce_sums_val
            {
                public int count { get; set; }
                public int[] V { get; set; }
            }
        }

        private static string GetKey(string seriesId, char step, DateTime dt)
        {
            string ed = FormatDatePartForStep(dt, step);
            return string.Format("{0}{1}#{2}", step, seriesId, ed);
        }

        public object GetDataSeries(string seriesId, DateTime start, DateTime end, string step)
        {
            var ds = Db.GetCollection<DataSeries>().FindOneById(seriesId);
            if (ds == null) throw new Exception("Invalid series: " + seriesId);
            char stp = string.IsNullOrEmpty(step) ? CalcDefaultStep(end - start, ds) : step[0];
            string st = FormatDatePartForStep(start, stp);
            string ed = FormatDatePartForStep(end, stp);
            st = string.Format("{0}{1}#{2}", stp, ds.Id, st);
            ed = string.Format("{0}{1}#{2}", stp, ds.Id, ed);
            var lst = Db.GetCollection(ds.AggDataCollection).FindAs<reduce_sums>(MongoQueryBuilder.DynQuery(x => x._id >= st && x._id <= ed)).SetSortOrder("_id").ToList();
            var ret = new List<object>();
            Dictionary<string, reduce_sums> d = new Dictionary<string, reduce_sums>();
            foreach (var s in lst)
            {
                d[s._id] = s;
            }

            DateTime st1 = RoundToStepBoundary(start, stp, false);
            while (st1 < end)
            {
                string k = GetKey(ds.Id, stp, st1);
                reduce_sums s;
                if (d.ContainsKey(k))
                    s = d[k];
                else
                    s = new reduce_sums { _id = k, value = new reduce_sums.reduce_sums_val { count = 0, V = (int[]) Array.CreateInstance(typeof(int), ds.Fields.Count) } };
                var bd = new Dictionary<string, object>();

                bd.Add("Id", s._id);
                bd.Add("Timestamp", st1);
                bd.Add("Count", s.value.count);
                bd.Add("Label", FormatLabelForTimestamp(st1, stp));
                for (int i = 0; i < s.value.V.Length; i++)
                {
                    bd.Add(ds.Fields[i].Name, s.value.V[i]);
                }
                ret.Add(bd);
                st1 = Increment(st1, stp, 1);
            }

            return ret;
        }

        public DataSeries GetDataSeriesInfo(string seriesId)
        {
            return Db.GetCollection<DataSeries>().FindOneById(seriesId);
        }

        public void AddEvent(Lib.DataRecord dr)
        {
            var ds = GetDataSeriesInfo(dr.Series);
            if (ds == null) throw new Exception("Invalid data series id");
            if (dr.DataMap != null && dr.DataMap.Count > 0)
            {
                if (ds.Fields.Count != dr.DataMap.Count) throw new Exception("Invalid number of fields in data map - expected " + ds.Fields.Count);
                var lst = ds.Fields.Select(x =>
                {
                    if (!dr.DataMap.ContainsKey(x.Name)) throw new Exception("Missing data field: " + x.Name);
                    return Convert.ToInt32(dr.DataMap[x.Name]);
                });
                this.AddEvent(dr.Series, dr.Ts, lst);
            }
            else
            {
                if (dr.Data == null || dr.Data.Length == 0) throw new Exception("Missing event data");
                if (dr.Data.Length != ds.Fields.Count) throw new Exception("Invalid number of data fields - expected " + ds.Fields.Count);
                this.AddEvent(dr.Series, dr.Ts, dr.Data.Select(x => Convert.ToInt32(x)));
            }
        }
    }
}
