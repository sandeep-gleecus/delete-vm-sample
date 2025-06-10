using System.Web.UI;
using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.Security;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is used when password expiration is enabled and the user's password is
    /// either expired or has never been changed and that option is enabled
	/// </summary>
	public partial class PasswordExpired : PageBase
	{
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.PasswordExpired::";

        #region Properties

        /// <summary>
        /// Displays whether the password cannot contain names or not
        /// </summary>
        protected string PasswordNoNamesLegend
        {
            get
            {
                if (ConfigurationSettings.Default.Security_PasswordNoNames)
                {
                    return Resources.Main.UserProfile_PasswordNoNames;
                }
                return "";
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, System.EventArgs e)
		{
            //Make sure password expiration or change on first login is enabled
            if (ConfigurationSettings.Default.Security_PasswordChangeInterval <= 0 && !ConfigurationSettings.Default.Security_ChangePasswordOnFirstLogin)
            {
                //Simply Redirect back to the My Page
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage), true);
            }

            //Get the username from the querystring
            this.ChangeUserPassword.UserName = Request.QueryString[GlobalFunctions.PARAMETER_USERNAME];

			//Display the page title and product name
			((MasterPages.Login)this.Master).PageTitle = Resources.Main.PasswordExpired_Title;

            //Register event handlers
            this.ChangeUserPassword.ChangedPassword += ChangeUserPassword_ChangedPassword;

            //Databind the page
            this.DataBind();
        }

        /// <summary>
        /// Called when the password is successfully changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeUserPassword_ChangedPassword(object sender, EventArgs e)
        {
            //Simply Redirect back to the My Page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage), true);
        }

        #endregion
    }
}
