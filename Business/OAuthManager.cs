namespace Inflectra.SpiraTest.Business
{
	using Inflectra.OAuth2;
	using Inflectra.SpiraTest.Common;
	using Inflectra.SpiraTest.DataModel;
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.Data.Objects;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Security;
	using static Inflectra.SpiraTest.Business.HistoryManager;

	/*
	 * The general, overview flow of how OAuth & Spira operate:
	 * 
	 * THe user hits the Login page. Assuming an administrator has everything set up porperly, the user
	 * will see the normal login control, and buttons allowing them to log in via one of the configured
	 * OAuth Providers. When a user clicks an OAuth Provider button, it will post back to Spira. Spira 
	 * will give the user a cookie (named "Inflectra.Spira.OAuth") that will allow us on return to know
	 * which OAuth provider they selected, and then redirect the user to the OAuth Provider's login page.
	 * The OAuth provider will handle all security and login activities (Forgot Password, Bot Checking,
	 * etc.) and once the user is logged in and secure, will redirect the user to the OAuthHandler, with
	 * with a query string.
	 * The OAuthHandler will read the query string, and data is read and converted into a cookie (named 
	 * "Inflectra.Spira.OAuth.Postback") and the user will be redirected to the Login page. It will then
	 * send data to the OAuth provider. The provider will read the query-string and then send a request
	 * (this is the Verification Token) back to the OAuth handler to get the Authentication Token with
	 * claims. Once that is done, Spira will look at the returned claims and get the user's custom
	 * Provider ID claim, and look in the user table to see if anyone matches. If someone matches, then
	 * they are logged in and redirected to their configured home page. THe user will see them go from
	 * Oauth Provider straight to their My Page.
	 * If no user is found in the databse, they will be given an option to Register for a new account
	 * (Request/Need Account) or link to an existing user in Spira.
	 */

	public class OAuthManager
	{
		#region Private Vars
		private const string CLASS = "Business.OAuthManager::";

		/// <summary>The list of available OAuth Providers.</summary>
		private static List<Provider> _providers = new List<Provider>();

		/// <summary>The locked key that provides encryption for our cookie.</summary>
		/// <remarks>Current value: zx81SPECTRUM [122 120 56 49 83 80 69 67 84 82 85 77]</remarks>
		private static readonly byte[] machineKey = new byte[] { 122, 120, 56, 49, 83, 80, 69, 67, 84, 82, 85, 77 };

		/// <summary>String used for the cookie name, so we can recieve data back.</summary>
		public const string AUTH_COOKNAME = "Inflectra.Spira.OAuth";
		/// <summary>String used for the cookie name, so we can recieve data back.</summary>
		public const string AUTHTOLOGIN_COOKNAME = "Inflectra.Spira.OAuth.ToLogin";
		/// <summary>String used for the cookie name, so we can recieve data back.</summary>
		public const string AUTHTOCTRL_COOKNAME = "Inflectra.Spira.OAuth.ToCtrl";
		/// <summary>String used for Session data.</summary>
		public const string AUTHTOCTRL_SESSNAME = AUTHTOCTRL_COOKNAME;

		/// <summary>Delegated function for Unlinking User account - this resets password.</summary>
		/// <param name="username">THe username to change.</param>
		/// <param name="newPassword">The user's new password.</param>
		public delegate bool MembershipProvider_ChangePasswordAsAdmin(string username, string newPassword);

		/// <summary>Delegated function for Unlinking User account - this resets the user's secret question and answer.</summary>
		/// <param name="username">The user's username.</param>
		/// <param name="newPasswordQuestion">THe new Secret Question</param>
		/// <param name="newPasswordAnswer">The new Answer.</param>
		public delegate bool MembershipProvider_ChangePasswordQuestionAndAnswerAsAdmin(string username, string newPasswordQuestion, string newPasswordAnswer);
		#endregion

		#region Static Functions
		/// <summary>Handles the loading of the DLLs on the first static load.</summary>
		static OAuthManager()
		{
			try
			{
				//Load the DLLs on first Static load.
				Providers_LoadDLLs();
			}
			catch (Exception ex)
			{
				try
				{
					Logger.LogErrorEvent(CLASS + "__OAuthManager", ex);
				}
				catch { }
			}
		}

		/// <summary>Loads up available DLLs in the directory, and adds any new ones to the directory, or flags missing ones as .. well, missing.</summary>
		public static void Providers_LoadDLLs()
		{
			const string METHOD = CLASS + "Providers_LoadDLLs()";
			Logger.LogEnteringEvent(METHOD);

			//Get the path that contains our DLLs!
			string filePath = Path.GetDirectoryName(new Uri(Assembly.GetCallingAssembly().CodeBase).LocalPath);
			filePath = Path.GetFullPath(filePath + @"\..\OAuthProviders\");

			//Get a list of DLLs in our directory.
			List<string> libraries = Directory.GetFiles(filePath, "OAuth2_*.dll", SearchOption.TopDirectoryOnly).ToList();

			//Create database instance to add, or update provider records.
			using (var ct = new SpiraTestEntitiesEx())
			{
				//Loop through each one, and pull out all classes the have a Provider available.
				foreach (string file in libraries)
				{
					try
					{
						//Load the types that this DLL materializes.
						List<Type> types = Assembly.LoadFrom(file).GetExportedTypes().ToList();

						//Loop throgh all available classes, and see if any are a Provider.
						foreach (Type type in types.Where(t => t.IsClass))
						{
							if (typeof(Provider).IsAssignableFrom(type))
							{
								//Create an instance of the class, and make sure it doesn't already exist as loaded.
								Provider prv = (Provider)Activator.CreateInstance(type);
								if (!_providers.Any(g => g.ProviderId == prv.ProviderId))
								{
									_providers.Add(prv);

									//Update or add entry to the database.
									GlobalOAuthProvider dbPrv = ct.GlobalOAuthProviders.SingleOrDefault(f => f.OAuthProviderId == prv.ProviderId);

									if (dbPrv == null)
									{
										//Add it to the database.
										ct.GlobalOAuthProviders.AddObject(new GlobalOAuthProvider
										{
											Description = prv.Description,
											IsActive = false,
											Name = prv.Name,
											OAuthProviderId = prv.ProviderId,
											IsLoaded = true,
											AuthorizationUrl = prv.Addresses.Authorization.ToSafeString(),
											TokenUrl = prv.Addresses.Token.ToSafeString(),
											ProfileUrl = prv.Addresses.Profile.ToSafeString(),
											IsUrlsEditable = prv.IsUrlsRequired,
											IsLogoEditable = !prv.IsLogoProvided
										});

										//We do NOT do a .StartInit() on the provider here, since we have NO configuration data for it!
									}
									else
									{
										//Update databae properties first..
										dbPrv.StartTracking();
										dbPrv.IsLoaded = true;
										if (dbPrv.Name != prv.Name) dbPrv.Name = prv.Name;
										if (dbPrv.Description != prv.Description) dbPrv.Description = prv.Description;

										//Update the Provider, if it's configured properly.
										if (dbPrv.IsActive)
										{
											//Now initialize the provier.
											prv.BeginInit();
											prv.ClientId = dbPrv.ClientId;
											prv.ClientSecretKey = dbPrv.ClientSecret;
											if (!string.IsNullOrWhiteSpace(dbPrv.AuthorizationUrl)) //TODO: Set up other needed URLs.
												prv.UrlAuthorization = new Uri(dbPrv.AuthorizationUrl);
											if (!string.IsNullOrWhiteSpace(dbPrv.TokenUrl))
												prv.UrlToken = new Uri(dbPrv.TokenUrl);
											if (!string.IsNullOrWhiteSpace(dbPrv.ProfileUrl))
												prv.UrlProfile = new Uri(dbPrv.ProfileUrl);
										}

										//Catch an error here. If a configuration issue, shoud log it and mark it as inactive.
										try
										{
											if (dbPrv.IsActive) prv.EndInit();
											ct.SaveChanges();
										}
										catch (Exception ex)
										{
											//Throw error.
											Logger.LogErrorEvent(METHOD, ex, "Trying to configure Provider.");
											//Update flag.
											dbPrv.IsActive = false;
										}
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD, ex, "Loading OAuth Provider DLL: " + file);
					}
				}

				//Save changes to the database.
				ct.SaveChanges();
			}

			//Now mark all NOT LOADED libraries as 'Not Loaded'
			using (var ct = new SpiraTestEntitiesEx())
			{
				//Get list of all..
				List<GlobalOAuthProvider> providers = ct.GlobalOAuthProviders.ToList();
				//Loop throgh each one, and see if it is loaded.
				foreach (var prov in providers)
				{
					//If there aren't any in our loaded collection, mark it as not being loaded.
					if (!_providers.Any(g => g.ProviderId == prov.OAuthProviderId))
					{
						prov.StartTracking();
						prov.IsLoaded = false;
					}
				}

				ct.SaveChanges();
			}

			Logger.LogExitingEvent(METHOD);
		}
		#endregion

		#region Database Functions
		/// <summary>Pulls all installed OAuth providers from the database.</summary>
		/// <returns></returns>
		public List<GlobalOAuthProvider> Providers_RetrieveAll(bool includeUsers = false)
		
		{
			const string METHOD = CLASS + "Providers_RetrieveAll()";
			Logger.LogEnteringEvent(METHOD);

			List<GlobalOAuthProvider> retList = new List<GlobalOAuthProvider>();

			using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
			{
				if (includeUsers)
					retList = ct.GlobalOAuthProviders.Include(g => g.Users).ToList();
				else
					retList = ct.GlobalOAuthProviders.ToList();
			}

			Logger.LogExitingEvent(METHOD);
			return retList;
		}

		/// <summary>Pulls the OAuth provider by its GUID.</summary>
		/// <param name="id">The Guid of the provider to pull.</param>
		/// <param name="includeUsers">All users that are using a login to this provider.</param>
		/// <returns></returns>
		public GlobalOAuthProvider Providers_RetrieveById(Guid id, bool includeUsers = false)
		{
			const string METHOD = CLASS + "Providers_RetrieveById()";
			Logger.LogEnteringEvent(METHOD);

			GlobalOAuthProvider retVal = null;

			using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
			{
				ObjectQuery<GlobalOAuthProvider> obj = ct.GlobalOAuthProviders;

				if (includeUsers)
					obj = obj
						.Include("Users")
						.Include("Users.Profile");

				retVal = obj.SingleOrDefault(y => y.OAuthProviderId == id);
			}

			Logger.LogExitingEvent(METHOD);
			return retVal;
		}

		private void InsertAuditTrailDetailEntry(long changeSetId, string fieldName, int userId, string newValue, string oldValue)
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			TST_ADMIN_HISTORY_DETAILS_AUDIT detail = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
			if (oldValue == null)
			{
				detail.ADMIN_CHANGESET_ID = changeSetId;
				detail.ADMIN_ARTIFACT_FIELD_NAME = fieldName;
				detail.ADMIN_ARTIFACT_FIELD_CAPTION = fieldName;
				detail.ADMIN_USER_ID = userId;
				detail.ADMIN_PROPERTY_NAME = fieldName;
				detail.NEW_VALUE = newValue;
				detail.OLD_VALUE = oldValue;

				adminAuditManager.DetailInsert1(detail);
			}
			else if (!oldValue.Equals(newValue))
			{				
				detail.ADMIN_CHANGESET_ID = changeSetId;
				detail.ADMIN_ARTIFACT_FIELD_NAME = fieldName;
				detail.ADMIN_ARTIFACT_FIELD_CAPTION = fieldName;
				detail.ADMIN_USER_ID = userId;
				detail.ADMIN_PROPERTY_NAME = fieldName;
				detail.NEW_VALUE = newValue;
				detail.OLD_VALUE = oldValue;

				adminAuditManager.DetailInsert1(detail);
			}
		}

		/// <summary>Update the database object with the object provided.</summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		public bool Provider_Update(GlobalOAuthProvider provider, int? userId = null)
		{
			const string METHOD = CLASS + "Provider_Update()";
			Logger.LogEnteringEvent(METHOD);

			bool retVal = false;

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Manage Login Providers";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			using (var ct = new SpiraTestEntitiesEx())
			{
				var dbProv = ct.GlobalOAuthProviders.SingleOrDefault(g => g.OAuthProviderId == provider.OAuthProviderId);
				if (dbProv != null)
				{
					//Get the provider.
					var providerDll = GetProvider(provider.OAuthProviderId);

					//Create a new changeset.
					TST_ADMIN_HISTORY_CHANGESET_AUDIT hsChangeSet = new TST_ADMIN_HISTORY_CHANGESET_AUDIT();
					hsChangeSet.ADMIN_USER_ID = (int)userId;
					hsChangeSet.ADMIN_SECTION_ID = adminSectionId;
					hsChangeSet.CHANGE_DATE = DateTime.UtcNow;
					hsChangeSet.HISTORY_CHANGESET_TYPE_ID = (int)ChangeSetTypeEnum.Modified;
					hsChangeSet.ACTION_DESCRIPTION = "Updated Manage Login Providers";
					hsChangeSet.ARTIFACT_GUID_ID = provider.OAuthProviderId;

					long changeSetId = adminAuditManager.Insert1(hsChangeSet);

					//If there is a DLL provided, save the values. OR, only save if we're going from Active to Not Active.
					if (providerDll != null || (dbProv.IsActive && !provider.IsActive))
					{
						//Copy over the basic stuff, firsrt.
						dbProv.StartTracking();
						InsertAuditTrailDetailEntry(changeSetId, "IsActive", (int)userId, provider.IsActive.ToString(), dbProv.IsActive.ToString());
						dbProv.IsActive = provider.IsActive;

						InsertAuditTrailDetailEntry(changeSetId, "ClientId", (int)userId, provider.ClientId, dbProv.ClientId);
						dbProv.ClientId = provider.ClientId;

						InsertAuditTrailDetailEntry(changeSetId, "ClientSecret", (int)userId, provider.ClientSecret, dbProv.ClientSecret);
						dbProv.ClientSecret = provider.ClientSecret;

						if (provider.ImageUrl != null)
						{
							InsertAuditTrailDetailEntry(changeSetId, "ImageUrl", (int)userId, provider.ImageUrl, dbProv.ImageUrl);
						}
							dbProv.ImageUrl = provider.ImageUrl;

						if (provider.Custom1 != null)
						{
							InsertAuditTrailDetailEntry(changeSetId, "Custom1", (int)userId, provider.Custom1, dbProv.Custom1);
						}
						dbProv.Custom1 = provider.Custom1;

						if (provider.Custom2 != null)
						{
							InsertAuditTrailDetailEntry(changeSetId, "Custom2", (int)userId, provider.Custom2, dbProv.Custom2);
						}
						dbProv.Custom2 = provider.Custom2;

						if (provider.Custom3 != null)
						{
							InsertAuditTrailDetailEntry(changeSetId, "Custom3", (int)userId, provider.Custom3, dbProv.Custom3);
						}
						dbProv.Custom3 = provider.Custom3;

						if (providerDll != null)
						{
							if (providerDll.IsUrlsRequired)
							{
								InsertAuditTrailDetailEntry(changeSetId, "ProfileUrl", (int)userId, provider.ProfileUrl, dbProv.ProfileUrl);
								dbProv.ProfileUrl = provider.ProfileUrl;

								InsertAuditTrailDetailEntry(changeSetId, "TokenUrl", (int)userId, provider.TokenUrl, dbProv.TokenUrl);
								dbProv.TokenUrl = provider.TokenUrl;

								InsertAuditTrailDetailEntry(changeSetId, "AuthorizationUrl", (int)userId, provider.AuthorizationUrl, dbProv.AuthorizationUrl);
								dbProv.AuthorizationUrl = provider.AuthorizationUrl;
							}
							if (!providerDll.IsLogoProvided)
							{ }
						}

						try
						{
							ct.SaveChanges();

							//Configure the provider! (Only if it's active. Otherwise, save processing power.)
							if (dbProv.IsActive)
							{
								providerDll.BeginInit();

								InsertAuditTrailDetailEntry(changeSetId, "ClientId", (int)userId, dbProv.ClientId, providerDll.ClientId);
								providerDll.ClientId = dbProv.ClientId;

								InsertAuditTrailDetailEntry(changeSetId, "ClientSecretKey", (int)userId, dbProv.ClientSecret, providerDll.ClientSecretKey);
								providerDll.ClientSecretKey = dbProv.ClientSecret;

								if (providerDll.IsUrlsRequired)
								{
									//InsertAuditTrailDetailEntry(changeSetId, "ClientId", (int)userId, dbProv.AuthorizationUrl, providerDll.UrlAuthorization);
									providerDll.UrlAuthorization = new Uri(dbProv.AuthorizationUrl);

									//InsertAuditTrailDetailEntry(changeSetId, "ClientId", (int)userId, dbProv.ProfileUrl, providerDll.UrlProfile.ToString());
									providerDll.UrlProfile = new Uri(dbProv.ProfileUrl);

									//InsertAuditTrailDetailEntry(changeSetId, "ClientId", (int)userId, dbProv.TokenUrl, providerDll.UrlToken.ToString());
									providerDll.UrlToken = new Uri(dbProv.TokenUrl);
								}
								providerDll.EndInit();
							}

							retVal = true;
						}
						catch (Exception ex)
						{
							Logger.LogErrorEvent(METHOD, ex, "Trying to save changed to OAuth Provider.");
						}
					}
					else
					{

					}
				}
			}

			Logger.LogExitingEvent(METHOD);
			return retVal;
		}

		/// <summary>Deletes the settings for the given provider.</summary>
		/// <param name="providerId">The ID of the provider to remove.</param>
		public bool Provider_Delete(Guid providerId)
		{
			const string METHOD = CLASS + "Provider_Delete()";
			Logger.LogEnteringEvent(METHOD);

			bool retVal = false;

			using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
			{
				var prov = ct.GlobalOAuthProviders
					.SingleOrDefault(p => p.OAuthProviderId.Equals(providerId));

				if (prov != null)
				{
					ct.DeleteObject(prov);
					ct.SaveChanges();
					retVal = true;
				}
			}

			Logger.LogExitingEvent(METHOD);
			return retVal;
		}

		/// <summary>Connects an existing user to the OAuth Provider</summary>
		/// <param name="userId">The Spira's UserID to link</param>
		/// <param name="providerId">The GUID of the OAuth Provider</param>
		/// <param name="providerKey">The OAuth's unique ID for the user.</param>
		public User User_LinkToProvider(int userId, Guid providerId, string providerKey)
		{
			const string METHOD = CLASS + "User_LinkToProvider()";
			Logger.LogEnteringEvent(METHOD);

			//The return object.
			User retVal = null;

			using (var ct = new SpiraTestEntitiesEx())
			{
				//Pull the user, if.
				var user = ct.Users
					.SingleOrDefault(u => u.UserId == userId);

				//Filter out the Admin account, and any account that has either the OAth fields filled out.
				if (user != null)
				{
					if (user.UserId != 1 &&
						(string.IsNullOrWhiteSpace(user.OAuthAccessToken) ||
						!user.OAuthProviderId.HasValue))
					{
						//LDAP users need to be unlinked first.
						//  TODO: When flag is added, allow this if option is set.
						if (string.IsNullOrWhiteSpace(user.LdapDn))
						{
							user.StartTracking();
							//Save the provider's ID & key.
							user.OAuthProviderId = providerId;
							user.OAuthAccessToken = providerKey;
							//Clear out the password and other uneeded values.
							user.Password = null;
							user.LdapDn = null;
							user.LastPasswordChangedDate = null;
							user.IsLocked = false;
							user.PasswordQuestion = null;
							user.PasswordAnswer = null;

							//Save!
							ct.SaveChanges();

							//Return our object.
							retVal = user;

							//TODO: Send out notification email.
						}
					}
				}
			}

			Logger.LogExitingEvent(METHOD);
			return retVal;
		}

		/// <summary>Unlinks a user from any provider they may be linked to.</summary>
		/// <param name="userId">The user's ID to unlink.</param>
		/// <param name="PasswordChange">The MembershipProvider function to change the Password.</param>
		/// <param name="SecretChange">The MembershipProvider function to change the Secret WQuestion/Answer.</param>
		/// <remarks>Since the Password and Secret Answer are encrypted, code is not duplicated here. Instead, the functions are passed,
		/// and called directly, so that any changes made to encryption in the database are</remarks>
		public User User_UnlinkProvider(
			int userId,
			string newUsername,
			string newPass,
			string newQuestion,
			string newAnswer,
			out string errorMsg,
			MembershipProvider_ChangePasswordAsAdmin PasswordChange,
			MembershipProvider_ChangePasswordQuestionAndAnswerAsAdmin SecretChange)
		{
			//Our return user.
			User retVal = null;
			errorMsg = null;

			//Create context.
			using (var ct = new SpiraTestEntitiesEx())
			{
				//Pull the user from the database, make sure we have 
				User existingUser = ct.Users.SingleOrDefault(u => u.UserId.Equals(userId));

				//Okay, we have the user. Now let us update things.
				if (existingUser != null)
				{
					bool passSucc = PasswordChange(existingUser.UserName, newPass);

					if (!passSucc)
						errorMsg = "Could not update the user's password.";
					else
					{
						//Update the password Q&A, if needed.
						if (!string.IsNullOrEmpty(newAnswer) && !string.IsNullOrWhiteSpace(newQuestion))
						{
							bool secretSucc = SecretChange(existingUser.UserName, newQuestion, newAnswer);

							if (!secretSucc)
								errorMsg = "Could not update the user's secret question/answer.";
						}

						//Now let us clear out the user fields.
						try
						{
							//We apparently have to get the user again. Because for some reason,
							//  despite the password not being changed *in this object*, THIS object
							//  has a null password. On saving, it causes the newly created password
							//  to be overwritte with the null on.
							UserManager uMgr = new UserManager();

							existingUser = uMgr.GetUserById((int)userId);
							existingUser.StartTracking();

							//Set new username.
							existingUser.UserName = newUsername;
							//Clear the OAuth fields and LDAP fields.
							existingUser.OAuthAccessToken = null;
							existingUser.OAuthProviderId = null;
							existingUser.LdapDn = null;
							//Save changes.
							uMgr.Update(existingUser);

							//Now, let's repull our user and send it back to the calling function.
							retVal = uMgr.GetUserById((int)userId);
						}
						catch (Exception ex)
						{
							Logger.LogErrorEvent(CLASS + "User_UnlinkProvider", ex, "Trying to update user object.");
							errorMsg = "Error updating user: " + ex.Message;
						}
					}
				}
			}

			//Return!
			return retVal;
		}

		/// <summary>Sets the Avatar image data from the given data. (Which is usually gotten from the claim.)</summary>
		/// <param name="userId">The UserId to set the avatar for.</param>
		/// <param name="avatarData">If binary data, sets the data into the avatar. if URL, the image URL is downloaded and saved.</param>
		/// <returns>True if avatar data was able to be saved. False if not.</returns>
		public async Task<bool> User_SetAvatarAsync(long userId, object avatarData)
		{
			const string METHOD = CLASS + "User_SetAvatarAsync()";
			Logger.LogEnteringEvent(METHOD);

			//Return value.
			bool retValue = false;

			//See if it's a URL, first.
			if (avatarData is string)
			{
				Uri tryAddr;
				if (Uri.TryCreate(avatarData as string, UriKind.RelativeOrAbsolute, out tryAddr))
				{
					//It's a URL. Try to download the image.
					using (var client = new WebClient())
					{
						client.DownloadDataCompleted += delegate (object o, DownloadDataCompletedEventArgs evt)
						{
							try
							{
								if (evt.Error != null)
								{
									Logger.LogErrorEvent(METHOD, evt.Error, "Error downloading user's avatar. (1)");
								}
								else if (!evt.Cancelled)
								{
									AvatarDownloadCompleted(evt.Cancelled, evt.Result, userId);
								}
							}
							catch (Exception ex)
							{
								Logger.LogErrorEvent(METHOD, ex, "Error downloading user's avatar. (2)");
							}
						};

						Logger.LogTraceEvent(METHOD, "Launching client.");
						System.Threading.Tasks.Task call = client.DownloadDataTaskAsync(tryAddr);
						await call;
						Logger.LogTraceEvent(METHOD, "Client returned.");
						//byte[] imageData = await client.DownloadDataTaskAsync(tryAddr);
						retValue = true;
					}
				}
				else
				{
					//It may be data. Skip for now, until we have something to test off of.
					retValue = false;
				}
			}
			else if (avatarData.GetType() == typeof(byte).MakeArrayType())
			{
				//Right now, we need to call the callback manually.
				AvatarDownloadCompleted(false, (byte[])avatarData, userId);
				retValue = true;
			}

			Logger.LogExitingEvent(METHOD);
			return retValue;
		}
		#endregion

		#region Login Functions
		/// <summary>Called on page reload, to see if we have a return from a client.</summary>
		/// <param name="providerId">The Guid of the provider to process the callback.</param>
		/// <returns>THe user account that this person logged in as, plus backup data in case they need to create/link up an existing user.</returns>
		public async Task<UserLoginInfo> Process_CallbackAsync()
		{
			const string METHOD = CLASS + "Process_CallbackAsync()";
			Logger.LogEnteringEvent(METHOD);

			//The user information of the user if they successfully logged on.
			UserLoginInfo userRet = null;

			//We need to see if we have our AuthorizationContext cookie set, and get the provider from that.
			var ctx = HttpContext.Current;
			var cookie = (string)ctx.Session[AUTH_COOKNAME];

			//If we have no cookie, return null - no login, or no attempt.
			if (cookie == null) return null;
			else
			{
				//Pull the cookie's data.
				AuthorizationContext authCtx = null;
				try
				{
					var json = Encoding.UTF8.GetString(Unprotect(cookie));
					authCtx = AuthorizationContext.Parse(json);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD, ex, "Could not decode OAuth cookie.");
				}

				//Remove the value.
				ctx.Session.Remove(AUTH_COOKNAME);

				if (authCtx != null)
				{
					//Get the Provider from the cookie..
					var prov = _providers.SingleOrDefault(g => g.ProviderId == authCtx.ProviderId);
					if (prov != null)
					{
						try
						{
							var result = await prov.ProcessCallbackAsync(authCtx, ctx.Request.QueryString);
							result.ReturnUrl = authCtx.ReturnUrl; //To redirect user at the end, if we need to.
							result.ProviderId = authCtx.ProviderId;

							if (result != null && result.Claims != null)
							{
								//See if the person is logged in.
								var kvp = result.Claims.FirstOrDefault(k => k.Key == (int)Provider.ClaimTypeEnum.NameIdentifier);
								var nameIdClaim = kvp.Value;

								if (nameIdClaim != null)
								{
									//Create a new instance.
									userRet = new UserLoginInfo
									{
										//Record the ProviderId & Provider Guid.. This so if there IS NO user account.
										//  we can link one up later.
										ProviderId = result.ProviderId,
										UserProviderId = nameIdClaim.ToString(),
										ProviderData = result.Claims,
										ProviderName = prov.Name
									};

									//We need to look up the matching user for the ProviderID and IDClaim.
									using (var ct = new SpiraTestEntitiesEx())
									{
										userRet.UserAcct = ct.Users
											.Include("Profile")
											.SingleOrDefault(u =>
												u.OAuthProviderId == userRet.ProviderId &&
												u.OAuthAccessToken == userRet.UserProviderId);

										//If we have a user, then let us copy ove the ReturnURL, to send them where we want them to go.
										if (userRet.UserAcct != null && !string.IsNullOrWhiteSpace(authCtx.ReturnUrl))
											userRet.ReturnUrl = authCtx.ReturnUrl;
									}
								}
								else
								{
									//See if we have an error claim.
									if (!string.IsNullOrWhiteSpace(result.Error))
									{
										userRet = new UserLoginInfo()
										{
											ProviderId = result.ProviderId,
											ProviderName = prov.Name,
											ProviderData = new Dictionary<int, object> { { Provider.ERROR_CODE, result.ErrorDetails } }
										};
									}
									else if (result.Claims.Any(k => k.Key == Provider.ERROR_CODE))
									{
										userRet = new UserLoginInfo()
										{
											ProviderId = result.ProviderId,
											ProviderName = prov.Name,
											ProviderData = new Dictionary<int, object> { { Provider.ERROR_CODE, result.Claims[Provider.ERROR_CODE].ToString() } }
										};
									}
									else
									{
										userRet = new UserLoginInfo()
										{
											ProviderId = result.ProviderId,
											ProviderName = prov.Name,
											ProviderData = new Dictionary<int, object> { { Provider.ERROR_CODE, "XX No useable claims returned from provider." } }
										};
									}
								}
							}
							else
							{
								//Handle cases where an error was thrown.
								if (!string.IsNullOrWhiteSpace(result.Error))
								{
									userRet = new UserLoginInfo()
									{
										ProviderId = result.ProviderId,
										ProviderData = new Dictionary<int, object> { { Provider.ERROR_CODE, "[" + result.Error + "] " + result.ErrorDetails } },
										ProviderName = prov.Name
									};
								}
							}
						}
						catch (Exception ex)
						{
							userRet = null;
							Logger.LogErrorEvent(METHOD, ex, "OAuth cookie had error.");
						}
					}
				}
				else
				{
					//Throw error?
					userRet = null;
				}
			}
			Logger.LogExitingEvent(METHOD);
			return userRet;
		}

		/// <summary>Redirects the user to the provider's page to hand the login.</summary>
		/// <param name="providerId">THe Guid of the provider that they are using.</param>
		/// <param name="returnUrl">If needed to redirect back to a different url, speficy it here.</param>
		public void Process_RedirectToProviderPage(Guid providerId, string returnUrl = null)
		{
			const string METHOD = CLASS + "Process_RediretToProviderPage()";
			Logger.LogEnteringEvent(METHOD);

			try
			{
				Providers_LoadDLLs();
				//Find the provider, make sure they gave us a valid Guid.
				var provider = _providers.SingleOrDefault(g => g.ProviderId == providerId);
				if (provider != null)
				{
					//The URL to send the user to, for logging in.
					var redirect = provider.GetRedirect();
					//Create our cookie, so that on the return, we know who this is.
					var authCtx = new AuthorizationContext
					{
						ProviderId = provider.ProviderId,
						ReturnUrl = returnUrl,
						State = redirect.State
					}.ToJson();
					var cookieData = Protect(Encoding.UTF8.GetBytes(authCtx)); //Encrypt it.

					//Create and configure the cookie.
					var ctx = HttpContext.Current;
					if (ctx != null)
					{
						ctx.Session.Add(AUTH_COOKNAME, cookieData);

						//Redirect the user.
						ctx.Response.Redirect(redirect.AuthorizationUrl);
					}
					else
					{
						throw new Exception("Could not access HTTPContext.");
					}
				}
				else
				{
					throw new Exception("Invalid OAuth provider Guid was passed. [" + providerId.ToString() + "]");
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD, ex);
			}

			Logger.LogExitingEvent(METHOD);
		}
		#endregion

		#region Private Functions
		/// <summary>Gets the Provider Object (DLL interface) for the given Guid. Null if not found.</summary>
		/// <param name="id">The Guid of the provider to return.</param>
		private Provider GetProvider(Guid id)
		{
			Provider ret = null;

			ret = _providers.SingleOrDefault(p => p.ProviderId.Equals(id));

			return ret;
		}

		/// <summary>Encrpys the byte data.</summary>
		/// <param name="data">The data to encrypt.</param>
		string Protect(byte[] data)
		{
			if (data == null || data.Length == 0) return null;
			var value = MachineKey.Protect(data, Encoding.UTF8.GetString(machineKey));
			return Convert.ToBase64String(value);
		}

		/// <summary>Decode an encrypted string.</summary>
		/// <param name="value">The string of data to decode.</param>
		/// <returns>Unencrypoted bytes.</returns>
		byte[] Unprotect(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return null;
			var bytes = Convert.FromBase64String(value);
			return MachineKey.Unprotect(bytes, Encoding.UTF8.GetString(machineKey));
		}

		private void AvatarDownloadCompleted(bool isCancelled, byte[] result, long userId)
		{
			const string METHOD = CLASS + "AvatarDownloadCompleted()";
			Logger.LogEnteringEvent(METHOD);

			//It's an array of bytes. Need to check it's valid image data first.
			byte[] btJpg = new byte[] { 0xFF, 0xD8 };                           //JPEG Header
			byte[] btGif = new byte[] { 0x47, 0x49, 0x46 };                     //GIF header
			byte[] btPng = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };      //PNG header
			string mimeType = null;
			if (result.SubArray(0, btJpg.Length).SequenceEqual(btJpg))
				mimeType = "image/jpeg";
			else if (result.SubArray(0, btGif.Length).SequenceEqual(btGif))
				mimeType = "image/gif";
			else if (result.SubArray(0, btPng.Length).SequenceEqual(btPng))
				mimeType = "image/png";

			if (!string.IsNullOrWhiteSpace(mimeType))
			{
				//It's either a JPG, GIF, or PNG. We're good. Save the data here.
				using (var ct = new SpiraTestEntitiesEx())
				{
					//Get the user's profile.
					var profile = ct.UserProfiles
						.SingleOrDefault(u => u.UserId == userId);

					//If we don't have a profile, skip it.
					if (profile != null)
					{
						profile.StartTracking();
						profile.AvatarImage = Convert.ToBase64String(result);
						profile.AvatarMimeType = mimeType;
						ct.SaveChanges();
					}
				}
			}
			else
			{
				//Throw error?
			}

			Logger.LogExitingEvent(METHOD);
		}
		#endregion Private Functions

		#region Helper Classes
		public class UserLoginInfo
		{
			/// <summary>THe user we are logged in as. Null if the user entered in something bad, or there is no link between account and user.</summary>
			public User UserAcct { get; set; }

			/// <summary>The Guid of the provider that verified these credentials.</summary>
			public Guid ProviderId { get; set; }

			/// <summary>The unique ID for this provider for this user. Unique with the Provider Id.</summary>
			public string UserProviderId { get; set; }

			/// <summary>Returns the data sent to us from the OAuth Provider.</summary>
			public Dictionary<int, object> ProviderData { get; set; }

			/// <summary>Contains the name of the provider, in case we need it for the Loginpage.</summary>
			public string ProviderName { get; set; }

			/// <summary>In case a Return URL was specified. If the user is logged in successfully, and this is set, we
			/// will redirect the user to this URL.</summary>
			public string ReturnUrl { get; set; }

			#region Translation Functions
			/// <summary>Convert our object into a JSON-parseable string.</summary>
			public string ToJson()
			{
				string retVal = "";

				if (this != null)
					retVal = JsonConvert.SerializeObject(this);
				else
					retVal = JsonConvert.SerializeObject(new UserLoginInfo());

				return retVal;
			}

			/// <summary>Convert the string back into an object.</summary>
			/// <param name="json">The JSON to parse.</param>
			/// <returns>An object, or null if there was an error parsing it.</returns>
			public static UserLoginInfo Parse(string json)
			{
				UserLoginInfo retVal = null;

				try
				{
					retVal = JsonConvert.DeserializeObject<UserLoginInfo>(json);
				}
				catch
				{
					retVal = null;
				}

				return retVal;
			}
			#endregion Translation Functions
		}
		#endregion
	}
}
