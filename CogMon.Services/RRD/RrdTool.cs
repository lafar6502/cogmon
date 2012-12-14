using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NLog;
using System.IO;
using Newtonsoft.Json;
using CogMon.Lib.Graph;
using CogMon.Lib.DataSeries;
using System.Xml;


namespace CogMon.Services.RRD
{
    public class RrdTool : IRrdTool
    {
        public string RrdExe { get; set; }
        public string RrdUpdate { get; set; }
        
        private Logger log = LogManager.GetCurrentClassLogger();

        public RrdTool()
        {
            
        }

        public void AppendData(string rrdFile, DateTime? timestamp, double[] values)
        {
            if (values == null) throw new ArgumentException("Missing input data");
            bool rrdUpdateSet = !string.IsNullOrEmpty(RrdUpdate);
            StringBuilder sb = new StringBuilder();
            string t = timestamp.HasValue ? ToUnixTime(timestamp.Value).ToString() : "N";
            sb.AppendFormat("{0} \"{1}\" {2}", rrdUpdateSet ? "" : "update", rrdFile, t);
            foreach (double v in values)
            {
                if (double.IsNaN(v))
                    sb.Append(":U");
                else
                {

                    if ((v - Math.Truncate(v)) < 2 * double.Epsilon)
                    {
                        sb.AppendFormat(":{0}", (long)v);
                    }
                    else
                    {
                        sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, ":{0:0.00}", v);
                    }
                }
            }
            if (rrdUpdateSet)
            {
                RunExeWithCommandline(RrdUpdate, sb.ToString());
            }
            else
            {
                RunRrdWithCommandline(sb.ToString());
            }
        }

        public void AppendData(string rrdFile, DateTime? timestamp, IDictionary<string, double> values)
        {
            if (values == null) throw new ArgumentException("Missing input data");
            bool rrdUpdateSet = !string.IsNullOrEmpty(RrdUpdate);
            StringBuilder sb = new StringBuilder();
            string t = timestamp.HasValue ? ToUnixTime(timestamp.Value).ToString() : "N";
            string[] names = values.Keys.ToArray();
            double[] vals = new double[names.Length];
            sb.AppendFormat("{0} {1} -t ", rrdUpdateSet ? "" : "update", rrdFile);
            for (int i = 0; i < names.Length; i++)
            {
                sb.AppendFormat(i > 0 ? ":{0}" : "{0}", names[i]);
                vals[i] = values[names[i]];
            }
            sb.AppendFormat(" {0}", t);
            foreach (double v in vals)
            {
                if (double.IsNaN(v))
                {
                    sb.Append(":U");
                }
                else if ((v - Math.Truncate(v)) < 2 * double.Epsilon)
                {
                    sb.AppendFormat(":{0}", (long)v);
                }
                else
                {
                    sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, ":{0:0.00}", v);
                }
            }
            if (rrdUpdateSet)
            {
                RunExeWithCommandline(RrdUpdate, sb.ToString());
            }
            else
            {
                RunRrdWithCommandline(sb.ToString());
            }
        }


        public static int ToUnixTime(DateTime dt)
        {
            return RrdUtil.ToUnixTime(dt);
        }

        public static DateTime FromUnixTime(int ut)
        {
            return RrdUtil.FromUnixTime(ut);
        }

        private string RunRrdWithCommandline(string cmdline)
        {
            return RunExeWithCommandline(RrdExe, cmdline);
        }

        /// <summary>
        /// Runs external command and returns its output text
        /// </summary>
        /// <param name="exe"></param>
        /// <param name="cmdline"></param>
        /// <returns></returns>
        private string RunExeWithCommandline(string exe, string cmdline)
        {
            log.Info("Running {0} {1}", exe, cmdline);
            var psi = new ProcessStartInfo(exe, cmdline);
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            
            using (var p = Process.Start(psi))
            {
                string ret = p.StandardOutput.ReadToEnd();
                if (!p.WaitForExit(3000))
                {
                    log.Error("Process did not terminate: {0} {1}", exe, cmdline);
                    throw new Exception("rrdtool did not finish processing: " + cmdline);
                }
                if (p.ExitCode != 0)
                {
                    string msg = "";
                    if (!p.StandardError.EndOfStream)
                        msg = p.StandardError.ReadToEnd();
                    if (string.IsNullOrEmpty(msg)) msg = ret;
                    log.Error("rrdtool failed exit code={0}, message: {2}, args: {1}", p.ExitCode, cmdline, msg);
                    throw new Exception("Error executing RRDTool with " + cmdline + ": " + msg);
                }
                //log.Debug("Output: {0}", ret);
                return ret;
            }
        }

        public void CreateRRDFile(RrdCreateInfo config, string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("create \"{0}\" --step {1}", fileName, config.Step);
            if (!string.IsNullOrEmpty(config.StartTime))
            {
                var start = config.StartTime;
                if (string.Equals(config.StartTime, "step_aligned", StringComparison.InvariantCultureIgnoreCase))
                {
                    int tm = ToUnixTime(DateTime.Now);
                    tm = tm - (tm % config.Step);
                    start = tm.ToString();
                    log.Info("CreateRRDFile {1}: start time aligned to previous step: {0}", start, fileName);
                }
                sb.AppendFormat(" --start \"{0}\"", start);
            }
            foreach (var df in config.Fields)
            {
                sb.AppendFormat(" DS:{0}:{1}:{2}:{3}:{4}", df.Name, df.SeriesType.ToString(), df.HeartbeatSeconds.HasValue ? df.HeartbeatSeconds.Value : config.Step * 10, df.Min.HasValue && !double.IsNaN(df.Min.Value) ? df.Min.Value.ToString() : "U", df.Max.HasValue && !double.IsNaN(df.Max.Value) ? df.Max.Value.ToString() : "U");
            }
            foreach (var ag in config.Aggregates)
            {
                sb.AppendFormat(" RRA:{0}:{1}:{2}:{3}", ag.Function, ag.XFilesFactor, ag.AggregateSteps, ag.StoredRows);
            }
            if (config.HWAggregates != null)
            {
                foreach (var hw in config.HWAggregates)
                {
                    sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, " RRA:{0}:{1}:{2}:{3}:{4}", hw.Op.ToString(), hw.Rows, hw.Alpha, hw.Beta, hw.SeasonalPeriod);
                }
            }
            RunRrdWithCommandline(sb.ToString());
        }


        public void SaveGraphFile(RRDGraphInfo graphConfig, string outputFile)
        {
            throw new NotImplementedException();
        }

        public RrdFileInfo GetRrdInfoFromFile(string fileName)
        {
            var cmd = string.Format("info \"{0}\"", fileName);
            var sr = RunRrdWithCommandline(cmd);
            var d = ParseRRDInfo(new StringReader(sr), true);
            var ret = JsonConvert.DeserializeObject<RrdFileInfo>(JsonConvert.SerializeObject(d));
            return ret;
        }

        private static object ParseRrdValue(string val)
        {
            if (val.StartsWith("\"")) return val.Substring(1, val.Length - 2);
            if (char.IsDigit(val[0]) || (val.Length > 1 && val[0] == '-' && char.IsDigit(val[1])))
            {
                var cult = System.Globalization.CultureInfo.InvariantCulture;
                int d;
                if (Int32.TryParse(val, System.Globalization.NumberStyles.Number, cult, out d)) return d;
                long ld;
                if (Int64.TryParse(val, System.Globalization.NumberStyles.Number, cult, out ld)) return ld;
                double dv;
                if (Double.TryParse(val, System.Globalization.NumberStyles.Any, cult, out dv)) return dv;
                throw new Exception("Can't parse value: " + val);
            }
            if (val == "NaN") return Double.NaN;
            throw new Exception("Unrecognized value: " + val);
        }

        private static double ParseDouble(string val)
        {
            var cult = System.Globalization.CultureInfo.InvariantCulture;
            double dv;
            if (Double.TryParse(val, System.Globalization.NumberStyles.Any, cult, out dv)) return dv;
            if (val == "NaN") return Double.NaN;
            throw new Exception("Can't parse value: " + val);
        }

        private static string ToString(double val)
        {
            var cult = System.Globalization.CultureInfo.InvariantCulture;
            return val.ToString(cult);
        }

        private Dictionary<string, object> ParseRRDInfo(string txt, bool dynAsArray)
        {
            return ParseRRDInfo(new StringReader(txt), dynAsArray);
        }

        private Dictionary<string, object> ParseRRDInfo(TextReader tr, bool dynAsArray)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            string l;
            while ((l = tr.ReadLine()) != null)
            {
                if (l.StartsWith(";")) continue;
                int idx = l.LastIndexOf('=');
                if (idx < 0) throw new Exception("Incorrect line: " + l);
                string rf = l.Substring(0, idx).Trim();
                string val = l.Substring(idx + 1).Trim();
                //log.Debug("Rf: {0}|{1}", rf, val);
                string[] parts = rf.Split('.');
                Dictionary<string, object> cur = ret;
                for (int i = 0; i < parts.Length; i++)
                {
                    var p = parts[i];
                    if (i == parts.Length - 1)
                    {
                        cur[p] = ParseRrdValue(val);
                    }
                    else
                    {
                        if (p.EndsWith("]"))
                        {
                            int st = p.IndexOf('[');
                            if (st <= 0) throw new Exception("Invalid format: " + l);
                            string indexer = p.Substring(st + 1, p.Length - st - 2);
                            string basei = p.Substring(0, st);
                            //log.Info("indexer: {0}, base: {1}", indexer, basei);
                            int tidx;
                            if (Int32.TryParse(indexer, out tidx) || dynAsArray)
                            {
                                if (!cur.ContainsKey(basei)) cur[basei] = new List<Dictionary<string, object>>();
                                List<Dictionary<string, object>> lb = (List<Dictionary<string, object>>)cur[basei];
                                if (dynAsArray)
                                {
                                    var td = lb.Find(x => x.ContainsKey("id") && string.Equals((string) x["id"], indexer));
                                    if (td == null)
                                    {
                                        td = new Dictionary<string, object>();
                                        td["id"] = indexer;
                                        lb.Add(td);
                                    }
                                    cur = td;
                                }
                                else
                                {
                                    if (tidx >= lb.Count)
                                    {
                                        lb.Add(new Dictionary<string, object>());
                                    }
                                    cur = lb[tidx];
                                }
                            }
                            else
                            {
                                if (!cur.ContainsKey(basei)) cur[basei] = new Dictionary<string, object>();
                                Dictionary<string, object> cb = (Dictionary<string, object>)cur[basei];
                                if (!cb.ContainsKey(indexer)) cb[indexer] = new Dictionary<string, object>();
                                cur = (Dictionary<string, object>) cb[indexer];
                            }
                        }
                        else
                        {
                            if (!cur.ContainsKey(p)) cur[p] = new Dictionary<string, object>();
                            cur = (Dictionary<string, object>)cur[p];
                        }
                    }
                }
            }
            return ret;
        }





        public RrdImageInfo DrawGraph(Lib.Graph.GraphDefinition gd, Lib.Graph.DrawOptions opts, string destinationFile)
        {
            return DrawGraph(gd, opts, null, destinationFile);
        }

        protected string BuildRrdGraphCmdline(GraphDefinition gd, DrawOptions opts, IEnumerable<RrdEventInfo> addEvents, string destinationFile, bool asXport)
        {
            StringWriter cmd = new StringWriter();
            if (asXport)
            {
                cmd.Write("xport ");
                if (opts.Width.HasValue) cmd.Write(" -m {0}", opts.Width.Value);
            }
            else
            {
                if (destinationFile != null) cmd.Write("graphv \"{0}\"", destinationFile);
                if (!string.IsNullOrEmpty(gd.AdditionalCmdParams)) cmd.Write(" " + gd.AdditionalCmdParams);
                if (opts.Width.HasValue) cmd.Write(" -w {0}", opts.Width.Value);
                if (opts.Height.HasValue) cmd.Write(" -h {0}", opts.Height.Value);
            }
            if (!string.IsNullOrEmpty(opts.StartTime)) cmd.Write(" -s \"{0}\"", opts.StartTime);
            if (!string.IsNullOrEmpty(opts.EndTime)) cmd.Write(" -e \"{0}\"", opts.EndTime);
            if (opts.Step.HasValue) cmd.Write(" --step {0}", opts.Step.Value);
            
            if (gd.Defs == null) throw new Exception("DEFs are required");

            foreach (var d in gd.Defs)
            {
                cmd.Write(" " + d.ToRRDString());
            }

            if (gd.CVDefs != null)
            {
                foreach (var cvd in gd.CVDefs)
                {
                    cmd.Write(" " + cvd.ToRRDString());
                }
            }
            for (int i = 0; i < gd.Elements.Count; i++)
            {
                var el = gd.Elements[i];
                if (el.Hide) continue;
                if (opts.SkipElements != null && opts.SkipElements.Contains(i)) continue;
                if (asXport)
                {
                    if (el.Op == GraphOperation.AREA || el.Op == GraphOperation.LINE1 || el.Op == GraphOperation.LINE2
                        || el.Op == GraphOperation.LINE3 || el.Op == GraphOperation.TICK)
                    {
                        cmd.Write(" XPORT:{0}{1}", el.Value, string.IsNullOrEmpty(el.Legend) ? "" : string.Format(":\"{0}\"", el.Legend));
                    }
                }
                else if (destinationFile != null) cmd.Write(" " + el.ToRRDString());
            }

            
            if (addEvents != null && destinationFile != null)
            {
                addEvents = addEvents.OrderBy(x => x.Time);
                foreach (var ev in addEvents)
                {
                    cmd.Write(" " + ev.ToRRDString());
                }
            }
            var cmdtext = cmd.ToString();
            var dic = PrepareGraphParams(gd, opts);
            return Utils.SubstValue(cmdtext, dic);
        }

        private Dictionary<string, object> PrepareGraphParams(GraphDefinition gd, DrawOptions opts)
        {
            var dic = new Dictionary<string, object>();
            dic["step"] = opts.Step.HasValue ? opts.Step.Value : 0;
            dic["width"] = opts.Width.HasValue ? opts.Width : 0;
            dic["height"] = opts.Height.HasValue ? opts.Height : 0;
            dic["graphDefinitionId"] = gd.Id;
            dic["graphTitle"] = gd.Title.Replace("\"", "\\\"");
            return dic;
        }

        public RrdImageInfo DrawGraph(GraphDefinition gd, DrawOptions opts, IEnumerable<RrdEventInfo> addEvents, string destinationFile)
        {
            var cmdtext = BuildRrdGraphCmdline(gd, opts, addEvents, destinationFile, false);
            
            string ret = RunRrdWithCommandline(cmdtext);
            var ii = new RrdImageInfo();
            ii.FileName = destinationFile;
            log.Debug("Img info: {0}", ret);
            var dt = ParseRRDInfo(ret, true);
            if (dt.ContainsKey("graph_start"))
            {
                ii.GraphStartTime = Convert.ToInt32(dt["graph_start"]);
                ii.Start = FromUnixTime(ii.GraphStartTime);
            }
            if (dt.ContainsKey("graph_end"))
            {
                ii.GraphEndTime = Convert.ToInt32(dt["graph_end"]);
                ii.End = FromUnixTime(ii.GraphEndTime);
            }
            if (dt.ContainsKey("graph_top")) ii.GraphTop = Convert.ToInt32(dt["graph_top"]);
            if (dt.ContainsKey("graph_left")) ii.GraphLeft = Convert.ToInt32(dt["graph_left"]);
            if (dt.ContainsKey("graph_width")) ii.GraphWidth = Convert.ToInt32(dt["graph_width"]);
            if (dt.ContainsKey("graph_height")) ii.GraphHeight = Convert.ToInt32(dt["graph_height"]);
            if (dt.ContainsKey("value_min")) ii.MinValue = Convert.ToDouble(dt["value_min"]);
            if (dt.ContainsKey("value_max")) ii.MaxValue = Convert.ToDouble(dt["value_max"]);
            if (addEvents != null)
            {
                ii.Events = new List<RrdImageInfo.ImgEventInfo>();
                foreach (var ev in addEvents)
                {
                    if (ev.Time < ii.GraphStartTime || ev.Time > ii.GraphEndTime) continue;
                    ii.Events.Add(new RrdImageInfo.ImgEventInfo
                    {
                        EventId = ev.EventId,
                        Time = ev.Time,
                        GraphX = GetGraphXForTime(ev.Time, ii.GraphStartTime, ii.GraphEndTime, ii.GraphWidth)
                    });
                }
            }
            ii.UpdateSec = 300;
            return ii;
        }

        public static int GetGraphXForTime(int time, int startTime, int endTime, int graphWidth)
        {
            if (time < startTime || time > endTime) return -1;
            int tw = endTime - startTime;
            if (tw <= 0) return -1;
            var sx = ((double)(time - startTime)) * graphWidth / tw;
            return (int) Math.Round(sx);
        }

        public void DrawGraph(Lib.Graph.GraphDefinition gd, Lib.Graph.DrawOptions opts, Stream writeImageTo)
        {
            throw new NotImplementedException();
        }





        public TimeSeriesData ExportGraphData(GraphDefinition gd, DrawOptions opts)
        {
            var cmdtext = BuildRrdGraphCmdline(gd, opts, null, null, true);
            string ret = RunRrdWithCommandline(cmdtext);
            var doc = new XmlDocument();
            doc.LoadXml(ret);
            var x = new TimeSeriesData();
            x.DataColumns = new List<string>();
            x.Rows = new List<TimeSeriesData.Row>();
            x.StartTime = FromUnixTime(int.Parse(doc.DocumentElement.SelectSingleNode("meta/start").InnerText));
            x.EndTime = FromUnixTime(int.Parse(doc.DocumentElement.SelectSingleNode("meta/end").InnerText));
            x.StepSecs = int.Parse(doc.DocumentElement.SelectSingleNode("meta/step").InnerText);
            foreach (XmlElement el in doc.DocumentElement.SelectNodes("meta/legend/entry"))
            {
                x.DataColumns.Add(el.InnerText);
            }
            foreach (XmlElement el in doc.DocumentElement.SelectNodes("data/row"))
            {
                var r = new TimeSeriesData.Row();
                r.T = Int32.Parse(el.SelectSingleNode("t").InnerText);
                r.Timestamp = FromUnixTime(r.T);
                var v = new List<double>();
                foreach (XmlElement xv in el.SelectNodes("v"))
                {
                    v.Add(ParseDouble(xv.InnerText));
                }
                r.V = v.ToArray();
                x.Rows.Add(r);
            }
            return x;
        }
    }
}
