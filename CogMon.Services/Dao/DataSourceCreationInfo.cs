using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services.Dao
{
    /// <summary>
    /// Information about a RRD file
    /// </summary>
    public class DataSourceCreationInfo 
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public string TemplateId { get; set; }
        /// <summary>
        /// path to rrd file (if not specified then Id is the file name)
        /// </summary>
        public string RrdPath { get; set; }
    }
}
