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
using System.IO;


namespace CogMon.Agent
{
    public class FileJobScheduler : IStartableService, IDisposable
    {
        public int ScheduleUpdateIntervalSec { get; set; }
        public IServiceResolver ServiceResolver { get; set; }
        public PerfMon.PerfCounterStore PerfCounters { get; set; }
        public string BaseDir { get; set; }
        private Logger log = LogManager.GetCurrentClassLogger();
        private Timer _scheduler = null;
        private Timer _updater = null;
        

        /// <summary>Id of job to be run in debug mode (for testing purposes)</summary>
        public string TestJobId { get; set; }

        public string SchedulerGroup { get; set; }

        public FileJobScheduler()
        {
            ScheduleUpdateIntervalSec = 60;
            SchedulerGroup = Environment.MachineName;
        }

        internal class SchedTask
        {
            public string Id { get; set; }
            public DateTime LoadDate = DateTime.Now;
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
                
                ExecuteTask(tsk);
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
            log.Error("Task {0} failed: {1}", id, ex);
        }

        internal void ExecuteTask(SchedTask t)
        {
            log.Debug("Executing task {0} {1} {2}", t.Id, t.TaskData.QueryMethod, t.TaskData.ToString());
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
            _scheduler = new Timer(10000);
            _scheduler.Elapsed += new System.Timers.ElapsedEventHandler(_scheduler_Elapsed);
            _scheduler.AutoReset = true;
            _scheduler.Start();
            
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(x => this.UpdateScheduledJobs()));
        }

        void _scheduler_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //log.Trace("Scheduler...");
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
            if (PerfCounters != null)
            {
                foreach(var p in PerfCounters.GetPerfCounterNames().OrderBy(x => x))
                {
                    var v = PerfCounters.GetPerfCounterValuesAndReset(p);
                    if (v != null) { 
                        log.Error("{0}: {1}, p98={2}", p, v.Count, v.Perc98);
                    }
                }
            }

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

        protected ScheduledTask LoadJob(string fileName)
        {
            var st = JsonConvert.DeserializeObject<ScheduledTask>(File.ReadAllText(fileName, Encoding.UTF8));
            st.Id = Path.GetFileNameWithoutExtension(fileName);
            return st;
        }

        protected void UpdateScheduledJobs()
        {
            var jobzFiles = Directory.GetFiles(BaseDir, "*.json");
            string[] pcl = PerfCounters == null ? new string[] { } : PerfCounters.GetPerfCounterNames().ToArray();
            
            List<SchedTask> lst = new List<SchedTask>();


            foreach(var f in jobzFiles)
            {
                var id = Path.GetFileNameWithoutExtension(f);
                var dm = File.GetLastWriteTime(f);
                var ct = _tasks.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.InvariantCultureIgnoreCase));
                if (ct == null || ct.LoadDate < dm)
                {
                    var jd = LoadJob(f);
                    if (!string.IsNullOrEmpty(this.SchedulerGroup))
                    {
                        if (!string.Equals(this.SchedulerGroup, jd.Group, StringComparison.InvariantCulture) && !string.Equals("all", jd.Group, StringComparison.InvariantCulture))
                        {
                            continue; //skip jobs with wrong group
                        };
                    }
                    if (ct != null)
                    {
                        ct.TaskData = jd;
                    }
                    else
                    {
                        ct = new SchedTask
                        {
                            TaskData = jd,
                            Id = jd.Id,
                            LoadDate = DateTime.Now,
                            NextTrigger = DateTime.Now.AddSeconds(jd.IntervalSeconds)
                        };
                    }
                }
                lst.Add(ct);
            }
            log.Debug("Updated job list: {0} jobs", lst.Count);
            _tasks = lst;
        }
    
        public void  Dispose()
        {
            Stop();
        }
    }
}
