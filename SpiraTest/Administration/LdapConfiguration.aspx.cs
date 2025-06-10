using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration LDAP Configuration Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "LdapConfiguration_Title", "System-Users/#importing-ldap-users", "LdapConfiguration_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class LdapConfiguration : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.LdapConfiguration";

        /// <summary>
        /// The current product name
        /// </summary>
        protected string ProductName
        {
            get
            {
                return ConfigurationSettings.Default.License_ProductType;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {

                //Register event handlers
                this.btnLdapUpdate.Click += new EventHandler(btnLdapUpdate_Click);

                //Load the LDAP configuration
                if (!IsPostBack)
                {
                    LoadLdapConfiguration();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

        }

        /// <summary>
        /// This event handler handles changes to the LDAP Configuration
        /// </summary>
        /// <param name="sender">The object sending the event</param>
        /// <param name="e">The event handler arguments</param>
        private void btnLdapUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnLdapUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            //Instantiate the LDAP client
            Common.LdapClient ldapClient = new Common.LdapClient();
            ldapClient.LoadSettings();

            //Populate with the form values
            ldapClient.LdapHost = this.txtLdapHost.Text.Trim();
            ldapClient.LdapBaseDn = this.txtLdapBaseDn.Text.Trim();
            ldapClient.LdapBindDn = this.txtLdapBindDn.Text.Trim();
            //Make sure we don't update if the password is just the mask
            if (!GlobalFunctions.IsMasked(this.txtLdapBindPassword.Text.Trim()))
            {
                ldapClient.LdapBindPassword = this.txtLdapBindPassword.Text.Trim();
            }
            ldapClient.LdapLogin = this.txtLdapLogin.Text.Trim();
            ldapClient.LdapFirstName = this.txtLdapFirstName.Text.Trim();
            ldapClient.LdapLastName = this.txtLdapLastName.Text.Trim();
            ldapClient.LdapMiddleInitial = this.txtLdapMiddleInitial.Text.Trim();
            ldapClient.LdapEmailAddress = this.txtLdapEmailAddress.Text.Trim();
            ldapClient.UseSSL = this.chkLdapUseSSL.Checked;

            //If we have a sample user, try authenticating
            if (this.txtLdapSampleUser.Text.Trim() == "")
            {
                this.lblMessage.Text = Resources.Messages.LdapConfiguration_SettingsSavedWithoutTesting;
                this.lblMessage.Type = MessageBox.MessageType.Information;
            }
            else
            {
                bool success = ldapClient.Authenticate(this.txtLdapSampleUser.Text, this.txtLdapSamplePassword.Text);
                if (success)
                {
                    this.lblMessage.Text = Resources.Messages.LdapConfiguration_SettingsSaved;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }
                else
                {
                    this.lblMessage.Text = Resources.Messages.LdapConfiguration_FailedToAuthenticate;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                }
            }

            //Save the settings
            ldapClient.SaveSettings();

            //Now refresh the page
            LoadLdapConfiguration();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Loads the LDAP Configuration
        /// </summary>
        protected void LoadLdapConfiguration()
        {
            //Load in the LDAP configuration settings
            Common.LdapClient ldapClient = new Common.LdapClient();
            ldapClient.LoadSettings();

            //Set the form values
            this.txtLdapHost.Text = ldapClient.LdapHost;
            this.txtLdapBaseDn.Text = ldapClient.LdapBaseDn;
            this.txtLdapBindDn.Text = ldapClient.LdapBindDn;
            this.txtLdapBindPassword.Text = GlobalFunctions.MaskPassword(ldapClient.LdapBindPassword);
            this.txtLdapLogin.Text = ldapClient.LdapLogin;
            this.txtLdapFirstName.Text = ldapClient.LdapFirstName;
            this.txtLdapLastName.Text = ldapClient.LdapLastName;
            this.txtLdapMiddleInitial.Text = ldapClient.LdapMiddleInitial;
            this.txtLdapEmailAddress.Text = ldapClient.LdapEmailAddress;
            this.chkLdapUseSSL.Checked = ldapClient.UseSSL;
        }
    }
}