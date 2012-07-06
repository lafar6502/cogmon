using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using CogMon.Services;
using Newtonsoft.Json;

namespace CogMon.WWW.Controllers
{
    public class TestController : Controller
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        
        public ActionResult EventAgg1()
        {
            return View();
        }
    }
}
