using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace CogMon.Agent.Tools
{
    public class ProcessUtil
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private static string EscArg(string arg)
        {
            if (arg.IndexOfAny(new char[] { ' ', '\t', '\r', '\n' }) < 0)
                return arg;
            return "\"" + arg + "\"";
        }

        public static int ExecProgram(string exe, IEnumerable<string> arguments, out string output)
        {
            return ExecProgram(exe, null, arguments, null, out output);
        }

        public static int ExecProgram(string exe, IEnumerable<string> arguments, string rawCommandLine, out string output)
        {
            return ExecProgram(exe, null, arguments, rawCommandLine, out output);
        }

        public static int ExecProgram(string exe, string workingDir, IEnumerable<string> arguments, string rawCommandline, out string output, int timeoutSec = 120)
        {
            if (arguments == null) arguments = new string[0];
            var dbg = arguments.Any(x => x == "-__debug__-");
            var args = arguments.Where(x => x != "-__debug__-").Select(x => EscArg(x));
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = (string.IsNullOrEmpty(rawCommandline) ? "" : rawCommandline + " ") + string.Join(" ", args)
            };
            if (workingDir != null) psi.WorkingDirectory = workingDir;
            try
            {
                
                

                var sout = new StringBuilder();
                var sErr = new StringBuilder();
                using (var p = new Process())
                {
                    p.StartInfo = psi;
                    p.EnableRaisingEvents = true;
                    p.OutputDataReceived += (s, e) => sout.AppendLine(e.Data);
                    p.ErrorDataReceived += (s, e) => sErr.AppendLine(e.Data);
                    var b0 = p.Start();
                    if (dbg)
                    {
                        log.Warn("Process started PID {0} r {1}", p.Id, b0);
                    }
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    bool ret = p.WaitForExit(timeoutSec * 1000);
                    if (!ret)
                    {
                        throw new Exception("Process timeout");
                    }

                    output = sout.ToString();
                    var errs = sErr.ToString().Trim();
                    if (errs.Length > 0)
                    {
                        output += "\r\n------ STDERR --------\r\n" + errs;
                    }
                    if (dbg || errs.Length > 0 || p.ExitCode != 0)
                    {
                        log.Warn("{0} {1}", psi.FileName, psi.Arguments);
                        if (sErr.Length > 0)
                        {
                            log.Warn("STDERR: {0}", errs);
                        }
                    }
                    return p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error running {0} {1}: {2}", psi.FileName, psi.Arguments, ex);
                throw;
            }
        }

        public static int ExecProgram(string exe, IEnumerable<string> arguments)
        {
            var args = arguments.Select(x => "\"" + x + "\"");

            var psi = new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true,
                Arguments = string.Join(" ", args),
                RedirectStandardOutput = false
            };
            using (var proc = Process.Start(psi))
            {
                bool ret = proc.WaitForExit(120 * 1000);
                if (!ret)
                {
                    throw new Exception("Process timeout");
                }
                return proc.ExitCode;
            }
        }
    }
}
