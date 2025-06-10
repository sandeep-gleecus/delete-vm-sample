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
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// List of Releases and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Releases, "SiteMap_Releases", "Release-Management/#release-list")]
	public partial class Releases : PageLayout
	{
        //Lists
        protected SortedList<string, string> flagList;

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Releases::";
	
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
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //If we have SpiraTest, hide the list/board selector (since board view is not available)
            if (Inflectra.SpiraTest.Common.License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                this.plcListBoardSelector.Visible = false;
            }

            //Populate the user and project id in the grid control
            this.grdReleaseList.ProjectId = this.ProjectId;
			this.grdReleaseList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);
            this.grdReleaseList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_RELEASE;

			//Only load the data once
			if (!IsPostBack) 
			{
                //Update the setting that we're using the Tree view if it's not already set
                string listPage = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
                if (listPage != "Tree")
                {
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "Tree");
                }

                //Instantiate the business classes
                Business.ReleaseManager release = new Business.ReleaseManager();
                
                //Populate the list of indent levels for the show indent level dropdown list
				this.ddlShowLevel.DataSource = CreateShowLevelList();
				this.ddlShowLevel.DataBind();

                //Populate the list of columns to show/hide and databind
                this.ddlShowHideColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.Release);
                this.ddlShowHideColumns.DataBind();

                //Databind the view selectors
                this.plcListBoardSelector.DataBind();

                //Custom CSS for the grid
                Dictionary<string, string> releasesCssClasses = new Dictionary<string, string>();
                releasesCssClasses.Add("VersionNumber", "priority2");
                this.grdReleaseList.SetCustomCssClasses(releasesCssClasses);

                //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
			}

			//Reset the error message
			this.divMessage.Text = "";

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// This method populates the dataset and binds to the datagrid
		/// </summary>
		private void LoadAndBindData ()
		{
            //Do nothing as it's all handled via the AJAX services
		}
	}
}
