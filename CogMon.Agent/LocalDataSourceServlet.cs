using NGinnBPM.MessageBus.Impl.HttpService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CogMon.Agent
{
    public class LocalDataSourceServlet : IServlet
    {
        public LocalDataSourceServlet(LastValueTsDatabase db) : base()
        {
            TheDb = db;
        }
        public string MatchUrl { get; set; } = @"(^/series$|^/series/(?<id>.+)?)";

        private LastValueTsDatabase TheDb { get; set; }

        public void HandleRequest(IRequestContext ctx)
        {
            ctx.ResponseContentType = "text/plain";
            if (ctx.UrlVariables.ContainsKey("id") && !string.IsNullOrEmpty(ctx.UrlVariables["id"]))
            {
                var dr = TheDb.GetLastStatus(ctx.UrlVariables["id"]);
                foreach(var kv in dr.DataMap)
                {
                    ctx.Output.Write(kv.Key);
                    ctx.Output.Write(":");
                    ctx.Output.Write(kv.Value);
                    ctx.Output.Write(" ");
                }
                ctx.Output.WriteLine();
            }
            else
            {
                var dss = TheDb.DataSources;
                foreach(var ds in dss)
                {
                    ctx.Output.WriteLine(ds);
                }
            }
        }
    }
}
