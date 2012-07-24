using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using CogMon.Lib;

namespace CogMon.Services.RRD
{
    
    public class DataField
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public int? HeartbeatSeconds { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        /// <summary>
        /// W przypadku danych typu 'compute'
        /// </summary>
        public string Expression { get; set; }
    }

    

    /// <summary>
    /// Configuration of the round-robin archive
    /// </summary>
    public class RRAConfig
    {
        public RRAConfig()
        {
            XFilesFactor = 0.3;
        }

        public ConsolidationFunction Function { get; set; }
        /// <summary>
        /// Percentage (in the range between 0.0 and 1.0) of missing data values that still 
        /// give a valid aggregation
        /// </summary>
        public double XFilesFactor { get; set; }
        /// <summary>
        /// Number of steps (BaseInterval) to aggregate
        /// </summary>
        public int AggregateSteps { get; set; }
        /// <summary>
        /// Number of aggregated rows to store
        /// </summary>
        public int StoredRows { get; set; }
    }

    
    /// <summary>
    /// HWPREDICT configuration for a rrd
    /// </summary>
    public class HWConfig
    {
        public ConsolidationFunction Op { get; set; }
        public int Rows { get; set; }
        public double Alpha { get; set; }
        public double Beta { get; set; }
        public int SeasonalPeriod { get; set; }
    }

    public class RrdFileInfo
    {
        public string filename { get;set;}
        public string rrd_version { get;set;}
        public int step { get;set;}
        public int last_update { get;set;}
        public int header_size  { get;set;}

        public class DSInfo
        {
            public string id {get;set;}
            public int index { get;set;}
            public string type { get;set;}
            public int minimal_heartbeat { get;set;}
            public double min { get;set;}
            public double max { get;set;}
            public string last_ds { get;set;}
            public double value { get;set;}
            public int unknown_sec { get;set;}
        }

        public List<DSInfo> ds { get; set; }

        public class CdpPrep
        {
            public double value { get; set; }
            public int unknown_datapoints { get; set; }
        }

        public class RraInfo
        {
            public string cf { get;set;}
            public int rows { get;set;}
            public int cur_row { get;set;}
            public int pdp_per_row { get;set;}
            public double xff {get;set;}
            public List<CdpPrep> cdp_prep { get; set; }
        }

        public List<RraInfo> rra { get; set; }
    }

    public class RrdCreateInfo
    {
        /// <summary>
        /// Data series start time
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// Step seconds
        /// </summary>
        public int Step { get; set; }

        public List<DataField> Fields { get; set; }
        public List<RRAConfig> Aggregates { get; set; }
        public List<HWConfig> HWAggregates { get; set; }
        /// <summary>
        /// template used for creating the rrd
        /// </summary>
        public string TemplateId { get; set; }
    }

    public class RrdInfo
    {
        /// <summary>
        /// Id (def file name)
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// display name
        /// </summary>
        public string Name { get; set; }
        [JsonIgnore]
        public string FileName { get; set; }

        public string RRDFilePath { get; set; }
        public string Description { get; set; }
        public string FromTemplate { get; set; }
        /// <summary>
        /// Bazowa rozdzielczość wykresu, w sekundach
        /// </summary>
        public int BaseInterval { get; set; }
        public DataField[] DataFields { get; set; }
        public RRAConfig[] Aggregates { get; set; }

        public int GetRRAConsolidationPeriodSecs(RRAConfig cfg)
        {
            return this.BaseInterval * cfg.AggregateSteps;
        }

        public TimeSpan GetRRATimeSpan(RRAConfig cfg)
        {
            return TimeSpan.FromSeconds(this.BaseInterval * cfg.AggregateSteps * cfg.StoredRows);
        }



        public void Validate()
        {
            if (string.IsNullOrEmpty(Name)) throw new Exception("Name");
            //if (string.IsNullOrEmpty(RRDFilePath)) throw new Exception("RRDFilePath");
            if (BaseInterval <= 0) throw new Exception("BaseInterval");
            if (DataFields == null || DataFields.Length == 0) throw new Exception("DataFields");
            if (Aggregates == null || Aggregates.Length == 0) throw new Exception("Aggregates");
            foreach (var df in DataFields)
            {
                if (string.IsNullOrEmpty(df.Name)) throw new Exception("DataField name missing");
            }
            foreach (var agg in Aggregates)
            {
                if (agg.StoredRows <= 0) throw new Exception("Aggregate.StoredRows");
                if (agg.AggregateSteps <= 0) throw new Exception("Aggregate.AggregateSteps");
            }
        }

        public static RrdInfo ParseRrdInfoString(string s)
        {
            var sr = new StringReader(s);
            string ln;
            while ((ln = sr.ReadLine()) != null)
            {
                if (ln.StartsWith(";") || ln.StartsWith("#")) continue;
                int idx = ln.IndexOf('=');
                if (idx < 0) continue;
                string pname = ln.Substring(0, idx).Trim();
                string pval = ln.Substring(idx + 1).Trim();

            }
            throw new NotImplementedException();
        }
    }
}
