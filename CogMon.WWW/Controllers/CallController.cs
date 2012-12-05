using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CogMon.Services;
using CogMon.Lib;
using NLog;
using NGinnBPM.MessageBus;
using NGinnBPM.MessageBus.Impl.HttpService;

namespace CogMon.WWW.Controllers
{

    /// <summary>
    /// Controller for handling NGinn json-rpc calls
    /// </summary>
    public class CallController : Controller
    {
        public ServiceCallServlet CallHandler { get; set; }
        private Logger log = LogManager.GetCurrentClassLogger();
        public IUserAuth UserAuth { get; set; }

        public bool RequireBasicAuth { get; set; }

        protected override void Execute(System.Web.Routing.RequestContext ctx)
        {
            var c2 = new HttpContextWrapper(ctx.HttpContext);
            if (RequireBasicAuth && String.IsNullOrEmpty(c2.Headers["Authorization"]))
            {
                var res = ctx.HttpContext.Response;
                res.StatusCode = 401;
                res.AddHeader("WWW-Authenticate", "Basic realm=\"CogMonSys\"");
                res.End();
                return;
            }

            string auth = c2.Headers["Authorization"];
            if (!string.IsNullOrEmpty(auth))
            {
                
            }

            string sname = c2.Url.Segments[c2.Url.Segments.Length - 1];
            if (sname.IndexOf('.') > 0) sname = sname.Substring(0, sname.IndexOf('.'));
            c2.UrlVariables = new Dictionary<string, string>();
            c2.UrlVariables["name"] = sname;
            try
            {
                RequestContext.CurrentRequest = c2;
                CallHandler.HandleRequest(c2);
            }
            catch (Exception ex)
            {
                log.Warn("Error handling request {0}: {1}", c2.Url, ex);
                c2.ClearResponse();
                c2.ResponseStatus = 500;
                c2.Output.WriteLine("Error: {0}", ex.Message);
            }
            finally
            {
                RequestContext.CurrentRequest = null;
            }
        }

        
        public ActionResult Index()
        {
            return new EmptyResult();
        }
    }
}
