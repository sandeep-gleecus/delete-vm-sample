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
	/// Incidents Details/Edit and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Incidents, null, "Incident-Tracking/#incident-details", "IncidentDetails_Title")]
	public partial class IncidentDetails : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.IncidentDetails::";
		protected string ArtifactTabName = null;

		//Local variables
		protected int testRunId;
		protected int releaseId;
        protected Placeholder _placeholder;
        protected AuthorizedControlBase authorizedControlBase;
        protected int incidentId;
        protected int placeholderId;

		#region Properties
        
        /// <summary>
        /// Returns the url to the generic incident page with a token for the incident id
        /// </summary>
        protected string IncidentRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -2));
            }
        }

        /// <summary>
        /// Returns the url to the incident list page
        /// </summary>
        protected string IncidentListUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(ReturnToIncidentListUrl);
            }
        }

        /// <summary>
        /// Returns the url to the new incident page
        /// </summary>
        protected string IncidentNewUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -1));
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
            this.txtResolution.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);
            this.btnNewComment.Visible = (SpiraContext.Current.CanUserAddCommentToArtifacts);


            //Link the message label handle to the user controls
            this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;
            this.tstAssociationPanel.MessageLabelHandle = this.lblMessage;

			//Add the URL to the release hierarchical drop-downs
			this.ddlVerifiedRelease.BaseUrl = this.ddlResolvedRelease.BaseUrl = this.ddlDetectedRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);

			//Only load the data once
			if (!IsPostBack)
			{
				//Capture the passed incident id from the querystring - use '-1' to denote missing
				//i.e. inserting a new incident instead of updating existing
				if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_INCIDENT_ID]))
					this.incidentId = -1;
				else
					this.incidentId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_INCIDENT_ID]);

				//Load the data and databind
				LoadAndBindData();
			}

            //Set the action on the Add Comment button
            this.btnNewComment.ClientScriptMethod = String.Format("add_comment('{0}')", this.txtResolution.ClientID);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Takes the user page to the appropriate incident list page
		/// </summary>
		protected string ReturnToIncidentListUrl
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
					//Otherwise redirect back to the incident list page
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
						//Redirect back to the Incident List Page
						return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId);
					}
				}
			}
		}

        /*
		/// <summary>
		/// Loads the incident ID specified in the text box
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		/// <remarks>Does not save any changes made to the current form</remarks>
		private void btnFind_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnFind_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Capture the incident id from the textbox
			if (this.txtIncidentId.Text != "")
			{
				try
				{
					int incidentId = Int32.Parse(txtIncidentId.Text);

					//Retrieve the incident in question to make sure it exists
					try
					{
                        Incident incident = new IncidentManager().RetrieveById(incidentId, false);

						//Redirect to the new incident
                        RedirectToDifferentArtifact(incidentId);
                        return;
					}
					catch (ArtifactNotExistsException)
					{
						//If the artifact doesn't exist let the user know nicely
						this.lblMessage.Text = Resources.Messages.IncidentDetails_UnableToRetrieve;
						this.lblMessage.Type = MessageBox.MessageType.Error;
					}
				}
				catch (FormatException)
				{
					this.lblMessage.Text = Resources.Messages.IncidentDetails_IncidentIdNotValid;
					this.lblMessage.Type = MessageBox.MessageType.Error;
                }
			}

            //Load and bind data
            LoadAndBindData();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}*/

		#endregion

		#region Methods used internally to the class

		/// <summary>
		/// This method populates the lookup dropdowns and loads the appropriate incident dataset
		/// </summary>
		private void LoadAndBindData()
		{
            //Instantiate the business classes
            IncidentManager incidentManager = new IncidentManager();

			//Retrieve all the lookups for the drop-downs
            List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(this.ProjectTemplateId, true);
            List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(this.ProjectTemplateId, true);
            List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(this.ProjectTemplateId, true);
            List<DataModel.User> allUsers = new UserManager().RetrieveForProject(this.ProjectId);
            List<DataModel.User> activeUsers = new UserManager().RetrieveActiveByProjectId(this.ProjectId);
            List<Component> components = new ComponentManager().Component_Retrieve(this.ProjectId, true);

			Business.ReleaseManager releaseManager = new Business.ReleaseManager();
            List<ReleaseView> releases;
			releases = releaseManager.RetrieveByProjectId(this.ProjectId, false);

            this.ddlOpener.DataSource = allUsers;
            this.ddlOwner.DataSource = activeUsers;
            this.ddlIncidentType.DataSource = incidentTypes;
            this.ddlPriority.DataSource = incidentPriorities;
            this.ddlSeverity.DataSource = incidentSeverities;
            this.ddlComponent.DataSource = components;

            //Set the context that applies to both new incident and update cases
            //Populate the context of the form manager and register client-side event handlers
            this.ajxFormManager.ProjectId = ProjectId;
            this.ajxFormManager.ArtifactTypePrefix = Incident.ARTIFACT_PREFIX;

            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("dataSaved", "ajxFormManager_dataSaved");
            handlers.Add("loaded", "ajxFormManager_loaded");
            handlers.Add("operationReverted", "ajxFormManager_operationReverted");
            this.ajxFormManager.SetClientEventHandlers(handlers);

            //Set the context on the workflow operations controls
            Dictionary<string, string> handlers2 = new Dictionary<string, string>();
            handlers2.Add("operationExecuted", "ajxWorkflowOperations_operationExecuted");

            this.ajxWorkflowOperations.ProjectId = ProjectId;
            this.ajxWorkflowOperations.PrimaryKey = incidentId;
            this.ajxWorkflowOperations.SetClientEventHandlers(handlers2);

            //Set the context on the comment list
            this.lstComments.ProjectId = ProjectId;
            this.lstComments.ArtifactId = incidentId;

            //Set the project ID on the RTEs so that the plugin gets loaded
            this.txtDescription.Screenshot_ProjectId = ProjectId;
            this.txtResolution.Screenshot_ProjectId = ProjectId;

            //For existing incidents, the 'detected-in-release' should display all releases (active and inactive)
            //Resolved and verified always show just active releases. However we bind to all releases and let the dropdown
            //filter out inactive ones. That prevents the display showing 'none' if the current value is not active
            this.ddlDetectedRelease.DataSource = releases;
            this.ddlResolvedRelease.DataSource = releases;
            this.ddlVerifiedRelease.DataSource = releases;

			//Only display active release in the detected release dropdown if project setting for this is turned on
			ProjectSettings projectSettings = new ProjectSettings(ProjectId);
			if (projectSettings.DisplayOnlyActiveReleasesForDetected)
			{
				this.ddlDetectedRelease.ActiveItemField = "IsActive";
			}

			//Add the various custom properties to the table of fields
			UnityCustomPropertyInjector.CreateControls(
                ProjectId,
                ProjectTemplateId,
                DataModel.Artifact.ArtifactTypeEnum.Incident, 
                this.customFieldsDefault, 
                this.ajxFormManager, 
                this.customFieldsUsers, 
                this.customFieldsDates, 
                this.customFieldsRichText
                );

			//If we editing an existing incident, load the items, populate the default field values
			//and set the button caption to 'Update'
			if (this.incidentId == -1)
			{
                //Create a new placeholder to store any attachments and specify that on the various panels
                this._placeholder = new PlaceholderManager().Placeholder_Create(ProjectId);
                this.placeholderId = this._placeholder.PlaceholderId;

                //Attachments
                this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Placeholder;
                this.tstAttachmentPanel.ArtifactId = this.placeholderId;
                this.tstAttachmentPanel.LoadAndBindData(true);

                //and the page title for the "new incident" name
                this.ajxFormManager.NewItemName = Resources.Dialogs.Global_NewIncident;

				//Default to Overview tab for new incidents
				tclIncidentDetails.SelectedTab = this.pnlOverview.ClientID;

                //Populate the context of the form manager
                this.ajxFormManager.PrimaryKey = null;

                //Set the RTE permissions
                this.txtDescription.Authorized_Permission = Project.PermissionEnum.Create;

                //Databind the controls
                this.DataBind();
			}
			else
			{
                //Existing incident update starting point
                this.ajxFormManager.PrimaryKey = incidentId;

                //Get the list of attachments
                this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Incident;
                this.tstAttachmentPanel.ArtifactId = this.incidentId;
                this.tstAttachmentPanel.LoadAndBindData(true);

                //Get the list of history items and databind the grid
                this.tstHistoryPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Incident;
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
                            tclIncidentDetails.SelectedTab = this.pnlAttachments.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

                        case GlobalFunctions.PARAMETER_TAB_HISTORY:
                            tclIncidentDetails.SelectedTab = this.pnlHistory.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

                        case GlobalFunctions.PARAMETER_TAB_ASSOCIATION:
                            tclIncidentDetails.SelectedTab = this.pnlAssociations.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

                        case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
                            tclIncidentDetails.SelectedTab = this.pnlOverview.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

						default:
							tclIncidentDetails.SelectedTab = this.pnlOverview.ClientID;
							this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
							break;
					}
                }

                //Retrieve the incident in question
                Incident incident = null;
                try
                {
                    incident = incidentManager.RetrieveById(this.incidentId, false);
                }
                catch (ArtifactNotExistsException)
                {
                    //If the artifact doesn't exist let the user know nicely
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The incident you selected has been deleted from the system.", true);
                }

                //Make sure that the current project is set to the project in the incident
                //this is important since the page may get loaded from an email notification URL
                VerifyArtifactProject(incident.ProjectId, UrlRoots.NavigationLinkEnum.Incidents, incidentId);
			}

            //Specify the association panel context
            this.tstAssociationPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Incident;
            this.tstAssociationPanel.ArtifactId = this.incidentId;
            this.tstAssociationPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.ArtifactLink;
            this.tstAssociationPanel.LoadAndBindData(true);

			//Set notification panel context
            this.tstEmailPanel.MessageBoxClientId = this.lblMessage.ClientID;

			//Specify the initial context for the navigation bar
			this.navIncidentList.ProjectId = ProjectId;
			this.navIncidentList.ListScreenUrl = ReturnToIncidentListUrl;
			if (this.incidentId != -1)
			{
				this.navIncidentList.SelectedItemId = this.incidentId;
			}
			if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST] == GlobalFunctions.PARAMETER_VALUE_TRUE)
			{
				//If we came from the 'My Page' > 'My Assigned' we need to set the navigation to 'My Assigned Incidents'
				this.navIncidentList.DisplayMode = NavigationBar.DisplayModes.Assigned;
				ProjectSettingsCollection settings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION);
				settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = (int)NavigationBar.DisplayModes.Assigned;
				settings.Save();
			}
            else if (Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST_DETECTED] != null && Request.QueryString[GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST_DETECTED] == GlobalFunctions.PARAMETER_VALUE_TRUE)
            {
				//If we came from the 'My Page' > 'My Detected' we need to set the navigation to 'My Detected Incidents'
				this.navIncidentList.DisplayMode = NavigationBar.DisplayModes.Detected;
				ProjectSettingsCollection settings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION);
                settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = (int)NavigationBar.DisplayModes.Detected;
				settings.Save();
            }
			else
			{
				//Otherwise use the last setting the user used
				this.navIncidentList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
			}
			this.navIncidentList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
			this.navIncidentList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

			//Specify the tab in the navigation control
			this.navIncidentList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -2, ArtifactTabName);
		}
		#endregion
	}
}
