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
	/// Administration Workflow Details Page and handling all raised events
	/// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_WorkflowDetails_Title", "Template-Incidents/#edit-workflow-details", "Admin_WorkflowDetails_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
	public partial class IncidentWorkflowDetails : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentWorkflowDetails::";

        protected int workflowId;
        protected List<IncidentStatus> incidentStatuses;

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

            //Set the return URL
            this.lnkWorkflowList.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "IncidentWorkflows");

            //Capture the workflow id from the querystring
            int workflowId;
            if (!Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_WORKFLOW_ID], out workflowId))
            {
                //Return back to the list of workflows
                Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "IncidentWorkflows"));
                return;
            }
            this.workflowId = workflowId;

			//Add the event handlers to the datagrid
			this.grdWorkflowSteps.RowCreated +=new GridViewRowEventHandler(grdWorkflowSteps_RowCreated);

			//Add the other event handlers
			this.btnAdd.Click +=new EventHandler(btnAdd_Click);

            //Now get the list of possible workflow steps (i.e. incident status) for the current project
            IncidentManager incidentManager = new IncidentManager();
            this.incidentStatuses = incidentManager.IncidentStatus_Retrieve(this.ProjectTemplateId, true);

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
            WorkflowManager workflowManager = new WorkflowManager();
            Workflow workflow = workflowManager.Workflow_RetrieveById(this.workflowId);

            if (workflow == null)
            {
                //Return back to the list of workflows
                Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "IncidentWorkflows"));
                return;
            }

			//Set the workflow name in the title
            this.ltrWorkflowName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(workflow.Name);

			//Databind the form
            this.grdWorkflowSteps.DataSource = incidentStatuses;
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
                int statusId = ((IncidentStatus)(e.Row.DataItem)).IncidentStatusId;
                List<DataModel.WorkflowTransition> transitions = new WorkflowManager().WorkflowTransition_RetrieveByInputStatus(this.workflowId, statusId);

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
				new WorkflowManager().WorkflowTransition_Delete(workflowTransitionId);

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
			int sourceIncidentStatusId = Int32.Parse (this.hdnSourceStep.Value);
			int destinationIncidentStatusId = Int32.Parse(this.ddlDestinationStep.RawSelectedValue);

			//We allow multiple transitions between the same two incident statuses
			//since they may have different business rules: 
			//E.g. managers can move from Open > Assigned with the detector being notified
			//but when developers do the same thing there are no notifications, etc.
            WorkflowManager workflowManager = new Business.WorkflowManager();
            workflowManager.WorkflowTransition_Insert(this.workflowId, sourceIncidentStatusId, destinationIncidentStatusId, this.txtTransitionName.Text);

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
        protected string GetIncidentStatusesAsJson()
        {
            const string METHOD_NAME = "GetIncidentStatusesAsJson";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            Dictionary<string, string> statusDic = new Dictionary<string, string>();
            foreach (IncidentStatus incidentStatus in this.incidentStatuses)
            {
                if (!statusDic.ContainsKey(incidentStatus.IncidentStatusId.ToString()))
                {
                    statusDic.Add(incidentStatus.IncidentStatusId.ToString(), incidentStatus.Name);
                }
            }

            JavaScriptSerializer serial = new JavaScriptSerializer();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return serial.Serialize(statusDic);
		}

		#endregion
	}
}
