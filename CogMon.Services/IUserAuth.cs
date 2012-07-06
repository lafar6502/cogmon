using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib;
using System.Security.Principal;

namespace CogMon.Services
{
    /// <summary>
    /// User authentication functionality
    /// </summary>
    public interface IUserAuth
    {
        bool PasswordAuthenticate(string user, string password);
        /// <summary>
        /// Authenticate user based on external identity
        /// This is used for example for automatic registration 
        /// of user accounts with Windows authentication
        /// </summary>
        /// <param name="userIdentity"></param>
        /// <returns></returns>
        bool IdentityAuthenticate(IIdentity userIdentity);
        /// <summary>
        /// Token authentication used with externally provided tokens
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        UserInfo AuthenticateByToken(string token);
        /// <summary>
        /// Get cogmon user information for specified user identity
        /// </summary>
        /// <param name="userIdentity"></param>
        /// <returns></returns>
        UserInfo GetUserInfo(IIdentity userIdentity);
    }
}
