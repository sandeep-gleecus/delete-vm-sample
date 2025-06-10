using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;
using System.Collections;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// details of a particular test case and associated test steps
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.TestCases, null, "Test-Case-Management/#test-case-details", "TestCaseDetails_Title")]
	public partial class TestCaseDetails : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TestCaseDetails::";

		//project settings
		private ProjectSettings projectSettings = null;
		protected string Feature_Local_Worx { get; private set; } = "false";
		protected string ArtifactTabName = null;

		//Business Objects
		protected TestCaseManager testCaseManager;
		protected TestRunManager testRunManager;

		//Other variables
		protected int testCaseId;
		protected SortedList<string, string> flagList;
		protected TestCase testCase;

		#region Properties

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
		/// Returns the url to the generic test case page with a token for the test case id
		/// </summary>
		protected string TestCaseRedirectUrl
		{
			get
			{
				return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, -2));
			}
		}


		/// <summary>
		/// Returns the url to the test case list page
		/// </summary>
		protected string TestCaseListUrl
		{
			get
			{
				return UrlRewriterModule.ResolveUrl(ReturnToTestCaseListUrl);
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Any code that needs to be executed before controls Load() event is fired
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			//Specify that the automated test script allows script to be entered
			this.txtAutomationScript.AllowScripting = true;
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

			//Determine if we are meant to display rich-text descriptions or not
			this.txtNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);

			//Instantiate the business classes
			this.testCaseManager = new TestCaseManager();
			this.testRunManager = new TestRunManager();

			//Capture the passed test case id from the querystring
			this.testCaseId = System.Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_TEST_CASE_ID]);

			//Add the URL to the artifact hyperlinks
			this.lnkAutomationDocument.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, ProjectId, -2);

			//Make sure that the current project is set to the project associated with the artifact
			//this is important since the page may get loaded from an email notification URL or cross-project association
			VerifyArtifactProject(UrlRoots.NavigationLinkEnum.TestCases, this.testCaseId);

			//Add the user controls' reference to the message box
			this.tstReleaseMappingPanel.MessageLabelHandle = this.lblMessage;
			this.tstCoveragePanel.MessageLabelHandle = this.lblMessage;
			this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;
			this.tstAssociationPanel.MessageLabelHandle = this.lblMessage;

			//Set the permissions and action on the Add Comment button
			this.btnNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);
			this.btnNewComment.ClientScriptMethod = String.Format("add_comment('{0}')", this.txtNewComment.ClientID);

			//Only load the data once
			if (!IsPostBack)
			{
				//Load the main page
				LoadAndBindData();
				this.btnRecord.NavigateUrl = GetTestMasterUrl();
			}

			//Populate the user and project id in the AJAX test step grid control
			this.grdTestSteps.ProjectId = this.ProjectId;
			this.grdTestSteps.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSteps, ProjectId, -2);
			this.grdTestSteps.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TEST_STEP;
			Dictionary<string, object> standardFilters = new Dictionary<string, object>();
			standardFilters.Add("TestCaseId", this.testCaseId);
			this.grdTestSteps.SetFilters(standardFilters);
			Dictionary<string, string> testStepGridHandlers = new Dictionary<string, string>();
			testStepGridHandlers.Add("rowEditAlternate", "Function.createDelegate(page, page.grdTestSteps_rowEditAlternate)");
			this.grdTestSteps.SetClientEventHandlers(testStepGridHandlers);

			//Custom CSS for test steps
			Dictionary<string, string> testStepCssClasses = new Dictionary<string, string>();
			testStepCssClasses.Add("ExecutionStatusId", "priority2");
			testStepCssClasses.Add("Description", "priority2");
			this.grdTestSteps.SetCustomCssClasses(testStepCssClasses);

			//Specify the context for the ajax form manager
			this.ajxFormManager.ProjectId = this.ProjectId;
			this.ajxFormManager.PrimaryKey = this.testCaseId;
			this.ajxFormManager.ArtifactTypePrefix = TestCase.ARTIFACT_PREFIX;
			this.ajxFormManager.FolderPathUrlTemplate = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, 0, GlobalFunctions.ARTIFACT_ID_TOKEN));
			Dictionary<string, string> handlers = new Dictionary<string, string>();
			handlers.Add("dataSaved", "ajxFormManager_dataSaved");
			handlers.Add("loaded", "ajxFormManager_loaded");
			this.ajxFormManager.SetClientEventHandlers(handlers);

			//Specify the context for the folders sidebar
			this.pnlFolders.ProjectId = ProjectId;
			this.pnlFolders.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
			this.pnlFolders.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

			// Set the context of the folders list
			this.trvFolders.ClientScriptServerControlId = this.navTestCaseList.UniqueID;
			this.trvFolders.ContainerId = this.ProjectId;

			//See if we have a stored node that we need to populate, use zero(0) otherwise so that the root is selected by default
			int selectedFolder = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
			TestCaseManager testCaseManager = new TestCaseManager();
			if (selectedFolder < 1)
			{
				selectedFolder = 0; //Makes sure that 'root' is selected
			}
			//If the folder does not exist reset to root and update settings-
			else if (!testCaseManager.TestCaseFolder_Exists(this.ProjectId, selectedFolder))
			{
				selectedFolder = 0; //Makes sure that 'root' is selected
				SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
			}
			this.trvFolders.SelectedNodeId = selectedFolder.ToString();

			//Set the context on the workflow operations control
			Dictionary<string, string> handlers2 = new Dictionary<string, string>();
			handlers2.Add("operationExecuted", "Function.createDelegate(page, page.ajxWorkflowOperations_operationExecuted)");

			//Mobile
			this.ajxWorkflowOperations.ProjectId = ProjectId;
			this.ajxWorkflowOperations.PrimaryKey = this.testCaseId;
			this.ajxWorkflowOperations.SetClientEventHandlers(handlers2);

			//Set the context on the comment list
			this.lstComments.ProjectId = ProjectId;

			//Set the context for the test case selector AJAX controls
			this.ajxLinkedTestCaseSelector.ProjectId = this.ProjectId;
			this.ajxImportTestCaseSelector.ProjectId = this.ProjectId;

			//Add the client event handler to the background task process
			handlers = new Dictionary<string, string>();
			handlers.Add("succeeded", "Function.createDelegate(page, page.ajxBackgroundProcessManager_success)");
			this.ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

			//When the test case selection is made, need to handle the event and display the test step parameters
			Dictionary<string, string> linkedTestCaseSelectorHandlers = new Dictionary<string, string>();
			linkedTestCaseSelectorHandlers.Add("selected", "Function.createDelegate(page, page.pnlInsertTestLink_ajxLinkedTestCaseSelector_selected)");
			linkedTestCaseSelectorHandlers.Add("loaded", "Function.createDelegate(page, page.pnlInsertTestLink_ajxLinkedTestCaseSelector_loaded)");
			this.ajxLinkedTestCaseSelector.SetClientEventHandlers(linkedTestCaseSelectorHandlers);

			Dictionary<string, string> importTestCaseSelectorHandlers = new Dictionary<string, string>();
			importTestCaseSelectorHandlers.Add("selected", "Function.createDelegate(page, page.pnlImportTestCase_ajxImportTestCaseSelector_selected)");
			this.ajxImportTestCaseSelector.SetClientEventHandlers(importTestCaseSelectorHandlers);

			//Get the ProjectSettings collection
			if (ProjectId > 0)
			{
				projectSettings = new ProjectSettings(ProjectId);
			}
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

			//Reset the error message
			this.lblMessage.Text = "";

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// This method populates the lookup dropdowns and loads the appropriate test case datasets
		/// </summary>
		protected void LoadAndBindData()
		{

			//Specify the artifact type and artifact id to retrieve the attachments for
			this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestCase;
			this.tstAttachmentPanel.ArtifactId = this.testCaseId;

			//Specify the artifact type and artifact id to retrieve the history log for
			this.tstHistoryPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestCase;
			this.tstHistoryPanel.ShowTestStepData = true;

			//Specify the artifact type and artifact id to retrieve the release coverage for
			this.tstReleaseMappingPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestCase;
			this.tstReleaseMappingPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.TestCase_Releases;
			this.tstReleaseMappingPanel.LoadAndBindData(true);

			//Specify the artifact type and artifact id to retrieve the requirements coverage for
			this.tstCoveragePanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestCase;
			this.tstCoveragePanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.TestCase_Requirements;
			this.tstCoveragePanel.LoadAndBindData(true);

			//Specify the artifact type and artifact id to retrieve the test run list for
			this.tstTestRunListPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestCase;

			//Specify the artifact type and artifact id to retrieve the task association list for
			this.tstAssociationPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestCase;
			this.tstAssociationPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.ArtifactLink;

			//Specify the artifact type of the test set panel
			this.tstTestSetListPanel.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.TestCase;
			this.tstTestSetListPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.TestCase_TestSets;

			//Specify the artifact type to retrieve the incident list for
			this.tstIncidentListPanel.ArtifactTypeEnum = Artifact.ArtifactTypeEnum.TestCase;

			//First load any lookups needed by the test case details section
			TestCaseManager testCaseManager = new TestCaseManager();
			List<DataModel.User> allUsers = new UserManager().RetrieveForProject(this.ProjectId);
			List<DataModel.User> activeUsers = new UserManager().RetrieveActiveByProjectId(this.ProjectId);
			this.ddlAuthor.DataSource = allUsers;
			this.ddlOwner.DataSource = activeUsers;
			this.flagList = testCaseManager.RetrieveFlagLookup();
			List<TestCasePriority> priorities = testCaseManager.TestCasePriority_Retrieve(ProjectTemplateId);
			this.ddlPriority.DataSource = priorities;
			List<TestCaseType> testCaseTypes = testCaseManager.TestCaseType_Retrieve(ProjectTemplateId);
			this.ddlType.DataSource = testCaseTypes;
			this.ddlComponent.DataSource = new ComponentManager().Component_Retrieve(ProjectId);

			//Add the various custom properties to the table of fields
			UnityCustomPropertyInjector.CreateControls(
				ProjectId,
				ProjectTemplateId,
				DataModel.Artifact.ArtifactTypeEnum.TestCase,
				this.customFieldsDefault,
				this.ajxFormManager,
				this.customFieldsUsers,
				this.customFieldsDates,
				this.customFieldsRichText
				);

			//Databind
			this.DataBind();

			//Set the project/artifact for the RTE so that we can upload screenshots
			this.txtDescription.Screenshot_ProjectId = ProjectId;
			this.txtNewComment.Screenshot_ProjectId = ProjectId;

			//Populate the automation panel lookups
			AutomationManager automationManager = new AutomationManager();
			List<AutomationEngine> automationEngines = automationManager.RetrieveEngines();
			this.ddlAutomationEngine.DataSource = automationEngines;
			this.ddlAutomationEngine.DataBind();

			//Populate the folder and document type dropdowns
			AttachmentManager attachmentManager = new AttachmentManager();
			List<DocumentType> attachmentTypes = attachmentManager.RetrieveDocumentTypes(ProjectTemplateId, true);
			List<ProjectAttachmentFolderHierarchy> attachmentFolders = attachmentManager.RetrieveFoldersByProjectId(ProjectId);
			this.ddlDocType.DataSource = attachmentTypes;
			this.ddlDocFolder.DataSource = attachmentFolders;
			this.ddlDocType.DataBind();
			this.ddlDocFolder.DataBind();

			//Populate the list of test step columns to show/hide and databind
			this.ddlShowHideTestStepColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.TestStep);
			this.ddlShowHideTestStepColumns.DataBind();

			//See if there was a tab item passed.
			if (!string.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
			{
				string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

				switch (tabRequest)
				{
					case GlobalFunctions.PARAMETER_TAB_TESTRUN:
						tclTestCaseDetails.SelectedTab = this.pnlTestRuns.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_COVERAGE:
						tclTestCaseDetails.SelectedTab = this.pnlRequirements.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_ATTACHMENTS:
						tclTestCaseDetails.SelectedTab = this.pnlAttachments.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_HISTORY:
						tclTestCaseDetails.SelectedTab = this.pnlHistory.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
						tclTestCaseDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_INCIDENT:
						tclTestCaseDetails.SelectedTab = this.pnlIncidents.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_RELEASE:
						tclTestCaseDetails.SelectedTab = this.pnlReleases.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_TESTSET:
						tclTestCaseDetails.SelectedTab = this.pnlTestSets.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_ASSOCIATION:
						tclTestCaseDetails.SelectedTab = this.pnlAssociations.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_SIGNATURES:
						tclTestCaseDetails.SelectedTab = this.pnlSignature.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					default:
						tclTestCaseDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
						break;
				}
			}

			//Specify the context for the navigation bar
			this.navTestCaseList.ProjectId = ProjectId;
			this.navTestCaseList.ListScreenUrl = ReturnToTestCaseListUrl;
			this.navTestCaseList.SelectedItemId = this.testCaseId;
			if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
			{
				//If we came from the 'My Page' we need to set the navigation to 'My Assigned Test Cases'
				this.navTestCaseList.DisplayMode = NavigationBar.DisplayModes.Assigned;
				ProjectSettingsCollection settings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS);
				settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = (int)NavigationBar.DisplayModes.Assigned;
				settings.Save();
			}
			else
			{
				//Otherwise use the last setting the user used
				this.navTestCaseList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
			}
			this.navTestCaseList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
			this.navTestCaseList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

			//Specify the tab in the navigation control
			this.navTestCaseList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, -2, ArtifactTabName);
		}

		/// <summary>
		/// Returns the url to the list of test cases
		/// </summary>
		protected string ReturnToTestCaseListUrl
		{
			get
			{
				//Return to the appropriate page depending on the referrer
				if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
				{
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId);
				}
				else if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_REFERER_TEST_SET_DETAILS]))
				{
					int testSetId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_REFERER_TEST_SET_DETAILS]);
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, testSetId);
				}
				else
				{
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId);
				}
			}
		}

		public string GetTestMasterUrl()
		{
			string userName = new UserProfile().UserName;

			object returnData = null;
			string testMasterUrl = String.Empty;
			var vm_endpoint = "http://localhost/ValidationMaster";
			using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(GetConnection()))
			{
				var sqlCom = new System.Data.SqlClient.SqlCommand("SELECT * FROM SHAREPOINT_USERS WHERE TST_USERNAME='" + userName + "'", connection);
				connection.Open();

				using (System.Data.SqlClient.SqlDataReader dr = sqlCom.ExecuteReader())
				{
					while (dr.Read())
					{
						var spData = new
						{
							sUserName = Convert.ToString(dr["UserName"] == null ? "" : dr["UserName"]),
							sPassword = Convert.ToString(dr["Password"] == null ? "" : dr["Password"]),
							SharePointUrl = Convert.ToString(dr["SH_URL"] == null ? "" : dr["SH_URL"]),
							IsExist = true

						};
						returnData = spData;

						testMasterUrl = $"valtm:://vm_login={UserName};vm_endpoint={vm_endpoint};vm_project={this.ProjectId};vm_testcase={this.testCaseId};sp_login={spData.sUserName};sp_endpoint={spData.SharePointUrl}";
					}

				}
			}

			 
			return testMasterUrl;
		}

		private string GetConnection()
		{
			var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SpiraTestEntities"].ConnectionString;
			System.Data.EntityClient.EntityConnectionStringBuilder ecsb = new System.Data.EntityClient.EntityConnectionStringBuilder(connectionString);
			string providerConnection = ecsb.ProviderConnectionString;
			return providerConnection;
		}
	}


	#endregion
}

