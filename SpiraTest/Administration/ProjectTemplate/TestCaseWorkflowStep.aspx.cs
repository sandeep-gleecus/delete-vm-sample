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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_WorkflowStep_Title", "Template-Test-Cases/#edit-workflow-step", "Admin_WorkflowStep_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class TestCaseWorkflowStep : AdministrationBase
	{
		protected System.Web.UI.WebControls.Label lblIncomingSymbol;

		protected int workflowId;
		protected int testCaseStatusId;
        protected TestCaseStatus testCaseStatus;

        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.TestCaseWorkflowStep::";

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

			//Capture the workflow id and testCase status id from the querystring
			this.workflowId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_WORKFLOW_ID]);
			this.testCaseStatusId = Int32.Parse (Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_STATUS_ID]);

            //Add the event handlers
            this.grdTestCaseFields.RowDataBound += new GridViewRowEventHandler(grdTestCaseFields_RowDataBound);
            this.grdTestCaseCustomProperties.RowDataBound += new GridViewRowEventHandler(grdTestCaseCustomProperties_RowDataBound);
            this.btnSave1.Click += new EventHandler(btnSave_Click);
            this.btnSave2.Click += new EventHandler(btnSave_Click);

            //Only load the data once
            if (!IsPostBack)
            {
                //Get the testCase status record itself
                TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();
                this.testCaseStatus = workflowManager.Status_RetrieveById(this.testCaseStatusId, workflowId);

                //Get the current workflow
                TestCaseWorkflow workflow = workflowManager.Workflow_RetrieveById(this.workflowId);

                //Get the incoming and outgoing workflow transitions for the diagram
                List<DataModel.TestCaseWorkflowTransition> incomingTransitions = workflowManager.WorkflowTransition_RetrieveByOutputStatus(this.workflowId, this.testCaseStatusId);
                List<DataModel.TestCaseWorkflowTransition> outgoingTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(this.workflowId, this.testCaseStatusId);
                this.grdIncomingTransitions.DataSource = incomingTransitions;
                this.grdOutgoingTransitions.DataSource = outgoingTransitions;

                //Get the list of artifact fields that can be workflow configured and custom properties
                List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.TestCase);
                List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(ProjectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);
                this.grdTestCaseFields.DataSource = artifactFields;
                this.grdTestCaseCustomProperties.DataSource = customProperties;

                //Set the testCase status (workflow step) name in the title
                this.lblStepName.Text = testCaseStatus.Name;

                //Set the labels and URLs on the workflow diagram
                this.lblStepName2.Text = testCaseStatus.Name + " (" + testCaseStatus.TestCaseStatusId.ToString() + ")";

                //Databind the form
                this.DataBind();
            }

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>
        /// Saves the changes to the testCase fields and custom properties
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
            foreach (GridViewRow gvr in this.grdTestCaseFields.Rows)
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
            foreach (GridViewRow gvr in this.grdTestCaseCustomProperties.Rows)
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

            //Get the testCase status record itself
            TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();
            this.testCaseStatusId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_STATUS_ID]);
            this.testCaseStatus = workflowManager.Status_RetrieveById(this.testCaseStatusId, workflowId);
            this.testCaseStatus.StartTracking();

            //Iterate through the rows of the datagrid and get the id and various control values
            foreach (GridViewRow gvr in this.grdTestCaseFields.Rows)
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
                    bool isHidden = (this.testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.TestCaseWorkflowId == this.workflowId) != null);
                    bool isDisabled = (this.testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.TestCaseWorkflowId == this.workflowId) != null);
                    bool isRequired = (this.testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.TestCaseWorkflowId == this.workflowId) != null);

                    //Unset any fields
                    if (isHidden && !chkHidden.Checked)
                    {
                        TestCaseWorkflowField field = this.testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.TestCaseWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }
                    if (isDisabled && !chkDisabled.Checked)
                    {
                        TestCaseWorkflowField field = this.testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.TestCaseWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }
                    if (isRequired && !chkRequired.Checked)
                    {
                        TestCaseWorkflowField field = this.testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.TestCaseWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }

                    //Add any fields
                    if (!isHidden && chkHidden.Checked)
                    {
                        TestCaseWorkflowField field = new TestCaseWorkflowField();
                        field.ArtifactFieldId = artifactFieldId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
                        field.TestCaseWorkflowId = this.workflowId;
                        this.testCaseStatus.WorkflowFields.Add(field);
                    }
                    if (!isDisabled && chkDisabled.Checked)
                    {
                        TestCaseWorkflowField field = new TestCaseWorkflowField();
                        field.ArtifactFieldId = artifactFieldId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
                        field.TestCaseWorkflowId = this.workflowId;
                        this.testCaseStatus.WorkflowFields.Add(field);
                    }
                    if (!isRequired && chkRequired.Checked)
                    {
                        TestCaseWorkflowField field = new TestCaseWorkflowField();
                        field.ArtifactFieldId = artifactFieldId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
                        field.TestCaseWorkflowId = this.workflowId;
                        this.testCaseStatus.WorkflowFields.Add(field);
                    }
                }
            }


            //Iterate through the rows of the datagrid and get the id and various control values
            foreach (GridViewRow gvr in this.grdTestCaseCustomProperties.Rows)
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
                    bool isHidden = (this.testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.TestCaseWorkflowId == this.workflowId) != null);
                    bool isDisabled = (this.testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.TestCaseWorkflowId == this.workflowId) != null);
                    bool isRequired = (this.testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.TestCaseWorkflowId == this.workflowId) != null);

                    //Unset any fields
                    if (isHidden && !chkHidden.Checked)
                    {
                        TestCaseWorkflowCustomProperty field = this.testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.TestCaseWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }
                    if (isDisabled && !chkDisabled.Checked)
                    {
                        TestCaseWorkflowCustomProperty field = this.testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.TestCaseWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }
                    if (isRequired && !chkRequired.Checked)
                    {
                        TestCaseWorkflowCustomProperty field = this.testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.TestCaseWorkflowId == this.workflowId);
                        field.MarkAsDeleted();
                    }

                    //Add any fields
                    if (!isHidden && chkHidden.Checked)
                    {
                        TestCaseWorkflowCustomProperty field = new TestCaseWorkflowCustomProperty();
                        field.CustomPropertyId = customPropertyId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
                        field.TestCaseWorkflowId = this.workflowId;
                        this.testCaseStatus.WorkflowCustomProperties.Add(field);
                    }
                    if (!isDisabled && chkDisabled.Checked)
                    {
                        TestCaseWorkflowCustomProperty field = new TestCaseWorkflowCustomProperty();
                        field.CustomPropertyId = customPropertyId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
                        field.TestCaseWorkflowId = this.workflowId;
                        this.testCaseStatus.WorkflowCustomProperties.Add(field);
                    }
                    if (!isRequired && chkRequired.Checked)
                    {
                        TestCaseWorkflowCustomProperty field = new TestCaseWorkflowCustomProperty();
                        field.CustomPropertyId = customPropertyId;
                        field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
                        field.TestCaseWorkflowId = this.workflowId;
                        this.testCaseStatus.WorkflowCustomProperties.Add(field);
                    }
                }
            }

            //Commit the changes
            workflowManager.Status_UpdateFieldsAndCustomProperties(testCaseStatus);

            //Display a confirmation message
            this.lblMessage.Text = Resources.Messages.Admin_WorkflowStep_UpdateSuccess;
            this.lblMessage.Type = MessageBox.MessageType.Information;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Called when the list of possible testCase fields is data-bound
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void grdTestCaseFields_RowDataBound(object sender, GridViewRowEventArgs e)
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
                chkHidden.Checked = (this.testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == fieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.TestCaseWorkflowId == this.workflowId) != null);
                chkDisabled.Checked = (this.testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == fieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.TestCaseWorkflowId == this.workflowId) != null);
                chkRequired.Checked = (this.testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == fieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.TestCaseWorkflowId == this.workflowId) != null);
                chkDefault.Checked = !chkHidden.Checked && !chkDisabled.Checked && !chkRequired.Checked;
            }
        }

        /// <summary>
        /// Called when the list of possible testCase custom properties is data-bound
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void grdTestCaseCustomProperties_RowDataBound(object sender, GridViewRowEventArgs e)
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
                chkHidden.Checked = (this.testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.TestCaseWorkflowId == this.workflowId) != null);
                chkDisabled.Checked = (this.testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.TestCaseWorkflowId == this.workflowId) != null);
                chkRequired.Checked = (this.testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.TestCaseWorkflowId == this.workflowId) != null);
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
