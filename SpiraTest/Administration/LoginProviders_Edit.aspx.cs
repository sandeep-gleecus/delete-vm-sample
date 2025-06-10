using Inflectra.OAuth2;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using System;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>This webform code-behind class is responsible to displaying the Administration User Details Page and handling all raised events</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_OAuthDetails_Title", "System-Users/#login-providers", "Admin_OAuthDetails_Title")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class LoginProviders_Edit : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.LoginProviders_Edit::";
		private Guid provId;

		#region Event Handlers
		/// <summary>This sets up the page upon loading</summary>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			btnUpdate.Click += btnUpdate_Click;
			btnCancel.Click += btnCancel_Click;

			//Get the Provider's ID.
			if (Guid.TryParse(Request.QueryString["id"].Trim(), out provId)) { }

			//Get the GUID and verify it's a given provider.
			GlobalOAuthProvider prov = new OAuthManager().Providers_RetrieveById(provId, true);

			if (prov != null)
			{
				//Disable the URL texts if the data isn't editable.
				txtUrlAuth.ReadOnly =
					txtUrlProf.ReadOnly =
					txtUrlTok.ReadOnly = !prov.IsUrlsEditable;

				//Only load the data once
				if (!IsPostBack)
				{
					//Populate the fields!
					txtClientId.Text = prov.ClientId;
					txtSecret.Text = prov.ClientSecret;
					txtUrlAuth.Text = prov.AuthorizationUrl;
					txtUrlProf.Text = prov.ProfileUrl;
					txtUrlTok.Text = prov.TokenUrl;
					ltlProviderName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(string.Format(Resources.Main.Admin_OAuthDetails_Subtitle, prov.Name));
					ltlDesc.Text = Microsoft.Security.Application.Encoder.HtmlEncode(prov.Description);
					chkActiveYn.Checked = prov.IsActive;
					numUsrs.Value = prov.Users.Count.ToSafeString();


					//Databind the form
					DataBind();
				}

                //Populate the Return URL for Admins to know what to use in configuration
                string baseUrl = ConfigurationSettings.Default.General_WebServerUrl;
                string url = "";
                if (!String.IsNullOrWhiteSpace(baseUrl))
                {
                    url = baseUrl + ((baseUrl.EndsWith("/")) ? "" : "/") + OAuth2Client.OAuthCallbackUrl;
                }

                //Keep the case
                txtUrlReturn.Text = url;

				//Display a warning if any provider is configured and UNloaded.
				if (!prov.IsLoaded)
				{
					if (prov.Users.Count > 0)
					{
						// There are Configured providers that are NOT loaded, and have Users assigned to them!
						lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderUnloadedUsers;
						lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
					}
					else
					{
						//There are configured providers that are not loaded, but there are no users for them.
						lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderUnloadedNoUser;
						lblMessage.Type = ServerControls.MessageBox.MessageType.Warning;
					}
				}
			}
			else
			{
				//Redirect. Not a valid provider.
				Response.Redirect("LoginProviders.aspx", true);
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Deletes the given provider, if possible.</summary>
		private void btnDelete_Click(object sender, EventArgs e)
		{
			//Pull the provider.
			OAuthManager oMgr = new OAuthManager();
			GlobalOAuthProvider prov = oMgr.Providers_RetrieveById(provId, true);

			if (prov != null)
			{
				//See if the provider is unloaded.
				if (!prov.IsLoaded)
				{
					//Verify the provider has no users.
					if (prov.Users.Count == 0)
					{
						bool ret = oMgr.Provider_Delete(provId);

						if (ret)
						{
							//Delete successful.
							lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderDeleted;
							lblMessage.Type = ServerControls.MessageBox.MessageType.Success;
						}
						else
						{
							//Error deleting.
							lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderErrorDelete;
							lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
						}
					}
					else
					{
						//Cannot delete a provider that has users assigned to it.
						lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderStillUsers;
						lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
					}
				}
				else
				{
					//Cannot delete a provider that is loaded into memory. 
					lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderStillLoaded;
					lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
				}
			}
			else
			{
				//Cannot delete a provider that doesn't exist.
				lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderDeleted;
				lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
			}
		}

		/// <summary>Validates the form, and updates the provider's information</summary>
		private void btnUpdate_Click(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "btnUpdate_Click()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Save the updated provider.
			OAuthManager oMgr = new OAuthManager();
			GlobalOAuthProvider provider = oMgr.Providers_RetrieveById(provId, true);

			//Check our validation logic.
			if (!chkActiveYn.Checked)
			{
				//User is, or left, the provider deactivated. We only throw an error in this case if
				//  the Provider WAS active, and has users assigned to it.
				if (provider.IsActive && provider.Users != null && provider.Users.Count > 0)
				{
					lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderStillUsers;
					lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
					return;
				}
			}
			else
			{
				//The provider is marked as active. Check the condition of our fields.

				//First, the Secret Key and Client Id is ALWAYS required.
				if (string.IsNullOrWhiteSpace(txtClientId.Text) || string.IsNullOrWhiteSpace(txtSecret.Text))
				{
					lblMessage.Text = Resources.Messages.Admin_OAuth_ClientIdSecretKeyRequired;
					lblMessage.Type = ServerControls.MessageBox.MessageType.Error;

					//See which one needs to have focus.
					if (string.IsNullOrWhiteSpace(txtSecret.Text))
						txtSecret.Focus();
					else
						txtClientId.Focus();

					return;
				}

				//Assuming those are entered, then we need to check that the URls are required.
				if (provider.IsUrlsEditable)
				{
					//Flag to keep track whether we're in error.
					bool urlsGood = true;

					//All three URLs are always required.
					Uri tstUri;
					if (string.IsNullOrWhiteSpace(txtUrlProf.Text) || !Uri.TryCreate(txtUrlProf.Text, UriKind.Absolute, out tstUri))
					{
						txtUrlProf.Focus();
						urlsGood = false;
					}
					if (string.IsNullOrWhiteSpace(txtUrlTok.Text) || !Uri.TryCreate(txtUrlTok.Text, UriKind.Absolute, out tstUri))
					{
						txtUrlProf.Focus();
						urlsGood = false;
					}
					if (string.IsNullOrWhiteSpace(txtUrlAuth.Text) || !Uri.TryCreate(txtUrlAuth.Text, UriKind.Absolute, out tstUri))
					{
						txtUrlProf.Focus();
						urlsGood = false;
					}

					if (!urlsGood)
					{
						lblMessage.Text = Resources.Messages.Admin_OAuth_URLsRequired;
						lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
						return;
					}
				}
			}

			//Set fields.
			provider.ClientId = txtClientId.Text.Trim();
			provider.ClientSecret = txtSecret.Text.Trim();
			provider.IsActive = chkActiveYn.Checked;

			if (provider.IsUrlsEditable)
			{
				//Token URL.
				try
				{
					Uri tkn = new Uri(txtUrlTok.Text);
					provider.TokenUrl = tkn.ToSafeString();
				}
				catch (Exception ex)
				{
					Logger.LogWarningEvent(METHOD_NAME, ex, "Could not translate Token URI.");
					lblMessage.Text = string.Format(
						Resources.Main.Global_CouldNotParse,
						Resources.Fields.TokenUrl);
					lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
				}

				//Authentication URL.
				try
				{
					Uri aut = new Uri(txtUrlAuth.Text);
					provider.AuthorizationUrl = aut.ToSafeString();
				}
				catch (Exception ex)
				{
					Logger.LogWarningEvent(METHOD_NAME, ex, "Could not translate Authorization URI.");
					lblMessage.Text = string.Format(
						Resources.Main.Global_CouldNotParse,
						Resources.Fields.AuthorizationUrl);
					lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
				}

				//Profile URL.
				try
				{
					Uri pro = new Uri(txtUrlProf.Text);
					provider.ProfileUrl = pro.ToSafeString();
				}
				catch (Exception ex)
				{
					Logger.LogWarningEvent(METHOD_NAME, ex, "Could not translate Profile URI.");
					lblMessage.Text = string.Format(
						Resources.Main.Global_CouldNotParse,
						Resources.Fields.ProfileUrl);
					lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
				}
			}

			if (provider.IsLogoEditable)
			{
				//At this time, we do nothing. No logo-editing available yet.
			}

			bool success = oMgr.Provider_Update(provider);

			if (success)
			{
				lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderInfoSaved;
				lblMessage.Type = ServerControls.MessageBox.MessageType.Success;
			}
			Logger.LogExitingEvent(METHOD_NAME);
			if (success) Response.Redirect("LoginProviders.aspx");
		}


		/// <summary>Redirects the user back to the administration home page when cancel clicked</summary>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			Response.Redirect("LoginProviders.aspx", true);
		}
		#endregion
	}
}
