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
            ctx.ResponseContentType = "application/json";
            if (ctx.UrlVariables.ContainsKey("id") && !string.IsNullOrEmpty(ctx.UrlVariables["id"]))
            {
                var dr = TheDb.GetLastStatus(ctx.UrlVariables["id"]);
                var json = JsonConvert.SerializeObject(dr);
                ctx.Output.Write(json);
            }
            else
            {
                var dss = TheDb.DataSources;
                ctx.Output.Write(JsonConvert.SerializeObject(dss));
            }
        }
    }
}
