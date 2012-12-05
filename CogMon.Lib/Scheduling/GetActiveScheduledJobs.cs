using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.Scheduling
{
    /// <summary>
    /// Query the server for job information
    /// </summary>
    public class GetActiveScheduledJobs
    {
        /// <summary>
        /// Get jobs from specified groups only
        /// If null, all jobs will be returned
        /// </summary>
        public string[] Groups { get; set; }
        public string AgentPID { get; set; }
        public DateTime? UpdatedAfter { get; set; }
    }

    

    public class GetActiveScheduledJobsResponse
    {
        public List<ScheduledTask> Tasks { get; set; }
    }

    public class ReportJobFailed
    {
        public string JobId { get; set; }
        public DateTime Timestamp { get; set; }
        public string ErrorInfo { get; set; }
    }
}
