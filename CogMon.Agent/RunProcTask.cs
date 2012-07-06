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
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.FileName = this.ScriptName;
            psi.Arguments = this.Arguments;
            
            log.Info("Starting {0} {1}", psi.FileName, psi.Arguments);
            string data = null;
            using (var proc = Process.Start(psi))
            {
                data = proc.StandardOutput.ReadToEnd();
                string err = proc.StandardError.ReadToEnd();
                if (!proc.WaitForExit(30000))
                    throw new Exception("Process timed out");
                if (proc.ExitCode != 0)
                    throw new Exception("Process exited with code " + proc.ExitCode);
                if (err.Length > 0)
                    throw new Exception("Process returned an error: " + err);
            }
            if (string.IsNullOrEmpty(data))
            {
                log.Warn("Process returned no data");
                return;
            }
            var rec = ParseData(new StringReader(data));
            this.UpdateDataSource(rec);
        }
    }
}
