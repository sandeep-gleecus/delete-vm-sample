using Inflectra.OAuth2;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Base32;
using OtpSharp;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>This webform code-behind class is responsible to allowing the user to login and
	/// authenticate using a secure Web Form that creates a session cookie once authenticated.</summary>
	public partial class Login : PageBase
	{
		private const string CLASS = "Web.Login::";
		private string OAuthLoginAttempt = "false";
		private int DAYS_EXPIRATION_IS_SOON = 7;

		#region MFA State Properties

		/// <summary>
		/// The entered username from step 1 (only used when you have MFA enabled)
		/// </summary>
		private string UserName
		{
			get
			{
				if (ViewState["UserName"] == null)
				{
					return "";
				}
				else
				{
					return (string)ViewState["UserName"];
				}
			}
			set
			{
				ViewState["UserName"] = value;
			}
		}

		/// <summary>
		/// The entered password from step 1 (only used when you have MFA enabled)
		/// </summary>
		private string Password
		{
			get
			{
				if (ViewState["Password"] == null)
				{
					return "";
				}
				else
				{
					return (string)ViewState["Password"];
				}
			}
			set
			{
				ViewState["Password"] = value;
			}
		}

		/// <summary>
		/// The 'remember me' setting from step 1 (only used when you have MFA enabled)
		/// </summary>
		private bool RememberMe
		{
			get
			{
				if (ViewState["RememberMe"] == null)
				{
					return false;
				}
				else
				{
					return (bool)ViewState["RememberMe"];
				}
			}
			set
			{
				ViewState["RememberMe"] = value;
			}
		}

		/// <summary>
		/// The number of incorrect OTP passwords, used for MFA
		/// </summary>
		private int FailedPasswordAttemptCount
		{
			get
			{
				if (ViewState["FailedPasswordAttemptCount"] == null)
				{
					return 0;
				}
				else
				{
					return (int)ViewState["FailedPasswordAttemptCount"];
				}
			}
			set
			{
				ViewState["FailedPasswordAttemptCount"] = value;
			}
		}

		/// <summary>
		/// The last time an incorrect OTP password was entered, used for MFA
		/// </summary>
		private DateTime? FailedPasswordAttemptWindowStart
		{
			get
			{
				return (DateTime?)ViewState["FailedPasswordAttemptWindowStart"];
			}
			set
			{
				ViewState["FailedPasswordAttemptWindowStart"] = value;
			}
		}		

		#endregion

		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			//Display the page title
			((MasterPages.Login)Master).PageTitle = "";

			//Check if the license will expire shortly - if so we show a warning message
			if (Common.License.Expiration.HasValue)
			{
				TimeSpan timeToExpiration = (TimeSpan)(Common.License.Expiration - DateTime.UtcNow);
				int daysToExpiration = timeToExpiration.Days + 1;
				bool expirationIsSoon = daysToExpiration <= DAYS_EXPIRATION_IS_SOON && daysToExpiration > 0;
				bool hasExpired = daysToExpiration <= 0;
				if (expirationIsSoon)
				{
					MessageBox loginMessageBox = LoginUser.FindControl("FailureText") as MessageBox;
					if (loginMessageBox != null)
					{
						if (License.LicenseType == LicenseTypeEnum.Demonstration)
						{
							loginMessageBox.Text = String.Format(Messages.Login_TrialExpire_Soon, daysToExpiration);
						}
						else
						{
							loginMessageBox.Text = String.Format(Messages.Login_LicenseExpire_Soon, daysToExpiration);
						}
					}
				}
				else if (hasExpired)
				{
					MessageBox loginMessageBox = LoginUser.FindControl("FailureText") as MessageBox;
					if (loginMessageBox != null)
					{
						if (License.LicenseType == LicenseTypeEnum.Demonstration)
						{
							loginMessageBox.Text = Messages.LicenseDetails_EvalLicenseExpired;
						}
						else
						{
							loginMessageBox.Text = Messages.LicenseDetails_LicenseExpired;
						}
					}
				}
			}

			//Display any admin messages
			PlaceHolder plcAdminMessage = (PlaceHolder)LoginUser.FindControl("plcAdminMessage");
			Literal ltrAdminMessage = (Literal)LoginUser.FindControl("ltrAdminMessage");
			if (plcAdminMessage != null && ltrAdminMessage != null)
			{
				if (!string.IsNullOrWhiteSpace(ConfigurationSettings.Default.General_AdminMessage))
				{
					plcAdminMessage.Visible = true;
					ltrAdminMessage.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ConfigurationSettings.Default.General_AdminMessage);
				}
			}

			//Display any notices
			PlaceHolder plcNotice = (PlaceHolder)LoginUser.FindControl("plcNotice");
			Literal ltrNotice = (Literal)LoginUser.FindControl("ltrNotice");
			if (plcNotice != null && ltrNotice != null)
			{
				if (!string.IsNullOrWhiteSpace(ConfigurationSettings.Default.General_LoginNotice))
				{
					plcNotice.Visible = true;
					ltrNotice.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ConfigurationSettings.Default.General_LoginNotice);
				}
				else
				{
					plcNotice.Visible = false;
				}
			}

			//See if the user is allowed to self-register, and also make sure that the administrator has logged-in once before
			if (LoginUser.FindControl("plcNeedAccount") != null)
				LoginUser.FindControl("plcNeedAccount").Visible = (ConfigurationSettings.Default.Membership_AllowUserRegistration && ConfigurationSettings.Default.Membership_SomeoneHasLoggedInOnce);

			//Set the UserID control focus.
			TextBoxEx userName = (TextBoxEx)LoginUser.FindControl("UserName");
			if (userName != null) userName.Focus();

			//Handle OAuth code.
			Page_Load_HandleOAuth();

			//Register any event handlers
			LoginUser.PasswordExpired += LoginUser_PasswordExpired;
			LoginUser.LoggingIn += LoginUser_LoggingIn;

			//The 2nd step handler for MFA based logins
			//this.btnLogInAfterOTP.Click += BtnLogInAfterOTP_Click;
		}

		/// <summary>
		/// See if this user needs to provide a MFA one-time password
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoginUser_LoggingIn(object sender, LoginCancelEventArgs e)
		{
			//See if we need to provide MFA
			if (ConfigurationSettings.Default.Security_EnableMfa)
			{
				TextBoxEx txtUserName = (TextBoxEx)this.LoginUser.FindControl("UserName");
				TextBoxEx txtPassword = (TextBoxEx)this.LoginUser.FindControl("Password");
				CheckBox chkRememberMe = (CheckBox)this.LoginUser.FindControl("RememberMe");
				if (txtUserName != null && txtPassword != null && chkRememberMe != null)
				{
					string userName = txtUserName.Text.Trim();
					string password = txtPassword.Text.Trim();

					//Get the user and see if it has a stored MFA token or not
					SpiraMembershipProvider provider = (SpiraMembershipProvider)(Membership.Provider);
					DataModel.User user = provider.GetProviderUser(userName);
					if (user != null && !String.IsNullOrEmpty(user.MfaToken))
					{
						//Store the # invalid attempts to prevent MFA brute-force attacks
						FailedPasswordAttemptCount = user.FailedPasswordAttemptCount;
						FailedPasswordAttemptWindowStart = user.FailedPasswordAttemptWindowStart;

						//Check that the user's login and password are OK
						if (Membership.ValidateUser(userName, password))
						{
							//If we're trying OAUTH linkup, need to tell them to remove MFA first
							if (Page.Request.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME] != null)
							{
								//Display the message
								((MessageBox)this.LoginUser.FindControl("FailureText")).Text = Resources.Messages.Login_CannotEnableSsoWithMfaToken;

								// Abort the login
								e.Cancel = true;
							}
							// BEGIN PCS
							//else
							//{

							//if (!OneTimePasswordSection.Visible)
							//{
							//	//Hide the login/password and show the OTP
							//	this.OneTimePasswordSection.Visible = true;
							//	this.LoginUser.Visible = false;

							//	//Clear out any existing values for the OTP (from previous attempts)
							//	this.OneTimePassword.Text = "";

							//	//Store the login and password for step 2
							//	this.UserName = userName;
							//	this.Password = password;
							//	this.RememberMe = chkRememberMe.Checked;

							//	// Abort the login until they enter the OTP
							//	e.Cancel = true;
							//}


							//}
							// BEGIN PCS
						}
					}
				}
			}
		}

		/// <summary>
		/// Sets the auth cookie and redirects upon successful entry of MFA one time password
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>
		/// We need to check for password expiry here since the normal check is bypassed when using MFA
		/// </remarks>
		private void BtnLogInAfterOTP_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "BtnLogInAfterOTP_Click";

			//Check that the user's login and password are still OK
			if (Membership.ValidateUser(UserName, Password))
			{
				//Get the user's entered OTP

				//string oneTimePassword = this.OneTimePassword.Text.Trim(); //Begin PCS 

				string oneTimePassword = String.Empty;

				//Get the user and see if it has a stored MFA token or not
				SpiraMembershipProvider provider = (SpiraMembershipProvider)(Membership.Provider);
				DataModel.User user = provider.GetProviderUser(UserName);
				if (user != null && !String.IsNullOrEmpty(user.MfaToken))
				{
					//Verify the MFA token

					//Now we need to compare that with the OTP we'd expect from the system
					byte[] secretKey = Base32Encoder.Decode(user.MfaToken);

					long timeStepMatched = 0;
					var otp = new Totp(secretKey);
					if (otp.VerifyTotp(oneTimePassword, out timeStepMatched, new VerificationWindow(2, 2)))
					{
						//Get the appropriate expiration length based on the type of login
						int expirationMinutes = 30;
						if (RememberMe)
						{
							expirationMinutes = ConfigurationSettings.Default.Authentication_ExpiryRememberMe;
						}
						else
						{
							expirationMinutes = ConfigurationSettings.Default.Authentication_Expiry;
						}

						//Now as long as the expiration is greater than zero and we have a username we need to resend the cookie
						//with the configured expiration, also store a custom GUID to prevent cookie
						if (expirationMinutes > 0 && !String.IsNullOrWhiteSpace(UserName))
						{
							//Check to see if the password has expired
							if (CheckPasswordExpired(user))
							{
								//Password has expired
								string username = user.UserName;

								//Remove the authentication cookie
								FormsAuthentication.SignOut();

								//Clear out any session variables
								Session.Abandon();

								//Redirect to Password Expired Page
								Response.Redirect("~/PasswordExpired.aspx?" + GlobalFunctions.PARAMETER_USERNAME + "=" + HttpUtility.UrlEncode(username), true);

							}
							else
							{
								//Clear the # failed login attemps
								new UserManager().UpdatePasswordInfo(
									this.UserName,
									true,
									true,
									ConfigurationSettings.Default.Membership_MaxInvalidPasswordAttempts,
									ConfigurationSettings.Default.Membership_PasswordAttemptWindow,
									DateTime.UtcNow,
									DateTime.UtcNow);

								//Password is fine
								SpiraMembershipProvider.SetAuthCookie(this.UserName, this.RememberMe, expirationMinutes);

								//Log a success audit
								Logger.LogSuccessAuditEvent(CLASS + METHOD_NAME, string.Format(Resources.Messages.Login_MfaCheckSuccess, this.UserName));

								//Redirect to the appropriate URL
								string returnUrl = Request.QueryString["ReturnUrl"];
								if (String.IsNullOrWhiteSpace(returnUrl))
								{
									Response.Redirect("~");
								}
								else
								{
									Response.Redirect(returnUrl);
								}
							}
						}
					}
					else
					{
						//Log a failure audit
						Logger.LogFailureAuditEvent(CLASS + METHOD_NAME, string.Format(Resources.Messages.Login_MfaCheckFailure, this.UserName));

						//Update the # failed login attempts, we have to use a separate method
						//because the standard membership provide check will have reset it on successful login/password
						new UserManager().LockAccountIfMfaRetriesExceeded(user.UserName, FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart);

						//Display the message
						((MessageBox)this.LoginUser.FindControl("FailureText")).Text = this.LoginUser.FailureText;
						//this.OneTimePasswordSection.Visible = false; // BEGIN PCS
						this.LoginUser.Visible = true;
					}
				}
				else
				{
					//They have no MFA token when they did a minute earlier, so fail with message
					((MessageBox)this.LoginUser.FindControl("FailureText")).Text = this.LoginUser.FailureText;
					//this.OneTimePasswordSection.Visible = false;  // BEGIN PCS
					this.LoginUser.Visible = true;

				}
			}
			else
			{
				//They have no MFA token when they did a minute earlier, so fail with message
				((MessageBox)this.LoginUser.FindControl("FailureText")).Text = this.LoginUser.FailureText;
				//this.OneTimePasswordSection.Visible = false;  // BEGIN PCS
				this.LoginUser.Visible = true;
			}
		}

		/// <summary>
		/// Handles the case when a user clicks on an OAuth Provider login button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnLogin2_Click(object sender, EventArgs e)
		{
			//We need to pull the guid from the button, and tell the provider that we're asking them to log on.
			Button but = sender as Button;
			if (but != null)
			{
				try
				{
					Guid id;
					if (Guid.TryParse(but.CommandArgument, out id))
					{
						// See if there is a return URL in the querystring.
						new OAuthManager().Process_RedirectToProviderPage(id, Request.QueryString["ReturnUrl"]);
					}
				}
				catch
				{
					MessageBox lblMsg = LoginUser.FindControl("FailureText") as MessageBox;
					if (lblMsg != null) lblMsg.Text = Messages.Login_OAuthProvider_Error;
				}
			}
		}

		/// <summary>Redirect to the password expiry page</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoginUser_PasswordExpired(object sender, EventArgs e)
		{
            //Get the current username
            string username = User.Identity.Name;

            //Remove the authentication cookie
            FormsAuthentication.SignOut();

            //Clear out any session variables
            Session.Abandon();

            //Redirect to Password Expired Page
            Response.Redirect("~/PasswordExpired.aspx?" + GlobalFunctions.PARAMETER_USERNAME + "=" + HttpUtility.UrlEncode(username), true);
		}

		/// <summary>
		/// Handles OAuth code on the Page Load.
		/// </summary>
		private void Page_Load_HandleOAuth()
		{
			const string METHOD = CLASS + "Page_Load_HandleOAuth()";
			Logger.LogEnteringEvent(METHOD);

			/* First, remove this cookie. It should NEVER be in the browser's storage if we get to this page.
			* This is for safety and security reasons. Double-check that the cookie is removed. No handling
			* data in this cookie is handled on this page. Should all be handled by the OAuthHandler.ashx. */
			if (Session[OAuthManager.AUTH_COOKNAME] != null)
			{
				Session.Remove(OAuthManager.AUTH_COOKNAME);
			}
			if (Session[OAuthManager.AUTHTOCTRL_SESSNAME] != null)
			{
				Session.Remove(OAuthManager.AUTHTOCTRL_SESSNAME);
			}

			/* Now, see if we got here from the OAuth handler. If we did (and we need to do anything 
			 * about it, we will have a cookie with the OAuthHandler.AUTHTOLOGIN_COOKNAME name set. */
			if (Request.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME] != null)
			{
				#region Handle OAuth Return
				if (!string.IsNullOrWhiteSpace(Request.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Value) &&
					!Request.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Value.Equals("."))
				{
					string value = Request.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Value.Trim();

					//First, we immediately remove the cookie.
					Response.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Value = ".";
					Response.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Expires = DateTime.UtcNow.AddYears(-1);

					//See if it's one of our AuthorizationContext objects.
					OAuthManager.UserLoginInfo authCtx = OAuthManager.UserLoginInfo.Parse(value);
					if (authCtx != null && !authCtx.ProviderData.Any(f => f.Key == Provider.ERROR_CODE))
					{
						//Show relevant message
						Literal ltrOAuthNoAccount = LoginUser.FindControlRecursive("ltrOAuthNoAccount") as Literal;
						ltrOAuthNoAccount.Text = string.Format(
							Main.Login_OAuthProvider_NoAccount,
							authCtx.ProviderName
						);

						//Hide the main login title and OAuth options, reset password link, register account link
						LoginUser.FindControlRecursive("wrapperOAuthOptions").Visible = false;
						LoginUser.FindControlRecursive("lnkForgotUserPassword").Visible = false;
						LoginUser.FindControlRecursive("plcNeedAccount").Visible = false;
						LoginUser.FindControlRecursive("ltrLogin").Visible = false;

						//Hide using CSS the main login form - we may unhide it in JS, hence why we use CSS here
						HtmlGenericControl pnlLoginForm = LoginUser.FindControlRecursive("pnlLoginForm") as HtmlGenericControl;
						if (pnlLoginForm != null) pnlLoginForm.Attributes.Add("class", "dn");

						//Show the title message and choices to the user
						LoginUser.FindControlRecursive("ltrOAuthNoAccount").Visible = true;
						LoginUser.FindControlRecursive("headingConnectAccount").Visible = true;
						LoginUser.FindControlRecursive("headingOAuthCreateAccount").Visible = (ConfigurationSettings.Default.Membership_AllowUserRegistration && ConfigurationSettings.Default.Membership_SomeoneHasLoggedInOnce);
						LoginUser.FindControlRecursive("headingOAuthRefresh").Visible = true;

						//Set the bool to true so that JS on the page can be run as needed.
						OAuthLoginAttempt = "true";

						//Create our cookie to be used in the LoginEx or NeedAccount.aspx pages.
						Response.Cookies.Add(new HttpCookie(OAuthManager.AUTHTOCTRL_COOKNAME)
						{
							Expires = DateTime.UtcNow.AddDays(1),
							Value = authCtx.ToJson(),
							HttpOnly = true
						});
					}
					else
					{
						string msgToDisplay = "";
						//This seems like a hack. But for now, since we already logged the error, we don't need
						// to give the user any other information. In the FUTURE, it would be nice to be able
						// to write out the extra data to the browser's console.
						//if (authCtx.ProviderData.Any(f => f.Key == Provider.ERROR_CODE)) value = "proverror";
						string errVal = (string)authCtx.ProviderData[Provider.ERROR_CODE];

						//We have an error. See which error it is.
						switch (errVal)
						{
							case "inactive":
								//The user is linked up, but the account was made inactive.
								msgToDisplay = Messages.Login_UnableToLogIn;
								break;

							case "notapproved":
								//The user is linked up, but the account has not yet been approved.
								msgToDisplay = Messages.Login_NotYetApproved;
								break;

							case "proverror":
							default:
								//A general error occured with the provider.
								msgToDisplay = Messages.Login_OAuthProvider_Error;
								break;
						}

						// And display it.
						if (!string.IsNullOrWhiteSpace(msgToDisplay))
						{
							MessageBox lblMsg = LoginUser.FindControl("FailureText") as MessageBox;
							if (lblMsg != null) lblMsg.Text = string.Format(msgToDisplay, authCtx.ProviderName);
						}
					}
				}

				//Remove the cookie!
				Response.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Expires = DateTime.UtcNow.AddYears(-1);
				Response.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Value = ".";
				#endregion Handle OAuth Return
			}
			else
			{
				#region Display Login Buttons

				/* There was no cookie given to us from the OAuthHandler.ashx page. Which means that this is
				 * the first time they are hitting the page, OR, they cancelled out of a previous OAuth
				 * action. In this case, all we do is display the buttons to allow them to select an OAuth
				 * provider, should they so choose. */

				//Remove everything.
				if (Request.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME] != null)
				{
					Response.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Value = ".";
					Response.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Expires = DateTime.UtcNow.AddYears(-1);
				}
				if (Request.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME] != null)
				{
					Response.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Value = ".";
					Response.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Expires = DateTime.UtcNow.AddYears(-1);
				}
				if (Session.Keys.OfType<string>().Any(n => n.Equals(OAuthManager.AUTHTOCTRL_SESSNAME))) Session.Remove(OAuthManager.AUTHTOCTRL_SESSNAME);

				// Retrieve OAuth providers and display their buttons.
				List<GlobalOAuthProvider> providers = new OAuthManager().Providers_RetrieveAll();
				if (providers.Count > 0 && providers.Any(p => p.IsActive && p.IsLoaded))
				{
					PlaceHolder wrapperOAuthOptions = LoginUser.FindControlRecursive("wrapperOAuthOptions") as PlaceHolder;
					wrapperOAuthOptions.Visible = true;

					//Load up any buttons for OAuth providers!
					foreach (var provider in providers)
					{
						if (provider.IsActive && provider.IsLoaded)
						{
							//Generate a wrapper
							HtmlGenericControl wrapperDiv = new HtmlGenericControl("div");
							wrapperDiv.ID = "wrapper_" + provider.Name;
							wrapperDiv.Attributes.Add("class", "pa3 w-50");

							//Generate a button.
							Button button = new Button();
							button.Click += btnLogin2_Click;
							button.Text = provider.Name;
							button.ID = "btn_" + provider.Name;
							button.CssClass = "btn w-100";
							button.CommandArgument = provider.OAuthProviderId.ToString("B");

							wrapperDiv.Controls.Add(button);

							//Add the button to the placeholder.
							PlaceHolder pnl = LoginUser.FindControlRecursive("plcOAuth") as PlaceHolder;
							if (pnl != null) pnl.Controls.Add(wrapperDiv);
						}
					}
				}
				#endregion Display Login Buttons
			}
		}

		public string GetOAuthLogin()
		{
			return OAuthLoginAttempt;
		}

		/* Not used, can be used in future if we want to allow users with MFA enabled to link OAUTH without first disabling MFA
		/// <summary>
		/// Links up the current user to an OAuth account with MFA one-time password required
		/// </summary>
		/// <param name="user">The existing user to link to OAuth</param>
		private void LinkUserToOAuth(DataModel.User user)
		{
			//Check to see if they have an OAuth to link up.
			//See if we have the Oauth item in the session.
			if (Page.Request.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME] != null)
			{
				if (user.UserId != UserManager.UserSystemAdministrator)
				{
					//Get the object.
					OAuthManager.UserLoginInfo userInfo = OAuthManager.UserLoginInfo.Parse(Page.Request.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Value);

					//Make sure everything *required* is populated.
					if (userInfo != null && userInfo.ProviderId != null)
					{
						//Have a Guid. Let's see if there's a provider!
						var oMgr = new OAuthManager();
						var provider = oMgr.Providers_RetrieveById(userInfo.ProviderId);
						if (provider != null)
						{
							//Call the business function.
							oMgr.User_LinkToProvider(user.UserId, userInfo.ProviderId, userInfo.UserProviderId);

							//Update the user's information.
							if (userInfo.ProviderData.ContainsKey((int)Provider.ClaimTypeEnum.Picture))
								oMgr.User_SetAvatarAsync((long)user.UserId, userInfo.ProviderData[(int)Provider.ClaimTypeEnum.Picture]);
						}
					}

				}
				else
				{
					//Show message saying that the account could not be connected up.
					MessageBox lblMsg = FindControl("FailureText") as MessageBox;
					if (lblMsg != null) lblMsg.Text = "XX The 'administrator' account can not be linked to a remote login account!";
					return;
				}

				//Make sure this cookie is gone!
				Page.Response.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Expires = DateTime.UtcNow.AddYears(-1);
				Page.Response.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Value = ".";
			}
		}*/

		/// <summary>
		/// Checks to see if the password has expired or needs to be changed on first login
		/// </summary>
		/// <returns>True if the user needs to change their password</returns>
		protected bool CheckPasswordExpired(DataModel.User user)
		{
			bool needToChangePassword = false;

			//If the password has expired or they have to change on first login, redirect to a different page
			DateTime lastPasswordChangedDate = DateTime.MinValue;
			if (user.LastPasswordChangedDate.HasValue)
			{
				lastPasswordChangedDate = user.LastPasswordChangedDate.Value;
			}
			if (ConfigurationSettings.Default.Security_ChangePasswordOnFirstLogin && lastPasswordChangedDate == user.CreationDate)
			{
				needToChangePassword = true;
			}
			if (ConfigurationSettings.Default.Security_PasswordChangeInterval > 0 && lastPasswordChangedDate.AddDays(ConfigurationSettings.Default.Security_PasswordChangeInterval) < DateTime.UtcNow)
			{
				needToChangePassword = true;
			}
			if (needToChangePassword)
			{
				//Make sure not LDAP-managed, since they cannot change password in Spira and the rules are set by LDAP
				if (!string.IsNullOrWhiteSpace(user.LdapDn))
				{
					needToChangePassword = false;
				}
			}

			return needToChangePassword;
		}
	}
}
