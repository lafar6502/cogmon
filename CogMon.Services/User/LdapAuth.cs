using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib;
using System.Security.Principal;
using MongoDB.Driver;
using System.DirectoryServices;

namespace CogMon.Services.User
{
    
    public class LdapAuth : CogMonAuth
    {
        

        protected override UserInfo UserFound(IIdentity identity, UserInfo userRec)
        {
            return base.UserFound(identity, userRec);
        }


        protected override UserInfo UserNotFound(IIdentity identity)
        {
            
            return base.UserNotFound(identity);
        }

        
    }
}
