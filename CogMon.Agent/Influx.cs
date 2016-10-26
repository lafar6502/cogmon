using CogMon.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfluxData.Net;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb.Models;
using NLog;

namespace CogMon.Agent
{
    //c:\tools\curl -i -XPOST http://10.10.104.30:8086/write?db=atmo --data-binary "cpu_load_short,host=server01,region=us-west value=0.64 1434055562000000000"
    public class Influx : ITimeSeriesDatabase
    {
        public string ServiceUrl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        /// <summary>
        /// database name
        /// </summary>
        public string DbName { get; set; }
        public string InfluxVersion { get; set; }

        private InfluxDbClient _cli;

        private static Logger log = LogManager.GetCurrentClassLogger();

        protected InfluxDbClient GetInflux()
        {
            if (_cli != null) return _cli;

            var ver = InfluxDbVersion.Latest;
            if (!string.IsNullOrEmpty(InfluxVersion))
            {
                ver = (InfluxDbVersion)Enum.Parse(typeof(InfluxDbVersion), InfluxVersion);
            }
            _cli = new InfluxDbClient(ServiceUrl, User, Password, ver);
            return _cli;
        }

        private Point toInfluxPoint(DataRecord dr)
        {
            if (dr.DataMap == null) throw new Exception("DataMap required");
            var f = new Dictionary<string, object>(dr.DataMap.Count);

            foreach (var kv in dr.DataMap) f[kv.Key] = kv.Value;

            return new Point
            {
                Name = dr.Series,
                Tags = dr.Tags ?? new Dictionary<string, object>(),
                Fields = f,
                Timestamp = dr.Ts
            };
        }

        public void UpdateDataSource(IEnumerable<DataRecord> batch)
        {
            if (batch == null || batch.Count() == 0) return;
            try
            {
                var dr = batch.First();
                var cli = GetInflux();
                var resp = cli.Client.WriteAsync(DbName, batch.Select(toInfluxPoint));
                resp.Wait();

                log.Debug("Updated data source {0}, tags: [{1}], data: [{2}], {3} points", dr.Series, 
                    string.Join(", ", dr.Tags.Select(x => string.Format("{0}:{1}", x.Key, x.Value))),
                    string.Join(", ", dr.DataMap.Select(x => string.Format("{0}:{1}", x.Key, x.Value))), 
                    batch.Count());
            }
            catch(Exception ex)
            {
                log.Error("Error updating data source: {0}", ex.ToString());
                throw;
            }
        }
    }
}
