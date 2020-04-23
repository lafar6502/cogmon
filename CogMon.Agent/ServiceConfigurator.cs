using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using System.Configuration;
using NGinnBPM.MessageBus.Impl;
using NGinnBPM.MessageBus.Windsor;
using NGinnBPM.MessageBus;
using System.IO;
using NGinnBPM.MessageBus.Impl.HttpService;

namespace CogMon.Agent
{
    public class ServiceConfigurator
    {
        private IWindsorContainer _wc;

        public static ServiceConfigurator Begin(IWindsorContainer wc)
        {
            return new ServiceConfigurator { _wc = wc };
        }

        public void FinishConfiguration()
        {
            string bd = System.AppDomain.CurrentDomain.BaseDirectory;
            string gn = ConfigurationManager.AppSettings["JobGroups"];
            if (!string.IsNullOrEmpty(gn))
            {
                gn = gn.Replace("${machinename}", Environment.MachineName);
            }
            var cu = ConfigurationManager.AppSettings["CogMon.Url"];
            if (_wc.GetService<IServiceResolver>() == null) _wc.Register(Component.For<IServiceResolver>().ImplementedBy<WindsorServiceResolver>().LifeStyle.Singleton);
            if (!string.IsNullOrEmpty(cu))
            {
                _wc.Register(Component.For<JobScheduler, IStartableService>().ImplementedBy<JobScheduler>()
                    .LifeStyle.Singleton
                    .DependsOn(new
                    {
                        CogMonUrl = ConfigurationManager.AppSettings["CogMon.Url"],
                        ScheduleUpdateIntervalSec = 200,
                        SchedulerGroup = gn
                    }));
            }
            else
            {
                var jd = ConfigurationManager.AppSettings["JobDirectory"];
                if (string.IsNullOrEmpty(jd)) jd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JOBS");
                if (!Directory.Exists(jd)) throw new Exception("JOBS directory does not exist and cogmon url not configured: " + jd);
                _wc.Register(Component.For<FileJobScheduler, IStartableService>().ImplementedBy<FileJobScheduler>()
                    .LifeStyle.Singleton
                    .DependsOn(new
                    {
                        BaseDir = jd,
                        ScheduleUpdateIntervalSec = 60,
                        SchedulerGroup = gn
                    }));
            }
            
            _wc.Register(Component.For<HttpGetTask, JobBase>().ImplementedBy<HttpGetTask>().Named("HttpGet").LifeStyle.Transient);
            _wc.Register(Component.For<RunProcTask, JobBase>().ImplementedBy<RunProcTask>().Named("SystemCommand").LifeStyle.Transient);
            _wc.Register(Component.For<BooScriptTask, JobBase>().ImplementedBy<BooScriptTask>().Named("BooScript").LifeStyle.Transient);
            _wc.Register(Component.For<SqlSelectTask, JobBase>().ImplementedBy<SqlSelectTask>().Named("SqlSelect").LifeStyle.Transient);
            _wc.Register(Component.For<WinPerfTask, JobBase>().ImplementedBy<WinPerfTask>().Named("WinPerf").LifeStyle.Transient);
            _wc.Register(Component.For<AgentPerfCounterTask, JobBase>().ImplementedBy<AgentPerfCounterTask>().Named("AgentPerfCnt").LifeStyle.Transient);
            _wc.Register(Component.For<ServerPerfCounterTask, JobBase>().ImplementedBy<ServerPerfCounterTask>().Named("ServerPerfCnt").LifeStyle.Transient);
            _wc.Register(Component.For<PingTask, JobBase>().ImplementedBy<PingTask>().Named("Ping").LifeStyle.Transient);

            var cogurl = ConfigurationManager.AppSettings["CogMon.Url"];
            if (!string.IsNullOrEmpty(cogurl))
            {
                _wc.Register(Component.For<IServiceClient>().ImplementedBy<ServiceClient>()
                    .DependsOn(new
                    {
                        BaseUrl = ConfigurationManager.AppSettings["CogMon.Url"],
                        UserName = ConfigurationManager.AppSettings["CogMon.User"],
                        Password = ConfigurationManager.AppSettings["CogMon.Password"]
                    }));
            }
            
            _wc.Register(Component.For<BooScript.IRunScript>().ImplementedBy<BooScript.BooScriptManager>()
                .LifeStyle.Singleton
                .DependsOn(new
                {
                    BaseDirectory = Path.Combine(bd, "booscripts")
                }));
            _wc.Register(Component.For<WinPerf>().ImplementedBy<WinPerf>().LifeStyle.Singleton);
            _wc.Register(Component.For<PerfMon.PerfCounterStore>().ImplementedBy<PerfMon.PerfCounterStore>().LifeStyle.Singleton);
            string udpIp = ConfigurationManager.AppSettings["UDPPerfMonListener.ListenIP"];
            string port = ConfigurationManager.AppSettings["UDPPerfMonListener.Port"];
            if (!string.IsNullOrEmpty(udpIp))
            {
                _wc.Register(Component.For<IStartableService>().ImplementedBy<PerfMon.UDPPerfmonListener2>()
                    .LifeStyle.Singleton.DependsOn(new
                    {
                        Port = string.IsNullOrEmpty(port) ? 29823 : Int32.Parse(port),
                        LocalIP = udpIp
                    }));
            }

            string influx = ConfigurationManager.AppSettings["InfluxDbUrl"];
            if (!string.IsNullOrEmpty(influx))
            {
                _wc.Register(Component.For<ITimeSeriesDatabase>().ImplementedBy<Influx>()
                    .LifeStyle.Singleton
                    .DependsOn(new
                    {
                        ServiceUrl = influx,
                        User = ConfigurationManager.AppSettings["InfluxUser"],
                        Password = ConfigurationManager.AppSettings["InfluxPassword"],
                        DbName = ConfigurationManager.AppSettings["InfluxDbName"],
                        InfluxVersion = ConfigurationManager.AppSettings["InfluxVersion"],
                        NaNToZero = string.Equals(ConfigurationManager.AppSettings["NaNToZero"], "true", StringComparison.InvariantCultureIgnoreCase)
                    }));
                
            }
            else if (!string.IsNullOrEmpty(cogurl))
            {
                _wc.Register(Component.For<ITimeSeriesDatabase>().ImplementedBy<CogmonDataService>().LifeStyle.Singleton
                    .DependsOn(new
                    {

                    }));
            }
            else
            {
                _wc.Register(Component.For<ITimeSeriesDatabase, LastValueTsDatabase>().ImplementedBy<LastValueTsDatabase>().LifeStyle.Singleton);
            }
            var locUrl = ConfigurationManager.AppSettings["HttpListenAddress"];
            if (!string.IsNullOrEmpty(locUrl))
            {
                ConfigureHttpListener(locUrl);
            }
        }

        protected ServiceConfigurator ConfigureHttpListener(string listenAddress)
        {
            string endpoint = listenAddress.Replace("+", Environment.MachineName);

            _wc.Register(Component.For<IStartableService>().ImplementedBy<NGinnBPM.MessageBus.Impl.HttpService.HttpServer>()
                .DependsOn(new { ListenAddress = listenAddress }).LifeStyle.Singleton);
            
            _wc.Register(Component.For<MasterDispatcherServlet>().ImplementedBy<MasterDispatcherServlet>().LifeStyle.Singleton);
            
            _wc.Register(Component.For<IServlet>()
                .ImplementedBy<StaticResourceServlet>()
                .LifeStyle.Singleton
                .DependsOn(new
                {
                    MatchUrl = @"(^/index.htm$|^/rc/(?<id>.+)?)",
                    SourceAssembly = typeof(IMessageBus).Assembly,
                    ResourcePrefix = "CogMon.Agent.StaticRC"
                }));

            _wc.Register(Component.For<IServlet>()
                .ImplementedBy<LocalDataSourceServlet>().LifeStyle.Singleton);

            return this;
        }
    }
}
