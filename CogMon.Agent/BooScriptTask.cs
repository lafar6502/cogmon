using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Agent.BooScript;
using NLog;
using CogMon.Lib;

namespace CogMon.Agent
{
    public class BooScriptTask : JobBase
    {

        public IRunScript Scripter { get; set; }

        protected override void Run()
        {
            var opts = this.Options == null ? new Dictionary<string, object>() : new Dictionary<string, object>(Options);
            opts["VariableNames"] = this.Variables;
            opts["JobArguments"] = this.Arguments;
            opts["Job"] = this;
            var ret = Scripter.RunScript(this.ScriptName, opts);
            
            foreach(var dr in ret)
            {
                if (string.IsNullOrEmpty(dr.Series)) dr.Series = this.DataSeries;
            }
            this.UpdateDataSource(ret);
        }
    }
}
