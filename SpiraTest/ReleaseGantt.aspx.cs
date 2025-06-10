using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Release Gantt chart view and handling all raised events
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Releases, "SiteMap_ReleaseGantt", "Release-Management/#release-gantt-chart")]
    public partial class ReleaseGantt : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ReleaseGantt::";

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//We need to make sure that the system is licensed for Gantt Charts (SpiraTeam or SpiraPlan only)
			if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidLicense, true);
            }

            //Redirect if the user has limited view permissions for releases. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

			//Add the URL to the release hierarchical drop-down
			this.ddlSelectRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);

            //Only load the data once
            if (!IsPostBack)
            {				
				//Update the setting that we're using the Gantt view if it's not already set
				string listPage = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
                if (listPage != "Gantt")
                {
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "Gantt");
                }

				//Populate the list of releases and databind. We include inactive ones and let the dropdown list filter by active
				//that ensures that a legacy filter is displayed even if it is no longer selectable now
				Business.ReleaseManager releaseManager = new Business.ReleaseManager();
				List<ReleaseView> releases = releaseManager.RetrieveByProjectId(ProjectId, false);
				this.ddlSelectRelease.DataSource = releases;

				//Reset the release drop-down, handing exceptions quietly (in case a release was made inactive)
				try
				{
					int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
					if (releaseId < 1)
					{
						this.ddlSelectRelease.SelectedValue = "";
					}
					else
					{
						this.ddlSelectRelease.SelectedValue = releaseId.ToString();
					}
				}
				catch (Exception)
				{
					//This occurs if the release has been subsequently deleted. In which case we need to update
					//the stored settings
					SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
				}

				//Databind
				this.DataBind();


				//This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
				this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            //Reset the error message
            this.lblMessage.Text = "";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
	}
}
