using System.Data;
using System.Web.UI;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.DataModel;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// List of incidents and handling all raised events
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Incidents, "SiteMap_Incidents", "Incident-Tracking/#incident-list")]
	public partial class IncidentList : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.IncidentList::";

		protected int incidentCount;

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

            //Redirect if the user has limited view permissions for this artifact type. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //If we have SpiraTest, hide the list/board selector (since board view is not available)
            if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                this.plcListBoardSelector.Visible = false;
            }
            //Update the setting that we're using the table view if it's not already set
            string listPage = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
            if (listPage != "Table")
            {
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_BOARD_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "Table");
            }

            //Populate the user and project id in the grid control
			this.grdIncidentList.ProjectId = this.ProjectId;
			this.grdIncidentList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -2);
            this.grdIncidentList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_INCIDENT;

            //Specify the context for the quick-filters sidebar
            this.pnlQuickFilters.ProjectId = ProjectId;
            this.pnlQuickFilters.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlQuickFilters.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Specify the context for the charts sidebar
            this.pnlCharts.ProjectId = ProjectId;
            this.pnlCharts.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlCharts.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Custom CSS for the grid
            Dictionary<string, string> incidentCssClasses = new Dictionary<string, string>();
            incidentCssClasses.Add("IncidentStatusId", "priority2");
            incidentCssClasses.Add("IncidentTypeId", "priority2");
            this.grdIncidentList.SetCustomCssClasses(incidentCssClasses);

			//Add the event handlers

			//Only load the data once
			if (!IsPostBack)
			{
				//Instantiate the business classes
				IncidentManager incidentManager = new IncidentManager();

                //Databind the list/board selector
                this.plcListBoardSelector.DataBind();

                //Populate the list of columns to show/hide and databind
                this.ddlShowHideColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.Incident);
				this.ddlShowHideColumns.DataBind();

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
