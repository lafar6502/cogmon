using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Agent.Tools
{
    public class HttpUtil
    {
        public static string Download(string url)
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                return wc.DownloadString(url);
                
            }

        }
    }
}
