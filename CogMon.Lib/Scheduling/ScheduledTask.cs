using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.Scheduling
{
    public enum QueryType
    {
        HttpGet = 0,
        
        BooScript = 1,
        
        SystemCommand = 2,

        SqlSelect = 3,
        ///windows perf counters
        WinPerf = 4,
        /// <summary>
        /// parsing log files
        /// </summary>
        LogParse = 5,
        /// <summary>
        /// mongodb map-reduce
        /// </summary>
        MongoMapReduce = 6,
        /// <summary>
        /// inline boo script
        /// executed by the agent
        /// </summary>
        InlineScript = 7,
        /// <summary>
        /// Cogmon agent perf counter
        /// </summary>
        AgentPerfCnt = 8,
        /// <summary>
        /// Cogmon persistent perf counter
        /// stored in server db
        /// </summary>
        ServerPerfCnt = 9,
        /// <summary>ICMP or http ping</summary>
        Ping = 10
    }

    /// <summary>
    /// Scheduled data source query task
    /// </summary>
    public class ScheduledTask
    {
        /// <summary>
        /// Job ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Query interval in seconds
        /// </summary>
        public int IntervalSeconds { get; set; }
        /// <summary>
        /// Id of RRD database
        /// </summary>
        public string DataSeries { get; set; }
        /// <summary>
        /// Send time-stamp with each data record
        /// </summary>
        public bool SendTimestamp { get; set; }
        /// <summary>
        /// RRD input variable names
        /// </summary>
        public string[] Variables { get; set; }
        /// <summary>
        /// Regular expressions for capturing
        /// the variables in script output (this is entirely optional)
        /// </summary>
        public string[] VariableRegex { get; set; }
        /// <summary>
        /// Query method
        /// </summary>
        public QueryType QueryMethod { get; set; }
        /// <summary>
        /// Script name for boo/system commands
        /// request URL for http get
        /// sql select query for SqlSelect
        /// each task type has its own interpretation of this field
        /// </summary>
        public string ScriptName { get; set; }
        /// <summary>
        /// Script arguments
        /// Connection string name for SqlSelect
        /// </summary>
        public string Arguments { get; set; }
        /// <summary>
        /// Job-specific configuration options
        /// </summary>
        public Dictionary<string, object> Options { get; set; }
        /// <summary>
        /// Job version tag (for detecting modifications)
        /// </summary>
        public string VersionTag { get; set; }
        /// <summary>
        /// Job group
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Last run information
        /// Used for calculating next run time
        /// </summary>
        public DateTime LastRun { get; set; }
    }
}
