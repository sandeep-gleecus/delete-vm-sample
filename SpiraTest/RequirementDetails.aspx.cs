using System;
using System.Linq;
using System.Data;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System.Collections.Generic;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// details of a particular requirement and its test coverage information
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Requirements, null, "Requirements-Management/#requirement-details", "RequirementDetails_Title")]
	public partial class RequirementDetails : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.RequirementDetails::";
		
		protected int requirementId;
		protected string ArtifactTabName = null;

		#region Event Handlers

		/// <summary>
		/// Adds any event handlers that need to be in place before the Load() event
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			//Call the base functionality first
			base.OnInit(e);

			//Pass the form manager to the task panel
            this.tstTaskListPanel.FormManager = this.ajxFormManager;
		}

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Determine if we are meant to display rich-text descriptions or not
			this.txtNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);

			//Capture the passed test case id from the querystring
			this.requirementId = System.Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_REQUIREMENT_ID]);

            //Make sure that the current project is set to the project associated with the artifact
            //this is important since the page may get loaded from an email notification URL or cross-project association
            VerifyArtifactProject(UrlRoots.NavigationLinkEnum.Requirements, this.requirementId);

			//Add the URL to the release hierarchical drop-down
			this.ddlRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);

            //Set the permissions and action on the Add Comment button
            this.btnNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);
            this.btnNewComment.ClientScriptMethod = String.Format("add_comment('{0}')", this.txtNewComment.ClientID);

            //Should we display the estimate in hours
            ProjectSettings projectSettings = new ProjectSettings(ProjectId);
            this.plcEstimatedEffort.Visible = projectSettings.DisplayHoursOnPlanningBoard;

            //Only load the data once
            if (!IsPostBack)
			{
				LoadAndBindData();
			}

			//Add the various panel event handlers
            this.tstCoveragePanel.MessageLabelHandle = this.lblMessage;
            this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;
			this.tstTaskListPanel.MessageLabelHandle = this.lblMessage;
            this.tstAssociationPanel.MessageLabelHandle = this.lblMessage;

			//Reset the error message
			this.lblMessage.Text = "";

			//Specify the context for the navigation bar
			this.navRequirementsList.ProjectId = ProjectId;

			//Specify the context for the ajax form control
			this.ajxFormManager.ProjectId = this.ProjectId;
            this.ajxFormManager.PrimaryKey = this.requirementId;
            this.ajxFormManager.ArtifactTypePrefix = Requirement.ARTIFACT_PREFIX;
            this.ajxFormManager.FolderPathUrlTemplate = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -2));
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("dataSaved", "ajxFormManager_dataSaved");
            handlers.Add("loaded", "ajxFormManager_loaded");
            handlers.Add("operationReverted", "ajxFormManager_operationReverted");
            this.ajxFormManager.SetClientEventHandlers(handlers);

            //Register client handlers on the tab control
            Dictionary<string, string> handlers3 = new Dictionary<string, string>();
            handlers3.Add("selectedTabChanged", "tclRequirementDetails_selectedTabChanged");
            this.tclRequirementDetails.SetClientEventHandlers(handlers3);

            //Populate the project id in the AJAX scenario grid control
            this.grdScenarioSteps.ProjectId = this.ProjectId;

            //Custom CSS for the scenario grid
            Dictionary<string, string> scenarioCssClasses = new Dictionary<string, string>();
            scenarioCssClasses.Add("Description", "priority1");
            this.grdScenarioSteps.SetCustomCssClasses(scenarioCssClasses);

            //Set the context on the workflow operations control
            Dictionary<string, string> handlers2 = new Dictionary<string, string>();
            handlers2.Add("operationExecuted", "ajxWorkflowOperations_operationExecuted");
            this.ajxWorkflowOperations.ProjectId = ProjectId;
            this.ajxWorkflowOperations.PrimaryKey = this.requirementId;
            this.ajxWorkflowOperations.SetClientEventHandlers(handlers2);

            //Set the context on the comment list
            this.lstComments.ProjectId = ProjectId;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Returns the URL to the requirements list or project home depending on the referrer
		/// </summary>
		protected string ReturnToRequirementsListUrl
		{
			get
			{
				//Return the URL to the requirements list or project home depending on the referrer
				if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME] == GlobalFunctions.PARAMETER_VALUE_TRUE)
				{
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0);
				}
				else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
				{
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0);
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
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, 0);
				}
			}
		}

        /// <summary>
        /// Returns the url to the generic requirement page with a token for the requirement id
        /// </summary>
        protected string RequirementRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -2));
            }
        }

		/// <summary>
		/// This method populates the lookup dropdowns and loads the appropriate requirement dataset
		/// </summary>
		protected void LoadAndBindData()
		{
			//Now load in the lookups needed for the requirements details
            RequirementManager requirementManager = new RequirementManager();
            Business.ReleaseManager releaseManager = new Business.ReleaseManager();
            ComponentManager componentManager = new ComponentManager();
			List<Importance> importances = requirementManager.RequirementImportance_Retrieve(ProjectTemplateId);
            List<RequirementType> types = requirementManager.RequirementType_Retrieve(ProjectTemplateId, true);
            List<DataModel.User> allUsers = new UserManager().RetrieveForProject(this.ProjectId);
            List<DataModel.User> activeUsers = new UserManager().RetrieveActiveByProjectId(this.ProjectId);
            List<ReleaseView> releases = releaseManager.RetrieveByProjectId(this.ProjectId, false, true);
            List<Component> components = componentManager.Component_Retrieve(this.ProjectId, true);

            //Force the package type to be inactive
            IEnumerable<RequirementType> virtualTypes = types.Where(r => r.RequirementTypeId < 0);
            foreach (RequirementType virtualType in virtualTypes)
            {
                virtualType.IsActive = false;
            }

			//Set the datasources
            this.ddlImportance.DataSource = importances;
            this.ddlAuthor.DataSource = allUsers;
            this.ddlOwner.DataSource = activeUsers;
            this.ddlRelease.DataSource = releases;
            this.ddlType.DataSource = types;
            this.ddlComponent.DataSource = components;
            this.ddlSplitRequirementOwner.DataSource = activeUsers;

            //Specify the artifact type and artifact id to retrieve the task list for
            this.tstTaskListPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Requirement;

            //Specify the artifact type and artifact id to retrieve the linked associations list for
            this.tstAssociationPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Requirement;
            this.tstAssociationPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.ArtifactLink;
            this.tstAssociationPanel.LoadAndBindData(true);

            //Specify the artifact type and artifact id to retrieve the test case coverage for
            this.tstCoveragePanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Requirement;
            this.tstCoveragePanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.Requirement_TestCases;
            this.tstCoveragePanel.LoadAndBindData(true);

			//Specify the artifact type and artifact id to retrieve the attachments for
			this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Requirement;
			this.tstAttachmentPanel.LoadAndBindData(true);
            this.tstAttachmentPanel.ArtifactId = this.requirementId;

            //Specify the artifact type and artifact id to retrieve the history log for
            this.tstHistoryPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Requirement;
            this.tstHistoryPanel.ShowTestStepData = true;   //Shows use case steps
            this.tstHistoryPanel.LoadAndBindData(true);

            //Add the various custom properties to the table of fields
            UnityCustomPropertyInjector.CreateControls(
                ProjectId,
                ProjectTemplateId,
                DataModel.Artifact.ArtifactTypeEnum.Requirement,
                this.customFieldsDefault,
                this.ajxFormManager,
                this.customFieldsUsers,
                this.customFieldsDates,
                this.customFieldsRichText
                );

            //Set the project/artifact for the RTE so that we can upload screenshots
            this.txtDescription.Screenshot_ProjectId = ProjectId;
            this.txtDescription.Screenshot_ArtifactId = this.requirementId;
            this.txtNewComment.Screenshot_ProjectId = ProjectId;
            this.txtNewComment.Screenshot_ArtifactId = this.requirementId;

			//Databind the main page
			this.DataBind();

			//Get the task list and databind
			tstTaskListPanel.LoadAndBindData(true);

			//Populate the link back to the list page
			this.navRequirementsList.ListScreenUrl = ReturnToRequirementsListUrl;
			this.navRequirementsList.SelectedItemId = this.requirementId;
			if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
			{
				//If we came from the 'My Page' we need to set the navigation to 'My Assigned Requirements'
				this.navRequirementsList.DisplayMode = NavigationBar.DisplayModes.Assigned;
				ProjectSettingsCollection settings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS);
				settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = (int)NavigationBar.DisplayModes.Assigned;
				settings.Save();
			}
			else
			{
				//Otherwise use the last setting the user used
				this.navRequirementsList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
			}
			this.navRequirementsList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
			this.navRequirementsList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

			//See if we need to load a tab up.
			if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
			{
				string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

				switch (tabRequest)
				{
					case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
						tclRequirementDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_ATTACHMENTS:
						tclRequirementDetails.SelectedTab = this.pnlAttachments.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_HISTORY:
						tclRequirementDetails.SelectedTab = this.pnlHistory.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_DIAGRAM:
						tclRequirementDetails.SelectedTab = this.pnlDiagram.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_TASK:
						tclRequirementDetails.SelectedTab = this.pnlTasks.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_ASSOCIATION:
                        tclRequirementDetails.SelectedTab = this.pnlAssociations.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_TESTCASE:
					case GlobalFunctions.PARAMETER_TAB_COVERAGE:
						tclRequirementDetails.SelectedTab = this.pnlCoverage.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					default:
						tclRequirementDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
						break;
				}
			}

			//Specify the tab in the navigation control
			this.navRequirementsList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -2, ArtifactTabName);
		}

		#endregion
	}
}
