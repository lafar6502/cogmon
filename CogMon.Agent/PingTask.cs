using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Data.Common;
using System.Configuration;
using CogMon.Lib;
using System.Net.NetworkInformation;

namespace CogMon.Agent
{
    /// <summary>
    /// ICMP or http ping task
    /// Returns 'undefined' if no response received
    /// </summary>
    public class PingTask : JobBase
    {
        protected override void Run()
        {
            string hostName = this.Arguments;
            if (string.IsNullOrEmpty(hostName)) return;
            if (hostName.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                DoHttpPing(hostName);
                return;
            }
            
            Ping p = new Ping();
            var resp = p.Send(hostName);
            log.Debug("Pinged {0}. Response: {1}, time: {2} ms", hostName, resp.Status, resp.RoundtripTime);
            DataRecord dr = new DataRecord { Series = this.DataSeries, Data = new double[] { resp.Status == IPStatus.Success ? resp.RoundtripTime : Double.NaN } };
            UpdateDataSource(dr);
        }

        protected void DoHttpPing(string hostName)
        {
            DataRecord dr = new DataRecord { Series = this.DataSeries, Data = new double[] { Double.NaN } };
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.DownloadData(hostName);
                sw.Stop();
                dr.Data[0] = sw.ElapsedMilliseconds;
            }
            catch (System.Net.WebException ex)
            {
                log.Warn("Http ping error in job {0} ({1}): {2}", Id, hostName, ex);
                dr.Data[0] = Double.NaN;
            }
            UpdateDataSource(dr);
        }

    }
}
