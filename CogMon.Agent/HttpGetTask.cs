using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Net;
using NGinnBPM.MessageBus;
using CogMon.Lib;
using System.IO;

namespace CogMon.Agent
{
    public class HttpGetTask : JobBase
    {
        

        protected override void Run()
        {
            try
            {
                log.Debug("http request to {0}", this.ScriptName);
                HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(this.ScriptName);
                using (var resp = (HttpWebResponse)rq.GetResponse())
                {
                    string body = null;
                    if (resp.ContentLength > 0)
                    {
                        using (var stm = resp.GetResponseStream())
                        {
                            body = new StreamReader(stm, Encoding.GetEncoding(string.IsNullOrEmpty(resp.ContentEncoding) ? "utf-8" : resp.ContentEncoding)).ReadToEnd();
                        }
                    }
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        log.Debug("Success. {0}", body);
                        if (!string.IsNullOrEmpty(this.DataSeries))
                        {
                            var dr = ParseData(new StringReader(body));
                            this.UpdateDataSource(dr);
                        }
                    }
                    else
                    {
                        log.Warn("Http request error for {0}: {1}", ScriptName, body);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error: {0}", ex);
                throw;
            }
        }
    }
}
