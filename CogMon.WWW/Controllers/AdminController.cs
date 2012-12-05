using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using CogMon.Services;

namespace CogMon.WWW.Controllers
{
    public class AdminController : Controller
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        public IJobStatusTracker JobTracker { get; set; }

        [CogmonAuthorize]
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            log.Info("User {0}, User {1}, User {2}", User.Identity.Name, UserSessionContext.CurrentUserRecordId, this.HttpContext.User.Identity.Name);
            return View();
        }

        public ActionResult CurrentJobStatus()
        {
            return new JsonNetResult(JobTracker.GetStatusOfAllJobs());
        }

        public ActionResult CurrentAgentStatus()
        {
            return new JsonNetResult(JobTracker.GetStatusOfAllAgents());
        }

        public ActionResult DataSourcesStatus()
        {
            throw new NotImplementedException();
        }
    }
}
