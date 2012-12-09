using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services
{
    public interface IEvalScript
    {
        object Eval(string script);
    }

    public interface IScriptHost
    {
        IEvalScript GetScriptInterpreter(string interpreterId);
    }
}
