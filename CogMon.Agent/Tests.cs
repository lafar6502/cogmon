using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Castle.Windsor;
using CogMon.Lib.Scheduling;
using CogMon.Lib;
using System.IO;
using Newtonsoft.Json;

namespace CogMon.Agent
{
    public class Tests
    {
        public static void TestTask(string serializedTaskFile)
        {
            string json = null;
            using (var sr = new StreamReader(serializedTaskFile, Encoding.UTF8))
            {
                json = sr.ReadToEnd();
            }
            var tsk = JsonConvert.DeserializeObject<ScheduledTask>(json);
            var st = new JobScheduler.SchedTask
            {
                Id = tsk.Id,
                NextTrigger = DateTime.Now,
                TaskData = tsk
            };
            var wc = new Castle.Windsor.WindsorContainer();
            ServiceConfigurator.Begin(wc)
                .FinishConfiguration();
            wc.Resolve<JobScheduler>().RunTask(st);
        }
    }
}
