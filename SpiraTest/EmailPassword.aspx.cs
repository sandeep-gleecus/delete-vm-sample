using System;
using System.Web.UI;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to allowing the user to enter his
	/// username and have his password emailed to the email address in his profile
	/// </summary>
	public partial class EmailPassword : PageBase
	{
        /// <summary>
        /// The return URL (the login page)
        /// </summary>
        protected string ReturnUrl
        {
            get
            {
                return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Login, 0, 0);
            }
        }

		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			//Display the page title
            ((MasterPages.Login)this.Master).PageTitle = Resources.Main.EmailPassword_Title;

            //Databind the page
            this.PasswordRecovery.DataBind();

            //Specify the different failure messages (so that we can localize them)
            this.PasswordRecovery.UserNameFailureText = Resources.Messages.EmailPassword_UserNameFailureText;
            this.PasswordRecovery.GeneralFailureText = Resources.Messages.EmailPassword_GeneralFailureText;
            this.PasswordRecovery.QuestionFailureText = Resources.Messages.EmailPassword_QuestionFailureText;
        }

		/// <summary>
		/// Handles the event raised when the Cancel button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void btnCancel_Click(object sender, EventArgs e)
		{
			//Simply Redirect back to the Login page
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Login, 0, 0), true);
		}

		#endregion

	}
}
