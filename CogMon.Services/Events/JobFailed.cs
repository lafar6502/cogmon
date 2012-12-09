using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services.Events
{
    /// <summary>
    /// Information about job execution error
    /// </summary>
    public class JobFailed
    {
        public string JobId { get; set; }
        public string AgentIP { get; set; }
        public string ErrorInfo { get; set; }
    }
}
