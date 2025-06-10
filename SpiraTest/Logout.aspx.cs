using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Logout : PageBase
    {
        /// <summary>
        /// Logs out the current user and ends sessions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Kill all sessions for this user
            KillAllSessionsForUser();

            //Remove the authentication cookie
            FormsAuthentication.SignOut();

            //Clear out any session variables
            Session.Abandon();

            //Redirect to the login page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Login, 0, 0), true);

        }

        /// <summary>
        /// Kills all user sessions for the currently logged in user (not API sessions)
        /// </summary>
        protected void KillAllSessionsForUser()
        {
            MembershipUser user = Membership.GetUser();
            if (user != null && user.ProviderUserKey != null)
            {
                int userId = (int)user.ProviderUserKey;
                if (userId > 0)
                {
                    Web.Global.KillUserSessions(userId, false);
                }
            }
        }
    }
}