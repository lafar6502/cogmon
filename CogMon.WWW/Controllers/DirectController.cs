using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NLog;

namespace CogMon.WWW.Controllers
{
    public class DirectController : BosonMVC.Services.DirectHandler.ExtDirectController
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        public override void Execute(System.Web.Routing.RequestContext requestContext)
        {
            log.Info("USER: {0}", HttpContext.Current.User.Identity.Name);    
            base.Execute(requestContext);
        }
    }
}