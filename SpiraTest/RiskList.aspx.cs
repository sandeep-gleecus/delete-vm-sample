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
	/// List of risks and handling all raised events
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Risks, "SiteMap_Risks", "Risks-Management/#risk-list")]
	public partial class RiskList : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.RiskList::";

		protected int riskCount;

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
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Risk, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //Populate the user and project id in the grid control
			this.grdRiskList.ProjectId = this.ProjectId;
			this.grdRiskList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, ProjectId, -2);
            this.grdRiskList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_INCIDENT;

            //Specify the context for the quick-filters sidebar
            this.pnlQuickFilters.ProjectId = ProjectId;
            this.pnlQuickFilters.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlQuickFilters.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Specify the context for the charts sidebar
            this.pnlCharts.ProjectId = ProjectId;
            this.pnlCharts.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlCharts.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Custom CSS for the grid
            Dictionary<string, string> riskCssClasses = new Dictionary<string, string>();
            riskCssClasses.Add("RiskStatusId", "priority2");
            riskCssClasses.Add("RiskTypeId", "priority2");
            this.grdRiskList.SetCustomCssClasses(riskCssClasses);

			//Add the event handlers

			//Only load the data once
			if (!IsPostBack)
			{
				//Instantiate the business classes
				RiskManager riskManager = new RiskManager();

				//Populate the list of columns to show/hide and databind
                this.ddlShowHideColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.Risk);
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
