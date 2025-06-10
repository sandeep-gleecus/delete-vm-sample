using Inflectra.OAuth2;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>This webform code-behind class is responsible to allowing the guest to enter his personal information, and have an email sent to the Administrator requesting a new account</summary>
	public partial class NeedAccount : PageBase
	{
		private const string CLASS_NAME = "Web.NeedAccount::";

		#region Properties
		/// <summary>Displays whether the password cannot contain names or not</summary>
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
		/// <summary>This sets up the page upon loading</summary>
		protected void Page_Load(object sender, EventArgs e)
		{
			string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're allowed to request new accounts
			if (!ConfigurationSettings.Default.Membership_AllowUserRegistration)
			{
				//Simply Redirect back to the Login page
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Login, 0, 0), true);
			}

			//Display the page title and product name
			((MasterPages.Login)Master).PageTitle = Resources.Main.Register_Title;

			//Register event handlers
			RegisterUser.ContinueDestinationPageUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Login, 0, 0);
			RegisterUser.CancelButtonClick += new EventHandler(RegisterUser_CancelButtonClick);

			//See if we have OAuth data to populate.
			processOAuth();

			//Databind the validators
			DataBind();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Redirects back to the login page when cancel clicked</summary>
		protected void RegisterUser_CancelButtonClick(object sender, EventArgs e)
		{
			//Simply Redirect back to the Login page
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Login, 0, 0), true);
		}
		#endregion

		private void processOAuth()
		{
			string METHOD_NAME = CLASS_NAME + "processOAuth()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//See if we have the Oauth item in the cookie.
			if (Request.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME] != null || Session[OAuthManager.AUTHTOCTRL_COOKNAME] != null)
			{
				//Get the cookie. 
				OAuthManager.UserLoginInfo userInfo = null;
				if (Request.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME] != null)
				{
					//Get the cookie value and remove it.
					string cookVal = Request.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Value;
					Response.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Value = ".";
					Response.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Expires = DateTime.UtcNow.AddYears(-1);

					//Set the session.
					Session[OAuthManager.AUTHTOCTRL_COOKNAME] = cookVal;

					//Get the object.
					userInfo = OAuthManager.UserLoginInfo.Parse(cookVal);
				}
				else if (Session[OAuthManager.AUTHTOCTRL_COOKNAME] != null)
				{
					string cookVal = (string)Session[OAuthManager.AUTHTOCTRL_COOKNAME];
					userInfo = OAuthManager.UserLoginInfo.Parse(cookVal);
				}

				//Deactivate the required validators.
				RequiredFieldValidator rq1 = RegisterUser.FindControlRecursive("PasswordRequired") as RequiredFieldValidator;
				if (rq1 != null) rq1.Enabled = false;
				RequiredFieldValidator rq2 = RegisterUser.FindControlRecursive("ConfirmPasswordRequired") as RequiredFieldValidator;
				if (rq2 != null) rq2.Enabled = false;
				RequiredFieldValidator rq3 = RegisterUser.FindControlRecursive("RequiredFieldValidator1") as RequiredFieldValidator;
				if (rq3 != null) rq3.Enabled = false;
				RequiredFieldValidator rq4 = RegisterUser.FindControlRecursive("RequiredFieldValidator2") as RequiredFieldValidator;
				if (rq4 != null) rq4.Enabled = false;
				RequiredFieldValidator rq5 = RegisterUser.FindControlRecursive("PasswordCompare") as RequiredFieldValidator;
				if (rq5 != null) rq4.Enabled = false;
				RegularExpressionValidator rq6 = RegisterUser.FindControlRecursive("UserNameRegEx") as RegularExpressionValidator;
				if (rq6 != null) rq4.Enabled = false;
				RequiredFieldValidator rq7 = RegisterUser.FindControlRecursive("UserNameRequired") as RequiredFieldValidator;
				if (rq7 != null) rq4.Enabled = false;

				//We only need to load the information up into the Text Boxes (from an OAuth provider) on an *INITIAL* page load.
				//  Values do not need to be reloaded into the textboxes on a PostBack because they're saved in the ViewState for\
				//  for the MS CreateUserWizard control.
				//if (!IsPostBack)
				//{
				//Make sure everything *required* is populated.
				if (userInfo != null && userInfo.ProviderId != null && !string.IsNullOrWhiteSpace(userInfo.UserProviderId))
				{
					//Have a Guid. Let's see if there's a provider!
					var provider = new OAuthManager().Providers_RetrieveById(userInfo.ProviderId);
					if (provider != null)
					{
						//We have a provider! Let's decode other information.
						bool propSet = false; //Record if all props needed are set. Hides the all the needed fields.

						foreach (var obj in userInfo.ProviderData)
						{
							if (obj.Value is string && !string.IsNullOrWhiteSpace((string)obj.Value))
							{
								//We ONLY want to populate and hide controls if we have an actual value, and not
								//   when (like in GitHub with the email address) we get an empty claim.
								switch ((Provider.ClaimTypeEnum)obj.Key)
								{
									case Provider.ClaimTypeEnum.NameIdentifier:
										{
											//This is the most important bit. Set our flag, letting the page know that
											//  we have enough information to link a user up. So, set the flag here to
											//  hide the important fields.
											propSet = true;
										}
										break;

									case Provider.ClaimTypeEnum.GivenName:
										{
											UnityTextBoxEx txt = RegisterUser.FindControlRecursive("FirstName") as UnityTextBoxEx;
											if (txt != null)
											{
												if (!IsPostBack)
													txt.Text = obj.Value.ToSafeString();
												txt.ReadOnly = true;
											}
										}
										break;

									case Provider.ClaimTypeEnum.SurName:
										{
											UnityTextBoxEx txt = RegisterUser.FindControlRecursive("LastName") as UnityTextBoxEx;
											if (txt != null)
											{
												if (!IsPostBack)
													txt.Text = obj.Value.ToSafeString();
												txt.ReadOnly = true;
											}
										}
										break;

									case Provider.ClaimTypeEnum.Email:
										{
											UnityTextBoxEx txt = RegisterUser.FindControlRecursive("Email") as UnityTextBoxEx;
											if (txt != null)
											{
												if (!IsPostBack)
													txt.Text = obj.Value.ToSafeString();
												txt.ReadOnly = true;
											}
											//For email, populate the userid control, too.
											txt = RegisterUser.FindControlRecursive("UserName") as UnityTextBoxEx;
											if (txt != null)
											{
												if (!IsPostBack)
													txt.Text = obj.Value.ToSafeString();
												txt.ReadOnly = true;
											}
										}
										break;
								}
							}
						}

						//Hide the security panels (fields that are NOT REQUIRED to be filled out when the account is an OAuth account)
						//  when we have at least one field entered in. We don't want the user entering any of this into in, since it's
						//  not used. ONLY if we have at least one field set by OAuth. If there was no informtion fed back to us by then
						//  OAuth provider, then we will NOT be creating an account for them linked to OAuth, and they must enter in all
						//  information.
						if (propSet)
						{
							Control pnl = RegisterUser.FindControlRecursive("userid") as Control;
							if (pnl != null) pnl.Visible = false;
							pnl = RegisterUser.FindControlRecursive("pass1") as Control;
							if (pnl != null) pnl.Visible = false;
							pnl = RegisterUser.FindControlRecursive("pass2");
							if (pnl != null) pnl.Visible = false;
							pnl = RegisterUser.FindControlRecursive("pass3");
							if (pnl != null) pnl.Visible = false;
							pnl = RegisterUser.FindControlRecursive("pass4");
							if (pnl != null) pnl.Visible = false;
							pnl = RegisterUser.FindControlRecursive("passwordHelp");
							if (pnl != null) pnl.Visible = false;
						}
					}
				}
				//}
				//else
				//{
				//	//If it is a postback (the user clicked 'Create!'), we need to hide these. The status of them being hidden (set above in the other IF branch)
				//	//  is not remembered on a postback call. So because right after this, the event on the MS control 'CreateUserWizard' is called to actually 
				//	//  create the user, we need to hide the fields again. If we do NOT hide them, then the CreateUserWizard thinks they are required (which they
				//	//  are not) and will throw an error, not letting the user create their acocunt.
				//	Control pnl = RegisterUser.FindControlRecursive("userid") as Control;
				//	if (pnl != null) pnl.Visible = false;
				//	pnl = RegisterUser.FindControlRecursive("pass1") as Control;
				//	if (pnl != null) pnl.Visible = false;
				//	pnl = RegisterUser.FindControlRecursive("pass2");
				//	if (pnl != null) pnl.Visible = false;
				//	pnl = RegisterUser.FindControlRecursive("pass3");
				//	if (pnl != null) pnl.Visible = false;
				//	pnl = RegisterUser.FindControlRecursive("pass4");
				//	if (pnl != null) pnl.Visible = false;
				//	pnl = RegisterUser.FindControlRecursive("passwordHelp");
				//	if (pnl != null) pnl.Visible = false;
				//}
			}
		}
	}
}
