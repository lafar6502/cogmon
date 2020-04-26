using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NGinnBPM.MessageBus;
using CogMon.Lib;
using System.IO;
using System.Globalization;
using CogMon.Lib.Scheduling;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CogMon.Agent
{
    public abstract class JobBase : ScheduledTask
    {
        protected Logger log = LogManager.GetCurrentClassLogger();

        public ITimeSeriesDatabase TsDb { get; set; }

        private Dictionary<string, object> _jobstate = null;
        protected DateTime _execStart;

        protected Dictionary<string, object> LoadJobState()
        {
            string fn = GetPersistFilePath();
            if (!File.Exists(fn)) return new Dictionary<string,object>();
            var ser = new JsonSerializer();
            using (var sr = new StreamReader(fn))
            {
                return ser.Deserialize<Dictionary<string, object>>(new JsonTextReader(sr));
            }
        }

        protected void SaveJobState(Dictionary<string, object> state)
        {
            string fn = GetPersistFilePath();
            if (state == null || state.Count == 0)
            {
                if (File.Exists(fn)) File.Delete(fn);
                return;
            }
            string d = Path.GetDirectoryName(fn);
            if (!Directory.Exists(d)) Directory.CreateDirectory(d);
            var ser = new JsonSerializer();
            using (var sw = new StreamWriter(fn, false, Encoding.UTF8))
            {
                ser.Serialize(new JsonTextWriter(sw), state);
            }
        }

        public void Execute()
        {
            try
            {
                log = LogManager.GetLogger("Job" + this.Id);
                NLog.MappedDiagnosticsContext.Set("jobid", this.Id);
                _execStart = DateTime.Now;
                _jobstate = LoadJobState();
                Run();
                SaveJobState(_jobstate);
            }
            catch (Exception ex)
            {
                log.Warn("Error executing job {0} ({2}): {1}", Id, ex.ToString(), this.GetType().Name);
                throw;
            }
            finally
            {
                NLog.MappedDiagnosticsContext.Remove("jobid");
            }
        }

        protected abstract void Run();

        protected static double ParseVal(string pt)
        {
            double v;
            if (double.TryParse(pt, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                return v;
            else if ("NaN".Equals(pt) || "U".Equals(pt))
                return double.NaN;
            else
                throw new Exception("Unrecognized value: " + pt);
        }

        /// <summary>
        /// reads command output data and converts it to a DataRecord
        /// returns null if no values are present in the output
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        protected DataRecord ParseData(TextReader tr)
        {
            DataRecord dr = new DataRecord();
            dr.Series = this.DataSeries;
            if (SendTimestamp) dr.Ts = DateTime.Now;
            List<string> flds = new List<string>();
            List<double> vals = new List<double>();

            if (this.VariableRegex != null && this.VariableRegex.Length > 0)
            {
                if (this.Variables != null && this.Variables.Length != this.VariableRegex.Length)
                    throw new Exception("Invalid configuration of variable names and variable regular expressions");
                var str = tr.ReadToEnd();

                for (int i = 0; i < VariableRegex.Length; i++)
                {
                    var re = VariableRegex[i];
                    Match m = Regex.Match(str, re);

                    if (m.Success)
                    {
                        var v = ParseVal(m.Groups[m.Groups.Count > 1 ? 1 : 0].Captures[0].Value);
                        vals.Add(v);
                    }
                    else
                    {
                        log.Info("Failed to match {0} in text {1}", re, str);
                        vals.Add(Double.NaN);
                    }
                    if (Variables != null && Variables.Length == VariableRegex.Length)
                        flds.Add(Variables[i]);
                }
            }
            else
            {
                string line;


                while ((line = tr.ReadLine()) != null)
                {
                    if (line.StartsWith(";") || line.StartsWith("#")) continue;
                    if (line.Trim().Length == 0) continue;
                    string[] parts = line.Split(' ', '\t');

                    foreach (string pt in parts)
                    {
                        int idx = pt.IndexOf(':');
                        if (idx < 0)
                        {
                            vals.Add(ParseVal(pt));
                        }
                        else
                        {
                            string n = pt.Substring(0, idx);
                            double v = ParseVal(pt.Substring(idx + 1));
                            flds.Add(n);
                            vals.Add(v);
                        }
                    }
                }
            }
            var dm = System.Configuration.ConfigurationManager.AppSettings["DataMap"];
            bool d = "true".Equals(dm, StringComparison.InvariantCultureIgnoreCase);
            if (flds.Count == vals.Count || (flds.Count > 0 && d))
            {
                dr.DataMap = new Dictionary<string, double>();
                for (int i = 0; i < flds.Count; i++)
                {
                    dr.DataMap[flds[i]] = vals[i];
                }
            }
            else if (vals.Count > 0 && flds.Count == 0)
            {
                dr.Data = vals.ToArray();
            }
            else if (vals.Count == 0 && flds.Count == 0)
            {
                return null;
            }
            else throw new Exception("Invalid data format");
            return dr;
        }

        protected void UpdateDataSource(DataRecord dr)
        {
            UpdateDataSource(new DataRecord[] { dr });
            
        }

        protected void UpdateDataSource(IEnumerable<DataRecord> batch)
        {
            foreach(var dr in batch)
            {
                if (string.IsNullOrEmpty(dr.Series)) dr.Series = this.DataSeries ?? this.Id;
                dr.Tags = GetTags();
            }
            log.Info("Job {0} Updating ds {1}, tags: {2}", this.Id, this.DataSeries, string.Join(", ", GetTags().Keys));
            TsDb.UpdateDataSource(batch);
        }

        protected Dictionary<string, object> GetTags()
        {
            var dic = new Dictionary<string, object>();
            if (this.Options != null)
            {
                foreach(var kv in this.Options)
                {
                    if (kv.Key.StartsWith("~"))
                    {
                        dic[kv.Key.Substring(1)] = kv.Value;
                    }
                }
            }
            return dic;
        }


        protected string GetPersistFilePath()
        {
            string bd = System.AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(bd, "..", "jobdata", this.Id + ".json");
        }

        public T GetJobPersistValue<T>(string name, T defaultVal)
        {
            if (_jobstate == null) throw new Exception("job state missing");
            return _jobstate.ContainsKey(name) ? (T)Convert.ChangeType(_jobstate[name], typeof(T)) : defaultVal;
        }

        public void SetJobPersistValue(string name, object value)
        {
            _jobstate.Remove(name);
            _jobstate[name] = value;
        }
    }
}
