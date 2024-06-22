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
                    string catName = null;
                    if (args.Length > 1 && args[1] != "*") catName = args[1];
                    ListPerfCounters(catName, args.Length > 2 ? args[2] : null);
                }
                else if (args[0] == "-listPerfCounterCategories")
                {
                    ListPerfCounterCategories();
                }
                else
                {
                    Console.WriteLine("Invalid arguments specified.");
                    Console.WriteLine("Possible options: \n-debug\n-testTask [task json file]\n-testJob [jobId]\r\n-listPerfCounters\r\n-listPerfCounterCategories");
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

        static void ListPerfCounterCategories()
        {
            foreach(var c in PerformanceCounterCategory.GetCategories().OrderBy(x => x.CategoryName))
            {
                Console.WriteLine(c.CategoryName);
            }
        }
        static void ListPerfCounters(IEnumerable<PerformanceCounter> counters)
        {
            foreach(var counter in counters)
            {
                var s = WinPerf.GetPerfCounterKey(counter.CategoryName, counter.CounterName, counter.InstanceName, counter.MachineName);
                Console.WriteLine(s);
            }
        }
        static void ListPerfCounters(string categoryName = null, string searchStr = null)
        {
            var lst = PerformanceCounterCategory
            .GetCategories()
            .Where(x => x.CategoryName != "Thread")
            .Where(x => categoryName == null || categoryName.Equals(x.CategoryName, StringComparison.InvariantCultureIgnoreCase));
            foreach(var cat in lst)
            {
                var insts = cat.GetInstanceNames();
                //Console.WriteLine("** " + cat.CategoryName + ": " + insts.Length + " instances");
                PerformanceCounter[] cnts;
                if (insts.Any())
                {
                    foreach (var instName in insts)
                    {
                        try
                        {
                            cnts = cat.GetCounters(instName);
                            ListPerfCounters(cnts.Where(x => searchStr == null || (x.InstanceName.IndexOf(searchStr, StringComparison.InvariantCultureIgnoreCase) >= 0  || x.CounterName.IndexOf(searchStr, StringComparison.InvariantCultureIgnoreCase) >= 0)));
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine(";Error getting instance " + instName + ":" + ex.Message);
                        }
                    }
                }
                else
                {
                    cnts = cat.GetCounters();
                    ListPerfCounters(cnts.Where(x => searchStr == null || (x.InstanceName.IndexOf(searchStr, StringComparison.InvariantCultureIgnoreCase) >= 0 || x.CounterName.IndexOf(searchStr, StringComparison.InvariantCultureIgnoreCase) >= 0)));
                }
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
