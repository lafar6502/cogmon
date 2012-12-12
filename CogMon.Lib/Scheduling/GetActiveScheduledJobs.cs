using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.Scheduling
{
    /// <summary>
    /// This message is sent by an agent.
    /// It serves two purposes
    /// - reports agent status to the server
    /// - asks the server for a list of jobs for the agent
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
        /// <summary>
        /// list of performance counters at the agent
        /// </summary>
        public string[] PerfCounters { get; set; }
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
