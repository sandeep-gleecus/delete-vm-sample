using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

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
	/// details of a particular automation host and handling updates
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.AutomationHosts, null, "Automation-Host-Management/#automation-host-details", "AutomationHostDetails_Title")]
	public partial class AutomationHostDetails : PageLayout
	{
		protected int automationHostId;
		protected string ArtifactTabName = null;

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.AutomationHostDetails::";

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Capture the passed automation host id from the querystring
			this.automationHostId = System.Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_AUTOMATION_HOST_ID]);

			//Only load the data once
			if (!IsPostBack)
			{
				LoadAndBindData();
			}

			//Add the various panel event handlers and message references
			this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;
			this.tstTestRunListPanel.MessageLabelHandle = this.lblMessage;

            //Specify the context for the ajax form manager control
            this.ajxFormManager.ProjectId = this.ProjectId;
            this.ajxFormManager.PrimaryKey = this.automationHostId;
            this.ajxFormManager.ArtifactTypePrefix = AutomationHost.ARTIFACT_PREFIX;
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("dataSaved", "ajxFormManager_dataSaved");
            handlers.Add("loaded", "ajxFormManager_loaded");
            this.ajxFormManager.SetClientEventHandlers(handlers);

			//Reset the error message
			this.lblMessage.Text = "";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Loads and binds the host details
		/// </summary>
		private void LoadAndBindData()
		{

            //Automation hosts don't have creator/owner fields so there is no need to check for a 'limited' view/edit user permission

            //Specify the context for the navigation bar
            this.navHostList.ProjectId = ProjectId;
            this.navHostList.ListScreenUrl = ReturnToListPageUrl;
            if (this.automationHostId != -1)
            {
                this.navHostList.SelectedItemId = this.automationHostId;
            }

			this.navHostList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
			this.navHostList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
			this.navHostList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

            //Add the various custom properties to the relevant field group
            UnityCustomPropertyInjector.CreateControls(
                ProjectId,
                ProjectTemplateId,
                DataModel.Artifact.ArtifactTypeEnum.AutomationHost,
                this.customFieldsDefault,
                this.ajxFormManager,
                this.customFieldsUsers,
                this.customFieldsDates,
                this.customFieldsRichText
            );

            //Set the project/artifact for the RTE so that we can upload screenshots
            this.txtDescription.Screenshot_ProjectId = ProjectId;
            this.txtDescription.Screenshot_ArtifactId = this.automationHostId;

            //Set the selected host in the navigation sidebar
			this.DataBind();

			//Specify the artifact type to retrieve the attachments for
			this.tstAttachmentPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.AutomationHost;
            this.tstAttachmentPanel.ArtifactId = this.automationHostId;
            this.tstAttachmentPanel.MessageLabelHandle = this.lblMessage;
			tstAttachmentPanel.LoadAndBindData(true);

			//Specify the artifact type to retrieve the history log for
			this.tstHistoryPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.AutomationHost;
			tstHistoryPanel.LoadAndBindData(true);

            //Specify the artifact type to retrieve the test runs for
            tstTestRunListPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.AutomationHost;
            tstTestRunListPanel.LoadAndBindData(true);

			//See if a tab's been selected.
			if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
			{
				string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

				switch (tabRequest)
				{
					case GlobalFunctions.PARAMETER_TAB_ATTACHMENTS:
						tclHostDetails.SelectedTab = this.pnlAttachments.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_HISTORY:
						tclHostDetails.SelectedTab = this.pnlHistory.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
						tclHostDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					case GlobalFunctions.PARAMETER_TAB_TESTRUN:
						tclHostDetails.SelectedTab = this.pnlTestRuns.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

					default:
                        tclHostDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
						break;
				}
			}

			//Specify the base URL in the navigation control
			this.navHostList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.AutomationHosts, ProjectId, -2, ArtifactTabName);
		}

        /// <summary>
        /// Returns the url to the generic automation host page with a token for the automation host id
        /// </summary>
        protected string AutomationHostRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.AutomationHosts, ProjectId, -2));
            }
        }

        /// <summary>
        /// Redirects back to the list page
        /// </summary>
        protected string ReturnToListPageUrl
        {
            get
            {
                //Redirect to the appropriate page
                return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.AutomationHosts, ProjectId);
            }
        }

		/// <summary>
		/// Redirects to a different host
		/// </summary>
		/// <param name="automationHostId">The id of the host</param>
		protected void RedirectToDifferentArtifact(int automationHostId)
		{
			//Perform the redirect
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.AutomationHosts, ProjectId, automationHostId, ArtifactTabName), true);
		}
	}
}
