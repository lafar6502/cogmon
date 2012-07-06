using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.DSL;
using NLog;

namespace CogMon.Agent.BooScript
{
    public interface IRunScript
    {
        /// <summary>
        /// Runs a script
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        IList<Lib.DataRecord> RunScript(string scriptName, IDictionary<string, object> inputParameters);
    }

    public class BooScriptManager : IRunScript
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        public string BaseDirectory { get; set; }
        private DslFactory _dsl = null;
        protected void InitializeIfNeeded()
        {
            lock (this)
            {
                if (_dsl != null) return;
                _dsl = new DslFactory();
                _dsl.BaseDirectory = BaseDirectory;
                _dsl.Register<ScriptDSLBase>(new ScriptDSLEngine(BaseDirectory));
            }
        }

        public IList<Lib.DataRecord> RunScript(string scriptName, IDictionary<string, object> inputParameters)
        {
            InitializeIfNeeded();
            var sb = _dsl.Create<ScriptDSLBase>(scriptName);
            sb.Initialize(inputParameters);
            return sb.DataRecords;
        }
    }
}
