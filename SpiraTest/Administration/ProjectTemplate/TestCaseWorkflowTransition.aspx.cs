using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Workflow Transition Details Page and handling all raised events
	/// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_WorkflowTransition_Title", "Template-Test-Cases/#edit-workflow-transition", "Admin_WorkflowTransition_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class TestCaseWorkflowTransition : AdministrationBase
	{
		//Datasets and entities
        DataModel.TestCaseWorkflowTransition workflowTransition;
        protected SortedList<string, string> flagList;

		protected int workflowId;
		protected int workflowTransitionId;

        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.TestCaseWorkflowTransition::";
		private UserManager userManager = new UserManager();
		private int ProjectApproversCount
		{
			get { return (int)ViewState["ProjectApproversCount"]; }
			set { ViewState["ProjectApproversCount"] = value; }
		}

		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Reset the error message
			this.lblMessage.Text = "";

			//Capture the workflow and transition id from the querystring
			this.workflowId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_WORKFLOW_ID]);
			this.workflowTransitionId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_WORKFLOW_TRANSITION_ID]);

			//Add the event handlers
			this.grdExecuteRoles.RowDataBound += new GridViewRowEventHandler(grdExecuteRoles_RowDataBound);
			this.btnUpdate.Click += new EventHandler(btnUpdate_Click);
			this.btnCancel.Click += new EventHandler(btnCancel_Click);

			//Only load the data once
			if (!IsPostBack)
			{
				//Get the workflow transition record (includes the roles)
				TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();
				TestCaseWorkflow workflow = workflowManager.Workflow_RetrieveById(this.workflowId);
				this.workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);

				//Get the list of all active project roles and the list of project roles for this transition
				Business.ProjectManager projectManager = new Business.ProjectManager();
				List<ProjectRole> projectRoles = projectManager.RetrieveProjectRoles(true);
				this.grdExecuteRoles.DataSource = projectRoles;
				
				//Get the flag list
				this.flagList = workflowManager.RetrieveFlagLookup();

				//Set the transition name in the title
				this.lblTransitionName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(workflowTransition.Name) + " (" + GlobalFunctions.ARTIFACT_PREFIX_WORKFLOW_TRANSITION + workflowTransition.WorkflowTransitionId.ToString() + ")";

				//Set the labels and URLs on the workflow diagram
				this.lblTransitionName2.Text = Microsoft.Security.Application.Encoder.HtmlEncode(workflowTransition.Name) + " (" + GlobalFunctions.ARTIFACT_PREFIX_WORKFLOW_TRANSITION + workflowTransition.WorkflowTransitionId.ToString() + ")";
				this.lnkIncomingStep.Text = workflowTransition.InputTestCaseStatus.Name + " (" + workflowTransition.InputTestCaseStatusId.ToString() + ")"; ;
				this.lnkIncomingStep.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "TestCaseWorkflowStep") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString() + "&" + GlobalFunctions.PARAMETER_TEST_CASE_STATUS_ID + "=" + workflowTransition.InputTestCaseStatusId.ToString();
				this.lnkOutgoingStep.Text = workflowTransition.OutputTestCaseStatus.Name + " (" + workflowTransition.OutputTestCaseStatusId.ToString() + ")"; ;
				this.lnkOutgoingStep.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "TestCaseWorkflowStep") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString() + "&" + GlobalFunctions.PARAMETER_TEST_CASE_STATUS_ID + "=" + workflowTransition.OutputTestCaseStatusId.ToString();

				//Set the transition details
				this.txtTransitionName.Text = workflowTransition.Name;
				this.chkRequireSignature.Checked = workflowTransition.IsSignatureRequired;

				//Databind the form
				this.DataBind();

				//Set the static execute/notify flag values
				this.chkExecuteByCreatorYn.Checked = (workflowTransition.IsExecuteByCreator) ? true : false;
				this.chkExecuteByOwnerYn.Checked = (workflowTransition.IsExecuteByOwner) ? true : false;
				this.BindApproversGrid();

			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>
        /// Redirects back to the workflow details page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "TestCaseWorkflowDetails") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId);
        }

		/// <summary>
		/// Called when the list of possible execute roles is data-bound
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
        private void grdExecuteRoles_RowDataBound(object sender, GridViewRowEventArgs e)
		{
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Get a handle to the checkbox in the grid
                CheckBoxEx checkBox = (CheckBoxEx)e.Row.FindControl("chkExecuteRole");
                if (checkBox != null)
                {
                    //See if we have this project role in the list execute dataset
                    int projectRoleId = Int32.Parse(checkBox.MetaData);
                    if (workflowTransition.TransitionRoles.Any(r => r.Role.ProjectRoleId == projectRoleId && r.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute))
                    {
                        checkBox.Checked = true;
                    }
                }
            }
		}

		/// <summary>
		/// Called when the UPDATE button on the conditions/notifcations table is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			//Make sure we have a workflow transition name
			if (String.IsNullOrWhiteSpace(this.txtTransitionName.Text))
			{
				this.lblMessage.Text = Resources.Messages.Admin_WorkflowTransition_NameRequired;
				this.lblMessage.Type = MessageBox.MessageType.Error;
				return;
			}

			//Get the workflow transition record (includes the roles)
			TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();
			this.workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			this.workflowTransition.StartTracking();

			//Update the name/signature
			this.workflowTransition.Name = this.txtTransitionName.Text.Trim();
			this.workflowTransition.IsSignatureRequired = this.chkRequireSignature.Checked;

			//First we need to check the opener/assignee drop-downs and update
			this.workflowTransition.IsExecuteByCreator = (this.chkExecuteByCreatorYn.Checked);
			this.workflowTransition.IsExecuteByOwner = (this.chkExecuteByOwnerYn.Checked);

			//Now we need to update the other roles
			UserManager userManager = new UserManager();


			//Now iterate through the execute rows and get the id and various control values
			foreach (GridViewRow gvr in this.grdExecuteRoles.Rows)
			{
				//We only look at item rows (i.e. not headers and footers)
				if (gvr.RowType == DataControlRowType.DataRow)
				{
					//Extract the various controls from the datagrid
					CheckBoxEx chkExecuteRole = (CheckBoxEx)gvr.FindControl("chkExecuteRole");

					//Now get the project role id
					int projectRoleId = Int32.Parse(chkExecuteRole.MetaData);

					//Find the matching row in the entity collection
					TestCaseWorkflowTransitionRole workflowTransitionRole = this.workflowTransition.TransitionRoles.FirstOrDefault(r => r.ProjectRoleId == projectRoleId && r.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute);

					//See if we have a matching row and either add/or delete if changes needed
					if (workflowTransitionRole == null && chkExecuteRole.Checked)
					{
						//Add the role since it's not already there
						workflowTransitionRole = new TestCaseWorkflowTransitionRole();
						workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
						workflowTransitionRole.ProjectRoleId = projectRoleId;
						this.workflowTransition.TransitionRoles.Add(workflowTransitionRole);
					}
					if (workflowTransitionRole != null && !chkExecuteRole.Checked)
					{
						//Delete the role since it shouldn't be there any more
						workflowTransitionRole.MarkAsDeleted();
					}
				}
			}

			//Now commit the changes
			workflowManager.WorkflowTransition_Update(this.workflowTransition);
			var transitionApprovers = workflowManager.GetApproversForTestCaseWorkflowTransition(this.ProjectId, this.workflowTransitionId);
			DateTime updatedateTime = DateTime.Now;
			List<TestCaseApprovalUser> changedEntities = new List<TestCaseApprovalUser>();

			if (chkRequireSignature.Checked)
			{
				foreach (GridViewRow gvr in this.assignedApprovers.Rows)
				{
					//We only look at item rows (i.e. not headers and footers)
					if (gvr.RowType == DataControlRowType.DataRow)
					{
						LabelEx projectSignatureId = gvr.FindControl("lblProjectSignatureId") as LabelEx;
						int userId = Int32.Parse(projectSignatureId.Text);
						var found = transitionApprovers.Where(x => x.UserId == userId).FirstOrDefault();

						TextBoxEx txtOrder = gvr.FindControl("txtOrder") as TextBoxEx;
						short? orderId = String.IsNullOrWhiteSpace(txtOrder.Text) ? new Nullable<short>() : short.Parse(txtOrder.Text);
						if (orderId.HasValue)
						{
							if (found == null)
							{
								found = new TestCaseApprovalUser
								{
									IsActive = true,
									OrderId = orderId.Value,
									ProjectId = this.ProjectId,
									TransitionId = this.workflowTransitionId,
									UpdateDate = updatedateTime,
									UserId = userId
								};

								changedEntities.Add(found.MarkAsAdded());
							}
							else
							{
								found.OrderId = orderId.Value;
								found.UpdateDate = updatedateTime;
								changedEntities.Add(found.MarkAsModified());
							}
						}
						else
						{
							if (found != null)
							{
								found.OrderId = null;
								found.IsActive = false;
								found.UpdateDate = updatedateTime;
								changedEntities.Add(found.MarkAsModified());
							}
						}
					}
				}

			}
			else
			{
				foreach (var item in transitionApprovers)
				{
					item.OrderId = null;
					item.IsActive = false;
					item.UpdateDate = updatedateTime;
					changedEntities.Add(item.MarkAsModified());
				}
			}

			workflowManager.UpdateApproversForTestCaseWorkflowTransition(changedEntities);

			//Display a confirmation message
			this.lblMessage.Text = Resources.Messages.Admin_WorkflowTransition_SettingsSaved;
			this.lblMessage.Type = MessageBox.MessageType.Information;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion

		protected void assignedApprovers_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				var dataItem = e.Row.DataItem as TestCaseApproverDataObject;
				LabelEx projectSignatureId = e.Row.FindControl("lblProjectSignatureId") as LabelEx;
				projectSignatureId.Text = dataItem.UserId.ToString();

				TextBoxEx txtOrder = e.Row.FindControl("txtOrder") as TextBoxEx;
				txtOrder.Text = dataItem.OrderId.HasValue ? dataItem.OrderId.Value.ToString() : "";

				RangeValidator txtRangeValidator = e.Row.FindControl("txtRangeValidator") as RangeValidator;
				txtRangeValidator.MaximumValue = this.ProjectApproversCount.ToString();
			}
		}

		private void BindApproversGrid()
		{
			var projectLevelApprovers = userManager.RetrieveAssignedApproversForProject(this.ProjectId);
			var transitionApprovers = new TestCaseWorkflowManager().GetApproversForTestCaseWorkflowTransition(this.ProjectId, this.workflowTransitionId);

			List<TestCaseApproverDataObject> dataSource = new List<TestCaseApproverDataObject>();
			foreach (var item in projectLevelApprovers)
			{
				var dataObject = new TestCaseApproverDataObject
				{
					 Department = item.User?.Profile?.Department,
					 FullName = item.User?.Profile?.FullName,
					 OrderId = transitionApprovers.Where(x => x.UserId == item.UserId).FirstOrDefault()?.OrderId,
					 ProjectId = item.ProjectId,
					 UserId = item.UserId,
					 UserName = item.User.UserName
				};

				dataSource.Add(dataObject);
			}

			this.ProjectApproversCount = dataSource.Count;
			this.assignedApprovers.DataSource = dataSource;
			this.assignedApprovers.DataBind();
		}
 
	}
}
