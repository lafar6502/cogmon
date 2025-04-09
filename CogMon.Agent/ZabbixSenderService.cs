using CogMon.Lib;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CogMon.Agent
{
    public class ZabbixSenderService : ITimeSeriesDatabase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public string ZabbixSenderPath { get; set; }
        public string[] ZabbixSenderArguments { get; set; }

        public string ZabbixSenderCommandline { get; set; }

        public void UpdateDataSource(IEnumerable<DataRecord> batch)
        {
            var tp = Path.GetTempPath();
            var fn = Guid.NewGuid().ToString() + ".txt";
            fn = Path.Combine(tp, fn);
            using (var sw = new StreamWriter(fn))
            {
                SaveDataFile(batch, sw);
            }
            try
            {
                var args = ZabbixSenderArguments == null ? new List<string>() : new List<string>(ZabbixSenderArguments);
                args.Add("-i");
                args.Add(fn);
                string result = null;
                var n = Tools.ProcessUtil.ExecProgram(ZabbixSenderPath, args, ZabbixSenderCommandline, out result);
                log.Info("Res: {0}. Out: {1}", n, result);
                if (log.IsDebugEnabled)
                {
                    log.Info("Input file:\r\n {0}", File.ReadAllText(fn));
                }
            }
            finally
            {
                if (File.Exists(fn)) File.Delete(fn);
            }
        }

        private static string EscapeZabbixString(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Any(x => char.IsWhiteSpace(x) || x == '"' || x == '\\'))
            {
                return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            }
            else return s;
        }

        private void SaveDataFile(IEnumerable<DataRecord> batch, TextWriter output)
        {
            foreach(var dr in batch)
            {
                if (string.IsNullOrEmpty(dr.Series)) throw new Exception("No series/hostname in data");
                foreach(var kv in dr.DataMap)
                {
                    if (double.IsNaN(kv.Value))
                    {
                        continue; //skip NaN - missing data
                    }
                    output.WriteLine("{0} {1} {2}", EscapeZabbixString(dr.Series), EscapeZabbixString(kv.Key), kv.Value);
                }
            }
        }
    }
}
