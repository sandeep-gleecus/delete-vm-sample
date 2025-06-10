using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
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
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Standard Field Data Mapping Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_DataSyncFieldMapping_Title", "System-Administration", "Admin_DataSyncFieldMapping_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class DataSyncFieldMapping : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.AdministrationDataSyncFieldMapping::";

        protected int dataSyncSystemId;
        protected int artifactFieldId;
        protected SortedList<string,string> activeFlagList;
        protected string dataSyncDisplayName = "";

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

            //Get the data sync system and artifact field id from the querystring
            this.dataSyncSystemId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID]);
            this.artifactFieldId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_FIELD_ID]);

            //Register the event handlers
            this.btnUpdate.Click += new EventHandler(btnUpdate_Click);

            //Only load the data once
            if (!IsPostBack)
            {
                //Set the project name, product name and the hyperlink
                this.lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
                this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");
                this.lnkProjectMappings.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "DataSyncProjects") + "?" + GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID + "=" + this.dataSyncSystemId;

                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Updates the field value mappings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First we need to retrieve the existing list of field value mappings
                DataMappingManager dataMappingManager = new DataMappingManager();
                List<DataSyncArtifactFieldValueMapping> fieldValueMappings = dataMappingManager.RetrieveDataSyncFieldValueMappings(this.dataSyncSystemId, this.ProjectId, this.artifactFieldId);

                //Now we need to iterate through the grid changing the external key and flag for each one
                foreach (GridViewRow gvr in this.grdDataMappings.Rows)
                {
                    if (gvr.RowType == DataControlRowType.DataRow)
                    {
                        //Make sure we can access the text box and 
                        if (gvr.Cells[2].FindControl("txtExternalKey") != null && gvr.Cells[3].FindControl("chkPrimary") != null)
                        {
                            TextBoxEx txtExternalKey = (TextBoxEx)gvr.Cells[2].FindControl("txtExternalKey");
                            CheckBoxYnEx chkPrimary = (CheckBoxYnEx)gvr.Cells[3].FindControl("chkPrimary");
                            int artifactFieldValue = Int32.Parse(txtExternalKey.MetaData);
                            //See if we have a matching row
                            DataSyncArtifactFieldValueMapping fieldValueMapping = fieldValueMappings.FirstOrDefault(d => d.ArtifactFieldValue == artifactFieldValue);
                            if (fieldValueMapping == null)
                            {
                                if (txtExternalKey.Text.Trim() != "")
                                {
                                    //Add the row
                                    DataSyncArtifactFieldValueMapping newFieldValueMapping = new DataSyncArtifactFieldValueMapping();
                                    newFieldValueMapping.DataSyncSystemId = this.dataSyncSystemId;
                                    newFieldValueMapping.ArtifactFieldId = this.artifactFieldId;
                                    newFieldValueMapping.ArtifactFieldValue = artifactFieldValue;
                                    newFieldValueMapping.ProjectId = this.ProjectId;
                                    newFieldValueMapping.ExternalKey = txtExternalKey.Text.Trim();
                                    newFieldValueMapping.PrimaryYn = (chkPrimary.Checked ? "Y" : "N");
                                    newFieldValueMapping.MarkAsAdded();
                                    fieldValueMappings.Add(newFieldValueMapping);
                                }
                            }
                            else
                            {
                                //Update the row
                                fieldValueMapping.StartTracking();
                                fieldValueMapping.PrimaryYn = (chkPrimary.Checked ? "Y" : "N");

                                if (txtExternalKey.Text.Trim() == "")
                                {
                                    fieldValueMapping.ExternalKey = null;
                                }
                                else
                                {
                                    fieldValueMapping.ExternalKey = txtExternalKey.Text.Trim();
                                }
                            }
                        }
                    }
                }

                //Now save the data, handling any exceptions
                try
                {
                    dataMappingManager.SaveDataSyncFieldValueMappings(fieldValueMappings);
                }
                catch (DataSyncPrimaryExternalKeyException exception)
                {
                    this.lblMessage.Text = exception.Message;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }
                this.lblMessage.Text = Resources.Messages.Admin_DataSyncFieldMapping_Success;
                this.lblMessage.Type = MessageBox.MessageType.Information;
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
        /// Loads the data sync information for the current standard field
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Populate the active dropdown list
                DataMappingManager dataMappingManager = new DataMappingManager();
                this.activeFlagList = dataMappingManager.RetrieveFlagLookup();

                //First get the data-sync record so that we can display its name
                try
                {
                    DataSyncSystem dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
                    this.ltrIntroText.Text = String.Format(Resources.Main.Admin_DataSyncFieldMapping_Intro, "<strong>" + Microsoft.Security.Application.Encoder.HtmlEncode(dataSyncSystem.DisplayName) + "</strong>", ConfigurationSettings.Default.License_ProductType);
                    this.dataSyncDisplayName = dataSyncSystem.DisplayName;
                }
                catch (ArtifactNotExistsException)
                {
                    this.lblMessage.Text = Resources.Messages.Admin_DataSyncProjects_DataSyncNotExists;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }

                //Get the artifact type name and field name to display on the page
                ArtifactManager artifactManager = new ArtifactManager();
                ArtifactField artifactField = artifactManager.ArtifactField_RetrieveById(this.artifactFieldId);
                grdDataMappings.Columns[0].HeaderText = Resources.Fields.FieldValue;
                if (artifactField != null)
                {
                    this.lblArtifactFieldName.Text = artifactField.Caption;
                    grdDataMappings.Columns[0].HeaderText = artifactField.Caption;
                    ArtifactType artifactType = artifactManager.ArtifactType_RetrieveById(artifactField.ArtifactTypeId);
                    if (artifactType != null)
                    {
                        this.lblArtifactTypeName.Text = artifactType.Name;
                    }
                }

                //Load the field mapping values
                List<DataSyncFieldValueMappingView> fieldValueMappings = dataMappingManager.RetrieveDataSyncFieldValueMappings(this.dataSyncSystemId, this.ProjectId, this.artifactFieldId, true);
                this.grdDataMappings.DataSource = fieldValueMappings;
                this.grdDataMappings.DataBind();
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
