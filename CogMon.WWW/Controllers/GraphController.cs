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

namespace CogMon.WWW.Controllers
{
    public class GetDataSourcesResponse
    {
        public DataSeriesInfo[] DataSources { get; set; }
    }

    public class GraphController : Controller
    {
        public IDataSeriesRepository DataSeriesRepo { get; set; }
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

        public ActionResult DrawGraph(string definitionId, string startTime, string endTime, int? w, int? h, string eventCategories, string skipElements, int? step)
        {
            List<int> skipList = null;
            if (!string.IsNullOrEmpty(skipElements))
            {
                var a1 = skipElements.Split(',');
                if (a1.Length > 0)
                {
                    skipList = a1.Select(x => Int32.Parse(x)).ToList();
                }
            }

            DrawOptions ops = new DrawOptions 
            {
                StartTime = startTime,
                EndTime = endTime,
                Width = w,
                Height = h,
                Step = step,
                SkipElements = skipList
            };
            Response.ContentType = "image/png";
            var expireTime = TimeSpan.FromSeconds(300);
            Response.Cache.SetMaxAge(expireTime);
            
            //Response.Expires = 5;
            var imgInfo = DataSeriesRepo.CreateGraph(definitionId, ops, Response.OutputStream);
            string imgInfoKey = GraphInfoKey(definitionId, startTime, endTime, w, h, eventCategories, skipElements, step);
            HttpContext.Cache.Add(imgInfoKey, imgInfo, null, DateTime.Now.AddMinutes(1) + expireTime, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
            return new EmptyResult();
        }

        protected T QueryParam<T>(string name, T defVal)
        {
            string s = Request[name];
            return string.IsNullOrEmpty(s) ? defVal : (T) Convert.ChangeType(s, typeof(T));
        }

        [HttpPost]
        public ActionResult DrawByDef()
        {
            GraphDefinition gd = null;
            if (Request.ContentLength == 0) throw new Exception("Missing http post content");
            using (var st = Request.InputStream)
            {
                gd = Newtonsoft.Json.JsonConvert.DeserializeObject<GraphDefinition>(new StreamReader(st, Request.ContentEncoding).ReadToEnd());
            }
            DrawOptions ops = new DrawOptions 
            {
                StartTime = QueryParam("start", "e-1d"),
                EndTime = QueryParam("end", "now"),
                Width = QueryParam("w", 300),
                Height = QueryParam("h", 200),
                Step = QueryParam("step", 1)
            };
            string gid = Guid.NewGuid().ToString("N");
            var ms = new MemoryStream();
            DataSeriesRepo.CreateGraph(gd, ops, ms);
            HttpContext.Cache.Add(gid, ms.ToArray(), null, DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
            return Content(gid, "text/plain");
        }

        public ActionResult DrawByDefImage(string id)
        {
            var ms = HttpContext.Cache.Get(id) as byte[];
            if (ms == null) return HttpNotFound();
            Response.ContentType = "image/png";
            Response.OutputStream.Write(ms, 0, ms.Length);
            return new EmptyResult();
        }

        

        public ActionResult Index(string definitionId, string startTime, string endTime, int? w, int? h, string eventCategories, string skipElements, int? step)
        {
            string key = GraphInfoKey(definitionId, startTime, endTime, w, h, eventCategories, skipElements, step);

            ViewBag.definitionId = definitionId;
            ViewBag.startTime = startTime;
            ViewBag.endTime = endTime;
            ViewBag.w = w;
            ViewBag.h = h;
            ViewBag.imgInfoKey = key;
            ViewBag.step = step;
            return View();
            
        }

        private string GraphInfoKey(string definitionId, string startTime, string endTime, int? w, int? h, string eventCats, string skipElements, int? step)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", definitionId, startTime, endTime, w, h, HttpContext.Session.SessionID, eventCats, skipElements, step);
        }

        public ActionResult ImageInfo(string key)
        {
            RrdImageInfo ii = (RrdImageInfo) HttpContext.Cache.Get(key);
            if (ii == null) return HttpNotFound();
            return Json(ii, JsonRequestBehavior.AllowGet);
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
        public ActionResult XportGraphData(string definitionId, string startTime, string endTime, int? maxRows, int? step)
        {
            DrawOptions opts = new DrawOptions 
            {
                StartTime = startTime,
                EndTime = endTime,
                Width = maxRows,
                Height = 0,
                Step = step
            };

            var res = this.DataSeriesRepo.ExportGraphData(definitionId, opts);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GraphInfo(string definitionId, string startTime, string endTime, int? w, int? h, string eventCategories, string skipElements, int? step)
        {
            string key = GraphInfoKey(definitionId, startTime, endTime, w, h, eventCategories, skipElements, step);
            List<int> skipList = null;
            if (!string.IsNullOrEmpty(skipElements))
            {
                var a1 = skipElements.Split(',');
                if (a1.Length > 0)
                {
                    skipList = a1.Select(x => Int32.Parse(x)).ToList();
                }
            }
            RrdImageInfo ii = (RrdImageInfo)HttpContext.Cache.Get(key);
            if (ii == null)
            {
                log.Info("Generating graph just to get graph information. Key: {0}", key);
                DrawOptions ops = new DrawOptions
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Width = w,
                    Height = h,
                    Step = step,
                    SkipElements = skipList
                };
                ii = DataSeriesRepo.CreateGraph(definitionId, ops, null);
                HttpContext.Cache.Add(key, ii, null, DateTime.Now.AddSeconds(300 + 60), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
            }
            else
            {
                log.Debug("Graph info cache hit: {0}", key);
            }
            return Json(ii, JsonRequestBehavior.AllowGet);
        }
        
    }
}