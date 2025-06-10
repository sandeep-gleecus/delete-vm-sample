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
    /// Responsible for displaying the list of pull request (tasks)
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.PullRequests, "SiteMap_PullRequests", "Pull-Requests")]
    public partial class PullRequests : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.PullRequests::";

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
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

			try
			{
				//See if we have at least one task type that supports pull requests, and that source code enabled for project
				SourceCodeManager sourceCodeManager = new SourceCodeManager();
				bool isActiveForSourceCode = sourceCodeManager.IsActiveProvidersForProject(ProjectId);
				List<TaskType> taskTypes = new TaskManager().TaskType_Retrieve(ProjectTemplateId);
				if (!taskTypes.Any(t => t.IsPullRequest))
				{
					this.divMessage.Type = MessageBox.MessageType.Warning;
					this.divMessage.Text = Resources.Messages.PullRequests_NoTaskTypeConfigured;
					this.grdPullRequests.Visible = false;
					this.plcLegend.Visible = false;
					this.plcToolbar.Visible = false;
				}
				else if (!isActiveForSourceCode)
				{
					this.divMessage.Type = MessageBox.MessageType.Warning;
					this.divMessage.Text = Resources.Messages.SourceCode_ProjectNotEnabled;
					this.grdPullRequests.Visible = false;
					this.plcLegend.Visible = false;
					this.plcToolbar.Visible = false;
				}
				else
				{
					//Populate the user and project id in the grid control
					this.grdPullRequests.ProjectId = this.ProjectId;
					this.grdPullRequests.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -2);
					this.grdPullRequests.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TASK;

					//Custom CSS for the grid
					Dictionary<string, string> pullRequestsCssClasses = new Dictionary<string, string>();
					pullRequestsCssClasses.Add("Token", "priority2");
					pullRequestsCssClasses.Add("IsActive", "priority3");
					this.grdPullRequests.SetCustomCssClasses(pullRequestsCssClasses);

					//Only load the data once
					if (!IsPostBack)
					{
						try
						{
							//Load any lookups in the new pull request dialog
							List<VersionControlBranch> branches = sourceCodeManager.RetrieveBranches2(ProjectId);
							List<DataModel.User> activeUsers = new UserManager().RetrieveActiveByProjectId(this.ProjectId);
							List<ReleaseView> activeReleases = new ReleaseManager().RetrieveByProjectId(ProjectId);
							this.ddlSourceBranch.DataSource = branches;
							this.ddlDestBranch.DataSource = branches;
							this.ddlOwner.DataSource = activeUsers;
							this.ddlRelease.DataSource = activeReleases;
							this.DataBind();
						}
						catch (Exception exception)
						{
							this.divMessage.Text = exception.Message;
							this.divMessage.Type = MessageBox.MessageType.Error;
						}
					}

					//Reset the error message
					this.divMessage.Text = "";
				}
			}
			catch (Exception exception)
			{
				this.divMessage.Text = exception.Message;
				this.divMessage.Type = MessageBox.MessageType.Error;
			}

			//This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
			this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
			this.btnEnterCatch.Attributes.Add("onclick", "return false;");
			this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        #endregion

    }
}
