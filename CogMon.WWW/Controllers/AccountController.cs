using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using NLog;
using CogMon.Services;

namespace CogMon.WWW.Controllers
{
    public class AccountController : Controller
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        public IUserAuth UserAuth { get; set; }

        public ActionResult LogOn()
        {
            return View();
        }

        public ActionResult LogOnByToken(string token)
        {
            var ui = UserAuth.AuthenticateByToken(token);
            if (ui != null)
            {
                FormsAuthentication.SetAuthCookie(ui.Login, false);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(Models.LogOn model, string returnUrl)
        {

            if (ModelState.IsValid)
            {
                if (UserAuth.PasswordAuthenticate(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}
