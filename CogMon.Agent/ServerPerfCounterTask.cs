using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Data.Common;
using System.Configuration;
using CogMon.Lib;
using CogMon.Lib.DataSeries;
using NGinnBPM.MessageBus;

namespace CogMon.Agent
{
    /// <summary>
    /// 
    /// </summary>
    public class ServerPerfCounterTask : JobBase
    {
        public IServiceClient ServiceClient { get; set; }

        protected override void Run()
        {
            ///we just call the server, all processing is done server-side...
            ServiceClient.CallService<string>(new CollectServerPerfCounterData { JobId = this.Id });
        }
    }
}
