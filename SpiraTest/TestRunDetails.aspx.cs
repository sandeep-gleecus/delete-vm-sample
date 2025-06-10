using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// details of a particular test run and associated test run steps
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.TestRuns, null, "Test-Run-Management/#test-run-details", "TestRunDetails_Title")]
	public partial class TestRunDetails : PageLayout
	{
		protected int testRunId;
        protected int testCaseId;
        protected int? testSetId;
        protected int? testSetCaseId;
        protected string incidentBaseUrl;
		protected string ArtifactTabName = null;

		protected const int NUMBER_OF_ROWS_PER_PAGE = 20;

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TestRunDetails::";

        //project settings
        private ProjectSettings projectSettings = null;
        protected string Feature_Local_Worx { get; private set; } = "false";

        #region Event Handlers

        /// <summary>
        /// Returns the base url for redirecting to a created pending test run entry
        /// </summary>
        protected string TestRunsPendingUrl
        {
            get
            {
                //The referrer is the Test Case list because the test execution page doesn't currently handle returning to the test case details page
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

        /// <summary>
        /// Return URL for the list page
        /// </summary>
        protected string ReturnToTestRunListUrl
        {
            get
            {
                string url;
                if (Request.QueryString[GlobalFunctions.PARAMETER_RELEASE_ID] != null)
                {
                    url = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_RELEASE_ID]), GlobalFunctions.PARAMETER_TAB_TESTRUN);
                }
                else if (Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID] != null)
                {
                    url = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID]), GlobalFunctions.PARAMETER_TAB_TESTRUN);
                }
                else if (Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_ID] != null)
                {
                    url = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_ID]), GlobalFunctions.PARAMETER_TAB_TESTRUN);
                }
                else
                {
                    url = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestRuns, ProjectId);
                }
                return url;
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

			//Capture the passed test run id from the querystring
            CaptureTestRunFromQuerystring();

            //Add the URL to the hierarchical drop-downs and artifact hyperlinks
			this.ddlRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);
            this.lnkTestSet.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, -2);
            this.lnkTestCase.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, -2);
            this.lnkAutomationHost.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.AutomationHosts, ProjectId, -2);

            //Set the client-side handlers on the test set artifact link
            Dictionary<string, string> handlers2 = new Dictionary<string, string>();
            handlers2.Add("changeClicked", "Function.createDelegate(page, page.lnkTestSet_changeClicked)");
            this.lnkTestSet.SetClientEventHandlers(handlers2);
            this.ajxTestSetSelector.ProjectId = ProjectId;
            this.ddlTestSetFolders.DataBind();  //Need to get the 'Root' default entry to show up

			//Load the lookups that don't change per test run (display all users not just active since records are historical)

			//Release
			ReleaseManager releaseManager = new ReleaseManager();
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(this.ProjectId, false);
            this.ddlRelease.DataSource = releases;
			this.ddlRelease.DataBind();

            //Set client-side event handler
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("selectedItemChanged", "Function.createDelegate(page, page.ddlRelease_selectedItemChanged)");
            this.ddlRelease.SetClientEventHandlers(handlers);

			//User
			Business.UserManager userManager = new Business.UserManager();
            List<DataModel.User> users = userManager.RetrieveForProject(ProjectId);
            this.ddlOwner.DataSource = users;
			this.ddlOwner.DataBind();

            //Add the client event handler to the background task process
            handlers = new Dictionary<string, string>();
            handlers.Add("succeeded", "Function.createDelegate(page, page.ajxBackgroundProcessManager_success)");
            this.ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

            //Add the various custom properties to the relevant field group
            UnityCustomPropertyInjector.CreateControls(
                ProjectId,
                ProjectTemplateId,
                DataModel.Artifact.ArtifactTypeEnum.TestRun,
                this.customFieldsDefault,
                this.ajxFormManager,
                this.customFieldsUsers,
                this.customFieldsDates,
                this.customFieldsRichText
            );

			//See if a tab's been selected.
			if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
			{
				string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

				switch (tabRequest)
				{
					case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
						tclTestRunDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_ATTACHMENTS:
						tclTestRunDetails.SelectedTab = this.pnlAttachments.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_INCIDENT:
						tclTestRunDetails.SelectedTab = this.pnlIncidents.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_HISTORY:
						tclTestRunDetails.SelectedTab = this.pnlHistory.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_ASSOCIATION:
						tclTestRunDetails.SelectedTab = this.pnlAssociations.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					default:
						tclTestRunDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
						break;
				}
			}

			//Load the main form data
			LoadAndBindData();

			//Set the default error message
			this.lblMessage.Text = "";

            //Pass the message control to the various user controls
            this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;
            this.tstAssociationPanel.MessageLabelHandle = this.lblMessage;

            //Get the base URL for incidents being displayed
            this.incidentBaseUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -2));

            //Get the ProjectSettings collection
            if (ProjectId > 0)
            {
                projectSettings = new ProjectSettings(ProjectId);
            }
            //set the local feature flag for Worx - to be used client side
            if (projectSettings != null)
            {
                //set the local feature flag for Worx - to be used client side
                Feature_Local_Worx = projectSettings.Testing_WorXEnabled.ToString().ToLowerInvariant();

                //Disable test execution
                if (projectSettings.Testing_ExecuteSetsOnly)
                {
                    this.btnExecuteTest.Visible = false;
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Loads and binds the data on the page
		/// </summary>
		protected void LoadAndBindData()
		{
			const string METHOD_NAME = "LoadAndBindData";

			//Get the dataset for the passed in test run id
			TestRunManager testRunManager = new TestRunManager();
			try
			{
				TestRunView testRun = testRunManager.RetrieveById(this.testRunId);

				//Make sure that it's not a pending run that has no End-Date set
				//Can't access through the strongly typed properties as it will cause an error
                if (testRun == null)
				{
					//If the artifact doesn't exist let the user know nicely
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Test Run TR" + this.testRunId + " does not exist in the system");
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.TestRunDetails_ArtifactDeleted, true);
				}
                if (!testRun.EndDate.HasValue)
				{
					//We are not allowed to display "pending" runs
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Test Run TR" + this.testRunId + " cannot be displayed as it is still in a pending state");
					Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.TestRunDetails_ArtifactNotExists, true);
				}

                //Make sure that the current project is set to the project in the test run
                //this is important since the page may get loaded from an email notification URL
                VerifyArtifactProject(testRun.ProjectId, UrlRoots.NavigationLinkEnum.TestRuns, testRunId);

                //Specify if the current user created/owns this artifact (used for permission-checking)
                SpiraContext.Current.IsArtifactCreatorOrOwner = (testRun.TesterId == UserId);

                //Redirect if the user has limited view permissions and does not have this flag set. Grant access to sys admins if they are a member of the project in any way
                //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
                int projectRoleIdToCheck = ProjectRoleId;
                if (UserIsAdmin && ProjectRoleId > 0)
                {
                    projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
                }
                if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View) == Project.AuthorizationState.Limited && !SpiraContext.Current.IsArtifactCreatorOrOwner)
                {
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
                }

                //Capture the test case and test set ids for use on the page
                this.testCaseId = testRun.TestCaseId;
                this.testSetId = testRun.TestSetId;
                this.testSetCaseId = testRun.TestSetTestCaseId;

                //Specify the context for the navigation bar
                this.navTestRunList.ProjectId = ProjectId;
                if (this.testRunId != -1)
                {
                    this.navTestRunList.SelectedItemId = this.testRunId;
                }

                this.navTestRunList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.AllItems);
                this.navTestRunList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
                this.navTestRunList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

                //Specify the base URL in the navigation control
                this.navTestRunList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestRuns, ProjectId, -2, ArtifactTabName) + "?" + UrlSuffix;

                //Specify the context for the ajax form manager
                this.ajxFormManager.ProjectId = this.ProjectId;
                this.ajxFormManager.PrimaryKey = this.testRunId;
                this.ajxFormManager.ArtifactTypePrefix = TestRun.ARTIFACT_PREFIX;
                Dictionary<string, string> handlers = new Dictionary<string, string>();
                handlers.Add("dataSaved", "ajxFormManager_dataSaved");
                handlers.Add("loaded", "ajxFormManager_loaded");
                this.ajxFormManager.SetClientEventHandlers(handlers);

                //Specify the artifact type to retrieve the incident list for
                this.tstIncidentListPanel.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.TestRun;

                //Specify the project for the new incident association
                this.ajxIncidentsSelector.ProjectId = this.ProjectId;
                handlers = new Dictionary<string, string>();
                handlers.Add("displayed", "pnlAddAssociation_displayed");
                this.pnlAddAssociation.SetClientEventHandlers(handlers);

				//Specify the return URL and display mode depending on whether we're passed a release ID, test set id, test case id, or none
                if (Request.QueryString[GlobalFunctions.PARAMETER_RELEASE_ID] != null && testRun.ReleaseId.HasValue)
				{
                    this.navTestRunList.SummaryItemImage = "Images/artifact-Release.svg";
                    this.navTestRunList.AlternateItemImage = "Images/artifact-Iteration.svg";
                    this.navTestRunList.IncludeRelease = true;
                    this.navTestRunList.DisplayMode = NavigationBar.DisplayModes.ForRelease;
                    this.navTestRunList.ContainerId = testRun.ReleaseId;
                    this.navTestRunList.ListScreenUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_RELEASE_ID]), GlobalFunctions.PARAMETER_TAB_TESTRUN);
				}
                else if (Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID] != null && testRun.TestSetId.HasValue)
				{
                    this.navTestRunList.SummaryItemImage = "Images/artifact-TestSet.svg";
                    this.navTestRunList.IncludeTestSet = true;
                    this.navTestRunList.DisplayMode = NavigationBar.DisplayModes.ForTestSet;
                    this.navTestRunList.ContainerId = testRun.TestSetId;
                    this.navTestRunList.ListScreenUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID]), GlobalFunctions.PARAMETER_TAB_TESTRUN);
				}
				else if (Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_ID] != null)
				{
                    this.navTestRunList.SummaryItemImage = "Images/artifact-TestCase.svg";
                    this.navTestRunList.IncludeTestCase = true;
                    this.navTestRunList.DisplayMode = NavigationBar.DisplayModes.ForTestCase;
                    this.navTestRunList.ContainerId = testRun.TestCaseId;
                    this.navTestRunList.ListScreenUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_ID]), GlobalFunctions.PARAMETER_TAB_TESTRUN);
				}
				else
				{
                    this.navTestRunList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_STEP_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
                    this.navTestRunList.ContainerId = null;
                    this.navTestRunList.ListScreenUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestRuns, ProjectId);
				}

				//Specify the artifact type and artifact id to retrieve the attachments for
				this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestRun;
                this.tstAttachmentPanel.ArtifactId = this.testRunId;
                this.tstAttachmentPanel.LoadAndBindData(true);

                //Specify the artifact type and artifact id to retrieve the history log for
                this.tstHistoryPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestRun;
                tstHistoryPanel.LoadAndBindData(true);

                //Specify the artifact type and artifact id to retrieve the tasks for
                this.tstAssociationPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestRun;
                this.tstAssociationPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.ArtifactLink;
			}
			catch (ArtifactNotExistsException)
			{
				//If the artifact doesn't exist let the user know nicely
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The test run you selected has been deleted from the system.", true);
			}
		}

        /// <summary>
        /// Gets the URL suffix to append
        /// </summary>
        protected string UrlSuffix
        {
            get
            {
                //Perform the redirect - pass the release, test case id or test set if we it was passed in
                string urlSuf = "";
                if (Request.QueryString[GlobalFunctions.PARAMETER_RELEASE_ID] != null && Request.QueryString[GlobalFunctions.PARAMETER_RELEASE_ID] != "")
                {
                    int releaseId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_RELEASE_ID]);
                    urlSuf += "&" + GlobalFunctions.PARAMETER_RELEASE_ID + "=" + releaseId.ToString();
                }
                if (Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID] != null && Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID] != "")
                {
                    int testSetId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID]);
                    urlSuf += "&" + GlobalFunctions.PARAMETER_TEST_SET_ID + "=" + testSetId.ToString();
                }
                if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_ID]))
                {
                    int testCaseId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_ID]);
                    urlSuf += "&" + GlobalFunctions.PARAMETER_TEST_CASE_ID + "=" + testCaseId.ToString();
                }
                return urlSuf.Trim('&');
            }
        }

        /// <summary>
        /// Returns the url to the generic test run page with a token for the test run id
        /// </summary>
        protected string TestRunRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestRuns, ProjectId, -2));
            }
        }

        /// <summary>
        /// Get the test run from the querysting (either directly or indirectly from the test case id)
        /// </summary>
        protected void CaptureTestRunFromQuerystring()
        {
            //If we don't have a test run, see if we have a test case, in which case get the most recent
            //test run for this test case
            if (String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_TEST_RUN_ID]))
            {
                if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_ID]))
                {
                    try
                    {
                        int testCaseId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_ID]);
                        TestRunManager testRunManager = new TestRunManager();
                        Hashtable testCaseFilter = new Hashtable();
                        testCaseFilter.Add("TestCaseId", testCaseId);
                        List<TestRunView> testRunLookupList = testRunManager.Retrieve(this.ProjectId, "EndDate", false, 1, 1, testCaseFilter, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                        if (testRunLookupList.Count > 0)
                        {
                            this.testRunId = testRunLookupList[0].TestRunId;
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Do nothing
                    }
                }
            }
            else
            {
                this.testRunId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_RUN_ID]);
            }
        }

		#endregion
	}
}
