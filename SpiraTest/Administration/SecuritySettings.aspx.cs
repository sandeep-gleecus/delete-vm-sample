using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.ServerControls;
using System.Globalization;
using System.IO;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the administration security settings (membership provider) configuration page
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "SecuritySettings_Title", "System/#security-settings", "SecuritySettings_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class SecuritySettings : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.SecuritySettings::";

        protected string productName = "";

        /// <summary>
        /// Called when the page is first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Load the page if not postback
            if (!Page.IsPostBack)
            {
                LoadAndBindData();
            }

            //Add event handlers
            this.btnCancel.Click += new EventHandler(btnCancel_Click);
            this.btnUpdate.Click += new EventHandler(btnUpdate_Click);

            //Set the licensed product name (used in several places)
            this.productName = ConfigurationSettings.Default.License_ProductType;
        }

        /// <summary>
        /// Returns to the administration home page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, EventArgs e)
        {
            //Redirect back to the administration home page
            Response.Redirect("Default.aspx");
        }

        /// <summary>
        /// Loads and displays the panel's contents when called
        /// </summary>
        protected void LoadAndBindData()
        {

            //Databind
            this.DataBind();

            //Populate the fields from the global configuration settings
            this.chkAllowUserRegistration.Checked = (ConfigurationSettings.Default.Membership_AllowUserRegistration ? true : false);
            this.txtMaxInvalidPasswordAttempts.Text = ConfigurationSettings.Default.Membership_MaxInvalidPasswordAttempts.ToString();
            this.txtMinRequiredNonalphanumericCharacters.Text = ConfigurationSettings.Default.Membership_MinRequiredNonalphanumericCharacters.ToString();
            this.txtMinRequiredPasswordLength.Text = ConfigurationSettings.Default.Membership_MinRequiredPasswordLength.ToString();
            this.txtPasswordAttemptWindow.Text = ConfigurationSettings.Default.Membership_PasswordAttemptWindow.ToString();
            this.txtAuthenticationExpiration.Text = ConfigurationSettings.Default.Authentication_Expiry.ToString();
            this.txtAuthenticationExpirationRememberMe.Text = ConfigurationSettings.Default.Authentication_ExpiryRememberMe.ToString();
            this.txtAllowedDomains.Text = ConfigurationSettings.Default.Api_AllowedCorsOrigins;
            this.txtPasswordChangeInterval.Text = ConfigurationSettings.Default.Security_PasswordChangeInterval.ToString();
            this.chkChangePasswordOnFirstLogin.Checked = ConfigurationSettings.Default.Security_ChangePasswordOnFirstLogin;
            this.chkPasswordContainNoNames.Checked = ConfigurationSettings.Default.Security_PasswordNoNames;
			this.chkEnableMfa.Checked = ConfigurationSettings.Default.Security_EnableMfa;
        }

        /// <summary>
        /// This event handler updates the stored security settings
        /// </summary>
        /// <param name="sender">The object sending the event</param>
        /// <param name="e">The event handler arguments</param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure all validators have succeeded
            if (!Page.IsValid)
            {
                return;
            }

            //Update the stored configuration values from the provided information
            ConfigurationSettings.Default.Membership_AllowUserRegistration = (this.chkAllowUserRegistration.Checked);
            ConfigurationSettings.Default.Membership_MaxInvalidPasswordAttempts = Int32.Parse(this.txtMaxInvalidPasswordAttempts.Text);
            ConfigurationSettings.Default.Membership_MinRequiredNonalphanumericCharacters = Int32.Parse(this.txtMinRequiredNonalphanumericCharacters.Text);
            ConfigurationSettings.Default.Membership_MinRequiredPasswordLength = Int32.Parse(this.txtMinRequiredPasswordLength.Text);
            ConfigurationSettings.Default.Membership_PasswordAttemptWindow = Int32.Parse(this.txtPasswordAttemptWindow.Text);
            ConfigurationSettings.Default.Authentication_Expiry = Int32.Parse(this.txtAuthenticationExpiration.Text);
            ConfigurationSettings.Default.Authentication_ExpiryRememberMe = Int32.Parse(this.txtAuthenticationExpirationRememberMe.Text);
            ConfigurationSettings.Default.Api_AllowedCorsOrigins = this.txtAllowedDomains.Text.Trim();
            ConfigurationSettings.Default.Security_PasswordChangeInterval = 0;
            if (!String.IsNullOrEmpty(this.txtPasswordChangeInterval.Text))
            {
                ConfigurationSettings.Default.Security_PasswordChangeInterval = Int32.Parse(this.txtPasswordChangeInterval.Text);
            }
            ConfigurationSettings.Default.Security_ChangePasswordOnFirstLogin = this.chkChangePasswordOnFirstLogin.Checked;
            ConfigurationSettings.Default.Security_PasswordNoNames = this.chkPasswordContainNoNames.Checked;
			ConfigurationSettings.Default.Security_EnableMfa = this.chkEnableMfa.Checked;
			ConfigurationSettings.Default.Save();

            //Let the user know that the settings were saved
            this.lblMessage.Text = Resources.Messages.SecuritySettings_SaveSuccess;
            this.lblMessage.Type = MessageBox.MessageType.Information;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
    }
}
