using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NLog;
using System.Net.Sockets;
using NGinnBPM.MessageBus.Impl;

namespace CogMon.Agent.PerfMon
{
    public class UDPPerfmonListener2 : IStartableService
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        public int Port { get; set; }
        public string LocalIP { get; set; }
        private Socket _receiverSocket = null;
        public PerfCounterStore Counters { get; set; }



        public UDPPerfmonListener2()
        {
            Port = 29983;
        }


        protected void DoReceive(IAsyncResult ar)
        {
            if (!_stop && _receiverSocket != null)
            {
                WaitForNextPacket();
            }
            EndPoint remote = null;
            string data = null;
            try
            {
                int nRead = _receiverSocket.EndReceive(ar);
                var buf = ar.AsyncState as byte[];
                if (buf == null) throw new Exception("no buf buf");
                data = Encoding.UTF8.GetString(buf, 0, nRead);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            if (log.IsDebugEnabled) log.Debug("From {0}: {1}", remote, data);
            if (Counters != null)
            {
                System.IO.StringReader sr = new System.IO.StringReader(data);
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0) continue;
                    int idx = line.LastIndexOf(':');
                    if (idx < 0)
                    {
                        log.Info("Invalid data from {0}: {1}", remote, line);
                        continue;
                    }
                    string iv = line.Substring(idx + 1);
                    double v;
                    if (!Double.TryParse(iv, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out v))
                    {
                        log.Info("Invalid data from {0}: {1}", remote, line);
                        continue;
                    }
                    string cname = line.Substring(0, idx).Trim();
                    if (cname.Length > 0)
                    {
                        Counters.UpdateCounter(cname, remote == null ? "" : remote.Serialize().ToString(), (int)v);
                    }
                }
            }
            else
            {
                log.Info("From {0}: {1}", remote == null ? "" : remote.Serialize().ToString(), data);
            }
        }

        public bool IsRunning
        {
            get { return _receiverSocket != null; }
        }

        private volatile bool _stop = false;

        private EndPoint _tmp = new IPEndPoint(IPAddress.Any, 0);

        protected void WaitForNextPacket()
        {
            if (_receiverSocket == null || _stop) return;
            var buf = new byte[500];
            _receiverSocket.BeginReceive(buf, 0, buf.Length, SocketFlags.None, new AsyncCallback(DoReceive), buf);
        }

        public void Start()
        {
            if (IsRunning) Stop();
            _stop = false;
            IPAddress addr = string.IsNullOrEmpty(LocalIP) || LocalIP == "*" ? IPAddress.Any : IPAddress.Parse(LocalIP);
            IPEndPoint endp = new IPEndPoint(addr, Port);
            log.Debug("Will listen on {0}", endp.ToString());
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
            sock.ReceiveBufferSize = 50000;
            sock.Bind(endp);
            _receiverSocket = sock;
            WaitForNextPacket();
            log.Debug("Service started, listening on {0}", endp);
        }

        public void Stop()
        {
            _stop = true;
            if (_receiverSocket != null)
            {
                _receiverSocket.Close();
                _receiverSocket.Dispose();
                _receiverSocket = null;
                log.Info("service stopped");
            }
        }
    }
}
