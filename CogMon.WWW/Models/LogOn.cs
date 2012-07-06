using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CogMon.WWW.Models
{
    public class LogOn
    {
        public string UserName { get;set;}
        public string Password { get;set;}
        public bool RememberMe { get; set; }
    }
}