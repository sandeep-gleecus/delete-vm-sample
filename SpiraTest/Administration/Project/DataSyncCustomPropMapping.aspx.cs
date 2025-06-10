using System;
using System.Linq;
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
    /// Administration Custom Property Data Mapping Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_DataSyncCustomPropertyMapping_Title", "System-Administration", "Admin_DataSyncCustomPropertyMapping_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class DataSyncCustomPropMapping : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.AdministrationDataSyncCustomPropMapping::";

        protected int dataSyncSystemId;
        protected int artifactTypeId;
        protected int customPropertyId;
        protected string dataSyncDisplayName;
        protected SortedList<string, string> activeFlagList;

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

            //Get the data sync system, artifact type id and custom property id from the querystring
            this.dataSyncSystemId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID]);
            this.artifactTypeId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID]);
            this.customPropertyId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_CUSTOM_PROPERTY_ID]);

            //Register the event handlers
            this.btnUpdate.Click += new EventHandler(btnUpdate_Click);

            //Only load the data once
            if (!IsPostBack)
            {
                //Set the project name and return hyperlink
                this.lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
                this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");
                this.lnkProjectMappings.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "DataSyncProjects") + "?" + GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID + "=" + this.dataSyncSystemId;

                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Updates the custom property and custom property value mappings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First we need to update the custom property mappings
                DataMappingManager dataMappingManager = new DataMappingManager();
                DataSyncCustomPropertyMapping customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(this.dataSyncSystemId, this.ProjectId, (DataModel.Artifact.ArtifactTypeEnum)this.artifactTypeId, this.customPropertyId);
                //See if we have an entry or now and either add, delete or update
                if (this.txtExternalKey.Text.Trim() == "")
                {
                    //We have an existing record that needs to be deleted
                    if (customPropertyMapping != null)
                    {
                        customPropertyMapping.MarkAsDeleted();
                    }
                }
                else
                {
                    //We have an existing record that needs to be updated, otherwise insert
                    if (customPropertyMapping == null)
                    {
                        customPropertyMapping = new DataSyncCustomPropertyMapping();
                        customPropertyMapping.MarkAsAdded();
                        customPropertyMapping.DataSyncSystemId = this.dataSyncSystemId;
                        customPropertyMapping.ProjectId = this.ProjectId;
                        customPropertyMapping.CustomPropertyId = this.customPropertyId;
                        customPropertyMapping.ExternalKey = this.txtExternalKey.Text.Trim();
                    }
                    else
                    {
                        customPropertyMapping.StartTracking();
                        customPropertyMapping.ExternalKey = this.txtExternalKey.Text.Trim();
                    }
                }
                dataMappingManager.SaveDataSyncCustomPropertyMappings(new List<DataSyncCustomPropertyMapping>() { customPropertyMapping });

                //Next we need to retrieve the existing list of custom property value mappings
                List<DataSyncCustomPropertyValueMapping> customPropertyValueMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(this.dataSyncSystemId, this.ProjectId, this.customPropertyId);

                //Now we need to iterate through the grid changing the external key for each one
                foreach (GridViewRow gvr in this.grdDataMappings.Rows)
                {
                    if (gvr.RowType == DataControlRowType.DataRow)
                    {
                        //Make sure we can access the text box
                        if (gvr.Cells[2].FindControl("txtExternalKey") != null)
                        {
                            TextBoxEx txtExternalKey = (TextBoxEx)gvr.Cells[2].FindControl("txtExternalKey");
                            int customPropertyValueId = Int32.Parse(txtExternalKey.MetaData);
                            //See if we have a matching row
                            DataSyncCustomPropertyValueMapping customPropertyValueMapping = customPropertyValueMappings.FirstOrDefault(d => d.CustomPropertyValueId == customPropertyValueId);
                            if (customPropertyValueMapping == null)
                            {
                                customPropertyValueMapping = new DataSyncCustomPropertyValueMapping();
                                customPropertyValueMapping.MarkAsAdded();
                                customPropertyValueMapping.DataSyncSystemId = this.dataSyncSystemId;
                                customPropertyValueMapping.ProjectId = this.ProjectId;
                                customPropertyValueMapping.CustomPropertyValueId = customPropertyValueId;
                                customPropertyValueMapping.ExternalKey = txtExternalKey.Text.Trim();
                                customPropertyValueMappings.Add(customPropertyValueMapping);
                            }
                            else
                            {
                                //Update the row
                                customPropertyValueMapping.StartTracking();
                                if (txtExternalKey.Text.Trim() == "")
                                {
                                    customPropertyValueMapping.ExternalKey = null;
                                    customPropertyValueMapping.MarkAsDeleted();
                                }
                                else
                                {
                                    customPropertyValueMapping.ExternalKey = txtExternalKey.Text.Trim();
                                }
                            }
                        }
                    }
                }

                //Now save the data
                dataMappingManager.SaveDataSyncCustomPropertyValueMappings(customPropertyValueMappings);

                this.lblMessage.Text = Resources.Messages.Admin_DataSyncCustomPropertyMapping_Success;
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
        /// Loads the data sync information for the current custom property
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
                    this.lblPlugInName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(dataSyncSystem.DisplayName);
                    this.dataSyncDisplayName = dataSyncSystem.DisplayName;
                }
                catch (ArtifactNotExistsException)
                {
                    this.lblMessage.Text = Resources.Messages.Admin_DataSyncProjects_DataSyncNotExists;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }

                //Next get the custom property name
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveById(this.customPropertyId);
                if (customProperty == null)
                {
                    this.lblMessage.Text = Resources.Messages.Admin_DataSyncProjects_CustomPropertyNotExists;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;

                }
                this.lblCustomPropertyName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(customProperty.Name);
                this.lblCustomPropertyType.Text = customProperty.CustomPropertyTypeName;

                //Get the artifact type name
                ArtifactManager artifactManager = new ArtifactManager();
                ArtifactType artifactType = artifactManager.ArtifactType_RetrieveById(customProperty.ArtifactTypeId);
                if (artifactType != null)
                {
                    this.lblArtifactTypeName.Text = artifactType.Name;
                }

                //Now get the custom property external key if there is one set
                DataSyncCustomPropertyMapping customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(this.dataSyncSystemId, this.ProjectId, (DataModel.Artifact.ArtifactTypeEnum)this.artifactTypeId, this.customPropertyId);
                if (customPropertyMapping != null)
                {
                    this.txtExternalKey.Text = customPropertyMapping.ExternalKey;
                }
                else
                {
                    this.txtExternalKey.Text = "";
                }

                //Load the custom property mapping values
                if (customProperty.CustomPropertyListId.HasValue && (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List || customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList))
                {
                    List<DataSyncCustomPropertyValueMappingView> customPropertyValueMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(this.dataSyncSystemId, this.ProjectId, (DataModel.Artifact.ArtifactTypeEnum)this.artifactTypeId, this.customPropertyId, true);
                    this.grdDataMappings.DataSource = customPropertyValueMappings;
                    this.grdDataMappings.DataBind();
                }
                else
                {
                    //Hide the value mappings
                    this.plcCustomValueMapping.Visible = false;
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
