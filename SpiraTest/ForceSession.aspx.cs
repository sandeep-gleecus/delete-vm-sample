using System;
using System.Web.Security;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to allowing the user to choose to
	/// make this session the enabled one for this user (in the case of a user logged in to multiple sessions)
	/// </summary>
	/// <remarks>Inherits off PageBase not PageLayout because we don't want the OnLoad() base implementation to be used </remarks>
	public partial class ForceSession : PageBase
	{
		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			//Display the appropriate product name
			this.lblLoggedInMessage.Text = String.Format(Resources.Messages.ForceSession_AccountSignedInMultiple, ConfigurationSettings.Default.License_ProductType);

            //Attach event handlers to buttons
			this.btnLogOut.Click += new EventHandler(btnLogOut_Click);
			this.btnSignOffOthers.Click += new EventHandler(btnSignOffOthers_Click);
		}

		/// <summary>
		/// Handles click events on the log out button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnLogOut_Click(object sender, EventArgs e)
		{
			//Abandon the session and redirect to the login page
			Session.Abandon();
			FormsAuthentication.SignOut();
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Login, 0, 0), true);
		}

		/// <summary>
		/// Handles click events on the sign off others button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnSignOffOthers_Click(object sender, EventArgs e)
		{
			//Remove the other sessions
			KillOtherUserSessions();

            //Now register this session
            int userId = (int)Membership.GetUser().ProviderUserKey;
            Web.Global.RegisterSession(Session.SessionID, userId);

			//We finally redirect back to the original page
			string originalUrl = Request.QueryString[GlobalFunctions.PARAMETER_ORIGINAL_URL];
			Response.Redirect(originalUrl, true);
		}

		#endregion

	}
}
