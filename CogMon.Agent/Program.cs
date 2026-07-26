using CogMon.Agent.PerfMon;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
                    TestPC2();
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
                else if (args.Contains("--install"))
                {
                    Console.WriteLine("Installing..");
                    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                    return;
                }
                else if (args.Contains("--uninstall"))
                {
                    Console.WriteLine("Uninstalling..");
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                    return;
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

        static void TestRunProcPowershell()
        {
            // Get-Process -IncludeUserName -Name w3wp | Select-Object Name,CPU,UserName,WorkingSet | ConvertTo-Json
            
        }

        static void TestPC2()
        {
            var pc2 = new PerfCounter2();
            pc2.MaxUpdates = 10;
            pc2.MaxSampleAgeSec = 300;

            var r0 = pc2.GetCurrentValue(true);
            if (r0.Count > 0) throw new Exception();

            pc2.Update(10);
            pc2.Update(20);
            pc2.Update(30);
            pc2.Update(40);

            var rt = pc2.GetCurrentValue(true);
            if (rt.Count != 4) throw new Exception();
            if (rt.Sum != 100) throw new Exception();
            if (rt.Median != 30) throw new Exception();

            for(var i=1; i<=15;i++)
            {
                pc2.Update(i);
            }
            var rt2 = pc2.GetCurrentValue(true); 
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
