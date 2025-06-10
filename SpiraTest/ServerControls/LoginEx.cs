using Inflectra.OAuth2;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>Extends the ASP.NET Login server control to use the timeout and 'remember me' timeouts from Global Settings
	/// instead of the ASP.NET web.config file. Also auto-unlocks the user after the password attempt window is exceeded</summary>
	[ToolboxData("<{0}:LoginEx runat=server></{0}:LoginEx>")]
	public class LoginEx : System.Web.UI.WebControls.Login
	{
		private const string CLASS_NAME = "Web.ServerControls.LoginEx::";

		private bool rememberMeSet = false;
		private string userName = "";
		private bool needToChangePassword = false;

		/// <summary>Constructor</summary>
		public LoginEx()
			: base()
		{
			const string METHOD_NAME = CLASS_NAME + "LoginEx()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Register the event handlers
				LoggingIn += new LoginCancelEventHandler(LoginEx_LoggingIn);
				LoggedIn += new EventHandler(LoginEx_LoggedIn);

				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#region Password Expired Event

		private static readonly object PasswordExpiredEvent = new object();

		/// <summary>
		/// The event delegate for when a password has expired
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected virtual void OnPasswordExpired(EventArgs e)
		{
			EventHandler handler = (EventHandler)base.Events[PasswordExpiredEvent];
			if (handler != null)
			{
				handler(this, e);
			}
		}

		/// <summary>
		/// The event that's called when a user's password has expired
		/// </summary>
		public event EventHandler PasswordExpired
		{
			add
			{
				base.Events.AddHandler(PasswordExpiredEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(PasswordExpiredEvent, value);
			}
		}

		#endregion

		/// <summary>Called when the user tries to login</summary>
		void LoginEx_LoggingIn(object sender, LoginCancelEventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "LoginEx_LoggingIn()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//We need to capture these properties now because ViewState is cleared when LoggedIn event is fired
			rememberMeSet = RememberMeSet;
			userName = UserName;

			//Check to see if the current user exists
			MembershipUser memUser = Membership.GetUser(UserName);
			if (memUser != null)
			{
				//Check to see if the user is currently locked out
				if (memUser.IsLockedOut)
				{
					//Get the last lockout  date from the user
					DateTime lastLockout = Membership.GetUser(UserName).LastLockoutDate;
					//Calculate the time the user should be unlocked
					DateTime unlockDate = lastLockout.AddMinutes(Membership.PasswordAttemptWindow);

					//Check to see if it is time to unlock the user
					//The membership provider has converted from UTC - server local time, so don't use DateTime.UtcNow
					if (DateTime.Now > unlockDate)
					{
						Membership.GetUser(UserName).UnlockUser();
					}
				}

				//If the return URL is invalid, redirect to root (without the return URL)
				if (!String.IsNullOrEmpty(Page.Request.QueryString["ReturnUrl"]))
				{
					//It must not contain newline characters
					string returnUrl = Page.Request.QueryString["ReturnUrl"];
					if (returnUrl.IndexOf('\n') >= 0 || returnUrl.IndexOf('\r') >= 0)
					{
						Page.Response.Redirect("~");
					}
				}

				//If the password has expired or they have to change on first login, redirect to a different page
				if (ConfigurationSettings.Default.Security_ChangePasswordOnFirstLogin && memUser.LastPasswordChangedDate == memUser.CreationDate)
				{
					needToChangePassword = true;
				}
				if (ConfigurationSettings.Default.Security_PasswordChangeInterval > 0 && memUser.LastPasswordChangedDate.AddDays(ConfigurationSettings.Default.Security_PasswordChangeInterval) < DateTime.UtcNow)
				{
					needToChangePassword = true;
				}
				if (needToChangePassword)
				{
					//Make sure not LDAP-managed, since they cannot change password in Spira and the rules are set by LDAP
					DataModel.User providerUser = ((SpiraMembershipProvider)Membership.Provider).GetProviderUser(UserName);
					if (providerUser != null && !string.IsNullOrWhiteSpace(providerUser.LdapDn))
					{
						needToChangePassword = false;
					}
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Called if the user is successfully authenticated</summary>
		void LoginEx_LoggedIn(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "LoginEx_LoggedIn()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Get the appropriate expiration length based on the type of login
			int expirationMinutes = (rememberMeSet) ?
				ConfigurationSettings.Default.Authentication_ExpiryRememberMe :
				ConfigurationSettings.Default.Authentication_Expiry;

			//Get the user.
			var user = Membership.GetUser(UserName);

			if (user != null)
			{
				int userId = Convert.ToInt32(user.ProviderUserKey);

				//Check to see if they have an OAuth to link up.
				//See if we have the Oauth item in the session.
				if (Page.Request.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME] != null)
				{
					if (userId != UserManager.UserSystemAdministrator)
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
								oMgr.User_LinkToProvider(Convert.ToInt32(user.ProviderUserKey), userInfo.ProviderId, userInfo.UserProviderId);

								//Update the user's information.
								if (userInfo.ProviderData.ContainsKey((int)Provider.ClaimTypeEnum.Picture))
									oMgr.User_SetAvatarAsync(Convert.ToInt64(user.ProviderUserKey), userInfo.ProviderData[(int)Provider.ClaimTypeEnum.Picture]);
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
				else
				{
					//If the password has expired or they have to change on first login, redirect to a different page
					if (needToChangePassword) OnPasswordExpired(new EventArgs());
				}

				//Now as long as the expiration is greater than zero and we have a username we need to resend the cookie
				//with the configured expiration
				if (!string.IsNullOrWhiteSpace(userName))
					SpiraMembershipProvider.SetAuthCookie(userName, rememberMeSet, expirationMinutes);
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>So that we aren't reliant on MS's version of this text. (Used in loginEx and OAuth handlers.</summary>
		public override string FailureText
		{
			get
			{
				return Resources.Messages.Login_UnableToLogIn;
			}
			set
			{ }
		}
	}
}
