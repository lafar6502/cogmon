using NGinnBPM.MessageBus.Impl.HttpService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CogMon.Agent.PerfMon;

namespace CogMon.Agent
{
    public class PerfCountersServlet : IServlet
    {
        public PerfCountersServlet(PerfCounterStore db) : base()
        {
            TheDb = db;
        }
        public string MatchUrl { get; set; } = @"(^/perf$|^/perf/(?<id>.+)?|^/perfget/(?<id>.+)?)";

        private PerfCounterStore TheDb { get; set; }

        public void HandleRequest(IRequestContext ctx)
        {
            
            ctx.ResponseContentType = "text/plain";
            if (ctx.UrlVariables.ContainsKey("id") && !string.IsNullOrEmpty(ctx.UrlVariables["id"]))
            {
                var doGet = ctx.RawUrl.StartsWith("/perfget/");
                var cid = ctx.UrlVariables["id"];
                if (string.IsNullOrEmpty(cid)) throw new Exception("counter id missing");
                var pc = TheDb.TryGetCounter(cid);
                if (pc == null) throw new Exception("Counter :" + cid);
                var val = pc.GetCurrentValue(!doGet);
                ctx.ResponseContentType = "application/json";
                var str = JsonConvert.SerializeObject(val);
                ctx.Output.Write(str);
            }
            else
            {
                var dss = TheDb.GetPerfCounterNames();
                ctx.ResponseContentType = "text/html";
                ctx.Output.WriteLine("<html><body>");
                ctx.Output.WriteLine("<ol>");
                foreach(var ds in dss.OrderBy(x => x))
                {
                    PerfCounterBase pb = TheDb.TryGetCounter(ds);
                    if (pb == null) continue;
                    
                    ctx.Output.WriteLine("<li>");
                    ctx.Output.WriteLine("<a href=\"/perfget/{0}\">[get] {1}  </a> <span style='padding-left:15px;'>[<b>{2}</b>  {3:MM-dd HH:mm:ss}]</span> <br/>", Uri.EscapeUriString(ds), ds, pb.LastValue, pb.LastUpdate);
                    
                    ctx.Output.WriteLine("<a href=\"/perf/{0}\">[get/reset]</a>", Uri.EscapeUriString(ds), ds);
                    ctx.Output.WriteLine("</li>");
                }
                ctx.Output.WriteLine("</ol>");
                ctx.Output.WriteLine("</body></html>");
            }
        }
    }
}
