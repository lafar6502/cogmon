using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGinnBPM.MessageBus;
using CogMon.Lib.DataSeries;
using CogMon.Lib;
using NLog;
using CogMon.Services.Dao;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using CogMon.Lib.Graph;
using Newtonsoft.Json;
using System.Threading;
using NGinnBPM.MessageBus.Impl.HttpService;

namespace CogMon.Services.SCall
{
    /// <summary>
    /// Remote call handler for various rrd management requests.
    /// </summary>
    public class DataSourceService : 
        IMessageHandlerService<GetDataSeriesInfo>,
        IMessageHandlerService<GetDataSeriesList>,
        IMessageHandlerService<GetDataSourceTemplateNames>,
        IMessageHandlerService<SaveDataSourceTemplate>,
        IMessageHandlerService<CreateDataSeriesFromTemplate>,
        IMessageHandlerService<UpdateData>,
        IMessageHandlerService<UpdateDataBatch>,
        IMessageHandlerService<DataRecord>,
        IMessageHandlerService<DrawGraphByDefinition>,
        IMessageHandlerService<ReCreateDataSeries>,
        IMessageHandlerService<CreateGraphDefinitionFromTemplate>,
        IMessageHandlerService<CreateDataSeriesForPerfCounter>,
        IMessageHandlerService<CollectServerPerfCounterData>
    {
        public IDataSeriesRepository DSRepo { get; set; }
        public IEventAggregator EventAggregator { get; set; }
        public IReportCogmonStatus StatusReporter { get; set; }
        public MongoDatabase Db { get; set; }
        private Logger log = LogManager.GetCurrentClassLogger();

        public object Handle(GetDataSeriesInfo message)
        {
            var dsi = DSRepo.GetDataSeries(message.Name, true);
            return dsi;
        }

        public object Handle(Lib.DataSeries.GetDataSeriesList message)
        {
            return DSRepo.DataSources;
        }

        public object Handle(GetDataSourceTemplateNames message)
        {
            var ret = new GetDataSourceTemplateNamesResponse
            {
                Templates = new List<GetDataSourceTemplateNamesResponse.TplInfo>()
            };
            foreach (var tpl in Db.GetCollection<DataSourceTemplate>().FindAll())
            {
                ret.Templates.Add(new GetDataSourceTemplateNamesResponse.TplInfo
                {
                    Id = tpl.Id,
                    Name = tpl.Name,
                    Description = tpl.Description
                });
            }
            return ret;

            throw new NotImplementedException();
        }

        public object Handle(SaveDataSourceTemplate message)
        {
            Db.GetCollection<DataSourceTemplate>().Save(message.Template);
            return message.Template;
        }

        public object Handle(CreateDataSeriesFromTemplate message)
        {
            return DSRepo.CreateDataSeriesFromTemplate(message, "system");
        }

        public object Handle(UpdateData message)
        {
            if (!string.IsNullOrEmpty(message.JobId)) ReportJobExecuted(message.JobId);
            Handle(message.Data);
            return "OK";
        }

        protected void ReportJobExecuted(string jobid)
        {
            string addr = RequestContext.CurrentRequest == null ? "" : RequestContext.CurrentRequest.ClientIP;
            StatusReporter.ReportJobExecuted(jobid, addr);
            /*ThreadPool.QueueUserWorkItem(new WaitCallback(x=> {
                try
                {
                    Db.GetCollection<ScheduledJob>().Update(Query.EQ("_id", jobid), Update.Set("LastRun", DateTime.Now));
                }
                catch (Exception ex)
                {
                    log.Warn("Failed to update job {0}: {1}", jobid, ex);
                }
            }));*/
        }

        public object Handle(DataRecord message)
        {
            if (message.Series.StartsWith("#"))
            {
                message.Series = message.Series.Substring(1);
                EventAggregator.AddEvent(message);
            }
            else
            {
                DSRepo.AppendData(message);
            }
            return "OK";
        }

        public object Handle(DrawGraphByDefinition message)
        {
            DSRepo.CreateGraph(message.Definition, new DrawOptions(), message.DestinationFile);
            return message.DestinationFile;
        }

        public object Handle(ReCreateDataSeries message)
        {
            return DSRepo.ReCreateDataSeries(message.Id, "system");
        }

        public object Handle(CreateGraphDefinitionFromTemplate message)
        {
            var tpl = Db.GetCollection<GraphDefinitionTemplate>().FindOneById(message.TemplateId);
            if (tpl == null) throw new Exception("Template not found: " + message.TemplateId);
            Dictionary<string, object> vals = new Dictionary<string, object>(message.Variables);
            foreach (var vd in tpl.Variables)
            {
                if (!vals.ContainsKey(vd.Name))
                {
                    if (vd.DefaultValue != null)
                    {
                        vals[vd.Name] = vd.DefaultValue;
                    }
                    else if (vd.Required) throw new Exception("Missing required variable: " + vd.Name);
                }
            }
            string json = Utils.SubstValue(tpl.JsonTemplate, vals);

            var gd = JsonConvert.DeserializeObject<GraphDefinition>(json);
            gd.TemplateId = tpl.Id;
            gd.TemplateVariables = vals;
            gd.ACL = tpl.ACL;
            if (string.IsNullOrEmpty(gd.Description)) gd.Description = tpl.Description;
            Db.GetCollection<GraphDefinition>().Save(gd);
            return gd;
        }

        public object Handle(UpdateDataBatch message)
        {
            
            throw new NotImplementedException();
        }

        public object Handle(CreateDataSeriesForPerfCounter message)
        {
            var tn = message.ServerSide ? "serverperfcounter" : "clientperfcounter";
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["Description"] = message.Description;
            vars["CounterId"] = message.CounterId;
            vars["JobGroup"] = message.JobGroup;
            vars["UpdateInterval"] = message.UpdateIntervalSec;
            
            var dsi = DSRepo.CreateDataSeriesFromTemplate(new CreateDataSeriesFromTemplate {
                OverwriteDataSourceId = null,
                TemplateId = tn,
                Parameters = vars
            }, "");
            return new CreateDataSeriesFromTemplateResponse { Success = true, Series = dsi.Id };
        }

        public IPerfCounters PerfCounters { get; set; }
        

        /// <summary>
        /// Collecting server-side perf counter data and appending it to a RRD
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public object Handle(CollectServerPerfCounterData message)
        {
            var job = Db.GetCollection<Dao.ScheduledJob>().FindOneById(message.JobId);
            if (job == null) throw new Exception("Job not found");
            if (job.QueryMethod != Lib.Scheduling.QueryType.ServerPerfCnt) throw new Exception("Incorrect job QueryMethod");

            DataRecord dr = new DataRecord { Series = job.DataSource };

            if (job.VariableNames == null || job.VariableNames.Length == 0)
            {
                if (string.IsNullOrEmpty(job.ScriptName)) throw new Exception("ScriptName parameter should contain perf counter Id if you are not using Variables");
                var pv = PerfCounters.GetCurrentStats(job.ScriptName, true);
                dr.Data = new double[] { pv.Count, pv.Sum, pv.Min, pv.Max, pv.Median, pv.Perc90, pv.Perc95, pv.Perc98, pv.Avg, pv.Freq };
            }
            else
            {
                if (job.VariableRetrieveRegex == null || job.VariableRetrieveRegex.Length == 0)
                {
                    string pcid = job.ScriptName;
                    var pv = PerfCounters.GetCurrentStats(pcid, true);
                    dr.DataMap = new Dictionary<string, double>();
                    for (int i = 0; i < job.VariableNames.Length; i++)
                    {
                        var pi = pv.GetType().GetProperty(job.VariableNames[i]);
                        if (pi == null) throw new Exception("Invalid variable: " + job.VariableNames[i]);
                        dr.DataMap[job.VariableNames[i]] = Convert.ToDouble(pi.GetValue(pv, null));
                    }
                }
                else if (job.VariableRetrieveRegex.Length != job.VariableNames.Length)
                {
                    throw new Exception("job VariableRetrieveRegex length invalid");
                }
                else
                {
                    Dictionary<string, PerfCounterStats> d = new Dictionary<string, PerfCounterStats>();
                    dr.DataMap = new Dictionary<string, double>();
                    for (int i = 0; i < job.VariableNames.Length; i++)
                    {
                        var vn = job.VariableRetrieveRegex[i];
                        int idx = vn.IndexOf('/');
                        string cid = idx < 0 ? job.ScriptName : vn.Substring(0, idx);
                        PerfCounterStats pv;
                        if (!d.TryGetValue(cid, out pv))
                        {
                            pv = PerfCounters.GetCurrentStats(cid, true);
                            d[cid] = pv;
                        }
                        string cv = idx < 0 ? vn : vn.Substring(idx + 1);
                        var pi = typeof(PerfCounterStats).GetProperty(cv);
                        if (pi == null) throw new Exception("Invalid perf counter statistic: " + vn);
                        dr.DataMap[job.VariableNames[i]] = Convert.ToDouble(pi.GetValue(pv, null));
                    }
                }
            }
            DSRepo.AppendData(dr);
            string addr = RequestContext.CurrentRequest == null ? "" : RequestContext.CurrentRequest.ClientIP;
            StatusReporter.ReportJobExecuted(message.JobId, addr);
            return "OK";
        }
    }
}
