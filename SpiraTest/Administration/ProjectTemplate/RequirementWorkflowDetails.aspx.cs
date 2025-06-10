using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Requirement Workflow Details Page and handling all raised events
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_WorkflowDetails_Title", "Template-Requirements/#edit-workflow-details", "Admin_WorkflowDetails_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class RequirementWorkflowDetails : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.ProjectTemplate.RequirementWorkflowDetails::";

		protected int workflowId;
		protected List<RequirementStatus> requirementStatuses;

		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";

			Logger.LogEnteringEvent(METHOD_NAME);

			//Reset the error message
			lblMessage.Text = "";

			//Set the return URL
			lnkWorkflowList.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "RequirementWorkflows");

			//Capture the workflow id from the querystring
			int workflowId;
			if (!int.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_WORKFLOW_ID], out workflowId))
			{
				//Return back to the list of workflows
				Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "RequirementWorkflows"));
				return;
			}
			this.workflowId = workflowId;

			//Add the event handlers to the datagrid
			grdWorkflowSteps.RowCreated += new GridViewRowEventHandler(grdWorkflowSteps_RowCreated);

			//Add the other event handlers
			btnAdd.Click += new EventHandler(btnAdd_Click);

			//Now get the list of possible workflow steps (i.e. requirement statuses)
			RequirementManager requirementManager = new RequirementManager();
			requirementStatuses = requirementManager.RetrieveStatuses();

			//Only load the data once
			if (!IsPostBack) LoadAndBindData();

			Logger.LogExitingEvent(METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Loads and databinds the data-grid and other form elements
		/// </summary>
		protected void LoadAndBindData()
		{
			//Get the workflow information
			RequirementWorkflowManager requirementWorkflowManager = new RequirementWorkflowManager();
			RequirementWorkflow workflow = requirementWorkflowManager.Workflow_RetrieveById(workflowId);

			if (workflow == null)
			{
				//Return back to the list of workflows
				Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "RequirementWorkflows.aspx"));
				return;
			}

			//Set the workflow name in the title
			ltrWorkflowName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(workflow.Name);

			//Databind the form
			grdWorkflowSteps.DataSource = requirementStatuses;
			DataBind();

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
				int statusId = ((RequirementStatus)(e.Row.DataItem)).RequirementStatusId;
				List<DataModel.RequirementWorkflowTransition> transitions = new RequirementWorkflowManager().WorkflowTransition_RetrieveByInputStatus(workflowId, statusId);

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
				int workflowTransitionId = Int32.Parse(e.CommandArgument.ToString());
				new RequirementWorkflowManager().WorkflowTransition_Delete(workflowTransitionId);

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
			if (!IsValid)
			{
				return;
			}

			//Get the id of the source and destination workflow step
			int sourceRequirementStatusId = Int32.Parse(hdnSourceStep.Value);
			int destinationRequirementStatusId = Int32.Parse(ddlDestinationStep.RawSelectedValue);

			//We allow multiple transitions between the same two incident statuses
			//since they may have different business rules: 
			//E.g. managers can move from Requested > InProgress
			//but when developers do the same thing they need to be the owner of the requirement
			RequirementWorkflowManager requirementWorkflowManager = new Business.RequirementWorkflowManager();
			requirementWorkflowManager.WorkflowTransition_Insert(workflowId, sourceRequirementStatusId, destinationRequirementStatusId, txtTransitionName.Text);

			//Refresh the dataset
			LoadAndBindData();

			//Display a confirmation message
			lblMessage.Type = MessageBox.MessageType.Information;
			lblMessage.Text = Resources.Messages.Admin_WorkflowDetails_AddSuccess;
		}

		/// <summary>
		/// Returns a javascript serialized dictionary of incident statues and ids
		/// </summary>
		/// <returns></returns>
		protected string GetRequirementStatusesAsJson()
		{
			const string METHOD_NAME = CLASS_NAME + "GetRequirementStatusesAsJson()";

			Logger.LogEnteringEvent(METHOD_NAME);

			Dictionary<string, string> statusDic = new Dictionary<string, string>();
			foreach (RequirementStatus requirementStatus in requirementStatuses)
			{
				if (!statusDic.ContainsKey(requirementStatus.RequirementStatusId.ToString()))
				{
					statusDic.Add(requirementStatus.RequirementStatusId.ToString(), requirementStatus.Name);
				}
			}

			JavaScriptSerializer serial = new JavaScriptSerializer();

			Logger.LogExitingEvent(METHOD_NAME);
			return serial.Serialize(statusDic);
		}

		#endregion
	}
}
