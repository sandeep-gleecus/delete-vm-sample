using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.MasterPages;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Security;


namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// User Profile Edit Page and handling all raised events
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.MyProfile, null, "User-Product-Management/#my-profile")]
	public partial class UserProfile : Inflectra.SpiraTest.Web.PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserProfile::";

		public const string SCREENSHOT_FILE_EXTENSION = "png";

		User user = null;

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
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Reset the error messages
			lblMessage.Text = "";

			//Add the event handlers to the page
			btnUpdate.Click += btnUpdate_Click;
			btnCancel.Click += btnCancel_Click;
			btnGenerateNewToken.Click += btnGenerateNewToken_Click;
			btnDeleteAvatar.Click += btnDeleteAvatar_Click;
            btnUnlinkOauth.Click += btnUnlinkOauth_Click;

			//Set the localized page title
			((MasterPageBase)Master).PageTitle = Resources.Main.GlobalNavigation_MyProfile;

			//Set pnlActivity properties..
			grdUserActivity.ProjectId = -1;
			grdUserActivity.SetFilters(new Dictionary<string, object>() { { "UserId", UserId } });

			//Retrieve the user from the membership provider directly
			SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;
			user = provider.GetProviderUser(UserId);


			//Only load the data once
			if (!IsPostBack)
			{
				//Set the Yes/No dropdown lists.
				ddlEmailActive.DataSource = new UserManager().RetrieveFlagLookup();

				//Get the list of possible cultures
				CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
				SortedDictionary<string, string> sortedCultures = new SortedDictionary<string, string>();
				foreach (CultureInfo culture in cultures)
				{
					if (!sortedCultures.ContainsKey(culture.Name))
					{
						sortedCultures.Add(culture.Name, culture.DisplayName);
					}
				}
				ddlUserCulture.DataSource = sortedCultures;

				//Get the list of possible timezones
				ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
				Dictionary<string, string> timeZonesDic = new Dictionary<string, string>();
				foreach (TimeZoneInfo timeZone in timeZones)
				{
					if (!timeZonesDic.ContainsKey(timeZone.Id))
					{
						//The timezone ID contains spaces which we don't want
						timeZonesDic.Add(timeZone.Id.Replace(" ", "-"), timeZone.DisplayName);
					}
				}
				ddlUserTimezone.DataSource = timeZonesDic;

				//Populate the list of possible start page options
				ddlStartPage.DataSource = new Dictionary<int, string>
				{
					{ (int)GlobalNavigation.NavigationHighlightedLink.MyPage, Resources.Main.UserProfile_StartPage_MyPage },
					{ (int)GlobalNavigation.NavigationHighlightedLink.ProjectHome, Resources.Main.UserProfile_StartPage_ProjectHome },
					{ (int)GlobalNavigation.NavigationHighlightedLink.ProjectGroupHome, Resources.Main.UserProfile_StartPage_ProjectGroupHome }
				};

				//Hide the sub panels that need to be hidden.
				pnlSubPass_Ldap.Visible = !string.IsNullOrWhiteSpace(user.LdapDn);
				pnlSubPass_Oauth.Visible = user.OAuthProviderId.HasValue || !string.IsNullOrWhiteSpace(user.OAuthAccessToken);
				pnlSubPass_Pass.Visible = !pnlSubPass_Ldap.Visible && !pnlSubPass_Oauth.Visible;

				//Specify if the history tab has data
				int activityCount = new HistoryManager().CountSet(
					null,
					new Hashtable { { "UserId", UserId } },
					GlobalFunctions.GetCurrentTimezoneUtcOffset());
				tabActions.HasData = (activityCount > 0);

				//Databind the form
				DataBind();

				//Populate the form with the existing values
				txtFirstName.Text = user.Profile.FirstName;
				txtLastName.Text = user.Profile.LastName;
				txtMiddleInitial.Text = (string.IsNullOrEmpty(user.Profile.MiddleInitial) ? "" : user.Profile.MiddleInitial);
				lblUserName.Text = user.UserName;
				lblUserId.Text = string.Format(GlobalFunctions.FORMAT_ID, user.UserId);
				txtEmailAddress.Text = user.EmailAddress;
				txtRssToken.Text = (string.IsNullOrEmpty(user.RssToken) ? "" : user.RssToken);
				chkRssEnabled.Checked = !(string.IsNullOrEmpty(user.RssToken));
				txtDepartment.Text = user.Profile.Department;
				txtOrganization.Text = user.Profile.Organization;
				txtQuestion.Text = user.PasswordQuestion;
				if (pnlSubPass_Oauth.Visible)
				{
					cleanUserName.Text = user.UserName;
					cleanUserName.ReadOnly = true;
				}

				//Populate the Oauth string.
				if (user.OAuthProviderId != null)
				{
					var prov = new OAuthManager().Providers_RetrieveById(user.OAuthProviderId.Value);

					string Oauth = Resources.Messages.UserProfile_OAuthLinkedSummary;
					if (user.OAuthProviders != null)
						Oauth = string.Format(Oauth,
							ConfigurationSettings.Default.License_ProductType,
							user.OAuthProviders.Name);
					else
					{
						if (prov != null)
							Oauth = string.Format(Oauth,
								ConfigurationSettings.Default.License_ProductType,
								prov.Name);
						else
							Oauth = string.Format(Oauth,
								ConfigurationSettings.Default.License_ProductType,
								user.OAuthProviderId.Value.ToString("B"));
					}
					litOAuth.Text = Oauth;

                    //Populate the unlink header.
                    dlgUnlinkOauth.Title = string.Format(Resources.Messages.UserProfile_OAuthUnlinkHead, prov.Name);
					litUnlinkSumm.Text = string.Format(Resources.Messages.UserProfile_OAuthUnlinkSummary, prov.Name);
				}

				ddlStartPage.SelectedValue = GetUserSetting(GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_START_PAGE, (int)GlobalNavigation.NavigationHighlightedLink.MyPage).ToString();

				if (ConfigurationSettings.Default.EmailSettings_Enabled)
				{
					if (ConfigurationSettings.Default.EmailSettings_AllowUserControl)
					{
						ddlEmailActive.SelectedValue = (user.Profile.IsEmailEnabled) ? "Y" : "N";
					}
					else
					{
						ddlEmailActive.SelectedValue = "Y";
						ddlEmailActive.Enabled = false;
					}
				}
				else
				{
					ddlEmailActive.SelectedValue = "N";
					ddlEmailActive.Enabled = false;
				}

				//Specify the user's culture and timezone if specified
				ddlUserCulture.SelectedValue = GetUserSetting(GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_CURRENT_CULTURE, "");
				ddlUserTimezone.SelectedValue = (user.Profile.Timezone == null) ? "" : user.Profile.Timezone.Replace(" ", "-");

				//The image
				imgAvatar.UserId = user.UserId;

				//Display the LDAP Distinguished Name if this user is an LDAP user
				//and disable the password box
				if (string.IsNullOrEmpty(user.LdapDn))
				{
					lblLdapDn.Text = Resources.Messages.UserProfile_NotLinked;
					txtCurrentPassword.Enabled = true;
					txtNewPassword.Enabled = true;
					txtConfirmPassword.Enabled = true;
				}
				else
				{
					lblLdapDn.Text = user.LdapDn;
					txtCurrentPassword.Enabled = false;
					txtNewPassword.Enabled = false;
					txtConfirmPassword.Enabled = false;
				}

				//Display the appropriate MFA settings link if MFA enabled
				//Make sure MFA is actually enabled for this instance
				if (ConfigurationSettings.Default.Security_EnableMfa)
				{
					//Display only for Login/Password or LDAP Users, but not OAUTH
					if (pnlSubPass_Oauth.Visible)
					{
						this.plcMfaSettings.Visible = false;
					}
					else
					{
						this.plcMfaSettings.Visible = true;

						//Display the appropriate MFA link (add or change)
						if (String.IsNullOrEmpty(user.MfaToken))
						{
							//There is no MFA, so display add link
							this.plcAddMFA.Visible = true;
							this.plcChangeMFA.Visible = false;
						}
						else
						{
							//There is an existing MFA, so display change link
							this.plcChangeMFA.Visible = true;
							this.plcAddMFA.Visible = false;
						}
					}
				}
				else
				{
					this.plcMfaSettings.Visible = false;
				}

				//Load Taravault info.
				VaultManager vMgr = new VaultManager();
				User tvUser = vMgr.User_RetrieveWithTaraVault(UserId);
				if (ProjectId > 0)
				{
					Project tvProj = vMgr.Project_RetrieveWithTaraVault(ProjectId);
					bool inProj = vMgr.User_RetrieveTaraProjectsForId(UserId).Any(p => p.ProjectId == ProjectId);
					if (ConfigurationSettings.Default.TaraVault_HasAccount && tvUser.TaraVault != null && tvProj != null && inProj)
					{
						lblTVaultLogin.Text = tvUser.TaraVault.VaultUserLogin;
						string clearPassword = new SimpleAES().DecryptString(tvUser.TaraVault.Password);
						txtTVaultPass.Text = GlobalFunctions.MaskPassword(clearPassword);
					}
					else
					{
						tabTaraV.Enabled = false;
						tabTaraV.Visible = false;
					}
				}
				else
				{
					tabTaraV.Enabled = false;
					tabTaraV.Visible = false;
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>Hit when the user wants to delete their profile avatar.</summary>
		/// <param name="sender">btnDelete</param>
		/// <param name="e">EventArgs</param>
		void btnDeleteAvatar_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "btnDelete_Click()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				if (UserId > 0)
				{
					//Remove the avatar
					ProfileEx profile = new ProfileEx();
					profile.AvatarImage = null;
					profile.AvatarMimeType = null;
					profile.Save();

					//Announce we did it..
					lblFileMessage.Type = MessageBox.MessageType.Information;
					lblFileMessage.Text = Resources.Messages.UserProfile_AvatarRemoved;
				}
			}
			catch
			{
				lblFileMessage.Type = MessageBox.MessageType.Error;
				lblFileMessage.Text = Resources.Messages.UserProfile_AvatarNotRemoved;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		/// Generates a new RSS token
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnGenerateNewToken_Click(object sender, EventArgs e)
		{
			txtRssToken.Text = GlobalFunctions.GenerateGuid();
		}

		/// <summary>
		/// Validates the form, and updates the user profile/password as appropriate
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		private void btnUpdate_Click(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!IsValid)
			{
				return;
			}

			//Retrieve the user from the membership provider directly
			//We do this because we need access to the extended properties such as RSS Token
			//that the normal membership user object does not have
			SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;
			DataModel.User user = provider.GetProviderUser(UserId);
			if (user != null)
			{
				//First see if we need to change the password question and answer
				//Need to do this before the password is changed (since it won't match afterwards!!)
				if (!string.IsNullOrWhiteSpace(txtCurrentPasswordForQuestion.Text) && !string.IsNullOrWhiteSpace(txtQuestion.Text) && !string.IsNullOrWhiteSpace(txtAnswer.Text))
				{
					bool success = provider.ChangePasswordQuestionAndAnswer(user.UserName, txtCurrentPasswordForQuestion.Text.Trim(), txtQuestion.Text.Trim(), txtAnswer.Text.Trim());
					if (!success)
					{
						//Display the message that the current password doesn't match
						lblMessage.Text = Resources.Messages.UserDetails_NewPasswordError5;
						lblMessage.Type = MessageBox.MessageType.Error;
						return;
					}
				}

				//Since the validators take care of the main form, we just need to ensure
				//that the passwords are entered and match
				string pasCurrent = txtCurrentPassword.Text;
				string pasNew = txtNewPassword.Text;
				string pasConfirm = txtConfirmPassword.Text;
				if (!string.IsNullOrEmpty(pasCurrent.Trim()) || !string.IsNullOrEmpty(pasNew.Trim()) || !string.IsNullOrEmpty(pasConfirm.Trim()))
				{
					//Now check that the current one is entered.
					if (!string.IsNullOrEmpty(pasCurrent.Trim()))
					{
						//Now make sure the new passwords match and aren't null.
						if (pasNew == pasConfirm)
						{
							//Now make sure the new password is long enough
							if (!string.IsNullOrWhiteSpace(pasNew) && pasNew.Length >= Membership.MinRequiredPasswordLength)
							{
								//Now make sure they have the minimum number of special characters if necessary
								if (provider.DoesPasswordHaveEnoughSpecialChars(pasNew))
								{
									//Make sure they don't break the 'no names in password' setting if enabled
									if (provider.DoesPasswordComplyWithNameRules(user, pasNew))
									{
										bool success = provider.ChangePassword(user.UserName, pasCurrent, pasNew);
										if (!success)
										{
											//Current password incorrect.
											lblMessage.Text = Resources.Messages.UserDetails_NewPasswordError4;
											lblMessage.Type = MessageBox.MessageType.Error;
											return;
										}
									}
									else
									{
										//New password contains names which is not allowed
										lblMessage.Text = Resources.Messages.UserDetails_NewPasswordContainsNames;
										lblMessage.Type = MessageBox.MessageType.Error;
										return;
									}
								}
								else
								{
									//New password does not have enough special characters
									lblMessage.Text = string.Format(Resources.Messages.UserDetails_NewPasswordError6, Membership.MinRequiredNonAlphanumericCharacters);
									lblMessage.Type = MessageBox.MessageType.Error;
									return;
								}
							}
							else
							{
								//New password is too short
								lblMessage.Text = string.Format(Resources.Messages.UserDetails_NewPasswordError7, Membership.MinRequiredPasswordLength);
								lblMessage.Type = MessageBox.MessageType.Error;
								return;
							}
						}
						else
						{
							//New passwords do not patch.
							lblMessage.Text = Resources.Messages.UserDetails_NewPasswordError2;
							lblMessage.Type = MessageBox.MessageType.Error;
							return;
						}
					}
					else
					{
						//Need to enter current password.
						lblMessage.Text = Resources.Messages.UserDetails_NewPasswordError1;
						lblMessage.Type = MessageBox.MessageType.Error;
						return;
					}
				}

				//Make the other updates
				user.StartTracking();
				user.Profile.StartTracking();
				user.Profile.LastUpdateDate = DateTime.UtcNow;
				user.Profile.FirstName = txtFirstName.Text.Trim();
				if (txtMiddleInitial.Text == "")
					user.Profile.MiddleInitial = null;
				else
					user.Profile.MiddleInitial = txtMiddleInitial.Text.Trim();
				user.Profile.LastName = txtLastName.Text.Trim();
				user.EmailAddress = txtEmailAddress.Text.Trim();
				user.Profile.Department = txtDepartment.Text.Trim();
				user.Profile.Organization = txtOrganization.Text.Trim();
				user.Profile.IsEmailEnabled = (ddlEmailActive.SelectedValue == "Y");
				user.Profile.Timezone = ddlUserTimezone.SelectedValue.Replace("-", " ");
				if (string.IsNullOrWhiteSpace(txtRssToken.Text))
					user.RssToken = null;
				else
					user.RssToken = txtRssToken.Text.Trim();

				//Uploading a new avatar?
				if (filAttachment.HasFile)
				{
					if (filAttachment.FileBytes.Length < (100 * 1024) + 1)
					{
						//Get the image type.
						string mime = "image/png";
						string ext = Path.GetExtension(filAttachment.FileName);
						if (ext.ToLowerInvariant().Trim() == ".gif")
							mime = "image/gif";
						else if (ext.ToLowerInvariant().Trim() == ".jpeg" || ext.ToLowerInvariant().Trim() == ".jpg")
							mime = "image/jpeg";
						else if (ext.ToLowerInvariant().Trim() == ".png")
							mime = "image/png";
						else
						{
							//Display the message that the user name was already in use
							lblMessage.Text = Resources.Messages.UserProfile_AvatarWrongType;
							lblMessage.Type = MessageBox.MessageType.Error;
							return;
						}

						//Base64-encode
						string base64Image = Convert.ToBase64String(filAttachment.FileBytes);
						user.Profile.AvatarImage = base64Image;
						user.Profile.AvatarMimeType = mime;  //Currently hard coded
					}
					else
					{
						//Display the message that the user name was already in use
						lblMessage.Text = Resources.Messages.UserProfile_AvatarTooLarge;
						lblMessage.Type = MessageBox.MessageType.Error;
						return;
					}
				}

				//Commit the changes
				provider.UpdateProviderUser(user);

				//Next update the culture info
				UserSettingsCollection userSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS);
				if (string.IsNullOrEmpty(ddlUserCulture.SelectedValue))
				{
					if (userSettings.ContainsKey(GlobalFunctions.USER_SETTINGS_KEY_CURRENT_CULTURE))
					{
						userSettings.Remove(GlobalFunctions.USER_SETTINGS_KEY_CURRENT_CULTURE);
						userSettings.Save();
					}
				}
				else
				{
					userSettings[GlobalFunctions.USER_SETTINGS_KEY_CURRENT_CULTURE] = ddlUserCulture.SelectedValue;
					userSettings.Save();
				}

				//Next update the start page
				if (!string.IsNullOrEmpty(ddlStartPage.SelectedValue))
				{
					int startPage;
					if (int.TryParse(ddlStartPage.SelectedValue, out startPage))
					{
						userSettings[GlobalFunctions.USER_SETTINGS_KEY_START_PAGE] = startPage;
						userSettings.Save();
					}
				}

				//Next, update the TaraVault password, if it has changed
				if (!GlobalFunctions.IsMasked(txtTVaultPass.Text.Trim()))
				{
					new VaultManager().User_CreateUpdateTaraAccount(user.UserId, lblTVaultLogin.Text.Trim(), txtTVaultPass.Text.Trim());
				}

				//Mention it's been saved.
				lblMessage.Text = Resources.Messages.UserProfile_PreferencesSaved;
				lblMessage.Type = MessageBox.MessageType.Information;
			}
			else
			{
				//Display an error
				lblMessage.Text = Resources.Messages.UserProfile_ErrorUpdatingProfile;
				lblMessage.Type = MessageBox.MessageType.Error;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Redirects the user back to the default application page when cancel clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "btnCancel_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId, 0), true);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>Clears the account's OAuth Login information.</summary>
		private void btnUnlinkOauth_Click(object sender, EventArgs e)
		{
			//Check that we have data in the needed fields.
			if (string.IsNullOrWhiteSpace(cleanUserName.Text) || string.IsNullOrWhiteSpace(unlinkOauth_Password.Text))
			{
				lblMessage.Text = "You need to supply a new username and password.";
				lblMessage.Type = MessageBox.MessageType.Error;
			}
			else
			{
				//Instantiate our custom membership provider
				SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;

				if (provider != null)
				{
					bool succ = provider.UnlinkAccountFromOAuth(
						user.UserId,
						cleanUserName.Text.Trim(),
                        unlinkOauth_Password.Text.Trim(),
						unlinkOauth_NewQuestion.Text.Trim(),
                        unlinkOauth_NewAnswer.Text.Trim());
				}

				//Sign the user out of EVERYTHING.
				Global.KillUserSessions(user.UserId, true);
				Session.RemoveAll();
				Session.Abandon();
				FormsAuthentication.SignOut();

				//Redirect them back here, BUT, not being logged in will cause them to require logging in again.
				Response.Redirect(Request.Url.ToString(), true);
			}
		}

		#endregion
	}
}
