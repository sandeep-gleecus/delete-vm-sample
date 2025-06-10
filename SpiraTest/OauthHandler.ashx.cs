using Inflectra.OAuth2;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Inflectra.SpiraTest.Web
{
	/// <summary></summary>
	public class OauthHandler : HttpTaskAsyncHandler, IRequiresSessionState
	{
		private const string CLASS = "Web.OauthHandler:";

		private HttpContext ctx = null;
		private string userName = null;

		public override async Task ProcessRequestAsync(HttpContext context)
		{
			const string METHOD = CLASS + "ProcessRequestAsync()";
			Logger.LogEnteringEvent(METHOD);

			//Save the Context.
			ctx = context;

			//See if we need to handle an OAuth Callback.
			var succLogin = await OAuthCallbackAsync();

			//If we returned with a success and have a username defined, send them off!
			Logger.LogExitingEvent(METHOD);
			if (succLogin.Success)
			{
				if (!string.IsNullOrWhiteSpace(succLogin.ReturnUrl))
				{
					ctx.Response.Redirect(succLogin.ReturnUrl, true);
				}
				else
				{
					//See source here: https://referencesource.microsoft.com/system.web/Security/FormsAuthentication.cs.html, under 'GetRedirectUrl' 
					//  for why we have to send 'dummy'. A 'string.Empty' may work, but this is just safer all-around. The resulting MS code 
					//  does nothing WITH the username anyways, as evidenced by the code linked. We just need to make sure that the username is
					//  not null.
					ctx.Response.Redirect(FormsAuthentication.GetRedirectUrl("dummy", false), true);
				}
			}
			else
				//Redirect to the login page after it's all said and done.
				ctx.Response.Redirect("Login.aspx", true);
		}

		/// <summary>Called each time the page is loaded. This is needed to handle any cookies or GET parameters thzat may be passed from the OAuth2 Provider.</summary>
		private async Task<StatusClass> OAuthCallbackAsync()
		{
			const string METHOD = CLASS + "OAuthCallbackAsync()";
			StatusClass retValue = new StatusClass();

			Logger.LogEnteringEvent(METHOD);

			try
			{
				//Let the manager handle the return data, and pull the user if there is one.
				OAuthManager oMgr = new OAuthManager();
				var userInfo = await oMgr.Process_CallbackAsync();

				//This is for safety and security reasons. Double-check that all unused cookies are removed.
				if (ctx.Session[OAuthManager.AUTH_COOKNAME] != null)
				{
					ctx.Session.Remove(OAuthManager.AUTH_COOKNAME);
				}
				if (ctx.Request.Cookies.AllKeys.Contains(OAuthManager.AUTHTOCTRL_COOKNAME))
				{
					ctx.Response.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Expires = DateTime.UtcNow.AddYears(-1);
					ctx.Response.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Value = ".";
				}

				//We should ALWAYS get a UserInfo object. If we do NOT, then it means something bad happened.
				if (userInfo != null)
				{
					//See if the user has an account.
					#region User Does Not Exist
					if (userInfo.UserAcct == null)
					{
						OAuthManager manager = new OAuthManager();
						var provider = manager.Providers_RetrieveById(userInfo.ProviderId);

						//If there was an error, and we couldn't get to finding a user.
						if (userInfo.ProviderData.ContainsKey(Provider.ERROR_CODE))
						{
							//Log the error to the Event Log.
							Logger.LogErrorEvent(CLASS + "OauthCallback", "Error with Login Provider:" + Environment.NewLine + userInfo.ProviderData[Provider.ERROR_CODE]);

							//Add our LoginPage message cookie.
							ctx.Response.Cookies.Add(new HttpCookie(OAuthManager.AUTHTOLOGIN_COOKNAME)
							{
								HttpOnly = true,
								Value = userInfo.ToJson(),
								Expires = DateTime.Now.AddDays(1)
							});
						}
						//If there's no error, no user, we need to tell them to sign up for an account, or log in with an existing account.
						else
						{
							//We need to have a provider, of course.
							if (userInfo.ProviderId != null && provider != null)
							{
								//Save the info into the Sesion.
								ctx.Response.Cookies.Add(new HttpCookie(OAuthManager.AUTHTOLOGIN_COOKNAME)
								{
									HttpOnly = true,
									Expires = DateTime.Now.AddDays(1),
									Value = userInfo.ToJson()
								});
							}
						}
					}
					#endregion User Does Not Exist

					#region User Exists!
					else
					{
						//If the user is no long active, display an error!
						if (!userInfo.UserAcct.IsActive)
						{
							//Add the error entry, and remove the userinfo.
							userInfo.ProviderData.Add(Provider.ERROR_CODE, "inactive");
							userInfo.UserAcct = null;

							//Add our LoginPage message cookie.
							ctx.Response.Cookies.Add(new HttpCookie(OAuthManager.AUTHTOLOGIN_COOKNAME)
							{
								Expires = DateTime.Now.AddDays(1),
								HttpOnly = true,
								Value = userInfo.ToJson()
							});
						}
						//If the user exists, but is not active.
						else if (!userInfo.UserAcct.IsApproved)
						{
							//Add the error entry, and remove the userinfo.
							userInfo.ProviderData.Add(Provider.ERROR_CODE, "notapproved");
							userInfo.UserAcct = null;

							//Add our LoginPage message cookie.
							ctx.Response.Cookies.Add(new HttpCookie(OAuthManager.AUTHTOLOGIN_COOKNAME)
							{
								Expires = DateTime.Now.AddDays(1),
								HttpOnly = true,
								Value = userInfo.ToJson()
							});
						}
						//Otherwise, the user can log in! We will!
						else
						{
							//Now need to make session, sure that we have enough concurrent user licenses
							Global.RegisterSession(HttpContext.Current.Session.SessionID, userInfo.UserAcct.UserId);
							//Make sure we've not exceeded our count of allowed licenses
							if (License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
							{
								int concurrentUserCount = Global.ConcurrentUsersCount();
								if (concurrentUserCount > License.Number)
								{
									//Log an error and throw an exception
									throw new LicenseViolationException(Resources.Messages.Services_LicenseViolationException + " (" + concurrentUserCount + ")");
								}
							}

							//Update the user's information.
							//if (userInfo.ProviderData.ContainsKey((int)Provider.ClaimTypeEnum.Picture))
							//	oMgr.User_SetAvatarAsync(userInfo.UserAcct.UserId, userInfo.ProviderData[(int)Provider.ClaimTypeEnum.Picture]);
							// Other info not set now?

							//Okay, so update user's login information..
							new UserManager().UpdatePasswordInfo(userInfo.UserAcct.UserName, true, true, 0, 0, DateTime.UtcNow, DateTime.UtcNow);

							//Create our cookie, and redirect user.
							SpiraMembershipProvider.SetAuthCookie(userInfo.UserAcct.UserName, false, ConfigurationSettings.Default.Authentication_Expiry);
							userName = userInfo.UserAcct.UserName;
							retValue.Success = true;
							retValue.ReturnUrl = userInfo.ReturnUrl;
						}
					}
					#endregion User Exists!
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD, ex);
			}

			//Go back!
			Logger.LogExitingEvent(METHOD);
			return retValue;
		}

		/// <summary>Small class to store data returned from our processing function.</summary>
		private class StatusClass
		{
			/// <summary>True if the user is logged in properly.</summary>
			public bool Success;

			/// <summary>If the user IS logged in properly, where to redirect them. (Null = Default)</summary>
			public string ReturnUrl;
		}
	}
}
