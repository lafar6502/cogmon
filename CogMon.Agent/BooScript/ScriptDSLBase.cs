using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Data.Common;
using BL = Boo.Lang;
using NLog;
using CogMon.Lib;
using System.Configuration;

namespace CogMon.Agent.BooScript
{
    public abstract class ScriptDSLBase
    {

        public ScriptDSLBase()
        {
            DataRecords = new List<DataRecord>();
            Out = new StringWriter();
        }

        public abstract void Prepare();

        public virtual void Initialize(IDictionary<string, object> inputParams)
        {
            log = LogManager.GetLogger(this.GetType().Name);
            InputParams = new QuackW(fld => inputParams[fld]);
            Prepare();
        }

        public List<DataRecord> DataRecords { get; set; }
        protected BL.IQuackFu InputParams { get; set; }
        protected TextWriter Out { get; set; }
        protected Logger log;

        protected void search_solr(string query)
        {
            
        }

        protected string http_get_text(string url)
        {
            return Tools.HttpUtil.Download(url);
        }

        protected void access_db(string connStr, Action<DbConnection> act)
        {
            string cs = connStr;
            string provider = "System.Data.SqlClient";
            var cc = ConfigurationManager.ConnectionStrings[connStr];
            if (cc != null)
            {
                provider = string.IsNullOrEmpty(cc.ProviderName) ? provider : cc.ProviderName;
                cs = cc.ConnectionString;
            }
            using (DbConnection conn = DbProviderFactories.GetFactory(provider).CreateConnection())
            {
                conn.ConnectionString = cs;
                conn.Open();
                act(conn);
            }
        }

        protected void sql_select(string connStr, string query, Action<BL.IQuackFu> act)
        {
            DbDataReader dr = null;
            var qw = new QuackW(fld =>
            {
                int ord = dr.GetOrdinal(fld);
                return dr.IsDBNull(ord) ? null : dr[ord];
            });
            access_db(connStr, delegate(DbConnection conn)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    using (dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            act(qw);
                        }
                    }
                }
            });
        }

        protected void add_data_record(params double[] values)
        {
            DataRecord dr = new DataRecord
            {
                Data = values
            };
            DataRecords.Add(dr);
        }

        protected void add_data_record(IDictionary<string, double> values)
        {
            DataRecord dr = new DataRecord
            {
                DataMap = new Dictionary<string, double>(values)
            };
            DataRecords.Add(dr);
        }

        protected void add_data_record(DateTime timestamp, double[] values)
        {
            DataRecord dr = new DataRecord
            {
                Data = values,
                Ts = timestamp
            };
            DataRecords.Add(dr);
        }

        protected void add_data_record(DateTime timestamp, IDictionary<string, double> values)
        {
            DataRecord dr = new DataRecord
            {
                Ts = timestamp,
                DataMap = new Dictionary<string, double>(values)
            };
            DataRecords.Add(dr);
        }

        protected void add_data_record(DataRecord dr)
        {
            DataRecords.Add(dr);
        }

        protected void add_data_record(BL.Hash values)
        {
            Dictionary<string, double> d = new Dictionary<string, double>();
            foreach (string k in values.Keys) d[k] = Convert.ToDouble(values[k]);
            add_data_record(d);
        }

        protected void add_data_record(DateTime timestamp, BL.Hash values)
        {
            Dictionary<string, double> d = new Dictionary<string, double>();
            foreach (string k in values.Keys) d[k] = Convert.ToDouble(values[k]);
            add_data_record(timestamp, d);
        }

        
        
    }

    internal class QuackW: BL.IQuackFu
    {
        private Func<string, object> _getter;
        public QuackW(Func<string, object> getter)
        {
            _getter = getter;
        }


        public object QuackGet(string name, object[] parameters)
        {
            return _getter(name);
        }

        public object QuackInvoke(string name, params object[] args)
        {
            throw new NotImplementedException();
        }

        public object QuackSet(string name, object[] parameters, object value)
        {
            throw new NotImplementedException();
        }
    }
}
