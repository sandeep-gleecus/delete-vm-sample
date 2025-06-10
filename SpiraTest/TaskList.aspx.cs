using System;
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

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// List of tasks and handling all raised events
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Tasks, "SiteMap_Tasks", "Task-Tracking/#task-list")]
    public partial class TaskList : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TaskList::";

        protected int taskCount;

        #region Event Handlers

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Redirect if the user has limited view permissions for this artifact type. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //If we have SpiraTest, hide the list/board selector (since board view is not available)
            if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                this.plcListBoardSelector.Visible = false;
            }
            //Update the setting that we're using the table view if it's not already set
            string listPage = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
            if (listPage != "Table")
            {
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "Table");
            }

            //Populate the user and project id in the grid control
            this.grdTaskList.ProjectId = this.ProjectId;
            this.grdTaskList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -2);
            this.grdTaskList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TASK;
            this.grdTaskList.FolderUrlTemplate = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -4, GlobalFunctions.ARTIFACT_ID_TOKEN);

            //Specify the context for the quick-filters sidebar
            this.pnlQuickFilters.ProjectId = ProjectId;
            this.pnlQuickFilters.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlQuickFilters.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Specify the context for the charts sidebar
            this.pnlCharts.ProjectId = ProjectId;
            this.pnlCharts.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlCharts.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Custom CSS for the grid
            Dictionary<string, string> taskCssClasses = new Dictionary<string, string>();
            taskCssClasses.Add("ReleaseId", "priority2");
            this.grdTaskList.SetCustomCssClasses(taskCssClasses);

            //Specify the context for the folders sidebar
            this.pnlFolders.ProjectId = ProjectId;
            this.pnlFolders.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlFolders.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Only load the data once
            if (!IsPostBack)
            {
                //Instantiate the business classes
                Business.TaskManager task = new Business.TaskManager();

                //Databind the list/board selector
                this.plcListBoardSelector.DataBind();

                //Populate the list of columns to show/hide and databind
                this.ddlShowHideColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.Task);
                this.ddlShowHideColumns.DataBind();

                //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            // Set the context of the folders list
            this.trvFolders.NodeLegendControlId = this.txtFolderInfo.ID;
            this.trvFolders.ContainerId = this.ProjectId;
            this.trvFolders.PageUrlTemplate = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -4, GlobalFunctions.ARTIFACT_ID_TOKEN);

            //See if a folder was specified through the URL
            TaskManager taskManager = new TaskManager();
            int selectedFolder = 0; //Root
            if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_FOLDER_ID]))
            {
                int queryStringFolderId;
                if (Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_FOLDER_ID], out queryStringFolderId))
                {
                    //Check the folder exists - if not reset to root
                    selectedFolder = taskManager.TaskFolder_Exists(ProjectId, queryStringFolderId) ? queryStringFolderId : 0;

                    //In this case we also need to add it to the grid as a 'standard filter' so it does not get overriden by a saved setting
                    Dictionary<string, object> folderFilter = new Dictionary<string, object>();
                    folderFilter.Add(GlobalFunctions.SPECIAL_FILTER_FOLDER_ID, selectedFolder);
                    this.grdTaskList.SetFilters(folderFilter);

                    //Update the user settings to mark this as the selected folder
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, selectedFolder);
                }
            }
            else
            {
                //See if we have a stored node that we need to populate, use zero(0) otherwise so that the root is selected by default
                selectedFolder = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (selectedFolder < 1)
                {
                    selectedFolder = 0; //Makes sure that 'root' is selected
                }
                //If the folder does not exist reset to root and update settings
                else if (!taskManager.TaskFolder_Exists(ProjectId, selectedFolder))
                {
                    selectedFolder = 0; //Makes sure that 'root' is selected
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                }
            }
            this.trvFolders.SelectedNodeId = selectedFolder.ToString();

            //See if we should have dragging (of tasks to folders) enabled
            //Requires that the user has permissions to Modify tasks (not limited modify)
            bool canBulkEditTasks = false;
            if (UserIsAdmin)
            {
                canBulkEditTasks = true;
            }
            else if (new ProjectManager().IsAuthorized(ProjectRoleId, Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.Modify) == Project.AuthorizationState.Authorized)
            {
                canBulkEditTasks = true;
            }
            this.trvFolders.AllowDragging = canBulkEditTasks;
            this.grdTaskList.AllowDragging = canBulkEditTasks;
            if (canBulkEditTasks)
            {
                //Client-side handlers on the treeview
                Dictionary<string, string> handlers2 = new Dictionary<string, string>();
                handlers2.Add("itemDropped", "trvFolders_dragEnd");
                this.trvFolders.SetClientEventHandlers(handlers2);
            }

            //Client-side handlers on the grid
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("focusOn", "grdTaskList_focusOn");
            handlers.Add("loaded", "grdTaskList_loaded");
            this.grdTaskList.SetClientEventHandlers(handlers);

            //Reset the error message
            this.divMessage.Text = "";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();

        }

        #endregion
    }



}
