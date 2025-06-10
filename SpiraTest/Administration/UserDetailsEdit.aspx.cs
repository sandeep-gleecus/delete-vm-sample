using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>This webform code-behind class is responsible to displaying the Administration User Details Page and handling all raised events</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "UserDetails_EditUser", "System-Users/#view-edit-users", "UserDetails_EditUser")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class UserDetailsEdit : AdministrationBase
	{
		protected SortedList<string, string> flagList;
		protected List<ProjectRole> projectRoles;
		protected User user = null;

		private const string CLASS_NAME = "Web.AdministrationUserDetailsEdit::";

		#region Event Handlers
		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Reset the error message
			lblMessage.Text = "";

			//Add the event handlers to the page
			btnUpdateTop.Click += btnUpdate_Click;
			btnCancelTop.Click += btnCancel_Click;
			btnUpdate.Click += btnUpdate_Click;
			btnCancel.Click += btnCancel_Click;
			btnUnlinkOauth.Click += btnUnlinkOauth_Click;
			btnUnlinkLdap.Click += btnUnlinkLdap_Click;
			btnGenerateNewToken.Click += btnGenerateNewToken_Click;
			btnMembershipUpdate.Click += btnMembershipUpdate_Click;
			btnMembershipAdd.Click += btnMembershipAdd_Click;
			btnMembershipDelete.Click += btnMembershipDelete_Click;
			grdTaraProjects.RowCommand += grdTaraProjects_RowCommand;
			btnRemoveMfa.Click += BtnRemoveMfa_Click;

			//Hide TaraVault tab if TV is not enabled.
			tabTara.Visible = (ConfigurationSettings.Default.TaraVault_HasAccount && Common.Global.Feature_TaraVault);

			//Retrieve the user from the querystring if we have one (Edit/Update Mode)
			//If we don't then we're in Add/Insert Mode
			int userId = -1;
			SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;

			//Get the userid..
			userId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);
			user = provider.GetProviderUser(userId);

			//Only load the data once
			if (!IsPostBack)
			{
				//Get the user datamapping if there are any
				DataMappingManager dataMappingManager = new DataMappingManager();
				List<DataSyncUserMappingView> userMappings = dataMappingManager.RetrieveDataSyncUserMappings(userId);
				rptDataSyncMappings.DataSource = userMappings;

				//Load & Bind TaraVault data..
				if (Common.Global.Feature_TaraVault)
				{
					LoadAndBindData_TaraVault(userId);
				}

				//Retrieve any lookups
				ProjectManager projectManager = new ProjectManager();
				flagList = projectManager.RetrieveFlagLookup();
				projectRoles = projectManager.RetrieveProjectRoles(true);

				//Retrieve the project user membership dataset
				if (userId != -1)
				{
					List<ProjectUser> projectUsers = projectManager.RetrieveProjectMembershipForUser(userId);
					grdProjectMembership.DataSource = projectUsers;
				}

				//Add user avatar image
				imgUserAvatar.ImageUrl = UrlRewriterModule.ResolveUserAvatarUrl(userId, Page.Theme);

				//Databind the form
				DataBind();

				//See if we have an LDAP user or not
				if (string.IsNullOrEmpty(user.LdapDn))
				{
					//Regular User
					lblLdapUser.Text = Resources.Main.Global_No;
					txtLdapDn.Text = "";
				}
				else
				{
					//LDAP User
					lblLdapUser.Text = Resources.Main.Global_Yes;
					txtLdapDn.Text = user.LdapDn;
					litUnlinkLdap.Text = string.Format(
						Resources.Main.Admin_UserDetails_UnlinkOAuthLdap_Summary,
						ConfigurationSettings.Default.License_ProductType,
						Resources.Main.Global_Ldap
					);
				}

				//Populate the form with the existing values (update case)
				lblUserName.Text = GlobalFunctions.HtmlRenderAsPlainText(user.FullName);
				txtFirstName.Text = user.Profile.FirstName;
				txtLastName.Text = user.Profile.LastName;
				txtMiddleInitial.Text = (string.IsNullOrEmpty(user.Profile.MiddleInitial)) ? "" : user.Profile.MiddleInitial;
				txtUserName.Text = user.UserName;
				txtEmailAddress.Text = user.EmailAddress;
				chkAdminYn.Checked = user.Profile.IsAdmin;
                chkActiveYn.Checked = user.IsActive;
                chkLockedOut.Checked = user.IsLocked;
				chkEmailEnabled.Checked = user.Profile.IsEmailEnabled;
				txtDepartment.Text = ((string.IsNullOrEmpty(user.Profile.Department)) ? "" : user.Profile.Department);
				txtOrganization.Text = ((string.IsNullOrEmpty(user.Profile.Organization)) ? "" : user.Profile.Organization);
				chkRssEnabled.Checked = !string.IsNullOrEmpty(user.RssToken);
				txtRssToken.Text = ((string.IsNullOrEmpty(user.RssToken)) ? "" : user.RssToken);
				txtPasswordQuestion.Text = ((string.IsNullOrEmpty(user.PasswordQuestion)) ? "" : user.PasswordQuestion);
				if (user.OAuthProviders != null)
				{
					txtOauthProviderInfo.Text = user.OAuthProviders.Name;
				}
				else if (user.OAuthProviderId.HasValue)
				{
					var prov = new OAuthManager().Providers_RetrieveById(user.OAuthProviderId.Value);
					if (prov != null)
						txtOauthProviderInfo.Text = prov.Name;
					else
						txtOauthProviderInfo.Text = user.OAuthProviderId.Value.ToString("B");

					litUnlinkOauth.Text = string.Format(
						Resources.Main.Admin_UserDetails_UnlinkOAuthLdap_Summary,
						ConfigurationSettings.Default.License_ProductType,
						txtOauthProviderInfo.Text
					);
				}

				//Hide or show the appropriate panel and sections.
				plcLDAPInfo.Visible = !string.IsNullOrWhiteSpace(user.LdapDn);
				plcOauthInfo.Visible = user.OAuthProviderId.HasValue;
				plcPassInfo.Visible =
					sectLastLock.Visible =
					sectLastChng.Visible =
						!(plcLDAPInfo.Visible || plcOauthInfo.Visible);
				plcLinkToLDAP.Visible = (!string.IsNullOrWhiteSpace(ConfigurationSettings.Default.Ldap_Host));

				sectLock.Visible = user.IsLocked;

				//If we are not OAUTH, display MFA section unless disabled
				this.plcMfaSettings.Visible = (ConfigurationSettings.Default.Security_EnableMfa && !user.OAuthProviderId.HasValue);

				//Show portfolio field if allowed
				if (Common.Global.Feature_Portfolios)
                {
                    this.portfolioFormGroup.Visible = true;
                    chkPortfolioAdmin.Checked = user.Profile.IsPortfolioAdmin;
                }
				//Populate the Report admin flag
				chkReportAdmin.Checked = user.Profile.IsReportAdmin;

				//Populate activity information into the table for these dates with existing values
				ltrLastActivityDate.Text = (user.LastActivityDate.HasValue) ? string.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(user.LastActivityDate).Value) : "";
				ltrLastLoginDate.Text = (user.LastLoginDate.HasValue) ? string.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(user.LastLoginDate).Value) : "";
				ltrLastLockoutDate.Text = (user.LastLockoutDate.HasValue) ? string.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(user.LastLockoutDate).Value) : "";
				ltrLastPasswordChangedDate.Text = (user.LastPasswordChangedDate.HasValue) ? string.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(user.LastPasswordChangedDate).Value) : "";

				//If this is the built-in system admin account, grey-out certain fields
				if (userId == UserManager.UserSystemAdministrator)
				{
					txtFirstName.Enabled = false;
					txtLastName.Enabled = false;
					txtMiddleInitial.Enabled = false;
					txtUserName.Enabled = false;
					chkAdminYn.Enabled = false;
					chkActiveYn.Enabled = false;
				}

				//Hide the disconnect button if needed.
				btnUnlinkOauth.Visible = (!string.IsNullOrWhiteSpace(user.OAuthAccessToken) || user.OAuthProviderId.HasValue);

				//If the user has a MFA token, provide option to remove
				bool isMfaEnabledForUser = !String.IsNullOrWhiteSpace(user.MfaToken);
				this.btnRemoveMfa.Visible = isMfaEnabledForUser;
				this.lblMfaEnabled.Text = (isMfaEnabledForUser) ? Resources.Fields.IsActive : Resources.Main.Global_Inactive;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		/// Removes the existing MFA information from a user
		/// </summary>
		private void BtnRemoveMfa_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "BtnRemoveMfa_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Simply set the token to NULL
				SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;
				
				//Get the userid..
				int userId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);
				user = provider.GetProviderUser(userId);
				user.StartTracking();
				user.MfaToken = null;
				user.LastActivityDate = DateTime.UtcNow;
				provider.UpdateProviderUser(user);

				//Finally redirect back to the same page (often we want to do something else like reset password)
				Response.Redirect("UserDetailsEdit.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + user.UserId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (ArtifactNotExistsException)
			{
				this.lblMessage.Text = Resources.Messages.ChangeMfa_UserNotExists1;
				this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				this.lblMessage.Text = exception.Message;
				this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
			}

		}

		/// <summary>
		/// Unlinks the user from LDAP / ActiveDirectory and makes it a normal Spira account
		/// </summary>
		private void btnUnlinkLdap_Click(object sender, EventArgs e)
		{
			try
			{
				//Check that we have data in the needed fields.
				if (string.IsNullOrWhiteSpace(unlinkLdap_Password.Text))
				{
					lblMessage.Text = Resources.Messages.Admin_UserDetails_NewPasswordRequired;
					lblMessage.Type = MessageBox.MessageType.Error;
				}
				else
				{
					//Instantiate our custom membership provider
					SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;

					//Retrieve the user from the ID held in the querysting
					int userId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);
					User user = provider.GetProviderUser(userId);

					string pasNew = unlinkLdap_Password.Text;
					string pasConfirm = unlinkLdap_VerifyPassword.Text;

					if (string.IsNullOrEmpty(pasConfirm.Trim()))
					{
						lblMessage.Text = Resources.Messages.UserDetails_ConfirmPasswordRequired;
						lblMessage.Type = MessageBox.MessageType.Error;
						return;
					}
					else if (pasNew != pasConfirm)
					{
						lblMessage.Text = Resources.Messages.UserDetails_PasswordsNotMatch;
						lblMessage.Type = MessageBox.MessageType.Error;
						return;
					}

					bool success = false;
					//Now make sure the new passwords match and aren't null.
					if (pasNew == pasConfirm)
					{
						try
						{
							success = provider.ChangePasswordAsAdmin(user.UserName, pasNew);
							if (!success)
							{
								//Display error message saying password was not changed.
								lblMessage.Text = Resources.Messages.Users_PasswordNotChanged;
								lblMessage.Type = MessageBox.MessageType.Error;
								return;
							}
						}
						catch (Exception ex)
						{
							lblMessage.Type = MessageBox.MessageType.Error;

							switch (ex.Message)
							{
								case "Password_too_short":
									{
										lblMessage.Text = string.Format(Resources.Messages.Users_PasswordError_TooShort, provider.MinRequiredPasswordLength);
									}
									break;

								case "Password_need_more_non_alpha_numeric_chars":
									{
										lblMessage.Text = string.Format(Resources.Messages.UserDetails_NewPasswordError6, provider.MinRequiredNonAlphanumericCharacters);
									}
									break;

								case "Password_does_not_match_regular_expression":
									{
										lblMessage.Text = Resources.Messages.Users_PasswordError_NoMatchRequired;
									}
									break;

								case "Membership_password_too_long":
									{
										lblMessage.Text = Resources.Messages.Users_PasswordError_TooLong;
									}
									break;

								case "Membership_Custom_Password_Validation_Failure":
									{
										lblMessage.Text = Resources.Messages.Users_PasswordError_NoMatchRequired;
									}
									break;

								default:
									{
										lblMessage.Text = Resources.Messages.Users_PasswordError_NoMatchRequired;
									}
									break;
							}
							return;
						}
					}
					else
					{
						//New passwords do not patch.
						lblMessage.Text = Resources.Messages.UserDetails_NewPasswordError2;
						this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
						return;
					}

					//Change the password question/answer
					string pasQuestion = unlinkLdap_NewQuestion.Text;
					string pasAnswer = unlinkLdap_NewAnswer.Text;

					if (String.IsNullOrWhiteSpace(pasQuestion))
					{
						lblMessage.Text = Resources.Messages.UserDetails_PasswordQuestionRequired;
						lblMessage.Type = MessageBox.MessageType.Error;
						return;
					}
					if (String.IsNullOrWhiteSpace(pasAnswer))
					{
						lblMessage.Text = Resources.Messages.UserDetails_PasswordAnswerRequired;
						lblMessage.Type = MessageBox.MessageType.Error;
						return;
					}

					if (pasQuestion != ((string.IsNullOrEmpty(user.PasswordQuestion)) ? "" : user.PasswordQuestion) && !string.IsNullOrWhiteSpace(pasQuestion) && !string.IsNullOrWhiteSpace(pasAnswer))
					{
						try
						{
							bool successQa = provider.ChangePasswordQuestionAndAnswerAsAdmin(user.UserName, pasQuestion.Trim(), pasAnswer.Trim());
							if (!successQa)
							{
								//Display the message if the change failed
								lblMessage.Text = Resources.Messages.Users_QuestionAnswerNotChanged;
								lblMessage.Type = MessageBox.MessageType.Error;
								return;
							}
						}
						catch (ProviderException)
						{
							//Display the message if the change failed
							lblMessage.Text = Resources.Messages.Users_QuestionAnswerNotChanged;
							lblMessage.Type = MessageBox.MessageType.Error;
							return;
						}
					}

					//Finally remove the LDAP DN
					user = provider.GetProviderUser(userId);
					user.StartTracking();
					user.LdapDn = null;
					provider.UpdateProviderUser(user);

					//Sign the specific user out of EVERYTHING.
					Global.KillUserSessions(user.UserId, true);
					Session.RemoveAll();
					Session.Abandon();

					//Redirect them back here
					Response.Redirect(Request.Url.ToString(), true);
				}
			}
			catch (Exception exception)
			{
				lblMessage.Text = exception.Message;
				lblMessage.Type = MessageBox.MessageType.Error;
			}
		}

		/// <summary>Clears the account's OAuth Login information.</summary>
		private void btnUnlinkOauth_Click(object sender, EventArgs e)
		{
			try
			{
				//Check that we have data in the needed fields.
				if (string.IsNullOrWhiteSpace(unlinkOauth_Password.Text))
				{
					lblMessage.Text = Resources.Messages.Admin_UserDetails_NewUsernamePasswordRequired;
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
							user.UserName,
							unlinkOauth_Password.Text.Trim(),
							unlinkOauth_NewQuestion.Text.Trim(),
							unlinkOauth_NewAnswer.Text.Trim());
					}

					//Sign the specific user out of EVERYTHING.
					Global.KillUserSessions(user.UserId, true);
					Session.RemoveAll();
					Session.Abandon();

					//Redirect them back here
					Response.Redirect(Request.Url.ToString(), true);
				}
			}
			catch (Exception exception)
			{
				lblMessage.Text = exception.Message;
				lblMessage.Type = MessageBox.MessageType.Error;
			}
		}

		/// <summary>Hit when the user tries to do an action on the TaraVault Project grid.</summary>
		/// <param name="sender">grdTaraProjects</param>
		/// <param name="e">GridViewCommandEventArgs</param>
		private void grdTaraProjects_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "grdTaraProjects_RowCommand()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Get the user id, the project id..
			int userId = -1;
			userId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);
			if (userId == -1)
				return;
			User user = new UserManager().GetUserById(userId);
			int projectId = int.Parse((string)e.CommandArgument);

			//Remove the project..
			new VaultManager().User_RemoveFromTaraVaultProject(userId, projectId);

			//Refresh data..
			LoadAndBindData_TaraVault(userId);

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Handles loading and binding for the TaraVault data.</summary>
		/// <param name="userId">The userId of the page.</param>
		private void LoadAndBindData_TaraVault(long userId)
		{
			//If TaraVault is selected..
			VaultManager vMgr = new VaultManager();
			User taraUser = vMgr.User_RetrieveWithTaraVault(userId);

			//Fill in the (potential) user name.
			lblTaraAccount.Text = string.Format(VaultManager.TARA_USER_ADDR_FORMAT, taraUser.UserName.Replace("@", "_"), ConfigurationSettings.Default.TaraVault_AccountName);

			//Now populate the other fields.
			if (taraUser.TaraVault != null)
			{
				//Show the project row..
				trTaraProjects.Visible = true;
				//Set data values.
				lblTaraAccount.Text = taraUser.TaraVault.VaultUserLogin;
				lblTaraAccountId.Text = taraUser.TaraVault.VaultUserId;
				string clearPassword = new SimpleAES().DecryptString(taraUser.TaraVault.Password);
				txtTaraPass.Text = GlobalFunctions.MaskPassword(clearPassword);
				chkTaraEnable.Checked = true;

				//Load up the projects that the user belongs to..
				List<DataModel.Project> projects = vMgr.User_RetrieveTaraProjectsForId(userId);
				grdTaraProjects.DataSource = projects;
				grdTaraProjects.DataBind();

				//If it's the admin, we want to prevent people disabling or removing from a project
				if (userId == UserManager.UserSystemAdministrator)
				{
					chkTaraEnable.Enabled = false;
					grdTaraProjects.Enabled = false;
				}
			}
			else
			{
				//Hide the project row..
				trTaraProjects.Visible = false;
			}
		}

		/// <summary>
		/// Updates the user details AND membership
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnMembershipUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!IsValid)
			{
				return;
			}

			//Make sure we have a user id
			if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
			{
				return;
			}
			int userId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);

			//Update the user details
			UpdateUserDetails();

			//Now update the membership details
			ProjectManager projectManager = new ProjectManager();
			List<ProjectUser> projectUsers = projectManager.RetrieveProjectMembershipForUser(userId);
			//Iterate through the grid and delete the appropriate items
			foreach (GridViewRow gvr in grdProjectMembership.Rows)
			{
				//Only consider data rows
				if (gvr.RowType == DataControlRowType.DataRow)
				{
					DropDownListEx dropDownList = (DropDownListEx)gvr.Cells[4].FindControl("ddlProjectRole");
					if (dropDownList != null)
					{
						int projectId = (int)grdProjectMembership.DataKeys[gvr.RowIndex].Value;
						int projectRoleId = int.Parse(dropDownList.SelectedValue);

						//Update the membership record
						ProjectUser projectUserRow = projectUsers.FirstOrDefault(p => p.ProjectId == projectId && p.UserId == userId);
						if (projectUserRow != null)
						{
							projectUserRow.StartTracking();
							projectUserRow.ProjectRoleId = projectRoleId;
						}
					}
				}
			}

			//Commit the changes
			projectManager.UpdateMembership(projectUsers);

			//Display confirmation message
			lblMessage.Text = "User Details and Project Membership Updated";
			lblMessage.Type = MessageBox.MessageType.Information;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Removes an existing project membership record
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnMembershipDelete_Click(object sender, EventArgs e)
		{
			//Make sure we have a user id
			if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
			{
				return;
			}
			int userId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);

			ProjectManager projectManager = new ProjectManager();
			bool deleteOccurred = false;
			//Iterate through the grid and delete the appropriate items
			foreach (GridViewRow gvr in grdProjectMembership.Rows)
			{
				//Only consider data rows
				if (gvr.RowType == DataControlRowType.DataRow)
				{
					CheckBoxEx checkBox = (CheckBoxEx)gvr.Cells[0].FindControl("chkDeleteMembership");
					if (checkBox != null && checkBox.Checked)
					{
						int projectId = (int)grdProjectMembership.DataKeys[gvr.RowIndex].Value;

						//Delete the membership
						projectManager.DeleteUserMembership(userId, projectId);
						deleteOccurred = true;
					}
				}
			}

			//Refresh the data
			if (deleteOccurred)
			{
				projectRoles = projectManager.RetrieveProjectRoles(true);
				List<ProjectUser> projectUsers = projectManager.RetrieveProjectMembershipForUser(userId);
				grdProjectMembership.DataSource = projectUsers.Where(f => f.Project != null);
				grdProjectMembership.DataBind();
			}
		}

		/// <summary>
		/// Redirects to the page that adds allows the adding of new projects
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnMembershipAdd_Click(object sender, EventArgs e)
		{
			//Make sure we have a user id
			if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
			{
				return;
			}
			int userId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);

			Response.Redirect("UserDetailsAddProjectMembership.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + userId, true);
		}

		/// <summary>
		/// Generates a new RSS token
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnGenerateNewToken_Click(object sender, EventArgs e)
		{
			txtRssToken.Text = GlobalFunctions.GenerateGuid();
		}

		/// <summary>
		/// Validates the form, and updates the user profile/password as appropriate
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		private void btnUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!IsValid)
			{
				return;
			}

			//Update the membership table so any changes to it are auto saved now
			btnMembershipUpdate_Click(sender, e);

			//Update the user details
			bool success = UpdateUserDetails();

			if (success)
			{
				Response.Redirect("UserList.aspx");
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Updates the user's details
		/// </summary>
		/// <remarks>True if successful</remarks>
		protected bool UpdateUserDetails()
		{
			//Instantiate our custom membership provider
			SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;

			//Retrieve the user from the ID held in the querysting
			int userId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);
			//User user = provider.GetProviderUser(userId);
			User user = new VaultManager().User_RetrieveWithTaraVault(userId);

			//Make the updates
			user.StartTracking();
			user.Profile.StartTracking();
			user.Profile.LastUpdateDate = DateTime.UtcNow;
			user.Profile.FirstName = txtFirstName.Text.Trim();
			if (txtMiddleInitial.Text == "")
				user.Profile.MiddleInitial = null;
			else
				user.Profile.MiddleInitial = txtMiddleInitial.Text.Trim();
			user.Profile.LastName = txtLastName.Text.Trim();
			user.UserName = txtUserName.Text.Trim();
			user.EmailAddress = txtEmailAddress.Text.Trim();
			if (string.IsNullOrWhiteSpace(txtDepartment.Text))
				user.Profile.Department = null;
			else
				user.Profile.Department = txtDepartment.Text.Trim();
			if (string.IsNullOrWhiteSpace(txtOrganization.Text))
				user.Profile.Organization = null;
			else
				user.Profile.Organization = txtOrganization.Text.Trim();
			user.Profile.IsEmailEnabled = (chkEmailEnabled.Checked);
			user.IsLocked = (chkLockedOut.Checked);
			if (string.IsNullOrWhiteSpace(txtRssToken.Text))
				user.RssToken = null;
			else
				user.RssToken = txtRssToken.Text.Trim();

			//Update user's TaraVault information.
			try
			{
				if (chkTaraEnable.Checked)
				{
					//Check that a password is given.
					if (string.IsNullOrWhiteSpace(txtTaraPass.Text))
					{
						lblMessage.Text = Resources.Messages.Admin_User_TaraVaultPassRequired;
						lblMessage.Type = MessageBox.MessageType.Error;
						return false;
					}

					//Create the user in the Vault, if mask changed
					if (!GlobalFunctions.IsMasked(txtTaraPass.Text.Trim()))
					{
						new VaultManager().User_CreateUpdateTaraAccount(user.UserId, user.UserName.Trim(), txtTaraPass.Text.Trim());
					}
				}
				else
				{
					//Remove the user.
					new VaultManager().User_RemoveTaraAccount(user.UserId);
				}
			}
			catch (Exception exception)
			{
				lblMessage.Text = exception.Message;
				lblMessage.Type = MessageBox.MessageType.Error;
				return false;
			}

			//See if we're setting the LDAP distinguished name field
			if (String.IsNullOrEmpty(txtLdapDn.Text))
			{
				user.LdapDn = null;
			}
			else
			{
				user.LdapDn = txtLdapDn.Text.Trim();
			}

            //See if we're changing the Porfolio Admin field
            user.Profile.IsPortfolioAdmin = this.chkPortfolioAdmin.Checked;
			user.Profile.IsReportAdmin = this.chkReportAdmin.Checked;

			//Make sure we aren't changing the active/admin status of the built-in admin
			if (user.Profile.IsAdmin != (chkAdminYn.Checked) || user.IsActive != (chkActiveYn.Checked))
			{
				if (userId == UserManager.UserSystemAdministrator)
				{
					lblMessage.Text = Resources.Messages.UserDetails_CannotChangeAdminUser1;
					return false;
				}
				else
				{
					user.Profile.IsAdmin = (chkAdminYn.Checked);
					user.IsActive = (chkActiveYn.Checked);
				}
			}
			try
			{
				provider.UpdateProviderUser(user);
			}
			catch (UserDuplicateUserNameException)
			{
				lblMessage.Text = Resources.Messages.UserDetails_UserNameInUse;
				lblMessage.Type = MessageBox.MessageType.Error;
				return false;
			}

			//Now we need to force the session to update this info if we are logged in as this user!
			if (userId == UserId)
			{
				Session.Abandon();
			}

			//Since the validators take care of the main form, we just need to ensure
			//that the passwords are entered and match
			string pasNew = txtNewPassword.Text;
			string pasConfirm = txtConfirmPassword.Text;

			if (!string.IsNullOrEmpty(pasNew.Trim()) && string.IsNullOrEmpty(pasConfirm.Trim()))
			{
				lblMessage.Text = Resources.Messages.UserDetails_ConfirmPasswordRequired;
				return false;
			}
			else if (pasNew != pasConfirm)
			{
				lblMessage.Text = Resources.Messages.UserDetails_PasswordsNotMatch;
				return false;
			}

			bool success = false;
			if (!string.IsNullOrEmpty(pasNew.Trim()))
			{

				//Now make sure the new passwords match and aren't null.
				if (pasNew == pasConfirm)
				{
					try
					{
						success = provider.ChangePasswordAsAdmin(user.UserName, txtNewPassword.Text);
						if (!success)
						{
							//Display error message saying password was not changed.
							lblMessage.Text = Resources.Messages.Users_PasswordNotChanged;
							lblMessage.Type = MessageBox.MessageType.Error;
							return false;
						}
					}
					catch (Exception ex)
					{
						lblMessage.Type = MessageBox.MessageType.Error;

						switch (ex.Message)
						{
							case "Password_too_short":
								{
									lblMessage.Text = string.Format(Resources.Messages.Users_PasswordError_TooShort, provider.MinRequiredPasswordLength);
								}
								break;

							case "Password_need_more_non_alpha_numeric_chars":
								{
									lblMessage.Text = string.Format(Resources.Messages.UserDetails_NewPasswordError6, provider.MinRequiredNonAlphanumericCharacters);
								}
								break;

							case "Password_does_not_match_regular_expression":
								{
									lblMessage.Text = Resources.Messages.Users_PasswordError_NoMatchRequired;
								}
								break;

							case "Membership_password_too_long":
								{
									lblMessage.Text = Resources.Messages.Users_PasswordError_TooLong;
								}
								break;

							case "Membership_Custom_Password_Validation_Failure":
								{
									lblMessage.Text = Resources.Messages.Users_PasswordError_NoMatchRequired;
								}
								break;

							default:
								{
									lblMessage.Text = Resources.Messages.Users_PasswordError_NoMatchRequired;
								}
								break;
						}
						return false;
					}
				}
				else
				{
					//New passwords do not patch.
					lblMessage.Text = Resources.Messages.UserDetails_NewPasswordError2;
					this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
					return false;
				}
			}

			//See if we need to change the password question/answer
			string pasQuestion = txtPasswordQuestion.Text;
			string pasAnswer = txtPasswordAnswer.Text;

			if (pasQuestion != ((string.IsNullOrEmpty(user.PasswordQuestion)) ? "" : user.PasswordQuestion) && !string.IsNullOrWhiteSpace(pasQuestion) && !string.IsNullOrWhiteSpace(pasAnswer))
			{
				try
				{
					bool successQa = provider.ChangePasswordQuestionAndAnswerAsAdmin(user.UserName, pasQuestion.Trim(), pasAnswer.Trim());
					if (!successQa)
					{
						//Display the message if the change failed
						lblMessage.Text = Resources.Messages.Users_QuestionAnswerNotChanged;
						lblMessage.Type = MessageBox.MessageType.Error;
						return false;
					}
				}
				catch (ProviderException)
				{
					//Display the message if the change failed
					lblMessage.Text = Resources.Messages.Users_QuestionAnswerNotChanged;
					lblMessage.Type = MessageBox.MessageType.Error;
					return false;
				}
			}


			//Finally we need to update any data-mapping entries (if necessary)
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<DataSyncUserMapping> userMappings = dataMappingManager.RetrieveDataSyncUserMappingsForUser(userId);
			//Now we need to iterate through the repeater changing the external key for each one
			foreach (RepeaterItem repeaterItem in rptDataSyncMappings.Items)
			{
				if (repeaterItem.ItemType == ListItemType.Item || repeaterItem.ItemType == ListItemType.AlternatingItem)
				{
					//Make sure we can access the text box
					if (repeaterItem.FindControl("txtExternalKey") != null)
					{
						TextBoxEx txtExternalKey = (TextBoxEx)repeaterItem.FindControl("txtExternalKey");
						int dataSyncSystemId = int.Parse(txtExternalKey.MetaData);
						//See if we have a matching row
						DataSyncUserMapping userMapping = userMappings.FirstOrDefault(d => d.DataSyncSystemId == dataSyncSystemId);
						if (userMapping == null)
						{
							if (txtExternalKey.Text.Trim() != "")
							{
								//Add the row
								DataSyncUserMapping newUserMapping = new DataSyncUserMapping();
								newUserMapping.MarkAsAdded();
								newUserMapping.DataSyncSystemId = dataSyncSystemId;
								newUserMapping.UserId = userId;
								newUserMapping.ExternalKey = txtExternalKey.Text.Trim();
								userMappings.Add(newUserMapping);
							}
						}
						else
						{
							//Update the row
							userMapping.StartTracking();
							if (txtExternalKey.Text.Trim() == "")
							{
								userMapping.ExternalKey = null;
							}
							else
							{
								userMapping.ExternalKey = txtExternalKey.Text.Trim();
							}
						}
					}
				}
			}

			//Now save the data
			dataMappingManager.SaveDataSyncUserMappings(userMappings);
			return true;
		}

		/// <summary>Redirects the user back to the administration home page when cancel clicked</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "btnCancel_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			Response.Redirect("UserList.aspx");

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion

		/// <summary>Hit when the user/admin wants to add projects to this user.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnAddTaraVault_Click(object sender, EventArgs e)
		{
			//make sure there's a userId..
			if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
			{
				string url = "~/Administration/UserDetailsAddTaraVault.aspx?" +
					GlobalFunctions.PARAMETER_USER_ID +
					"=" + Request.QueryString[GlobalFunctions.PARAMETER_USER_ID];

				Response.Redirect(url, true);
			}
		}
	}
}
