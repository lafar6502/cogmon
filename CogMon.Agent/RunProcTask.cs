using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CogMon.Agent
{
    public class RunProcTask : JobBase
    {
        protected override void Run()
        {
            var bdir = AppDomain.CurrentDomain.BaseDirectory;

            var psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.FileName = this.ScriptName == null ? null : this.ScriptName.Replace("${BaseDir}", bdir);
            psi.Arguments = this.Arguments == null ? null : this.Arguments.Replace("${BaseDir}", bdir);
            
            if (this.Options != null)
            {
                foreach (string k in this.Options.Keys)
                {
                    psi.EnvironmentVariables[k] = Convert.ToString(this.Options[k]);
                }
            }
            log.Info("Starting {0} {1}", psi.FileName, psi.Arguments);
            var outData = new StringBuilder();
            var errData = new StringBuilder();
            try
            {
                using (var proc = Process.Start(psi))
                {
                    /*proc.OutputDataReceived += (s, e) =>
                    {
                        outData.Append(e.Data);
                    };
                    proc.ErrorDataReceived += (s, e) =>
                    {
                        errData.Append(e.Data);
                    };
                    */
                    outData.Append(proc.StandardOutput.ReadToEnd());
                    errData.Append(proc.StandardError.ReadToEnd());
                    if (!proc.WaitForExit(30000))
                    {
                        throw new Exception("Process timed out");
                    }

                    if (proc.ExitCode != 0)
                        throw new Exception("Process exited with code " + proc.ExitCode);
                    if (errData.Length > 0)
                        throw new Exception("Process returned an error: " + errData.ToString());
                }
                if (outData.Length == 0)
                {
                    log.Warn("Process returned no data");
                    return;
                }
                if (!string.IsNullOrEmpty(this.DataSeries))
                {
                    var rec = ParseData(new StringReader(outData.ToString()));
                    if (rec != null)
                    {
                        this.UpdateDataSource(rec);
                    }
                }
                else
                {
                    log.Debug("{0} ignoring output", this.Id);
                }
            }
            catch (Exception ex) {
                log.Error("Error running {0} {1} {2}: {3}. ERR: {4}, OUT: {5}", this.Id, this.ScriptName, this.Arguments, ex.Message, errData.ToString(), outData.ToString());
                throw;
            }
        }

    }
}
