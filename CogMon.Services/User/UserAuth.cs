using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using CogMon.Services.Dao;
using NLog;
using CogMon.Lib;
using System.Security.Principal;

namespace CogMon.Services.User
{
    public class UserAuth : IUserAuth
    {
        public MongoDatabase Db { get; set; }
        private Logger log = LogManager.GetCurrentClassLogger();

        public bool PasswordAuthenticate(string user, string password)
        {

            var usr = Db.GetCollection<UserInfo>().Find(Query.EQ("Login", user)).FirstOrDefault();
            if (usr == null)
            {
                log.Info("user not found: {0}", user);
                return false;
                usr = new UserInfo
                {
                    Login = user,
                    Passwd = password,
                    Email = user + "@wypas",
                    Active = true
                };
                Db.GetCollection<UserInfo>().Insert(usr);
                return true;
            }
            log.Info("User {0} found: {1}", usr.Login, usr.Id);
            return true;
        }



        private Dictionary<string, UserInfo> _users = new Dictionary<string, UserInfo>();

        public UserInfo CurrentUserInfo(IIdentity user)
        {
            if (string.IsNullOrEmpty(UserSessionContext.CurrentUserLogin)) return null;
            UserInfo ret;
            if (_users.TryGetValue(UserSessionContext.CurrentUserLogin, out ret)) return ret;
            var lst = Db.Find<UserInfo>(x => x.Login == UserSessionContext.CurrentUserLogin && x.Active == true).ToList();
            if (lst.Count == 0)
            {
                log.Info("User not found: " + UserSessionContext.CurrentUserLogin);
                return null;
            }
            var ui = lst[0];
            lock (_users)
            {
                if (_users.TryGetValue(UserSessionContext.CurrentUserLogin, out ret)) return ret;
                _users[UserSessionContext.CurrentUserLogin] = ui;
            }
            return ui;
        }


        public bool IdentityAuthenticate(IIdentity userIdentity)
        {
            throw new NotImplementedException();
        }

        public UserInfo GetUserInfo(IIdentity userIdentity)
        {
            throw new NotImplementedException();
        }
    }
}
