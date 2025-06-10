using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using System.Web.Security;
using System.Configuration.Provider;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>This webform code-behind class is responsible to displaying the Administration User Details Page and handling all raised events</summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "UserDetails_AddUser", "System-Users/#view-edit-users", "UserDetails_AddUser")]
    [AdministrationLevelAttribute(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
    public partial class UserAdd : Inflectra.SpiraTest.Web.Administration.AdministrationBase
    {
        protected SortedList<string, string> flagList;
        protected List<ProjectRole> projectRoles;
        protected List<ProjectView> projects;

        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.UserAdd::";

        #region Event Handlers

        /// <summary>This sets up the page upon loading</summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            const string METHOD_NAME = "Page_Load()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the error message
            this.lblMessage.Text = "";

            //Add the event handlers to the page
            this.btnCancel.Click += this.btnCancel_Click;
            this.btnInsert.Click += this.btnInsert_Click;
            this.btnGenerateNewToken.Click += this.btnGenerateNewToken_Click;

            //Only load the data once
            if (!IsPostBack)
            {
                //Get a member to our custom membership provider
                SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;

                //Default to RSS disabled for security
                this.chkRssEnabled.Checked = false;
                this.txtRssToken.Text = "";
                //this.txtRssToken.Text = GlobalFunctions.GenerateGuid();

                //Retrieve any lookups
                Business.ProjectManager projectManager = new Business.ProjectManager();
                this.flagList = projectManager.RetrieveFlagLookup();
                this.projectRoles = projectManager.RetrieveProjectRoles(true);
                this.projects = projectManager.Retrieve();

                //Set the datasources
                this.ddlProjectRole.DataSource = projectRoles;
                this.ddlProjectMultiSelect.DataSource = projects;

                //Databind the form
                this.DataBind();

                //Default to admin=no and email=yes
                this.chkEmailEnabled.Checked = true;
                this.chkRssEnabled.Checked = false;
                this.chkAdminYn.Checked = false;
                this.chkPortfolioAdmin.Checked = false;

                //Show portfolio field if allowed
                if (Common.Global.Feature_Portfolios)
                {
                    this.portfolioFormGroup.Visible = true;
                    chkPortfolioAdmin.Checked = false;
                }

                //Lock the email, if necessary.
                if (ConfigurationSettings.Default.EmailSettings_Enabled)
                {
                    this.chkEmailEnabled.Checked = true;
                    if (!ConfigurationSettings.Default.EmailSettings_AllowUserControl)
                        this.chkEmailEnabled.Enabled = false;
                }
                else
                {
                    this.chkEmailEnabled.Checked = false;
                    this.chkEmailEnabled.Enabled = false;
                }

            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>
        /// Generates a new RSS token
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGenerateNewToken_Click(object sender, EventArgs e)
        {
            this.txtRssToken.Text = GlobalFunctions.GenerateGuid();
        }

        /// <summary>
        /// Validates the form, and inserts the new user with password
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        private void btnInsert_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnInsert_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            //Instantiate the custom membership provider
            SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;

            //Since the validators take care of the main form, we just need to ensure
            //that the passwords are entered and match
            if (String.IsNullOrWhiteSpace(this.txtNewPassword.Text))
            {
                this.lblMessage.Text = Resources.Messages.UserDetails_NewPasswordRequired;
            }
            else if (String.IsNullOrWhiteSpace(this.txtConfirmPassword.Text))
            {
                this.lblMessage.Text = Resources.Messages.UserDetails_ConfirmPasswordRequired;
            }
            else if (this.txtNewPassword.Text != this.txtConfirmPassword.Text)
            {
                this.lblMessage.Text = Resources.Messages.UserDetails_PasswordsNotMatch;
            }
            else if (String.IsNullOrWhiteSpace(this.txtPasswordQuestion.Text))
            {
                this.lblMessage.Text = Resources.Messages.UserDetails_PasswordQuestionRequired;
            }
            else if (String.IsNullOrWhiteSpace(this.txtPasswordAnswer.Text))
            {
                this.lblMessage.Text = Resources.Messages.UserDetails_PasswordAnswerRequired;
            }
            else
            {
                //First create the user
                MembershipCreateStatus status;
                MembershipUser user = provider.CreateUser(
                    this.txtUserName.Text.Trim(),
                    this.txtNewPassword.Text.Trim(),
                    this.txtEmailAddress.Text.Trim(),
                    this.txtPasswordQuestion.Text.Trim(),
                    this.txtPasswordAnswer.Text.Trim(),
                    true,
                    null,
                    this.txtRssToken.Text.Trim(),
                    out status
                    );

                //Check the status
                switch (status)
                {
                    case MembershipCreateStatus.Success:
                        break;

                    case MembershipCreateStatus.DuplicateUserName:
                        this.lblMessage.Text = Resources.Messages.UserDetails_UserNameInUse;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;

                    case MembershipCreateStatus.ProviderError:
                        this.lblMessage.Text = Resources.Messages.UserDetails_ProviderError;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;

                    case MembershipCreateStatus.InvalidAnswer:
                        this.lblMessage.Text = Resources.Messages.UserDetails_InvalidPasswordAnswer;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;

                    case MembershipCreateStatus.InvalidEmail:
                        this.lblMessage.Text = Resources.Messages.UserDetails_InvalidEmailAddress;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;

                    case MembershipCreateStatus.InvalidPassword:
                        this.lblMessage.Text = Resources.Messages.UserDetails_InvalidPassword;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;

                    case MembershipCreateStatus.InvalidQuestion:
                        this.lblMessage.Text = Resources.Messages.UserDetails_InvalidPasswordQuestion;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;

                    case MembershipCreateStatus.InvalidUserName:
                        this.lblMessage.Text = Resources.Messages.UserDetails_InvalidUserName;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;
                }

                //Set the user profile
                ProfileEx profile = new ProfileEx(user.UserName);
                profile.FirstName = this.txtFirstName.Text.Trim();
                profile.MiddleInitial = this.txtMiddleInitial.Text.Trim();
                profile.LastName = this.txtLastName.Text.Trim();
                profile.Department = this.txtDepartment.Text.Trim();
                profile.Organization = this.txtOrganization.Text.Trim();
                profile.IsAdmin = this.chkAdminYn.Checked;
                profile.IsEmailEnabled = this.chkEmailEnabled.Checked;
                profile.IsPortfolioAdmin = this.chkPortfolioAdmin.Checked;
				profile.IsReportAdmin = this.chkReportAdmin.Checked;
                profile.Save();

                //Add the project(s) to user membership
                int userId = (int)user.ProviderUserKey;

                ProjectManager projectManager = new ProjectManager();
                if (!String.IsNullOrEmpty(this.ddlProjectMultiSelect.SelectedValue) && !String.IsNullOrEmpty(this.ddlProjectRole.SelectedValue)) 
                {
                    List<uint> projectIds = this.ddlProjectMultiSelect.SelectedValues();
                    int projectRoleId;
                    if (Int32.TryParse(this.ddlProjectRole.SelectedValue, out projectRoleId))
                    {
                        foreach (uint projectId in projectIds)
                        {
                            //Add the membership record
                            projectManager.InsertUserMembership(userId, (int)projectId, projectRoleId);
                        }
                    }
                }

                //Return to the admin home page
                Response.Redirect("UserList.aspx");
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
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
    }
}
