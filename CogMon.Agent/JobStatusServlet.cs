using NGinnBPM.MessageBus.Impl.HttpService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CogMon.Agent.PerfMon;
using NGinnBPM.MessageBus;
using System.Configuration;

namespace CogMon.Agent
{
    public class JobStatusServlet : IServlet
    {
        
        public string MatchUrl { get; set; } = @"^/$";

        private IServiceResolver Services { get; set; }

        public JobStatusServlet(IServiceResolver sr) : base()
        {
            Services = sr;
        }

        public void HandleRequest(IRequestContext ctx)
        {
            var fs = Services.GetInstance<FileJobScheduler>();

            ctx.ResponseContentType = "text/html";
            ctx.Output.WriteLine("<html><body>");
            ctx.Output.WriteLine("<ol>");
            ctx.Output.WriteLine("<li><a href=\"/perf/\">perf counters</a></li>");
            ctx.Output.WriteLine("<li><a href=\"/series\">local data series</a></li>");
            ctx.Output.WriteLine("</ol>");
            if (fs != null)
            {
                ctx.Output.WriteLine("<h2>Job files</h2>");
                ctx.Output.WriteLine("<table border=1 cellpadding=3>");
                ctx.Output.WriteLine("<tr>");
                ctx.Output.WriteLine("<th>ID</th><th>Success?</th><th>Last run date</th><th>Status</th>");
                ctx.Output.WriteLine("</tr>");
                foreach (var t in fs.Tasks)
                {
                    ctx.Output.WriteLine("<tr>");
                    ctx.Output.WriteLine("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td>", t.Id, t.IsLastRunSuccess ? "Y" : "N", t.LastRun.ToString("yyyy-MM-dd HH:mm:ss"), t.StatusInfo);
                    ctx.Output.WriteLine("</tr>");
                }
                ctx.Output.WriteLine("</table>");
            }
            ctx.Output.WriteLine("</body></html>");
            
        }

        
    }
}
