using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.IO;
using Newtonsoft.Json;
using CogMon.Lib.DataSeries;
using CogMon.Services.Dao;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using CogMon.Lib;
using CogMon.Lib.Graph;
using System.Collections.Concurrent;
using System.Timers;

namespace CogMon.Services.RRD
{
    

    public class DataSeriesRepository : IDataSeriesRepository, IDisposable
    {
        public string BaseDirectory { get; set; }
        public IRrdTool RRDTool { get; set; }
        private Logger log = LogManager.GetCurrentClassLogger();
        public MongoDatabase Db { get; set; }
        public string TempDirectory { get; set; }


        public DataSeriesRepository()
        {
            TempDirectory = Path.GetTempPath();
        }



        private Dictionary<string, DataSeriesInfo> _dscache = new Dictionary<string, DataSeriesInfo>();

        public DataSeriesInfo GetDataSeries(string id, bool withCurrentFileInformation)
        {
            DataSeriesInfo dsi;
            if (!withCurrentFileInformation)
            {
                if (_dscache.TryGetValue(id, out dsi)) return dsi;
            }
            string fn = Path.Combine(BaseDirectory, id + ".rrd");
            if (!File.Exists(fn)) throw new Exception("RRD file not found: " + id);
            var ri = RRDTool.GetRrdInfoFromFile(fn);
            
            dsi = new DataSeriesInfo
            {
                Id = Path.GetFileNameWithoutExtension(ri.filename),
                Name = Path.GetFileNameWithoutExtension(ri.filename)
            };
            foreach (var fd in ri.ds)
            {
                dsi.Fields.Add(new DataFieldInfo { Name = fd.id, Value = fd.value});
            }
            dsi.LastUpdate = RrdTool.FromUnixTime(ri.last_update);
            dsi.StepSeconds = ri.step;

            var dci = Db.GetCollection<DataSourceCreationInfo>().FindOneById(id);
            if (dci != null)
            {
                dsi.Description = dci.Description;
            }
            _dscache.Remove(id);
            _dscache[id] = dsi;
            return dsi;
        }

        public void AppendData(Lib.DataRecord dr)
        {
            DateTime dt = DateTime.Now;
            string    dataPath = dr.Series.EndsWith(".rrd") ? dr.Series : dr.Series + ".rrd";
            string df = Path.Combine(BaseDirectory, dataPath);
            if (!File.Exists(df))
            {
                throw new Exception("Data file does not exist: " + dataPath);
            }
            double[] data = dr.Data;
            if (data == null && dr.DataMap != null)
            {
                RRDTool.AppendData(df, dr.Ts, dr.DataMap);
            }
            else
            {
                RRDTool.AppendData(df, dr.Ts, data);
            }
            NotifyRrdUpdated(dr.Series);
            log.Debug("Finished updating {0}. Time: {1}", dr.Series, DateTime.Now - dt);
            
        }

        protected void NotifyRrdUpdated(string rrd)
        {
            
        }

        private static T SubstTemplate<T>(string tpl, T defaultValue, Dictionary<string, object> variables)
        {
            return Utils.SubstTemplate<T>(tpl, defaultValue, variables);   
        }


        public DataSeriesInfo ReCreateDataSeries(string seriesId, string userId)
        {
            var dci = Db.GetCollection<DataSourceCreationInfo>().FindOneById(seriesId);
            if (dci == null) throw new Exception("Database creation info not found: " + seriesId);

            var rq = new CreateDataSeriesFromTemplate
            {
                OverwriteDataSourceId = seriesId,
                Parameters = dci.Variables,
                TemplateId = dci.TemplateId
            };
            return CreateDataSeriesFromTemplate(rq, userId);
        }

        public DataSeriesInfo CreateDataSeriesFromTemplate(CreateDataSeriesFromTemplate request, string userId)
        {
            var tpl = Db.GetCollection<DataSourceTemplate>().FindOneById(request.TemplateId);
            if (tpl == null) throw new Exception("Template not found: " + request.TemplateId);

            DataSourceCreationInfo dci = null;
            bool alreadyExist = false;
            if (!string.IsNullOrEmpty(request.OverwriteDataSourceId))
            {
                dci = Db.GetCollection<DataSourceCreationInfo>().FindOneById(request.OverwriteDataSourceId);
                alreadyExist = true;
                if (dci == null) throw new Exception("No database information found to overwrite: " + request.OverwriteDataSourceId);
            }

            var variables = new Dictionary<string, object>();
            variables["step"] = "${step}";
            variables["width"] = "${width}";
            variables["height"] = "${height}";
            foreach (var k in request.Parameters)
            {
                variables.Remove(k.Key);
                variables[k.Key] = k.Value;
            }
            foreach (var vd in tpl.Variables)
            {
                if (!variables.ContainsKey(vd.Name))
                {
                    if (!string.IsNullOrEmpty(vd.DefaultValue)) variables[vd.Name] = vd.DefaultValue;
                }
            }

            if (dci == null)
            {
                dci = new DataSourceCreationInfo();
                dci.TemplateId = tpl.Id;
                dci.CreatedDate = DateTime.Now;
                dci.Description = SubstTemplate<string>(tpl.Description, null, variables);
                dci.Variables = variables;
            }
            else
            {
                dci.TemplateId = tpl.Id;
                dci.CreatedDate = DateTime.Now;
                dci.Description = SubstTemplate<string>(tpl.Description, null, variables);
                dci.Variables = variables;
            }
            
            RrdCreateInfo rci = new RrdCreateInfo();
            rci.TemplateId = tpl.Id;
            rci.Step = SubstTemplate<int>(tpl.Step, 300, variables);
            rci.Fields = new List<DataField>();
            rci.StartTime = SubstTemplate<string>(tpl.StartTime, null, variables); 
            foreach (SeriesTemplate dst in tpl.Series)
            {
                DataField df = new DataField();
                df.Name = SubstTemplate<string>(dst.Name, null, variables);
                df.HeartbeatSeconds = SubstTemplate<int>(dst.Heartbeat, 3600, variables);
                df.SeriesType = SubstTemplate<SeriesTypes>(dst.SeriesType, SeriesTypes.GAUGE, variables);
                df.Min = SubstTemplate<double>(dst.Min, double.NaN, variables);
                df.Max = SubstTemplate<double>(dst.Max, double.NaN, variables);
                df.Expression = SubstTemplate<string>(dst.Expression, null, variables);
                rci.Fields.Add(df);
            }
            rci.Aggregates = new List<RRAConfig>();
            foreach (RRATemplate rrat in tpl.Aggregates)
            {
                RRAConfig rra = new RRAConfig();
                rra.AggregateSteps = Convert.ToInt32(rrat.AggregateSteps);
                rra.Function = SubstTemplate<ConsolidationFunction>(rrat.ConsolidationFunction, ConsolidationFunction.AVERAGE, variables);
                rra.StoredRows = SubstTemplate<int>(rrat.StoredRows, 100, variables);
                rra.XFilesFactor = SubstTemplate<double>(rrat.XFilesFactor, 0.9, variables);
                rci.Aggregates.Add(rra);
            }
            string tf = null;
            string dsid = null;
            try
            {
                Db.GetCollection<DataSourceCreationInfo>().Save(dci);
                dsid = dci.Id;
                tf = Path.Combine(BaseDirectory, dsid + ".rrd");
                RRDTool.CreateRRDFile(rci, tf);
                if (tpl.CreateSchedule)
                {
                    ScheduledJob sj = null;
                    if (alreadyExist)
                    {
                        sj = Db.Find<ScheduledJob>(x => x.DataSource == dci.Id && x.Active == true).FirstOrDefault();
                    }
                    string[] vregex = null;
                    if (tpl.VariableValueRegex != null && tpl.VariableValueRegex.Count > 0)
                    {
                        vregex = new List<string>(tpl.VariableValueRegex.Select(x => SubstTemplate<string>(x, null, variables))).ToArray();
                    }
                    
                    if (sj == null) sj = new ScheduledJob 
                    {
                        Active = true,
                        DataSource = dsid,
                        TemplateId = tpl.Id
                    };
                        
                    sj.Arguments = SubstTemplate<string>(tpl.Arguments, null, variables);
                    sj.ScriptName = SubstTemplate<string>(tpl.ScriptOrUrl, null, variables);
                    sj.IntervalSeconds = SubstTemplate<int>(tpl.IntervalSecs, 300, variables);
                    sj.QueryMethod = tpl.Query;
                    sj.VariableNames = new List<string>(rci.Fields.Where(x => x.SeriesType != SeriesTypes.COMPUTE).Select(x => x.Name)).ToArray();
                    sj.VariableRetrieveRegex = vregex;
                    sj.LastModified = DateTime.Now;
                    if (tpl.Options != null)
                    {
                        sj.Options = new Dictionary<string, object>();
                        foreach (string k in tpl.Options.Keys)
                        {
                            sj.Options[k] = SubstTemplate<string>(tpl.Options[k], null, variables);
                        }
                    }
                    else
                    {
                        sj.Options = request.Parameters;
                    }
                    Db.GetCollection<ScheduledJob>().Save(sj);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error creating data source: {0}", ex.ToString());
                try
                {
                    if (!string.IsNullOrEmpty(dci.Id)) Db.GetCollection<DataSourceCreationInfo>().Remove(Query.EQ("_id", dci.Id));
                    if (File.Exists(tf)) File.Delete(tf);
                }
                catch (Exception e2) { log.Warn("Clean-up error: {0}", e2); }
                throw;
            }
            return this.GetDataSeries(dsid, false);
        }

        public IEnumerable<DataSourceRef> DataSources
        {
            get 
            {
                List<DataSourceRef> ret = new List<DataSourceRef>();
                var fl = Directory.GetFiles(BaseDirectory, "*.rrd");
                var lst = fl.Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
                var col = Db.Find<DataSourceCreationInfo>(x => x._id.In(lst)).ToList();
                foreach (string s in lst)
                {
                    var dr = new DataSourceRef();
                    dr.Id = s;
                    var ci = col.Find(x => x.Id == s);
                    if (ci != null)
                    {
                        dr.Description = ci.Description;
                    }
                    ret.Add(dr);
                }
                return ret;
            }
        }


        protected IList<string> GetEventCategoriesForGraph(IList<string> drawEventCategories, GraphDefinition gd)
        {
            if (drawEventCategories == null || (drawEventCategories.Count == 1 && string.IsNullOrEmpty(drawEventCategories[0]))) return gd.EventCategories;
            return drawEventCategories;
        }

        protected static int SelectReasonableStep(DateTime start, DateTime end)
        {
            TimeSpan ts = end - start;
            if (ts.TotalMinutes <= 60) return 1;
            if (ts.TotalMinutes <= 240) return 300;
            if (ts.TotalHours <= 24) return 1800;
            if (ts.TotalHours <= 72) return 3600;
            return 24 * 3600;
        }


        public RrdImageInfo CreateGraph(Lib.Graph.GraphDefinition gd, DrawOptions opts, string destinationFile)
        {
            foreach (var d in gd.Defs)
            {
                var p = Path.Combine(BaseDirectory, d.DataSourceId + ".rrd");
                d.DataSourceId = p.Replace(":\\", "\\:\\");
            }
            DateTime s, e;
            if (!RrdUtil.ParseRrdDateRange(opts.StartTime, opts.EndTime, out s, out e)) throw new Exception("Unparsable graph date range");
            int actualStep = 1;
            if (opts.Step.HasValue) actualStep = opts.Step.Value;
            if (actualStep == 0)
            {
                actualStep = SelectReasonableStep(s, e);
            }
            if (gd.Resolution != null && actualStep > 0)
            {
                for (int i=0; i<gd.Resolution.Count; i++)
                {
                    var ri = gd.Resolution[i];
                    var dl = DateTime.Now.AddSeconds(-ri.SpanSec - 1);
                    log.Debug("res: {0}, l1: {1}, s: {2}, lt: {3}, t1: {4}, t2: {5}", ri.ResSec, dl, s, dl < s, dl.Ticks, s.Ticks);
                    if (i == gd.Resolution.Count - 1 || dl < s)
                    {
                        if (actualStep < ri.ResSec) actualStep = ri.ResSec;
                        log.Debug("Selected resolution {0}", ri.ResSec);
                        break;
                    }
                }
            }
            log.Debug("Setting step size to {0}", actualStep);
            opts.Step = actualStep > 0 ? actualStep : (int?) null;
            IList<string> eventCats = GetEventCategoriesForGraph(opts.EventCategories, gd);
            List<RrdEventInfo> eventlst = null;
            Dictionary<string, EventInfo> dbevents = null;
            if (eventCats != null && eventCats.Count > 0)
            {
                var allCats = CogmonDbUtil.GetAllEventCategories(Db);
                eventlst = new List<RrdEventInfo>();
                dbevents = new Dictionary<string, EventInfo>();
                var evs = Db.Find<EventInfo>(x => x.Timestamp > s && x.Timestamp < e && x.Category.In(eventCats)).OrderBy(x => x.Timestamp).ToList();
                foreach (var ev in evs)
                {
                    dbevents[ev.Id] = ev;
                    EventCategory cat;
                    allCats.TryGetValue(ev.Category, out cat);
                    eventlst.Add(new RrdEventInfo { EventId = ev.Id, Color = cat != null ? cat.Color : "FF0000AA", Time = RrdUtil.ToUnixTime(ev.Timestamp)});
                }
                log.Info("Events: {0}", evs.Count);
            }
            var ret = this.RRDTool.DrawGraph(gd, opts, eventlst, destinationFile);
            log.Info("s: {0}|{1}, e: {2}|{3}", s, ret.Start, e, ret.End);
            if (ret.Events != null)
            {
                foreach (var e2 in ret.Events)
                {
                    var ev = dbevents[e2.EventId];
                    e2.Category = ev.Category;
                    e2.Text = ev.Label;
                }
            }
            return ret;
        }


        public RrdImageInfo CreateGraph(Lib.Graph.GraphDefinition gd, Lib.Graph.DrawOptions options, Stream output)
        {
            string df = Path.Combine(TempDirectory, Guid.NewGuid().ToString("N") + ".png");
            var gi = CreateGraph(gd, options, df);
            if (output != null)
            {
                using (var fs = new FileStream(df, FileMode.Open))
                {
                    fs.CopyTo(output);
                }
            }
            File.Delete(df);
            gi.FileName = null;
            return gi;
        }

        public RrdImageInfo CreateGraph(string definitionId, Lib.Graph.DrawOptions options, Stream output)
        {
            var gdef = Db.GetCollection<GraphDefinition>().FindOneById(definitionId);
            if (gdef == null) throw new Exception("Graph definition not found: " + gdef);
            return CreateGraph(gdef, options, output);
        }

        protected class EventCounter
        {
            public int Time { get; set; }
            public double Sum { get; set; }
        }

        private ConcurrentDictionary<string, EventCounter> _dic = new ConcurrentDictionary<string, EventCounter>();
        private Timer _flusher = null;

        protected void DoFlushEvent(string id)
        {
            EventCounter ei;
            if (_dic.TryRemove(id, out ei) && ei != null)
            {
                log.Debug("Flushing event {0}, time: {1}, sum: {2}", id, ei.Time, ei.Sum);
                this.AppendData(new DataRecord { Series = id, Ts = RrdUtil.FromUnixTime(ei.Time), Data = new double[] { ei.Sum } });
            }
        }

        public void AddEvent(string id, double? value)
        {
            DataRecord dr = null;
            var now = RrdUtil.ToUnixTime(DateTime.Now);
            bool doFlush = false;

            lock (_dic)
            {
                _dic.AddOrUpdate(id,
                    delegate(string x)
                    {
                        doFlush = true;
                        return new EventCounter { Time = now, Sum = value.HasValue ? value.Value : 1.0 };
                    },
                    delegate(string x, EventCounter r)
                    {
                        if (r.Time == now)
                        {
                            r.Sum += (value.HasValue ? value.Value : 1.0);
                            //log.Debug("Accumulating event {0}, time: {1}, sum: {2}", id, r.Time, r.Sum);
                        }
                        else
                        {
                            //log.Debug("Flushing event {0}, time: {1}, sum: {2}", id, r.Time, r.Sum);
                            dr = new DataRecord { Series = id, Ts = RrdUtil.FromUnixTime(r.Time), Data = new double[] { r.Sum } };
                            doFlush = true;
                            r.Time = now;
                            r.Sum = value.HasValue ? value.Value : 1.0;
                        }
                        return r;
                    }
                );
                
            }
            if (dr != null)
            {
                AppendData(dr);
            }
            
            if (doFlush)
            {

                //log.Info("I should flush now {0}", id);
                lock (this)
                {
                    if (_flusher == null)
                    {
                        _flusher = new Timer(TimeSpan.FromSeconds(5).TotalMilliseconds);
                        _flusher.Elapsed += new ElapsedEventHandler(_flusher_Elapsed);
                        _flusher.AutoReset = true;
                        _flusher.Start();
                    }
                }
                //System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(x => DoFlushEvent((string)x)));
            };
        }

        void _flusher_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_dic.Count == 0) return;
            var now = RrdUtil.ToUnixTime(DateTime.Now);
            List<DataRecord> lst = new List<DataRecord>();

            lock (_dic)
            {
                foreach (var v in _dic)
                {
                    if (v.Value.Time < now)
                    {
                        lst.Add(new DataRecord { Series = v.Key, Ts = RrdUtil.FromUnixTime(v.Value.Time), Data = new double[] { v.Value.Sum } });
                    }
                }
                foreach (var dr in lst)
                {
                    EventCounter ec;
                    _dic.TryRemove(dr.Series, out ec);
                }
            }
            if (lst.Count > 0) log.Info("Flushing data for {0} series", lst.Count);
            foreach (var dr in lst)
            {
                AppendData(dr);
            }
        }

        public void Dispose()
        {
            if (_flusher != null)
            {
                _flusher.Dispose();
                _flusher = null;
            }
        }


        public IList<double> GetLastDataValues(IEnumerable<KeyValuePair<string, string>> dataSeries)
        {
            Dictionary<string, DataSeriesInfo> dic = new Dictionary<string, DataSeriesInfo>();
            List<double> ret = new List<double>();
            foreach (var kv in dataSeries)
            {
                DataSeriesInfo si;
                if (!dic.TryGetValue(kv.Key, out si))
                {
                    si = this.GetDataSeries(kv.Key, true);
                    dic[kv.Key] = si;
                }
                var fi = si.Fields.Find(x => x.Name == kv.Value);
                if (fi == null) throw new Exception(string.Format("Field {0} not found in rrd {1}", kv.Value, kv.Key));
                ret.Add(fi.Value);
            }
            return ret;
        }


        public RrdDataXport ExportGraphData(string graphDefinitionId, DrawOptions opts)
        {
            var gd = Db.GetCollection<GraphDefinition>().FindOneById(graphDefinitionId);
            if (gd == null) throw new Exception("Invalid graph definition Id");
            return ExportGraphData(gd, opts);
        }

        public RrdDataXport ExportGraphData(GraphDefinition gd, DrawOptions opts)
        {
            foreach (var d in gd.Defs)
            {
                var p = Path.Combine(BaseDirectory, d.DataSourceId + ".rrd");
                d.DataSourceId = p.Replace(":\\", "\\:\\");
            }
            DateTime s, e;
            if (!RrdUtil.ParseRrdDateRange(opts.StartTime, opts.EndTime, out s, out e)) throw new Exception("Unparsable graph date range");
            int actualStep = 1;
            if (opts.Step.HasValue) actualStep = opts.Step.Value;
            if (actualStep == 0)
            {
                actualStep = SelectReasonableStep(s, e);
            }
            if (gd.Resolution != null && actualStep > 0)
            {
                for (int i = 0; i < gd.Resolution.Count; i++)
                {
                    var ri = gd.Resolution[i];
                    var dl = DateTime.Now.AddSeconds(-ri.SpanSec - 1);
                    log.Debug("res: {0}, l1: {1}, s: {2}, lt: {3}, t1: {4}, t2: {5}", ri.ResSec, dl, s, dl < s, dl.Ticks, s.Ticks);
                    if (i == gd.Resolution.Count - 1 || dl < s)
                    {
                        if (actualStep < ri.ResSec) actualStep = ri.ResSec;
                        log.Debug("Selected resolution {0}", ri.ResSec);
                        break;
                    }
                }
            }
            log.Debug("Setting step size to {0}", actualStep);
            opts.Step = actualStep > 0 ? actualStep : (int?)null;
            
            return this.RRDTool.ExportGraphData(gd, opts);
        }
    }
}
