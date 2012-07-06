using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using CogMon.Services;

namespace CogMon.WWW.Controllers
{
    public class HomeController : Controller
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        [CogmonAuthorize]
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            log.Info("User {0}, User {1}, User {2}", User.Identity.Name, UserSessionContext.CurrentUserRecordId, this.HttpContext.User.Identity.Name);
            return View();
        }

        [CogmonAuthorize]
        public ActionResult UserInfo()
        {
            return Json(UserSessionContext.CurrentUserInfo, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            return View();
        }

        [CogmonAuthorize]
        public ActionResult UserScript()
        {
            ViewBag.UserInfoJson = Newtonsoft.Json.JsonConvert.SerializeObject(UserSessionContext.CurrentUserInfo);
            return View();
        }

        [CogmonAuthorize]
        public ActionResult Admin()
        {
            return View();
        }
    }
}
