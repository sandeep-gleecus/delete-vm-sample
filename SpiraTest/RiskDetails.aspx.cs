#pragma warning disable 1591

using System;
using System.Collections;
using System.Threading;
using System.Web.UI;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;
using System.Globalization;
using Inflectra.SpiraTest.Web.ServerControls.Authorization;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Risks Details/Edit and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Risks, null, "Risks-Management/#risk-details", "RiskDetails_Title")]
	public partial class RiskDetails : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.RiskDetails::";
		
		protected string ArtifactTabName = null;

		//Local variables
		protected int testRunId;
		protected int releaseId;
        protected Placeholder _placeholder;
        protected AuthorizedControlBase authorizedControlBase;
        protected int riskId;
        protected int placeholderId;

		#region Properties
        
        /// <summary>
        /// Returns the url to the generic risk page with a token for the risk id
        /// </summary>
        protected string RiskRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, ProjectId, -2));
            }
        }

        /// <summary>
        /// Returns the url to the risk list page
        /// </summary>
        protected string RiskListUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(ReturnToRiskListUrl);
            }
        }

        /// <summary>
        /// Returns the url to the new risk page
        /// </summary>
        protected string RiskNewUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, ProjectId, -1));
            }
        }

        /// <summary>
        /// Returns the current user's authorization for the page
        /// </summary>
        protected Project.PermissionEnum Authorized_Permission
        {
            get
            {
                return authorizedControlBase.Authorized_Permission;
            }
            set
            {
                authorizedControlBase.Authorized_Permission = value;
            }
        }

        #endregion

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
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Reset the error message
			this.lblMessage.Text = "";

			//If we're passed in a test run id then store that locally
			if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TEST_RUN_ID]))
				this.testRunId = -1;
			else
				this.testRunId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_RUN_ID]);

			//If we're passed in a release id then store locally
			if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_RELEASE_ID]))
				this.releaseId = -1;
			else
				this.releaseId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_RELEASE_ID]);

			//Determine if we should use rich-text boxes
			this.txtNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);

			//Link the message label handle to the user controls
			this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;
            this.tstAssociationPanel.MessageLabelHandle = this.lblMessage;

			//Add the URL to the release hierarchical drop-downs
			this.ddlRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);

			//Only load the data once
			if (!IsPostBack)
			{
				//Capture the passed risk id from the querystring - use '-1' to denote missing
				//i.e. inserting a new risk instead of updating existing
				if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_RISK_ID]))
					this.riskId = -1;
				else
					this.riskId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_RISK_ID]);

				//Load the data and databind
				LoadAndBindData();
			}

            //Set the action on the Add Comment button
            this.btnNewComment.ClientScriptMethod = String.Format("add_comment('{0}')", this.txtNewComment.ClientID);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Takes the user page to the appropriate risk list page
		/// </summary>
		protected string ReturnToRiskListUrl
		{
			get
			{
				//Return to the project list/home if we were passed a referrer
				if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
				{
					return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId, 0);
				}
                else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST_DETECTED] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST_DETECTED] == GlobalFunctions.PARAMETER_VALUE_TRUE)
                {
                    return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId, 0);
                }
				else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME] == GlobalFunctions.PARAMETER_VALUE_TRUE)
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
					//If we were passed in a test run id then redirect back to the test run details page
					//If we we were passed in a release id then redirect back to the release details page
					//Otherwise redirect back to the risk list page
					if (this.testRunId != -1)
					{
						//Redirect back to the Test Run Details Page
						return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestRuns, ProjectId, testRunId);
					}
					else if (this.releaseId != -1)
					{
						//Redirect back to the Test Run Details Page
						return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, releaseId);
					}
					else
					{
						//Redirect back to the Risk List Page
						return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, ProjectId);
					}
				}
			}
		}

		#endregion

		#region Methods used internally to the class

		/// <summary>
		/// This method populates the lookup dropdowns and loads the appropriate risk dataset
		/// </summary>
		private void LoadAndBindData()
		{
            //Instantiate the business classes
            RiskManager riskManager = new RiskManager();

			//Retrieve all the lookups for the drop-downs
            List<RiskType> riskTypes = riskManager.RiskType_Retrieve(this.ProjectTemplateId, true);
            List<RiskProbability> riskProbabilities = riskManager.RiskProbability_Retrieve(this.ProjectTemplateId, true);
            List<RiskImpact> riskImpacts = riskManager.RiskImpact_Retrieve(this.ProjectTemplateId, true);
            List<DataModel.User> allUsers = new UserManager().RetrieveForProject(this.ProjectId);
            List<DataModel.User> activeUsers = new UserManager().RetrieveActiveByProjectId(this.ProjectId);
            List<Component> components = new ComponentManager().Component_Retrieve(this.ProjectId, true);

			Business.ReleaseManager releaseManager = new Business.ReleaseManager();
            List<ReleaseView> releases = releaseManager.RetrieveByProjectId(this.ProjectId, false);
            List<ReleaseView> activeReleases = releaseManager.RetrieveByProjectId(this.ProjectId, true);

            this.ddlCreator.DataSource = allUsers;
            this.ddlOwner.DataSource = activeUsers;
            this.ddlRiskType.DataSource = riskTypes;
            this.ddlProbability.DataSource = riskProbabilities;
            this.ddlImpact.DataSource = riskImpacts;
            this.ddlComponent.DataSource = components;

            //Set the context that applies to both new risk and update cases
            //Populate the context of the form manager and register client-side event handlers
            this.ajxFormManager.ProjectId = ProjectId;
            this.ajxFormManager.ArtifactTypePrefix = Risk.ARTIFACT_PREFIX;

            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("dataSaved", "ajxFormManager_dataSaved");
            handlers.Add("loaded", "ajxFormManager_loaded");
            handlers.Add("operationReverted", "riskDetails_ajxFormManager_operationReverted");
            this.ajxFormManager.SetClientEventHandlers(handlers);

            //Set the context on the workflow operations controls
            Dictionary<string, string> handlers2 = new Dictionary<string, string>();
            handlers2.Add("operationExecuted", "riskDetails_ajxWorkflowOperations_operationExecuted");

            this.ajxWorkflowOperations.ProjectId = ProjectId;
            this.ajxWorkflowOperations.PrimaryKey = riskId;
            this.ajxWorkflowOperations.SetClientEventHandlers(handlers2);

            //Set the context on the comment list
            this.lstComments.ProjectId = ProjectId;
            this.lstComments.ArtifactId = riskId;

            //Set the project ID on the RTEs so that the plugin gets loaded
            this.txtDescription.Screenshot_ProjectId = ProjectId;
            this.txtNewComment.Screenshot_ProjectId = ProjectId;

            //For existing risks, the 'detected-in-release' should display all releases (active and inactive)
            //Resolved and verified always show just active releases. However we bind to all releases and let the dropdown
            //filter out inactive ones. That prevents the display showing 'none' if the current value is not active
            this.ddlRelease.DataSource = releases;

            //Add the various custom properties to the table of fields
            UnityCustomPropertyInjector.CreateControls(
                ProjectId,
                ProjectTemplateId,
                DataModel.Artifact.ArtifactTypeEnum.Risk, 
                this.customFieldsDefault, 
                this.ajxFormManager, 
                this.customFieldsUsers, 
                this.customFieldsDates, 
                this.customFieldsRichText
                );

            //Populate the project id in the AJAX mitigations grid control
            this.grdMitigations.ProjectId = this.ProjectId;

            //Custom CSS for the mitigations grid
            Dictionary<string, string> mitigationsCssClasses = new Dictionary<string, string>();
            mitigationsCssClasses.Add("Description", "priority1");
            this.grdMitigations.SetCustomCssClasses(mitigationsCssClasses);

            //If we editing an existing risk, load the items, populate the default field values
            //and set the button caption to 'Update'
            if (this.riskId == -1)
			{
                //Create a new placeholder to store any attachments and specify that on the various panels
                this._placeholder = new PlaceholderManager().Placeholder_Create(ProjectId);
                this.placeholderId = this._placeholder.PlaceholderId;

                //Attachments
                this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Placeholder;
                this.tstAttachmentPanel.ArtifactId = this.placeholderId;
                this.tstAttachmentPanel.LoadAndBindData(true);

				//Default to Overview tab for new risks
				tclRiskDetails.SelectedTab = this.pnlOverview.ClientID;

                //Populate the context of the form manager
                this.ajxFormManager.PrimaryKey = null;

                //Set the RTE permissions
                this.txtDescription.Authorized_Permission = Project.PermissionEnum.Create;

                //Databind the controls
                this.DataBind();
			}
			else
			{
                //Existing risk update starting point
                this.ajxFormManager.PrimaryKey = riskId;

                //Get the list of attachments
                this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Risk;
                this.tstAttachmentPanel.ArtifactId = this.riskId;
                this.tstAttachmentPanel.LoadAndBindData(true);

                //Get the list of history items and databind the grid
                this.tstHistoryPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Risk;
                this.tstHistoryPanel.ShowTestStepData = true;   //Shows mitigations
                this.tstHistoryPanel.LoadAndBindData(true);

                //Set the RTE permissions
                this.txtDescription.Authorized_Permission = Project.PermissionEnum.Modify;

                //Databind the controls
                this.DataBind();

                //See if a tab's been specified.
                if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
                {
					string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

					switch (tabRequest)
					{
                        case GlobalFunctions.PARAMETER_TAB_ATTACHMENTS:
                            tclRiskDetails.SelectedTab = this.pnlAttachments.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

                        case GlobalFunctions.PARAMETER_TAB_HISTORY:
                            tclRiskDetails.SelectedTab = this.pnlHistory.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

						case GlobalFunctions.PARAMETER_TAB_TASK:
							tclRiskDetails.SelectedTab = this.pnlTasks.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

						case GlobalFunctions.PARAMETER_TAB_ASSOCIATION:
                            tclRiskDetails.SelectedTab = this.pnlAssociations.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

                        case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
                            tclRiskDetails.SelectedTab = this.pnlOverview.ClientID;
							this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
							break;
                    }
                }

                //Retrieve the risk in question
                Risk risk = null;
                try
                {
                    risk = riskManager.Risk_RetrieveById(this.riskId);
                }
                catch (ArtifactNotExistsException)
                {
                    //If the artifact doesn't exist let the user know nicely
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The risk you selected has been deleted from the system.", true);
                }

                //Make sure that the current project is set to the project in the risk
                //this is important since the page may get loaded from an email notification URL
                VerifyArtifactProject(risk.ProjectId, UrlRoots.NavigationLinkEnum.Risks, riskId);
			}

            //and the page title for the "new risk" name
            this.ajxFormManager.NewItemName = Resources.Dialogs.Global_NewRisk;

            //Specify the association panel context
            this.tstAssociationPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Risk;
            this.tstAssociationPanel.ArtifactId = this.riskId;
            this.tstAssociationPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.ArtifactLink;
            this.tstAssociationPanel.LoadAndBindData(true);

            //Set the task panel context
            this.tstTaskListPanel.MessageLabelHandle = this.lblMessage;
            this.tstTaskListPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Risk;
            tstTaskListPanel.LoadAndBindData(true);

            //Set notification panel context
            this.tstEmailPanel.MessageBoxClientId = this.lblMessage.ClientID;

			//Specify the initial context for the navigation bar
			this.navRiskList.ProjectId = ProjectId;
			this.navRiskList.ListScreenUrl = ReturnToRiskListUrl;
			if (this.riskId != -1)
			{
				this.navRiskList.SelectedItemId = this.riskId;
			}
			if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
			{
				//If we came from the 'My Page' > 'My Assigned' we need to set the navigation to 'My Assigned Risks'
				this.navRiskList.DisplayMode = NavigationBar.DisplayModes.Assigned;
				ProjectSettingsCollection settings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL);
				settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = (int)NavigationBar.DisplayModes.Assigned;
				settings.Save();
			}
            else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST_DETECTED] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST_DETECTED] == GlobalFunctions.PARAMETER_VALUE_TRUE)
            {
				//If we came from the 'My Page' > 'My Detected' we need to set the navigation to 'My Detected Risks'
				this.navRiskList.DisplayMode = NavigationBar.DisplayModes.Detected;
				ProjectSettingsCollection settings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL);
                settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = (int)NavigationBar.DisplayModes.Detected;
				settings.Save();
            }
			else
			{
				//Otherwise use the last setting the user used
				this.navRiskList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
			}
			this.navRiskList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
			this.navRiskList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

			//Specify the tab in the navigation control
			this.navRiskList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, ProjectId, -2, ArtifactTabName);
		}
		#endregion
	}
}
