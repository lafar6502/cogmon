using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services.Events
{
    /// <summary>
    /// Notification that a job has been successfully executed
    /// </summary>
    public class JobExecuted
    {
        public string JobId { get; set; }
        public string AgentIP { get; set; }
        public string DataSourceId { get; set; }
    }
}
