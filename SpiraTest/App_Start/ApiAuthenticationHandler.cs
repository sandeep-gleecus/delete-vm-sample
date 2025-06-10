using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Classes;
using System.Text;
using System.Security.Principal;
using System.Web.Security;
using System.Collections.Specialized;

namespace Inflectra.SpiraTest.Web.App_Start
{
	/// <summary>
	/// Handles the authentication of Web Api REST requests
	/// </summary>
	/// <remarks>
	/// Supports passing credentials in the same headers and/or URLs as WCF REST API calls:
	/// 1) Using Basic Auth header
	/// 2) Using a custom Header
	/// 3) Using the URL
	/// </remarks>
	public class ApiAuthenticationHandler : DelegatingHandler
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.App_Start.ApiAuthenticationHandler::";

		public const string API_KEY_NAME = "api-key";
		public const string API_USERNAME = "username";
		public const string HEADER_AUTHORIZATION = "Authorization";
		private const string PLUGIN_WEB_API = "Web API";

		/// <summary>
		/// Checks if the user is authenticated
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>The HTTP response message</returns>
		protected async override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			const string METHOD_NAME = "SendAsync";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			bool authenticated = false;

			try
			{
				//See if they're using the standard BASIC AUTH header
				string basicAuth = request.Headers.GetValues(HEADER_AUTHORIZATION).FirstOrDefault();
				if (!String.IsNullOrWhiteSpace(basicAuth) && basicAuth.StartsWith("Basic "))
				{
					//Decode the header
					string header = basicAuth.Substring("Basic ".Length);
					byte[] decodedHeader = Convert.FromBase64String(header);
					if (decodedHeader != null && decodedHeader.Length > 0)
					{
						string[] usernamePassword = new UTF8Encoding().GetString(decodedHeader).Split(':');
						string username = usernamePassword[0];
						string apiKey3 = "";
						if (usernamePassword.Length > 1)
						{
							apiKey3 = usernamePassword[1];
						}

						if (String.IsNullOrWhiteSpace(username))
						{
							authenticated = false;
						}
						else
						{
							//Validate the user based in username and RSS token combination. Locks the user if retries exceeded
							//which prevents a brute force attack
							SpiraMembershipProvider spiraProvider = (SpiraMembershipProvider)Membership.Provider;
							authenticated = spiraProvider.ValidateUserByRssToken(username, apiKey3);
							if (authenticated)
							{
								User user = spiraProvider.GetProviderUser(username);
								if (user == null)
								{
									throw new ApplicationException("User object null when checking BASIC header");
								}

								//Now need to make sure that we have enough concurrent user licenses
								if (HttpContext.Current.Session != null)
								{
									Web.Global.RegisterSession(HttpContext.Current.Session.SessionID, user.UserId, PLUGIN_WEB_API);
								}

								//Make sure we've not exceeded our count of allowed licenses
								if (Common.License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
								{
									int concurrentUserCount = Web.Global.ConcurrentUsersCount();
									if (concurrentUserCount > Common.License.Number)
									{
										//Log an error and throw an exception
										LicenseViolationException exception = new LicenseViolationException(Resources.Messages.Services_LicenseViolationException + " (" + concurrentUserCount + ")");
										Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
										return await System.Threading.Tasks.Task.Factory.StartNew(() =>
										{
											return new HttpResponseMessage(HttpStatusCode.BadRequest)
											{
												Content = new StringContent(Resources.Messages.Services_LicenseViolationException)
											};
										});
									}
								}

								//Put the user id into the thread, will be checked by the controller if it needs authentication
								GenericIdentity identity = new GenericIdentity(user.UserId.ToString());
								Thread.CurrentPrincipal = new GenericPrincipal(identity, new string[] { });
							}
						}
					}
				}

				//See if they're using the older custom API header
				if (!authenticated)
				{
					string apiKey = request.Headers.GetValues(API_KEY_NAME).FirstOrDefault();
					string username3 = request.Headers.GetValues(API_USERNAME).FirstOrDefault();
					if (!String.IsNullOrWhiteSpace(apiKey) && !String.IsNullOrWhiteSpace(username3))
					{
						//Validate the user based in username and RSS token combination. Locks the user if retries exceeded
						//which prevents a brute force attack
						SpiraMembershipProvider spiraProvider = (SpiraMembershipProvider)Membership.Provider;
						authenticated = spiraProvider.ValidateUserByRssToken(username3, apiKey);
						if (authenticated)
						{
							User user = spiraProvider.GetProviderUser(username3);
							if (user == null)
							{
								throw new ApplicationException("User object null when checking custom API header");
							}

							//Now need to make sure that we have enough concurrent user licenses
							if (HttpContext.Current.Session != null)
							{
								Web.Global.RegisterSession(HttpContext.Current.Session.SessionID, user.UserId, PLUGIN_WEB_API);
							}

							//Make sure we've not exceeded our count of allowed licenses
							if (Common.License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
							{
								int concurrentUserCount = Web.Global.ConcurrentUsersCount();
								if (concurrentUserCount > Common.License.Number)
								{
									//Log an error and throw an exception
									LicenseViolationException exception = new LicenseViolationException(Resources.Messages.Services_LicenseViolationException + " (" + concurrentUserCount + ")");
									Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
									return await System.Threading.Tasks.Task.Factory.StartNew(() =>
									{
										return new HttpResponseMessage(HttpStatusCode.BadRequest)
										{
											Content = new StringContent(Resources.Messages.Services_LicenseViolationException)
										};
									});
								}
							}

							//Put the user id into the thread, will be checked by the controller if it needs authentication
							GenericIdentity identity = new GenericIdentity(user.UserId.ToString());
							Thread.CurrentPrincipal = new GenericPrincipal(identity, new string[] { });
						}
					}
				}
			}
			catch (InvalidOperationException)
			{
				//The headers are not present, so ignore in case it's in the URL
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				return await System.Threading.Tasks.Task.Factory.StartNew(() =>
				{
					return new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
						Content = new StringContent(exception.Message)
					};
				});
			}

			//See if we have a URL token instead
			if (!authenticated)
			{
				try
				{
					Uri uri = request.RequestUri;
					NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
					if (!String.IsNullOrWhiteSpace(query[API_KEY_NAME]))
					{
						string apiKey2 = query[API_KEY_NAME].Trim();
						string username2 = query[API_USERNAME];
						if (String.IsNullOrWhiteSpace(username2))
						{
							authenticated = false;
						}
						else
						{
							username2 = username2.Trim();
							//Validate the user based in username and RSS token combination. Locks the user if retries exceeded
							//which prevents a brute force attack
							SpiraMembershipProvider spiraProvider = (SpiraMembershipProvider)Membership.Provider;
							authenticated = spiraProvider.ValidateUserByRssToken(username2, apiKey2);
							if (authenticated)
							{
								User user = spiraProvider.GetProviderUser(username2);
								if (user == null)
								{
									throw new ApplicationException("User object null when checking URL token");
								}

								//Now need to make sure that we have enough concurrent user licenses
								if (HttpContext.Current.Session != null)
								{
									Web.Global.RegisterSession(HttpContext.Current.Session.SessionID, user.UserId, PLUGIN_WEB_API);
								}

								//Make sure we've not exceeded our count of allowed licenses
								if (Common.License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
								{
									int concurrentUserCount = Web.Global.ConcurrentUsersCount();
									if (concurrentUserCount > Common.License.Number)
									{
										//Log an error and throw an exception
										LicenseViolationException exception = new LicenseViolationException(Resources.Messages.Services_LicenseViolationException + " (" + concurrentUserCount + ")");
										Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
										return await System.Threading.Tasks.Task.Factory.StartNew(() =>
										{
											return new HttpResponseMessage(HttpStatusCode.BadRequest)
											{
												Content = new StringContent(Resources.Messages.Services_LicenseViolationException)
											};
										});
									}
								}

								//Put the user id into the thread, will be checked by the controller if it needs authentication
								GenericIdentity identity = new GenericIdentity(user.UserId.ToString());
								Thread.CurrentPrincipal = new GenericPrincipal(identity, new string[] { });
							}
						}
					}
				}
				catch (UriFormatException)
				{
					authenticated = false;
				}
				catch (ArgumentNullException)
				{
					authenticated = false;
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					return await System.Threading.Tasks.Task.Factory.StartNew(() =>
					{
						return new HttpResponseMessage(HttpStatusCode.BadRequest)
						{
							Content = new StringContent(exception.Message)
						};
					});
				}
			}

			//Return the authentication status
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Authenticated=" + authenticated);
			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			//Return if authenticated or not, since the controller will need to check the Principal
			return await base.SendAsync(request, cancellationToken);
		}
	}
}
