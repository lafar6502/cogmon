using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NGinnBPM.MessageBus;
using System.Timers;
using NLog;
using CogMon.Lib.Scheduling;
using Castle.Windsor;
using NGinnBPM.MessageBus.Impl;

namespace CogMon.Agent
{
    public partial class Service1 : ServiceBase
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        private WindsorContainer _container;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var wc = new WindsorContainer();
            ServiceConfigurator.Begin(wc)
                .FinishConfiguration();
            _container = wc;
            foreach (IStartableService s in _container.ResolveAll<IStartableService>())
            {
                s.Start();
            }
        }

        

       

        public void Start(string[] args)
        {
            OnStart(args);
        }

        protected override void OnStop()
        {
            if (_container != null)
            {
                foreach (IStartableService s in _container.ResolveAll<IStartableService>())
                {
                    s.Stop();
                }
                _container.Dispose();
                _container = null;
            }
        }
    }
}
