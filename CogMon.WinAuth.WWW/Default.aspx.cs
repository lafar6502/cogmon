using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NLog;
using MongoDB.Driver;
using CogMon.Lib;
using System.Security.Principal;

namespace CogMon.WinAuth.WWW
{
    public partial class _Default : System.Web.UI.Page
    {
        protected Logger log = LogManager.GetCurrentClassLogger();
        protected void Page_Load(object sender, EventArgs e)
        {
            log.Info("User: {0}, auth: {1}", Context.User.Identity.Name, Context.User.Identity.IsAuthenticated);
            if (User.Identity.IsAuthenticated)
            {
                WindowsIdentity wi = Context.User.Identity as WindowsIdentity;
                AuthToken at = new AuthToken { _id = Guid.NewGuid().ToString("N"), Active = true, UserIdentity = Context.User.Identity.Name };
                if (wi != null) at.UserSID = wi.User.Value;
                Global.Db.GetCollection("authtokens").Save(at);
                Response.Redirect("../Account/LogOnByToken?token=" + at._id);
            }
        }
    }
}
