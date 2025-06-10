using System;
using System.Web.UI.WebControls;
using System.Collections.Generic;
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
	/// details of a particular test step and associated information
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.TestCases, null, "Test-Case-Management/#test-step-details", "TestStepDetails_Title")]
	public partial class TestStepDetails : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TestStepDetails::";

		//Other variables
		protected int testStepId;
        protected int testCaseId;
		protected string ArtifactTabName = null;

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

			//Capture the passed test step id from the querystring
			this.testStepId = System.Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_TEST_STEP_ID]);

			//Link the message label handle to the user controls
			this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;
			this.tstHistoryPanel.MessageLabelHandle = this.lblMessage;
            this.tstCoveragePanel.MessageLabelHandle = this.lblMessage;

			//Only load the data once
			if (!IsPostBack)
			{
				//Specify the artifact type and artifact id for the various panels
				this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestStep;
				this.tstHistoryPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestStep;

                //Specify the artifact type and artifact id to retrieve the requirement coverage for
                this.tstCoveragePanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.TestStep;
                this.tstCoveragePanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.TestStep_Requirements;
                this.tstCoveragePanel.LoadAndBindData(true);

				//On first load get the viewing option from the querystring
				if (!string.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
				{
					string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

					switch (tabRequest)
					{
						case GlobalFunctions.PARAMETER_TAB_ATTACHMENTS:
							tclTestStepDetails.SelectedTab = this.pnlAttachments.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

						case GlobalFunctions.PARAMETER_TAB_HISTORY:
							tclTestStepDetails.SelectedTab = this.pnlHistory.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

						case GlobalFunctions.PARAMETER_TAB_INCIDENT:
							tclTestStepDetails.SelectedTab = this.pnlIncidents.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

                        case GlobalFunctions.PARAMETER_TAB_REQUIREMENT:
                            tclTestStepDetails.SelectedTab = this.pnlRequirements.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

						default:
							tclTestStepDetails.SelectedTab = this.pnlIncidents.ClientID;
							this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_INCIDENT;
							break;
					}
				}

				//Load and databind the various datasets
				LoadAndBindData();
			}

			//Reset the error message
			this.lblMessage.Text = "";

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion

		#region Methods Used Internally

        /// <summary>
        /// Redirects back to the list page
        /// </summary>
        protected string ReturnToListPageUrl
        {
            get
            {
                //Redirect to the appropriate page
                return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, this.testCaseId);
            }
        }

        /// <summary>
        /// Returns the url to the generic test step page with a token for the test step id
        /// </summary>
        protected string TestStepRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSteps, ProjectId, -2));
            }
        }

		/// <summary>
		/// Loads the various datasets used in the form
		/// </summary>
		private void LoadAndBindData()
		{
			const string METHOD_NAME = "LoadAndBindData";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			this.testStepId = System.Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_TEST_STEP_ID]);

            //Need to load in the test step
            TestCaseManager testCaseManager = new TestCaseManager();
            TestStep testStep = testCaseManager.RetrieveStepById(this.ProjectId, this.testStepId);
            if (testStep == null)
            {
                //If the artifact doesn't exist let the user know nicely
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.TestCaseDetails_ArtifactNotExists, true);
            }

            //Store the test case id
            this.testCaseId = testStep.TestCaseId;

			//First make sure that the current project is set to the project in the test case
			//this is important since the page may get loaded from an email notification URL
            VerifyArtifactProject(testStep.TestCase.ProjectId, UrlRoots.NavigationLinkEnum.TestSteps, testStepId);

            //Specify if the current user created/owns this artifact (used for permission-checking)
            SpiraContext.Current.IsArtifactCreatorOrOwner = (testStep.TestCase.AuthorId == UserId || (testStep.TestCase.OwnerId.HasValue && testStep.TestCase.OwnerId.Value == UserId));

            //Redirect if the user has limited view permissions and does not have this flag set. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.TestStep, Project.PermissionEnum.View) == Project.AuthorizationState.Limited && !SpiraContext.Current.IsArtifactCreatorOrOwner)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //Make sure that this is not actually a test link (they would need to spoof the URL to get this condition)
            if (testStep.LinkedTestCaseId.HasValue)
            {
                //Return to the test case details page
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, this.ProjectId, testStep.TestCaseId), true);
            }

            //Get the list of attachments and databind the grid
            tstAttachmentPanel.ArtifactId = testStepId;
			tstAttachmentPanel.LoadAndBindData(true);

			//Get the history change log and databind the grid
			tstHistoryPanel.LoadAndBindData(true);

            //Add the various custom properties to the table of fields
            UnityCustomPropertyInjector.CreateControls(
                ProjectId,
                ProjectTemplateId,
                DataModel.Artifact.ArtifactTypeEnum.TestStep,
                this.customFieldsDefault,
                this.ajxFormManager,
                this.customFieldsDefault,
                this.customFieldsDefault,
                this.customFieldsRichText
                );

            //Populate the incident grid show/hide columns list
            this.ddlShowHideIncidentColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.Incident);

            //Set the context on the rich text boxes
            this.txtDescription.Screenshot_ProjectId = ProjectId;
            this.txtExpectedResult.Screenshot_ProjectId = ProjectId;
            this.txtSampleData.Screenshot_ProjectId = ProjectId;

            //Databind the page
            this.DataBind();

            //Specify the context for the navigation bar
            this.navTestStepList.ProjectId = ProjectId;
            this.navTestStepList.ListScreenUrl = ReturnToListPageUrl;
            this.navTestStepList.ContainerId = testStep.TestCaseId;
            if (this.testStepId != -1)
            {
                this.navTestStepList.SelectedItemId = this.testStepId;
            }

            this.navTestStepList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_STEP_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.AllItems);
            this.navTestStepList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_STEP_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.navTestStepList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_STEP_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

            //Specify the base URL in the navigation control
            this.navTestStepList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSteps, ProjectId, -2, ArtifactTabName);

            //Specify the context for the ajax form manager
            this.ajxFormManager.ProjectId = this.ProjectId;
            this.ajxFormManager.PrimaryKey = this.testStepId;
            this.ajxFormManager.ArtifactTypePrefix = TestStep.ARTIFACT_PREFIX;
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("dataSaved", "ajxFormManager_dataSaved");
            handlers.Add("loaded", "ajxFormManager_loaded");
            this.ajxFormManager.SetClientEventHandlers(handlers);

            //Populate the user and project id in the incident AJAX grid control
            //Need to tell the incident list that it's being filtered on a specific test step
            this.grdIncidentList.ProjectId = this.ProjectId;
            this.grdIncidentList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -2);
            this.grdIncidentList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_INCIDENT;
            Dictionary<string, object> incidentFilters = new Dictionary<string, object>();
            incidentFilters.Add("TestStepId", this.testStepId);
            this.grdIncidentList.SetFilters(incidentFilters);

            //Make sure the test step is not locked due to its workflow status
            //We can do this in code-behind because all the test steps would be affected (live loading does not change)
            if (!testCaseManager.AreTestStepsEditableInStatus(testCaseId))
            {
                this.btnSave.Enabled = false;
                this.btnCreate.Enabled = false;
                this.btnDelete.Enabled = false;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion
	}
}
