using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CogMon.Agent
{
    /// <summary>
    /// Parses a text log file in an incremental way (line by line)
    /// retrieving matching values
    /// </summary>
    public class LogGrepTask : JobBase
    {
        private int CurrentPosition { get; set; }
        
        protected override void Run()
        {
            
            
            
        }
    }
}
