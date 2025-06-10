using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;
using System.Web.Script.Serialization;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Document Workflow Details Page and handling all raised events
	/// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_WorkflowDetails_Title", "Template-Documents/#workflow-details", "Admin_WorkflowDetails_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class DocumentWorkflowDetails : AdministrationBase
	{
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.DocumentWorkflowDetails::";

        protected int workflowId;
        protected List<DocumentStatus> documentStatuses;

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

			//Capture the workflow id from the querystring
			int workflowId;
            if (!Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_WORKFLOW_ID], out workflowId))
            {
                //Return back to the list of workflows
                Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "DocumentWorkflows"));
                return;
            }
            this.workflowId = workflowId;

			//Add the event handlers to the datagrid
			this.grdWorkflowSteps.RowCreated +=new GridViewRowEventHandler(grdWorkflowSteps_RowCreated);

			//Add the other event handlers
			this.btnAdd.Click +=new EventHandler(btnAdd_Click);

            //Now get the list of possible workflow steps (i.e. document statuses)
            AttachmentManager attachmentManager = new AttachmentManager();
            this.documentStatuses = attachmentManager.DocumentStatus_Retrieve(ProjectTemplateId);

            //Set the the return URL
            this.lnkWorkflowList.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "DocumentWorkflows");

            //Only load the data once
            if (!IsPostBack) 
			{
				LoadAndBindData();
			}

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Loads and databinds the data-grid and other form elements
		/// </summary>
		protected void LoadAndBindData()
		{
            //Get the workflow information
            DocumentWorkflowManager documentWorkflowManager = new DocumentWorkflowManager();
            DocumentWorkflow workflow = documentWorkflowManager.Workflow_RetrieveById(this.workflowId);

            if (workflow == null)
            {
                //Return back to the list of workflows
                Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "DocumentWorkflows"));
                return;
            }

			//Set the workflow name in the title
            this.ltrWorkflowName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(workflow.Name);

			//Databind the form
            this.grdWorkflowSteps.DataSource = documentStatuses;
            this.DataBind();

		}

		/// <summary>
		/// Filters the list of transitions to the current workflow step
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void grdWorkflowSteps_RowCreated(object sender, GridViewRowEventArgs e)
		{
            //Get the incident status being databound, and add the transitions to the repeater
            if (e.Row.DataItem != null && e.Row.RowType == DataControlRowType.DataRow)
            {
                int statusId = ((DocumentStatus)(e.Row.DataItem)).DocumentStatusId;
                List<DataModel.DocumentWorkflowTransition> transitions = new DocumentWorkflowManager().WorkflowTransition_RetrieveByInputStatus(this.workflowId, statusId);

                //Now set the datasource on the repeater
                Repeater rptWorkflowStepTransitions = (Repeater)e.Row.FindControl("rptWorkflowStepTransitions");
                if (rptWorkflowStepTransitions != null)
                {
                    rptWorkflowStepTransitions.DataSource = transitions;
                }
            }
		}

		/// <summary>
		/// Handles the various button clicks within the workflow transition nested repeater
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void rptWorkflowStepTransitions_ItemCommand(object source, RepeaterCommandEventArgs e)
		{
			//Determine which command was executed and get the appropriate command argument
			if (e.CommandName.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "delete")
			{
				//Delete the workflow transition
				int workflowTransitionId = Int32.Parse (e.CommandArgument.ToString());
                new DocumentWorkflowManager().WorkflowTransition_Delete(workflowTransitionId);

				//Now refresh the datagrid
				LoadAndBindData();
			}
		}

		/// <summary>
		/// Handles clicks on the Insert Transition button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnAdd_Click(object sender, EventArgs e)
		{
			//First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			//Get the id of the source and destination workflow step
			int sourceDocumentStatusId = Int32.Parse (this.hdnSourceStep.Value);
			int destinationDocumentStatusId = Int32.Parse(this.ddlDestinationStep.RawSelectedValue);

			//We allow multiple transitions between the same two incident statuses
			//since they may have different business rules: 
			//E.g. managers can move from Requested > InProgress
			//but when developers do the same thing they need to be the owner of the document
            DocumentWorkflowManager documentWorkflowManager = new Business.DocumentWorkflowManager();
            documentWorkflowManager.WorkflowTransition_Insert(this.workflowId, sourceDocumentStatusId, destinationDocumentStatusId, this.txtTransitionName.Text);

			//Refresh the dataset
			LoadAndBindData();

            //Display a confirmation message
            this.lblMessage.Type = MessageBox.MessageType.Information;
            this.lblMessage.Text = Resources.Messages.Admin_WorkflowDetails_AddSuccess;
        }

        /// <summary>
        /// Returns a javascript serialized dictionary of incident statues and ids
        /// </summary>
        /// <returns></returns>
        protected string GetDocumentStatusesAsJson()
        {
            const string METHOD_NAME = "GetDocumentStatusesAsJson";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            Dictionary<string, string> statusDic = new Dictionary<string, string>();
            foreach (DocumentStatus documentStatus in this.documentStatuses)
            {
                if (!statusDic.ContainsKey(documentStatus.DocumentStatusId.ToString()))
                {
                    statusDic.Add(documentStatus.DocumentStatusId.ToString(), documentStatus.Name);
                }
            }

            JavaScriptSerializer serial = new JavaScriptSerializer();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return serial.Serialize(statusDic);
		}

		#endregion
	}
}
