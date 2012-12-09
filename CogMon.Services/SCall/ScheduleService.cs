using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGinnBPM.MessageBus;
using NGinnBPM.MessageBus.Impl.HttpService;
using CogMon.Lib.Scheduling;
using NLog;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using CogMon.Services.Dao;
using CogMon.Lib.DataSeries;
using NLog;

namespace CogMon.Services.SCall
{
    public class ScheduleService : IMessageHandlerService<GetActiveScheduledJobs>,
        IMessageHandlerService<ReportJobFailed>
    {
        public MongoDatabase Db { get; set; }
        public IDataSeriesRepository DsRepo { get; set; }
        public IMessageDispatcher EventDispatcher { get; set; }
        

        private Logger log = LogManager.GetCurrentClassLogger();

        public object Handle(GetActiveScheduledJobs message)
        {
            string addr = RequestContext.CurrentRequest == null ? "" : RequestContext.CurrentRequest.ClientIP;
            EventDispatcher.Publish(new Events.AgentQuery { AgentIP = addr, AgentPID = message.AgentPID, JobGroup = (message.Groups != null && message.Groups.Length > 0) ? message.Groups[0] : null });
            DateTime lm = message.UpdatedAfter.HasValue ? message.UpdatedAfter.Value : new DateTime(2000, 1, 1);
            var l = Db.Find<ScheduledJob>(x => x.Active == true && x.LastModified >= lm && (message.Groups == null || message.Groups.Length == 0 ? x.Group.IsNull() : x.Group.In(message.Groups)));
            //var l = Db.GetCollection<ScheduledJob>().Find(Query.EQ("Active", true));
            var r = new GetActiveScheduledJobsResponse
            {
                Tasks = new List<ScheduledTask>()
            };
            
            foreach (var sj in l)
            {
                r.Tasks.Add(new ScheduledTask
                {
                    Id = sj.Id.ToString(),
                    QueryMethod = sj.QueryMethod,
                    ScriptName = sj.ScriptName,
                    IntervalSeconds = sj.IntervalSeconds,
                    DataSeries = sj.DataSource,
                    Arguments = sj.Arguments,
                    VersionTag = sj.LastModified.Ticks.ToString(),
                    Variables = sj.VariableNames,
                    VariableRegex = sj.VariableRetrieveRegex,
                    Options = sj.Options,
                    LastRun = sj.LastRun
                });
            }
            return r;
        }

        public object Handle(ReportJobFailed message)
        {
            log.Warn("Job {0} failed: {1}", message.JobId, message.ErrorInfo);
            string addr = RequestContext.CurrentRequest == null ? "" : RequestContext.CurrentRequest.ClientIP;
            EventDispatcher.Publish(new Events.JobFailed
            {
                AgentIP = addr,
                ErrorInfo = message.ErrorInfo,
                JobId = message.JobId
            });
            return "";
        }
    }
}
