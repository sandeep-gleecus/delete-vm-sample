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
using System.Collections;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// details of a particular document and handling updates
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Releases, null, "Release-Management/#build-details", "BuildDetails_Title")]
    public partial class BuildDetails : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.BuildDetails::";

        protected int buildId;
        protected int releaseId;

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Capture the passed build id from the querystring
            this.buildId = System.Convert.ToInt32(Request.QueryString[GlobalFunctions.PARAMETER_BUILD_ID]);

            //Specify the context for the ajax form manager control
            this.ajxFormManager.ProjectId = this.ProjectId;
            this.ajxFormManager.PrimaryKey = this.buildId;
            this.ajxFormManager.ArtifactTypePrefix = Build.ARTIFACT_PREFIX;
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("loaded", "ajxFormManager_loaded");
            this.ajxFormManager.SetClientEventHandlers(handlers);

            //Only load the data once
            if (!IsPostBack)
            {
                //Load and bind the data on the page
                LoadAndBindData();
            }

            //Reset the error message
            this.lblMessage.Text = "";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Returns to the build list
        /// </summary>
        protected string ReturnToBuildListUrl
        {
            get
            {
                return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, releaseId, GlobalFunctions.PARAMETER_TAB_BUILD);
            }
        }

        /// <summary>
        /// Returns the url to the generic build page with a token for the build id
        /// </summary>
        protected string BuildRedirectUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Builds, ProjectId, -2));
            }
        }

        /// <summary>
		/// Loads and binds the build details
		/// </summary>
        private void LoadAndBindData()
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Load the current build from its id
                BuildManager buildManager = new BuildManager();
                Build build = buildManager.RetrieveById(this.buildId);

                //Update the page title/description with the artifact id and name
                ((MasterPages.Main)this.Master).PageTitle = Build.ARTIFACT_PREFIX + build.BuildId + " - " + build.Name;

                //Get the release of the build
                this.releaseId = build.ReleaseId;

                //Make sure that the current project is set to the project associated with the build
                //this is important since the page may get loaded from an email notification URL
                VerifyArtifactProject(build.ProjectId, UrlRoots.NavigationLinkEnum.Builds, buildId);

                //Specify the context for the nav-bar
                this.navBuildList.ProjectId = ProjectId;
                this.navBuildList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
                this.navBuildList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));
                this.navBuildList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
                this.navBuildList.SelectedItemId = this.buildId;
                this.navBuildList.ListScreenUrl = ReturnToBuildListUrl;
                this.navBuildList.ItemBaseUrl = BuildRedirectUrl;

                //Set the context for the associations grid
                this.tstAssociationPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Build;
                this.tstAssociationPanel.DisplayTypeEnum = DataModel.Artifact.DisplayTypeEnum.Build_Associations;

                //Specify the artifact type and artifact id to retrieve the test run list coverage for
                this.tstTestRunListPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Build;
                this.tstIncidentListPanel.ArtifactTypeEnum = DataModel.Artifact.ArtifactTypeEnum.Build;

                //Set the release on the navigation bar
                this.navBuildList.ContainerId = releaseId;

                //Make sure we have SpiraPlan/SpiraTeam, otherwise hide revisions
                if (!Common.Global.Feature_SourceCode)
                {
                    this.boxCommits.Visible = false;
                }
                else
                {
                    //Set the context for the Revision grid
                    this.grdSourceCodeRevisionList.ProjectId = this.ProjectId;
                    this.grdSourceCodeRevisionList.BaseUrl = "~/SourceCodeRedirect.aspx?" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_SESSION_ID + "={art}";

                    //Set the filter that it knows to filter by build and branch
                    Dictionary<string, object> revisionFilters = new Dictionary<string, object>();
                    string currentBranch = SourceCodeManager.Get_UserSelectedBranch(UserId, ProjectId);
                    if (!String.IsNullOrEmpty(currentBranch))
                    {
                        revisionFilters.Add("BranchKey", currentBranch);
                    }
                    revisionFilters.Add("BuildId", this.buildId);
                    this.grdSourceCodeRevisionList.SetFilters(revisionFilters);
					this.grdSourceCodeRevisionList.DisplayTypeId = (int)Artifact.DisplayTypeEnum.Build_Revisions;

					//Register client events handlers on grid
					Dictionary<string, string> handlers = new Dictionary<string, string>();
					handlers.Add("loaded", "grdSourceCodeRevisionList_loaded");
					this.grdSourceCodeRevisionList.SetClientEventHandlers(handlers);
				}

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (ArtifactNotExistsException)
            {
                //If the artifact doesn't exist let the user know nicely
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.BuildDetails_BuildNotExists, true);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }
    }
}
