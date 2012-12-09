using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services.Events
{
    /// <summary>
    /// Notification about agent contacting the server
    /// </summary>
    public class AgentQuery
    {
        public string AgentIP { get; set; }
        public string AgentPID { get; set; }
        public string JobGroup { get; set; }
    }
}
