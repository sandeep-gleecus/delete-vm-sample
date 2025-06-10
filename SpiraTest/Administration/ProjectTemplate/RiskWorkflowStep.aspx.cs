using System;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Data;
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
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Workflow Step Details Page and handling all raised events
	/// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_WorkflowStep_Title", "Template-Risks/#edit-workflow-step", "Admin_WorkflowStep_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class RiskWorkflowStep : AdministrationBase
	{
		protected System.Web.UI.WebControls.Label lblIncomingSymbol;

		protected int workflowId;
		protected int riskStatusId;
        protected RiskStatus riskStatus;

        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RiskWorkflowStep::";

		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

			//Reset the error message
			this.lblMessage.Text = "";

			//Capture the workflow id and risk status id from the querystring
			this.workflowId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_WORKFLOW_ID]);
			this.riskStatusId = Int32.Parse (Request.QueryString[GlobalFunctions.PARAMETER_TASK_STATUS_ID]);

            //Add the event handlers
            this.grdRiskFields.RowDataBound += new GridViewRowEventHandler(grdRiskFields_RowDataBound);
            this.grdRiskCustomProperties.RowDataBound += new GridViewRowEventHandler(grdRiskCustomProperties_RowDataBound);
            this.btnSave1.Click += new EventHandler(btnSave_Click);
            this.btnSave2.Click += new EventHandler(btnSave_Click);

            //Only load the data once
            if (!IsPostBack)
            {
                //Get the risk status record itself
                RiskWorkflowManager workflowManager = new RiskWorkflowManager();
                this.riskStatus = workflowManager.Status_RetrieveById(this.riskStatusId, workflowId);

                //Get the current workflow
                RiskWorkflow workflow = workflowManager.Workflow_RetrieveById(this.workflowId);

                //Get the incoming and outgoing workflow transitions for the diagram
                List<DataModel.RiskWorkflowTransition> incomingTransitions = workflowManager.WorkflowTransition_RetrieveByOutputStatus(this.workflowId, this.riskStatusId);
                List<DataModel.RiskWorkflowTransition> outgoingTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(this.workflowId, this.riskStatusId);
                this.grdIncomingTransitions.DataSource = incomingTransitions;
                this.grdOutgoingTransitions.DataSource = outgoingTransitions;

                //Get the list of artifact fields that can be workflow configured and custom properties
                List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Risk);
                List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(ProjectTemplateId, Artifact.ArtifactTypeEnum.Risk, false);
                this.grdRiskFields.DataSource = artifactFields;
                this.grdRiskCustomProperties.DataSource = customProperties;

                //Set the risk status (workflow step) name in the title
                this.lblStepName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(riskStatus.Name);

                //Set the labels and URLs on the workflow diagram
                this.lblStepName2.Text = Microsoft.Security.Application.Encoder.HtmlEncode(riskStatus.Name + " (" + riskStatus.RiskStatusId.ToString() + ")");

                //Databind the form
                this.DataBind();
            }

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>
        /// Saves the changes to the risk fields and custom properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnSave_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnSave_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            //NOTE: this block should not be needed now that, as of 6.1, we are using radio buttons.
            //First iterate through the standard fields to make sure we don't have any exception cases
            foreach (GridViewRow gvr in this.grdRiskFields.Rows)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (gvr.RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    RadioButtonEx chkHidden = (RadioButtonEx)gvr.FindControl("chkHidden");
                    RadioButtonEx chkDisabled = (RadioButtonEx)gvr.FindControl("chkDisabled");
                    RadioButtonEx chkRequired = (RadioButtonEx)gvr.FindControl("chkRequired");

                    if (chkRequired.Checked && (chkDisabled.Checked || chkHidden.Checked))
                    {
                        this.lblMessage.Text = Resources.Messages.Admin_WorkflowStep_RequiredNotAllowed;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;
                    }
                }
            }

            //NOTE: this block should not be needed now that, as of 6.1, we are using radio buttons.
            //First iterate through the custom properties to make sure we don't have any exception cases
            foreach (GridViewRow gvr in this.grdRiskCustomProperties.Rows)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (gvr.RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    RadioButtonEx chkHidden = (RadioButtonEx)gvr.FindControl("chkHidden");
                    RadioButtonEx chkDisabled = (RadioButtonEx)gvr.FindControl("chkDisabled");
                    RadioButtonEx chkRequired = (RadioButtonEx)gvr.FindControl("chkRequired");

                    if (chkRequired.Checked && (chkDisabled.Checked || chkHidden.Checked))
                    {
                        this.lblMessage.Text = Resources.Messages.Admin_WorkflowStep_RequiredNotAllowed;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;
                    }
                }
            }

            //Get the risk status record itself
            RiskWorkflowManager workflowManager = new RiskWorkflowManager();
            this.riskStatusId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TASK_STATUS_ID]);
            this.riskStatus = workflowManager.Status_RetrieveById(this.riskStatusId, workflowId);
            this.riskStatus.StartTracking();

            //Iterate through the rows of the datagrid and get the id and various control values
            foreach (GridViewRow gvr in this.grdRiskFields.Rows)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (gvr.RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    RadioButtonEx chkHidden = (RadioButtonEx)gvr.FindControl("chkHidden");
                    RadioButtonEx chkDisabled = (RadioButtonEx)gvr.FindControl("chkDisabled");
                    RadioButtonEx chkRequired = (RadioButtonEx)gvr.FindControl("chkRequired");

                    //Now get the artifact field id
                    int artifactFieldId = Int32.Parse(chkDisabled.MetaData);

                    //Now update the fields in the entity
                    bool isHidden = (this.riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.RiskWorkflowId == this.workflowId) != null);
                    bool isDisabled = (this.riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.RiskWorkflowId == this.workflowId) != null);
                    bool isRequired = (this.riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.RiskWorkflowId == this.workflowId) != null);

                    //Unset any fields
                    if (isHidden && !chkHidden.Checked)
                    {
                        RiskWorkflowField field = this.riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.RiskWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }
                    if (isDisabled && !chkDisabled.Checked)
                    {
                        RiskWorkflowField field = this.riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.RiskWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }
                    if (isRequired && !chkRequired.Checked)
                    {
                        RiskWorkflowField field = this.riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.RiskWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }

                    //Add any fields
                    if (!isHidden && chkHidden.Checked)
                    {
                        RiskWorkflowField field = new RiskWorkflowField();
                        field.ArtifactFieldId = artifactFieldId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
                        field.RiskWorkflowId = this.workflowId;
                        this.riskStatus.WorkflowFields.Add(field);
                    }
                    if (!isDisabled && chkDisabled.Checked)
                    {
                        RiskWorkflowField field = new RiskWorkflowField();
                        field.ArtifactFieldId = artifactFieldId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
                        field.RiskWorkflowId = this.workflowId;
                        this.riskStatus.WorkflowFields.Add(field);
                    }
                    if (!isRequired && chkRequired.Checked)
                    {
                        RiskWorkflowField field = new RiskWorkflowField();
                        field.ArtifactFieldId = artifactFieldId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
                        field.RiskWorkflowId = this.workflowId;
                        this.riskStatus.WorkflowFields.Add(field);
                    }
                }
            }


            //Iterate through the rows of the datagrid and get the id and various control values
            foreach (GridViewRow gvr in this.grdRiskCustomProperties.Rows)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (gvr.RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    RadioButtonEx chkHidden = (RadioButtonEx)gvr.FindControl("chkHidden");
                    RadioButtonEx chkDisabled = (RadioButtonEx)gvr.FindControl("chkDisabled");
                    RadioButtonEx chkRequired = (RadioButtonEx)gvr.FindControl("chkRequired");

                    //Now get the custom property id
                    int customPropertyId = Int32.Parse(chkDisabled.MetaData);

                    //Now update the fields in the entity
                    bool isHidden = (this.riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.RiskWorkflowId == this.workflowId) != null);
                    bool isDisabled = (this.riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.RiskWorkflowId == this.workflowId) != null);
                    bool isRequired = (this.riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.RiskWorkflowId == this.workflowId) != null);

                    //Unset any fields
                    if (isHidden && !chkHidden.Checked)
                    {
                        RiskWorkflowCustomProperty field = this.riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.RiskWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }
                    if (isDisabled && !chkDisabled.Checked)
                    {
                        RiskWorkflowCustomProperty field = this.riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.RiskWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }
                    if (isRequired && !chkRequired.Checked)
                    {
                        RiskWorkflowCustomProperty field = this.riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.RiskWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }

                    //Add any fields
                    if (!isHidden && chkHidden.Checked)
                    {
                        RiskWorkflowCustomProperty field = new RiskWorkflowCustomProperty();
                        field.CustomPropertyId = customPropertyId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
                        field.RiskWorkflowId = this.workflowId;
                        this.riskStatus.WorkflowCustomProperties.Add(field);
                    }
                    if (!isDisabled && chkDisabled.Checked)
                    {
                        RiskWorkflowCustomProperty field = new RiskWorkflowCustomProperty();
                        field.CustomPropertyId = customPropertyId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
                        field.RiskWorkflowId = this.workflowId;
                        this.riskStatus.WorkflowCustomProperties.Add(field);
                    }
                    if (!isRequired && chkRequired.Checked)
                    {
                        RiskWorkflowCustomProperty field = new RiskWorkflowCustomProperty();
                        field.CustomPropertyId = customPropertyId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
                        field.RiskWorkflowId = this.workflowId;
                        this.riskStatus.WorkflowCustomProperties.Add(field);
                    }
                }
            }

            //Commit the changes
            workflowManager.Status_UpdateFieldsAndCustomProperties(riskStatus);

            //Finally clear the cached data used in the planning boards
            //Services.Ajax.RiskBoardService.ClearRiskWorkflowCache();

            //Display a confirmation message
            this.lblMessage.Text = Resources.Messages.Admin_WorkflowStep_UpdateSuccess;
            this.lblMessage.Type = MessageBox.MessageType.Information;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Called when the list of possible risk fields is data-bound
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void grdRiskFields_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Get a handle to the two checkboxes in the row (if the row has any)
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                RadioButtonEx chkDefault = (RadioButtonEx)e.Row.FindControl("chkDefault");
                RadioButtonEx chkHidden = (RadioButtonEx)e.Row.FindControl("chkHidden");
                RadioButtonEx chkDisabled = (RadioButtonEx)e.Row.FindControl("chkDisabled");
                RadioButtonEx chkRequired = (RadioButtonEx)e.Row.FindControl("chkRequired");

                //Get the artifact field id
                int fieldId = ((ArtifactField)(e.Row.DataItem)).ArtifactFieldId;

                //Now we need to determine if this is hidden/disabled/required or not                
                chkHidden.Checked = (this.riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == fieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.RiskWorkflowId == this.workflowId) != null);
                chkDisabled.Checked = (this.riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == fieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.RiskWorkflowId == this.workflowId) != null);
                chkRequired.Checked = (this.riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == fieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.RiskWorkflowId == this.workflowId) != null);
                chkDefault.Checked = !chkHidden.Checked && !chkDisabled.Checked && !chkRequired.Checked;
            }
        }

        /// <summary>
        /// Called when the list of possible risk custom properties is data-bound
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void grdRiskCustomProperties_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Get a handle to the two checkboxes in the row (if the row has any)
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                RadioButtonEx chkDefault = (RadioButtonEx)e.Row.FindControl("chkDefault");
                RadioButtonEx chkHidden = (RadioButtonEx)e.Row.FindControl("chkHidden");
                RadioButtonEx chkDisabled = (RadioButtonEx)e.Row.FindControl("chkDisabled");
                RadioButtonEx chkRequired = (RadioButtonEx)e.Row.FindControl("chkRequired");

                //Get the custom property id
                int customPropertyId = ((CustomProperty)(e.Row.DataItem)).CustomPropertyId;

                //Now we need to determine if this is hidden/disabled/required or not                
                chkHidden.Checked = (this.riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.RiskWorkflowId == this.workflowId) != null);
                chkDisabled.Checked = (this.riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.RiskWorkflowId == this.workflowId) != null);
                chkRequired.Checked = (this.riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.RiskWorkflowId == this.workflowId) != null);
                chkDefault.Checked = !chkHidden.Checked && !chkDisabled.Checked && !chkRequired.Checked;
            }
        }

		#endregion

        #region Page Methods

        /// <summary>
        /// Gets the localized name for a field (if there is one)
        /// </summary>
        /// <param name="captionKey">The resource key</param>
        /// <returns>The localized name</returns>
        public string GetLocalizedFieldCaption(string captionKey)
        {
            if (String.IsNullOrEmpty(Resources.Fields.ResourceManager.GetString(captionKey)))
            {
                return captionKey;
            }
            else
            {
                return Resources.Fields.ResourceManager.GetString(captionKey);
            }
        }

        #endregion
    }
}
