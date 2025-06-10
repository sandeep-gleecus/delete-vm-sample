using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    /// details of a particular test configuration set and handling updates
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.TestConfigurations, null, "Test-Configuration-Management/#test-configuration-details", "TestConfigurationDetails_Title")]
    public partial class TestConfigurationDetails : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TestConfigurationDetails::";

        protected int testConfigurationSetId;
		protected string ArtifactTabName = null;

		#region Properties

		/// <summary>
		/// Redirects back to the list page
		/// </summary>
		protected string ReturnToListPageUrl
        {
            get
            {
                //Redirect to the appropriate page
                return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestConfigurations, ProjectId);
            }
        }

        /// <summary>
        /// Returns the url to the generic automation host page with a token for the test configuration set id
        /// </summary>
        protected string TestConfigurationSetRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestConfigurations, ProjectId, -2));
            }
        }

        #endregion

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Capture the passed test configuration set id from the querystring
            this.testConfigurationSetId = System.Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_TEST_CONFIGURATION_SET_ID]);

            //Only load the data once
            if (!IsPostBack)
            {
                LoadAndBindData();
            }

            //Specify the context for the ajax form manager control
            this.ajxFormManager.ProjectId = this.ProjectId;
            this.ajxFormManager.PrimaryKey = this.testConfigurationSetId;
            this.ajxFormManager.ArtifactTypePrefix = TestConfigurationSet.ARTIFACT_PREFIX;
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
            //Specify the context for the navigation bar
            this.navTestConfigurationSets.ProjectId = ProjectId;
            this.navTestConfigurationSets.ListScreenUrl = ReturnToListPageUrl;
            if (this.testConfigurationSetId != -1)
            {
                this.navTestConfigurationSets.SelectedItemId = this.testConfigurationSetId;
            }
            this.navTestConfigurationSets.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
            this.navTestConfigurationSets.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.navTestConfigurationSets.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

            //Set the project/artifact for the RTE so that we can upload screenshots
            this.txtDescription.Screenshot_ProjectId = ProjectId;
            this.txtDescription.Screenshot_ArtifactId = this.testConfigurationSetId;

            //Set the context on the configurations grid
            this.grdTestConfigurations.ProjectId = ProjectId;

			//Set the selected host in the navigation sidebar
			this.DataBind();

            //See if a tab's been selected.
            if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
            {
				string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

				switch (tabRequest)
				{
                    case GlobalFunctions.PARAMETER_TAB_TESTSET:
                        tclTestConfigurationDetails.SelectedTab = this.pnlTestSets.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

                    case GlobalFunctions.PARAMETER_TAB_OVERVIEW:
                        tclTestConfigurationDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = tabRequest;
						break;

                    default:
                        tclTestConfigurationDetails.SelectedTab = this.pnlOverview.ClientID;
						this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_OVERVIEW;
						break;
                }
            }

			//Specify the base URL in the navigation control
			this.navTestConfigurationSets.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestConfigurations, ProjectId, -2, ArtifactTabName);
		}
    }
}
