using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib
{
    /// <summary>
    /// Authentication token for external authentication
    /// </summary>
    public class AuthToken
    {
        public string _id { get; set; }
        public bool Active { get; set; }
        public string UserIdentity { get; set; }
        public string UserSID { get; set; }
    }
}
