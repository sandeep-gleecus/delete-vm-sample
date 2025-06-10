using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Web.UI;

namespace Inflectra.SpiraTest.Web.Administration
{
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_Notification_EMailServerSettings", "System/#email-configuration", "Admin_Notification_EMailServerSettings")]
	public partial class EmailConfiguration : AdministrationBase
	{
		internal string productName = "";

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.EmailConfiguration::";

		#region Event Handlers

		/// <summary>Handles initialization that needs to come before Page_Onload is fired</summary>
		/// <param name="e">EventArgs</param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
		}

		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			productName = ConfigurationSettings.Default.License_ProductType;

			//Attach to controls.
			btnEmailUpdate.Click += new EventHandler(btnEmailUpdate_Click);
			valEmailFormat.ValidationExpression = GlobalFunctions.VALIDATION_REGEX_EMAIL_ADDRESS;
			valEmailReplyFormat.ValidationExpression = GlobalFunctions.VALIDATION_REGEX_EMAIL_ADDRESS;
			valServerPort.ValidationExpression = GlobalFunctions.VALIDATION_REGEX_INTEGER;

			//If this is a hosted site, don't allow them to update the mail server settings
			txtEmailHostName.ReadOnly =
				txtEmailPortNumber.ReadOnly =
				txtEmailUserName.ReadOnly =
				txtEmailPassword.ReadOnly =
				txtEmailFromAddress.ReadOnly = !Common.Properties.Settings.Default.LicenseEditable;
			chkUseSSL.Enabled =
				chkFromUser.Enabled = Common.Properties.Settings.Default.LicenseEditable;

			if (!Page.IsPostBack)
			{
				//Populate the fields from the global configuration settings
				txtEmailFromAddress.Text = ConfigurationSettings.Default.EmailSettings_EMailFrom;
				txtEmailReplyAddress.Text = ConfigurationSettings.Default.EmailSettings_EMailReplyTo;
				txtEmailHostName.Text = ConfigurationSettings.Default.EmailSettings_MailServer;
				txtEmailPortNumber.Text = ConfigurationSettings.Default.EmailSettings_MailServerPort.ToString();
				txtEmailUserName.Text = ConfigurationSettings.Default.EmailSettings_SmtpUser;
				txtEmailPassword.Text = GlobalFunctions.MaskPassword(SecureSettings.Default.EmailSettings_SmtpPassword);
				chkActiveYn.Checked = ((ConfigurationSettings.Default.EmailSettings_Enabled) ? true : false);
				chkUseSSL.Checked = ((ConfigurationSettings.Default.EmailSettings_UseSSL) ? true : false);
				chkSendHTML.Checked = ((ConfigurationSettings.Default.EmailSettings_SendInHTML) ? true : false);
				chkAllowUser.Checked = ((ConfigurationSettings.Default.EmailSettings_AllowUserControl) ? true : false);
				chkSendSeperator.Checked = ((ConfigurationSettings.Default.EmailSettings_SendSeperator) ? true : false);
				chkFromUser.Checked = ((ConfigurationSettings.Default.EmailSettings_SendFromUser) ? true : false);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		void btnEmailUpdate_Click(object sender, EventArgs e)
		{
			//Save specified settings.
			if (IsValid)
			{
				//Update the stored configuration values from the provided information
				ConfigurationSettings.Default.EmailSettings_MailServer = txtEmailHostName.Text.Trim();
				if (String.IsNullOrWhiteSpace(txtEmailPortNumber.Text))
				{
					//Set to the default port (25)
					ConfigurationSettings.Default.EmailSettings_MailServerPort = 25;
				}
				else
				{
					if (txtEmailPortNumber.Text.IsInteger())
					{
						ConfigurationSettings.Default.EmailSettings_MailServerPort = Int32.Parse(txtEmailPortNumber.Text.Trim());
					}
				}
				ConfigurationSettings.Default.EmailSettings_SmtpUser = txtEmailUserName.Text.Trim();
				ConfigurationSettings.Default.EmailSettings_Enabled = (chkActiveYn.Checked);
				ConfigurationSettings.Default.EmailSettings_UseSSL = (chkUseSSL.Checked);
				ConfigurationSettings.Default.EmailSettings_EMailFrom = txtEmailFromAddress.Text.Trim();
				ConfigurationSettings.Default.EmailSettings_SendInHTML = (chkSendHTML.Checked);
				ConfigurationSettings.Default.EmailSettings_EMailReplyTo = txtEmailReplyAddress.Text.Trim();
				ConfigurationSettings.Default.EmailSettings_AllowUserControl = (chkAllowUser.Checked);
				ConfigurationSettings.Default.EmailSettings_SendSeperator = (chkSendSeperator.Checked);
				ConfigurationSettings.Default.EmailSettings_SendFromUser = (chkFromUser.Checked);

				//Clean settings for hosted peeps. See IN:5302
				if (!Common.Properties.Settings.Default.LicenseEditable)
				{
					ConfigurationSettings.Default.EmailSettings_SendFromUser = false;
					ConfigurationSettings.Default.EmailSettings_EMailFrom = "donotreply@spiraservice.net";
					ConfigurationSettings.Default.EmailSettings_UseSSL = false;
				}

				//Save the settings.
				ConfigurationSettings.Default.Save();

				if (!GlobalFunctions.IsMasked(txtEmailPassword.Text.Trim()))
				{
					SecureSettings.Default.EmailSettings_SmtpPassword = txtEmailPassword.Text;
					SecureSettings.Default.Save();
				}

				lblMessage.Text = Resources.Messages.Admin_EmailConfiguration_Success;
				lblMessage.Type = MessageBox.MessageType.Information;
			}
		}
		#endregion
	}
}
