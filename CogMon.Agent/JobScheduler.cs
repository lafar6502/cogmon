using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGinnBPM.MessageBus.Impl;
using NGinnBPM.MessageBus;
using Newtonsoft.Json;
using NLog;
using System.Timers;
using CogMon.Lib.Scheduling;
using System.Timers;

namespace CogMon.Agent
{
    public class JobScheduler : IStartableService, IDisposable
    {
        public int ScheduleUpdateIntervalSec { get; set; }
        public IServiceClient CogMon { get; set; }
        public IServiceResolver ServiceResolver { get; set; }

        private Logger log = LogManager.GetCurrentClassLogger();
        private Timer _scheduler = null;
        private Timer _updater = null;

        public string SchedulerGroup { get; set; }

        public JobScheduler()
        {
            ScheduleUpdateIntervalSec = 300;
            SchedulerGroup = "cogmon";
        }

        internal class SchedTask
        {
            public string Id { get; set; }
            public DateTime NextTrigger;
            public ScheduledTask TaskData { get; set; }
        }

        private List<SchedTask> _tasks = new List<SchedTask>();

        public bool IsRunning
        {
            get { return _scheduler != null; }
        }

        

        private HashSet<string> _running = new HashSet<string>();

        internal void RunTask(SchedTask tsk)
        {
            lock (_running)
            {
                if (_running.Contains(tsk.Id))
                {
                    log.Debug("Task already running: {0}", tsk.Id);
                    return;
                }
                _running.Add(tsk.Id);
            }
            DateTime dt = DateTime.Now;
            try
            {
                tsk.NextTrigger = DateTime.Now.AddSeconds(tsk.TaskData.IntervalSeconds);
                CreateAndRunTask(tsk);
            }
            catch (Exception ex)
            {
                log.Error("Error running task {0}: {1}", tsk.Id, ex);
                ReportTaskFailed(tsk.Id, ex);
            }
            finally
            {
                lock (_running)
                {
                    _running.Remove(tsk.Id);
                }
                log.Debug("Task {0} finished. Time: {1}, Next trigger: {2}", tsk.Id, DateTime.Now - dt, tsk.NextTrigger);
            }
        }

        internal void ReportTaskFailed(string id, Exception ex)
        {
            try
            {
                this.CogMon.CallService<string>(new ReportJobFailed { JobId = id, ErrorInfo = ex.Message, Timestamp = DateTime.Now });
            }
            catch (Exception e2)
            {
                log.Warn("Failed to report job failure to cogmon: {0}", e2);
            }
        }

        internal void CreateAndRunTask(SchedTask t)
        {
            var ti = ServiceResolver.GetInstance<JobBase>(t.TaskData.QueryMethod.ToString());
            var json = JsonConvert.SerializeObject(t.TaskData);
            JsonConvert.PopulateObject(json, ti);
            ti.Execute();
        }

        public void Start()
        {
            Stop();
            log.Info("Starting scheduler");
            _updater = new Timer(this.ScheduleUpdateIntervalSec * 1000);
            _updater.Elapsed += new ElapsedEventHandler(_updater_Elapsed);
            _updater.AutoReset = true;
            _updater.Start();
            _scheduler = new Timer(2000);
            _scheduler.Elapsed += new System.Timers.ElapsedEventHandler(_scheduler_Elapsed);
            _scheduler.AutoReset = true;
            _scheduler.Start();
            
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(x => this.UpdateScheduledJobs()));
        }

        void _scheduler_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            log.Debug("Scheduler...");
            var tsks = _tasks;
            if (tsks == null) return;
            System.Threading.Tasks.Parallel.ForEach(tsks.Where(x => x.NextTrigger <= DateTime.Now), x => RunTask(x));
        }

        void _updater_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateScheduledJobs();
        }

        

        public void Stop()
        {
            if (_updater != null)
            {
                log.Info("Stopping scheduler");
                _updater.Dispose();
                _updater = null;
            }
            if (_scheduler != null)
            {
                _scheduler.Dispose();
                _scheduler = null;
            }
        }

        private Random _r = new Random(DateTime.Now.Millisecond);

        protected void UpdateScheduledJobs()
        {
            log.Debug("Updating scheduler jobs. group: {0}", this.SchedulerGroup);
            var resp = CogMon.CallService<GetActiveScheduledJobsResponse>(new GetActiveScheduledJobs { Groups = string.IsNullOrEmpty(SchedulerGroup) ? (string[]) null : new string[] {SchedulerGroup} });
            List<SchedTask> lst = new List<SchedTask>();
            var tsks = _tasks;
            
            foreach (var t in resp.Tasks)
            {
                SchedTask pt = tsks.FirstOrDefault(x => x.Id == t.Id);
                if (pt != null)
                {
                    pt.TaskData = t;
                }
                else pt = new SchedTask
                {
                    Id = t.Id,
                    TaskData = t,
                    NextTrigger = t.LastRun.AddSeconds(t.IntervalSeconds) < DateTime.Now ? DateTime.Now.AddSeconds(_r.Next(60)) : t.LastRun.AddSeconds(t.IntervalSeconds)
                };
                lst.Add(pt);
            }
            log.Debug("Updated job list: {0}", lst.Count);
            _tasks = lst;
        }
    
        public void  Dispose()
        {
            Stop();
        }
    }
}
