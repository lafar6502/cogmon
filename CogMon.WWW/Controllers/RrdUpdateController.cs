using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CogMon.Services;
using CogMon.Lib;

namespace CogMon.WWW.Controllers
{
    public class RrdUpdateController : Controller
    {
        public IDataSeriesRepository DataSeries { get; set; }
        public IEventAggregator EventAggregator { get; set; }

        public ActionResult Update(string seriesName)
        {
            var si = DataSeries.GetDataSeries(seriesName, false);
            if (si == null) throw new Exception("Data series not found: " + seriesName);
            var dr = new DataRecord();
            dr.Ts = DateTime.Now;
            dr.Series = si.Name;
            List<double> l = new List<double>();
            foreach (var f in si.Fields)
            {
                string s = Request[f.Name];
                if (string.IsNullOrEmpty(s)) throw new Exception("Missing data field: " + f.Name);
                double v;
                if (!double.TryParse(s, out v)) throw new Exception("Invalid field: " + f.Name);
                l.Add(v);
            }
            dr.Data = l.ToArray();
            DataSeries.AppendData(dr);
            return new EmptyResult();
        }

        public ActionResult DSInfo(string dsname)
        {
            var dsi = DataSeries.GetDataSeries(dsname, true);
            return Json(dsi, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Event(string id, double? val)
        {
            DataSeries.AddEvent(id, val);
            return new EmptyResult();
        }
        
    }
}
