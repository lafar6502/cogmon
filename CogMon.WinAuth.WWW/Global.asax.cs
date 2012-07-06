using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using MongoDB.Driver;
using NLog;
using CogMon.Lib;

namespace CogMon.WinAuth.WWW
{
    public class Global : System.Web.HttpApplication
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        void Application_Start(object sender, EventArgs e)
        {
            log.Info("Application start");
            DatabaseInit();
        }

        protected void DatabaseInit()
        {
            if (System.Configuration.ConfigurationManager.ConnectionStrings["cogmon"] == null) throw new Exception("Missing 'cogmon' connection string in web.config");
            MongoDatabase db = MongoDatabase.Create(System.Configuration.ConfigurationManager.ConnectionStrings["cogmon"].ConnectionString);
            if (!db.CollectionExists("authtokens")) throw new Exception("Missing collection - authtokens. Run cogmon root website first");
            Db = db;
            log.Info("Db inited");
        }

        public static MongoDatabase Db { get; set; }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
