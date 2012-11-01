using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib.DataSeries;

namespace CogMon.Services
{
    

    /// <summary>
    /// Performance counters interface
    /// </summary>
    public interface IPerfCounters
    {
        void AddEvent(string counterId, int value);
        PerfCounterStats GetCurrentStats(string counterId, bool reset);
    }
}
