using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Data.Common;
using System.Configuration;
using CogMon.Lib;

namespace CogMon.Agent
{
    /// <summary>
    /// SqlSelect task runs an sql select query.
    /// Every row returned by the query is treated as a data record
    /// Column names should match data field names in the rrd file.
    /// If there is a column named 'timestamp' it should contain a timestamp for the record.
    /// Otherwise 'now' is assumed.
    /// </summary>
    public class SqlSelectTask : JobBase
    {
        protected override void Run()
        {
            string cs = this.Arguments;

            var cstring = string.IsNullOrEmpty(cs) ? ConfigurationManager.ConnectionStrings[0] : ConfigurationManager.ConnectionStrings[cs];
            if (cstring == null) throw new Exception("Connection string not found (Arguments: " + cs + ")");
            string pn = string.IsNullOrEmpty(cstring.ProviderName) ? "System.Data.SqlClient" : cstring.ProviderName;
            if (string.IsNullOrEmpty(this.ScriptName)) throw new Exception("SQL not defined (ScriptName parameter)");

            List<DataRecord> lst = new List<DataRecord>();
            using (var cn = DbProviderFactories.GetFactory(pn).CreateConnection())
            {
                cn.ConnectionString = cstring.ConnectionString;
                cn.Open();
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = this.ScriptName;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var rec = new DataRecord { Series = this.DataSeries, DataMap = new Dictionary<string,double>() };
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                string fn = dr.GetName(i);
                                if (fn == "timestamp")
                                {
                                    rec.Ts = dr.GetDateTime(i);
                                }
                                else
                                {
                                    rec.DataMap[fn] = dr.IsDBNull(i) ? double.NaN : Convert.ToDouble(dr[i]);
                                }
                            }
                            lst.Add(rec);
                        }
                    }
                }
            }
            if (lst.Count == 0)
            {
                log.Warn("Job {0}: No data returned by sql query", this.Id);
                return;
            }
            UpdateDataSource(lst);
        }
    }
}
