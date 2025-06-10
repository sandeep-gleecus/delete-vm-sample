using System;
using System.Data;
using System.Web.UI.WebControls;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System.Collections;
using System.Collections.Generic;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// details of a particular task and handling updates
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Tasks, null, "Task-Tracking/#task-details", "TaskDetails_Title")]
	public partial class TaskDetails : PageLayout
	{
		protected int taskId;
		protected string ArtifactTabName = null;

		/// <summary>
		/// Is source code enabled
		/// </summary>
		protected bool IsActiveForSourceCode
		{
			get;
			set;
		}

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TaskDetails::";


		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Capture the passed task id from the querystring
			this.taskId = System.Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_TASK_ID]);

            //Make sure that the current project is set to the project associated with the artifact
            //this is important since the page may get loaded from an email notification URL or cross-project association
            VerifyArtifactProject(UrlRoots.NavigationLinkEnum.Tasks, this.taskId);

			//Determine if the task description should be rich-text or not
			this.txtNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);

            //Set the permissions and action on the Add Comment button
            this.btnNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);
            this.btnNewComment.ClientScriptMethod = String.Format("add_comment('{0}')", this.txtNewComment.ClientID);

			//Add the URL to the release hierarchical drop-downs and artifact hyperlinks
			this.ddlRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);
			this.lnkRequirement.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -2);
            this.lnkRisk.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, ProjectId, -2);

            //Only load the data once
            if (!IsPostBack)
			{
				LoadAndBindData();
			}

			//Add the various panel event handlers and message references
			this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;
			this.tstAssociationPanel.MessageLabelHandle = this.lblMessage;

			//Reset the error message
			this.lblMessage.Text = "";

            //Specify the context for the navigation bar
            this.navTaskList.ProjectId = ProjectId;

            //Specify the context for the ajax form control
            this.ajxFormManager.ProjectId = this.ProjectId;
            this.ajxFormManager.PrimaryKey = this.taskId;
            this.ajxFormManager.ArtifactTypePrefix = Task.ARTIFACT_PREFIX;
            this.ajxFormManager.FolderPathUrlTemplate = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -4, GlobalFunctions.ARTIFACT_ID_TOKEN));
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("dataSaved", "ajxFormManager_dataSaved");
            handlers.Add("loaded", "ajxFormManager_loaded");
            handlers.Add("operationReverted", "ajxFormManager_operationReverted");
            this.ajxFormManager.SetClientEventHandlers(handlers);

            //Set the client-side handlers on the requirement artifact link
            Dictionary<string, string> handlers2 = new Dictionary<string, string>();
            handlers2.Add("changeClicked", "lnkRequirement_changeClicked");
            this.lnkRequirement.SetClientEventHandlers(handlers2);

            //Set the context on the requirements selector
            this.ajxRequirementsSelector.ProjectId = this.ProjectId;

            //Specify the context for the folders sidebar
            this.pnlFolders.ProjectId = ProjectId;
            this.pnlFolders.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlFolders.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

            // Set the context of the folders list
            this.trvFolders.ClientScriptServerControlId = this.navTaskList.UniqueID;
            this.trvFolders.ContainerId = this.ProjectId;

            //Populate the project on the source code revisions (for pull requests)
            this.grdSourceCodeRevisionList.ProjectId = this.ProjectId;
			this.grdSourceCodeRevisionList.DisplayTypeId = (int)Artifact.DisplayTypeEnum.PullRequest_Revisions;

			//Register client events handlers on grid
			Dictionary<string, string> grdSourceCodeRevisionList_handlers = new Dictionary<string, string>();
            grdSourceCodeRevisionList_handlers.Add("loaded", "grdSourceCodeRevisionList_loaded");
            this.grdSourceCodeRevisionList.SetClientEventHandlers(grdSourceCodeRevisionList_handlers);

            //See if we have a stored node that we need to populate, use zero(0) otherwise so that the root is selected by default
            TaskManager taskManager = new TaskManager();
            int selectedFolder = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
            if (selectedFolder < 1)
            {
                selectedFolder = 0; //Makes sure that 'root' is selected
            }
            //If the folder does not exist reset to root and update settings-
            else if (!taskManager.TaskFolder_Exists(this.ProjectId, selectedFolder))
            {
                selectedFolder = 0; //Makes sure that 'root' is selected
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
            }
            this.trvFolders.SelectedNodeId = selectedFolder.ToString();

            //Set the context on the comment list
            this.lstComments.ProjectId = ProjectId;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Loads and binds the task details and general tab
		/// </summary>
		private void LoadAndBindData()
		{
			//Load any lookups
            Business.TaskManager taskManager = new Business.TaskManager();
            Business.UserManager userManager = new Business.UserManager();
            Business.RequirementManager requirementManager = new Business.RequirementManager();
            ReleaseManager releaseManager = new ReleaseManager();
            List<DataModel.User> allUsers = new UserManager().RetrieveForProject(this.ProjectId);
            List<DataModel.User> activeUsers = new UserManager().RetrieveActiveByProjectId(this.ProjectId);
            List<TaskPriority> taskPriorities = taskManager.TaskPriority_Retrieve(ProjectTemplateId);
            List<TaskType> taskTypes = taskManager.TaskType_Retrieve(ProjectTemplateId);
            List<ReleaseView> releases = releaseManager.RetrieveByProjectId(this.ProjectId, false);
            this.ddlPriority.DataSource = taskPriorities;
            this.ddlType.DataSource = taskTypes;
            this.ddlCreator.DataSource = allUsers;
            this.ddlOwner.DataSource = activeUsers;
            this.ddlSplitTaskOwner.DataSource = activeUsers;
            this.ddlRelease.DataSource = releases;

			//See if source code is enabled for this project
			IsActiveForSourceCode = new SourceCodeManager().IsActiveProvidersForProject(ProjectId);


			//Set the context on the workflow operations control itself
			Dictionary<string, string> handlers2 = new Dictionary<string, string>();
            handlers2.Add("operationExecuted", "ajxWorkflowOperations_operationExecuted");
            this.ajxWorkflowOperations.ProjectId = ProjectId;
            this.ajxWorkflowOperations.PrimaryKey = this.taskId;
            this.ajxWorkflowOperations.SetClientEventHandlers(handlers2);

            //Populate the link back to the list page and item base url
            this.navTaskList.ListScreenUrl = ReturnToTasksListUrl;
            this.navTaskList.SelectedItemId = this.taskId;
            if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
            {
                //If we came from the 'My Page' we need to set the navigation to 'My Assigned Tasks'
                this.navTaskList.DisplayMode = NavigationBar.DisplayModes.Assigned;
                ProjectSettingsCollection settings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION);
                settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = (int)NavigationBar.DisplayModes.Assigned;
                settings.Save();
            }
            else if ((Request.QueryString[GlobalFunctions.PARAMETER_REFERER_REQUIREMENT_DETAILS] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_REQUIREMENT_DETAILS] == GlobalFunctions.PARAMETER_VALUE_TRUE)
                || (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_RELEASE_DETAILS] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_RELEASE_DETAILS] == GlobalFunctions.PARAMETER_VALUE_TRUE))
            {
                //If we came from the requirement or release details pages we need to show 'All Tasks'
                //since that includes the requirements and releases
                this.navTaskList.DisplayMode = NavigationBar.DisplayModes.AllItems;
                ProjectSettingsCollection settings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION);
                settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = (int)NavigationBar.DisplayModes.AllItems;
                settings.Save();
            }
            else
            {
                //Otherwise use the last setting the user used
                this.navTaskList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
            }
            this.navTaskList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.navTaskList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

			//Specify the artifact type and artifact id to retrieve the attachments for
			this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Task;
            this.tstAttachmentPanel.ArtifactId = this.taskId;
            tstAttachmentPanel.LoadAndBindData(true);

			//Display any source code revision associations
			this.tstAssociationPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Task;
            this.tstAssociationPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.ArtifactLink;

			//Specify the artifact type and artifact id to retrieve the history log for
			this.tstHistoryPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Task;

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

            //Set the project/artifact for the RTE so that we can upload screenshots
            this.txtDescription.Screenshot_ProjectId = ProjectId;
            this.txtNewComment.Screenshot_ProjectId = ProjectId;

            //Databind the form
            this.DataBind();

			//See if a tab's been selected.
			if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
			{
				string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

				switch (tabRequest)
				{
					case GlobalFunctions.PARAMETER_TAB_ATTACHMENTS:
						tclTaskDetails.SelectedTab = this.pnlAttachments.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

                    case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
                        tclTaskDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_HISTORY:
						tclTaskDetails.SelectedTab = this.pnlHistory.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_ASSOCIATION:
						tclTaskDetails.SelectedTab = this.pnlAssociations.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_REVISION:
						tclTaskDetails.SelectedTab = this.pnlRevisions.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					default:
						tclTaskDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
						break;
				}
			}

			//Specify the tab in the navigation control
			this.navTaskList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -2, ArtifactTabName);
		}

        /// <summary>
		/// Returns the URL to the tasks list, my page or project home depending on the referrer
		/// </summary>
        protected string ReturnToTasksListUrl
        {
            get
            {
                try
                {
                    //Retrieve the task to get the requirement id and release id
                    this.taskId = System.Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_TASK_ID]);
                    Business.TaskManager taskManager = new Business.TaskManager();
                    Task task = taskManager.RetrieveById(this.taskId);
                    int releaseId = -1;
                    if (task.ReleaseId.HasValue)
                    {
                        releaseId = task.ReleaseId.Value;
                    }
                    int requirementId = -1;
                    if (task.RequirementId.HasValue)
                    {
                        requirementId = task.RequirementId.Value;
                    }

                    //Redirect to the appropriate page
                    if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_RELEASE_DETAILS] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_RELEASE_DETAILS] == GlobalFunctions.PARAMETER_VALUE_TRUE && releaseId != -1)
                    {
                        return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, releaseId, GlobalFunctions.PARAMETER_TAB_TASK);
                    }
                    else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
                    {
                        return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0);
                    }
                    else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME] == GlobalFunctions.PARAMETER_VALUE_TRUE)
                    {
                        return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0);
                    }
                    else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_REQUIREMENT_DETAILS] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_REQUIREMENT_DETAILS] == GlobalFunctions.PARAMETER_VALUE_TRUE && requirementId != -1)
                    {
                        return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, requirementId, GlobalFunctions.PARAMETER_TAB_TASK);
                    }
                    else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_ITERATION_PLAN] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_ITERATION_PLAN] == GlobalFunctions.PARAMETER_VALUE_TRUE)
                    {
                        return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Iterations, ProjectId);
                    }
                    else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PLANNING_BOARD] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PLANNING_BOARD] == GlobalFunctions.PARAMETER_VALUE_TRUE)
                    {
                        return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.PlanningBoard, ProjectId);
                    }
                    else
                    {
                        return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId);
                    }
                }
                catch (ArtifactNotExistsException)
                {
                    //If the artifact doesn't exist let the user know nicely
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The task you selected has been deleted from the system.", true);
                    return "";
                }
            }
        }

        /// <summary>
        /// Returns the url to the generic task page with a token for the task id
        /// </summary>
        protected string TaskRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -2));
            }
        }

		/// <summary>
		/// Redirects back to the parent release/iteration or requirement respectively
		/// </summary>
		private void ReturnToParentArtifact()
		{
            Response.Redirect(ReturnToTasksListUrl, true);
		}
	}
}
