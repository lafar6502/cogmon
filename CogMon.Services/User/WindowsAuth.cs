using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib;
using System.Security.Principal;
using MongoDB.Driver;
using System.DirectoryServices.AccountManagement;

namespace CogMon.Services.User
{
    /// <summary>
    /// Windows/domain authentication
    /// </summary>
    public class WindowsAuth : CogMonAuth
    {
        public WindowsAuth()
        {
            ContextType = "Domain";
            Security = ContextOptions.Negotiate;
        }

        protected override UserInfo UserFound(IIdentity identity, UserInfo userRec)
        {
            return base.UserFound(identity, userRec);
        }

        public string ContextType { get; set; }
        public string DomainUser { get; set; }
        public string Password { get; set; }
        public string ServerName { get; set; }
        public string Container { get; set; }
        public ContextOptions Security { get; set; }

        protected PrincipalContext GetPrincipalContext()
        {
            var ct = (ContextType) Enum.Parse(typeof(ContextType), ContextType);
            var pc = new PrincipalContext(ct, ServerName, Container, Security, DomainUser, Password);
            return pc;
        }

        protected UserInfo UpdateUserInfo(WindowsIdentity wi, UserInfo ui)
        {
            
            ui.Login = wi.Name;
            ui.MemberOf = new List<string>();
            ui.MemberOf.Add("ALL");
            ui.Name = wi.User.Translate(typeof(NTAccount)).Value;
            string[] pts = wi.Name.Split('\\');
            string domain = pts.Length > 1 ? pts[0] : null;
            foreach (var g in wi.Groups)
            {
                var gn = g.Translate(typeof(NTAccount)).Value;
                if (gn == null) continue;
                if (domain != null)
                {
                    if (!gn.StartsWith(domain + '\\')) continue;
                    gn = gn.Substring(domain.Length + 1);
                }
                if (!ui.MemberOf.Contains(g.Value)) ui.MemberOf.Add(g.Value);
            }
            return ui;
        }


        protected void UpdateUser(UserPrincipal up, UserInfo user)
        {
            user.Name = up.DisplayName;
            user.Email = up.EmailAddress;
            user.Active = true;
            foreach (var g in up.GetGroups())
            {
                user.MemberOf.Add(g.SamAccountName);
            }
            user.ExtId = up.DistinguishedName;
        }

        protected override UserInfo UserNotFound(IIdentity identity)
        {
            if (!(identity.IsAuthenticated)) throw new Exception("Identity not authenticated");
            log.Info("Creating new user record for {0}", identity.Name);
            int idx = identity.Name.IndexOf('\\');
            string login = idx < 0 ? identity.Name : identity.Name.Substring(idx + 1);
            log.Info("Searching for login {0}", login);
            using (var pc = GetPrincipalContext())
            {
                UserPrincipal up = UserPrincipal.FindByIdentity(pc, login);
                if (up == null)
                {
                    log.Info("User not found: {0}", login);
                    return null;
                }
                log.Info("Creating new user record for {0}", up.DistinguishedName);
                var ui = new UserInfo();
                ui.Login = identity.Name;
                ui.Active = true;
                ui.MemberOf.Add("ALL");
                UpdateUser(up, ui);
                Db.GetCollection<UserInfo>().Save(ui);
                log.Info("User {0} saved: {1}", ui.Login, ui.Id);
                return ui;
            }
        }
    }
}
