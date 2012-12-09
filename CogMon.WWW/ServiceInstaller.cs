using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NLog;
using BosonMVC.Services;
using CogMon.Services;
using CogMon.Services.RRD;
using System.IO;
using NGinnBPM.MessageBus.Impl.HttpService;
using MongoDB.Driver;
using CogMon.Services.Database;

namespace CogMon.WWW
{
    public class ServiceInstaller : IWindsorInstaller
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        public void Install(IWindsorContainer wc, IConfigurationStore store)
        {
            string baseDir = Path.GetFullPath( Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
            log.Info("Base directory is {0}", baseDir);
            
            wc.Register(Component.For<NGinnBPM.MessageBus.IServiceResolver>().ImplementedBy<NGinnBPM.MessageBus.Windsor.WindsorServiceResolver>().LifeStyle.Singleton);
            wc.Register(Component.For<IServiceResolver>().ImplementedBy<WindsorServiceResolver>().LifeStyle.Singleton);
            wc.Register(Component.For<IRrdTool>().ImplementedBy<RrdTool>().LifeStyle.Singleton
                .DependsOn(new
                {
                    RrdExe = Path.Combine(baseDir, "tools\\rrdtool\\rrdtool.exe"),
                    RrdUpdate = Path.Combine(baseDir, "tools\\rrdtool\\rrdupdate.exe")
                }));

            wc.Register(Component.For<IDataSeriesRepository>().ImplementedBy<DataSeriesRepository>().LifeStyle.Singleton
                .DependsOn(new {
                    BaseDirectory = Path.Combine(baseDir, "GraphData")
                }));
            wc.Register(Component.For<ServiceCallServlet, IServlet>().ImplementedBy<ServiceCallServlet>());
            wc.Register(Component.For<NGinnBPM.MessageBus.Impl.IServiceMessageDispatcher>()
                .ImplementedBy<NGinnBPM.MessageBus.Impl.ServiceMessageDispatcher>().LifeStyle.Singleton);
            wc.Register(Component.For<JsonServiceCallHandler>().ImplementedBy<JsonServiceCallHandler>().LifeStyle.Singleton);

            string authType = System.Configuration.ConfigurationManager.AppSettings["AuthType"];
            if (string.IsNullOrEmpty(authType) || authType == "Domain" || authType == "Machine" || authType == "ApplicationDirectory")
            {
                if (string.IsNullOrEmpty(authType)) authType = "Domain";
                wc.Register(Component.For<IUserAuth>().ImplementedBy<CogMon.Services.User.WindowsAuth>().LifeStyle.Singleton
                    .DependsOn(new
                    {
                        ContextType = authType,
                        ServerName = System.Configuration.ConfigurationManager.AppSettings["Auth.ServerName"],
                        Container = System.Configuration.ConfigurationManager.AppSettings["Auth.Container"],
                        DomainUser = System.Configuration.ConfigurationManager.AppSettings["Auth.DomainUser"],
                        Password = System.Configuration.ConfigurationManager.AppSettings["Auth.Password"]
                    }));
            }
            else if (authType == "LDAP")
            {
                wc.Register(Component.For<IUserAuth>().ImplementedBy<CogMon.Services.User.LdapAuth>().LifeStyle.Singleton);
            }
            else throw new Exception("AuthType:" + authType);

            foreach (Type t in typeof(DataSeriesRepository).Assembly.GetTypes())
            {
                if (typeof(BosonMVC.Services.DirectHandler.IDirectAction).IsAssignableFrom(t))
                {
                    wc.Register(Component.For<BosonMVC.Services.DirectHandler.IDirectAction>().ImplementedBy(t).Named(t.Name)
                        .LifeStyle.Singleton);
                }
            }
            NGinnBPM.MessageBus.Windsor.MessageBusConfigurator.Begin(wc)
                .RegisterHttpMessageServicesFromAssembly(typeof(DataSeriesRepository).Assembly)
                .AddMessageHandlersFromAssembly(typeof(DataSeriesRepository).Assembly);               
            wc.Register(Component.For<MongoKeyGen>().ImplementedBy<MongoKeyGen>().LifeStyle.Singleton);
            MongoDatabase db = MongoDatabase.Create(System.Configuration.ConfigurationManager.ConnectionStrings["cogmon"].ConnectionString);
            wc.Register(Component.For<MongoDatabase>().Instance(db));
            wc.Register(Component.For<IEventAggregator>().ImplementedBy<CogMon.Services.EventStats.SimpleEventAggregator>().LifeStyle.Singleton);
            wc.Register(Component.For<IPerfCounters>().ImplementedBy<CogMon.Services.EventStats.PersistentPerfCounterManager>().LifeStyle.Singleton);
            wc.Register(Component.For<IJobStatusTracker, CogMon.Services.Management.JobStatusTracker>().ImplementedBy<CogMon.Services.Management.JobStatusTracker>().LifeStyle.Singleton);
            CogMon.Services.Database.DatabaseInit.Configure();
            CogMon.Services.Database.DatabaseInit.InitializeCogMonDatabase(db);
            wc.Register(Component.For<NGinnBPM.MessageBus.Impl.MessageDispatcher>().ImplementedBy<NGinnBPM.MessageBus.Impl.MessageDispatcher>().LifeStyle.Singleton);
            wc.Register(Component.For<IMessageDispatcher>().ImplementedBy<Services.Util.NGinnMessageDispatcher>().LifeStyle.Singleton);
        }
    }
}