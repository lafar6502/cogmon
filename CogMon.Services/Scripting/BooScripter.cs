using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGinnBPM.MessageBus;
using MongoDB.Driver;
using Boo.Lang.Interpreter;
using BL = Boo.Lang;

namespace CogMon.Services.Scripting
{
    public interface IEvalScript
    {
        object Eval(string script);
    }

    public class BooScripter
    {
        public MongoDatabase Db { get; set; }
        public IServiceResolver ServiceResolver { get; set; }
        public IDataSeriesRepository DSRepo { get; set; }
        public string InitScriptFile { get; set; }        

        internal class BooEnv : IEvalScript
        {
            private InteractiveInterpreter _interp;

            public BooEnv(BooScripter parent)
            {
                _interp = new InteractiveInterpreter();
                _interp.Ducky = true;
                _interp.RememberLastValue = true;
                _interp.SetValue("Db", parent.Db);
                _interp.SetValue("Services", parent.ServiceResolver);
                _interp.SetValue("DSRepo", parent.DSRepo);
                _interp.References.Add(typeof(BooEnv).Assembly);
                _interp.References.Add(typeof(CogMon.Lib.DataRecord).Assembly);
            }

            public object Eval(string script)
            {
                var cc = _interp.Eval(script);
                if (cc.Errors.Count > 0)
                {
                    throw new Exception("Error: " + cc.Errors[0].ToString());
                }
                else return _interp.LastValue;
            }
        }


        public IEvalScript GetScriptInterpreter()
        {
            var be = new BooEnv(this);
            if (!string.IsNullOrEmpty(this.InitScriptFile))
            {
            }
            return be;
        }
    }
}
