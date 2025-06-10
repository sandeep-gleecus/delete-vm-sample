using System;
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
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// List of test cases and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.TestCases, "SiteMap_TestCases", "Test-Case-Management/#test-case-list")]
	public partial class TestCaseList : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TestCaseList::";

        private ProjectSettings projectSettings = null;

        #region Properties

        /// <summary>
        /// Returns the base url for redirecting to a created pending test run entry
        /// </summary>
        protected string TestRunsPendingUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestExecute, -2, -2) + "?" + GlobalFunctions.PARAMETER_REFERER_TEST_CASE_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE);
            }
        }

        /// <summary>
        /// Returns the base url for redirecting to a created pending test run entry - when execution should be in exploratory mode
        /// </summary>
        protected string TestRunsPendingExploratoryUrl
        {
            get
            {
                //The referrer is the Test Case list because the test execution page doesn't currently handle returning to the test case details page
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestExecuteExploratory, -2, -2) + "?" + GlobalFunctions.PARAMETER_REFERER_TEST_CASE_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE);
            }
        }

        #endregion

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

            //Redirect if the user has limited view permissions for this artifact type. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //Populate the user and project id in the grid control
            this.grdTestCaseList.ProjectId = this.ProjectId;
			this.grdTestCaseList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, -2);
            this.grdTestCaseList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE;
            this.grdTestCaseList.FolderUrlTemplate = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, 0, GlobalFunctions.ARTIFACT_ID_TOKEN);

            // Set the context of the folders list
            this.trvFolders.NodeLegendControlId = this.txtFolderInfo.ID;
            this.trvFolders.ContainerId = this.ProjectId;
            this.trvFolders.PageUrlTemplate = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, 0, GlobalFunctions.ARTIFACT_ID_TOKEN);

            //Specify the context for the quick-filters sidebar
            this.pnlQuickFilters.ProjectId = ProjectId;
            this.pnlQuickFilters.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlQuickFilters.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Specify the context for the folders sidebar
            this.pnlFolders.ProjectId = ProjectId;
            this.pnlFolders.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlFolders.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Specify the context for the charts sidebar
            this.pnlCharts.ProjectId = ProjectId;
            this.pnlCharts.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlCharts.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Custom CSS for the grid
            Dictionary<string, string> testCaseCssClasses = new Dictionary<string, string>();
            testCaseCssClasses.Add("TestCaseStatusId", "priority2");
            testCaseCssClasses.Add("OwnerId", "priority3");
            testCaseCssClasses.Add("ExecutionDate", "priority4");
            this.grdTestCaseList.SetCustomCssClasses(testCaseCssClasses);

            //See if a folder was specified through the URL
            TestCaseManager testCaseManager = new TestCaseManager();
            int selectedFolder = 0; //Root
            if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_FOLDER_ID]))
            {
                int queryStringFolderId;
                if (Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_FOLDER_ID], out queryStringFolderId))
                {
                    //Check the folder exists - if not reset to root
                    selectedFolder = testCaseManager.TestCaseFolder_Exists(ProjectId, queryStringFolderId) ? queryStringFolderId : 0;

                    //In this case we also need to add it to the grid as a 'standard filter' so it does not get overriden by a saved setting
                    Dictionary<string, object> folderFilter = new Dictionary<string, object>();
                    folderFilter.Add(GlobalFunctions.SPECIAL_FILTER_FOLDER_ID, selectedFolder);
                    this.grdTestCaseList.SetFilters(folderFilter);

                    //Update the user settings to mark this as the selected folder
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, selectedFolder);
                }
            }
            else
            {
                //See if we have a stored node that we need to populate, use zero(0) otherwise so that the root is selected by default
                selectedFolder = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                if (selectedFolder < 1)
                {
                    selectedFolder = 0; //Makes sure that 'root' is selected
                }
                //If the folder does not exist reset to root and update settings
                else if (!testCaseManager.TestCaseFolder_Exists(ProjectId, selectedFolder))
                {
                    selectedFolder = 0; //Makes sure that 'root' is selected
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                }
            }
            this.trvFolders.SelectedNodeId = selectedFolder.ToString();

            //See if we should have dragging (of test cases to folders) enabled
            //Requires that the user has permissions to Modify test cases (not limited modify)
            bool canBulkEditTestCases = false;
            if (UserIsAdmin)
            {
                canBulkEditTestCases = true;
            }
            else if (new ProjectManager().IsAuthorized(ProjectRoleId, Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify) == Project.AuthorizationState.Authorized)
            {
                canBulkEditTestCases = true;
            }
            this.trvFolders.AllowDragging = canBulkEditTestCases;
            this.grdTestCaseList.AllowDragging = canBulkEditTestCases;

            //Client-side handlers on the grid
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("focusOn", "grdTestCaseList_focusOn");
            handlers.Add("loaded", "grdTestCaseList_loaded");
            this.grdTestCaseList.SetClientEventHandlers(handlers);

            //Client-side handlers on the treeview
            Dictionary<string, string> handlers2 = new Dictionary<string, string>();
            handlers2.Add("itemDropped", "trvFolders_dragEnd");
            this.trvFolders.SetClientEventHandlers(handlers2);

            //Add the URL to the release hierarchical drop-down
            this.ddlSelectRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);

            //Get the ProjectSettings collection
            if (ProjectId > 0)
            {
                projectSettings = new ProjectSettings(ProjectId);
            }
            if (projectSettings != null)
            {
                //WorX - Add the worx menu - needs to be configurable
                if (projectSettings.Testing_WorXEnabled)
                {
                    this.plcWorX.Visible = true;
                    this.mnuWorX.DropMenuItems.Add(new DropMenuItem() { Name = "Record", Value = "Record", GlyphIconCssClass="mr3 fas fa-circle", NavigateUrl = "worx:spira/record/pr" + ProjectId });
                    this.mnuWorX.DropMenuItems.Add(new DropMenuItem() { Name = "Design", Value = "Design", GlyphIconCssClass = "fas fa-cubes", NavigateUrl = "worx:spira/design/pr" + ProjectId });

                }
                //Disable test execution - by removing the execute buttons
                if (projectSettings.Testing_ExecuteSetsOnly)
                {
                    this.btnTools.DropMenuItems.Remove(btnTools_execute);
                    this.grdTestCaseList.ContextMenuItems.Remove(grdTestCaseList_contextExecute);
                }
            }

			//Only load the data once
			if (!IsPostBack) 
			{
                //Instantiate the business classes
                Business.ReleaseManager releaseManager = new Business.ReleaseManager();
                
                //Get the current test case release filter
                int passedInReleaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
 
                //Populate the list of releases and databind. We include inactive ones and let the dropdown list filter by active
                //that ensures that a legacy filter is displayed even if it is no longer selectable now
                List<ReleaseView> releases = releaseManager.RetrieveByProjectId(this.ProjectId, false);
                this.ddlSelectRelease.DataSource = releases;
                this.ddlSelectRelease.DataBind();
                if (passedInReleaseId == -1)
                {
                    this.ddlSelectRelease.SelectedValue = "";
                }
                else
                {
                    try
                    {
                        this.ddlSelectRelease.SelectedValue = passedInReleaseId.ToString();
                    }
                    catch (Exception)
                    {
                        //This occurs if the release has been subsequently deleted. In which case we need to update
                        //the stored settings
                        SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                    }
                }

                //Populate the list of columns to show/hide and databind
                this.ddlShowHideColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.TestCase);
                this.ddlShowHideColumns.DataBind();

                //Databind the artifact dropdown used in Tools
                this.ddlCustomOperationArtifact.DataBind();

                //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");

                //Add the client event handler to the background task process
                Dictionary<string, string> backgroundHandlers = new Dictionary<string, string>();
                backgroundHandlers.Add("succeeded", "ajxBackgroundProcessManager_success");
                this.ajxBackgroundProcessManager.SetClientEventHandlers(backgroundHandlers);
			}	

			//Reset the error message
			this.divMessage.Text = ""; //Used for client-side controls


			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
		
		#endregion
	}
}
