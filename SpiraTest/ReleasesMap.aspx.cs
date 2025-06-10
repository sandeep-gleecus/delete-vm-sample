using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Data;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the project releases as a mind-map
    /// </summary>
    /// <remarks>
    /// See https://dagrejs.github.io/project/dagre-d3/latest/demo/interactive-demo.html for the sample it was based on
    /// </remarks>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Releases, "SiteMap_ReleasesMap", "Release-Management/#release-mindmap")]
    public partial class ReleasesMap : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ReleasesMap::";

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //We need to make sure that the system is licensed for mind maps (SpiraTeam or SpiraPlan only)
            if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.RequirementsFeature_InvalidLicense, true);
            }

            //Redirect if the user has limited view permissions for this artifact type. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized && !(UserIsAdmin && ProjectRoleId > 0))
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //Update the setting that we're using the map view if it's not already set
            string listPage = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
            if (listPage != "Map")
            {
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "Map");
            }

            //Only load the data once
            if (!IsPostBack)
            {
                //Populate the list of indent levels for the show indent level dropdown list
                this.ddlShowLevel.DataSource = CreateShowLevelList();

                //Databind the controls
                this.DataBind();

                //Set the initial value of the controls
                int openLevel = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_OPEN_LEVEL, 0);
                try
                {
                    this.ddlShowLevel.SelectedValue = openLevel.ToString();
                }
                catch (ArgumentOutOfRangeException)
                {
                    //leave unset
                }

                /*
                bool includeAssociations = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_INCLUDE_ASSOCIATIONS, true);
                this.chkIncludeAssociations.Checked = includeAssociations;
                */

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