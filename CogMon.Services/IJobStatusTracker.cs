using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services
{
    

    public class JobStatusInfo
    {
        public string Id { get; set; }
        public string Group { get; set; }
        public string DataSeriesId { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? LastSuccessfulRun { get; set; }
        public string AgentAddress { get; set; }
        public bool IsError { get; set; }
        public string StatusInfo { get; set; }
        public int IntervalSeconds { get; set; }
        public int? LastExecTimeMs { get; set; }
        public string Description { get; set; }
    }

    public class DataSeriesStatusInfo
    {
        public string SeriesId { get; set; }
        public string Description { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string LastUpdateJob { get; set; }
    }

    public class AgentStatusInfo
    {
        public string Id { get; set; }
        public string AgentAddress { get; set; }
        public string Groups { get; set; }
        public string AgentPID { get; set; }
        public DateTime? LastSeen { get; set; }
        public string StatusInfo { get; set; }
    }

    public interface IJobStatusTracker
    {
        IEnumerable<JobStatusInfo> GetStatusOfAllJobs();
        IEnumerable<AgentStatusInfo> GetStatusOfAllAgents();
        IEnumerable<DataSeriesStatusInfo> GetStatusOfAllDataSeries();
    }
}
