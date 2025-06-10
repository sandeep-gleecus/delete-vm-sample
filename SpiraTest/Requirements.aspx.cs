using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.MasterPages;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Requirements Matrix and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Requirements, "SiteMap_Requirements", "Requirements-Management/#requirement-list")]
	public partial class Requirements : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Requirements::";
		
		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

            //Redirect if the user has limited view permissions for this artifact type. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //If we have SpiraTest, hide the list/board selector (since board view is not available)
            if (Inflectra.SpiraTest.Common.License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                this.plcListBoardSelector.Visible = false;
            }
            //Update the setting that we're using the tree view if it's not already set
            string listPage = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
            if (listPage != "Tree")
            {
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "Tree");
            }

            //Populate the user and project id in the grid control
            this.grdRequirementsMatrix.ProjectId = this.ProjectId;
            this.grdRequirementsMatrix.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -2);
            this.grdRequirementsMatrix.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT;

            //Add any event handlers
            //grdRequirementsMatrix_loaded
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("loaded", "grdRequirementsMatrix_loaded");
            this.grdRequirementsMatrix.SetClientEventHandlers(handlers);

            //Specify the context for the quick-filters sidebar
            this.pnlQuickFilters.ProjectId = ProjectId;
            this.pnlQuickFilters.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlQuickFilters.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Specify the context for the charts sidebar
            this.pnlCharts.ProjectId = ProjectId;
            this.pnlCharts.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlCharts.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

			//Only load the data once
			if (!IsPostBack) 
			{
                //Databind the list/board selector
                this.plcListBoardSelector.DataBind();

                //Populate the list of indent levels for the show indent level dropdown list
                this.ddlShowLevel.DataSource = CreateShowLevelList();
				this.ddlShowLevel.DataBind();

                //Get the list of fields in the show/hide columns list
                this.ddlShowHideColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.Requirement);
                this.ddlShowHideColumns.DataBind();

                //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
			}

            //See if we have any shared requirements to worry about
            bool hasChildProjects = (new ProjectManager().ProjectAssociation_RetrieveForDestProjectAndArtifact(ProjectId, Artifact.ArtifactTypeEnum.Requirement).Count > 0);

            if (hasChildProjects)
            {
                plcProjectSelector.Visible = true;

                //Choose the filter by project option
                bool filterbyProject = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, true);
                this.radAllProjects.Checked = !filterbyProject;
                this.lblAllProjects.Attributes["data-checked"] = (filterbyProject) ? "" : "checked";
                this.radCurrentProjects.Checked = filterbyProject;
                this.lblCurrentProjects.Attributes["data-checked"] = (filterbyProject) ? "checked" : "";
            }
            else
            {
                plcProjectSelector.Visible = false;
            }

			//Reset the error message
            this.divMessage.Text = "";

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion
	}
}
