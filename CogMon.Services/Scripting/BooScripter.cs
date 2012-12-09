using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGinnBPM.MessageBus;
using MongoDB.Driver;
using Boo.Lang.Interpreter;
using BL = Boo.Lang;
using System.Collections.Concurrent;

namespace CogMon.Services.Scripting
{
    

    public class BooScripter : IScriptHost
    {
        public MongoDatabase Db { get; set; }
        public IServiceResolver ServiceResolver { get; set; }
        public IDataSeriesRepository DSRepo { get; set; }
        public string InitScriptFile { get; set; }
        private ConcurrentDictionary<string, BooEnv> _interpreters = new ConcurrentDictionary<string, BooEnv>();

        internal class BooEnv : IEvalScript
        {
            private InteractiveInterpreter _interp;
            public DateTime CreatedDate { get; set; }

            public BooEnv(BooScripter parent)
            {
                CreatedDate = DateTime.Now;
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


        public IEvalScript GetScriptInterpreter(string id)
        {
            var en = _interpreters.GetOrAdd(id, x =>
            {
                var be = new BooEnv(this);
                be.Eval(@"
import System
import NLog
import CogMon.Lib
import CogMon.Lib.DataSeries
import CogMon.Lib.Scheduling
import CogMon.Lib.Graph
import CogMon.Services
import CogMon.Services.Dao
import CogMon.Services.Events
import NGinnBPM.MessageBus
");
                if (!string.IsNullOrEmpty(this.InitScriptFile))
                {
                    be.Eval(System.IO.File.ReadAllText(this.InitScriptFile, Encoding.UTF8));
                }
                return be;
            });
            
            
            return en;
        }
    }
}
