using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls;
using System.Web.UI.HtmlControls;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Test Case Execution page and handling all raised events
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.TestCases, "TestCaseExecution_Title", "Test-Execution", "TestCaseExecution_Title")]
    public partial class TestCaseExecution : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TestCaseExecution::";

        private ProjectSettings projectSettings = null;
        protected TestRunsPending testRunsPending;
        protected Nullable<int> testSetId;
        protected Nullable<int> releaseId;
        protected int testRunsPendingId;
        protected string FullReferrerUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(ReferrerUrl);
            }
        }
        public string IncidentBaseUrl
        {
            get 
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -2));
            }
        }

        public string ScreenshotUploadUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -2, "{artifactType}"));
            }
        }
        public string ActualResultAlwaysRequired { get; private set; } = "false";
        public string RequireIncident { get; private set; } = "false";
        public string AllowTasks { get; private set; } = "false";

        #region Properties

        /// <summary>
        /// Is rich-text enabled for editing test steps
        /// </summary>
        public bool RichTextEnabled
        {
            get
            {
                return richTextEnabled;
            }
        }
        private bool richTextEnabled = true;

        #endregion

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Attach event handlers
            this.btnCancel.Click += new EventHandler(btnCancel_Click);
            //this.pbBtnLeave.Click += new EventHandler(pbBtnLeave_Click);
            this.pbBtnFinish.Click += new EventHandler(pbBtnFinish_Click);
            this.btnNoStepsBackToTests.Click += new EventHandler(btnNoStepsBackToTests_Click);

            //Get the ProjectSettings collection
            if (ProjectId > 0)
            {
                projectSettings = new ProjectSettings(ProjectId);
            }
            //Manage the project settings toggles for the page
            if (projectSettings != null)
            {
                //If the display of builds is disabled, make it hidden
                if (!projectSettings.Testing_ExecutionDisplayBuild)
                {
                    this.liBuildInfo.Visible = false;
                }
                //Disable different execution statuses per settings
                if (projectSettings.Testing_ExecutionDisablePassAll)
                {
                    this.btnExecutionStatusPassAll_grid.Visible = false;
                    this.btnExecutionStatusPassAll_inspector.Visible = false;
                }
                if (projectSettings.Testing_ExecutionDisableBlocked)
                {
                    this.btnExecutionStatusBlocked_grid.Visible = false;
                    this.btnExecutionStatusBlocked_inspector.Visible = false;
                }
                if (projectSettings.Testing_ExecutionDisableCaution)
                {
                    this.btnExecutionStatusCaution_grid.Visible = false;
                    this.btnExecutionStatusCaution_inspector.Visible = false;
                }
                if (projectSettings.Testing_ExecutionDisableNA)
                {
                    this.btnExecutionStatusNA_grid.Visible = false;
                    this.btnExecutionStatusNA_inspector.Visible = false;
                }
                if (projectSettings.Testing_ExecutionActualResultAlwaysRequired)
                {
                    ActualResultAlwaysRequired = "true";
                }
                if (projectSettings.Testing_ExecutionRequireIncident)
                {
                    RequireIncident = "true";
                }
                if (Common.Global.Feature_Tasks && projectSettings.Testing_ExecutionAllowTasks)
                {
                    AllowTasks = "true";
                }
            }

            //Get the pending test run id from the querystring
            if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TEST_RUNS_PENDING_ID]))
            {
                //Make sure that the we're passed a pending test run id
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "No pending test run specified.", true);
            }
            //Get the test-run-pending-id and use that to retrieve the test-run information
            this.testRunsPendingId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_RUNS_PENDING_ID]);

            //Set the label of the attachments
            tstAttachmentPanel.MessageLabelHandle = this.lblMessage;

            //Specify the context for the test run ajax form manager
            this.ajxTestRunFormManager.ProjectId = ProjectId;
            this.ajxTestRunFormManager.PrimaryKey = this.testRunsPendingId;

            //Specify the context for the incident ajax form manager
            this.ajxIncidentFormManager.ProjectId = ProjectId;
            this.ajxIncidentFormManager.PrimaryKey = null;  //New incidents only

            //Add client-side event handlers
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("loaded", "pageWizard.ajxTestRunFormManager_loaded");
            handlers.Add("dataSaved", "pageWizard.ajxTestRunFormManager_dataSaved");
            handlers.Add("dataFailure", "pageWizard.ajxTestRunFormManager_dataFailure");
            this.ajxTestRunFormManager.SetClientEventHandlers(handlers);

            //Retrieve the saved pending test run set
            TestRunManager testRunManager = new TestRunManager();
            try
            {
                this.testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

                //Make sure that the current project is set to the project in the test run
                //this is important since the page may get loaded from an email notification URL
                VerifyArtifactProject(testRunsPending.ProjectId, UrlRoots.NavigationLinkEnum.TestExecute, testRunsPendingId);

                //Make sure that the current user is the owner of this pending test run or at least one of the test runs in the group
                bool isAnOwner = false;
                if (testRunsPending.TesterId == this.UserId)
                {
                    isAnOwner = true;
                }
                foreach (TestRun testRun in testRunsPending.TestRuns)
                {
                    if (testRun.TesterId == this.UserId)
                    {
                        isAnOwner = true;
                    }
                }
                if (!isAnOwner)
                {
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.TestCaseExecution_NotAuthorizedToResume, true);
                }

                //Make sure that we have at least one test run in the pending list
                if (testRunsPending.TestRuns.Count == 0)
                {
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "There are no test runs in this pending set to be resumed.", true);
                }

                //Load the first page
                LoadAndBindFirstPage();
                LoadAndBindTestCaseAndStepCustomProperties();

                //Now load the various incident lookups
                LoadIncidentLookups();

                //Now bind the datasource for the task owner dropdown
                Business.UserManager userManager = new Business.UserManager();
                List<DataModel.User> users = userManager.RetrieveActiveByProjectId(this.ProjectId);
                this.ddlTaskOwner.DataSource = users;
                this.ddlTaskOwner.DataBind();
            }
            catch (ArtifactNotExistsException)
            {
                //If the artifact doesn't exist let the user know nicely
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The pending test run you selected has been deleted from the system.", true);
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();

        }

        /// <summary>
        /// Loads and databinds the first page used to select the release and Test Run custom properties
        /// </summary>
        protected void LoadAndBindFirstPage()
        {
            //See if we need the user to select a release and custom properties for this project
            //this.lblRelease.Text = "";
            string releaseVersionNumber = "";
            if (this.testRunsPending.TestRuns[0].ReleaseId.HasValue)
            {
                this.releaseId = this.testRunsPending.TestRuns[0].ReleaseId.Value;
                releaseVersionNumber = this.testRunsPending.TestRuns[0].Release.VersionNumber;
                //this.lblRelease.Text = Resources.Fields.ReleaseVersionNumber + ": <b>" + releaseVersionNumber + "</b>";
            }
            if (this.testRunsPending.TestSetId.HasValue)
            {
                this.testSetId = this.testRunsPending.TestSetId;
            }

            //See if we have releases
            Business.ReleaseManager releaseManager = new Business.ReleaseManager();
            List<ReleaseView> releases = releaseManager.RetrieveByProjectId(this.ProjectId, true);

            //We now need to inject the custom properties into the first page table
            UnityCustomPropertyInjector.CreateControls(
                this.ProjectId,
                ProjectTemplateId,
                Artifact.ArtifactTypeEnum.TestRun, 
                this.customFieldsTestRunDefault,
                this.ajxTestRunFormManager,
                this.customFieldsTestRunDefault,
                this.customFieldsTestRunDefault,
                this.customFieldsTestRunRichText
                );

            if (releases.Count > 0)
            {
                //Set the datasource for the release drop-down list
                ddlRelease.DataSource = releases;
                this.ddlRelease.Enabled = true;
                this.ddlRelease.NoValueItem = false;
            }
            else
            {
                ddlRelease.DataSource = releases;
                this.ddlRelease.NoValueItem = true;
                this.ddlRelease.NoValueItemText = Resources.Dialogs.TestCaseExecution_NoReleasesDropDown;
                this.ddlRelease.Enabled = false;
                this.ddlBuild.NoValueItem = true;
                this.ddlBuild.NoValueItemText = Resources.Dialogs.TestCaseExecution_NoReleasesDropDown;
                this.ddlBuild.Enabled = false;
            }

            //Databind the release dropdown
            this.ddlRelease.DataBind();
        }

        /// <summary>
        /// Loads and creates the custom properties for test cases and test steps for their optional display on the main page
        /// /// </summary>
        protected void LoadAndBindTestCaseAndStepCustomProperties()
        {
            //inject the custom properties for the test run (ie those from the matching test case)
            UnityCustomPropertyInjector.CreateControls(
                this.ProjectId,
                ProjectTemplateId,
                Artifact.ArtifactTypeEnum.TestCase,
                this.customFieldsTestRunCaseDefault,
                this.ajxTestRunCaseFormManager,
                this.customFieldsTestRunCaseDefault,
                this.customFieldsTestRunCaseDefault,
                this.customFieldsTestRunCaseRichText,
                isReadOnly: true
                );

            //inject the custom properties for the test run step
            UnityCustomPropertyInjector.CreateControls(
                this.ProjectId,
                ProjectTemplateId,
                Artifact.ArtifactTypeEnum.TestStep,
                this.customFieldsTestRunStepDefault,
                this.ajxTestRunStepFormManager,
                this.customFieldsTestRunStepDefault,
                this.customFieldsTestRunStepDefault,
                this.customFieldsTestRunStepRichText,
                isReadOnly: true
                );
        }

        /// <summary>
        /// Loads the incident lookups and custom properties
        /// </summary>
        protected void LoadIncidentLookups()
        {
            IncidentManager incidentManager = new IncidentManager();
            Business.UserManager userManager = new Business.UserManager();
            List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(this.ProjectTemplateId, true);
            this.ddlPriority.DataSource = incidentPriorities;
            List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(this.ProjectTemplateId, true);
            this.ddlSeverity.DataSource = incidentSeverities;
            List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(this.ProjectTemplateId, true);
            this.ddlIncidentType.DataSource = incidentTypes;
            List<ReleaseView> releases = new Business.ReleaseManager().RetrieveByProjectId(this.ProjectId);
            this.ddlResolvedRelease.DataSource = releases;
            this.ddlVerifiedRelease.DataSource = releases;
            List<DataModel.User> users = userManager.RetrieveActiveByProjectId(this.ProjectId);
            this.ddlOwner.DataSource = users;
            List<Component> components = new ComponentManager().Component_Retrieve(this.ProjectId, true);
            this.ddlComponent.DataSource = components;

            //Create the custom properties
            UnityCustomPropertyInjector.CreateControls(
                ProjectId,
                ProjectTemplateId,
                DataModel.Artifact.ArtifactTypeEnum.Incident,
                this.customFieldsIncidentsDefault,
                this.ajxIncidentFormManager,
                this.customFieldsIncidentsUsers,
                this.customFieldsIncidentsDates,
                this.customFieldsIncidentsRichText
                );


            //Databind
            this.pnlTestExecution_Incidents.DataBind();

            //Set the default incident type
            int incidentTypeId = incidentManager.GetDefaultIncidentType(this.ProjectTemplateId);
            try
            {
                this.ddlIncidentType.SelectedValue = incidentTypeId.ToString();
            }
            catch (ArgumentOutOfRangeException)
            {
                //Do nothing
            }
        }
        
        /// <summary>
        /// Redirects back to the referring page when cancel clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnCancel_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            ReturnToReferrer();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Redirects back to the referring page when clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNoStepsBackToTests_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnNoStepsBackToTests_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            ReturnToReferrer();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Completes the pending test run entry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pbBtnFinish_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "pbBtnFinish_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Create the business object(s)
            TestRunManager testRunManager = new TestRunManager();

            //Get the user and current pending run id
            int testRunsPendingId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_RUNS_PENDING_ID]);

            //Mark the set of pending test runs as complete
            testRunManager.CompletePending(testRunsPendingId, this.UserId);

            //Redirect to the referring page
            ReturnToReferrer();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Returns the referring URL
        /// </summary>
        protected string ReferrerUrl
        {
            get
            {
                //Return to the referring page
                if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
                {
                    return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0);
                }
                else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_TEST_CASE_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_TEST_CASE_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
                {
                    return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId);
                }
                else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_TEST_SET_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_TEST_SET_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
                {
                    return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId);
                }
                else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_TEST_SET_DETAILS] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_TEST_SET_DETAILS] == GlobalFunctions.PARAMETER_VALUE_TRUE && this.testSetId.HasValue && this.testSetId.Value > 0)
                {
                    return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, this.testSetId.Value);
                }
                else
                {
                    return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId);
                }
            }
        }

        /// <summary>
        /// Ends execution and returns to the calling page
        /// </summary>
        protected void ReturnToReferrer()
        {
            Response.Redirect(ReferrerUrl, true);
        }

    }
}