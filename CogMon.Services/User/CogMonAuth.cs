using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib;
using System.Security.Principal;
using MongoDB.Driver;
using NLog;

namespace CogMon.Services.User
{
    public class CogMonAuth : IUserAuth
    {
        public MongoDatabase Db { get; set; }
        protected Logger log = LogManager.GetCurrentClassLogger();

        protected class UserEntry
        {
            public DateTime ReadDate { get; set; }
            public UserInfo User { get; set; }
        }

        protected System.Collections.Concurrent.ConcurrentDictionary<string, UserEntry> _users = new System.Collections.Concurrent.ConcurrentDictionary<string, UserEntry>();


        public virtual bool PasswordAuthenticate(string user, string password)
        {
            var users = Db.Find<UserInfo>(x => x.Login == user && x.Active == true).ToList();
            UserInfo ui = null;
            if (users.Count == 0) return false;
            ui = users[0];
            if (!ui.Active) return false;
            return true;
        }

        public virtual bool IdentityAuthenticate(IIdentity userIdentity)
        {
            if (!userIdentity.IsAuthenticated) return false;
            var ui = GetUserInfo(userIdentity);
            return ui != null && ui.Active;
        }

        public virtual UserInfo GetUserInfo(IIdentity userIdentity)
        {
            UserEntry ue = null;
            if (_users.TryGetValue(userIdentity.Name, out ue))
            {
                if (ue.ReadDate.AddMinutes(5) < DateTime.Now)
                {
                    ue = null;
                }
                else
                {
                    return ue.User;
                }
            }
            //: ue == null now
            var users = Db.Find<UserInfo>(x => x.Login == userIdentity.Name && x.Active == true).ToList();
            UserInfo ui = null;
            if (users.Count == 0)
            {
                lock (this)
                {
                    if (_users.TryGetValue(userIdentity.Name, out ue) && ue.ReadDate.AddMinutes(5) < DateTime.Now)
                        return ue.User;
                    else 
                        ui = UserNotFound(userIdentity);
                }
            }
            else
            {
                ui = UserFound(userIdentity, users[0]);
            }
            if (ui != null)
            {
                ui.Passwd = null;
                ue = new UserEntry
                {
                    ReadDate = DateTime.Now,
                    User = ui
                };
                _users.TryAdd(userIdentity.Name, ue);
            }
            //
            return ui;
        }

        protected virtual UserInfo UserNotFound(IIdentity identity)
        {
            return null;
        }

        protected virtual UserInfo UserFound(IIdentity identity, UserInfo userRec)
        {
            return userRec;
        }


        public UserInfo AuthenticateByToken(string token)
        {
            var tok = Db.Find<AuthToken>(x => x._id == token).FirstOrDefault();
            if (tok == null || !tok.Active)
            {
                log.Info("Auth token not found or not active: {0}", token);
                return null;
            }
            tok.Active = false;
            GenericIdentity gi = new GenericIdentity(tok.UserIdentity, "Token");
            
            var ui = this.GetUserInfo(gi);
            if (ui == null)
            {
                log.Warn("User not found for token {0} uid {1} sid {2}", token, tok.UserIdentity, tok.UserSID);
            }
            Db.GetCollection<AuthToken>().Save(tok);
            return ui;
        }
    }
}
