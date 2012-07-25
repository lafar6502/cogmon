using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CogMon.Services;
using CogMon.Lib;
using Newtonsoft.Json;

namespace CogMon.WWW.Controllers
{
    public class EventStatController : Controller
    {
        public IEventAggregator EventAggregator { get; set; }
        
        public ActionResult AddEvent(string series, string ts, string values)
        {
            DateTime? dt = null;
            if (!string.IsNullOrEmpty(ts))
            {
                dt = DateTime.Parse(ts);
            }
            string[] arr = values.Split(',');
            if (arr.Length == 0) throw new Exception("One or more values required");
            var lst = arr.Select(x => Int32.Parse(x));
            EventAggregator.AddEvent(series, dt, lst); 
            return new EmptyResult();
        }


        public ActionResult RunAggregation(string id)
        {
            EventAggregator.RunAggregation(id);
            return new EmptyResult();
        }

        public ActionResult GetData(string series, string start, string end, string step)
        {
            DateTime st, ed;
            Services.RRD.RrdUtil.ParseRrdDateRange(start, end, out st, out ed);
            var dt = EventAggregator.GetTimeSeries(series, st, ed, step);
            return Content(JsonConvert.SerializeObject(dt, new Newtonsoft.Json.Converters.IsoDateTimeConverter()), "application/json");
            //return Json(dt, JsonRequestBehavior.AllowGet);
        }
        
    }
}
