using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.DSL;
using NLog;
using System.Reflection;

namespace CogMon.Agent.BooScript
{
    internal class ScriptDSLEngine : DslEngine
    {
        public override string CanonizeUrl(string url)
        {
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(_baseDir, url));
        }

        private Logger log = LogManager.GetCurrentClassLogger();
        private string _baseDir;

        public ScriptDSLEngine(string baseDirectory)
        {
            _baseDir = baseDirectory;
            Namespaces = new string[] { };
            ReferencedAssemblies = new Assembly[] { };
        }

        public string[] Namespaces { get; set; }
        public Assembly[] ReferencedAssemblies { get; set; }
        public bool RefAllLoadedAssemblies { get; set; }

    
        protected override void CustomizeCompiler(Boo.Lang.Compiler.BooCompiler compiler, Boo.Lang.Compiler.CompilerPipeline pipeline, string[] urls)
        {
            compiler.Parameters.Ducky = true;
                
            List<Assembly> asmss = new List<Assembly>();
            if (RefAllLoadedAssemblies)
            {
                asmss.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            }
            if (ReferencedAssemblies != null) asmss.AddRange(ReferencedAssemblies);
            
            foreach (Assembly asm in asmss)
            {
                try
                {
                    string loc = asm.Location;
                    if (!compiler.Parameters.References.Contains(asm)) compiler.Parameters.References.Add(asm);
                }
                catch (Exception) { log.Debug("Error adding assembly dependency: {0}", asm.FullName); }
            }

            pipeline.Insert(1, new ImplicitBaseClassCompilerStep(typeof(ScriptDSLBase), "Prepare", Namespaces));
            var st2 = new AutoReferenceFilesCompilerStep(_baseDir + "\\include");
            pipeline.Insert(2, st2);
        }

    }
}
