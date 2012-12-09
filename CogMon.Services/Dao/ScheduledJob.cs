using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib.Scheduling;

namespace CogMon.Services.Dao
{
    

    public class ScheduledJob
    {
        public ScheduledJob()
        {
            LastModified = DateTime.MinValue;
        }

        public string Id { get; set; }
        /// <summary>
        /// Data series
        /// </summary>
        public string DataSource { get; set; }
        /// <summary>
        /// Data source template Id...
        /// </summary>
        public string TemplateId { get; set; }
        /// <summary>
        /// Job is active?
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Repeat interval, in seconds
        /// </summary>
        public int IntervalSeconds { get; set; }
        /// <summary>
        /// Data query method
        /// </summary>
        public virtual QueryType QueryMethod { get; set; }
        /// <summary>
        /// Script name or URL to be called
        /// </summary>
        public virtual string ScriptName { get; set; }
        /// <summary>
        /// Script arguments
        /// </summary>
        public virtual string Arguments { get; set; }
        
        /// <summary>
        /// Error information, if the job fails
        /// </summary>
        public string ErrorInfo { get; set; }
        /// <summary>
        /// last modification date
        /// </summary>
        public DateTime LastModified { get; set; }
        /// <summary>
        /// Variables in data source
        /// </summary>
        public string[] VariableNames { get; set; }
        /// <summary>
        /// Regular expressions for retrieving the values
        /// </summary>
        public string[] VariableRetrieveRegex { get;set;}
        /// <summary>
        /// Command-specific options
        /// </summary>
        public Dictionary<string, object> Options { get; set; }
        /// <summary>
        /// Job group
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Last execution time
        /// </summary>
        public DateTime LastRun { get; set; }
        /// <summary>Job description</summary>
        public string Description { get; set; }
    }
}
