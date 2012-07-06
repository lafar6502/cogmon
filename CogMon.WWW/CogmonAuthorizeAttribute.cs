using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CogMon.Services;
using NLog;

namespace CogMon.WWW
{
    public class CogmonAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated) return false;
            if (UserSessionContext.CurrentUserInfo != null) return true;
            var ret = UserSessionContext.UserAuth.IdentityAuthenticate(httpContext.User.Identity);
            if (!ret)
            {
                log.Info("Failed to authenticate cogmon user: {0}", UserSessionContext.CurrentUserIdentity.Name);
                return false;
            }
            if (UserSessionContext.CurrentUserIdentity == null) UserSessionContext.CurrentUserIdentity = httpContext.User.Identity;
            return true;
        }
    }
}