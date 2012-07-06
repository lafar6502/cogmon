using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using CogMon.Lib;

namespace CogMon.Services
{
    /// <summary>
    /// Current user's session context
    /// </summary>
    public class UserSessionContext
    {
        [ThreadStatic]
        private static IIdentity _curUser;
        public static string CurrentUserRecordId
        {
            get 
            { 
                var r = UserAuth.GetUserInfo(CurrentUserIdentity);
                return r == null ? null : r.Id;
            }
        }

        
        public static IIdentity CurrentUserIdentity
        {
            get { return _curUser; }
            set { _curUser = value; }
        }

        public static IUserAuth UserAuth { get; set; }

        public static UserInfo CurrentUserInfo
        {
            get { return UserAuth.GetUserInfo(CurrentUserIdentity); }
        }
        
    }

    
}
