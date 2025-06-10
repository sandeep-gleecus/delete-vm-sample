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
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Data Mapping Details Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_DataSyncProjects_Title", "System-Administration", "Admin_DataSyncProjects_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class DataSyncProjects : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.DataSyncProjects::";

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
            this.pnlProjectConfiguration.Visible = true;
            this.pnlDataMappings.Visible = true;

            //Get the user id and project id from session
            if (ProjectId < 1)
            {
                this.lblMessage.Text = Resources.Messages.Admin_DataSyncProjects_NeedToSelectProject;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                this.lblProjectName.Text = Resources.Main.Admin_DataSyncProjects_Unselected;
                this.pnlProjectConfiguration.Visible = false;
                this.pnlDataMappings.Visible = false;

                return;
            }

            //Get the data sync system from the querystring
            this.dataSyncSystemId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID]);

            //Register the event handlers
            this.grdArtifacts.RowCreated += new GridViewRowEventHandler(grdArtifacts_RowCreated);
            this.btnUpdate.Click += new EventHandler(btnUpdate_Click);

            //Only load the data once
            if (!IsPostBack)
            {
                //Set the project name and link back to the data sync home
                this.lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
                lblProjectName2.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
                this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

                //See if we're system or project admin
                if (UserIsAdmin)
                {
                    this.lnkDataSyncHome.NavigateUrl = "~/Administration/DataSynchronization.aspx";
                }
                else
                {
                    this.lnkDataSyncHome.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "DataSynchronization");
                }

                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Updates the project data sync record
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Make ure an external key is set
                if (this.txtProjectExtenalKey.Text.Trim() == "")
                {
                    this.lblMessage.Text = Resources.Messages.Admin_DataSyncProjects_NeedToSetExternalKey;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }

                //Retrieve the existing project mapping info
                DataMappingManager dataMappingManager = new DataMappingManager();
                try
                {
                    DataSyncProject dataSyncProject = dataMappingManager.RetrieveDataSyncProject(this.dataSyncSystemId, this.ProjectId);
                    //Update the external key
                    dataSyncProject.StartTracking();
                    dataSyncProject.ExternalKey = this.txtProjectExtenalKey.Text.Trim();
                    dataSyncProject.ActiveYn = this.chkActive.Checked ? "Y" : "N";
                    dataMappingManager.UpdateDataSyncProject(dataSyncProject);
                    this.lblMessage.Text = Resources.Messages.Admin_DataSyncProjects_SettingsSaved;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }
                catch (DataSyncNotConfiguredException)
                {
                    //We need to add a new record
                    bool active = (this.chkActive.Checked == true);
                    dataMappingManager.InsertDataSyncProject(this.dataSyncSystemId, this.ProjectId, this.txtProjectExtenalKey.Text.Trim(), active);

                    //Need to reload the page to show the field mappings for the first time
                    LoadAndBindData();

                    //Display the message
                    this.lblMessage.Text = Resources.Messages.Admin_DataSyncProjects_SettingsSaved;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }
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
        /// Gets the data used for populating the child repeaters in the artifact table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdArtifacts_RowCreated(object sender, GridViewRowEventArgs e)
        {
			//Get the incident status being databound, make sure we have a data item for this row
            if (e.Row.DataItem != null)
            {
                int artifactTypeId = (((ArtifactType)(e.Row.DataItem)).ArtifactTypeId);
                DataMappingManager dataMappingManager = new DataMappingManager();
                
                //First load the artifact fields
                List<ArtifactField> artifactFields = dataMappingManager.RetrieveArtifactFields((DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
                Repeater repeater = (Repeater)e.Row.Cells[1].FindControl("rptStandardFields");
                if (repeater != null)
                {
                    repeater.DataSource = artifactFields;
                }

                //Next load the custom properties
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeId(this.ProjectTemplateId, artifactTypeId, false, false);
                repeater = (Repeater)e.Row.Cells[2].FindControl("rptCustomProperties");
                if (repeater != null)
                {
                    repeater.DataSource = customProperties;
                }

            }
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
                //Populate the active dropdown list
                DataMappingManager dataMappingManager = new DataMappingManager();

                //First get the data-sync record so that we can display its name
                try
                {
                    DataSyncSystem dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
                    this.lblDataSyncName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(dataSyncSystem.DisplayName);
                    this.lblDataSyncName2.Text = Microsoft.Security.Application.Encoder.HtmlEncode(dataSyncSystem.DisplayName);
                }
                catch (ArtifactNotExistsException)
                {
                    this.lblMessage.Text = Resources.Messages.Admin_DataSyncProjects_DataSyncNotExists;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    this.pnlProjectConfiguration.Visible = false;
                    this.pnlDataMappings.Visible = false;
                    return;
                }

                //See if we have existing mapping data for this project
                try
                {
                    DataSyncProject dataSyncProject = dataMappingManager.RetrieveDataSyncProject(this.dataSyncSystemId, this.ProjectId);

                    //We have a record, so populate the fields
                    this.chkActive.Checked = ((dataSyncProject.ActiveYn == "Y") ? true : false);
                    this.txtProjectExtenalKey.Text = dataSyncProject.ExternalKey;

                    List<ArtifactType> artifactTypes = dataMappingManager.RetrieveArtifactTypes();
                    this.grdArtifacts.DataSource = artifactTypes;
                    this.grdArtifacts.DataBind();
                }
                catch (DataSyncNotConfiguredException)
                {
                    //No mapping configured for this project, so display fields as empty.
                    this.chkActive.Checked = false;
                    this.txtProjectExtenalKey.Text = "";
                    this.pnlDataMappings.Visible = false;
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
