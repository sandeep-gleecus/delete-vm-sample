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

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// Responsible for displaying the Test Configurations Page
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.TestConfigurations, "SiteMap_TestConfigurationsDesc", "Test-Configuration-Management/#test-configurations-list")]
    public partial class TestConfigurations : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TestConfigurations::";

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

            //Redirect if the user has limited view permissions for this artifact type. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //Populate the user and project id in the grid control
            this.grdTestConfigurationSets.ProjectId = this.ProjectId;
            this.grdTestConfigurationSets.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestConfigurations, ProjectId, -2);
            this.grdTestConfigurationSets.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TEST_CONFIGURATION_SET;

            //Custom CSS for the grid
            Dictionary<string, string> automationHostsCssClasses = new Dictionary<string, string>();
            automationHostsCssClasses.Add("Token", "priority2");
            automationHostsCssClasses.Add("IsActive", "priority3");
            this.grdTestConfigurationSets.SetCustomCssClasses(automationHostsCssClasses);

            //Only load the data once
            if (!IsPostBack)
            {
                //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
            }


            //Reset the error message
            this.divMessage.Text = "";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        #endregion

    }
}