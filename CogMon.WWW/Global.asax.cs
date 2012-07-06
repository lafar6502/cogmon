using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NLog;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using BosonMVC.Services;
using CogMon.Services;
using CogMon.Services.RRD;
using System.IO;
using NGinnBPM.MessageBus.Impl.HttpService;


namespace CogMon.WWW
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static Logger log = LogManager.GetCurrentClassLogger();


        public MvcApplication()
        {
            this.AuthenticateRequest += new EventHandler(MvcApplication_AuthenticateRequest);
            this.PostAuthenticateRequest += new EventHandler(MvcApplication_PostAuthenticateRequest);
            this.BeginRequest += new EventHandler(MvcApplication_BeginRequest);
            this.EndRequest += new EventHandler(MvcApplication_EndRequest);
            this.PreRequestHandlerExecute += new EventHandler(MvcApplication_PreRequestHandlerExecute);
        }

        void MvcApplication_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            
        }

        void MvcApplication_EndRequest(object sender, EventArgs e)
        {
            
        }

        void MvcApplication_BeginRequest(object sender, EventArgs e)
        {
            var wc = (IWindsorContainer)Application["container"];
            
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			//routes.IgnoreRoute("Sys/{*pathInfo}");
			//#routes.IgnoreRoute("NGTasks/{*pathInfo}");
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            log.Info("Application start");
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            SetupContainer();
            
        }

        void MvcApplication_AuthenticateRequest(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            //log.Info("PostAuth. User: {0} ({1}). Authenticated: {2}", Context.User.Identity.Name, Context.User.Identity.GetType().Name, Context.User.Identity.IsAuthenticated);
            if (Context.User.Identity.IsAuthenticated)
            {
                if (UserSessionContext.UserAuth.IdentityAuthenticate(Context.User.Identity))
                {
                    UserSessionContext.CurrentUserIdentity = Context.User.Identity;
                }
            }
        }

        protected void SetupContainer()
        {
            WindsorContainer wc = new WindsorContainer();
            Application["container"] = wc;
            wc.Install(new ServiceInstaller());
            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(wc));
            WindsorControllerFactory.RegisterControllersFromAssembly(typeof(Controllers.HomeController).Assembly, wc);
            WindsorControllerFactory.RegisterControllersFromAssembly(typeof(WindsorControllerFactory).Assembly, wc);
            UserSessionContext.UserAuth = wc.Resolve<IUserAuth>();
        }

        

	
    }
}