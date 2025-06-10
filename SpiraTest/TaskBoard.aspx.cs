using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// planning board for the current release and handling all raised events
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Tasks, "SiteMap_TaskBoard", "Task-Tracking/#task-board")]
    public partial class TaskBoard : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TaskBoard::";

		public string BulkEditStatus { get; private set; } = "true";

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //We need to make sure that the system is licensed for planning boards (SpiraTeam or SpiraPlan only)
            if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidLicense, true);
            }

            //Redirect if the user has limited view permissions for releases. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }
            //Redirect if the user has limited view permissions for tasks. Grant access to sys admins if they are a member of the project in any way
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //Populate the user and project id in the planning board
            this.ajxPlanningBoard.ProjectId = this.ProjectId;

            //Specify the context for the ajax form control
            this.ajxFormManager.ProjectId = this.ProjectId;
            Dictionary<string, string> formManagerHandlers = new Dictionary<string, string>();
            formManagerHandlers.Add("dataSaved", "ajxFormManager_dataSaved");
            formManagerHandlers.Add("loaded", "ajxFormManager_loaded");
            this.ajxFormManager.SetClientEventHandlers(formManagerHandlers);

            //Only load the data once
            if (!IsPostBack)
            {
                //Get the stored settings
                int selectedReleaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -2);
                bool includeDetails = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_DETAILS, true);
                int groupByOption = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_GROUP_BY_OPTION, (int)ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus);

                //Update the setting that we're using the board view if it's not already set
                string listPage = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
                if (listPage != "Board")
                {
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "Board");
                }

                //Populate the list of releases and iterations and databind
                //Also need to add the 'All Releases' entry
                ReleaseManager releaseManager = new ReleaseManager();
                List<ReleaseView> releases = releaseManager.RetrieveByProjectId(this.ProjectId, true, true);
                ReleaseView allReleasesRow = new ReleaseView();
                allReleasesRow.FullName = Resources.Dialogs.Global_AllReleases2;
                allReleasesRow.Name = allReleasesRow.FullName;
                allReleasesRow.ReleaseId = -2;
                allReleasesRow.IndentLevel = "";
                allReleasesRow.IsSummary = false;
                allReleasesRow.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Planned;
                allReleasesRow.ReleaseTypeId = (int)Release.ReleaseTypeEnum.MajorRelease;
                releases.Insert(0, allReleasesRow);
                this.ddlSelectRelease.DataSource = releases;

                //Make sure our selected release still exists
                if (!releases.Any(r => r.ReleaseId == selectedReleaseId))
                {
                    //This occurs if the release has been subsequently deleted. In which case we need to update
                    //the stored settings
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                    selectedReleaseId = -1;
                }

                //Populate the standard fields of the task creation dialog
                this.ddlRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);
                TaskManager taskManager = new TaskManager();
                List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(ProjectTemplateId);
                List<TaskType> types = taskManager.TaskType_Retrieve(ProjectTemplateId);
				List<TaskStatus> statuses = taskManager.RetrieveStatusesInUse(ProjectTemplateId);
				List<DataModel.User> allUsers = new UserManager().RetrieveForProject(this.ProjectId);
                List<DataModel.User> activeUsers = new UserManager().RetrieveActiveByProjectId(this.ProjectId);
                List<ReleaseView> releases2 = releaseManager.RetrieveByProjectId(this.ProjectId, false, true);
                this.ddlPriority.DataSource = priorities;
                this.ddlCreator.DataSource = allUsers;
                this.ddlOwner.DataSource = activeUsers;
                this.ddlRelease.DataSource = releases2;
                this.ddlType.DataSource = types;
				this.ddlStatus.DataSource = statuses;

				//Add the various custom properties to the table of fields
				UnityCustomPropertyInjector.CreateControls(
                    ProjectId,
                    ProjectTemplateId,
                    DataModel.Artifact.ArtifactTypeEnum.Task,
                    this.customFieldsDefault,
                    this.ajxFormManager,
                    this.customFieldsUsers,
                    this.customFieldsDates,
                    this.customFieldsRichText
                );

                //See if we have an iteration
                bool isIteration = false;
                if (releases.Any(r => r.ReleaseId == selectedReleaseId && r.IsIteration))
                {
                    isIteration = true;
                }

                //Load the list of group by options (depends on release selection)
                List<ServerControls.PlanningBoard.GroupByOption> groupByOptions = this.ajxPlanningBoard.GetTaskBoardGroupByOptions((selectedReleaseId == -1), (selectedReleaseId == -2), (selectedReleaseId > 0), isIteration);
                this.ddlGroupBy.DataSource = groupByOptions;

                //Specify the project on the rich text editors
                this.txtDescription.Screenshot_ProjectId = ProjectId;

				//Set the bulk edit status flag
				ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(ProjectTemplateId);
				if (!projectTemplateSettings.Workflow_BulkEditCanChangeStatus)
				{
					BulkEditStatus = "false";
				}

				//Databind the controls
				this.DataBind();

                //Set the selected release
                if (selectedReleaseId == -1)
                {
                    this.ddlSelectRelease.SelectedValue = "";
                }
                else
                {
                    try
                    {
                        this.ddlSelectRelease.SelectedValue = selectedReleaseId.ToString();
                    }
                    catch (Exception)
                    {
                        //This occurs if the release has been subsequently deleted. In which case we need to update
                        //the stored settings
                        SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                        selectedReleaseId = -1;
                    }
                }

                //Set the group-by dropdown and option
                if (groupByOption > 0)
                {
                    //Make sure we have this in the list of active options
                    if (groupByOptions.Any(g => g.Id == groupByOption && g.IsActive))
                    {
                        this.ajxPlanningBoard.GroupBy = (ServerControls.PlanningBoard.PlanningGroupByOptions)groupByOption;
                        this.ddlGroupBy.SelectedValue = groupByOption.ToString();
                    }
                }

                //Set the 'include artifact' checkboxes
                this.chkIncludeDetails.Checked = includeDetails;

                //Set the options on the planning board
                this.ajxPlanningBoard.ReleaseId = selectedReleaseId;
                this.ajxPlanningBoard.IncludeDetails = includeDetails;
                this.ajxPlanningBoard.IsIteration = isIteration;

                //Add any client-side event handlers
                Dictionary<string, string> planningBoardHandlers = new Dictionary<string, string>();
                planningBoardHandlers.Add("addItem", "ajxPlanningBoard_addItem");
				planningBoardHandlers.Add("editItem", "ajxPlanningBoard_editItem");
				this.ajxPlanningBoard.SetClientEventHandlers(planningBoardHandlers);

                //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            //Reset the error message
            this.lblMessage.Text = "";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
    }
}
