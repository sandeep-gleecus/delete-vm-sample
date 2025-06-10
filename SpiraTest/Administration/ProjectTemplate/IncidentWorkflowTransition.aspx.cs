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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_WorkflowTransition_Title", "Template-Incidents/#edit-workflow-transition", "Admin_WorkflowTransition_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class IncidentWorkflowTransition : AdministrationBase
	{
		//Datasets and entities
        DataModel.WorkflowTransition workflowTransition;
        protected SortedList<string, string> flagList;

		protected int workflowId;
		protected int workflowTransitionId;

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentWorkflowTransition::";

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

			//Capture the workflow and transition id from the querystring
			this.workflowId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_WORKFLOW_ID]);
			this.workflowTransitionId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_WORKFLOW_TRANSITION_ID]);

            //Set the return URL
            this.lnkWorkflowDetails.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "IncidentWorkflowDetails") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId;

            //Add the event handlers
            this.grdExecuteRoles.RowDataBound += new GridViewRowEventHandler(grdExecuteRoles_RowDataBound);
            this.grdNotifyRoles.RowDataBound += new GridViewRowEventHandler(grdNotifyRoles_RowDataBound);
            this.btnUpdate.Click +=new EventHandler(btnUpdate_Click);
            this.btnUpdate2.Click += new EventHandler(btnUpdate_Click);
            this.btnUpdate3.Click += new EventHandler(btnUpdate_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);
            this.btnCancel2.Click += new EventHandler(btnCancel_Click);
            this.btnCancel3.Click += new EventHandler(btnCancel_Click);
            this.grdFields.ItemDataBound += new System.Web.UI.WebControls.DataListItemEventHandler(grdFields_ItemDataBound);

			//Only load the data once
			if (!IsPostBack) 
			{
                //Get the workflow transition record (includes the roles)
                WorkflowManager workflowManager = new WorkflowManager();
                Workflow workflow = workflowManager.Workflow_RetrieveById(this.workflowId);
                this.workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);

				//Get the list of all active project roles and the list of project roles for this transition
                Business.ProjectManager projectManager = new Business.ProjectManager();
                List<ProjectRole> projectRoles = projectManager.RetrieveProjectRoles(true);
                this.grdExecuteRoles.DataSource = projectRoles;
                this.grdNotifyRoles.DataSource = projectRoles;

				//Get the flag list
				this.flagList = workflowManager.RetrieveFlagLookup();

				//Set the transition name in the title
                this.lblTransitionName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(workflowTransition.Name) + " (" + GlobalFunctions.ARTIFACT_PREFIX_WORKFLOW_TRANSITION + workflowTransition.WorkflowTransitionId.ToString() + ")";

				//Set the labels and URLs on the workflow diagram
				this.lblTransitionName2.Text = Microsoft.Security.Application.Encoder.HtmlEncode(workflowTransition.Name + " (" + GlobalFunctions.ARTIFACT_PREFIX_WORKFLOW_TRANSITION + workflowTransition.WorkflowTransitionId.ToString() + ")");
				this.lnkIncomingStep.Text = Microsoft.Security.Application.Encoder.HtmlEncode(workflowTransition.InputStatus.Name + " (" + GlobalFunctions.ARTIFACT_PREFIX_INCIDENT_STATUS + workflowTransition.InputIncidentStatusId.ToString() + ")");
                this.lnkIncomingStep.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "IncidentWorkflowStep") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString() + "&" + GlobalFunctions.PARAMETER_INCIDENT_STATUS_ID + "=" + workflowTransition.InputIncidentStatusId.ToString();
				this.lnkOutgoingStep.Text = Microsoft.Security.Application.Encoder.HtmlEncode(workflowTransition.OutputStatus.Name + " (" + GlobalFunctions.ARTIFACT_PREFIX_INCIDENT_STATUS + workflowTransition.OutputIncidentStatusId.ToString() + ")");
                this.lnkOutgoingStep.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "IncidentWorkflowStep") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString() + "&" + GlobalFunctions.PARAMETER_INCIDENT_STATUS_ID + "=" + workflowTransition.OutputIncidentStatusId.ToString();

                //Set the transition details
                this.txtTransitionName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(workflowTransition.Name);
                this.chkRequireSignature.Checked = workflowTransition.IsSignatureRequired;
                if (String.IsNullOrEmpty(workflowTransition.NotifySubject))
                {
                    this.txtNotifySubject.Text = "";
                }
                else
                {
                    this.txtNotifySubject.Text = workflowTransition.NotifySubject;
                }

                //Load available fields for specified template.
                List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveAll((int)Artifact.ArtifactTypeEnum.Incident, true, true);
                //--Strip ID from the end of any that have the id.
                for (int i = 0; i < artifactFields.Count; i++)
                {
                    if (artifactFields[i].Name.EndsWith("Id"))
                        artifactFields[i].Name = artifactFields[i].Name.Replace("Id", "");
                }
                //--Add standard fields.
                artifactFields.Add(new ArtifactField(0, 1, (int)Artifact.ArtifactTypeEnum.Incident, "URL", "", false, true, false, false, 0, false, false, true, Resources.Main.Notification_URL));
                artifactFields.Add(new ArtifactField(0, 1, (int)Artifact.ArtifactTypeEnum.Incident, "NotifyTo", "", false, true, false, false, 0, false, false, true, Resources.Main.Notification_NotifyTo));
                artifactFields.Add(new ArtifactField(0, 1, (int)Artifact.ArtifactTypeEnum.Incident, "Product", "", false, true, false, false, 0, false, false, true, Resources.Main.Notification_Product));
                artifactFields.Add(new ArtifactField(0, 1, (int)Artifact.ArtifactTypeEnum.Incident, "ProjectName", "", false, true, false, false, 0, false, false, true, Resources.Main.Notification_Project));
                artifactFields.Add(new ArtifactField(0, 1, (int)Artifact.ArtifactTypeEnum.Incident, "ID#", "", false, true, false, false, 0, false, false, true, Resources.Main.Notification_ID));
                //Display it.
                this.grdFields.DataSource = artifactFields;

                this.litTokenHeader.Text = string.Format(Resources.Messages.Admin_Notification_EditTemplateTokens, Resources.Fields.Incident);

                //Databind the form
                this.DataBind();

				//Set the static execute/notify flag values
                this.chkExecuteByDetectorYn.Checked = (workflowTransition.IsExecuteByDetector) ? true : false;
                this.chkExecuteByOwnerYn.Checked = (workflowTransition.IsExecuteByOwner) ? true : false;
                this.chkNotifyDetectorYn.Checked = (workflowTransition.IsNotifyDetector) ? true : false;
                this.chkNotifyOwnerYn.Checked = (workflowTransition.IsNotifyOwner) ? true : false;
			}

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>
        /// Redirects back to the workflow details page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "IncidentWorkflowDetails") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId);
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
		/// Called when the list of possible notify roles is data-bound
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
        private void grdNotifyRoles_RowDataBound(object sender, GridViewRowEventArgs e)
		{
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Get a handle to the checkbox in the grid
                CheckBoxEx checkBox = (CheckBoxEx)e.Row.FindControl("chkNotifyRole");
                if (checkBox != null)
                {
                    //See if we have this project role in the list execute dataset
                    int projectRoleId = Int32.Parse(checkBox.MetaData);
                    if (workflowTransition.TransitionRoles.Any(r => r.Role.ProjectRoleId == projectRoleId && r.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify))
                    {
                        checkBox.Checked = true;
                    }
                }
            }
		}

        void grdFields_ItemDataBound(object sender, System.Web.UI.WebControls.DataListItemEventArgs e)
        {
            //The item is bound, get and populate the desc label.
            //Add the onClick function to the item.
            HtmlAnchor aTokenClick = (HtmlAnchor)e.Item.FindControl("aTokenClick");
            if (aTokenClick != null)
            {
                aTokenClick.Attributes.Add("onclick", "javascript:insert_token('${" + (string)((ArtifactField)e.Item.DataItem).Name + "}');");
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

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

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
            WorkflowManager workflowManager = new WorkflowManager();
            this.workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
            this.workflowTransition.StartTracking();

            //Update the name, signature and notification fields
            this.workflowTransition.Name = this.txtTransitionName.Text.Trim();
            this.workflowTransition.IsSignatureRequired = this.chkRequireSignature.Checked;
            if (String.IsNullOrWhiteSpace(this.txtNotifySubject.Text))
            {
                this.workflowTransition.NotifySubject = "";
            }
            else
            {
                this.workflowTransition.NotifySubject = this.txtNotifySubject.Text.Trim();
            }

            //First we need to check the opener/assignee drop-downs and update
            this.workflowTransition.IsExecuteByDetector = (this.chkExecuteByDetectorYn.Checked);
            this.workflowTransition.IsExecuteByOwner = (this.chkExecuteByOwnerYn.Checked);
            this.workflowTransition.IsNotifyDetector = (this.chkNotifyDetectorYn.Checked);
            this.workflowTransition.IsNotifyOwner = (this.chkNotifyOwnerYn.Checked);

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
                    WorkflowTransitionRole workflowTransitionRole = this.workflowTransition.TransitionRoles.FirstOrDefault(r => r.ProjectRoleId == projectRoleId && r.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute);

                    //See if we have a matching row and either add/or delete if changes needed
                    if (workflowTransitionRole == null && chkExecuteRole.Checked)
                    {
                        //Add the role since it's not already there
                        workflowTransitionRole = new WorkflowTransitionRole();
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

            //Now iterate through the notify rows and get the id and various control values
            foreach (GridViewRow gvr in this.grdNotifyRoles.Rows)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (gvr.RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    CheckBoxEx chkNotifyRole = (CheckBoxEx)gvr.FindControl("chkNotifyRole");

                    //Now get the project role id
                    int projectRoleId = Int32.Parse(chkNotifyRole.MetaData);

                    //Find the matching row in the entity collection
                    WorkflowTransitionRole workflowTransitionRole = this.workflowTransition.TransitionRoles.FirstOrDefault(r => r.ProjectRoleId == projectRoleId && r.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify);

                    //See if we have a matching row and either add/or delete if changes needed
                    if (workflowTransitionRole == null && chkNotifyRole.Checked)
                    {
                        //Add the role since it's not already there
                        workflowTransitionRole = new WorkflowTransitionRole();
                        workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify;
                        workflowTransitionRole.ProjectRoleId = projectRoleId;
                        this.workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                    }
                    if (workflowTransitionRole != null && !chkNotifyRole.Checked)
                    {
                        //Delete the role since it shouldn't be there any more
                        workflowTransitionRole.MarkAsDeleted();
                    }
                }
            }

            //Now commit the changes
            workflowManager.WorkflowTransition_Update(this.workflowTransition);

            //Display a confirmation message
            this.lblMessage.Text = Resources.Messages.Admin_WorkflowTransition_SettingsSaved;
            this.lblMessage.Type = MessageBox.MessageType.Information;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
		}

		#endregion
	}
}
