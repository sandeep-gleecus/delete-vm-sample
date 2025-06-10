using System;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.UserControls.WebParts;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;
using Microsoft.Security.Application;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Project Home Page and handling all raised events
	/// </summary>
	[HeaderSettings(Inflectra.SpiraTest.Web.UserControls.GlobalNavigation.NavigationHighlightedLink.ProjectHome, null, "Product-Homepage")]
	public partial class ProjectHome : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ProjectHome::";

		#region Event Handlers

		/// <summary>
		/// Prevents items in the declarative catalog from being added more than once (which is the default behavior)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void prtManager_WebPartAdding(object sender, WebPartAddingEventArgs e)
		{
			// get element being added - used a base class to check each control can/not be added multiple times
			WebPartBase addElement = (WebPartBase)((GenericWebPart)e.WebPart).ChildControl;

			// get the new element's control pathname
			string path = addElement.GetType().Name;

			// check pathname against each element currently in the document
			foreach (WebPart part in prtManager.WebParts)
			{
				if (part is GenericWebPart)
				{
					GenericWebPart genericPart = (GenericWebPart)part;
					if (genericPart.ChildControl.GetType().Name.Equals(path) && !genericPart.IsClosed)
					{
						// found that the web part control has already been added, tell user and bail out of loop
						e.Cancel = true;
                        this.lblMessage.Text = Resources.Messages.Global_CannotAddWidgetMoreThanOnce;
						this.lblMessage.Type = MessageBox.MessageType.Error;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Adds the project id to the web part manager during Init phase
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		protected void prtManager_Init(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "prtManager_Init";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			if (Page.User.Identity.IsAuthenticated)
			{
				Business.UserManager userManager = new Business.UserManager();
                try
                {
                    DataModel.User user = userManager.GetUserByLogin(Page.User.Identity.Name);

                    //Make sure we have a project selected
                    if (user.Profile.LastOpenedProjectId.HasValue)
                    {
                        int projectId = user.Profile.LastOpenedProjectId.Value;

                        //Tell the web part manager that the settings are specific to this project
                        this.prtManager.DashboardInstanceId = projectId;
                    }
                }
                catch (ArtifactNotExistsException)
                {
                    //Do nothing
                }
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Called to validate that a webpart should be displayed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void prtManager_AuthorizeWebPart(object sender, WebPartAuthorizationEventArgs e)
		{
            const string METHOD_NAME = "prtManager_AuthorizeWebPart";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Default to authorized
			e.IsAuthorized = true;

			//Store the base path of all the user controls
			const string USER_CONTROL_BASE_PATH = "~/UserControls/WebParts/ProjectHome/";

			//Check the permissions for the user and hide the appropriate widgets
			ProjectManager projectManager = new ProjectManager();

            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }

            //Check to see if user is authorized to view incidents
            if (projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
			{
				//Make all incident related items hidden
				if (
					e.Path == USER_CONTROL_BASE_PATH + "OpenIssues.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "IncidentSummary.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "IncidentTestCoverage.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "IncidentOpenCount.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "IncidentAging.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "RequirementIncidentCount.ascx")
				{
					e.IsAuthorized = false;
				}
			}

			//Check to see if user is authorized to view tasks
            if (projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
			{
				//Make all task related items hidden
				if (e.Path == USER_CONTROL_BASE_PATH + "TasksLateStarting.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "TasksLateFinishing.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "ReleaseTaskProgress.ascx" ||
                    e.Path  == USER_CONTROL_BASE_PATH + "TaskGraphs.ascx")
				{
					e.IsAuthorized = false;
				}
			}

			//Check to see if user is authorized to view test cases
            if (projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
			{
				//Make all test related items hidden
				if (e.Path == USER_CONTROL_BASE_PATH + "TestExecutionStatus.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "RequirementsCoverageAll.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "RequirementsCoverageNew.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "IncidentTestCoverage.ascx" ||
                    e.Path == USER_CONTROL_BASE_PATH + "PendingTestRuns.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "ReleaseTestSummary.ascx")
				{
					e.IsAuthorized = false;
				}
			}

			//Check to see if user is authorized to view test sets
            if (projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
			{
				//Make all test set related items hidden
				if (e.Path == USER_CONTROL_BASE_PATH + "TestSetStatus.ascx")
				{
					e.IsAuthorized = false;
				}
			}

            //Check to see if user is authorized to view risks
            if (projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Risk, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                //Make all risk related items hidden
                if (e.Path == USER_CONTROL_BASE_PATH + "OpenRisks.ascx" || e.Path == USER_CONTROL_BASE_PATH + "RiskSummary.ascx")
                {
                    e.IsAuthorized = false;
                }
            }

            //Check to see if user is authorized to view requirements
            if (projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
			{
				//Make all requirement related items hidden
				if (e.Path == USER_CONTROL_BASE_PATH + "RequirementsSummary.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "RequirementsCoverageAll.ascx" ||
					e.Path == USER_CONTROL_BASE_PATH + "RequirementsCoverageNew.ascx" ||
                    e.Path == USER_CONTROL_BASE_PATH + "RequirementGraphs.ascx" ||
                    e.Path == USER_CONTROL_BASE_PATH + "ReleaseCompletion.ascx" ||
                    e.Path == USER_CONTROL_BASE_PATH + "ReleaseRelativeSize.ascx" ||
                    e.Path == USER_CONTROL_BASE_PATH + "Schedule.ascx" ||
                    e.Path == USER_CONTROL_BASE_PATH + "OverallCompletion.ascx" ||
                    e.Path == USER_CONTROL_BASE_PATH + "RequirementIncidentCount.ascx")
				{
					e.IsAuthorized = false;
				}
			}

			//Check to see if user is authorized to view releases
            if (projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
			{
				//Make all release related items hidden
				if (e.Path == USER_CONTROL_BASE_PATH + "ReleaseTaskProgress.ascx" ||
                    e.Path == USER_CONTROL_BASE_PATH + "RecentBuilds.ascx" ||
                    e.Path == USER_CONTROL_BASE_PATH + "ReleaseTestSummary.ascx")
				{
					e.IsAuthorized = false;
				}
			}

            //Check to see if user is authorized to view source code
            if (!projectManager.IsAuthorizedToViewSourceCode(projectRoleIdToCheck) || !Common.Global.Feature_SourceCode)
            {
                //Make all source code related items hidden
                if (e.Path == USER_CONTROL_BASE_PATH + "SourceCodeCommits.ascx")
                {
                    e.IsAuthorized = false;
                }
            }
        }

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Add the event handlers to the page
			this.ddlSelectRelease.SelectedIndexChanged += new EventHandler(ddlSelectRelease_SelectedIndexChanged);
			this.btnDesignView.Click += new EventHandler(btnDesignView_Click);
			this.btnBrowseView.Click += new EventHandler(btnBrowseView_Click);
			this.btnCustomize.Click += new EventHandler(btnCustomize_Click);
			this.prtManager.DisplayModeChanged += new WebPartDisplayModeEventHandler(prtManager_DisplayModeChanged);
			this.prtManager.WebPartAdding += new WebPartAddingEventHandler(prtManager_WebPartAdding);
			this.prtManager.WebPartAdded += new WebPartEventHandler(prtManager_WebPartAdded);
			this.prtManager.WebPartDrillDown += new WebPartEventHandler(prtManager_WebPartDrillDown);

			//Add the URL to the release hierarchical drop-down
			this.ddlSelectRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);

			//Load any controls that are not in webparts
			if (!IsPostBack)
			{
				//Display the project name and id
                this.lblProjectId.Text = GlobalFunctions.ARTIFACT_PREFIX_PROJECT + ProjectId;
                string projectName = ProjectName;
                this.lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(projectName);

                //Update the page title/description with the project id and name
                ((MasterPages.Main)((MasterPages.Dashboard)((MasterPages.ProjectDashboard)this.Master).Master).Master).PageTitle = GlobalFunctions.ARTIFACT_PREFIX_PROJECT + ProjectId + " - " + projectName;

				//Default the page to browse mode
				this.btnBrowseView.Visible = false;
				this.btnDesignView.Visible = true;
				this.btnCustomize.Visible = true;
				this.prtManager.DisplayMode = WebPartManager.BrowseDisplayMode;
                this.prtLeftZone.CssClass = "BrowseView";
                this.prtRightZone.CssClass = "BrowseView";
                this.prtTopZone.CssClass = "BrowseView";
                this.prtBottomZone.CssClass = "BrowseView";

                //Populate the list of releases and databind. We include inactive ones and let the dropdown list filter by active
                //that ensures that a legacy filter is displayed even if it is no longer selectable now
                Business.ReleaseManager releaseManager = new Business.ReleaseManager();
                List<ReleaseView> releases = releaseManager.RetrieveByProjectId(ProjectId, false);
                this.ddlSelectRelease.DataSource = releases;

                //Add the URLs for the 3 versions of the home page
                this.lnkGeneral.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId, 0, "General");
                this.lnkDevelopment.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId, 0, "Dev");
                this.lnkTesting.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId, 0, "Test");
                this.DataBind();

				//Reset the release drop-down, handing exceptions quietly (in case a release was made inactive)
				try
				{
                    int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
					if (releaseId < 1)
					{
						this.ddlSelectRelease.SelectedValue = "";
					}
					else
					{
						this.ddlSelectRelease.SelectedValue = releaseId.ToString();
					}
				}
				catch (Exception)
				{
					//This occurs if the release has been subsequently deleted. In which case we need to update
					//the stored settings
					SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
				}
			}

			//See if we have any error message passed from the calling page
			if (Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE] != null)
			{
				this.lblMessage.Text = Encoder.HtmlEncode(Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE]);
				this.lblMessage.Type = MessageBox.MessageType.Error;
			}
			else
			{
				this.lblMessage.Text = "";
			}

            //See if we need to update the preferred version of the project home page
            string preferredProjectHome = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
            if (preferredProjectHome != "General")
            {
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "General");
            }

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Called whenever the subtitle drilldown links are clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void prtManager_WebPartDrillDown(object sender, WebPartEventArgs e)
		{
			//See which webpart was clicked on and take the appropriate action
			if (e.WebPart != null && e.WebPart is GenericWebPart)
			{
				GenericWebPart genericWebPart = (GenericWebPart)e.WebPart;
				if (genericWebPart.ChildControl != null && genericWebPart.ChildControl is WebPartBase)
				{
					//First the parts that are already on the page by default
					WebPartBase userControl = (WebPartBase)genericWebPart.ChildControl;
					if (userControl == this.ucRequirementsSummary)
					{
						ucRequirementSummary_DrillDown();
					}
					if (userControl == this.ucRequirementsCoverageNew)
					{
						ucRequirementsCoverage_DrillDown();
					}
					if (userControl == this.ucTasksLateFinishing)
					{
						ucTasksLateFinishing_DrillDown();
					}
					if (userControl == this.ucOpenIssues)
					{
						ucIssueList_DrillDown((UserControls.WebParts.ProjectHome.OpenIssues)userControl);
					}
					if (userControl == this.ucOpenRisks)
					{
						ucRiskList_DrillDown();
					}
					if (userControl == this.ucIncidentSummary)
					{
						ucIncidentSummary_DrillDown();
					}
					if (userControl == this.ucRequirementIncidentCount)
					{
						ucRequirementSummary_DrillDown();
					}
					if (userControl == this.ucIncidentOpenCount)
					{
						ucIncidentOpenCount_DrillDown();
					}
                    if (userControl == this.ucTaskReports)
                    {
                        ucTaskReports_DrillDown();
                    }
                    if (userControl == this.ucActivityFeed)
                    {
                        ucActivityFeed_DrillDown();
                    }

                    //Now, any parts in the declarative catalog need to be located by their type as there is no initial reference
                    if (userControl is UserControls.WebParts.ProjectHome.TestExecutionStatus)
                    {
                        ucTestExecutionStatus_DrillDown();
                    }
                    if (userControl is UserControls.WebParts.ProjectHome.ReleaseTestSummary)
                    {
                        ucReleaseTestSummary_DrillDown();
                    }
                    if (userControl is UserControls.WebParts.ProjectHome.RequirementsCoverageAll)
					{
                        //Turn off redirecting because this graph doesn't actually match the RegressionCoverage widget
						//ucRequirementsCoverage_DrillDown();
					}
					if (userControl is UserControls.WebParts.ProjectHome.TestSetStatus)
					{
						ucTestSetStatus_DrillDown();
					}
					if (userControl is UserControls.WebParts.ProjectHome.IncidentAging)
					{
						ucIncidentAging_DrillDown();
					}
                    if (userControl is UserControls.WebParts.ProjectHome.TasksLateStarting)
                    {
                        ucTasksLateStarting_DrillDown();
                    }
                    if (userControl is UserControls.WebParts.ProjectHome.RisksSummary)
                    {
                        ucRiskList_DrillDown();
                    }
                    if (userControl is UserControls.WebParts.ProjectHome.ReleaseTaskProgress)
                    {
                        ucReleaseTaskProgress_DrillDown();
                    }

                }
            }
		}

		/// <summary>
		/// Called when a new web part has been added
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void prtManager_WebPartAdded(object sender, WebPartEventArgs e)
		{
			//If implements reloadable, then fire the LoadAndBind() method
			if (((GenericWebPart)e.WebPart).ChildControl is IWebPartReloadable)
			{
				IWebPartReloadable reloadableWebPart = (IWebPartReloadable)(((GenericWebPart)e.WebPart).ChildControl);
				reloadableWebPart.LoadAndBindData();
			}
		}

		/// <summary>
		/// Changes the skin if the display mode is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void prtManager_DisplayModeChanged(object sender, WebPartDisplayModeEventArgs e)
		{
            if (this.prtManager.DisplayMode.AllowPageDesign)
            {
                this.prtTopZone.CssClass = "DesignView";
                this.prtLeftZone.CssClass = "DesignView";
                this.prtRightZone.CssClass = "DesignView";
                this.prtBottomZone.CssClass = "DesignView";
            }
            else
            {
                this.prtTopZone.CssClass = "BrowseView";
                this.prtLeftZone.CssClass = "BrowseView";
                this.prtRightZone.CssClass = "BrowseView";
                this.prtBottomZone.CssClass = "BrowseView";
            }
        }

		/// <summary>
		/// Switches the dashboard to catalog view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnCustomize_Click(object sender, EventArgs e)
		{
			this.btnBrowseView.Visible = false;
			this.btnDesignView.Visible = true;
			this.prtManager.DisplayMode = WebPartManager.CatalogDisplayMode;
		}

		/// <summary>
		/// Switches the dashboard to browse view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnBrowseView_Click(object sender, EventArgs e)
		{
			this.btnBrowseView.Visible = false;
			this.btnDesignView.Visible = true;
			this.prtManager.DisplayMode = WebPartManager.BrowseDisplayMode;
		}

		/// <summary>
		/// Switches the dashboard to design view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnDesignView_Click(object sender, EventArgs e)
		{
			this.btnBrowseView.Visible = true;
			this.btnDesignView.Visible = false;
			this.prtManager.DisplayMode = WebPartManager.EditDisplayMode;
		}

		/// <summary>
		/// Redirects to the task list with late starting tasks displayed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ucTasksLateStarting_DrillDown()
		{
			//We need to redirect to the task list with filters and sorts applied
			ProjectSettingsCollection filters = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST);
			filters.Clear();
			//Add a release filter if one is specified
			int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
			if (releaseId != -1)
			{
				filters.Add("ReleaseId", releaseId);
			}
			filters.Add("ProgressId", 2);   //Progress = Started Late
			filters.Save();
			SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "TaskPriorityName ASC");
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, this.ProjectId), true);
		}

		/// <summary>
		/// Redirects to the task list with late finishing tasks displayed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ucTasksLateFinishing_DrillDown()
		{
			//We need to redirect to the task list with filters and sorts applied
			ProjectSettingsCollection filters = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST);
			filters.Clear();
			//Add a release filter if one is specified
			int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
			if (releaseId != -1)
			{
				filters.Add("ReleaseId", releaseId);
			}
			filters.Add("ProgressId", 4);   //Progress = Running Late
			filters.Save();
			SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "TaskPriorityName ASC");
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, this.ProjectId), true);
		}

		/// <summary>
		/// Called when the release filter drop-down-list is changed
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void ddlSelectRelease_SelectedIndexChanged(object sender, EventArgs e)
		{
			const string METHOD_NAME = "ddlSelectRelease_SelectedIndexChanged";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Capture the release id in project settings
			int releaseId;
			if (this.ddlSelectRelease.SelectedValue == "")
			{
				releaseId = -1;
			}
			else
			{
				releaseId = Int32.Parse(this.ddlSelectRelease.SelectedValue);
			}
			SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);

			//Now need to reload the widgets on the page
			ReloadWidgets(false);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Reloads all widgets on the page that implement IWebPartReloadable and sets the release dropdown
		/// </summary>
		/// <param name="setRelease">Should we update the release dropdown</param>
		public void ReloadWidgets(bool setRelease)
		{
			foreach (WebPart webPart in this.prtManager.WebParts)
			{
				if (webPart is GenericWebPart)
				{
					//Get the child control
					if (((GenericWebPart)webPart).ChildControl is IWebPartReloadable)
					{
						IWebPartReloadable webPartReloadable = (IWebPartReloadable)(((GenericWebPart)webPart).ChildControl);
						webPartReloadable.LoadAndBindData();
					}
				}
			}

			if (setRelease)
			{
				try
				{
					int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
					if (releaseId == -1)
					{
						this.ddlSelectRelease.SelectedValue = "";
					}
					else
					{
						this.ddlSelectRelease.SelectedValue = releaseId.ToString();
					}
				}
				catch (Exception)
				{
					//Fail quietly
				}
			}
		}

		/// <summary>
		/// Handles clicks on the View Details link
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void ucRequirementSummary_DrillDown()
		{
			//Simply redirect to the requirements list page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId), true);
		}

        /// <summary>
        /// Redirects to the activity page
        /// </summary>
        private void ucActivityFeed_DrillDown()
        {
            //We just redirect to the activity stream
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ActivityStream, ProjectId), true);
        }

		/// <summary>
		/// Handles clicks on the View Details link
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void ucRequirementsCoverage_DrillDown()
		{
			//We just redirect to the requirements list page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId), true);
		}

		/// <summary>
		/// Handles clicks on the View Details link
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void ucIncidentAging_DrillDown()
		{
			//We just redirect to the incident list page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId), true);
		}

		/// <summary>
		/// Handles clicks on the View Details link
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void ucIncidentOpenCount_DrillDown()
		{
			//We just redirect to the incident list page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId), true);
		}

		/// <summary>
		/// Handles clicks on the View Details link
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void ucTestSetStatus_DrillDown()
		{
			//We just redirect to the test set list page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId), true);
		}

		/// <summary>
		/// Handles clicks on the View All link
		/// </summary>
		/// <param name="userControl">The user control reference</param>
		private void ucIssueList_DrillDown(UserControls.WebParts.ProjectHome.OpenIssues userControl)
		{
			//We need to redirect to the incident list with filters and sorts applied
			ProjectSettingsCollection filters = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST);
            int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
            filters.Clear();
			//Set status=all open, type=all issues
            filters.Add("IncidentStatusId", IncidentManager.IncidentStatusId_AllOpen);
            filters.Add("IncidentTypeId", IncidentManager.IncidentTypeId_AllIssues);
            //Set the detected release if appropriate
            if (releaseId > 0)
            {
                filters.Add("DetectedReleaseId", releaseId);
            }
            filters.Save();
			//Sort by either priority or severity
			if (userControl.OrganizeBy == UserControls.WebParts.ProjectHome.OpenIssues.OpenIssueOrganizeBy.Severity)
			{
				SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "SeverityName ASC");
			}
			else
			{
				SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "PriorityName ASC");
			}
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, this.ProjectId), true);
		}

		/// <summary>
		/// Handles clicks on the View All link
		/// </summary>
		private void ucRiskList_DrillDown()
		{
			//We need to redirect to the risk list
			//Need to set the sorts and filters and then redirect
			ProjectSettingsCollection filters = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_FILTERS);
            int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
            filters.Clear();
			//Set status=all open, release = (current release)
            filters.Add("RiskStatusId", RiskManager.RiskStatusId_AllOpen);
            //Set the release if appropriate
            if (releaseId > 0)
            {
                filters.Add("ReleaseId", releaseId);
            }
			filters.Save();
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, ProjectId), true);
		}

		/// <summary>
		/// Handles clicks on the View Details link
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void ucIncidentSummary_DrillDown()
		{
			//Simply redirect to the incident list page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId), true);
		}

        /// <summary>
        /// Handles clicks on the View Details link
        /// </summary>
        private void ucTaskReports_DrillDown()
        {
            //Redirect to the Task list page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId), true);
        }

		/// <summary>
		/// Handles clicks on the View Details link
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void ucReleaseTestSummary_DrillDown()
		{
			//We simply need to redirect to the release list
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, this.ProjectId), true);
		}

		/// <summary>
		/// Handles clicks on the View Details link
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void ucReleaseTaskProgress_DrillDown()
		{
			//We simply need to redirect to the release list
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, this.ProjectId), true);
		}

		/// <summary>
		/// Handles clicks on the View Details link
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void ucTestExecutionStatus_DrillDown()
		{
			//We just redirect to the test-case list
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId), true);
		}

		#endregion
	}
}
