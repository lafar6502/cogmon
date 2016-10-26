using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib
{
    /// <summary>
    /// Data sample
    /// </summary>
    public class DataRecord
    {
        public DataRecord()
        {
        }

        public DataRecord(string series, double[] data)
        {
            Series = series;
            Data = data;
        }

        public DataRecord(string series, DateTime ts, double[] data)
        {
            Series = series;
            Data = data;
            Ts = ts;
        }

        public DataRecord(string series, DateTime ts, IDictionary<string, double> data)
        {
            Series = series;
            DataMap = new Dictionary<string, double>(data);
            Ts = ts;
        }

        /// <summary>optional timestamp</summary>
        public DateTime? Ts { get; set; }
        /// <summary>Data source Id </summary>
        public string Series { get; set; }
        /// <summary>Data in array form (the order of values corresponds to the order of data series in rrd file)</summary>
        public double[] Data { get; set; }
        /// <summary>Data in name=>value form</summary>
        public Dictionary<string, double> DataMap { get; set; }
        /// <summary>
        /// extra tags for the data point,
        /// used only in influx
        /// </summary>
        public Dictionary<string, object> Tags { get; set; }
    }

    /// <summary>
    /// Data update message sent by collector agents to CogMon server
    /// </summary>
    public class UpdateData
    {
        public DataRecord Data { get; set; }
        /// <summary>Job that collected the data</summary>
        public string JobId { get; set; }
        /// <summary>Job run time (for detecting slow running jobs)</summary>
        public int JobExecTimeMs { get; set; }
    }

    public class UpdateDataBatch
    {
        public DataRecord[] Data { get; set; }
        public string JobId { get; set; }
    }

}
