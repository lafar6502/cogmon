using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services.Events
{
    /// <summary>
    /// Agent did not report for some time and is considered inactive
    /// </summary>
    public class AgentDisappeared
    {
        public string AgentIP { get; set; }
        public string AgentPID { get; set; }
    }
}
