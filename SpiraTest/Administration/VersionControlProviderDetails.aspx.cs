using System;
using System.Collections;
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

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Data Sync Details Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_VersionControlProvider_Title", "System-Integration/#version-control-integration-on-premise-customers-only", "Admin_VersionControlProvider_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class VersionControlProviderDetails : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.VersionControlProviderDetails::";

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
            this.lnkViewEvents.NavigateUrl = "EventLog.aspx?" + GlobalFunctions.PARAMETER_EVENT_CATEGORY + "=" + Logger.EVENT_CATEGORY_VERSION_CONTROL;

            //Get the version control provider from the querystring (if we have one)
            if (string.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_VERSION_CONTROL_SYSTEM_ID]))
            {
                //Denotes insert mode
                this.versionControlSystemId = -1;
                this.btnUpdate.Visible = false;
                this.btnInsert.Visible = true;
            }
            else
            {
                this.versionControlSystemId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_VERSION_CONTROL_SYSTEM_ID]);
                this.btnUpdate.Visible = true;
                this.btnInsert.Visible = false;
            }

            //Register the button event handlers
            this.btnInsert.Click += new EventHandler(btnInsert_Click);
            this.btnUpdate.Click += new EventHandler(btnUpdate_Click);
            this.btnTest.Click += new EventHandler(btnTest_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);

            //Only load the data once
            if (!IsPostBack)
            {
                //Set url of the link back to the version control home
                this.lnkVersionControlHome.NavigateUrl = "VersionControl.aspx";

                LoadAndBindData();
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
            const string METHOD_NAME = CLASS_NAME + "btnTest_Click()";

            try
            {
                //Generate our settings class..
                SourceCodeSettings settings = new SourceCodeSettings();
                settings.ProviderName = this.txtName.Text.Trim();
                settings.Connection = this.txtConnection.Text.Trim();
                settings.Custom01 = this.txtCustom01.Text.Trim();
                settings.Custom02 = this.txtCustom02.Text.Trim();
                settings.Custom03 = this.txtCustom03.Text.Trim();
                settings.Custom04 = this.txtCustom04.Text.Trim();
                settings.Custom05 = this.txtCustom05.Text.Trim();
                NetworkCredential credential = new NetworkCredential();
                credential.UserName = this.txtLogin.Text.Trim();
                credential.Domain = this.txtDomain.Text.Trim();
                //See if they entered a new password, otherwise use the saved one
                if (GlobalFunctions.IsMasked(this.txtPassword.Text.Trim()))
                {
                    this.versionControlSystemId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_VERSION_CONTROL_SYSTEM_ID]);
                    VersionControlSystem versionControlSystem = new SourceCodeManager().RetrieveSystemById(this.versionControlSystemId);
                    credential.Password = versionControlSystem.Password;
                }
                else
                {
                    credential.Password = this.txtPassword.Text.Trim();
                }                
                settings.Credentials = credential;
                bool success = new SourceCodeManager().TestConnection(settings);
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
                Logger.LogErrorEvent(METHOD_NAME, exception, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
                throw;
            }
        }

        /// <summary>
        /// Updates the version control provider
        /// </summary>
        protected void UpdateDetails()
        {
            const string METHOD_NAME = "UpdateDetails";

            try
            {
                //Retrieve the existing record and make the updates
                SourceCodeManager sourceCodeManager = new SourceCodeManager();
                try
                {
                    VersionControlSystem versionControlSystem = sourceCodeManager.RetrieveSystemById(this.versionControlSystemId);
                    versionControlSystem.StartTracking();
                    versionControlSystem.Name = this.txtName.Text.Trim();
                    versionControlSystem.Description = GlobalFunctions.HtmlScrubInput(this.txtDescription.Text.Trim());
                    versionControlSystem.ConnectionString = this.txtConnection.Text.Trim();
                    versionControlSystem.Login = this.txtLogin.Text.Trim();
                    //Make sure we don't update if the password is just the mask
                    if (!GlobalFunctions.IsMasked(this.txtPassword.Text.Trim()))
                    {
                        versionControlSystem.Password = this.txtPassword.Text.Trim();
                    }
                    versionControlSystem.IsActive = this.chkActive.Checked;
                    versionControlSystem.Domain = this.txtDomain.Text.Trim();
                    versionControlSystem.Custom01 = this.txtCustom01.Text.Trim();
                    versionControlSystem.Custom02 = this.txtCustom02.Text.Trim();
                    versionControlSystem.Custom03 = this.txtCustom03.Text.Trim();
                    versionControlSystem.Custom04 = this.txtCustom04.Text.Trim();
                    versionControlSystem.Custom05 = this.txtCustom05.Text.Trim();
                    sourceCodeManager.UpdateSystem(versionControlSystem);
                }
                catch (ArtifactNotExistsException)
                {
                    //Just redirect back to the version control home
                    Response.Redirect("VersionControl.aspx");
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

            //Display a success message
            this.lblMessage.Text = Resources.Messages.Admin_VersionControlProvider_Success;
            this.lblMessage.Type = MessageBox.MessageType.Information;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Inserts a new provider
        /// </summary>
        /// <returns>The id of the new provider</returns>
        protected int InsertProvider()
        {
            const string METHOD_NAME = "InsertProvider";

            try
            {
                //Now add the new version control provider handling NULLs correctly
                SourceCodeManager sourceCodeManager = new SourceCodeManager();
                string description = null;
                if (this.txtDescription.Text.Trim() != "")
                {
                    description = GlobalFunctions.HtmlScrubInput(this.txtDescription.Text.Trim());
                }
                string domain = null;
                if (this.txtDomain.Text.Trim() != "")
                {
                    domain = GlobalFunctions.HtmlScrubInput(this.txtDomain.Text.Trim());
                }

                //Custom configuration fields
                string custom01 = null;
                string custom02 = null;
                string custom03 = null;
                string custom04 = null;
                string custom05 = null;
                if (this.txtCustom01.Text.Trim() != "")
                {
                    custom01 = this.txtCustom01.Text.Trim();
                }
                if (this.txtCustom02.Text.Trim() != "")
                {
                    custom02 = this.txtCustom02.Text.Trim();
                }
                if (this.txtCustom03.Text.Trim() != "")
                {
                    custom03 = this.txtCustom03.Text.Trim();
                }
                if (this.txtCustom04.Text.Trim() != "")
                {
                    custom04 = this.txtCustom04.Text.Trim();
                }
                if (this.txtCustom05.Text.Trim() != "")
                {
                    custom05 = this.txtCustom05.Text.Trim();
                }

                return sourceCodeManager.InsertSystem(
                    this.txtName.Text.Trim(),
                    description,
                    this.chkActive.Checked,
                    this.txtConnection.Text.Trim(),
                    this.txtLogin.Text.Trim(),
                    this.txtPassword.Text.Trim(),
                    domain,
                    custom01,
                    custom02,
                    custom03,
                    custom04,
                    custom05
                    );
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Inserts the new version control provider into the system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnInsert_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnInsert_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure the page validated OK
            if (!Page.IsValid)
            {
                return;
            }

            //Actually do the insert
            this.versionControlSystemId = InsertProvider();

            //Reload the page
            Response.Redirect("VersionControlProviderDetails.aspx?" + GlobalFunctions.PARAMETER_VERSION_CONTROL_SYSTEM_ID + "=" + this.versionControlSystemId);

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

            Response.Redirect("VersionControl.aspx", true);

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
                // Databind the form so that the validators work
                this.DataBind();

                //See if we're inserting or updating
                if (this.versionControlSystemId == -1)
                {
                    this.lblProviderName.Text = Resources.Main.Admin_VersionControlProvider_NewProvider;
                    this.chkActive.Checked = true;
                }
                else
                {
                    //First we need to retrieve the provider record
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
                        this.btnUpdate.Enabled = false;
                        return;
                    }

                    //Populate the form, handling NULLs correctly
                    this.lblProviderName.Text = versionControlSystem.Name;
                    this.txtName.Text = versionControlSystem.Name;
                    this.txtDescription.Text = versionControlSystem.Description;
                    this.txtConnection.Text = versionControlSystem.ConnectionString;
                    this.chkActive.Checked = versionControlSystem.IsActive;

                    //Credentials
                    this.txtLogin.Text = versionControlSystem.Login;
                    this.txtPassword.Text = GlobalFunctions.MaskPassword(versionControlSystem.Password);
                    this.txtDomain.Text = versionControlSystem.Domain;

                    //Custom Configuration Parameters
                    this.txtCustom01.Text = versionControlSystem.Custom01;
                    this.txtCustom02.Text = versionControlSystem.Custom02;
                    this.txtCustom03.Text = versionControlSystem.Custom03;
                    this.txtCustom04.Text = versionControlSystem.Custom04;
                    this.txtCustom05.Text = versionControlSystem.Custom05;
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
    }
}
