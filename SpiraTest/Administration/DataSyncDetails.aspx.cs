using System;
using System.Linq;
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
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Data Sync Details Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_DataSyncDetails_Title", "System-Integration/#data-synchronization", "Admin_DataSyncDetails_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class DataSyncDetails : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.AdministrationDataSyncProjects::";

        protected int dataSyncSystemId;

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

            //Get the data sync system from the querystring (if we have one)
            if (string.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID]))
            {
                //Denotes insert mode
                this.dataSyncSystemId = -1;
                this.btnUpdate.Visible = false;
                this.btnAdd.Visible = true;
            }
            else
            {
                this.dataSyncSystemId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID]);
                this.btnUpdate.Visible = true;
                this.btnAdd.Visible = false;
            }

            //Register the button event handlers
            this.btnAdd.Click += new EventHandler(btnAdd_Click);
            this.btnUpdate.Click += new EventHandler(btnUpdate_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);

            //Only load the data once
            if (!IsPostBack)
            {

                LoadAndBindData();
            }

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

            Response.Redirect("DataSynchronization.aspx", true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Updates the changes to the data-sync
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

            try
            {
                //First make sure the page validated OK
                if (!Page.IsValid)
                {
                    return;
                }

                //Retrieve the existing record and make the updates
                DataMappingManager dataMappingManager = new DataMappingManager();
                try
                {
                    DataSyncSystem dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(this.dataSyncSystemId);
                    dataSyncSystem.StartTracking();
                    dataSyncSystem.Name = this.txtName.Text.Trim();
                    if (this.txtCaption.Text.Trim() == "")
                    {
                        dataSyncSystem.Caption = null;
                    }
                    else
                    {
                        dataSyncSystem.Caption = this.txtCaption.Text.Trim();
                    }
                    if (this.txtDescription.Text.Trim() == "")
                    {
                        dataSyncSystem.Description = null;
                    }
                    else
                    {
                        dataSyncSystem.Description = GlobalFunctions.HtmlScrubInput(this.txtDescription.Text.Trim());
                    }
                    dataSyncSystem.ConnectionString = this.txtConnection.Text.Trim();
                    dataSyncSystem.ExternalLogin = this.txtExternalLogin.Text.Trim();
                    if (this.txtExternalPassword.Text.Trim() == "")
                    {
                        dataSyncSystem.ExternalPassword = null;
                    }
                    else
                    {
                        //Make sure we don't update if the password is just the mask
                        if (!GlobalFunctions.IsMasked(this.txtExternalPassword.Text.Trim()))
                        {
                            dataSyncSystem.ExternalPassword = this.txtExternalPassword.Text.Trim();
                        }
                    }
                    dataSyncSystem.TimeOffsetHours = Int32.Parse(this.txtTimeOffset.Text);
                    dataSyncSystem.AutoMapUsersYn = (this.chkAutoMapUsers.Checked) ? "Y" : "N";
                    dataSyncSystem.IsActive = this.chkActive.Checked;

                    //Custom configuration fields
                    if (this.txtCustom01.Text.Trim() == "")
                    {
                        dataSyncSystem.Custom01 = null;
                    }
                    else
                    {
                        dataSyncSystem.Custom01 = this.txtCustom01.Text.Trim();
                    }
                    if (this.txtCustom02.Text.Trim() == "")
                    {
                        dataSyncSystem.Custom02 = null;
                    }
                    else
                    {
                        dataSyncSystem.Custom02 = this.txtCustom02.Text.Trim();
                    }
                    if (this.txtCustom03.Text.Trim() == "")
                    {
                        dataSyncSystem.Custom03 = null;
                    }
                    else
                    {
                        dataSyncSystem.Custom03 = this.txtCustom03.Text.Trim();
                    }
                    if (this.txtCustom04.Text.Trim() == "")
                    {
                        dataSyncSystem.Custom04 = null;
                    }
                    else
                    {
                        dataSyncSystem.Custom04 = this.txtCustom04.Text.Trim();
                    }
                    if (this.txtCustom05.Text.Trim() == "")
                    {
                        dataSyncSystem.Custom05 = null;
                    }
                    else
                    {
                        dataSyncSystem.Custom05 = this.txtCustom05.Text.Trim();
                    }
                    dataMappingManager.UpdateDataSyncSystem(dataSyncSystem);
                }
                catch (ArtifactNotExistsException)
                {
                    //Do nothing since the next line will redirect
                }
                //Just redirect back to the data-sync home
                Response.Redirect("DataSynchronization.aspx");
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

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Inserts the new data-sync into the system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First make sure the page validated OK
                if (!Page.IsValid)
                {
                    return;
                }

                //Now add the new data-sync handling NULLs correctly
                DataMappingManager dataMappingManager = new DataMappingManager();
                string description = null;
                if (this.txtDescription.Text.Trim() != "")
                {
                    description = GlobalFunctions.HtmlScrubInput(this.txtDescription.Text.Trim());
                }
                string password = null;
                if (this.txtExternalPassword.Text.Trim() != "")
                {
                    password = this.txtExternalPassword.Text.Trim();
                }

                string caption = null;
                if (this.txtCaption.Text.Trim() != "")
                {
                    caption = this.txtCaption.Text.Trim();
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
                dataMappingManager.InsertDataSyncSystem(
                    this.txtName.Text.Trim(),
                    caption,
                    description,
                    this.txtConnection.Text.Trim(),
                    this.txtExternalLogin.Text.Trim(),
                    password,
                    Int32.Parse(this.txtTimeOffset.Text),
                    this.chkAutoMapUsers.Checked,
                    custom01,
                    custom02,
                    custom03,
                    custom04,
                    custom05,
                    this.chkActive.Checked
                    );

                //Just redirect back to the data-sync home
                Response.Redirect("DataSynchronization.aspx");
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Loads the data sync information for the current project
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
                if (this.dataSyncSystemId == -1)
                {
                    this.lblPlugInName.Text = Resources.Main.Admin_DataSyncDetails_NewPlugIn;
                    this.ltrPluginName.Text = Resources.Main.Admin_DataSyncDetails_NewPlugIn;
                    this.txtTimeOffset.Text = "0";  //default to zero
                    this.chkActive.Checked = true;
                }
                else
                {
                    //First we need to retrieve the plug-in record
                    DataMappingManager dataMappingManager = new DataMappingManager();
                    DataSyncSystem dataSyncSystem = null;
                    try
                    {
                        dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(this.dataSyncSystemId);
                    }
                    catch (ArtifactNotExistsException)
                    {
                        this.lblMessage.Text = Resources.Messages.Admin_DataSyncDetails_PlugInDeleted;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        this.btnUpdate.Enabled = false;
                        return;
                    }

                    //Populate the form, handling NULLs correctly
                    this.lblPlugInName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(dataSyncSystem.DisplayName);
                    this.ltrPluginName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(dataSyncSystem.DisplayName);
                    this.txtName.Text = dataSyncSystem.Name;
                    if (dataSyncSystem.Caption == null)
                    {
                        this.txtCaption.Text = "";
                    }
                    else
                    {
                        this.txtCaption.Text = dataSyncSystem.Caption;
                    }
                    if (dataSyncSystem.Description == null)
                    {
                        this.txtDescription.Text = "";
                    }
                    else
                    {
                        this.txtDescription.Text = dataSyncSystem.Description;
                    }
                    this.txtConnection.Text = dataSyncSystem.ConnectionString;
                    this.txtTimeOffset.Text = dataSyncSystem.TimeOffsetHours.ToString();
                    this.chkAutoMapUsers.Checked = (dataSyncSystem.AutoMapUsersYn == "Y");
                    this.chkActive.Checked = dataSyncSystem.IsActive;

                    //Credentials
                    this.txtExternalLogin.Text = dataSyncSystem.ExternalLogin;
                    if (dataSyncSystem.ExternalPassword == null)
                    {
                        this.txtExternalPassword.Text = "";
                    }
                    else
                    {
                        this.txtExternalPassword.Text = GlobalFunctions.MaskPassword(dataSyncSystem.ExternalPassword);
                    }

                    //Custom Configuration Parameters
                    if (dataSyncSystem.Custom01 == null)
                    {
                        this.txtCustom01.Text = "";
                    }
                    else
                    {
                        this.txtCustom01.Text = dataSyncSystem.Custom01;
                    }
                    if (dataSyncSystem.Custom02 == null)
                    {
                        this.txtCustom02.Text = "";
                    }
                    else
                    {
                        this.txtCustom02.Text = dataSyncSystem.Custom02;
                    }
                    if (dataSyncSystem.Custom03 == null)
                    {
                        this.txtCustom03.Text = "";
                    }
                    else
                    {
                        this.txtCustom03.Text = dataSyncSystem.Custom03;
                    }
                    if (dataSyncSystem.Custom04 == null)
                    {
                        this.txtCustom04.Text = "";
                    }
                    else
                    {
                        this.txtCustom04.Text = dataSyncSystem.Custom04;
                    }
                    if (dataSyncSystem.Custom05 == null)
                    {
                        this.txtCustom05.Text = "";
                    }
                    else
                    {
                        this.txtCustom05.Text = dataSyncSystem.Custom05;
                    }
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
