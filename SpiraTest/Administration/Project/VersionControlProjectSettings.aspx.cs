using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using System.Net;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Data Sync Details Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_VersionControlProject_LongTitle", "Product-General-Settings/#source-code", "Admin_VersionControlProject_LongTitle"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class VersionControlProjectSettings : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.VersionControlProjectSettings::";

        protected int versionControlSystemId;

        /// <summary>
        /// Sets up the page when first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the error message
            this.lblMessage.Text = "";

            //Add the link to the error log
            this.lnkViewEvents.NavigateUrl = "~/Administration/EventLog.aspx?" + GlobalFunctions.PARAMETER_EVENT_CATEGORY + "=" + Logger.EVENT_CATEGORY_VERSION_CONTROL;

            //Get the user id and project id from session
            if (ProjectId < 1)
            {
                this.lblMessage.Text = Resources.Messages.Admin_VersionControlProject_NeedToSelectProject;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                this.lblProjectName.Text = Resources.Main.Admin_VersionControlProject_Unselected;
                ShowHideProjectSettings(false);

                //Also hide the active flag and project name
                this.divSettings.Visible = false;

                return;
            }
            this.divSettings.Visible = true;

            //Get the version control provider from the querystring, if none provided, use the first active one for this project
            if (String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_VERSION_CONTROL_SYSTEM_ID]))
            {
                SourceCodeManager sourceCodeManager = new SourceCodeManager();
                List<VersionControlProject> versionControlProjects = sourceCodeManager.RetrieveProjectSettings(ProjectId);
                if (versionControlProjects.Count < 1)
                {
                    //See if we're a system admin or not (determines where we can redirect to)
                    if (UserIsAdmin)
                    {
                        //Version Control Home
                        Response.Redirect("~/Administration/VersionControl.aspx");
                    }
                    else
                    {
                        //Project Admin Home
                        Response.Redirect("Default.aspx" + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_VersionControlProvider_NotActiveForProduct, true);
                    }
                }
                else
                {
                    this.versionControlSystemId = versionControlProjects[0].VersionControlSystemId;
                }
            }
            else
            {
                this.versionControlSystemId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_VERSION_CONTROL_SYSTEM_ID]);
            }

            //Register the button event handlers
            this.btnUpdate.Click += new EventHandler(btnUpdate_Click);
            this.btnTest.Click += new EventHandler(btnTest_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);

            //Only load the data once
            if (!IsPostBack)
            {
                //Set the project name labels
                this.lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
                this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");
                this.lblProjectName2.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);

                LoadAndBindData();
            }

            //Set the correct back link
            //See if we're a system admin or not (determines where we can redirect to)
            if (UserIsAdmin)
            {
                //Version Control Home
                this.lnkVersionControlHome.NavigateUrl = "~/Administration/VersionControl.aspx";
            }
            else
            {
                //Project Admin Home
                this.lnkVersionControlHome.NavigateUrl = "Default.aspx";
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Tests the version control provider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnTest_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnTest_Click";

            try
            {
                //First we need to retrieve the provider record itself
                SourceCodeManager sourceCodeManager = new SourceCodeManager();
                VersionControlSystem versionControlSystem = null;
                try
                {
                    versionControlSystem = sourceCodeManager.RetrieveSystemById(this.versionControlSystemId);
                }
                catch (ArtifactNotExistsException)
                {
                    this.lblMessage.Text = Resources.Messages.Admin_VersionControlProvider_HasBeenDeleted;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    ShowHideProjectSettings(false);
                    return;
                }

                //Create our settings..
                SourceCodeSettings settings = new SourceCodeSettings();
                NetworkCredential credential = new NetworkCredential();

                //Use the default settings unless overriden by the project settings
                settings.ProviderName = versionControlSystem.Name;
                settings.Connection = versionControlSystem.ConnectionString;
                settings.Custom01 = String.IsNullOrWhiteSpace(this.txtCustom01.Text) ? ((String.IsNullOrEmpty(versionControlSystem.Custom01)) ? "" : versionControlSystem.Custom01) : this.txtCustom01.Text.Trim();
                settings.Custom02 = String.IsNullOrWhiteSpace(this.txtCustom02.Text) ? ((String.IsNullOrEmpty(versionControlSystem.Custom02)) ? "" : versionControlSystem.Custom02) : this.txtCustom02.Text.Trim();
                settings.Custom03 = String.IsNullOrWhiteSpace(this.txtCustom03.Text) ? ((String.IsNullOrEmpty(versionControlSystem.Custom03)) ? "" : versionControlSystem.Custom03) : this.txtCustom03.Text.Trim();
                settings.Custom04 = String.IsNullOrWhiteSpace(this.txtCustom04.Text) ? ((String.IsNullOrEmpty(versionControlSystem.Custom04)) ? "" : versionControlSystem.Custom04) : this.txtCustom04.Text.Trim();
                settings.Custom05 = String.IsNullOrWhiteSpace(this.txtCustom05.Text) ? ((String.IsNullOrEmpty(versionControlSystem.Custom05)) ? "" : versionControlSystem.Custom05) : this.txtCustom05.Text.Trim();
                credential.UserName = String.IsNullOrWhiteSpace(this.txtLogin.Text) ? versionControlSystem.Login : this.txtLogin.Text.Trim();
                //If masked, use the saved project setting password
                if (GlobalFunctions.IsMasked(this.txtPassword.Text))
                {
                    credential.Password = sourceCodeManager.RetrieveProjectSettings(versionControlSystemId, ProjectId).Password;
                }
                else
                {
                    //Otherwise use the entered password or the system password if no project password entered
                    credential.Password = String.IsNullOrWhiteSpace(this.txtPassword.Text) ? versionControlSystem.Password : this.txtPassword.Text.Trim();
                }
                credential.Domain = String.IsNullOrWhiteSpace(this.txtDomain.Text) ? ((String.IsNullOrEmpty(versionControlSystem.Domain)) ? "" : versionControlSystem.Domain) : this.txtDomain.Text.Trim();
                settings.Credentials = credential;

                bool success = sourceCodeManager.TestConnection(settings);
                if (success)
                {
                    this.lblMessage.Text = Resources.Messages.VersionControlProviderDetails_TestSuccess;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }
                else
                {
                    this.lblMessage.Text = Resources.Messages.VersionControlProviderDetails_TestFailed;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    this.plcViewEvents.Visible = true;
                }
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Shows or hides the part of the table containing the project settings
        /// </summary>
        /// <param name="show">Show we show the settings rows?</param>
        protected void ShowHideProjectSettings(bool show)
        {
            this.pnlProjectSettings.Visible = show;
        }

        /// <summary>
        /// Updates the version control provider
        /// </summary>
        protected void UpdateDetails()
        {
            const string METHOD_NAME = "UpdateDetails";
            try
            {
                //Retrieve the existing record and make the updates or insert if not already there
                SourceCodeManager sourceCodeManager = new SourceCodeManager();
                VersionControlProject versionControlProject = sourceCodeManager.RetrieveProjectSettings(this.versionControlSystemId, this.ProjectId);
                if (versionControlProject == null)
                {
                    //Only insert if active flag = Yes
                    if (this.chkActive.Checked)
                    {
                        sourceCodeManager.InsertProjectSettings(
                            this.versionControlSystemId,
                            this.ProjectId,
                            true,
                            this.txtConnection.Text.Trim(),
                            this.txtLogin.Text.Trim(),
                            this.txtPassword.Text.Trim(),
                            this.txtDomain.Text.Trim(),
                            this.txtCustom01.Text.Trim(),
                            this.txtCustom02.Text.Trim(),
                            this.txtCustom03.Text.Trim(),
                            this.txtCustom04.Text.Trim(),
                            this.txtCustom05.Text.Trim()
                            );
                    }
                }
                else
                {
                    versionControlProject.StartTracking();
                    versionControlProject.IsActive = (this.chkActive.Checked);
                    versionControlProject.ConnectionString = this.txtConnection.Text.Trim();
                    versionControlProject.Login = this.txtLogin.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(this.txtPassword.Text) && !GlobalFunctions.IsMasked(this.txtPassword.Text.Trim()))
                        versionControlProject.Password = this.txtPassword.Text.Trim();
                    versionControlProject.Domain = this.txtDomain.Text.Trim();
                    versionControlProject.Custom01 = this.txtCustom01.Text.Trim();
                    versionControlProject.Custom02 = this.txtCustom02.Text.Trim();
                    versionControlProject.Custom03 = this.txtCustom03.Text.Trim();
                    versionControlProject.Custom04 = this.txtCustom04.Text.Trim();
                    versionControlProject.Custom05 = this.txtCustom05.Text.Trim();
                    sourceCodeManager.UpdateProjectSettings(versionControlProject);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                //Ignore since it's caused by the redirect
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Updates the changes to the version control provider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure the page validated OK
            if (!Page.IsValid)
            {
                return;
            }

            //Actually perform the update
            UpdateDetails();

            //Load and bind the page
            LoadAndBindData();

            //Display a success message
            this.lblMessage.Text = Resources.Messages.Admin_VersionControlProject_Success;
            this.lblMessage.Type = MessageBox.MessageType.Information;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Redirects the user back to the parent page when cancel clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnCancel_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //See if we're a system admin or not (determines where we can redirect to)
            if (UserIsAdmin)
            {
                //Version Control Home
                Response.Redirect("~/Administration/VersionControl.aspx", true);
            }
            else
            {
                //Project Admin Home
                Response.Redirect("Default.aspx", true);
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }


        /// <summary>
        /// Loads the version control provider information
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First we need to retrieve the provider record itself
                SourceCodeManager sourceCodeManager = new SourceCodeManager();
                VersionControlSystem versionControlSystem = null;
                try
                {
                    versionControlSystem = sourceCodeManager.RetrieveSystemById(this.versionControlSystemId);
                }
                catch (ArtifactNotExistsException)
                {
                    this.lblMessage.Text = Resources.Messages.Admin_VersionControlProvider_HasBeenDeleted;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    ShowHideProjectSettings(false);
                    return;
                }
                //Populate the provider name
                this.lblProviderName.Text = versionControlSystem.Name;
                this.lblProviderName2.Text = versionControlSystem.Name;

                //Display if the cache is running or not, or if never initiatized
                if (SourceCodeManager.IsCacheUpdateRunning)
                {
                    this.msgStatus.Text = Resources.Messages.VersionControl_CacheUpdateRunning;
                    this.msgStatus.Type = ServerControls.MessageBox.MessageType.Error;
                }
                else
                {
                    if (SourceCodeManager.IsInitialized(ProjectId))
                    {
                        this.msgStatus.Text = Resources.Messages.VersionControl_CacheUpToDate;
                        this.msgStatus.Type = ServerControls.MessageBox.MessageType.Information;
                    }
                    else
                    {
                        this.msgStatus.Text = Resources.Messages.VersionControl_CacheNotInitialized;
                        this.msgStatus.Type = ServerControls.MessageBox.MessageType.Warning;
                    }
                }

                //Now we need to see if we have an actual project record for this provider
                VersionControlProject versionControlProject = sourceCodeManager.RetrieveProjectSettings(this.versionControlSystemId, this.ProjectId);
                if (versionControlProject != null)
                {
                    //See if version control is active for this project
                    if (versionControlProject.IsActive)
                    {
                        //Show the whole form
                        ShowHideProjectSettings(true);

                        //Populate the form, handling NULLs correctly
                        this.chkActive.Checked = true;
                        this.txtConnection.Text = versionControlProject.ConnectionString;
                        this.txtLogin.Text = versionControlProject.Login;
                        if (String.IsNullOrEmpty(versionControlProject.Password))
                        {
                            this.txtPassword.Text = "";
                        }
                        else
                        {
                            this.txtPassword.Text = GlobalFunctions.MaskPassword(versionControlProject.Password);
                        }
                        this.txtDomain.Text = versionControlProject.Domain;
                        this.txtCustom01.Text = versionControlProject.Custom01;
                        this.txtCustom02.Text = versionControlProject.Custom02;
                        this.txtCustom03.Text = versionControlProject.Custom03;
                        this.txtCustom04.Text = versionControlProject.Custom04;
                        this.txtCustom05.Text = versionControlProject.Custom05;

                        //Enable cache clear/delete
                        this.btnClearCache.Visible = true;
                        this.btnDeleteCache.Visible = true;
                    }
                    else
                    {
                        //Just show the active flag and the first update button only
                        ShowHideProjectSettings(false);
                        this.chkActive.Checked = false;

                        //Disable cache clear/delete
                        this.btnClearCache.Visible = false;
                        this.btnDeleteCache.Visible = false;
                    }
                }
                else
                {
                    //Just show the active flag and the first update button only
                    ShowHideProjectSettings(false);
                    this.chkActive.Checked = false;

                    //Disable cache clear/delete
                    this.btnClearCache.Visible = false;
                    this.btnDeleteCache.Visible = false;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>Hit when the user wants to refresh the cache..</summary>
        /// <param name="sender">btnClearCache</param>
        /// <param name="e">EventArgs</param>
        protected void btnClearCache_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnClearCache_Click";
            try
            {
                //Create instance..
                SourceCodeManager mgr = new SourceCodeManager(this.ProjectId);

                //Force reload..
                mgr.LaunchCacheRefresh();

                this.lblMessage.Text = Resources.Messages.Admin_VersionControl_ReloadingCache;
                this.lblMessage.Type = MessageBox.MessageType.Information;

                //Avoid duplicate messages
                this.msgStatus.Text = "";
                this.msgStatus.Type = ServerControls.MessageBox.MessageType.Information;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                this.lblMessage.Text = exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
        }

        /// <summary>Hit when the user wants to delete the cache..</summary>
        /// <param name="sender">btnDeleteCache</param>
        /// <param name="e">EventArgs</param>
        protected void btnDeleteCache_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnDeleteCache_Click";
            try
            {
                //Create instance..
                SourceCodeManager mgr = new SourceCodeManager(this.ProjectId, true);

                //Force reload..
                mgr.ClearCacheAndRefresh();

                this.lblMessage.Text = Resources.Messages.Admin_VersionControl_ReloadingCache;
                this.lblMessage.Type = MessageBox.MessageType.Information;

                //Avoid duplicate messages
                this.msgStatus.Text = "";
                this.msgStatus.Type = ServerControls.MessageBox.MessageType.Information;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                this.lblMessage.Text = exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
        }
    }
}
