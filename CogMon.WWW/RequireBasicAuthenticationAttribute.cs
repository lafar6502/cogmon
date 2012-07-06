using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CogMon.WWW
{
    public class RequireBasicAuthenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var req = filterContext.HttpContext.Request;
            if (String.IsNullOrEmpty(req.Headers["Authorization"]))
            {
                var res = filterContext.HttpContext.Response;
                res.StatusCode = 401;
                res.AddHeader("WWW-Authenticate", "Basic realm=\"Twitter\"");
                res.End();
            }
        }
    }
}