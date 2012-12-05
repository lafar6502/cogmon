﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NLog;
using System.Net.Sockets;
using NGinnBPM.MessageBus.Impl;

namespace CogMon.Agent.PerfMon
{
    public class UDPPerfmonListener : IStartableService
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        public int Port { get; set; }
        public string LocalIP { get; set; }
        public PerfCounterStore Counters { get; set; }

        public UDPPerfmonListener()
        {
            Port = 29983;
        }

        public void Test()
        {
            System.Net.Sockets.UdpClient cli = new System.Net.Sockets.UdpClient(11000);
            cli.BeginReceive(new AsyncCallback(x => {
                
            }), null);

            cli.BeginReceive(new AsyncCallback(x =>
            {
                IPEndPoint ep = null;
                var ret = cli.EndReceive(x, ref ep);
            }), null);
        }

        protected void DoReceive(IAsyncResult ar)
        {
            UdpClient cli = ar.AsyncState as UdpClient;
            if (!_stop && cli != null)
            {
                cli.BeginReceive(new AsyncCallback(DoReceive), cli);
            }
            IPEndPoint remote = null;
            var ret = cli.EndReceive(ar, ref remote);
            string data = Encoding.UTF8.GetString(ret);
            log.Debug("From {0}: {1}", remote, data);
            if (Counters != null)
            {
                int idx = data.IndexOf(':');
                if (idx < 0)
                {
                    log.Info("Invalid data from {0}: {1}", remote, data);
                    return;
                }
                string iv = data.Substring(idx + 1);
                int v;
                if (!Int32.TryParse(iv, out v))
                {
                    log.Info("Invalid data from {0}: {1}", remote, data);
                    return;
                }
                Counters.UpdateCounter(data.Substring(0, idx), remote.Address.ToString(), v);
            }
            else
            {
                log.Info("From {0}: {1}", remote.Address, data);
            }
        }

        public bool IsRunning
        {
            get { return _sock != null; }
        }

        private volatile bool _stop = false;
        private UdpClient _sock = null;
        
        public void Start()
        {
            if (IsRunning) Stop();
            _stop = false;
            IPAddress addr = string.IsNullOrEmpty(LocalIP) || LocalIP == "*" ? IPAddress.Any : IPAddress.Parse(LocalIP);
            IPEndPoint endp = new IPEndPoint(addr, Port);
            log.Debug("Will listen on {0}", endp.ToString());
            UdpClient cli = new UdpClient(endp);
            cli.BeginReceive(new AsyncCallback(DoReceive), cli);
            log.Debug("Service started, listening on {0}", endp);
            _sock = cli;
        }

        public void Stop()
        {
            _stop = true;
            if (_sock != null)
            {
                _sock.Close();
                _sock = null;
                log.Info("service stopped");
            }
        }
    }
}