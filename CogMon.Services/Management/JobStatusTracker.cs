using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using MongoDB.Driver;
using CogMon.Services.Dao;
using System.Collections.Concurrent;
using NGinnBPM.MessageBus;

namespace CogMon.Services.Management
{
    public class JobStatusTracker : IJobStatusTracker
    {
        public MongoDatabase Db { get; set; }
        public IDataSeriesRepository DSRepo { get; set; }

        private ConcurrentDictionary<string, JobStatusInfo> _jobs = new ConcurrentDictionary<string, JobStatusInfo>();
        private ConcurrentDictionary<string, AgentStatusInfo> _agents = new ConcurrentDictionary<string, AgentStatusInfo>();

       
      
        public IEnumerable<JobStatusInfo> GetStatusOfAllJobs()
        {
            List<JobStatusInfo> ret = new List<JobStatusInfo>();
            var list = Db.Find<ScheduledJob>(x => x.Active == true).SetFields("_id", "DataSource", "IntervalSeconds", "Group").ToList();
            foreach (var sj in list)
            {
                JobStatusInfo jsi;
                if (_jobs.TryGetValue(sj.Id, out jsi))
                {
                    jsi.DataSeriesId = sj.DataSource;
                    jsi.IntervalSeconds = sj.IntervalSeconds;
                    jsi.Group = sj.Group;
                    if (jsi.LastRun.HasValue && jsi.IntervalSeconds > 0 && jsi.LastRun.Value.AddSeconds(10 * jsi.IntervalSeconds) < DateTime.Now)
                    {
                        jsi.IsError = true;
                        jsi.StatusInfo = string.Format("Did not receive an update since {0}", jsi.LastRun);
                    }
                    ret.Add(jsi);
                }
                else
                {
                    ret.Add(new JobStatusInfo { Id = sj.Id, DataSeriesId = sj.DataSource, IsError = false, StatusInfo = "Not reported yet", IntervalSeconds = sj.IntervalSeconds });
                }
            }
            return ret;
        }

        public IEnumerable<AgentStatusInfo> GetStatusOfAllAgents()
        {
            List<AgentStatusInfo> ret = new List<AgentStatusInfo>();
            foreach (var st in _agents.Values)
            {
                if (st.LastSeen.HasValue && st.LastSeen.Value.AddMinutes(30) >= DateTime.Now)
                {
                    ret.Add(st);
                }
            }
            return ret;
        }


        public IEnumerable<DataSeriesStatusInfo> GetStatusOfAllDataSeries()
        {
            List<DataSeriesStatusInfo> ret = new List<DataSeriesStatusInfo>();
            List<JobStatusInfo> jobs = _jobs.Values.ToList();
            var lst = DSRepo.DataSources;
            foreach (var dr in lst)
            {
                var st = new DataSeriesStatusInfo { SeriesId = dr.Id, Description = dr.Description };
                var si = jobs.Find(x => x.DataSeriesId == st.SeriesId);
                if (si != null)
                {
                    st.LastUpdateJob = si.Id;
                    st.LastUpdate = si.LastSuccessfulRun;
                }
                ret.Add(st);
            }
            return ret;
        }

        public void Handle(Events.AgentQuery message)
        {
            string k = string.Format("{0}_{1}", message.AgentIP, message.AgentPID);
            var ag = _agents.GetOrAdd(k, x =>
            {
                return new AgentStatusInfo { Id = k, AgentAddress = message.AgentIP, AgentPID = message.AgentPID};
            });
            ag.LastSeen = DateTime.Now;
            ag.StatusInfo = "";
            ag.Groups = message.JobGroup;
        }

        public void Handle(Events.JobExecuted message)
        {
            var js = _jobs.GetOrAdd(message.JobId, x =>
            {
                return new JobStatusInfo { Id = message.JobId};
            });
            js.LastRun = DateTime.Now;
            js.StatusInfo = "OK";
            js.AgentAddress = message.AgentIP;
            js.IsError = false;
            js.LastSuccessfulRun = DateTime.Now;
        }

        public void Handle(Events.JobFailed message)
        {
            var js = _jobs.GetOrAdd(message.JobId, x =>
            {
                return new JobStatusInfo { Id = message.JobId };
            });
            js.LastRun = DateTime.Now;
            js.StatusInfo = message.ErrorInfo;
            if (!js.IsError)
            {
                js.IsError = true;
            }
            js.AgentAddress = message.AgentIP;
        }
    }


    public class JobTrackerEventHandler : IMessageConsumer<Events.AgentQuery>,
        IMessageConsumer<Events.JobExecuted>,
        IMessageConsumer<Events.JobFailed>
    {
        public JobStatusTracker StatusTracker { get; set; }

        public void Handle(Events.AgentQuery message)
        {
            StatusTracker.Handle(message);
        }

        public void Handle(Events.JobExecuted message)
        {
            StatusTracker.Handle(message);
        }

        public void Handle(Events.JobFailed message)
        {
            StatusTracker.Handle(message);
        }
    }
}
