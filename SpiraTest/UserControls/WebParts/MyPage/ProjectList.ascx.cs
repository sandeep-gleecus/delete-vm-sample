using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
	/// <summary>
	/// Displays the list of projects that the user is currently a member of
	/// </summary>
	public partial class ProjectList : WebPartBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.ProjectList::";

        #region User Configurable Properties

        /// <summary>
        /// Stores how many rows of data to display, default is 5
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
        DefaultValue(5)
        ]
        public int RowsToDisplay
        {
            get
            {
                return this.rowsToDisplay;
            }
            set
            {
				int rowsToDisplayMax = 50;
				this.rowsToDisplay = value < rowsToDisplayMax ? value : rowsToDisplayMax;
				//Force the data to reload
				LoadAndBindData();
            }
        }
        protected int rowsToDisplay = 5;

        #endregion


        /// <summary>
        /// Loads the control data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			try
			{
				//Add any event handlers
				this.grdProjectList.RowDataBound += new GridViewRowEventHandler(grdProjectList_RowDataBound);

				//Now load the content
				if (!IsPostBack && WebPartVisible)
				{
                    LoadAndBindData();
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//Don't rethrow as this is loaded by an update panel and can't redirect to error page
				if (this.Message != null)
				{
					this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
					this.Message.Type = MessageBox.MessageType.Error;
				}
			}
		}

        /// <summary>
        /// Loads and binds the data
        /// </summary>
        protected void LoadAndBindData()
        {
			//Get the list of projects, the user has access to
			ProjectManager projectManager = new ProjectManager();
			List<ProjectForUserView> projects = projectManager.RetrieveForUser(UserId);

			//Get the list of most recently accessed projects and set the date
			List<UserRecentProject> recentProjects = projectManager.RetrieveRecentProjectsForUser(UserId, RowsToDisplay, projects.Select(p => p.ProjectId).ToList());

			//Now get the top X most recently access projects
			this.grdProjectList.DataSource = recentProjects;
            this.grdProjectList.DataBind();

            //Only show the portfolio column when on the right version of Spira
            if (Common.Global.Feature_Portfolios)
            {
                this.grdProjectList.Columns[2].Visible = true;
            }

            //Loop through the dataset to highlight the currently selected project
            if (ProjectId > 0)
            {
                for (int i = 0; i < recentProjects.Count; i++)
                {
                    if (ProjectId == recentProjects[i].ProjectId)
                    {
                        grdProjectList.SelectedIndex = i;
                    }
                }
            }
        }

        /// <summary>
        /// Applies any selective formatting to the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdProjectList_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Ignore headers, footers, etc.
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				//See if the current user is allowed to view the project group dashboard
                Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();
				UserRecentProject userRecentProject = ((UserRecentProject)e.Row.DataItem);
                int projectGroupId = userRecentProject.Project.ProjectGroupId;
                int projectId = userRecentProject.ProjectId;
                e.Row.Cells[1].Enabled = projectGroupManager.IsAuthorized(UserId, projectGroupId);

                //Set the project group URL
                HyperLinkEx lnkProjectGroup = (HyperLinkEx)e.Row.Cells[1].FindControl("lnkProjectGroup");
                if (lnkProjectGroup != null)
                {
                    lnkProjectGroup.NavigateUrl = UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.ProjectGroupHome, projectGroupId);
                }

                //Set the portfolio URL - if on the right version of Spira and if the user has access to portfolios
                if (Common.Global.Feature_Portfolios)
                {
                    HyperLinkEx lnkPortfolio = (HyperLinkEx)e.Row.Cells[2].FindControl("lnkPortfolio");
                    int? portfolioId = userRecentProject.Project.Group.PortfolioId;

                    if (lnkPortfolio != null && portfolioId.HasValue)
                    {
                        e.Row.Cells[2].Enabled = UserIsPortfolioViewer;
                        lnkPortfolio.NavigateUrl = UrlRewriterModule.RetrievePortfolioRewriterURL(UrlRoots.NavigationLinkEnum.PortfolioHome, portfolioId.Value);
                    }
                }
            }
		}
	}
}
