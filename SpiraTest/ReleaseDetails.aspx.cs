using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// details of a particular release and its list of test runs
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Releases, null, "Release-Management/#release-details", "ReleaseDetails_Title")]
	public partial class ReleaseDetails : PageLayout
	{
		protected int releaseId;
		protected SortedList<string, string> flagList;
		protected string paginationLegend = "";
		protected string ArtifactTabName = null;

		//Incidents
		protected SortedList<int, int> paginationOptionsList;

		//ViewState keys
		private const string ViewStateKey_PaginationStartRow = "PaginationStartRow";
		private const string CLASS_NAME = "Web.ReleaseDetails::";

		#region Event Handlers

		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";

			Logger.LogEnteringEvent(METHOD_NAME);

			//Capture the passed release id from the querystring
			releaseId = Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_RELEASE_ID]);

			//Make sure that the current project is set to the project associated with the artifact
			//this is important since the page may get loaded from an email notification URL or cross-project association
			VerifyArtifactProject(UrlRoots.NavigationLinkEnum.Releases, releaseId);

			//Set the permissions and action on the Add Comment button
			btnNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);
			btnNewComment.ClientScriptMethod = string.Format("add_comment('{0}')", txtNewComment.ClientID);

			//Configure the description for rich-text if appropriate
			txtNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);

			//Populate the user and project id in the incident AJAX grid control
			grdIncidentList.ProjectId = ProjectId;
			grdIncidentList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -2) + "?" + GlobalFunctions.PARAMETER_RELEASE_ID + "=" + releaseId;
			grdIncidentList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_INCIDENT;

			//Populate the user and project id in the build AJAX grid control
			//Also specify the release as a 'standard filter'
			grdBuildList.ProjectId = ProjectId;
			grdBuildList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Builds, ProjectId, -2);
			grdBuildList.ArtifactPrefix = Build.ARTIFACT_PREFIX;

			//See if we need to hide the Baselines tab.
			BaselineManager bMgr = new BaselineManager();
            ProjectSettings projectSettings = new ProjectSettings(ProjectId);
            tabBaseline.Visible = Common.Global.Feature_Baselines && projectSettings.BaseliningEnabled;
			
            //Hide certain fields depending on whether it's plan by points vs. hours
            if (projectSettings.DisplayHoursOnPlanningBoard)
            {
                //Plan by Hours
                this.plcPlannedPoints.Visible = false;
            }
            else
            {
                //Plan by Point
                this.plcPlannedEffort.Visible = false;
            }

            //Set the state of the incident release filter (from settings) if we have a stored setting
            int incidentReleaseFilterType = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_INCIDENTS_RELEASE_FILTER_TYPE, -1);
			if (incidentReleaseFilterType != -1)
			{
				ddlIncidentReleaseFilterType.SelectedValue = incidentReleaseFilterType.ToString();
			}

			//See what state the dropdown list is in
			Dictionary<string, object> incidentFilters = new Dictionary<string, object>();
			if (ddlIncidentReleaseFilterType.SelectedValue == "1")
			{
				incidentFilters.Add("DetectedReleaseId", releaseId);
			}
			else if (ddlIncidentReleaseFilterType.SelectedValue == "2")
			{
				incidentFilters.Add("ResolvedReleaseId", releaseId);
			}
			else if (ddlIncidentReleaseFilterType.SelectedValue == "3")
			{
				incidentFilters.Add("VerifiedReleaseId", releaseId);
			}
			else
			{
				//Default to detected by
				incidentFilters.Add("DetectedReleaseId", releaseId);
			}
			grdIncidentList.SetFilters(incidentFilters);

			//Only load the data once
			if (!IsPostBack)
			{
				//Load and bind the data on the page
				LoadAndBindData();
			}

			//Add the various panel event handlers
			tstTestCaseMapping.MessageLabelHandle = lblMessage;
			tstAttachmentPanel.MessageLabelHandle = lblMessage;

			//Reset the error message
			lblMessage.Text = "";

			//Specify the context for the ajax form manager control
			ajxFormManager.ProjectId = ProjectId;
			ajxFormManager.PrimaryKey = releaseId;
			ajxFormManager.ArtifactTypePrefix = Release.ARTIFACT_PREFIX;
			ajxFormManager.FolderPathUrlTemplate = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2));
			Dictionary<string, string> handlers = new Dictionary<string, string>();
			handlers.Add("dataSaved", "ajxFormManager_dataSaved");
			handlers.Add("loaded", "ajxFormManager_loaded");
			ajxFormManager.SetClientEventHandlers(handlers);

			//Set the context on the workflow operations control
			Dictionary<string, string> handlers2 = new Dictionary<string, string>();
			handlers2.Add("operationExecuted", "ajxWorkflowOperations_operationExecuted");
			ajxWorkflowOperations.ProjectId = ProjectId;
			ajxWorkflowOperations.PrimaryKey = releaseId;
			ajxWorkflowOperations.SetClientEventHandlers(handlers2);

			//Set the context on the comment list
			lstComments.ProjectId = ProjectId;

			//We need to add some client-code that handles the changes in release filter on the incident panel
			ddlIncidentReleaseFilterType.ClientScriptMethod = "ddlIncidentReleaseFilterType_selectedIndexChanged";

			Logger.LogExitingEvent(METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>Returns the URL back to the appropriate release list page</summary>
		protected string ReturnToReleaseListUrl
		{
			get
			{
				//Return to the release list, iteration plan, or project home depending on the referrer
				if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME] == GlobalFunctions.PARAMETER_VALUE_TRUE)
				{
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId, 0);
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
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, 0);
				}
			}
		}

		/// <summary>Returns the url to the generic release page with a token for the release id</summary>
		protected string ReleaseRedirectUrl
		{
			get
			{
				return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2));
			}
		}
		#endregion

		#region Functions Used Internally

		/// <summary>
		/// Loads and databinds all the data on the page
		/// </summary>
		protected void LoadAndBindData()
		{
			//Load any lookups
			Business.ReleaseManager releaseManager = new Business.ReleaseManager();
			Business.UserManager userManager = new Business.UserManager();
			List<DataModel.User> users = userManager.RetrieveForProject(ProjectId);
			List<ReleaseType> releaseTypes = releaseManager.RetrieveTypes();
			List<PeriodicReviewAlertType> alertTypes = releaseManager.RetrievePeriodicReviewAlertTypes();
			ddlCreator.DataSource = users;
			ddlOwner.DataSource = users;
			ddlType.DataSource = releaseTypes;
			ddlPeriodicAlertTypes.DataSource = alertTypes;
			flagList = releaseManager.RetrieveFlagLookup();

			//Add the various custom properties to the table of fields
			UnityCustomPropertyInjector.CreateControls(
				ProjectId,
				ProjectTemplateId,
				Artifact.ArtifactTypeEnum.Release,
				customFieldsDefault,
				ajxFormManager,
				customFieldsUsers,
				customFieldsDates,
				customFieldsRichText
				);

			//Set the project/artifact for the RTE so that we can upload screenshots
			txtDescription.Screenshot_ProjectId = ProjectId;
			txtNewComment.Screenshot_ProjectId = ProjectId;

			DataBind();

			//Populate the link back to the list page
			navReleaseList.ProjectId = ProjectId;
			navReleaseList.ListScreenUrl = ReturnToReleaseListUrl;
			navReleaseList.SelectedItemId = releaseId;
			navReleaseList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
			navReleaseList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
			navReleaseList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

			//Specify the artifact type and artifact id to retrieve the test case coverage for
			tstTestCaseMapping.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.Release;
			tstTestCaseMapping.DisplayTypeEnum = Artifact.DisplayTypeEnum.Release_TestCases;

			//Specify the artifact type and artifact id to retrieve the attachments for
			tstAttachmentPanel.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.Release;
			tstAttachmentPanel.ArtifactId = releaseId;
			tstAttachmentPanel.MessageLabelHandle = lblMessage;

			//Specify the artifact type and artifact id to retrieve the history & Baseline log for
			tstHistoryPanel.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.Release;
			tstBaselinePanel.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.Release;

			//Specify the artifact type and artifact id to retrieve the task list for
			tstRequirementTaskPanel.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.Release;

			//Specify the artifact type and artifact id to retrieve the test run list coverage for
			tstTestRunListPanel.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.Release;

			//Specify the artifact type and artifact id to retrieve the test run list coverage for
			tstRequirementTaskPanel.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.Release;

			//See if we need to load a tab up.
			if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
			{
				string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

				switch (tabRequest)
				{
					case GlobalFunctions.PARAMETER_TAB_ATTACHMENTS:
						tclReleaseDetails.SelectedTab = pnlAttachments.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_HISTORY:
						tclReleaseDetails.SelectedTab = pnlHistory.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
						tclReleaseDetails.SelectedTab = pnlOverview.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_REQUIREMENT:
					case GlobalFunctions.PARAMETER_TAB_TASK:
						tclReleaseDetails.SelectedTab = pnlTasks.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_COVERAGE:
					case GlobalFunctions.PARAMETER_TAB_TESTCASE:
						tclReleaseDetails.SelectedTab = pnlTestCases.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_TESTRUN:
						tclReleaseDetails.SelectedTab = pnlTestRuns.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_INCIDENT:
						tclReleaseDetails.SelectedTab = pnlIncidents.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

                    case GlobalFunctions.PARAMETER_TAB_BASELINE:
                        tclReleaseDetails.SelectedTab = pnlBaseline.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					default:
						tclReleaseDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
						break;
				}
			}

			//Specify the tab in the navigation control
			navReleaseList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2, ArtifactTabName);

			//Load and bind the various panels
			LoadAndBindPanels();
		}

		/// <summary>
		/// Loads and databinds the various panels
		/// </summary>
		private void LoadAndBindPanels()
		{
			//Populate the list of incident columns to show/hide and databind
			IncidentManager incidentManager = new IncidentManager();
			ddlShowHideIncidentColumns.DataSource = CreateShowHideColumnsList(Artifact.ArtifactTypeEnum.Incident);
			ddlShowHideIncidentColumns.DataBind();

			//Get the list of mapped and available test cases
			tstTestCaseMapping.LoadAndBindData(true);

			//Get the list of attachments and databind the grid
			tstAttachmentPanel.LoadAndBindData(true);

			//Get the history change log and databind the grid
			tstHistoryPanel.LoadAndBindData(true);

			//Get the history change log and databind the grid
			tstBaselinePanel.LoadAndBindData(true);
		}

		#endregion
	}
}
