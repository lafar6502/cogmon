using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Agent.BooScript;
using NLog;
using CogMon.Lib;
using Boo.Lang.Interpreter;

namespace CogMon.Agent
{
    public class InlineScriptTask : JobBase
    {
        protected InteractiveInterpreter _interp = new InteractiveInterpreter();
        
        protected override void Run()
        {
            _interp.SetValue("job", this);
            _interp.Eval(this.ScriptName);
        }
    }
}
