using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NLog;

namespace CogMon.Agent.PerfMon
{
    public class UDPPerfmonListener
    {

        public void Test()
        {
            System.Net.Sockets.UdpClient cli = new System.Net.Sockets.UdpClient(11000);
            
            cli.BeginReceive(new AsyncCallback(x =>
            {
                IPEndPoint ep = null;
                var ret = cli.EndReceive(x, ref ep);
            }), null);
        }
    }
}
