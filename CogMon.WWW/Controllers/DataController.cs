using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CogMon.Services;
using CogMon.Lib.DataSeries;
using CogMon.Lib;
using CogMon.Lib.Graph;
using NLog;
using System.IO;
using Newtonsoft.Json;

namespace CogMon.WWW.Controllers
{
    /// <summary>
    /// Data controller is responsible for delivering graph data
    /// it unifies rrd and mongodb databases
    /// </summary>
    public class DataController : Controller
    {
        public IDataSeriesRepository DataSeriesRepo { get; set; }
        public IEventAggregator AggDb { get; set; }
        public MongoDB.Driver.MongoDatabase Db { get; set; }

        private Logger log = LogManager.GetCurrentClassLogger();

        private bool IsClientCached(DateTime contentModified)
        {
            string header = Request.Headers["If-Modified-Since"];
            if (header != null)
            {
                DateTime isModifiedSince;
                if (DateTime.TryParse(header, out isModifiedSince))
                {
                    return isModifiedSince > contentModified;
                }
            }
            return false;
        }

        
        protected T QueryParam<T>(string name, T defVal)
        {
            string s = Request[name];
            return string.IsNullOrEmpty(s) ? defVal : (T) Convert.ChangeType(s, typeof(T));
        }

        protected TimeSeriesData GetRrdData(string id, string startTime, string endTime, int? step, int? maxRows)
        {
            DrawOptions opts = new DrawOptions
            {
                StartTime = startTime,
                EndTime = endTime,
                Width = maxRows,
                Height = 0,
                Step = step
            };

            return this.DataSeriesRepo.ExportGraphData(id, opts);
        }
        /// <summary>
        /// Export RRD graph data as xml
        /// </summary>
        /// <param name="definitionId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="w"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public ActionResult GetData(string id, string startTime, string endTime, int? maxRows, int? step, string format)
        {
            TimeSeriesData tsd = null;
            if (id.StartsWith("RRD"))
            {
                tsd = GetRrdData(id.Substring(3), startTime, endTime, step, maxRows);
            }
            else if (id.StartsWith("MAP"))
            {
            }
            else throw new Exception("Unknown id prefix");

            if (string.IsNullOrEmpty(format) || format == "json")
            {
                return Content(JsonConvert.SerializeObject(tsd, new Newtonsoft.Json.Converters.IsoDateTimeConverter()), "application/json");
            }
            else if (format == "csv")
            {
                var sw = new StringWriter();
                foreach (var r in tsd.Rows)
                {
                    sw.Write("{0},{1}", r.T, r.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    foreach (var v in r.V)
                    {
                        sw.Write(",");
                        sw.Write(v);
                    }
                    sw.WriteLine();
                }
                return Content(sw.ToString(), "text/csv");
            }
            else
            {
                throw new Exception("Format not supported: " + format);
            }
        }

        public class TimeSeriesRef
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string SeriesType { get; set; }
            public string Description { get; set; }
        }

        public ActionResult ListDataSeries()
        {
            List<TimeSeriesRef> lst = new List<TimeSeriesRef>();
            foreach(var gd in Db.Find<GraphDefinition>(x => x.ACL.In(UserSessionContext.CurrentUserInfo.GetUserACL())).SetFields("_id", "Title", "Description"))
            {
                lst.Add(new TimeSeriesRef
                {
                    Id = "RRD" + gd.Id,
                    Name = gd.Title,
                    Description = gd.Description,
                    SeriesType = "RRD"
                });
            }
            foreach (var ds in Db.Find<Services.EventStats.DataSeries>(x => x.ACL.In(UserSessionContext.CurrentUserInfo.GetUserACL())))
            {
                lst.Add(new TimeSeriesRef
                {
                    Id = "MAP" + ds.Id,
                    Name = ds.Name,
                    Description = ds.Description,
                    SeriesType = "MapRed"
                });
            }
            return Json(lst.OrderBy(x => x.Name), JsonRequestBehavior.AllowGet);
        }

        
    }
}