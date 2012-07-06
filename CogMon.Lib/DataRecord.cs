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


        public DateTime? Ts { get; set; }
        public string Series { get; set; }
        public double[] Data { get; set; }
        public Dictionary<string, double> DataMap { get; set; }
    }

    public class UpdateData
    {
        public DataRecord Data { get; set; }
        public string JobId { get; set; }
    }

    public class UpdateDataBatch
    {
        public DataRecord[] Data { get; set; }
        public string JobId { get; set; }
    }

}
