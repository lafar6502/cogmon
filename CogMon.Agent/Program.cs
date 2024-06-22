using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace CogMon.Agent
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                //NLog.Config.SimpleConfigurator.ConfigureForConsoleLogging(NLog.LogLevel.Debug);
                if (args[0] == "-debug")
                {
                    Debug(args);
                }
                else if (args[0] == "-testTask")
                {
                    if (args.Length < 2) throw new Exception("Task file name missing");
                    TestTask(args[1]);
                }
                else if (args[0] == "-testJob")
                {
                    if (args.Length < 2) throw new Exception("Job ID missing");
                    TestJob(args[1]);
                }
                else if (args[0] == "-listPerfCounters")
                {
                    ListPerfCounters();
                }
                else
                {
                    Console.WriteLine("Invalid arguments specified.");
                    Console.WriteLine("Possible options: \n-debug\n-testTask [task json file]\n-testJob [jobId]");
                }
                return;
            }
            
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new Service1() 
			};
            ServiceBase.Run(ServicesToRun);
        }


        static void ListPerfCounters(string searchStr = null)
        {
            var lst = PerformanceCounterCategory
            .GetCategories()
            .Select(cat => cat.GetInstanceNames().Any() ? cat.GetInstanceNames().Select(i => cat.GetCounters(i)).SelectMany(counter => counter) : cat.GetCounters("")).SelectMany(counter => counter)
            .Where(x => searchStr == null || (x.InstanceName.Contains(searchStr) || x.CounterName.Contains(searchStr)))
            .Select(counter => string.Format("{0} : {1}.{2}", counter.InstanceName, counter.CategoryName, counter.CounterName));

            foreach(var s in lst)
            {
                Console.WriteLine(s);
            }

        }
        static void Debug(string[] args)
        {
            
            var s = new Service1();
            Console.WriteLine("Enter to start...");
            Console.ReadLine();
            s.Start(args);
            Console.WriteLine("Enter to exit...");
            Console.ReadLine();
            s.Stop();
        }

        static void TestTask(string fileName)
        {
            Console.WriteLine("Enter to continue with {0}", fileName);
            Console.ReadLine();
            Tests.TestTask(fileName);
        }

        static void TestJob(string jobId)
        {
            Console.WriteLine("Hit Enter to run job ", jobId);
            Console.ReadLine();
            Tests.TestJob(jobId);
        }
    }
}
