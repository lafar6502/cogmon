using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Configuration;

namespace CogMon.Agent
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            if (ConfigurationManager.AppSettings["ServiceName"] != null)
            {
                serviceInstaller1.ServiceName = ConfigurationManager.AppSettings["ServiceName"];
                serviceInstaller1.DisplayName = "CogMon agent " + ConfigurationManager.AppSettings["ServiceName"];
            }
        }
    }
}
