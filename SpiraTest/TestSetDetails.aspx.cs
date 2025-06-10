using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// details of a particular test set and associated test cases
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.TestSets, null, "Test-Set-Management/#test-set-details", "TestSetDetails_Title")]
	public partial class TestSetDetails : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TestSetDetails::";

		//Other variables
		protected int testSetId;
		protected string ArtifactTabName = "";

		#region Properties

		/// <summary>
		/// Returns the base url for redirecting to a created pending test run entry
		/// </summary>
		protected string TestRunsPendingUrl
        {
            get
            {
                //The referrer is the Test Set list because the test execution page doesn't currently handle returning to the test set details page
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestExecute, -2, -2) + "?" + GlobalFunctions.PARAMETER_REFERER_TEST_SET_DETAILS + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE);
            }
        }

        /// <summary>
        /// Returns the base url for redirecting to a created pending test run entry - when execution should be in exploratory mode
        /// </summary>
        protected string TestRunsPendingExploratoryUrl
        {
            get
            {
                //The referrer is the Test Case list
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestExecuteExploratory, -2, -2) + "?" + GlobalFunctions.PARAMETER_REFERER_TEST_CASE_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE);
            }
        }

        /// <summary>
        /// Returns the URL for redirecting to the page that launches an automated test
        /// </summary>
        protected string TestSetLaunchUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestLaunch, ProjectId, testSetId));
            }
        }

		#endregion

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

			//Determine if we are meant to display rich-text descriptions or not
			this.txtNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);

			//Capture the passed test set id from the querystring
			this.testSetId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID]);

            //Make sure that the current project is set to the project associated with the artifact
            //this is important since the page may get loaded from an email notification URL or cross-project association
            VerifyArtifactProject(UrlRoots.NavigationLinkEnum.TestSets, this.testSetId);

            //Set the permissions and action on the Add Comment button
            this.btnNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);
            this.btnNewComment.ClientScriptMethod = String.Format("add_comment('{0}')", this.txtNewComment.ClientID);

			//Populate the user and project id in the AJAX test case grid control
            //The BASE URL needs to point to a special rewriter URL since we have the TestSetTestCaseId as primary key not TestCaseId
			this.grdTestCases.ProjectId = this.ProjectId;
			this.grdTestCases.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE;
            this.grdTestCases.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSetTestCases, ProjectId, -2); 

            //Custom CSS for test set test cases
            Dictionary<string, string> testCaseCssClasses = new Dictionary<string, string>();
            testCaseCssClasses.Add("ExecutionStatusId", "priority2");
            testCaseCssClasses.Add("OwnerId", "priority3");
            this.grdTestCases.SetCustomCssClasses(testCaseCssClasses);

			//Specify the context for the ajax form manager
			this.ajxFormManager.ProjectId = this.ProjectId;
            this.ajxFormManager.PrimaryKey = this.testSetId;
            this.ajxFormManager.ArtifactTypePrefix = TestSet.ARTIFACT_PREFIX;
            this.ajxFormManager.FolderPathUrlTemplate = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, 0, GlobalFunctions.ARTIFACT_ID_TOKEN));
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("dataSaved", "ajxFormManager_dataSaved");
            handlers.Add("loaded", "ajxFormManager_loaded");
            this.ajxFormManager.SetClientEventHandlers(handlers);

            //Specify the context for the folders sidebar
            this.pnlFolders.ProjectId = ProjectId;
            this.pnlFolders.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlFolders.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

            // Set the context of the folders list
            this.trvFolders.ClientScriptServerControlId = this.navTestSetList.UniqueID;
            this.trvFolders.ContainerId = this.ProjectId;

            //See if we have a stored node that we need to populate, use zero(0) otherwise so that the root is selected by default
            TestSetManager testSetManager = new TestSetManager();
            int selectedFolder = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
            if (selectedFolder < 1)
            {
                selectedFolder = 0; //Makes sure that 'root' is selected
            }
            //If the folder does not exist reset to root and update settings-
            else if (!testSetManager.TestSetFolder_Exists(this.ProjectId, selectedFolder))
            {
                selectedFolder = 0; //Makes sure that 'root' is selected
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
            }
            this.trvFolders.SelectedNodeId = selectedFolder.ToString();

            //Set the context on the comment list
            this.lstComments.ProjectId = ProjectId;

            //Add the client event handler to the background task process
            handlers = new Dictionary<string, string>();
            handlers.Add("succeeded", "ajxBackgroundProcessManager_success");
            this.ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

			//Add the URL to the release hierarchical drop-down
			this.ddlRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);

			//Set the error message handles
			this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;

			//See if we need to load a tab up.
			if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
			{
				string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

				switch (tabRequest)
				{
					case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
						tclTestSetDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_ATTACHMENTS:
						tclTestSetDetails.SelectedTab = this.pnlAttachments.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_INCIDENT:
						tclTestSetDetails.SelectedTab = this.pnlIncidents.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_HISTORY:
						tclTestSetDetails.SelectedTab = this.pnlHistory.ClientID;
						this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_HISTORY;
						break;

					case GlobalFunctions.PARAMETER_TAB_TESTRUN:
						tclTestSetDetails.SelectedTab = this.pnlTestRuns.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					default:
						tclTestSetDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
						break;
				}
			}

			//Load the various datasets and databind
			LoadAndBindData(true);

			//Add a handler on the test case grid so that the test set execution data gets updated if changes
			//are made to the grid
			handlers = new Dictionary<string, string>();
            handlers.Add("loaded", "grdTestCases_loaded");
			this.grdTestCases.SetClientEventHandlers(handlers);

			//Reset the error message
			this.lblMessage.Text = "";

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>
        /// Returns the url to the generic test set page with a token for the test set id
        /// </summary>
        protected string TestSetRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, -2));
            }
        }

		/// <summary>
		/// Redirects to a different test set
		/// </summary>
		/// <param name="testSetId"></param>
		protected void RedirectToDifferentArtifact(int testSetId)
		{
			//Perform the redirect
			Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, testSetId, ArtifactTabName), true);
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// This method calls the data access methods to populate the datasets
		/// </summary>
		/// <param name="dataBind">Should we databind or not</param>
		private void LoadAndBindData(bool dataBind)
		{
			//Instantiate the business objects
			Business.TestSetManager testSetManager = new Business.TestSetManager();
			Business.UserManager userManager = new Business.UserManager();
			Business.ReleaseManager releaseManager = new Business.ReleaseManager();

			//Capture the passed test set id from the querystring
			this.testSetId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID]);

			//Specify the artifact type and artifact id to retrieve the attachments for
			this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestSet;
            this.tstAttachmentPanel.ArtifactId = this.testSetId;

            //Specify the artifact type and artifact id to retrieve the history log for
            this.tstHistoryPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestSet;

			//Specify the artifact type and artifact id to retrieve the test run list for
			this.tstTestRunListPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestSet;

            //Specify the artifact type to retrieve the incident list for
            this.tstIncidentListPanel.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.TestSet;

			try
			{
                //Next load any lookups needed by the test set details section
                TestRunManager testRunManager = new TestRunManager();
                AutomationManager automationManager = new AutomationManager();
                List<DataModel.User> allUsers = new UserManager().RetrieveForProject(this.ProjectId);
                List<DataModel.User> activeUsers = new UserManager().RetrieveActiveByProjectId(this.ProjectId);
                List<TestSetStatus> testSetStati = testSetManager.RetrieveStatuses();
                List<ReleaseView> releases = releaseManager.RetrieveByProjectId(this.ProjectId, false); //Dropdown limits to active in UI
                List<TestRunType> testRunTypes = testRunManager.RetrieveTypes();
                List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(ProjectId);
                List<TestConfigurationSet> testConfigurationSets = new TestConfigurationManager().RetrieveSets(ProjectId);
                this.ddlTestRunType.DataSource = testRunTypes;
                this.ddlAutomationHost.DataSource = automationHosts;
                this.ddlRecurrence.DataSource = testSetManager.RetrieveRecurrences();
                this.ddlCreator.DataSource = allUsers;
                this.ddlOwner.DataSource = activeUsers;
                this.ddlTestSetStatus.DataSource = testSetStati;
                this.ddlRelease.DataSource = releases;
                this.ddlTestConfigurationSet.DataSource = testConfigurationSets;

                //Add the various custom properties to the relevant field group
                UnityCustomPropertyInjector.CreateControls(
                    ProjectId,
                    ProjectTemplateId,
                    DataModel.Artifact.ArtifactTypeEnum.TestSet,
                    this.customFieldsDefault,
                    this.ajxFormManager,
                    this.customFieldsUsers,
                    this.customFieldsDates,
                    this.customFieldsRichText
                );

                //Populate the test set test cases show/hide columns list
                this.ddlShowHideColumns.DataSource = GetTestCaseColumnsList();

                //Set the project/artifact for the RTE so that we can upload screenshots
                this.txtDescription.Screenshot_ProjectId = ProjectId;
                this.txtDescription.Screenshot_ArtifactId = this.testSetId;
                this.txtNewComment.Screenshot_ProjectId = ProjectId;
                this.txtNewComment.Screenshot_ArtifactId = this.testSetId;

                //Databind if the flag is set - needs to occur before the custom properties panel is databound
                if (dataBind)
                {
                    this.DataBind();
                }

                //Specify the context for the navigation bar
                this.navTestSetList.ProjectId = ProjectId;
                this.navTestSetList.ListScreenUrl = ReturnToTestSetListUrl;
                this.navTestSetList.SelectedItemId = this.testSetId;
                if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
                {
                    //If we came from the 'My Page' we need to set the navigation to 'My Assigned Test Sets'
                    this.navTestSetList.DisplayMode = NavigationBar.DisplayModes.Assigned;
                    ProjectSettingsCollection settings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS);
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = (int)NavigationBar.DisplayModes.Assigned;
                    settings.Save();
                }
                else
                {
                    //Otherwise use the last setting the user used
                    this.navTestSetList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
                }
                this.navTestSetList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
                this.navTestSetList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

                //Specify the tab in the navigation control
                this.navTestSetList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, -2, ArtifactTabName);
			}
			catch (ArtifactNotExistsException)
			{
				//If the artifact doesn't exist let the user know nicely
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.TestSetDetails_ArtifactNotExists, true);
			}
		}

        /// <summary>
        /// Gets the list of columns that can be shown/hidden for test cases in a set
        /// </summary>
        /// <returns>The show/hide list</returns>
        /// <remarks>
        /// 1) Unlike other grids this comes from project settings collection not from the usual field/custom properties tables
        /// 2) The name, owner and ID fields are always shown and cannot be changed
        /// </remarks>
        protected Dictionary<string, string> GetTestCaseColumnsList()
        {
            //Get the list of configurable fields and their visibility status
            TestSetManager testSetManager = new TestSetManager();
            List<ArtifactListFieldDisplay> artifactFields = testSetManager.RetrieveTestSetTestCaseFieldsForList(ProjectId, ProjectTemplateId, UserId);

            //Convert to dropdown list dictionary
            Dictionary<string, string> showHideList = new Dictionary<string, string>();
            foreach (ArtifactListFieldDisplay artifactField in artifactFields)
            {
                //See if we can localize the field name or not
                string localizedName = Resources.Fields.ResourceManager.GetString(artifactField.Name);
                if (!String.IsNullOrEmpty(localizedName))
                {
                    artifactField.Caption = localizedName;
                }
                string legend = (artifactField.IsVisible) ? Resources.Dialogs.Global_Hide + " " + artifactField.Caption : Resources.Dialogs.Global_Show + " " + artifactField.Caption;
                showHideList.Add(artifactField.Name, legend);
            }
            return showHideList;
        }

		/// <summary>
		/// Gets the URL to redirects back to the test set list page or my page list
		/// </summary>
		protected string ReturnToTestSetListUrl
		{
			get
			{
				//Return to the test set list or project list depending on the referrer
				if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
				{
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0);
				}
				else
				{
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId);
				}
			}
		}

		#endregion
	}
}
